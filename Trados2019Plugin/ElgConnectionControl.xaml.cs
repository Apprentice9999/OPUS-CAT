﻿using Sdl.LanguagePlatform.TranslationMemoryApi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
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

namespace OpusCatTranslationProvider
{
    /// <summary>
    /// OPUS models available through ELG can also be used to generate translation, but
    /// this requires registration on the ELG website. 
    /// </summary>
    public partial class ElgConnectionControl : UserControl, INotifyPropertyChanged
    {
        private ElgServiceConnection elgConnection;
        private string connectionStatus;
        private OpusCatOptions options;
        private Brush connectionColor;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public Brush ConnectionColor
        {
            get => connectionColor;
            set { connectionColor = value; NotifyPropertyChanged(); }
        }

        public string ConnectionStatus
        {
            get => connectionStatus;
            set
            {
                connectionStatus = value;
                NotifyPropertyChanged();
            }
        }

        public List<string> LanguagePairs { get; internal set; }

        public ElgConnectionControl()
        {
            InitializeComponent();
           
            this.DataContextChanged += ConnectionControl_DataContextChanged;

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                //BeginInvoke just makes sure that this is performed after data context has changed
                Dispatcher.BeginInvoke(new Action(StartCheck), System.Windows.Threading.DispatcherPriority.ContextIdle);
            }
        }

        private void StartCheck()
        {
            Task.Run(() => this.CheckLanguagePairs());
        }

        private void CheckLanguagePairs()
        {
            if (this.options.opusCatSource != OpusCatOptions.OpusCatSource.Elg)
            {
                return;
            }

            Dispatcher.Invoke(() => this.ConnectionStatus = $"Checking if models exist for project language pairs.");
            Dispatcher.Invoke(() => this.ConnectionColor = new RadialGradientBrush(Colors.Yellow, Colors.DarkGoldenrod));

            this.connectionStatus = "";
            foreach (var languagePair in this.LanguagePairs)
            {
                var pairsplit = languagePair.Split('-');
                var result = this.elgConnection.CheckLanguagePairAvailability(pairsplit[0], pairsplit[1]);
                if (result == null)
                {
                    Dispatcher.Invoke(() =>
                        {
                            this.ConnectionStatus = "No valid ELG credentials. Fetch new ELG credentials by following the instructions on the Connection tab";
                            this.ConnectionColor = new RadialGradientBrush(Colors.Red, Colors.DarkRed);
                        });
                    return;
                }
                else if (result.Value)
                {
                    Dispatcher.Invoke(() => this.ConnectionStatus += $"ELG model available for {languagePair}");
                }
                else if (!result.Value)
                {
                    Dispatcher.Invoke(() => this.ConnectionStatus += $"ELG model not available for {languagePair}");
                }
            }
            Dispatcher.Invoke(() => this.ConnectionColor = new RadialGradientBrush(Colors.LightGreen, Colors.Green));
        }

        private void ConnectionControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is IHasOpusCatOptions)
            {

                this.options = ((IHasOpusCatOptions)e.NewValue).Options;
                var credStore = ((OpusCatOptionControl)this.DataContext).CredentialStore;
                var elgCreds = new TradosElgCredentialWrapper(credStore);
                this.elgConnection = new ElgServiceConnection(elgCreds);
            }
        }
        
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
        
        private void ConfirmCode_Click(object sender, RoutedEventArgs e)
        {
            var enteredCode = this.ElgSuccessCodeBox.Text;
            this.ConnectionStatus = "Fetching token with success code";
            if (!String.IsNullOrWhiteSpace(enteredCode))
            {
                Task.Run(() =>
                    {
                        this.elgConnection.GetAccessAndRefreshToken(enteredCode);
                        if (this.elgConnection.Status == ElgServiceConnection.ElgConnectionStatus.Ok)
                        {
                            this.CheckLanguagePairs();
                        }
                        else
                        {
                            Dispatcher.Invoke(() => this.ConnectionStatus = $"Success code was not accepted, try again.");
                        }
                    });
            }
        }

    }


}
