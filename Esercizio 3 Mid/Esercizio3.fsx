#load "LWC.fsx"
#load "IumButton.fsx"

open System.Windows.Forms
open System.Drawing
open LWC
open IumButton

let f = new Form(Text="ImageEditor",TopMost=true,Size=Size(980,600))
f.Show()

type NavBut=
   | Load = 0

type NavBut2 =
   | Up = 0
   | Down = 1
   | Left = 2
   | Right = 3
   | RotateL = 4
   | RotateR = 5
   | ZoomUp = 6
   | ZoomDown = 7



type MyImage() as this = 
  inherit LWC()

  let mutable selected = false
  let mutable img : Image = null
  let mutable p1 = PointF(single(this.Location.X), single(this.Location.Y))
  let mutable p4 = PointF(this.Size.Width, this.Size.Height)
  let mutable p2 = PointF(p4.X / 5.f, p4.Y / 5.f)
  let mutable p3 = PointF(p4.X, p4.Y / 2.f)
  let semiTransBrush = new SolidBrush(Color.FromArgb(128, 0, 0, 255));
  let transformP (m:Drawing2D.Matrix) (p:PointF) =
    let a = [| PointF(single p.X, single p.Y) |]
    m.TransformPoints(a)
    a.[0]
  member this.Sel
   with get() = selected
   and set(v) = selected <- v
  member this.Immagine
   with get() = img
   and set(v) = img <- v

  member this.P1
   with get() = p1
   and set(v) = p1 <- v

  member this.P2 
   with get() = p2
   and set(v) = p2 <- v

  member this.P3
   with get() = p3
   and set(v) = p3 <- v

  member this.P4
   with get() = p4
   and set(v) = p4 <- v

  override this.OnMouseDown e = 
   base.OnMouseDown(e)

  override this.OnPaint e =
   let g = e.Graphics
   let w = int(this.P4.X - this.P1.X)
   let h = int(this.P4.Y - this.P1.Y)
   g.DrawImage(this.Immagine, new Rectangle(0, 0, h, w))
   if this.Sel then
     e.Graphics.FillRectangle(semiTransBrush,new RectangleF(this.P1.X,this.P1.Y,single(this.P4.Y - this.P1.Y),single(this.P4.X - this.P1.X)))
   base.OnPaint(e)  

//IMAGE EDITOR
type ImageEditor() as this =
  inherit LWContainer()
  do 
      this.SetStyle(ControlStyles.DoubleBuffer,true)
      this.SetStyle(ControlStyles.AllPaintingInWmPaint,true)
      this.SetStyle(ControlStyles.UserPaint,true)

  let mutable imgs = ResizeArray<MyImage>() //vector di primitivew
  let mutable sizeprimw = 0 //dimensione del vector immagini
  let mutable timg : Image = null
  let mutable ratio = 0.
  let mutable selection = new Rectangle()
  let mutable w2v = new Drawing2D.Matrix()
  let mutable v2w = new Drawing2D.Matrix()
  let buttonsv = [|
      new IumButton(Text="Load",Location=PointF(0.f,single(f.ClientSize.Height)-64.f));
    |]
  let buttonsman = [|
        new IumButton(Text="Up",Location=PointF(128.f,single(f.ClientSize.Height)-64.f));
        new IumButton(Text="Down",Location=PointF(192.f,single(f.ClientSize.Height)-64.f));
        new IumButton(Text="Left",Location=PointF(256.f,single(f.ClientSize.Height)-64.f));
        new IumButton(Text="Right",Location=PointF(320.f,single(f.ClientSize.Height)-64.f));
        new IumButton(Text="Rotate Left",Location=PointF(384.f,single(f.ClientSize.Height)-64.f));
        new IumButton(Text="Rotate Right",Location=PointF(448.f,single(f.ClientSize.Height)-64.f));
        new IumButton(Text="Zoom In",Location=PointF(512.f,single(f.ClientSize.Height)-64.f));
        new IumButton(Text="Zoom Out",Location=PointF(576.f,single(f.ClientSize.Height)-64.f)); 
    |]

  do buttonsman |> Seq.iter (fun b ->
        b.Parent <- this;
        this.LWControls.Add(b)
    )

  do buttonsv |> Seq.iter (fun b ->
     b.Parent <- this;
     this.LWControls.Add(b)
    )

  do
        buttonsv.[int(NavBut.Load)].MouseDown.Add(fun _ ->  
          if (buttonsv.[int(NavBut.Load)].Selected = true) then
            buttonsv.[int(NavBut.Load)].Selected <- false
            buttonsv.[int(NavBut.Load)].Color <- Color.SlateGray
          else
            buttonsv |> Seq.iter (fun b ->
                b.Selected <- false
                b.Color <- Color.SlateGray
            )
            buttonsv.[int(NavBut.Load)].Selected <- true
            buttonsv.[int(NavBut.Load)].Color <- Color.Red            
        )
  //---------------------OPERAZIONI DI TRASFORMAZIONE------------------//
  let translateW (tx, ty) =
        w2v.Translate(tx, ty)
        v2w.Translate(-tx, -ty, Drawing2D.MatrixOrder.Append)
  let rotateW a =
        w2v.Rotate a
        v2w.Rotate(-a, Drawing2D.MatrixOrder.Append)

  let rotateAtW p a =
        w2v.RotateAt(a, p)
        v2w.RotateAt(-a, p, Drawing2D.MatrixOrder.Append)

  let scaleW (sx, sy) =
        w2v.Scale(sx, sy)
        v2w.Scale(1.f/sx, 1.f/sy, Drawing2D.MatrixOrder.Append)
  let transformP (m:Drawing2D.Matrix) (p:Point) =
        let a = [| PointF(single p.X, single p.Y) |]
        m.TransformPoints(a)
        a.[0]

  let scrollBy dir =
        match dir with
        | NavBut2.Up -> (0.f,-10.f)
        | NavBut2.Down -> (0.f,10.f)
        | NavBut2.Left -> (-10.f,0.f)
        | NavBut2.Right -> (10.f,0.f)
        | _ -> (0.f,0.f)

  let translate (x, y) =
        let t = [| PointF(0.f, 0.f); PointF(x, y) |]
        v2w.TransformPoints(t)
        translateW(t.[1].X - t.[0].X, t.[1].Y - t.[0].Y)

  let translateAll (x, y) =
        imgs |> Seq.iter(fun b ->
          if b.Sel then
            let t = [| PointF(0.f, 0.f); PointF(x, y) |]
            b.V2W.TransformPoints(t)
            b.W2V.Translate(t.[1].X - t.[0].X, t.[1].Y - t.[0].Y)
            b.V2W.Translate(-(t.[1].X - t.[0].X), -(t.[1].Y - t.[0].Y), Drawing2D.MatrixOrder.Append)
        )
  let rotateAll x =
        imgs |> Seq.iter(fun b ->
        if b.Sel then
          let p = transformP b.V2W (Point(this.Width / 2, this.Height / 2))
          b.W2V.RotateAt(x, p)
          b.V2W.RotateAt(-x, p, Drawing2D.MatrixOrder.Append)  
        )

  let zoomAll x =
        imgs |> Seq.iter(fun b ->
        if b.Sel then
          let p = transformP b.V2W (Point(this.Width / 2, this.Height / 2))
          b.W2V.Scale(x, x)
          b.V2W.Scale(1.f/x, 1.f/x, Drawing2D.MatrixOrder.Append)
          let p1 = transformP b.V2W (Point(this.Width / 2, this.Height / 2))
          b.W2V.Translate(p1.X - p.X, p1.Y - p.Y)
          b.V2W.Translate(-(p1.X - p.X), -(p1.Y - p.Y), Drawing2D.MatrixOrder.Append)
        )

  let RotateBy dir =
        match dir with
        | NavBut2.RotateL -> -10.f
        | NavBut2.RotateR -> 10.f
        | _ -> 0.f

  let rotate x =
        let p = transformP v2w (Point(this.Width / 2, this.Height / 2))
        rotateAtW p x

  let ZoomBy dir =
        match dir with
        | NavBut2.ZoomUp -> 1.1f
        | NavBut2.ZoomDown -> (1.f / 1.1f)
        | _ -> 0.f

  let zoom x =
        let p = transformP v2w (Point(this.Width / 2, this.Height / 2))
        scaleW(x, x)
        let p1 = transformP v2w (Point(this.Width / 2, this.Height / 2))
        translateW(p1.X - p.X, p1.Y - p.Y)      
 //-------------fine delle operazioni---------------//

 //-------------------PULSANTI-------------//
  let handleCommand (k:Keys) =
        match k with
        | Keys.W -> 
          translateAll(0.f,-10.f)
          this.Invalidate()
        | Keys.D -> 
          translateAll(10.f,0.f)
          this.Invalidate()
        | Keys.A -> 
          translateAll(-10.f,0.f)
          this.Invalidate()
        | Keys.S -> 
          translateAll(0.f,10.f)
          this.Invalidate()
        | Keys.Q ->
          rotateAll -10.f
          this.Invalidate()
        | Keys.E ->
          rotateAll 10.f
          this.Invalidate()
        | Keys.Z ->
          zoomAll 1.1f
          this.Invalidate()
        | Keys.X ->
          zoomAll (1.f / 1.1f)
          this.Invalidate()
        | _ -> ()


 //-------------------FINE PULSANTI--------------//

 //----------------timer------------------//
  let scrollTimer = new Timer(Interval = 100)
  let rotateTimer = new Timer(Interval = 100)
  let zoomTimer = new Timer(Interval = 100)
  let mutable scrollDir = NavBut2.Up
  let mutable rotateDir = NavBut2.RotateL
  let mutable zoomDir = NavBut2.ZoomUp    

  do scrollTimer.Tick.Add(fun _ ->
      scrollBy scrollDir |> translateAll
      this.Invalidate()
    )
  do rotateTimer.Tick.Add(fun _ ->
      RotateBy rotateDir |> rotateAll
      this.Invalidate()
    )
  do zoomTimer.Tick.Add(fun _ ->
      ZoomBy zoomDir |> zoomAll
      this.Invalidate()
    )  
 //---------fine timer----------//

 //-----------PULSANTI PER LE TRASFORMAZIONI----------//
  do
      buttonsman.[int(NavBut2.Up)].MouseDown.Add(fun _ -> 
        scrollDir <- NavBut2.Up
        buttonsv |> Seq.iter (fun b ->
          b.Selected <- false
          b.Color <- Color.SlateGray
        )
        buttonsman.[int(NavBut2.Up)].Selected <- true
        buttonsman.[int(NavBut2.Up)].Color <- Color.Blue
      )
      buttonsman.[int(NavBut2.Up)].MouseUp.Add(fun _ ->
        buttonsman.[int(NavBut2.Up)].Selected <- false
        buttonsman.[int(NavBut2.Up)].Color <- Color.SlateGray
      )
      //UP
      buttonsman.[int(NavBut2.Down)].MouseDown.Add(fun _ -> 
        scrollDir <- NavBut2.Down
        buttonsv |> Seq.iter (fun b ->
          b.Selected <- false
          b.Color <- Color.SlateGray
        )
        buttonsman.[int(NavBut2.Down)].Selected <- true
        buttonsman.[int(NavBut2.Down)].Color <- Color.Blue
      )
      buttonsman.[int(NavBut2.Down)].MouseUp.Add(fun _ ->
        buttonsman.[int(NavBut2.Down)].Selected <- false
        buttonsman.[int(NavBut2.Down)].Color <- Color.SlateGray
      )
      //DOWN
      buttonsman.[int(NavBut2.Left)].MouseDown.Add(fun _ -> 
        scrollDir <- NavBut2.Left
        buttonsv |> Seq.iter (fun b ->
          b.Selected <- false
          b.Color <- Color.SlateGray
        )
        buttonsman.[int(NavBut2.Left)].Selected <- true
        buttonsman.[int(NavBut2.Left)].Color <- Color.Blue
      )
      buttonsman.[int(NavBut2.Left)].MouseUp.Add(fun _ ->
        buttonsman.[int(NavBut2.Left)].Selected <- false
        buttonsman.[int(NavBut2.Left)].Color <- Color.SlateGray
      )
      //LEFT
      buttonsman.[int(NavBut2.Right)].MouseDown.Add(fun _ -> 
        scrollDir <- NavBut2.Right
        buttonsv |> Seq.iter (fun b ->
          b.Selected <- false
          b.Color <- Color.SlateGray
        )
        buttonsman.[int(NavBut2.Right)].Selected <- true
        buttonsman.[int(NavBut2.Right)].Color <- Color.Blue
      )
      buttonsman.[int(NavBut2.Right)].MouseUp.Add(fun _ ->
        buttonsman.[int(NavBut2.Right)].Selected <- false
        buttonsman.[int(NavBut2.Right)].Color <- Color.SlateGray
      )
      //RIGHT
      buttonsman.[int(NavBut2.RotateL)].MouseDown.Add(fun _ -> 
        rotateDir <- NavBut2.RotateL
        buttonsv |> Seq.iter (fun b ->
          b.Selected <- false
          b.Color <- Color.SlateGray
        )
        buttonsman.[int(NavBut2.RotateL)].Selected <- true
        buttonsman.[int(NavBut2.RotateL)].Color <- Color.Blue
      )
      buttonsman.[int(NavBut2.RotateL)].MouseUp.Add(fun _ ->
        buttonsman.[int(NavBut2.RotateL)].Selected <- false
        buttonsman.[int(NavBut2.RotateL)].Color <- Color.SlateGray
      )
      //ROTATE LEFT
      buttonsman.[int(NavBut2.RotateR)].MouseDown.Add(fun _ -> 
        rotateDir <- NavBut2.RotateR
        buttonsv |> Seq.iter (fun b ->
          b.Selected <- false
          b.Color <- Color.SlateGray
        )
        buttonsman.[int(NavBut2.RotateR)].Selected <- true
        buttonsman.[int(NavBut2.RotateR)].Color <- Color.Blue
      )
      buttonsman.[int(NavBut2.RotateR)].MouseUp.Add(fun _ ->
        buttonsman.[int(NavBut2.RotateR)].Selected <- false
        buttonsman.[int(NavBut2.RotateR)].Color <- Color.SlateGray
      )
      //ROTATE RIGHT
      buttonsman.[int(NavBut2.ZoomUp)].MouseDown.Add(fun _ -> 
        zoomDir <- NavBut2.ZoomUp
        buttonsv |> Seq.iter (fun b ->
          b.Selected <- false
          b.Color <- Color.SlateGray
        )
        buttonsman.[int(NavBut2.ZoomUp)].Selected <- true
        buttonsman.[int(NavBut2.ZoomUp)].Color <- Color.Blue
      )
      buttonsman.[int(NavBut2.ZoomUp)].MouseUp.Add(fun _ ->
        buttonsman.[int(NavBut2.ZoomUp)].Selected <- false
        buttonsman.[int(NavBut2.ZoomUp)].Color <- Color.SlateGray
      )
      //ZOOM IN
      buttonsman.[int(NavBut2.ZoomDown)].MouseDown.Add(fun _ -> 
        zoomDir <- NavBut2.ZoomDown
        buttonsv |> Seq.iter (fun b ->
          b.Selected <- false
          b.Color <- Color.SlateGray
        )
        buttonsman.[int(NavBut2.ZoomDown)].Selected <- true
        buttonsman.[int(NavBut2.ZoomDown)].Color <- Color.Blue
      )
      buttonsman.[int(NavBut2.ZoomDown)].MouseUp.Add(fun _ ->
        buttonsman.[int(NavBut2.ZoomDown)].Selected <- false
        buttonsman.[int(NavBut2.ZoomDown)].Color <- Color.SlateGray
      )
      //ZOOM OUT
      //per il funzionamento dei tasti
      for v in [ NavBut2.Up; NavBut2.Down; NavBut2.Left; NavBut2.Right; ] do
        let idx = int(v)
        buttonsman.[idx].MouseDown.Add(fun _ -> scrollTimer.Start())
        buttonsman.[idx].MouseUp.Add(fun _ -> scrollTimer.Stop())
      for v in [ NavBut2.RotateL; NavBut2.RotateR ] do
        let idx = int(v)
        buttonsman.[idx].MouseDown.Add(fun _ -> rotateTimer.Start())
        buttonsman.[idx].MouseUp.Add(fun _ -> rotateTimer.Stop())
      for v in [ NavBut2.ZoomUp; NavBut2.ZoomDown ] do
        let idx = int(v)
        buttonsman.[idx].MouseDown.Add(fun _ -> zoomTimer.Start())
        buttonsman.[idx].MouseUp.Add(fun _ -> zoomTimer.Stop())
      //per il funzionamento dei tasti
      for v in [ NavBut2.Up; NavBut2.Down; NavBut2.Left; NavBut2.Right; ] do
        let idx = int(v)
        buttonsman.[idx].MouseDown.Add(fun _ -> scrollTimer.Start())
        buttonsman.[idx].MouseUp.Add(fun _ -> scrollTimer.Stop())
     
 //-----------FINE PULSANTI PER LE TRASFORMAZIONI--------//

  let transformP (m:Drawing2D.Matrix) (p:Point) =
        let a = [| PointF(single p.X, single p.Y) |]
        m.TransformPoints(a)
        a.[0]
  let ImageHitTest (mousew:Point) =
      for i in imgs.Count-1..-1..0 do
        let mutable trovato = false
        let h = imgs.[i]
        let p = transformP h.V2W mousew
        let rect = RectangleF(h.P1.X,h.P1.Y,(h.P4.X-h.P1.X), (h.P4.Y-h.P1.Y))
        trovato <- rect.Contains(p)
        if trovato then
          printfn("hit an image")
          if h.Sel then
            h.Sel <- false
          else
            h.Sel <- true          
        else
          printfn("hit nothing")
 
  member this.MyImage
      with get() = imgs
      and set(v) = imgs <- v

  override this.OnResize e =
     buttonsv |> Seq.iter ( fun b ->
            b.Location <- PointF(b.Location.X,single(f.ClientSize.Height) - 64.f)
        )
     buttonsman |> Seq.iter ( fun b ->
            b.Location <- PointF(b.Location.X,single(f.ClientSize.Height) - 64.f)
        )
  override this.OnMouseDown e = 
    printfn "mouse %f;%f" (single e.Location.X) (single e.Location.Y)
    if(not buttonsv.[0].Selected) then 
      ImageHitTest e.Location
    else  
      let od = new OpenFileDialog() 
      od.CheckFileExists <- true
      if od.ShowDialog() = DialogResult.OK then
        timg <- Image.FromFile(od.FileName)
        let temp = new MyImage(Immagine = timg, Mondo = true, Parent = this)
        this.LWControls.Add(temp)
        imgs.Add(temp)
        sizeprimw <- sizeprimw + 1
        imgs.[sizeprimw - 1].P1 <- PointF(0.f,0.f)
        imgs.[sizeprimw - 1].P4 <- PointF(imgs.[sizeprimw - 1].P1.X+single(timg.Width),imgs.[sizeprimw - 1].P1.Y+single(timg.Height))
        ratio <- float(timg.Width) / float(timg.Height)
    this.Invalidate()
    base.OnMouseDown(e)  

  override this.OnPaint e =
    let g = e.Graphics
    g.SmoothingMode <- Drawing2D.SmoothingMode.HighQuality
    let s = g.Save()
    this.W2V <- w2v
    this.V2W <- v2w
    g.Restore(s)        
    base.OnPaint(e)
  override this.OnKeyDown e = 
      handleCommand e.KeyData
      base.OnKeyDown e   



let c = new ImageEditor(Dock=DockStyle.Fill)
f.Controls.Add(c)

