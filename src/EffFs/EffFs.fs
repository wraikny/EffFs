namespace EffFs

type Eff<'a, ^h> = Eff of (^h -> 'a)

module Internal =
  type Default4 = class end
  type Default3 = class inherit Default4 end
  type Default2 = class inherit Default3 end
  type Default1 = class inherit Default2 end

  // open System.Runtime.InteropServices

  type Handle =
    inherit Default1

    static member inline Apply h (Eff e) = e h
    static member inline Bind(e: ^``E<^a>``, k: 'a -> Eff< ^b, ^h >): Eff< ^b, ^h > =
      ((^h or ^``E<^a>``): (static member Handle:_*_->_)e,k)

    static member inline Handle((Eff e: Eff< ^a, ^h >, k: ^a -> Eff< ^b, ^h >), _mthd: Handle) =
      Eff(fun h -> e h |> k |> Handle.Apply h)

    static member inline Handle((e: ^``E<^a>`` when (^h or ^``E<^a>``): (static member Handle:^``E<^a>``*(^a -> Eff< ^b, ^h >)->Eff< ^b, ^h >), k: ^a -> Eff< ^b, ^h>)
      , _mthd: Default1) = Handle.Bind(e, k)

    static member inline Handle((e: ^``E<^a>`` when (^g or ^``E<^a>``): (static member Handle:^``E<^a>``*(^a -> Eff<Eff< ^b, ^g >, ^h>)->Eff<Eff< ^b, ^g >, ^h>), k: ^a -> Eff<Eff< ^b, ^g >, ^h>)
      , _mthd: Default2) = Eff(fun g -> Handle.Bind(e, k) |> Handle.Apply g)

    static member inline Invoke(k: ^a -> Eff< ^b, ^h >) (e: ^``E<^a>``): Eff< ^b, ^h > =
      let inline call(x: ^M, e: ^E) = ((^M or ^E): (static member Handle:(_*_)*_->_)(e,k),x)
      call(Unchecked.defaultof<Handle>, e)


module Eff =
  let inline handle (handler: ^handler) (Eff e) = e handler

  let inline capture(f: ^h -> Eff< ^a, ^h >) = Eff(fun h -> f h |> handle h)

  let inline nest (Eff e: Eff<'a, ^handler>) k: Eff<'b, ^handler> = capture(e >> k)

  let inline pure' (x: ^a): Eff< ^b, ^h > = Eff(fun _ -> (^h: (static member Handle:_->_)x))

  (*
    EffFs.fs(44,5): error FS0193: 型パラメーターに制約
    'when (EffFs.Internal.Handle or  ^Effect<^a>) : (static member Handle : ( ^Effect<^a> * ( ^a -> EffFs.Eff< ^b, ^handler>)) * EffFs.Internal.Handle -> EffFs.Eff< ^b, ^handler>)'
    がありません
  *)
  let inline bind (k: ^a -> Eff< ^b, ^handler >) (effect: ^``Effect<^a>``) =
    Internal.Handle.Invoke k effect
  // let inline bind (k: ^a -> Eff< ^b, ^handler >) (effect: ^``Effect<^a>``) = Internal.Handle.Bind(effect, k)

  let inline map (f: ^a -> ^b) (e: ^``Effect<^a>``): Eff< ^b, ^h> = bind (f >> pure') e


[<AutoOpen>]
module Builder =
  type EffBuilder() =
    member inline __.Return(x: ^a): Eff< ^b, ^h > = Eff.pure' x
    member inline __.ReturnFrom(e: ^e) = Eff.bind id e
    member inline __.Bind(e: ^``E<^a>``, f): Eff< ^b, ^h > = Eff.bind f e

  let eff = EffBuilder()
