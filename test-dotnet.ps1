# This script semi-automates testing the dotnet templates.
# It creates a solution at the given directory, instantiates all templates,
# builds, and runs them one after the other so that they can be visually checked.
param (
    [Parameter(Mandatory=$true)]
    [string] $directory,
    [string] $nupkg
)

if (test-path "$directory") {
    echo "This directory already exists."
    exit 1
}
if ("$nupkg") {
    dotnet new -u WebSharper.Templates
    dotnet new -i "$nupkg"
}

dotnet new sln -o "$directory"
pushd "$directory"

try {

    dotnet new websharper-lib -o Fs.Lib -lang f#
    dotnet sln add Fs.Lib/Fs.Lib.fsproj

    dotnet new websharper-lib -o Cs.Lib -lang c#
    dotnet sln add Cs.Lib/Cs.Lib.csproj

    dotnet new websharper-ext -o Fs.Ext -lang f#
    dotnet sln add Fs.Ext/Fs.Ext.fsproj

    dotnet new websharper-web -o Fs.Web -lang f#
    dotnet sln add Fs.Web/Fs.Web.fsproj
    dotnet add Fs.Web/Fs.Web.fsproj reference Fs.Lib/Fs.Lib.fsproj
    dotnet add Fs.Web/Fs.Web.fsproj reference Fs.Ext/Fs.Ext.fsproj
    dotnet add Fs.Web/Fs.Web.fsproj reference Cs.Lib/Cs.Lib.csproj

    dotnet new websharper-web -o Cs.Web -lang c#
    dotnet sln add Cs.Web/Cs.Web.csproj

    dotnet new websharper-spa -o Fs.Spa -lang f#
    dotnet sln add Fs.Spa/Fs.Spa.fsproj

    dotnet new websharper-spa -o Cs.Spa -lang c#
    dotnet sln add Cs.Spa/Cs.Spa.csproj

    dotnet new websharper-html -o Fs.Html -lang f#
    dotnet sln add Fs.Html/Fs.Html.fsproj

    dotnet new websharper-html -o Cs.Html -lang c#
    dotnet sln add Cs.Html/Cs.Html.csproj

    dotnet build

    start-process http://localhost:5000

    echo "=== TESTING C# SPA; close its window and press Enter to continue ==="
    start-process dotnet $("run", "-p", "Cs.Spa/Cs.Spa.csproj")
    read-host

    echo "=== TESTING F# SPA; close its window and press Enter to continue ==="
    start-process dotnet $("run", "-p", "Fs.Spa/Fs.Spa.fsproj")
    read-host

    echo "=== TESTING C# Web; close its window and press Enter to continue ==="
    start-process dotnet $("run", "-p", "Cs.Web/Cs.Web.csproj")
    read-host

    echo "=== TESTING F# Web; close its window and press Enter to continue ==="
    start-process dotnet $("run", "-p", "Fs.Web/Fs.Web.fsproj")
    read-host

    echo "=== TESTING C# HTML; press Enter to continue ==="
    ./Cs.Html/bin/html/index.html
    read-host

    echo "=== TESTING F# HTML; press Enter to continue ==="
    ./Fs.Html/bin/html/index.html
    read-host

} finally { popd }
