#r "paket:
source https://api.nuget.org/v3/index.json
nuget Fake.DotNet.Cli
nuget Fake.IO.FileSystem
nuget Fake.Core.Target
nuget Fake.Core.ReleaseNotes
nuget Fake.DotNet.AssemblyInfoFile //"

#load ".fake/build.fsx/intellisense.fsx"
#r "netstandard"

open System

open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators

Target.initEnvironment()

let dotnet cmd arg =
  let res = DotNet.exec id cmd arg
  if not res.OK then
    res.Messages |> String.concat "\n" |> Trace.trace
    failwithf "failed 'dotnet %s %s'" cmd arg

let fsiExec path =
  dotnet "fsi" $"--langversion:preview %s{path}"

let release = ReleaseNotes.load "RELEASE_NOTES.md"

Target.create "AssemblyInfo" (fun _ ->
  [
    AssemblyInfo.Version release.AssemblyVersion
    AssemblyInfo.FileVersion release.AssemblyVersion
    AssemblyInfo.InformationalVersion release.NugetVersion
  ]
  |> AssemblyInfoFile.createFSharp "./src/EffFs/AssemblyInfo.fs"
)

Target.create "Test" (fun _ ->
  !! "example/*.fsx"
  |> Seq.iter fsiExec
)

Target.create "Tool" (fun _ ->
  dotnet "tool" "update fake-cli"
)

Target.create "Clean" (fun _ ->
  !! "src/**/bin"
  ++ "src/**/obj"
  |> Shell.cleanDirs
)

Target.create "Build" (fun _ ->
  !! "src/**/*.*proj"
  |> Seq.iter (DotNet.build id)
)

Target.create "Pack" (fun _ ->
  !! "src/**/*.*proj"
  |> Seq.iter (DotNet.pack (fun opt ->
    { opt with
        Configuration = DotNet.BuildConfiguration.Release
        OutputPath = Some "output/packages/"
        MSBuildParams = {
          opt.MSBuildParams with
            Properties = [
              "PackageVersion", release.NugetVersion
            ] @ opt.MSBuildParams.Properties
        }
    }
  ))
)


Target.create "All" ignore

"Clean"
  ==> "Build"
  ==> "All"

Target.runOrDefault "All"
