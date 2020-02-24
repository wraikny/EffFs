namespace EffFs

type Eff<'a, ^h> = Eff of (^h -> 'a)

module Eff =
  let inline handle (handler: ^handler) (Eff e) = e handler

  let inline capture(f: ^handler -> Eff<'a, ^handler>) = Eff(fun h -> f h |> handle h)

  let inline nest (Eff e: Eff<'a, ^handler>) k: Eff<'b, ^handler> = capture (e >> k)

  let inline pure' (x: 'a): Eff<'b, ^handler> = Eff(fun _ -> (^handler: (static member Handle:_->_)x))

  let inline bind (f: 'a -> Eff<'b, ^handler>) (e: ^``Effect<'a>``): Eff<'b, ^handler> =
    ((^handler or ^``Effect<'a>``): (static member Handle:_*_->_)e,f)

  let inline map (f: 'a -> 'b) (e: ^``Effect<'a>``): Eff<'b, ^handler> = bind (f >> pure') e


[<AutoOpen>]
module Builder =
  type EffBuilder() =
    member inline __.Return(x: 'a): Eff<'b, ^h> = Eff.pure' x
    member inline __.ReturnFrom(e: ^e) = Eff.bind id e
    member inline __.Bind(e: ^e, f): Eff<'b, ^h> = Eff.bind f e

  let eff = EffBuilder()
