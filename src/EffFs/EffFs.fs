namespace EffFs

type Eff<'a, ^h> = Eff of (^h -> 'a)

module Eff =
  let inline handle (handler: ^handler) (Eff e) = e handler

  let inline nest (Eff e: Eff<'a, ^handler>) k: Eff<'b, ^handler> = Eff(fun h -> e h |> k |> handle h)

  let inline capture(f: ^h -> Eff<'a, ^h>) = Eff(fun h -> f h |> handle h)

  let inline pure' (x: 'a): Eff<'b, ^h> = Eff(fun _ -> (^h: (static member Handle:_->_)x))

  let inline bind (f: 'a -> Eff<'b, ^handler>) (e: ^``Effect<'a>``): Eff<'b, ^handler> =
    ((^handler or ^``Effect<'a>``): (static member Handle:_*_->_)e,f)

  let inline map (f: 'a -> 'b) (e: ^``Effect<'a>``): Eff<'b, ^h> = bind (f >> pure') e


[<AutoOpen>]
module Builder =
  open System
  type EffBuilder() =
    member inline __.Return(x: 'a): Eff<'b, ^h> = Eff.pure' x
    member inline __.ReturnFrom(e: ^e) = Eff.bind id e
    member inline __.Bind(e: ^e, f): Eff<'b, ^h> = Eff.bind f e

    // member inline __.Zero() = Eff(ignore)
    // member inline __.Delay(f: unit -> Eff<'a, ^h>) = f
    // member inline __.Run(f): Eff<'a, ^h> = f()
    // member inline __.Combine(a, b) =
    //   Eff(fun h -> a |> Eff.handle h |> b |> Eff.handle h)

  let eff = EffBuilder()
