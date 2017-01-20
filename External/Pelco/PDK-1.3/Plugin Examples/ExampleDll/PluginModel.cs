﻿using Pelco.Phoenix.PluginHostInterfaces;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using PluginLogger;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace ExampleDllPlugin
{
    class PluginModel : INotifyPropertyChanged
    {
        #region ConstructorAndVariables
        private PluginLog _pluginLog = new PluginLog("TraceExampleDllPlugin");

        private MainUserControl _mainUserControl = null;
        private IHost _host = null;
        private IOCCHost1 _OCCHost1 = null;
        private IOCCHostJoystick _OCCHostJoystick = null;
        BackgroundWorker _worker = null;

        // Used for updating listbox on main UI with log contents.
        public ObservableCollection<string> LogEntries { get; set; }
        public Object LockObject = new Object();

        public System.Windows.Threading.Dispatcher Dispatcher { get; set; }

        public PluginModel(IHost host)
        {
            // Used for updating listbox on main UI with log contents.
            LogEntries = new ObservableCollection<string>(); // Collection of log entries to be displayed in the example list.
            BindingOperations.EnableCollectionSynchronization(LogEntries, LockObject);

            Dispatcher = null;

            TraceEvent(TraceEventType.Information, 0, "PluginModel() - Constructor");

            _host = host;
            if (_host != null)
            {
                _OCCHost1 = host.GetService<IOCCHost1>(); 
                TraceEvent(TraceEventType.Information, 0, "PluginModel() -> IHost.GetService<IOCCHost1>()");

                _OCCHostJoystick = host.GetService<IOCCHostJoystick>(); 
                TraceEvent(TraceEventType.Information, 0, "PluginModel() -> IHost.GetService<IOCCHostJoystick>()");

                if (_OCCHost1 == null || _OCCHostJoystick == null)
                {
                    HostReportFatalError("Unable to create required interfaces.", "IOCCHost1 and/or IOCCHostJoystick");
                }
            } 
            else
            {
                TraceEvent(TraceEventType.Information, 0, "PluginModel() -> Host interface is null.  We must be running as a stand alone app.");
            }
        }

        private Visibility _loading = Visibility.Visible;
        public Visibility Loading
        {
            get
            {
                return _loading;
            }
            set
            {
                if (_loading == value) return;
                _loading = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        #region UtilityMethods
        public void TraceEvent(TraceEventType traceEventType, int id, String message)
        {
            // Log the event to TraceExampleDllPlugin.txt file next to the .exe file IF the appropriate .config settings are set.
            _pluginLog.TraceEvent(traceEventType, id, message);

            // Add this log entry into the LogEntries list to show whats going on in the UI
            LogEntries.Add(message);
        }

        private void ShowMessage(string message)
        {
            if (Dispatcher == null) return;
            Dispatcher.Invoke((Action)(() =>
            {
                MessageBox.Show(message);
            }));
        }

        private bool CheckHostInterface(bool hostIsNull, string traceTxt)
        {
            if (hostIsNull)
            {
                TraceEvent(TraceEventType.Information, 0, "FAIL: Host interface not available: Attempting " + traceTxt);
                return false;
            }
            else
            {
                TraceEvent(TraceEventType.Information, 0, "Plug-in calling " + traceTxt);
                return true;
            }
        }
        #endregion

        #region INotifyPropertyChanged
        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region IHost
        public void HostReportFatalError(string userMessage, string fullExceptionText)
        {
            if (CheckHostInterface((_host==null), "HostReportFatalError()"))
            {
                TraceEvent(TraceEventType.Critical, 0, "UserMessage: " + userMessage + "\nFull Exception Text: " + fullExceptionText);
                _host.ReportFatalError(userMessage, fullExceptionText);
            }
        }

        public void HostRequestClose()
        {
            if (CheckHostInterface((_host == null), "HostRequestClose()"))
            {
                _host.RequestClose();
            }
        }

        public void HostRequestFullScreen()
        {
            if (CheckHostInterface((_host == null), "HostRequestFullScreen()"))
            {
                _host.RequestFullScreen();
            }
        }

        public void HostReturnFromFullScreen()
        {
            if (CheckHostInterface((_host == null), "HostReturnFromFullScreen()"))
            {
                _host.ReturnFromFullScreen();
            }
        }
        #endregion

        #region IOCCHost1
        public void Host1RegisterForThumbnailPreference(bool sendThumbnailPreference)
        {
            if (CheckHostInterface((_OCCHost1 == null), "Host1RegisterForThumbnailPreference()"))
            {
                _OCCHost1.RegisterForThumbnailPreference(sendThumbnailPreference);
            }
        }

        public void Host1RegisterForOnScreenNotifications(bool sendOnScreenNotifications)
        {
            if (CheckHostInterface((_OCCHost1 == null), "Host1RegisterForOnScreenNotifications()"))
            {
                _OCCHost1.RegisterForOnScreenNotifications(sendOnScreenNotifications);
            }
        }

        public void Host1InitiateStreams(System.Collections.Generic.List<string> cameraList)
        {
            if (CheckHostInterface((_OCCHost1 == null), "Host1InitiateStreams()"))
            {
                _OCCHost1.InitiateStreams(cameraList);
            }
        }

        public void Host1StoreCredentials(string credentials)
        {
            if (CheckHostInterface((_OCCHost1 == null), "Host1StoreCredentials()"))
            {
                _OCCHost1.StoreCredentials(credentials);
            }
        }
        #endregion

        #region IOCCHostJoystick
        public void HostJoystickRegisterForJoystickNotifications(bool sendJoystickNotifications)
        {
            if (CheckHostInterface((_OCCHostJoystick == null), "Host1RegisterForJoystickNotifications()"))
            {
                _OCCHostJoystick.RegisterForJoystickNotifications(sendJoystickNotifications);
            }
        }
        #endregion

        #region IPlugin
        public FrameworkElement PluginCreateControl()
        {
            // Note: DO NOT DO BLOCK THIS API FROM RETURNING. IT WILL MAKE THE OPS CENTER APPEAR TO HANG.
            // Message boxes, synchronous calls to a server, bad ideas here.  Create the framework element and return.
            if (_mainUserControl == null)
            {
                _mainUserControl = new MainUserControl();
                _mainUserControl.DataContext = this;
                _mainUserControl.Loaded += MainUserControl_Loaded;
                Dispatcher = _mainUserControl.Dispatcher;
            }

            return _mainUserControl;
        }

        #region LoadingMethods
        public void MainUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Note: DO NOT DO BLOCK THIS API FROM RETURNING. IT WILL MAKE THE OPS CENTER APPEAR TO HANG.
            // Message boxes, synchronous calls to a server, bad ideas here.  Spin up a new thread and do your work asynchronously.
            // Return from this asap.
            TraceEvent(TraceEventType.Information, 0, "MainUserControl_Loaded() -> Start a BackgroundWorker() to complete loading.");
            _worker = new BackgroundWorker();
            _worker.DoWork += new DoWorkEventHandler(worker_LoadInBackground);
            _worker.RunWorkerAsync();
        }

        void worker_LoadInBackground(object sender, DoWorkEventArgs e)
        {
            // Do background loading stuff here.
            Thread.Sleep(5000);
            // When you are done, hide the loading UI.

            if (Dispatcher == null) return;
            Dispatcher.Invoke((Action)(() =>
            {
                TraceEvent(TraceEventType.Information, 0, "worker_LoadInBackground() -> Finished loading in the background.");
                Loading = Visibility.Hidden;
            }));
        }
        #endregion

        public void PluginShutdown()
        {
            TraceEvent(TraceEventType.Information, 0, "PluginShutdown()");
            HostRequestClose();  // This is the last thing we do.  The Ops Center will unload us after we RequestClose().
        }
        #endregion

    }
}