#WebSharper Proxy project template

The proxy template is intended for creating a JavaScript-only accompanying
library for an existing .NET library. Add code files to the project, and it
will produce a `.dll` file that has no .NET content, but contains full
WebSharper translation and metadata. It can be referenced next to the original
library in another project, so that both server-side and client-side
functionality are available.

`JavaScript` attribute annotations are not needed, it is implied for the
whole project.

Steps:

1. In `wsconfig.json` set the `"proxyTargetName"` to the original assembly name.
Make sure this  project has a different assembly name,
for example `OriginalLib.Proxy`.
2. Add the code files of the original library to this project.
3. Compile, if there are `not found in JavaScript compilation` errors, additional
proxies are needed. You can add extra code files with WebSharper proxies.
(Use `[<WebSharper.Proxy(typeof<TargetType>)>]` attribute.)