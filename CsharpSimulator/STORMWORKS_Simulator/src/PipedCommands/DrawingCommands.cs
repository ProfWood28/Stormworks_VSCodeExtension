﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel.Composition;

namespace STORMWORKS_Simulator
{
    [Export(typeof(IPipeCommandHandler))]
    public class DrawRect : IPipeCommandHandler
    {
        public bool CanHandle(string commandName) => commandName == "RECT";

        public void Handle(MainVM vm, string[] commandParts)
        {
            if (commandParts.Length < 7)
            {
                return;
            }

            var screenNumber = int.Parse(commandParts[1]);
            var screen = vm.ScreenVMs[screenNumber];

            var filled = commandParts[2] == "1";
            var x       = (int)double.Parse(commandParts[3]) * screen.DrawScale;
            var y       = (int)double.Parse(commandParts[4]) * screen.DrawScale;
            var width   = (int)double.Parse(commandParts[5]) * screen.DrawScale;
            var height  = (int)double.Parse(commandParts[6]) * screen.DrawScale;

            var shape = new Path
            {
                Data = new RectangleGeometry(new Rect(x, y, width, height)),
                StrokeThickness = 2,
                Fill   = filled ? screen.Monitor.Color : null,
                Stroke = !filled ? screen.Monitor.Color : null
            };

            screen.Draw(shape);
        }
    }

    [Export(typeof(IPipeCommandHandler))]
    public class DrawCircle : IPipeCommandHandler
    {
        public bool CanHandle(string commandName) => commandName == "CIRCLE";

        public void Handle(MainVM vm, string[] commandParts)
        {
            if (commandParts.Length < 6)
            {
                return;
            }

            var screenNumber = int.Parse(commandParts[1]);
            var screen = vm.ScreenVMs[screenNumber];

            var filled = commandParts[2] == "1";
            var x       = (int)double.Parse(commandParts[3]) * screen.DrawScale;
            var y       = (int)double.Parse(commandParts[4]) * screen.DrawScale;
            var radius  = (int)double.Parse(commandParts[5]) * screen.DrawScale;

            var shape = new Path
            {
                Data = new EllipseGeometry(new Rect(x, y, radius, radius)),
                StrokeThickness = 2,
                Fill    = filled  ? screen.Monitor.Color : null,
                Stroke  = !filled ? screen.Monitor.Color : null
            };

            screen.Draw(shape);
        }
    }

    [Export(typeof(IPipeCommandHandler))]
    public class DrawLine : IPipeCommandHandler
    {
        public bool CanHandle(string commandName) => commandName == "LINE";

        public void Handle(MainVM vm, string[] commandParts)
        {
            if (commandParts.Length < 6)
            {
                return;
            }

            var screenNumber = int.Parse(commandParts[1]);
            var screen = vm.ScreenVMs[screenNumber];

            var x   = (int)double.Parse(commandParts[2])  * screen.DrawScale;
            var y   = (int)double.Parse(commandParts[3])  * screen.DrawScale;
            var x2  = (int)double.Parse(commandParts[4])  * screen.DrawScale;
            var y2  = (int)double.Parse(commandParts[5])  * screen.DrawScale;

            var line = new Line();
            line.X1 = x;
            line.X2 = x2;
            line.Y1 = y;
            line.Y2 = y2;
            line.Stroke = screen.Monitor.Color;

            screen.Draw(line);
        }
    }


    // todo: from here downards
    [Export(typeof(IPipeCommandHandler))]
    public class DrawText : IPipeCommandHandler
    {
        public static FontFamily MonitorFont = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#PixelFont");

        public bool CanHandle(string commandName) => commandName == "TEXT";

        public void Handle(MainVM vm, string[] commandParts)
        {
            if (commandParts.Length < 5)
            {
                return;
            }

            var screenNumber = int.Parse(commandParts[1]);
            var screen = vm.ScreenVMs[screenNumber];

            var x = (int)(double.Parse(commandParts[2]))   * screen.DrawScale;
            var y = (int)(double.Parse(commandParts[3])-1) * screen.DrawScale;
            var text = commandParts[4];

            var textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.Foreground = screen.Monitor.Color;
            textBlock.FontSize   = 5 * screen.DrawScale;
            textBlock.FontFamily = MonitorFont;

            Canvas.SetLeft(textBlock, x);
            Canvas.SetTop(textBlock, y);

            screen.Draw(textBlock);
        }
    }

    [Export(typeof(IPipeCommandHandler))]
    public class DrawTextbox : IPipeCommandHandler
    {
        public static FontFamily MonitorFont = new FontFamily(new Uri("pack://application:,,,/"), "./Fonts/#PixelFont");

        public bool CanHandle(string commandName) => commandName == "TEXTBOX";

        public void Handle(MainVM vm, string[] commandParts)
        {
            if (commandParts.Length < 9)
            {
                return;
            }

            var screenNumber = int.Parse(commandParts[1]);
            var screen = vm.ScreenVMs[screenNumber];

            var x               = (int)(double.Parse(commandParts[2])) * screen.DrawScale;
            var y               = (int)(double.Parse(commandParts[3])-1) * screen.DrawScale;
            var horizontalAlign = int.Parse(commandParts[4]);
            var verticalAlign   = int.Parse(commandParts[5]);
            var width           = (int)double.Parse(commandParts[6]) * screen.DrawScale;
            var height          = (int)double.Parse(commandParts[7]) * screen.DrawScale;
            var text            = commandParts[8];

            var textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.HorizontalAlignment = (HorizontalAlignment)(horizontalAlign - 1);
            textBlock.VerticalAlignment = (VerticalAlignment)(verticalAlign - 1);
            textBlock.Width = width;
            textBlock.Height = height;
            textBlock.Foreground = screen.Monitor.Color;
            textBlock.FontSize = 8 * screen.DrawScale;
            textBlock.FontFamily = MonitorFont;

            Canvas.SetLeft(textBlock, x);
            Canvas.SetTop(textBlock, y);

            screen.Draw(textBlock);
        }
    }

    [Export(typeof(IPipeCommandHandler))]
    public class DrawTriangle : IPipeCommandHandler
    {
        public bool CanHandle(string commandName) => commandName == "TRIANGLE";

        public void Handle(MainVM vm, string[] commandParts)
        {
            if (commandParts.Length < 9)
            {
                return;
            }

            var screenNumber = int.Parse(commandParts[1]);
            var screen = vm.ScreenVMs[screenNumber];

            var filled = commandParts[1] == "1";
            var p1x = (int)double.Parse(commandParts[3]) * screen.DrawScale;
            var p1y = (int)double.Parse(commandParts[4]) * screen.DrawScale;
                                     
            var p2x = (int)double.Parse(commandParts[5]) * screen.DrawScale;
            var p2y = (int)double.Parse(commandParts[6]) * screen.DrawScale;
                                              
            var p3x = (int)double.Parse(commandParts[7]) * screen.DrawScale;
            var p3y = (int)double.Parse(commandParts[8]) * screen.DrawScale;

            var points = new PointCollection();
            points.Add(new Point(p1x, p1y));
            points.Add(new Point(p2x, p2y));
            points.Add(new Point(p3x, p3y));

            var polygon = new Polygon();
            polygon.Fill   =  filled ? screen.Monitor.Color : null;
            polygon.Stroke = !filled ? screen.Monitor.Color : null; ;
            polygon.Points = points;
            screen.Draw(polygon);
        }
    }

    [Export(typeof(IPipeCommandHandler))]
    public class SetColour : IPipeCommandHandler
    {
        public bool CanHandle(string commandName) => commandName == "COLOUR";

        public void Handle(MainVM vm, string[] commandParts)
        {
            if (commandParts.Length < 6)
            {
                return;
            }

            var screenNumber = int.Parse(commandParts[1]);
            var screen = vm.ScreenVMs[screenNumber];

            var r = Convert.ToByte(Math.Min(255, Math.Max(0, (int)double.Parse(commandParts[2]))));
            var g = Convert.ToByte(Math.Min(255, Math.Max(0, (int)double.Parse(commandParts[3]))));
            var b = Convert.ToByte(Math.Min(255, Math.Max(0, (int)double.Parse(commandParts[4]))));
            var a = Convert.ToByte(Math.Min(255, Math.Max(0, (int)double.Parse(commandParts[5]))));

            screen.Monitor.Color = new SolidColorBrush(Color.FromArgb(a, r, g, b));
        }
    }

    [Export(typeof(IPipeCommandHandler))]
    public class ClearScreen : IPipeCommandHandler
    {
        public bool CanHandle(string commandName) => commandName == "CLEAR";

        public void Handle(MainVM vm, string[] commandParts)
        {
            if (commandParts.Length < 2)
            {
                return;
            }

            var screenNumber = int.Parse(commandParts[1]);
            var screen = vm.ScreenVMs[screenNumber];

            screen.ClearScreen();
        }
    }
}
