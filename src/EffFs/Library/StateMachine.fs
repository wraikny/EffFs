[<RequireQualifiedAccess>]
module EffFs.Library.StateMachine
open EffFs

let inline private callStateEnter k s: ^a = (^a: (static member StateEnter: _*_ -> _) s,k)

[<Struct; RequireQualifiedAccess>]
type StateEnterEffect< ^s, 'o when ^s : (static member StateOut: ^s -> EffectTypeMarker< 'o >) > = StateEnterEffect of ^s
with
  static member inline Effect (_: StateEnterEffect< ^s, 'o >) = Eff.marker< 'o >

  static member inline Handle (StateEnterEffect (s: ^state), k: 'o -> Eff< ^a, ^h >): Eff< ^a, ^h > =
    Eff.capture(fun h -> s |> callStateEnter (k >> Eff.handle h) |> Eff.pure')

[<Struct>]
type StateStatus< ^s, 'o when ^s : (static member StateOut: ^s -> EffectTypeMarker< 'o >)> =
  | Pending of state:^s
  | Completed of output:'o

let inline stateEnter (state: ^s) = StateEnterEffect.StateEnterEffect state

let inline stateMap (f: ^s -> StateStatus< ^s, 'o>) (s: ^s, k: 'o -> ^state) =
  match f s with
  | Pending s -> callStateEnter k s
  | Completed o -> k o

let inline stateMapEff (f: ^s -> Eff<StateStatus< ^s, 'o>, _>) (s: ^s, k: 'o -> ^state) =
  eff {
    match! f s with
    | Pending s -> return callStateEnter k s
    | Completed o -> return k o
  }
