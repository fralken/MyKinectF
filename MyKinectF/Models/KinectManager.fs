namespace Models

open Microsoft.Kinect
open System

type KinectFrames(depth: DepthFrame, color: ColorFrame, bodyIndex: BodyIndexFrame, body: BodyFrame) =
    interface IDisposable with
        member __.Dispose() =
            // dispose frames, if not null
            use d = depth
            use c = color
            use bi = bodyIndex
            use b = body
            (d, c, bi, b) |> ignore

    member __.Color = color
    member __.Depth = depth
    member __.BodyIndex = bodyIndex
    member __.Body = body
    member __.AreValid = depth <> null && color <> null && bodyIndex <> null && body <> null

type KinectManager() as self =
    let kinectSensor = KinectSensor.GetDefault()
    let multiFrameSourceReader = kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Depth ||| FrameSourceTypes.Color ||| FrameSourceTypes.BodyIndex ||| FrameSourceTypes.Body)
    let multiFrameSourceArrived = 
        multiFrameSourceReader.MultiSourceFrameArrived
        |> Observable.choose (fun e ->
            let multiSourceFrame = e.FrameReference.AcquireFrame() 
            match multiSourceFrame with
            | null -> None
            | m ->
                let kinectFrames = new KinectFrames( 
                                        m.DepthFrameReference.AcquireFrame(),
                                        m.ColorFrameReference.AcquireFrame(),
                                        m.BodyIndexFrameReference.AcquireFrame(),
                                        m.BodyFrameReference.AcquireFrame()
                                    )
                if (kinectFrames.AreValid) then
                    Some(kinectFrames)
                else
                    self.DisposeFrames kinectFrames
                    None
            )

    do
        kinectSensor.Open()

    interface IDisposable with
        member __.Dispose() =
            use m = multiFrameSourceReader // dispose it
            m |> ignore
            kinectSensor.Close()

    member __.KinectSensor = kinectSensor
    member __.MultiFrameSourceArrived = multiFrameSourceArrived
    member __.DisposeFrames (frames: KinectFrames) =
        use f = frames
        f |> ignore

// global kinect manager
module Kinect =
    let manager = new KinectManager()