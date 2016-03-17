namespace ViewModels

open FSharp
open LightBuzz.Vitruvius
open Models
open System
open System.Windows.Media

type FpsTick = {count: int; time: DateTime; min: float; max: float}

type KinectViewModel() as self =
    inherit ViewModule.ViewModelBase()

    let greenScreen = GreenScreenBitmapGenerator.Create Kinect.manager.KinectSensor.CoordinateMapper
    let image = self.Factory.Backing(<@ self.ImageSource @>, greenScreen.Bitmap)
    let updateGreenScreen (frames: KinectFrames) =
        image.Value <- (frames |> Effects.updateGreenScreen greenScreen).Bitmap
        frames

    // Create the drawing group we'll use for drawing
    let drawingGroup = DrawingGroup()

    let fps = self.Factory.Backing(<@ self.FPS @>, "")

    let countFps current _ =
        let {count=c; time=t; min=min; max=max} = current
        let now = DateTime.Now
        let elapsed = now - t
        if (elapsed.TotalMilliseconds >= 1000.0) then
            let last = float (c + 1) * 1000.0 / elapsed.TotalMilliseconds
            let new_min = if (min = 0.0 || last < min) then last else min
            let new_max = if (last > max) then last else max
            fps.Value <- sprintf "FPS: %.02f  min: %.02f  max: %.02f" last new_min new_max
            {count=0; time=now; min=new_min; max=new_max}
        else
            {count=c+1; time=t; min=min; max=max}

    let subscribe = Kinect.manager.MultiFrameSourceArrived
                    |> Observable.map updateGreenScreen
                    |> Observable.map (Effects.drawImage drawingGroup greenScreen)
                    |> Observable.map Kinect.manager.DisposeFrames
                    |> Observable.scan countFps {count=0; time=DateTime.Now; min=0.0; max=0.0}
                    |> Observable.subscribe ignore
       
    member __.ImageSource = image.Value
    member __.DrawImage = DrawingImage(drawingGroup)
    member __.FPS = fps.Value
