#load "LWC.fsx"

open System.Windows.Forms
open System.Drawing
open LWC


type IumButton() as this =
  inherit LWC()

  let clickevt = new Event<System.EventArgs>()
  let downevt = new Event<System.EventArgs>()
  let upevt = new Event<System.EventArgs>()
  let moveevt = new Event<System.EventArgs>()

  do this.Size <- SizeF(64.f, 64.f)

  let mutable text = ""
  let mutable selected = false
  let mutable color = new Color()
  do color <- Color.SlateGray

  member this.Click = clickevt.Publish
  member this.MouseDown = downevt.Publish
  member this.MouseUp = upevt.Publish
  member this.MouseMove = moveevt.Publish

  member this.Text
    with get() = text
    and set(v) = text <- v; this.Invalidate()

  member this.Color
    with get() = color
    and set(v) = color <- v; this.Invalidate()

  member this.Selected
    with get() = selected
    and set(v) = selected <- v
   
  override this.OnMouseUp e = upevt.Trigger(e); clickevt.Trigger(new System.EventArgs())
  override this.OnMouseMove e = moveevt.Trigger(e)
  override this.OnMouseDown e = downevt.Trigger(e)
  
  override this.OnPaint e =
    let g = e.Graphics
    use b = new SolidBrush(this.Color)
    g.FillRectangle(b,0,0,300,300)
    let sz = g.MeasureString(text, this.Parent.Font)
    g.DrawString(text, this.Parent.Font, Brushes.White, PointF((this.Size.Width - sz.Width) / 2.f, (this.Size.Height - sz.Height) / 2.f))