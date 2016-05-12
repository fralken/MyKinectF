namespace MainWindow

open FsXaml
open System.Windows

open Models

/// Top-level container for all XAML elements.
type MainWindowBase = XAML<"MainWindow.xaml">

type MainWindow() as window =
    inherit MainWindowBase()

    do
        window.Loaded.Add(fun _ ->
            try
                let secondaryScreen = Forms.Screen.AllScreens |> Array.find (fun (s: Forms.Screen) -> not s.Primary)
                let workingArea = secondaryScreen.WorkingArea
                let w = Point(float workingArea.Left, float workingArea.Top)
                let t = PresentationSource.FromVisual(window).CompositionTarget.TransformFromDevice
                let p = t.Transform(w)
                window.Left <- p.X
                window.Top <- p.Y
            with
                | e -> e |> ignore
            window.WindowState <- WindowState.Maximized)
