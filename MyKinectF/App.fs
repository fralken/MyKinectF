module MainApp

open System
open FsXaml

open Models

type App = XAML<"App.xaml">

(* Entry Point *)
[<STAThread>]
[<EntryPoint>]
let main argv =
    let ret = App().Root.Run()
    use k = Kinect.manager
    k |> ignore
    ret