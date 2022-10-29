#load "../src/EffFs/EffFs.fs"
open EffFs

[<Struct; NoEquality; NoComparison>]
type IntE = IntE
  with

    static member Effect(_) = Eff.marker<int>

[<Struct; NoEquality; NoComparison>]
type PrintE =
  | PrintE of string

  static member Effect(_) = Eff.marker<unit>

let inline test n =
  Eff.fix
    (fun f n ->
      eff {
        if n > 100 then
          return n
        else
          let! a = IntE
          do! PrintE $"{a}"
          return! f (n + a)
      }
    )
    n

type Handler() =
  member val Count = 0 with get, set
  static member inline Value(_, x) = x

  static member inline Handle(IntE, k) =
    Eff.capture (fun (h: Handler) ->
      let count = h.Count
      h.Count <- count + 1
      count |> k
    )

  static member inline Handle(PrintE s, k) =
    let date = System.DateTime.Now
    printfn "[Log][%O] %s" date s
    k ()

test 3 |> Eff.handle (Handler()) |> printfn "%d"
