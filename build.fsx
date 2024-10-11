#!dotnet fsi

#r "netstandard"
#r "nuget: MSBuild.StructuredLogger"
#r "nuget: Fake.Core"
#r "nuget: Fake.Core.Target"
#r "nuget: Fake.Core.ReleaseNotes"
#r "nuget: Fake.IO.FileSystem"
#r "nuget: Fake.DotNet.Cli"
#r "nuget: Fake.DotNet.AssemblyInfoFile"
#r "nuget: Fake.Net.Http"
#r "nuget: FSharp.Json"

open System


open Fake.Core
open Fake.Core.TargetOperators
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators

// Boilerplate
System.Environment.GetCommandLineArgs()
|> Array.skip 2 // skip fsi.exe; build.fsx
|> Array.toList
|> Context.FakeExecutionContext.Create false __SOURCE_FILE__
|> Context.RuntimeContext.Fake
|> Context.setExecutionContext

Target.initEnvironment ()

let dotnet cmd arg =
  let res = DotNet.exec id cmd arg

  if not res.OK then
    res.Messages |> String.concat "\n" |> Trace.trace
    failwithf "failed 'dotnet %s %s'" cmd arg

let fsiExec path = dotnet "fsi" path

let release = ReleaseNotes.load "RELEASE_NOTES.md"

Target.create
  "AssemblyInfo"
  (fun _ ->
    [ AssemblyInfo.Version release.AssemblyVersion
      AssemblyInfo.FileVersion release.AssemblyVersion
      AssemblyInfo.InformationalVersion release.NugetVersion ]
    |> AssemblyInfoFile.createFSharp "./src/EffFs/AssemblyInfo.fs"
  )

Target.create "Test" (fun _ -> !! "example/*.fsx" |> Seq.iter fsiExec)

Target.create "Tool" (fun _ -> dotnet "tool" "update fake-cli")

Target.create "Clean" (fun _ -> !! "src/**/bin" ++ "src/**/obj" |> Shell.cleanDirs)

Target.create "Build" (fun _ -> !! "src/**/*.*proj" |> Seq.iter (DotNet.build id))

Target.create
  "Pack"
  (fun _ ->
    !! "src/**/*.*proj"
    |> Seq.iter (
      DotNet.pack (fun opt ->
        { opt with
            Configuration = DotNet.BuildConfiguration.Release
            OutputPath = Some "output/packages/"
            MSBuildParams =
              { opt.MSBuildParams with
                  Properties = [ "PackageVersion", release.NugetVersion ] @ opt.MSBuildParams.Properties } }
      )
    )
  )


let formatTargets =
  !! "src/**/*.fs" ++ "build.fsx" ++ "example/*.fsx"
  -- "src/*/obj/**/*.fs"
  -- "src/*/bin/**/*.fs"

Target.create "Format" (fun _ -> formatTargets |> String.concat " " |> dotnet "fantomas")

Target.create "Format.Check" (fun _ -> formatTargets |> String.concat " " |> sprintf "--check %s" |> dotnet "fantomas")

Target.create "All" ignore

"Clean" ==> "Build" ==> "All"

Target.runOrDefault "All"
