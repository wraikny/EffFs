module EffFs
open System.ComponentModel

type Eff< ^Handler, 'a > = Eff of (^Handler -> 'a)

[<RequireQualifiedAccess>]
module Eff =
  [<EditorBrowsable(EditorBrowsableState.Never)>]
  let inline __get (Eff x) = x

  [<EditorBrowsable(EditorBrowsableState.Never)>]
  let inline __handle (_: ^h) (x: ^``Effect<'a>``, k: 'a -> 'b) : 'b =
    ((^``Effect<'a>`` or ^h): (static member Handle:_*_->_)x,k)

  let inline handle (handler: ^Handler) eff: 'a = __get eff handler

type EffBuilder() =
  member inline __.Return(x): Eff< ^h, 'a> =
    Eff(fun _ -> (^h: (static member Handle:_->_)x))
  
  member inline __.Bind(effect: ^``Effect<'a>``, f: 'a -> Eff< ^h, 'b>) =
    Eff(fun h -> Eff.__handle h (effect, f) |> Eff.__get <| h)
  
  member inline __.Bind(eff: Eff< ^h, 'a>, f: 'a -> Eff< ^h, 'b>) =
    Eff(fun h -> Eff.__get eff h |> f |> Eff.__get <| h)
  
  member inline __.ReturnFrom(x) = x

  member __.Combine (g,f) = g >> f

let eff = EffBuilder()
