//    
// This simple control plot a (random) graph changing every 200 milliseconds
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace TestForm
{
    public class Plot : Control
    {
        Timer timer;

        const int pointsNum = 50;

        public Plot()
          : this(2000)
        {
        }

        //graph each line and arc section individually... otherwise problems.
        public double scalefactor = 10;
        public List<List<double[]>> lines = new List<List<double[]>>();
        public List<List<double[]>> arcs = new List<List<double[]>>();
        public List<List<double[]>> vlines = new List<List<double[]>>();

        public Plot(int milliseconds)
        {
            //timer = new Timer();
            //timer.Interval = milliseconds;
            //timer.Tick += new EventHandler(timer_Tick);
            //timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            //this.Invalidate();
        }

        public void reset()
        {
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var dx0 = (int)(e.ClipRectangle.Width / 2);
            var dy0 = (int)(e.ClipRectangle.Height / 2);
      
            // draw border
            e.Graphics.FillRectangle(Brushes.Black, e.ClipRectangle);

            // draw X axis
            e.Graphics.DrawLine(new Pen(Color.Red), 0, e.ClipRectangle.Height / 2, e.ClipRectangle.Width, e.ClipRectangle.Height / 2);
            // draw Y axis
            e.Graphics.DrawLine(new Pen(Color.Red), e.ClipRectangle.Width / 2, 0, e.ClipRectangle.Width / 2, e.ClipRectangle.Height);
            // draw the points

            List<Point[]> points = new List<Point[]>();

            Pen yellowPen = new Pen(Color.Yellow);
            Pen whitePen = new Pen(Color.White);
            yellowPen.Width = 1f;
            whitePen.Width = 1f;
            yellowPen.Alignment = PenAlignment.Center;
            whitePen.Alignment = PenAlignment.Center;

            if (lines != null)
            {
                foreach (List<double[]> linept in lines)
                {
                    int x1 = (int)((linept[0][0]) * scalefactor) + dx0;
                    int y1 = (int)((-linept[0][1]) * scalefactor) + dy0;
                    int x2 = (int)((linept[1][0]) * scalefactor) + dx0;
                    int y2 = (int)((-linept[1][1]) * scalefactor) + dy0;
                    e.Graphics.DrawLine(yellowPen, x1, y1, x2, y2);
                }
            }

            if (arcs != null)
            {
                foreach (List<double[]> arcpt in arcs)
                {
                    int x1 = (int)((arcpt[0][0]) * scalefactor) + dx0;
                    int y1 = (int)((-arcpt[0][1]) * scalefactor) + dy0;
                    int x2 = (int)((arcpt[1][0]) * scalefactor) + dx0;
                    int y2 = (int)((-arcpt[1][1]) * scalefactor) + dy0;
                    e.Graphics.DrawLine(yellowPen, x1, y1, x2, y2);
                }
            }

            if (vlines != null)
            {
                foreach (List<double[]> vlinept in vlines)
                {
                    int x1 = (int)((vlinept[0][0]) * scalefactor) + dx0;
                    int y1 = (int)((-vlinept[0][1]) * scalefactor) + dy0;
                    int x2 = (int)((vlinept[1][0]) * scalefactor) + dx0;
                    int y2 = (int)((-vlinept[1][1]) * scalefactor) + dy0;
                    e.Graphics.DrawLine(whitePen, x1, y1, x2, y2);
                }
            }
            base.OnPaint(e);
        }
    }
}