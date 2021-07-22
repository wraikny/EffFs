[![](https://github.com/wraikny/EffFs/workflows/CI/badge.svg)](https://github.com/wraikny/EffFs/actions?workflow=CI)
[![Nuget](https://img.shields.io/nuget/v/EffFs?style=plastic)](https://www.nuget.org/packages/EffFs/)

# EffFs
F# Effect System based on SRTP.  
Caution: Multiple handlers cannot be composed.  

[SharpLab compilation result](https://sharplab.io/#v2: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) 
of [example.fsx](example/example.fsx)

## Installation
- Available on Nuget: [`dotnet add package EffFs`](https://www.nuget.org/packages/EffFs)


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
  static member inline Value(_, x) = x

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

## Build
.NET 5.0 or later is required.

```sh
$ dotnet build src/EffFs # Debug
$ dotnet build src/EffFs -c Release
```


## Examples
```
$ dotnet fsi --exec example/example.fs
```

## Make Nuget Package

```
$ # edit RELEASE_NOTES.md
$ dotnet fake build -t assemblyinfo
$ dotnet fake build -t pack
```
