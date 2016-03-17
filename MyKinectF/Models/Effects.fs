namespace Models

open LightBuzz.Vitruvius
open Microsoft.Kinect
open System.Collections.Generic
open System.Windows
open System.Windows.Media

module Effects =

    // Constant for clamping Z values of camera space points from being negative
    let inferredZPositionClamp = 0.1f

    // Brush used for drawing hands that are currently tracked as closed
    let handClosedBrush = SolidColorBrush(Color.FromArgb(byte 128, byte 255, byte 0, byte 0))

    // Brush used for drawing hands that are currently tracked as opened
    let handOpenBrush = SolidColorBrush(Color.FromArgb(byte 128, byte 0, byte 255, byte 0))

    // Brush used for drawing hands that are currently tracked as in lasso (pointer) position
    let handLassoBrush = SolidColorBrush(Color.FromArgb(byte 128, byte 0, byte 0, byte 255))

    let handSize = double 30

    // Thickness of clip edge rectangles
    let clipBoundsThickness = double 10

    let updateGreenScreen (g: GreenScreenBitmapGenerator) (f: KinectFrames) =
        g.Update(f.Color, f.Depth, f.BodyIndex)
        g 

    /// <summary>
    /// Draws a hand symbol if the hand is tracked: red circle = closed, green circle = opened; blue circle = lasso
    /// </summary>
    /// <param name="handState">state of the hand</param>
    /// <param name="handPosition">position of the hand</param>
    /// <param name="drawingContext">drawing context to draw to</param>
    let drawHand (handState: HandState) (handPosition: Point) (drawingContext: DrawingContext) =
        match handState with
        | HandState.Closed -> drawingContext.DrawEllipse(handClosedBrush, null, handPosition, handSize, handSize)
        | HandState.Open -> drawingContext.DrawEllipse(handOpenBrush, null, handPosition, handSize, handSize)
        | HandState.Lasso -> drawingContext.DrawEllipse(handLassoBrush, null, handPosition, handSize, handSize)
        | _ -> ()

    /// <summary>
    /// Draws indicators to show which edges are clipping body data
    /// </summary>
    /// <param name="body">body to draw clipping information for</param>
    /// <param name="drawingContext">drawing context to draw to</param>
    let drawClippedEdges (body: Body) (drawingContext: DrawingContext) (b: ImageSource) =
        let clippedEdges = body.ClippedEdges

        let width = b.Width
        let height = b.Height

        if (clippedEdges.HasFlag(FrameEdges.Bottom)) then
            drawingContext.DrawRectangle(
                Brushes.Red,
                null,
                Rect(0.0, height - clipBoundsThickness, width, clipBoundsThickness))

        if (clippedEdges.HasFlag(FrameEdges.Top)) then
            drawingContext.DrawRectangle(
                Brushes.Red,
                null,
                Rect(0.0, 0.0, width, clipBoundsThickness))

        if (clippedEdges.HasFlag(FrameEdges.Left)) then
            drawingContext.DrawRectangle(
                Brushes.Red,
                null,
                Rect(0.0, 0.0, clipBoundsThickness, height))

        if (clippedEdges.HasFlag(FrameEdges.Right)) then
            drawingContext.DrawRectangle(
                Brushes.Red,
                null,
                Rect(width - clipBoundsThickness, 0.0, clipBoundsThickness, height))

    let drawImage (dg: DrawingGroup) (g: GreenScreenBitmapGenerator) (frames: KinectFrames) =
        use dc = dg.Open()
        let coordinateMapper = Kinect.manager.KinectSensor.CoordinateMapper
        let bodies = frames.Body.Bodies()
        // Draw a transparent background to set the render size
        dc.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, g.Bitmap.Width, g.Bitmap.Height))
        for body in bodies do
            if (body.IsTracked) then
                drawClippedEdges body dc g.Bitmap
                let joints = body.Joints
                // convert the joint points to depth (display) space
                let jointPoints = Dictionary<JointType, Point>()
                for jointType in joints.Keys do
                    // sometimes the depth(Z) of an inferred joint may show as negative
                    // clamp down to 0.1f to prevent coordinatemapper from returning (-Infinity, -Infinity)
                    let mutable position = joints.[jointType].Position
                    if (position.Z < float32 0.0) then
                        position.Z <- inferredZPositionClamp

                    if (g.isHD) then
                        let colorSpacePoint = coordinateMapper.MapCameraPointToColorSpace(position)
                        jointPoints.[jointType] <- Point(float colorSpacePoint.X, float colorSpacePoint.Y)
                    else
                        let depthSpacePoint = coordinateMapper.MapCameraPointToDepthSpace(position)
                        jointPoints.[jointType] <- Point(float depthSpacePoint.X, float depthSpacePoint.Y)

                drawHand body.HandLeftState jointPoints.[JointType.HandLeft] dc
                drawHand body.HandRightState jointPoints.[JointType.HandRight] dc

        // prevent drawing outside of our render area
        //_drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, _depthWidth, _depthHeight));
        dg.ClipGeometry <- RectangleGeometry(Rect(0.0, 0.0, g.Bitmap.Width, g.Bitmap.Height))
        
        // return frames, so that we can chain observable operations
        frames

