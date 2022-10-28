#load "../src/EffFs/EffFs.fs"

open EffFs

[<Struct; NoEquality; NoComparison>]
type SeqE<'a> =
  | RepeatE of seq<'a>

  static member Effect(_: SeqE<'a>) = Eff.marker<'a>

type Handler() =
  static member Value(_, x) = x

  static member Handle(RepeatE xs, k: int -> Eff<_, _>) =
    Eff(fun (h: Handler) -> Seq.map (k >> Eff.handle h) xs)

let inline test1 () : Eff<_, _> =
  eff {
    let! i = RepeatE [ 0..9 ]
    return i + 2
  }

let handler = Handler()

test1 () |> Eff.handle handler |> Seq.toArray |> printfn "%A"
