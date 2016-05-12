module MainApp

open System
open FsXaml

open Models

type App = XAML<"App.xaml">

(* Entry Point *)
[<STAThread>]
[<EntryPoint>]
let main argv =
    Wpf.installSynchronizationContext()
    let ret = App().Run(MainWindow.MainWindow())
    use k = Kinect.manager
    k |> ignore
    ret