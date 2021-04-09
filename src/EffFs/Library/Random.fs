module EffFs.Library.Random
open EffFs

[<Struct; NoEquality; NoComparison>]
type RandomEffect<'a> = RandomEffect of (System.Random -> 'a)
with
  static member Effect(_: RandomEffect<'a>) = Eff.marker<'a>

[<RequireQualifiedAccess>]
module RandomEffect =
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

  let tuple2 (RandomEffect r1) (RandomEffect r2) =
    RandomEffect (fun r -> (r1 r, r2 r))

  let tuple3 (RandomEffect r1) (RandomEffect r2) (RandomEffect r3) =
    RandomEffect (fun r -> (r1 r, r2 r, r3 r))

  let tuple2V (RandomEffect r1) (RandomEffect r2) =
    RandomEffect (fun r -> struct(r1 r, r2 r))

  let tuple3V (RandomEffect r1) (RandomEffect r2) (RandomEffect r3) =
    RandomEffect (fun r -> struct(r1 r, r2 r, r3 r))

  let inline until f (RandomEffect random) =
    RandomEffect (fun r ->
      let rec loop () =
        let x = random r
        if f x then x else loop ()
      loop ()
    )

  let initSeq count (RandomEffect random) =
    RandomEffect (fun r -> Seq.init count (fun _ -> random r))

  let initArray count (RandomEffect random) =
    RandomEffect (fun r -> Array.init count (fun _ -> random r))

  let infinit (RandomEffect random) =
    RandomEffect (fun r -> seq { while true do yield random r })

  let shuffleSeq (xs: 'a seq) =
    RandomEffect (fun r -> Seq.sortBy (fun _ -> r.Next()) xs)

  let shuffleList (xs: 'a list) =
    RandomEffect (fun r -> List.sortBy (fun _ -> r.Next()) xs)

  let shuffleArray (xs: 'a[]) =
    RandomEffect (fun r -> Array.sortBy (fun _ -> r.Next()) xs)

  let inline sequence (rs: _ RandomEffect seq) =
    RandomEffect (fun r ->
      rs |> Seq.map (apply r)
    )

  let inline sequenceList (rs: _ RandomEffect list) =
    RandomEffect (fun r -> List.map (apply r) rs)

  let inline sequenceArray (rs: _ RandomEffect []) =
    RandomEffect (fun r -> Array.map (apply r) rs)

  let inline sequenceMap (rs: Map<_, _ RandomEffect>) =
    RandomEffect (fun r -> Map.map (fun _ -> apply r) rs)
