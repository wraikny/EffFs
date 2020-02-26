namespace EffFs

type Eff<'a, ^h> = Eff of (^h -> 'a)

module Internal =
  type Default4 = class end
  type Default3 = class inherit Default4 end
  type Default2 = class inherit Default3 end
  type Default1 = class inherit Default2 end

  open System.Runtime.InteropServices

  type Handle =
    inherit Default1

    static member inline Apply h (Eff e) = e h

    static member inline Return(x: ^a): Eff< ^b, ^handler > =
      Eff(fun _ -> (^handler: (static member Handle:_->_)x))

    static member inline Bind(e: ^``E<^a>``, k: 'a -> Eff< ^b, ^h >): Eff< ^b, ^h > =
      ((^h or ^``E<^a>``): (static member Handle:_*_->_)e,k)

    static member inline Handle((Eff e: Eff< ^a, ^h >, k: ^a -> Eff< ^b, ^h >), [<Optional>]_mthd: Default1): Eff< ^b, ^h > =
      Eff(fun h -> e h |> k |> Handle.Apply h)

    static member inline Handle((e: ^``E<^a>``, k: ^a -> Eff< ^b, ^h>), [<Optional>]_mthd: Default2): Eff< ^b, ^h >
      when (^h or ^``E<^a>``): (static member Handle:^``E<^a>``*(^a -> Eff< ^b, ^h >)->Eff< ^b, ^h >) =
        Handle.Bind(e, k)

    static member inline Handle((e: ^``E<^a>``, k: ^a -> Eff< ^b, ^h >), [<Optional>]_mthd: Default3): Eff<Eff< ^b, ^g >, ^h>
      when (^g or ^``E<^a>``) : (static member Handle: ^``E<^a>``*(^a -> Eff< ^a, ^g >)->Eff< ^a, ^g >) =
        Eff(fun (h: ^h) -> Eff(fun (g: ^g) -> Handle.Bind(e, Handle.Return) |> Handle.Apply g |> k |> Handle.Apply h))

    static member inline Invoke(k: ^a -> Eff< ^b, ^h >) (e: ^``E<^a>``): Eff< ^b, ^h > =
      let inline call(x: ^M, e: ^E) = ((^M or ^E): (static member Handle:(_*_)*_->_)(e,k),x)
      call(Unchecked.defaultof<Handle>, e)

open Internal

module Eff =
  let inline pure' (x: ^a): Eff< ^b, ^handler > = Handle.Return x

  let inline bind (k: ^a -> Eff< ^b, ^handler >) (effect: ^``Effect<^a>``): _
    when (Handle or ^``Effect<^a>``) : (static member Handle : ( ^``Effect<^a>`` * ( ^a -> Eff< ^b, ^handler>)) * Handle -> Eff< ^b, ^handler>) =
      Handle.Invoke k effect

  let inline map (f: ^a -> ^b) (e: ^``Effect<^a>``): Eff< ^b, ^handler >
    when (Handle or ^``Effect<^a>``) : (static member Handle : ( ^``Effect<^a>`` * ( ^a -> Eff< ^b, ^handler>)) * Handle -> Eff< ^b, ^handler>) =
      bind (f >> pure') e

  // let inline handle (handler: ^handler) (e: ^``Effect<^a>``): ^b
  //   when (Handle or ^``Effect<^a>``) : (static member Handle : ( ^``Effect<^a>`` * ( ^a -> Eff< ^b, ^handler>)) * Handle -> Eff< ^b, ^handler>)
  //   = e |> bind pure' |> Handle.Apply handler

  let inline handle (handler: ^handler) (Eff e) = e handler

  let inline capture(f: ^handler -> Eff< ^a, ^handler >) =
    Eff(fun h -> f h |> Handle.Apply h)


[<AutoOpen>]
module Builder =
  type EffBuilder() =
    member inline __.Return(x: ^a): Eff< ^b, ^h > = Eff.pure' x

    member inline __.ReturnFrom(e: ^``Effect<^a>``): Eff< ^b, ^handler >
      when (Handle or ^``Effect<^a>``): (static member Handle : ( ^``Effect<^a>`` * ( ^a -> Eff< ^b, ^handler>)) * Handle -> Eff< ^b, ^handler>)
        = Eff.bind Eff.pure' e

    member inline __.Bind(e: ^``Effect<^a>``, f): Eff< ^b, ^handler >
      when (Handle or ^``Effect<^a>``): (static member Handle : ( ^``Effect<^a>`` * ( ^a -> Eff< ^b, ^handler>)) * Handle -> Eff< ^b, ^handler>)
        = Eff.bind f e

  let eff = EffBuilder()
