﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace NetworkGameCopier
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>

    public partial class App : Application
    {
       
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // Perform tasks at application exit
           FtpManager.GetInstance().StopFtpServer();
        }

    }
    

}