[<RequireQualifiedAccess>]
module EffFs.Library.Log
open EffFs

[<Struct; NoEquality; NoComparison>]
type LogEffect = LogEffect of string
with
  static member Effect(_) = Eff.marker<unit>

let inline log msg = LogEffect msg

let inline logf fmt = Printf.kprintf (LogEffect) fmt
