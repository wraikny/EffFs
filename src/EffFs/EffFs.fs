namespace EffFs

open System.ComponentModel

[<AbstractClass; Sealed>]
type EffectTypeMarker<'a> = class end

[<Struct>]
type Eff<'a, 'h> = Eff of ('h -> 'a)

[<RequireQualifiedAccess>]
module Eff =
  open System.ComponentModel

  let inline private apply h (Eff e) = e h

  [<EditorBrowsable(EditorBrowsableState.Never)>]
  module Internal =
    open System.Runtime.InteropServices

    type Default1 = class inherit Default2 end
    and  Default2 = class inherit Default3 end
    and  Default3 = class end

    type Dummy1<'t>(x: 't) = class member val Value1 = x end
    type Dummy2<'t>(x: 't) = class member val Value2 = x end

    type Handle =
      inherit Default1

      // Priority 0
      static member inline Handle(Eff e: Eff<'a, ^h>, f: 'a -> Eff<'b, ^h>, [<Optional>]_impl: Handle): Eff<'b, ^h> =
        Eff(fun h -> e h |> f |> apply h)

      // Priority 1
      static member inline Handle(e: ^``Effect<'a>``, f:'a -> Eff<'b, ^h1>, [<Optional>]_impl: Default1): Eff<'c, ^h2>
        when ^``Effect<'a>``: (static member Effect: ^``Effect<'a>`` -> EffectTypeMarker<'a>) =
        (^h1: (static member Handle:_*_->_)e,f)

      static member Handle(e: Dummy1<_>, f, _impl: Default1): Eff<'c, _> = f e.Value1

      // // Priority 2
      static member inline Handle(e: ^``Effect<'a>``, f:'a -> Eff<'b, ^h1>, [<Optional>]_impl: Default2): Eff<'c, ^h2>
        when ^``Effect<'a>``: (static member Effect: ^``Effect<'a>`` -> EffectTypeMarker<'a>) =
        (^``Effect<'a>``: (static member Handle:_*_->_)e,f)

      static member Handle(e: Dummy1<_>, f, _impl: Default2): Eff<'c, _> = f e.Value1

      // Priority 3
      static member inline Handle(e: ^``Effect<'a>``, f:'a -> Eff<'b, ^h1>, [<Optional>]_impl: Default3): Eff<Eff<'c, ^h2>, ^h3>
        when ^``Effect<'a>``: (static member Effect: ^``Effect<'a>`` -> EffectTypeMarker<'a>) =
        Eff (fun _ ->
          (^h3: (static member Handle:_*_->_)e,f)
        )

      static member Handle(e: Dummy1<_>, f, _impl: Default3): Eff<'c, _> = f e.Value1

      // invoke
      static member inline Invoke (f: 'a -> Eff<'b, ^g>) (e: ^``Effect<'a>``): Eff<'c, ^h>
        when ^``Effect<'a>``: (static member Effect: ^``Effect<'a>`` -> EffectTypeMarker<'a>) =
        let inline call_2 (impl: ^impl, input, f, _handler :^handler) = ((^impl or ^handler): (static member Handle: _*_*_ -> _) input,f,impl)
        let inline call   (impl: 'impl, input, f, handler) = call_2 (impl, input, f, handler)
        call (Unchecked.defaultof<Handle>, e, f, Unchecked.defaultof< ^h>)

  /// <summary>Mark Effect's type</summary>
  let inline marker<'a> = Unchecked.defaultof<EffectTypeMarker<'a>>

  /// <summary>Access to handler instance</summary>
  let inline capture (f: ^h -> Eff<'a, ^h>) = Eff(fun h -> f h |> apply h)

  // let inline pure'(x: 'a): Eff<'b, ^h> = Eff(fun _ -> (^h: (static member Handle:'a->'b)x))
  let inline pure'(x: 'a): Eff<'a, ^h> = Eff(fun _ -> x)

  let inline bind (f: 'a -> Eff<'b, ^g>) (e: ^``Effect<'a>``): Eff<'c, ^h> = Internal.Handle.Invoke f e

  let inline flatten (e: ^``Effect<^Effect<'a>>``): Eff<'b, ^h> = bind id e

  let inline map f e = bind (fun x -> Eff(fun _ -> f x)) e

  [<EditorBrowsable(EditorBrowsableState.Never)>]
  type Recursive<'a, 'b, 'h> = delegate of Recursive<'a, 'b, 'h> -> ('a -> Eff<'b, 'h>)

  /// <summary>Z Combinator to define recursive function</summary>
  let inline fix f =
    Recursive(fun g -> f (fun x -> g.Invoke g x))
    |> (fun (r: Recursive<_, _, _>) -> r.Invoke r)

  /// <summary>Handle effect with given handler</summary>
  let inline handle (handler: ^h) (eff: ^``Effect<'a>``): 'b =
    let inline valueHandle x = (^h: (static member Value:_*_->_)handler,x)
    bind (valueHandle >> pure') eff |> apply handler

type Eff<'a, 'h> with
  static member Effect(_: Eff<'t, _>) = Eff.marker<'t>


[<AutoOpen>]
module Builder =
  type EffBuilder() =
    member inline __.Return(x): Eff<'a, ^h> = Eff.pure' x

    member inline __.Bind(e: ^``Effect<'a>``, f: 'a -> Eff<'b, ^g>): Eff<'c, ^h> = Eff.bind f e

    member inline __.ReturnFrom(e: ^``Effect<'a>``): Eff<'c, ^h> = Eff.bind Eff.pure' e

    member inline __.Zero() = Eff.pure' ()

    member inline __.Delay f = f ()

    member inline __.Combine(a, b) = Eff.bind (fun() -> b) a

  let eff = EffBuilder()
