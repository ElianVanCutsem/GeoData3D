// <copyright file="MainWindow.xaml.cs" company="Odisee">
//     Elian Van Cutsem - 2ICT2
// </copyright>
// <author>Elian Van Cutsem</author>

namespace Project2
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Media.Imaging;
    using System.Windows.Media.Media3D;
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using GeoJSON.Net.Feature;
    
    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window
    {
        #region <<fields>>
        /// <summary>
        /// this is the dataset that will be used upon load
        /// </summary>
        private static List<double> gemeenten = Belgium.Gemeenten.ToList<double>();

        /// <summary>
        /// This is the array with all geo map links
        /// </summary>
        private List<string> maps = new List<string>();

        /// <summary>
        /// The scale the maps will be displayed on
        /// </summary>
        private double zoomLevel = 5;

        /// <summary>
        /// The maximal X coordinate for all geo maps
        /// </summary>
        private double xmax = 0;

        /// <summary>
        /// The maximal Y coordinate for all geo maps
        /// </summary>
        private double ymax = 0;

        /// <summary>
        /// The minimal X coordinate for all geo maps
        /// </summary>
        private double ymin = 100000000;

        /// <summary>
        /// The minimal Y coordinate for all geo maps
        /// </summary>
        private double xmin = 100000000;

        /// <summary>
        /// The integer that holds the selection for a color
        /// </summary>
        private int colorCounter = 0;

        /// <summary>
        /// this is the dataset that will be used upon load
        /// </summary>
        private List<double> inwoners = Belgium.Inwoners.ToList<double>();

        /// <summary>
        /// this is the dataset that will be used upon load
        /// </summary>
        private List<double> arrayToUse = gemeenten;

        private System.Windows.Media.Color FeatureColor = System.Windows.Media.Color.FromArgb(255,100,100,100);

        #endregion

        #region <<START>>
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow" /> class.
        /// When the program start, do this:
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
            this.maps.Add("Belgium");
        }

        /// <summary>
        /// This method sends every map to the DrawMaps method
        /// </summary>
        private void DrawMaps()
        {
            for (int i = 0; i < this.maps.Count; i++)
            {
                this.DrawMap(@"..\..\geoJSON files\" + this.maps[i] + ".GeoJson");
            }
        }

        /// <summary>
        /// This method sends every map to the StartProcess method
        /// </summary>
        private void Start()
        {
            for (int i = 0; i < this.maps.Count; i++)
            {
                this.StartProcess(@"..\..\geoJSON files\" + this.maps[i] + ".GeoJson");
            }
        }

        /// <summary>
        /// This method makes a Geo collection from a map and searches the minima and maxima
        /// </summary>
        /// <param name="path">the provided geo map</param>
        private void StartProcess(string path)
        {
            FeatureCollection geoCollection = GetData.GetFeatures(path);
            this.GetMinAndMaxValues(geoCollection);
        }

        /// <summary>
        /// this method draws the map on the canvas
        /// </summary>
        /// <param name="path">The provided map</param>
        private void DrawMap(string path)
        {
            FeatureCollection geoCollection = GetData.GetFeatures(path);

            this.GetMinAndMaxValues(geoCollection);

            for (var i = 0; i < geoCollection.Features.Count(); i++)
            {
                string naam = geoCollection.Features[i].Properties["localname"].ToString();
                GeoJSON.Net.Geometry.MultiPolygon geo = (GeoJSON.Net.Geometry.MultiPolygon)geoCollection.Features[i].Geometry;
                this.colorCounter = i;
                Console.WriteLine("printing: " + naam);
                Console.WriteLine(this.colorCounter);
                for (int j = 0; j < geo.Coordinates.Count(); j++)
                {
                    for (int k = 0; k < geo.Coordinates[j].Coordinates.Count(); k++)
                    {
                        Polygon newPoly = new Polygon();
                        List<System.Windows.Point> pointList = new List<System.Windows.Point>();

                        // get all x and y points in a generic collection PointsList
                        for (int l = 0; l < geo.Coordinates[j].Coordinates[k].Coordinates.Count(); l++)
                        {
                            for (int q = 0; q < 1; q++)
                            {
                                double x = this.ConvertX(geo.Coordinates[j].Coordinates[k].Coordinates[l].Longitude);
                                double y = this.ConvertY(geo.Coordinates[j].Coordinates[k].Coordinates[l].Latitude);

                                pointList.Add(new System.Windows.Point(this.AddScalabilityX(x), this.AddScalabilityY(y)));
                            }
                        }

                        // point reduction
                        try
                        {
                            pointList = PointReduction.DouglasPeuckerReduction(pointList, double.Parse(Epsilon.Text));
                        }
                        catch
                        {
                            pointList = PointReduction.DouglasPeuckerReduction(pointList, 1.0);
                        }

                        List<PointF> newPoints = new List<PointF>();
                        PointCollection pointsCollection = new PointCollection();

                        for (int z = 0; z < pointList.Count(); z++)
                        {
                            newPoints.Add(new PointF((float)pointList[z].X, (float)pointList[z].Y));

                            // Console.WriteLine(PointList[z]);
                        }

                        // triangulation
                        // http://csharphelper.com/blog/2014/07/triangulate-a-polygon-in-c/
                        if (newPoints.ToArray().Length >= 3)
                        {
                            Polygon2 triPoly = new Polygon2(newPoints.ToArray());
                            List<Triangle> triangles = triPoly.Triangulate();

                            foreach (Triangle tri in triangles)
                            {
                                newPoly = new Polygon();
                                pointsCollection = new PointCollection();
                                for (int o = 0; o < tri.Points.Length; o++)
                                {
                                    pointsCollection.Add(new System.Windows.Point(tri.Points[o].X / 300, tri.Points[o].Y / 300));
                                }

                                // Console.WriteLine(pointsCollection.ToString());
                                this.Print3DTriangle(pointsCollection, this.colorCounter, this.arrayToUse);

                                // this.PrintPolygonTriangle(naam, pointsCollection, newPoly);
                            }

                            /*
                            pointsCollection = new PointCollection();

                            // Redraw the polygon.
                            if (newPoints.Count >= 3)
                            {
                                newPoly = new Polygon();
                                for (int o = 0; o < newPoints.Count; o++)
                                {
                                    pointsCollection.Add(new System.Windows.Point(newPoints[o].X, newPoints[o].Y));
                                }

                                // Draw the polygon.
                               // this.PrintPolygon(naam, pointsCollection, newPoly);
                            }*/
                            GetNewColor();
                            this.colorCounter++;
                        }
                    }
                }
            }
        }

        private void GetNewColor()
        {
            Random random = new Random();
            byte c1 = (byte) random.Next(0, 255);
            byte c2 = (byte) random.Next(0, 255);
            byte c3 = (byte) random.Next(0, 255);
            this.FeatureColor = System.Windows.Media.Color.FromArgb(255, c1, c2, c3);
            Console.WriteLine(this.FeatureColor.ToString());
        }

        #endregion

        #region <<ConvertJSON>>

        /// <summary>
        /// This method adds scalability to the x coordinates
        /// </summary>
        /// <param name="x">old x value</param>
        /// <returns>new x value</returns>
        private double AddScalabilityX(double x)
        {
            return x - this.xmin;
        }

        /// <summary>
        /// This method adds scalability to the y coordinates
        /// </summary>
        /// <param name="y">old y value</param>
        /// <returns>new y value</returns>
        private double AddScalabilityY(double y)
        {
            return y - this.ymin;
        }

        /// <summary>
        /// This takes the longitude of a coordinate and makes it a GPS Projection coordinate
        /// From: https://en.wikipedia.org/wiki/Web_Mercator_projection
        /// </summary>
        /// <param name="longitude">This is the original longitude</param>
        /// <returns>The converted longitude</returns>
        private double ConvertX(double longitude)
        {
            longitude = this.DegreeToRadian(longitude);
            longitude = 1000 / (2 * Math.PI) * Math.Pow(2, this.zoomLevel) * (longitude + Math.PI);
            return longitude;
        }

        /// <summary>
        /// This takes the latitude of a coordinate and makes it a GPS Projection coordinate
        /// From: https://en.wikipedia.org/wiki/Web_Mercator_projection
        /// </summary>
        /// <param name="latitude">This is the original latitude</param>
        /// <returns>The converted latitude</returns>
        private double ConvertY(double latitude)
        {
            latitude = this.DegreeToRadian(latitude);
            latitude = 1000 / (2 * Math.PI) * Math.Pow(2, this.zoomLevel) * (Math.PI - Math.Log(Math.Tan((Math.PI / 4) + (latitude / 2))));
            return latitude;
        }

        /// <summary>
        /// This method is used to change degree to radian
        /// only used for GPS projection
        /// </summary>
        /// <param name="angle">the provided angle</param>
        /// <returns>the value changed to radians</returns>
        private double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        /// <summary>
        /// This method checks all coordinates of a collection if they're smaller or bigger then minima and maxima
        /// </summary>
        /// <param name="collection">the provided point collection</param>
        private void GetMinAndMaxValues(FeatureCollection collection)
        {
            for (var i = 0; i < collection.Features.Count(); i++)
            {
                GeoJSON.Net.Geometry.MultiPolygon geo = (GeoJSON.Net.Geometry.MultiPolygon)collection.Features[i].Geometry;
                
                // get all maxes and mins
                for (int o = 0; o < geo.Coordinates.Count(); o++)
                {
                    for (int p = 0; p < geo.Coordinates[o].Coordinates.Count(); p++)
                    {
                        for (int q = 0; q < geo.Coordinates[o].Coordinates[p].Coordinates.Count(); q++)
                        {
                            double x = this.ConvertX(geo.Coordinates[o].Coordinates[p].Coordinates[q].Longitude);
                            double y = this.ConvertY(geo.Coordinates[o].Coordinates[p].Coordinates[q].Latitude);

                            if (x <= this.xmin)
                            {
                                this.xmin = x;
                            }

                            if (x > this.xmax)
                            {
                                this.xmax = x;
                            }

                            if (y < this.ymin)
                            {
                                this.ymin = y;
                            }

                            if (y > this.ymax)
                            {
                                this.ymax = y;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region <<3D SYSTEM>>

        /// <summary>
        /// This makes a 3D triangle
        /// </summary>
        /// <param name="points">The three points to create this</param>
        /// <param name="counter">color the triangle will receive</param>
        /// <param name="arrayToUse">the height used for this triangle</param>
        /// <returns>a new 3D triangle</returns>
        private MeshGeometry3D MakeTriangle(PointCollection points, int counter, List<double> arrayToUse)
        {
            Point3DCollection corners = new Point3DCollection();
            MeshGeometry3D triangle = new MeshGeometry3D();

            try
            {
                // Create the corners of the cube
                corners = new Point3DCollection
                {
                    // Point3D( x, h, y)
                    // Bottom Points
                    new Point3D(points[0].X, 0, points[0].Y),
                    new Point3D(points[1].X, 0, points[1].Y),
                    new Point3D(points[2].X, 0, points[2].Y),

                    // Top points
                    new Point3D(points[0].X, arrayToUse[counter], points[0].Y),
                    new Point3D(points[1].X, arrayToUse[counter], points[1].Y),
                    new Point3D(points[2].X, arrayToUse[counter], points[2].Y),
                };
            }
            catch
            {
                Console.WriteLine("tis weer antwerpen zeker");
            }

            triangle.Positions = corners;

            int[] indices =
            {
                // top
                4, 3, 5,

                // left
                5, 3, 0,
                2, 5, 0,

                // right
                0, 3, 4,
                4, 1, 0,

                // front
                4, 5, 1,
                5, 2, 1,

                // bottom
                0, 1, 2
            };

            triangle.TriangleIndices = new Int32Collection(indices);

            // adding TextureCoordinates for every corner
            triangle.TextureCoordinates = new PointCollection
            {
                new System.Windows.Point(1, 1),
                new System.Windows.Point(1, 1),
                new System.Windows.Point(0, 1),
                new System.Windows.Point(1, 1),

                new System.Windows.Point(0, 0),
                new System.Windows.Point(1, 0),
                new System.Windows.Point(0, 0),
                new System.Windows.Point(1, 0),

                new System.Windows.Point(1, 0),
                new System.Windows.Point(0, 0),
                new System.Windows.Point(1, 1),
                new System.Windows.Point(0, 1)
            };

            return triangle;
        }

        /// <summary>
        /// this function triggers when the 3D window is loaded
        /// </summary>
        /// <param name="p">The three points to create this</param>
        /// <param name="counter">color the triangle will receive</param>
        /// <param name="arrayToUse">the height used for this triangle</param>
        private void Print3DTriangle(PointCollection p, int counter, List<double> arrayToUse)
        {
            GeometryModel3D triangle = new GeometryModel3D();
            triangle.Geometry = this.MakeTriangle(p, counter, arrayToUse);

            // Make the surface's material using an image brush.
            triangle.Material = new DiffuseMaterial(new SolidColorBrush(this.FeatureColor));
            DirectionalLight dirLight1 = new DirectionalLight();
            dirLight1.Color = Colors.White;
            dirLight1.Direction = new Vector3D(0, -4, -1);

            DirectionalLight dirLight2 = new DirectionalLight();
            dirLight2.Color = Colors.White;
            dirLight2.Direction = new Vector3D(0, 4, 0);

            this.SetCamera();

            Model3DGroup modelGroup = new Model3DGroup();
            modelGroup.Children.Add(triangle);

            modelGroup.Children.Add(dirLight2);
            modelGroup.Children.Add(dirLight1);
            ModelVisual3D modelsVisual = new ModelVisual3D
            {
                Content = modelGroup
            };

            myViewport.Children.Add(modelsVisual);

            AxisAngleRotation3D axis = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0);
            RotateTransform3D rotate = new RotateTransform3D(axis);
            triangle.Transform = rotate;
            DoubleAnimation rotateAnimation = new DoubleAnimation();

            NameScope.SetNameScope(this.myViewport, new NameScope());
            this.myViewport.RegisterName("cubeaxis", axis);
            Storyboard.SetTargetName(rotateAnimation, "cubeaxis");
            Storyboard.SetTargetProperty(rotateAnimation, new PropertyPath(AxisAngleRotation3D.AngleProperty));
            Storyboard rotCube = new Storyboard();
            rotCube.Children.Add(rotateAnimation);
            rotCube.Begin(this.myViewport);
        }

        /// <summary>
        /// This sets and resets the camera in the 3D space
        /// </summary>
        private void SetCamera()
        {
            PerspectiveCamera camera1 = new PerspectiveCamera
            {
                NearPlaneDistance = 0.1,
                FieldOfView = 45,
                Position = new Point3D(2, 1.8, 2.5),
                LookDirection = new Vector3D(-1.6, -2, -3),
            };

            myViewport.Camera = camera1;
        }

        #endregion

        #region <<GUI controls>>

        /// <summary>
        /// This triggers when the camera center button is clicked
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void CameraCenter_Click(object sender, RoutedEventArgs e)
        {
            this.SetCamera();
        }

        /// <summary>
        /// This triggers when a new dataset is clicked on the GUI
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataCombo.SelectedIndex == 0)
            {
                this.arrayToUse = gemeenten;
            }
            else if (dataCombo.SelectedIndex == 1)
            {
                this.arrayToUse = this.inwoners;
            }
        }

        /// <summary>
        /// This triggers when a new map is selected on the GUI
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            this.maps.Clear();
            this.arrayToUse.Clear();
            gemeenten.Clear();
            this.inwoners.Clear();

            if (CheckBoxBelgium.IsChecked == true)
            {
                this.maps.Add("Belgium");
                for (int i = 0; i < Belgium.Gemeenten.Length; i++)
                {
                    gemeenten.Add(Belgium.Gemeenten[i]);
                }

                for (int i = 0; i < Belgium.Inwoners.Length; i++)
                {
                    this.inwoners.Add(Belgium.Inwoners[i]);
                }
            }

            if (CheckBoxNetherlands.IsChecked == true)
            {
                this.maps.Add("Netherlands");
                for (int i = 0; i < Netherlands.Gemeenten.Length; i++)
                {
                    gemeenten.Add(Netherlands.Gemeenten[i]);
                }

                for (int i = 0; i < Netherlands.Inwoners.Length; i++)
                {
                    this.inwoners.Add(Netherlands.Inwoners[i]);
                }
            }

            this.Start();
        }

        /// <summary>
        /// This draws the maps again when clicked on the button
        /// </summary>
        /// <param name="sender">The parameter is not used.</param>
        /// <param name="e">The parameter is not used.</param>
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            myViewport.Children.Clear();
            this.DrawMaps();
        }

        #endregion

        #region <<Old 2D SYSTEM>>

        /// <summary>
        /// This method prints a polygon without filling on the canvas
        /// </summary>
        /// <param name="name">the name of the feature</param>
        /// <param name="p">the point collection of the polygon</param>
        /// <param name="polygonToDraw">the actual polygon that will be drawn</param>
        private void PrintPolygon(string name, PointCollection p, Polygon polygonToDraw)
        {
            polygonToDraw.Points = p;
            polygonToDraw.Stroke = System.Windows.Media.Brushes.Black;
            polygonToDraw.StrokeThickness = 1;

            // Console.WriteLine("printed " + name);
            PaintSurface.Children.Add(polygonToDraw);
        }

        /// <summary>
        /// This method prints a triangle with filling on the canvas
        /// </summary>
        /// <param name="name">the name of the feature</param>
        /// <param name="p">the point collection of the polygon</param>
        /// <param name="polygonToDraw">the actual polygon that will be drawn</param>
        private void PrintPolygonTriangle(string name, PointCollection p, Polygon polygonToDraw)
        {
            polygonToDraw.Points = p;
            polygonToDraw.StrokeThickness = this.zoomLevel / 10;
            
            // polygonToDraw.Stroke = ColorPicker.BrushColors[this.colorCounter];
            // polygonToDraw.Fill = ColorPicker.BrushColors[this.colorCounter];

            // Console.WriteLine("printed " + name);
            PaintSurface.Children.Add(polygonToDraw);
        }

        #endregion
    }
}
