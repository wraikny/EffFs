﻿namespace EffFs

open System.ComponentModel

[<AbstractClass; Sealed>]
type EffectTypeMarker<'a> = class end

[<Struct>]
type Eff<'a, 'h> = Eff of ('h -> 'a)

[<RequireQualifiedAccess>]
module Eff =

  let inline private apply h (Eff e) = e h

  /// <summary>Mark Effect's type</summary>
  let inline marker<'a> = Unchecked.defaultof<EffectTypeMarker<'a>>

  /// <summary>Access to handler instance</summary>
  let inline capture (f: ^h -> Eff<'a, ^h>) = Eff(fun h -> f h |> apply h)

  // let inline pure'(x: 'a): Eff<'b, ^h> = Eff(fun _ -> (^h: (static member Handle:'a->'b)x))
  let inline pure' (x: 'a) : Eff<'a, ^h> = Eff(fun _ -> x)

  let inline bind
    (f: 'a -> Eff<'b, ^g>)
    (e: ^ea)
    : Eff<'c, ^h> when ^ea: (static member Effect: ^ea -> EffectTypeMarker<'a>) =
    ((^h or ^g or ^ea): (static member Handle: _ * _ -> _) e, f)

  let inline flatten (e: ^eea) : Eff<'b, ^h> = bind id e

  let inline map f e = bind (fun x -> Eff(fun _ -> f x)) e

  [<EditorBrowsable(EditorBrowsableState.Never)>]
  type Recursive<'a, 'b, 'h> = delegate of Recursive<'a, 'b, 'h> -> ('a -> Eff<'b, 'h>)

  /// <summary>Z Combinator to define recursive function</summary>
  let inline fix f =
    Recursive(fun g -> f (fun x -> g.Invoke g x))
    |> (fun (r: Recursive<_, _, _>) -> r.Invoke r)

  /// <summary>Handle effect with given handler</summary>
  let inline handle (handler: ^h) (eff: ^ea) : 'b =
    let inline valueHandle x =
      (^h: (static member Value: _ * _ -> _) handler, x)

    bind (valueHandle >> pure') eff |> apply handler

type Eff<'a, 'h> with

  static member Effect(_: Eff<'t, _>) = Eff.marker<'t>
  static member inline Handle(Eff e: Eff<'b, ^g>, f: 'b -> Eff<'c, ^g>) : Eff<'c, ^g> = Eff.capture (e >> f)


[<AutoOpen>]
module Builder =
  type EffBuilder() =
    member inline __.Return(x) = Eff.pure' x

    member inline __.Bind(e, f) = Eff.bind f e

    member inline __.ReturnFrom(e) = Eff.bind Eff.pure' e

    member inline __.Zero() = Eff.pure' ()

    member inline __.Delay f = f ()

    member inline __.Combine(a, b) = Eff.bind (fun () -> b) a

  let eff = EffBuilder()
