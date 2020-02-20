module EffFs
open System.ComponentModel

type Eff< ^Handler, 'a > = Eff of (^Handler -> 'a)

/// internal function
[<EditorBrowsable(EditorBrowsableState.Never)>]
let inline __getEffValue (Eff x) = x

/// internal function
[<EditorBrowsable(EditorBrowsableState.Never)>]
let inline __handleEff (_: ^Handler) (x: ^``Effect<'a>``, k: 'a -> 'b) : 'b =
  ((^``Effect<'a>`` or ^Handler): (static member Handle:_*_->_)x,k)

let inline perform (handler: ^Handler) eff: 'a = __getEffValue eff handler

type EffBuilder() =
  member inline __.Return(x): Eff< ^Handler, 'a> = Eff(fun _ -> x)
  
  member inline __.Bind(effect: ^``Effect<'a>``, f: 'a -> Eff< ^Handler, 'b>) =
    Eff(fun h -> __handleEff h (effect, f) |> __getEffValue <| h)
  
  member inline __.Bind(eff: Eff< ^Handler, 'a>, f: 'a -> Eff< ^Handler, 'b>) =
    Eff(fun h -> __getEffValue eff h |> f |> __getEffValue <| h)
  
  member inline __.ReturnFrom(x) = x

  member __.Combine (g,f) = g >> f

let eff = EffBuilder()
