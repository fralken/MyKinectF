namespace MainWindow

open FsXaml
open System.Windows

open Models

/// Top-level container for all XAML elements.
type MainWindow = XAML<"MainWindow.xaml", true>

type MainWindowController() = 
    inherit WindowViewController<MainWindow> ()

    //override __.OnInitialized _ =
    //    base.DisposeOnUnload(Kinect.manager)

    override __.OnLoaded view =
        let window = view.Root
        try
            let secondaryScreen = Forms.Screen.AllScreens |> Array.find (fun (s: Forms.Screen) -> not s.Primary)
            if (not window.IsLoaded) then window.WindowStartupLocation <- WindowStartupLocation.Manual
            let workingArea = secondaryScreen.WorkingArea
            let w = Point(float workingArea.Left, float workingArea.Top)
            let t = PresentationSource.FromVisual(window).CompositionTarget.TransformFromDevice
            let p = t.Transform(w)
            window.Left <- p.X
            window.Top <- p.Y
        with
            | e -> e |> ignore
        if (window.IsLoaded) then window.WindowState <- WindowState.Maximized
