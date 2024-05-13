using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

namespace CourseWork_471223028
{
    public partial class DrawingForm : Form
    {
        public DrawingForm()
        {
            InitializeComponent();
        }

        public enum ShapeEnum { None, Circle, Triangle, Rectangle };
        private ShapeEnum selectedShape = ShapeEnum.None;
        private List<Shape> shapes = new List<Shape>();

        private readonly Stack<List<Shape>> UndoneShapes = new Stack<List<Shape>>();
        private Stack<List<Shape>> RedoneShapes = new Stack<List<Shape>>();

        private string shapeOption = "All";
        private Color color = Color.Snow;
        private float brushThickness = 1f;

        private int _trRadius = 0;
        private int _cRadius = 0;
        private int _rWidth = 0;
        private int _rHeight = 0;

        private bool IsFilling = false;
        private bool isDragging = false;
        private Point mousePosition;
        private Shape selectedDraggingShape = null;

        private void pic_MouseClick(object sender, MouseEventArgs e)
        {
            if (selectedShape != ShapeEnum.None)
            {
                if (e.Button == MouseButtons.Left)
                {
                    SaveCurrentState();
                    Point coordinates = e.Location;

                    if (!IsFilling)
                    {
                        switch (selectedShape)
                        {
                            case ShapeEnum.Circle:
                                shapes.Add(new Circle(coordinates, _cRadius, color, brushThickness));
                                break;
                            case ShapeEnum.Triangle:
                                shapes.Add(new Triangle(coordinates, _trRadius, color, brushThickness));
                                break;
                            case ShapeEnum.Rectangle:
                                shapes.Add(new Rectangle(coordinates, _rWidth, _rHeight, color, brushThickness));
                                break;
                        }
                    }
                    else
                    {
                        FillShape(e.Location);
                    }

                    pic.Invalidate();
                }
            }
            else
            {
                MessageBox.Show("Please select a shape first.");
            }
        }

        private void pic_Paint(object sender, PaintEventArgs e)
        {
            IEnumerable<Shape> filteredShapes = shapes;

            switch (shapeOption)
            {
                case "Circles":
                    filteredShapes = shapes.Where(shape => shape.GetShapeType() == ShapeEnum.Circle);
                    break;
                case "Rectangles":
                    filteredShapes = shapes.Where(shape => shape.GetShapeType() == ShapeEnum.Rectangle);
                    break;
                case "Triangles":
                    filteredShapes = shapes.Where(shape => shape.GetShapeType() == ShapeEnum.Triangle);
                    break;
                case "All":
                default:
                    break;
            }

            foreach (var shape in filteredShapes)
            {
                shape.Draw(e.Graphics);
                if (shape.IsFilling)
                {
                    shape.Fill(e.Graphics);
                }
            }
        }

        private void btn_color_Click(object sender, EventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                Opacity = 0.8;
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    Opacity = 1;
                    color = colorDialog.Color;
                    box_color.BackColor = colorDialog.Color;
                }
            }
        }

        private void FillShape(Point coordinates)
        {
            foreach (var shape in shapes)
            {
                if (shape.IsShapeSelected(coordinates))
                {
                    shape.FillColor = color;
                    shape.IsFilling = true;

                }
            }
           
        }

        private void btn_fill_Click(object sender, EventArgs e)
        {
            IsFilling = true;
        }

        private void btn_triangle_Click(object sender, EventArgs e)
        {
            IsFilling = false;
            Opacity = .80;
            TriangleMenu tm = new TriangleMenu();
            tm.TriangleDataEntered += TriangleMenu_DataEntered;
            tm.ShowDialog();
            Opacity = 1;
            selectedShape = ShapeEnum.Triangle;
        }

        private void TriangleMenu_DataEntered(object sender, TriangleDataEventArgs e)
        {
            _trRadius = e.Radius;
        }

        private void btn_rectangle_Click(object sender, EventArgs e)
        {
            IsFilling = false;
            Opacity = .80;
            RectangleMenu rm = new RectangleMenu();
            rm.RectangleDataEntered += RectangleMenu_DataEntered;
            rm.ShowDialog();
            Opacity = 1;
            selectedShape = ShapeEnum.Rectangle;
        }

        private void RectangleMenu_DataEntered(object sender, RectangleDataEventArgs e)
        {
            _rWidth = e.Width;
            _rHeight = e.Height;
        }

        private void btn_circle_Click(object sender, EventArgs e)
        {
            IsFilling = false;
            Opacity = .80;
            CircleMenu cm = new CircleMenu();
            cm.CircleDataEntered += CircleMenu_DataEntered;
            cm.ShowDialog();
            Opacity = 1;
            selectedShape = ShapeEnum.Circle;
        }

        private void CircleMenu_DataEntered(object sender, CircleDataEventArgs e)
        {
            _cRadius = e.Radius;
        }

        private void btn_save_MouseClick(object sender, MouseEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "Image(*.jpg)|*.jpg|(*.*|*.*";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                Bitmap btm = new Bitmap(pic.Width, pic.Height);
                pic.DrawToBitmap(btm, new System.Drawing.Rectangle(0, 0, pic.Width, pic.Height));
                btm.Save(sfd.FileName, ImageFormat.Jpeg);
            }
        }

        private void btn_clear_Click(object sender, EventArgs e)
        {
            if (selectedShape != ShapeEnum.None)
            {
                DialogResult result = MessageBox.Show("Everything will be deleted!", "Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (result != DialogResult.OK) return;
            }

            color = Color.Snow;
            box_color.BackColor = color;
            IsFilling = false;

            UndoneShapes.Clear();
            RedoneShapes.Clear();
            shapes.Clear();
            pic.Invalidate();
        }

        private void btn_undo_Click(object sender, EventArgs e)
        {
            if (UndoneShapes.Count > 0)
            {
                RedoneShapes.Push(new List<Shape>(shapes));
                shapes = UndoneShapes.Pop();
                pic.Invalidate();
            }
        }

        private void btn_redo_Click(object sender, EventArgs e)
        {
            if (RedoneShapes.Count > 0)
            {
                UndoneShapes.Push(new List<Shape>(shapes));
                shapes = RedoneShapes.Pop();
                pic.Invalidate();
            }
        }

        private void SaveCurrentState()
        {
            UndoneShapes.Push(shapes.Select(shape => shape.Clone()).ToList());
            RedoneShapes.Clear();
        }

        private void ShapeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            shapeOption = ShapeComboBox.SelectedItem.ToString();
            pic.Invalidate();
        }

        private void pic_MouseHover(object sender, EventArgs e)
        {
            if (isDragging)
            {
                pic.Cursor = Cursors.Cross;
            }
        }

        private void pic_MouseLeave(object sender, EventArgs e)
        {
            pic.Cursor = Cursors.Default;
        }

        private void thicknessBar_Scroll(object sender, EventArgs e)
        {
            thicknessBar.Minimum = 1;
            thicknessBar.Maximum = 10;
            float thickness = (float)thicknessBar.Value;
            brushThickness = thickness;
            labelThickness.Text = $"Thickness: {thickness}";
        }

        private void pic_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                pic.Cursor = Cursors.Hand;
                foreach (var shape in shapes)
                {
                    if (shape.IsShapeSelected(e.Location))
                    {
                        isDragging = true;
                        mousePosition = e.Location;
                        selectedDraggingShape = shape;
                        
                        break;
                    }
                }
            }
        }

        private void pic_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && selectedDraggingShape != null)
            {
                int deltaX = e.X - mousePosition.X;
                int deltaY = e.Y - mousePosition.Y;
                selectedDraggingShape.Move(deltaX, deltaY);
                mousePosition = e.Location;
                pic.Invalidate();
            }
        }

        private void pic_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
            selectedDraggingShape = null;
            pic.Cursor = Cursors.Default;
        }
    }
}
