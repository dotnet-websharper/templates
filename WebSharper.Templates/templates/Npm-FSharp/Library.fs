namespace WebSharper.Npm.FSharp

open WebSharper

/// Example functionality exported from the generated npm package.
[<JavaScript>]
module Library =

    /// Adds two numbers.
    let add x y = x + y

    /// Greets the given name.
    let greet name = "Hello, " + name + "!"
