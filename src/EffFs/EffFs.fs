module EffFs

type Eff<'a, ^h> = Eff of (^h -> 'a)

module Eff =
  let inline handle handler (Eff e: Eff<'a, ^Handler>) = e handler

type EffBuilder() =
  member inline __.Return(x: 'a): Eff<'b, ^h> =
    Eff(fun _ -> (^h: (static member Handle:_->_)x))
  member inline __.Bind(eff: ^Ea, f: 'a -> Eff<'b, ^h>): Eff<'b, ^h> =
    ((^Ea or ^h): (static member Handle:_*_->_)eff, f)
  member inline __.ReturnFrom(x) = x
  member inline __.Combine(g,f) = g >> f

let eff = EffBuilder()
