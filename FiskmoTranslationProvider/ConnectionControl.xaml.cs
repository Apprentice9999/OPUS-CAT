﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace FiskmoTranslationProvider
{
    /// <summary>
    /// Interaction logic for ConnectionControl.xaml
    /// </summary>
    public partial class ConnectionControl : UserControl, INotifyPropertyChanged
    {
        private string connectionStatus;
        private ObservableCollection<string> allModelTags;
        private bool connectionExists;
        
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        private void ServicePortBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void FetchServiceData(string host, string port)
        {

            string connectionResult;
            try
            {
                var serviceLanguagePairs = FiskmöMTServiceHelper.ListSupportedLanguages(host,port);
                IEnumerable<string> modelTagLanguagePairs;
                if (this.LanguagePairs != null)
                {
                    var projectLanguagePairsWithMt = serviceLanguagePairs.Intersect(this.LanguagePairs);
                    modelTagLanguagePairs = projectLanguagePairsWithMt;
                    if (projectLanguagePairsWithMt.Count() == 0)
                    {
                        connectionResult = "No MT models available for the language pairs of the project";
                    }
                    else if (this.LanguagePairs.Count == projectLanguagePairsWithMt.Count())
                    {
                        connectionResult = "MT models available for all the language pairs of the project";
                    }
                    else
                    {
                        connectionResult = $"MT models available for some of the language pairs of the project: {String.Join(", ", projectLanguagePairsWithMt)}";
                    }
                }
                else
                {
                    modelTagLanguagePairs = serviceLanguagePairs;
                    connectionResult = $"MT models available for following language pairs: {String.Join(", ", serviceLanguagePairs)}";
                }
                

                //Get a list of model tags that are supported for these language pairs
                List<string> modelTags = new List<string>();
                foreach (var languagePair in modelTagLanguagePairs)
                {
                    modelTags.AddRange(FiskmöMTServiceHelper.GetLanguagePairModelTags(host, port, languagePair.ToString()));
                }

                Dispatcher.Invoke(() => UpdateModelTags(modelTags));
            }
            catch (Exception ex) when (ex is EndpointNotFoundException || ex is CommunicationObjectFaultedException)
            {
                connectionResult = $"No connection to Fiskmö MT service at {host}:{port}.";
            }

            Dispatcher.Invoke(() => this.ConnectionStatus = connectionResult);
        }



        private void UpdateModelTags(List<string> tags)
        {
            this.AllModelTags.Clear();

            foreach (var tag in tags)
            {
                this.AllModelTags.Add(tag);
            }

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

        public ObservableCollection<string> AllModelTags { get => allModelTags; set { allModelTags = value; NotifyPropertyChanged(); } }

        public bool NoConnection
        {
            get => connectionExists;
            set
            {
                connectionExists = value; NotifyPropertyChanged();
            }
        }

        public List<string> LanguagePairs { get; set; }

        public void AddModelTag(string tag)
        {
            
            //It's possible that the options contain a tag which is not present
            //at the service. Include that tag in the list, since the omission might
            //be due to the service not being up properly etc.
            if (tag != "" && tag != null)
            {
                if (!this.AllModelTags.Contains(tag))
                {
                    this.AllModelTags.Add(tag);
                }
            }
            
        }

        public ConnectionControl()
        {
            this.AllModelTags = new ObservableCollection<string>();
            InitializeComponent();

            //Fetch data only after data context has been set and the bindings have been resolved.
            Dispatcher.BeginInvoke(new Action(StartFetch), System.Windows.Threading.DispatcherPriority.ContextIdle);
        }

        private void StartFetch()
        {
            var host = this.ServiceAddressBoxElement.Text;
            var port = this.ServicePortBoxElement.Text;
            Task.Run(() => this.FetchServiceData(host, port));
        }

        private void RetryConnection_Click(object sender, RoutedEventArgs e)
        {
            this.StartFetch();
        }

        private void SaveAsDefault_Click(object sender, RoutedEventArgs e)
        {
            FiskmoTpSettings.Default.MtServicePort = this.ServicePortBoxElement.Text;
            FiskmoTpSettings.Default.MtServiceAddress = this.ServiceAddressBoxElement.Text;
            FiskmoTpSettings.Default.Save();
        }
    }
}