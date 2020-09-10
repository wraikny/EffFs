module EffFs.Library.StateMachine
open EffFs

let inline private callStateEnter (k: 'o -> 'os) (s: ^s): ^state
  when ^s: (static member StateOut: ^s -> EffectTypeMarker<'o>) =
  (^state: (static member StateEnter: ^s*('o -> 'os) -> ^state) s,k)

[<Struct; RequireQualifiedAccess>]
type StateEnterEffect< ^s, 'o when ^s : (static member StateOut: ^s -> EffectTypeMarker<'o>) > = StateEnterEffect of ^s
with
  static member inline Effect (_: StateEnterEffect< ^s, 'o >) = Eff.marker< 'o >

let inline handle (StateEnterEffect.StateEnterEffect (s: ^s), k: 'o -> Eff<'os, ^h>): Eff< ^state, ^h >
  when ^state : (static member StateEnter: ^s*('o -> 'os) -> ^state) =
  Eff.capture(fun h -> s |> callStateEnter (k >> Eff.handle h) |> Eff.pure')


[<Struct>]
type StateStatus< 's, 'o > =
  | Pending of state:'s
  | Completed of output:'o
with
  static member inline StateEnter(s: ^a, k: 'b -> 'c): StateStatus< ^state, _ >
    when ^a : (static member StateOut: ^a->EffectTypeMarker<'b>)
    and ^state : (static member StateEnter: ^a*('b -> 'c) -> ^state) =
    Pending (callStateEnter k s)

[<RequireQualifiedAccess>]
module StateStatus =
  let inline pure' x = Completed x

  let inline bind f state = state |> function
    | Pending x -> f x
    | Completed x -> Completed x

  let inline map f state = bind (f >> pure') state


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
