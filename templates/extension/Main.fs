namespace $safeprojectname$

open WebSharper
open WebSharper.JavaScript
open WebSharper.InterfaceGenerator

module Definition =

    let I1 =
        Interface "I1"
        |+> [
                "test1" => T<string> ^-> T<string>
                "radius1" =@ T<float>
            ]

    let I2 =
        Generic - fun t1 t2 ->
            Interface "I2"
            |+> [
                    Generic - fun m1 -> "foo" => m1 * t1 ^-> t2
                ]

    let C1 =
        Class "C1"
        |+> Instance [
                "foo" =@ T<int>
            ]
        |+> Static [
                Constructor (T<unit> + T<int>)
                "mem"   => (T<unit> + T<int> ^-> T<unit>)
                "test2" => (TSelf -* T<int> ^-> T<unit>) * T<string> ^-> T<string>
                "radius2" =? T<float>
                |> WithSourceName "R2"
            ]

    let Assembly =
        Assembly [
            Namespace "Extension1" [
                 I1
                 I2
                 C1
            ]
        ]

[<Sealed>]
type Extension() =
    interface IExtension with
        member ext.Assembly =
            Definition.Assembly

[<assembly: Extension(typeof<Extension>)>]
do ()
