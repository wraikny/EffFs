System.Environment.CurrentDirectory <- __SOURCE_DIRECTORY__

#load "../src/EffFs/EffFs.fs"
#load "../src/EffFs/Library/StateMachine.fs"
#load "../src/EffFs/Library/Log.fs"

open EffFs
open EffFs.Library
open EffFs.Library.Log

module ESM = StateMachine

module A =
  type State = { a: int }

  type Msg = NoOps

  let inline update (msg: Msg) (state) =
    match msg with
    | NoOps -> state

module B =
  type State = { b: int }

  type StateOut = int

  // EffFs.StateMachine
  type State with

    static member StateOut(_) = Eff.marker<StateOut>


  type Msg =
    | Incr
    | Apply

  let inline update (msg: Msg) ({ b = b }) =
    match msg with
    | Incr -> ESM.Pending { b = b + 1 }
    | Apply -> ESM.Completed b

module C =
  type StateOut = int

  type State =
    | Base of int
    | StateOfB of B.State * (B.StateOut -> ESM.StateStatus<State, B.StateOut>)

    static member StateOut(_) = Eff.marker<StateOut>
    static member StateEnter(s, k) = StateOfB(s, k)

    override x.ToString() =
      x
      |> function
        | Base i -> sprintf "Base (%d)" i
        | StateOfB(s, _) -> sprintf "StateOfB (%O)" s

  type Msg =
    | Apply
    | Trans'B
    | MsgOfB of B.Msg

    static member Lift(x) = MsgOfB x

  let inline update msg state =
    eff {
      match (state, msg) with
      | Base i, Apply -> return ESM.Completed i

      | Base i, Trans'B ->
        let! i = ESM.stateEnter { B.State.b = i }
        return Base i |> ESM.Pending

      | StateOfB(s, k), MsgOfB m -> return ESM.stateMap (B.update m) (s, k)

      | _ -> return ESM.Pending state
    }

module Program =
  type StateMachine =
    | StateOfA of A.State
    | StateOfB of B.State * (B.StateOut -> StateMachine)
    | StateOfC of C.State * (C.StateOut -> StateMachine)

    static member inline Init(s) = StateOfA s

    override x.ToString() =
      x
      |> function
        | StateOfA s -> sprintf "StateOfA (%O)" s
        | StateOfB(s, _) -> sprintf "StateOfB (%O)" s
        | StateOfC(s, _) -> sprintf "StateOfC (%O)" s

  // EffFs.StateMachine
  type StateMachine with

    static member inline StateEnter(s, k) = StateOfB(s, k)
    static member inline StateEnter(s, k) = StateOfC(s, k)

  [<RequireQualifiedAccess>]
  type Msg =
    | Trans'AtoB
    | Trans'AtoC
    | MsgOfA of A.Msg
    | MsgOfB of B.Msg
    | MsgOfC of C.Msg

    static member Lift(m) = MsgOfA m
    static member Lift(m) = MsgOfB m
    static member Lift(m) = MsgOfC m

  let inline update (msg: Msg) (state) =
    eff {
      match (state, msg) with
      | StateOfA s, Msg.Trans'AtoB ->
        do! log "A ---> B"
        let! x = ESM.stateEnter { B.b = s.a }
        do! log "A <--- B"
        return StateOfA { a = x }

      | StateOfA s, Msg.Trans'AtoC ->
        do! log "A ---> C"
        let! x = ESM.stateEnter (C.Base s.a)
        do! log "A <--- C"
        return StateOfA { a = x }

      | StateOfA s, Msg.MsgOfA m ->
        let s = A.update m s
        return StateOfA s

      | StateOfB(s, k), Msg.MsgOfB m -> return ESM.stateMap (B.update m) (s, k)

      | StateOfC(s, k), Msg.MsgOfC m -> return! ESM.stateMapEff (C.update m) (s, k)

      | _ -> return state
    }


open Program

let inline lift (a: ^a) : ^b =
  ((^a or ^b): (static member Lift: ^a -> ^b) a)

[<Sealed>]
type Handler() =
  static member inline Value(_, x) = x

  static member inline Handle(e, k) = ESM.handle (e, k)

  static member inline Handle(LogEffect s, k) =
    printfn "[Log] %s" s
    k ()


let handler = Handler()

let sm = StateMachine.Init { a = 0 }

printfn "--- Start: \t%O ---\n" sm

let result: StateMachine =
  [ Msg.Trans'AtoB
    lift B.Msg.Incr
    lift B.Msg.Incr
    lift B.Msg.Incr
    lift B.Msg.Apply
    Msg.Trans'AtoC
    lift C.Msg.Trans'B
    lift <| lift B.Msg.Incr
    lift <| lift B.Msg.Incr
    lift <| lift B.Msg.Apply
    lift C.Msg.Apply ]
  |> Seq.fold
    (fun sm msg ->
      let sm = sm |> update msg |> Eff.handle handler
      printfn "%A:    %O\n" msg sm
      sm
    )
    sm

result
|> function
  | StateOfA a when a.a = 5 -> printfn "Success %O" result
  | _ -> failwithf "Unexpected result %O" result
