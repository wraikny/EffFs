namespace EffFs

[<RequireQualifiedAccess>]
type EffectOutput<'a> = EffectOutput of 'a

[<Struct>]
type Eff<'a, 'h> = Eff of ('h -> 'a)

[<AutoOpen>]
module private Helper =
  let inline apply h (Eff e) = e h
  let inline output<'a> = EffectOutput.EffectOutput(Unchecked.defaultof<'a>)
  let inline capture (f: ^h -> Eff<'a, ^h>) = Eff(fun h -> f h |> apply h)

// type Eff<'a, 'h> with
//   static member Effect = output<'a>
//   static member inline Handle(Eff e: Eff<'s, ^g>, k): Eff<'t, ^g> = capture (e >> k)

module Internal =
  type Default4 = class end
  type Default3 = class inherit Default4 end
  type Default2 = class inherit Default3 end
  type Default1 = class inherit Default2 end

  open System.Runtime.InteropServices

  type Handle =
    inherit Default1

    static member inline Return(x: 'a): Eff<'b, ^h> =
      Eff(fun _ -> (^h: (static member Handle:_->_)x))

    static member inline InvokeOnInstance(k: 'a -> Eff<'b, ^h>) (e: ^``E<'a>``): Eff<'b, ^h>
      when ^``E<'a>``: (static member Effect: EffectOutput<'a>) =
      (^h: (static member Handle:_*_->_)e,k)

    static member inline Handle ((Eff e: Eff<'a, 'h>, k: 'a -> Eff<'b, 'h>), _mthd: Handle): Eff<'b, 'h> =
      capture (e >> k)

    static member inline Handle ((e: ^``E<'a>``, k: 'a -> Eff<'b, ^h>), _mthd: Handle): Eff<'b, ^h> =
      Eff(fun h -> Handle.InvokeOnInstance k e |> apply h)

    // static member inline Handle((e: ^``E<'a>``, k: 'a -> Eff<'b, 'h>), [<Optional>]_mthd: Default2): Eff<Eff<'b, ^g>, 'h> =
    //   Eff(fun (h: 'h) -> Eff(fun (g: ^g) -> Handle.InvokeOnInstance Eff e |> apply g |> k |> apply h))

    static member inline Invoke (k: 'a -> Eff<'b, 'h>) (e: '``E<'a>``): Eff<'c, 'h> =
      let inline call (x: ^M) = (^M: (static member Handle:(_*_)*_ ->_)(e,k),x)
      call (Unchecked.defaultof<Handle>)

open Internal

[<RequireQualifiedAccess>]
module Eff =
  /// <summary>Mark Effect's output type</summary>
  let inline output<'a> = output<'a>

  /// <summary>Access to handler instance</summary>
  let inline capture (f: _ -> Eff<'a, 'handler>) = capture f

  let inline pure'(x: 'a): Eff<'b, 'h> = Handle.Return x

  let inline bind (f: 'a -> Eff<'b, 'h>) (e: '``Effect<'a>``): Eff<'c, 'h> = Handle.InvokeOnInstance f e

  let inline join (e: '``Effect<'Effect<'a>>``): Eff<'a, 'handler> = bind id e

  let inline map f e = bind (f >> pure') e

  /// <summary>Handle effect with given handler</summary>
  let inline handle (handler: 'handler) (eff: '``Effect<'a>``) = bind pure' eff |> apply handler

[<AutoOpen>]
module Builder =
  type EffBuilder() =
    member inline __.Return(x: 'a): Eff<'b, 'h> = Eff.pure' x

    member inline __.Bind(e: '``Effect<'a>``, f: 'a -> Eff<'b, 'h>) = Eff.bind f e

    member inline __.ReturnFrom(e: '``Effect<'a>``): Eff<'c, 'h> = Eff.bind Eff.pure' e

  let eff = EffBuilder()

#if DEBUG
module Debug =
  type RandomInt = RandomInt of int with
    static member Effect = Eff.output<int>

  type Println = Println of obj with
    static member Effect = Eff.output<unit>

  let inline hoge(): Eff<'a, ^h> = eff {
    let! a = RandomInt 10000
    do! Println "Hello!"
    return (float a / 10000.0)
  }

  // let inline foo() =
  //   eff {
  //     let! a = RandomInt 100
  //     let! x = hoge()
  //     do! Println x
  //     do! Println a
  //     let b = a + a
  //     return (a, b)
  //   }
#endif