System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__
#load "../src/EffFs/EffFs.fs"
#load "../src/EffFs/Library/StateMachine.fs"

module ESM = EffFs.Library.StateMachine

module Effect =
  open EffFs

  [<Struct>]
  type LogEffect = LogEffect of string
  with
    static member Effect(_) = Eff.marker<unit>

  let inline log format = Printf.kprintf (sprintf "%s" >> LogEffect) format


module A =
  type State = { a: int }

  type Msg = NoOps

  let inline update (msg: Msg) (state) =
    match msg with
    | NoOps -> state

module B =
  open EffFs

  type State = { b: int }

  // EffFs.StateMachine
  type State with
    static member StateOut(_) = Eff.marker<int>

  type StateOut = int

  type Msg = Incr | Apply

  let inline update (msg: Msg) ({ b = b }) =
    match msg with
    | Incr -> ESM.Pending { b = b + 1 }
    | Apply -> ESM.Completed b


module Program =
  type StateMachine =
    | StateOfA of A.State
    | StateOfB of B.State * (B.StateOut -> StateMachine)
  with
    static member inline Init(s) = StateOfA s

    override x.ToString() = x |> function
      | StateOfA s -> sprintf "StateOfA %A" s
      | StateOfB (s, _) -> sprintf "StateOfB %A" s

  // EffFs.StateMachine
  type StateMachine with
    static member inline StateEnter (s, k) = StateOfB (s, k)

  [<RequireQualifiedAccess>]
  type Msg =
    | Trans'AtoB
    | MsgOfA of A.Msg
    | MsgOfB of B.Msg
  with
    static member Lift(m) = MsgOfA m
    static member Lift(m) = MsgOfB m

  open EffFs

  let inline update (msg: Msg) (state) = eff {
    match (state, msg) with
    | StateOfA s, Msg.Trans'AtoB ->
      do! Effect.log "A ---> B"
      let! x = ESM.stateEnter { B.b = s.a }
      do! Effect.log "A <--- B"
      return StateOfA { a = x }

    | StateOfA s, Msg.MsgOfA m ->
      let s = A.update m s
      return StateOfA s

    | StateOfB (s, k), Msg.MsgOfB m ->
      return ESM.stateMap (B.update m) (s, k)

    | _ ->
      return state
  }


open Program
open EffFs

let inline lift (a: ^a): ^b = ((^a or ^b): (static member Lift: ^a -> ^b) a)

[<Sealed>]
type Handler() =
  static member inline Handle(x) = x

  static member inline Handle(Effect.LogEffect s, k) =
    printfn "[Log] %s" s; k()


let handler = Handler()

let sm = StateMachine.Init { a = 0 }

printfn "--- Start: \t%O ---\n" sm

let result: StateMachine =
  [ Msg.Trans'AtoB
    lift B.Msg.Incr
    lift B.Msg.Incr
    lift B.Msg.Incr
    lift B.Msg.Apply
  ] |> Seq.fold (fun sm msg ->
    let sm = sm |> update msg |> Eff.handle handler
    printfn "%A:    %O\n" msg sm
    sm
  ) sm

result |> function
| StateOfA a when a.a = 3 -> printfn "Success %O" result
| _ -> failwithf "Unexpected result %O" result
