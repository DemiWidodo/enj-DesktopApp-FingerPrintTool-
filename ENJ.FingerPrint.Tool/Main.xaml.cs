using ENJ.FingerPrint.SystemMessage;
using ENJ.FingerPrint.Tool;
using ENJ.FingerPrint.Tool.Commands;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ENJ.FingerPrint.Entity.ViewObject;
using ENJ.FingerPrint.Repository.CustomFunctions;
using ENJ.FingerPrint.Repository.Implements;
using ENJ.FingerPrint.Repository.Interfaces;

namespace ENJ.FingerPrint.Tool
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : Window
    {
        private IRemoteCheckInOutRepository remoteCheckInOutRepository = new RemoteCheckInOutRepository();
        private ILocalCheckInOutRepository localCheckInOutRepository = new LocalCheckInOutRepository();
        private bool remoteConn = false;
        private bool localConn = false;
        private FancyBalloon balloon = new FancyBalloon();

        public Main()
        {
            InitializeComponent();
            CheckingConnection();
        }


        private async void ApplicationStart()
        {
            await Task.Delay(10000);
            balloon = new FancyBalloon();
            balloon.BalloonText = "Started...";
            FingerNotifyIcon.ShowCustomBalloon(balloon, PopupAnimation.Slide, 10000);
            await Task.Delay(5000);
            StartInjectFingerPrintData();
        }


        private async void StartInjectFingerPrintData()
        {
            bool localConn = CheckLocalConnection();
            bool remoteConn = CheckRemoteConnection();
            await Task.Delay(5000);
            if (localConn && remoteConn)
            {
                InsertDataToRemoteServer();
            } else if (!localConn)
            {
                await Task.Delay(5000);
                balloon = new FancyBalloon();
                balloon.BalloonText = "Local Failed...";
                FingerNotifyIcon.ShowCustomBalloon(balloon, PopupAnimation.Slide, 10000);
                await Task.Delay(10000);
                ApplicationStart();
            } else if (!remoteConn)
            {
                await Task.Delay(5000);
                balloon = new FancyBalloon();
                balloon.BalloonText = "Remote Failed...";
                FingerNotifyIcon.ShowCustomBalloon(balloon, PopupAnimation.Slide, 10000);
                await Task.Delay(10000);
                ApplicationStart();
            }
        }


        private async void InsertDataToRemoteServer()
        {

            bool localConn = CheckLocalConnection();
            bool remoteConn = CheckRemoteConnection();
            await Task.Delay(5000);
            bool injectResult = remoteCheckInOutRepository.ProceedInjectFingerPrintData();

            if (localConn && remoteConn)
            {
                if (injectResult)
                {
                    await Task.Delay(5000);
                    bool compareLocalMDB = localCheckInOutRepository.CompareMDBLocalToFPCENTRAL();

                    if (compareLocalMDB)
                    {
                        await Task.Delay(5000);
                        bool compareRemoteMDB = remoteCheckInOutRepository.CompareFPCENTRALToMDLocal();

                        if (compareRemoteMDB)
                        {
                            balloon = new FancyBalloon();
                            balloon.BalloonText = "Inject Completed...";
                            FingerNotifyIcon.ShowCustomBalloon(balloon, PopupAnimation.Slide, 15000);
                            StartInjectFingerPrintData();
                        }
                        else if (!compareRemoteMDB)
                        {
                            await Task.Delay(5000);
                            StartInjectFingerPrintData();
                        }
                    }
                    else if (!compareLocalMDB)
                    {

                        await Task.Delay(5000);
                        bool compareRemoteMDB = remoteCheckInOutRepository.CompareFPCENTRALToMDLocal();

                        if (compareRemoteMDB)
                        {
                            balloon = new FancyBalloon();
                            balloon.BalloonText = "Inject Completed...";
                            FingerNotifyIcon.ShowCustomBalloon(balloon, PopupAnimation.Slide, 15000);
                            StartInjectFingerPrintData();
                        }
                        else if (!compareRemoteMDB)
                        {
                            await Task.Delay(5000);
                            StartInjectFingerPrintData();
                        }
                    }

                }
                else if (!injectResult)
                {
                    await Task.Delay(5000);
                    bool compareLocalMDB = localCheckInOutRepository.CompareMDBLocalToFPCENTRAL();

                    if (compareLocalMDB)
                    {
                        await Task.Delay(5000);
                        bool compareRemoteMDB = remoteCheckInOutRepository.CompareFPCENTRALToMDLocal();

                        if (compareRemoteMDB)
                        {
                            balloon = new FancyBalloon();
                            balloon.BalloonText = "Inject Completed...";
                            FingerNotifyIcon.ShowCustomBalloon(balloon, PopupAnimation.Slide, 15000);
                            StartInjectFingerPrintData();
                        }
                        else if (!compareRemoteMDB)
                        {
                            await Task.Delay(5000);
                            StartInjectFingerPrintData();
                        }
                    }
                    else if (!compareLocalMDB)
                    {

                        await Task.Delay(5000);
                        bool compareRemoteMDB = remoteCheckInOutRepository.CompareFPCENTRALToMDLocal();

                        if (compareRemoteMDB)
                        {
                            balloon = new FancyBalloon();
                            balloon.BalloonText = "Inject Completed...";
                            FingerNotifyIcon.ShowCustomBalloon(balloon, PopupAnimation.Slide, 15000);
                            StartInjectFingerPrintData();
                        }
                        else if (!compareRemoteMDB)
                        {
                            await Task.Delay(5000);
                            StartInjectFingerPrintData();
                        }
                    }
                }
            }
        }

        private bool CheckRemoteConnection()
        {            
            bool dataConn = remoteCheckInOutRepository.CheckRemoteConnection();
            return dataConn;
        }

        private bool CheckLocalConnection()
        {
            bool dataConn = localCheckInOutRepository.CheckLocalConnection();
            return dataConn;
        }


        private async void CheckingConnection()
        {
            await Task.Delay(10000);

            try
            {
                balloon = new FancyBalloon();
                balloon.BalloonText = "Connecting...";
                FingerNotifyIcon.ShowCustomBalloon(balloon, PopupAnimation.Slide, 20000);
                remoteConn = remoteCheckInOutRepository.CheckRemoteConnection();
                localConn = localCheckInOutRepository.CheckLocalConnection();

                if (remoteConn && localConn)
                {
                    await Task.Delay(10000);
                    balloon = new FancyBalloon();
                    balloon.BalloonText = "Connected";
                    FingerNotifyIcon.ShowCustomBalloon(balloon, PopupAnimation.Slide, 10000);
                    await Task.Delay(5000);
                    ApplicationStart();
                } else if (!remoteConn && !localConn)
                {
                    await Task.Delay(10000);
                    balloon = new FancyBalloon();
                    balloon.BalloonText = "Not Connected";
                    FingerNotifyIcon.ShowCustomBalloon(balloon, PopupAnimation.Slide, 10000);
                    this.CheckingConnection();
                } else if (!localConn)
                {
                    await Task.Delay(10000);
                    balloon = new FancyBalloon();
                    balloon.BalloonText = "Not Connected";
                    FingerNotifyIcon.ShowCustomBalloon(balloon, PopupAnimation.Slide, 10000);
                    this.CheckingConnection();
                } else if (!remoteConn)
                {
                    await Task.Delay(10000);
                    balloon = new FancyBalloon();
                    balloon.BalloonText = "Not Connected";
                    FingerNotifyIcon.ShowCustomBalloon(balloon, PopupAnimation.Slide, 10000);
                    this.CheckingConnection();
                }

            }
            catch (Exception)
            {
                
                throw;
            }

        }
    }
}
