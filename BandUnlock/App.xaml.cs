using System;
using System.Windows;

namespace BandUnlock
{
    public partial class App : Application
    {
        public App()
        {
            Console.WriteLine("App Constructor");
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Console.WriteLine("App Startup");

            base.OnStartup(e);
        }
    }
}