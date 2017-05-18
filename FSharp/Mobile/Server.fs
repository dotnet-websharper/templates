namespace $safeprojectname$

open WebSharper

module Server =

    [<Rpc>]
    let GetNames () =
        async {
            return [
                "John"
                "Paul"
            ]
        }
