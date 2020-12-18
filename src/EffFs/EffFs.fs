namespace EffFs

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
  let inline pure'(x: 'a): Eff<'a, ^h> = Eff(fun _ -> x)

  open System.Runtime.InteropServices

  type Default3 = class end
  type Default2 = class inherit Default3 end
  type Default1 = class inherit Default2 end

  type Dummy1<'t>(x: 't) = class member val Value1 = x end
  type Dummy2<'t>(x: 't) = class member val Value2 = x end

  type Handle =
    inherit Default1

    // 優先順位0
    static member inline Handle (Eff e: Eff<'a, ^h>, f, [<Optional>]_impl: Handle): Eff<'b, ^h> = capture (e >> f)
    static member Handle (e: Dummy1<_>, f, _impl: Handle) = f e.Value1
    static member Handle (e: Dummy2<_>, f, _impl: Handle) = f e.Value2

    // 優先順位1
    static member inline Handle (e: ^``Effect<'a>``, f: 'a -> Eff<'b, ^h0>, [<Optional>]_impl: Default1): Eff<'b, ^h1>
      when ^``Effect<'a>``: (static member Effect: ^``Effect<'a>`` -> EffectTypeMarker<'a>) =
      ((^h0 or ^h1): (static member Handle:_*_->_)e,f)

    static member Handle (e: Dummy1<_>, f, _impl: Default1) = f e.Value1
    static member Handle (e: Dummy2<_>, f, _impl: Default1) = f e.Value2

    static member inline Invoke (f: 'a -> Eff<'b, ^h0>) (effect: ^``Effect<'a>``): Eff<'b, ^h1>
      when ^``Effect<'a>``: (static member Effect: ^``Effect<'a>`` -> EffectTypeMarker<'a>) =
      let inline call_2 (impl: ^s, input: ^t, f) = ((^s or ^t): (static member Handle:_*_*_->_) input,f,impl)
      let inline call (impl: 's, input: 't, f) = call_2 (impl, input, f)
      call (Unchecked.defaultof<Handle>, effect, f)

  let inline bind(f: 'a -> Eff<'b, ^h0>) (e: ^``Effect<'a>``): Eff<'b, ^h1> = Handle.Invoke f e

  let inline flatten (e: ^``Effect<^Effect<'a>>``): Eff<'a, ^h> = bind id e

  let inline map f e = bind (fun x -> Eff(fun _ -> f x)) e

  /// <summary>Handle effect with given handler</summary>
  let inline handle (handler: ^h) (effect: ^``Effect<'a>``): 'b =
    let inline valueHandle x = (^h: (static member Handle:_->_)x)
    bind (valueHandle >> pure') effect |> apply handler

type Eff<'a, 'h> with
  static member Effect(_: Eff<'t, _>) = Eff.marker<'t>
  // static member inline Handle(Eff e: Eff<'b, ^g>, f: 'b -> Eff<'c, ^g>): Eff<'c, ^g> = Eff.capture (e >> f)


[<AutoOpen>]
module Builder =
  type EffBuilder() =
    member inline __.Return(x) = Eff.pure' x

    member inline __.Bind(e, f) = Eff.bind f e

    member inline __.ReturnFrom(e) = Eff.bind Eff.pure' e

    member inline __.Zero() = Eff.pure' ()

    member inline __.Delay f = f ()

    member inline __.Combine(a, b) = Eff.bind (fun() -> b) a

  let eff = EffBuilder()
