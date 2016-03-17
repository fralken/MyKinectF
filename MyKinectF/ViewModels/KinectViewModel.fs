namespace ViewModels

open FSharp
open LightBuzz.Vitruvius
open Models
open Models
open System.Windows.Media

type KinectViewModel() as self =
    inherit ViewModule.ViewModelBase()

    let greenScreen = GreenScreenBitmapGenerator.Create Kinect.manager.KinectSensor.CoordinateMapper
    let image = self.Factory.Backing(<@ self.ImageSource @>, greenScreen.Bitmap)
    let updateGreenScreen (frames: KinectFrames) =
        image.Value <- (frames |> Effects.updateGreenScreen greenScreen).Bitmap
        frames

    // Create the drawing group we'll use for drawing
    let drawingGroup = DrawingGroup()
    // Create an image source that we can use in our image control
    let drawImage = self.Factory.Backing(<@ self.DrawImage @>, DrawingImage(drawingGroup))
    let updateImage (frames: KinectFrames) =
        drawImage.Value <- frames |> Effects.drawImage drawingGroup drawImage.Value greenScreen
        frames

    let subscribe = Kinect.manager.MultiFrameSourceArrived
                    |> Observable.map updateGreenScreen
                    |> Observable.map updateImage
                    |> Observable.subscribe Kinect.manager.DisposeFrames
       
    member __.ImageSource = image.Value
    member __.DrawImage = drawImage.Value
