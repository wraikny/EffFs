# !/bin/sh

dotnet build src/EffFs
dotnet fsi --exec example/example.fsx
