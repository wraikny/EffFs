[<RequireQualifiedAccess>]
module EffFs.Library.Random
open EffFs

[<Struct; NoEquality; NoComparison>]
type RandomEffect<'a> = RandomEffect of (System.Random -> 'a)
with
  static member Effect(_: RandomEffect<'a>) = Eff.marker<'a>

let inline private apply r (RandomEffect f) = f r

let inline pure' a = RandomEffect (fun _ -> a)

let inline bind f (RandomEffect random) =
  RandomEffect (fun r -> (f (random r)) |> apply r)

let inline map f (RandomEffect random) =
  RandomEffect (fun r -> random r |> f)

let bool = RandomEffect (fun r -> r.Next() % 2 = 0)

let int min max = RandomEffect (fun r -> r.Next(min, max))

let float = RandomEffect (fun r -> r.NextDouble())

let float32 = RandomEffect (fun r -> r.NextDouble() |> float32)

let array count (RandomEffect random) =
  RandomEffect (fun r ->
    let arr = Array.zeroCreate count
    for i in 0..count-1 do arr.[i] <- random r
    arr
  )

let infinit (RandomEffect random) =
  RandomEffect (fun r -> seq { while true do yield random r })

let shuffleSeq (xs: seq<'a>) =
  RandomEffect (fun r -> xs |> Seq.sortBy (fun _ -> r.Next()) )

let shuffleArray (xs: 'a[]) =
  RandomEffect (fun r -> xs |> Array.sortBy (fun _ -> r.Next()) )

