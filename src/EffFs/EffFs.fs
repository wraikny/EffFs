namespace EffFs

[<RequireQualifiedAccess>]
type EffectOutput<'a> = EffectOutput of 'a

[<Struct>]
type Eff<'a, 'h> = Eff of ('h -> 'a)

[<AutoOpen>]
module private Internal =
  let inline apply h (Eff e) = e h
  let inline output<'a> = EffectOutput.EffectOutput(Unchecked.defaultof<'a>)
  let inline capture (f: ^handler -> Eff<'a, ^handler>) = Eff(fun h -> f h |> apply h)

type Eff<'a, 'h>  with
  static member Effect = output<'a>
  static member inline Handle(Eff e: Eff<'b, ^g>, f): Eff<'c, ^g> = capture (e >> f)

[<RequireQualifiedAccess>]
module Eff =
  /// <summary>Mark Effect's output type</summary>
  let inline output<'a> = output<'a>

  /// <summary>Access to handler instance</summary>
  let inline capture (f: _ -> Eff<'a, ^handler>) = capture f

  let inline pure'(x: 'a): Eff<'b, ^h> = Eff(fun _ -> (^h: (static member Handle:'a->'b)x))

  let inline bind(f: 'a -> Eff<'b, ^h>) (e: ^``Effect<'a>``): Eff<'b, ^h>
    when ^``Effect<'a>``: (static member Effect: EffectOutput<'a>) =
    ((^h or ^``Effect<'a>``): (static member Handle:_*_->_)e,f)

  let inline join (e: ^``Effect<^Effect<'a>>``): Eff<'a, ^handler> = bind id e

  let inline map f e = bind (f >> pure') e

  /// <summary>Handle effect with given handler</summary>
  let inline handle (handler: ^handler) (eff: ^``Effect<'a>``) = bind pure' eff |> apply handler

[<AutoOpen>]
module Builder =
  type EffBuilder() =
    member inline __.Return(x) = Eff.pure' x

    member inline __.Bind(e, f) = Eff.bind f e

    member inline __.ReturnFrom(e) = Eff.bind Eff.pure' e

  let eff = EffBuilder()
