namespace EffFs

type Eff<'a, ^h> = Eff of (^h -> 'a)

module Eff =
  /// <summary>Handle effect with given handler</summary>
  let inline handle (handler: ^handler) (eff: Eff< ^a, ^handler >) = eff |> function Eff e -> e handler

  /// <summary>Access to handler instance</summary>
  let inline capture (f: ^handler -> Eff<'a, ^handler>) = Eff(fun h -> f h |> handle h)

  let inline pure' (x: 'a): Eff<'a, ^handler> = Eff (fun _ -> x)

  let inline bind (f: 'a -> Eff<'b, ^handler>) (Eff e): Eff<'b, ^handler> = capture (e >> f)

  let inline join (e: Eff<Eff<'a, ^handler>, ^handler>): Eff<'a, ^handler> = bind id e

  let inline map (f: 'a -> 'b) (e: Eff<'a, ^handler>) = bind (f >> pure') e

  /// <summary>Bind given monad with given bind function</summary>
  let inline bindWith bind (x: '``Monad<'a>``) (k: 'a -> Eff<'``Monad<'b>``, ^h>) = Eff(fun h -> bind (k >> handle h) x)

[<AutoOpen>]
module Builder =
  type EffBuilder() =
    member inline __.Return(x: 'a): Eff<'b, ^h> = Eff(fun _ -> (^h: (static member Handle:_->_)x))
    member inline __.Bind(e: ^e, f): Eff<'b, ^h> = ((^h or ^e): (static member Handle:_*_->_)e,f)
    member inline this.ReturnFrom(e: ^e) =  this.Bind(e, this.Return)

  let eff = EffBuilder()

// For FSharpPlus
type Eff<'a, ^h> with
  static member inline Return(x) = Eff.pure' x
  static member inline (>>=) (x, f) = Eff.bind f x
