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

namespace STORMWORKS_Simulator
{
    public partial class MainWindow : Window
    {
        public StormworksMonitor Monitor;
        public MainVM ViewModel;
        public SocketConnection VSConnection;
        public System.Threading.Timer KeepAliveTimer;

        public MainWindow()
        {
            Logger.SetLog(@"C:\personal\STORMWORKS_VSCodeExtension\debug.txt");

            InitializeComponent();

            ViewModel = new MainVM();
            VSConnection = new SocketConnection(ViewModel);
            VSConnection.OnPipeClosed += Pipe_OnPipeClosed;

            ViewModel.OnScreenResolutionChanged += (s, vm) => CanvasZoom.Reset();
            ViewModel.OnScreenResolutionChanged += (s, vm) => VSConnection.SendMessage("SCREENSIZE", $"{vm.ScreenNumber + 1}|{vm.Monitor.Size.X}|{vm.Monitor.Size.Y}");
            ViewModel.OnScreenTouchChanged += SendTouchDataIfChanged;
            ViewModel.OnPowerChanged += (s, vm) => VSConnection.SendMessage("SCREENPOWER", $"{vm.ScreenNumber + 1}|{ (vm.IsPowered ? "1" : "0") }");

            DataContext = ViewModel;

            KeepAliveTimer = new System.Threading.Timer(OnTickTimer, null, 100, 100);
        }

        private void OnTickTimer(object state)
        {   // sends simple Alive message once every 100 millis, to ensure the connection is still alive
            // allows the UI to close sooner than it might otherwise, once the debugger ends
            try
            {
                VSConnection.SendMessage("ALIVE");
            }
            catch (Exception e)
            {   // squash the error or it will confuse users in VSCode wondering why there's a bright red error
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Application.Current.Shutdown();
                });
            }
        }

        private void Pipe_OnPipeClosed(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.Shutdown();
            });
        }

        private void OnResetClicked(object sender, RoutedEventArgs e)
        {
            CanvasZoom.Reset();
        }

        private void SendTouchDataIfChanged(object sender, ScreenVM vm)
        {
            // only send the update if things actually changed
            var newCommand = $"{vm.ScreenNumber + 1}|{(vm.IsLDown ? '1' : '0') }|{ (vm.IsRDown ? '1' : '0') }|{vm.TouchPosition.X}|{vm.TouchPosition.Y}";
            if (newCommand != vm.LastTouchCommand)
            {
                vm.LastTouchCommand = newCommand;
                VSConnection.SendMessage("TOUCH", newCommand);
            }
        }

        private void OnAddScreenClicked(object sender, RoutedEventArgs e)
        {
            ViewModel.GetOrAddScreen(ViewModel.ScreenVMs.Count + 1);
        }

        // dirty event handling, but easier than going through all the bindings for this; as they're not designed to work with commands by default
        // redirect the event manually, to the correct screenVM
        private void Canvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e) { ((sender as Canvas).DataContext as ScreenVM).OnRightButtonUp(sender as Canvas, e);}

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) { ((sender as Canvas).DataContext as ScreenVM).OnLeftButtonUp(sender as Canvas, e); }

        private void Canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e) { ((sender as Canvas).DataContext as ScreenVM).OnRightButtonDown(sender as Canvas, e); }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { ((sender as Canvas).DataContext as ScreenVM).OnLeftButtonDown(sender as Canvas, e); }

        private void Canvas_MouseLeave(object sender, MouseEventArgs e) { ((sender as Canvas).DataContext as ScreenVM).OnMouseLeave(sender as Canvas, e); }

        private void Canvas_MouseEnter(object sender, MouseEventArgs e) { ((sender as Canvas).DataContext as ScreenVM).OnMouseEnter(sender as Canvas, e); }

        private void Canvas_MouseMove(object sender, MouseEventArgs e) { ((sender as Canvas).DataContext as ScreenVM).OnMouseMove(sender as Canvas, e); }

        // really horrid
        private void Canvas_FrontInitialized(object sender, EventArgs e)
        {
            ((sender as Canvas).DataContext as ScreenVM).SetFront(sender as Canvas);
        }

        private void Canvas_BackInitialized(object sender, EventArgs e)
        {
            ((sender as Canvas).DataContext as ScreenVM).SetBack(sender as Canvas);
        }
    }
}
