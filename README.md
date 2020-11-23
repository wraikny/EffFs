[![](https://github.com/wraikny/EffFs/workflows/CI/badge.svg)](https://github.com/wraikny/EffFs/actions?workflow=CI)

# EffFs
F# Effect System based on SRTP.  
Caution: Multiple handlers cannot be composed.  

[SharpLab compilation result](https://sharplab.io/#v2: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=) 
of [example.fsx](example/example.fsx)

## Example
And [more examples](example/example.fsx)

```fsharp
#load "../src/EffFs/EffFs.fs"
open EffFs

type RandomInt = RandomInt of int with
  static member Effect = Eff.marker<int>

type Logging = Logging of string with
  static member Effect = Eff.marker<unit>

let inline foo() = eff {
  let! a = RandomInt 100
  do! Logging (sprintf "%d" a)
  let b = a + a
  return (a, b)
}

let rand = System.Random()

type Handler = Handler with
  static member inline Handle(x) = x

  static member inline Handle(RandomInt a, k) =
    rand.Next(a) |> k

  static member inline Handle(Logging s, k) =
    printfn "[Log] %s" s; k()

foo()
|> Eff.handle Handler
|> printfn "%A"


// example output
(*
[Log] 66
(66, 132)
*)
```

## .NET Core
```sh
$ dotnet --version
3.1.101
```

## Build
```sh
$ dotnet build src/EffFs # Debug
$ dotnet build src/EffFs -c Release
```

## Example
```
$ dotnet fsi --exec example/example.fs
```
