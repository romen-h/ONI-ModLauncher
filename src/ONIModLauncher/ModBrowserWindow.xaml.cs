using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Web.WebView2.Core;

namespace ONIModLauncher
{
    /// <summary>
    /// Interaction logic for ModBrowserWindow.xaml
    /// </summary>
    public partial class ModBrowserWindow : Window
    {
        private static ModBrowserWindow s_instance;

        public static ModBrowserWindow Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new ModBrowserWindow();
                }

                return s_instance;
            }
        }

        private ulong _currentWorkshopID = 0;
		private bool? _subscribed = null;

		DispatcherTimer _timer;

		internal bool Shutdown
		{ get; set; } = false;

        public ModBrowserWindow()
        {
            InitializeComponent();

			_timer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
			_timer.Interval = TimeSpan.FromMilliseconds(500);
			_timer.Tick += Timer_Tick;
			_timer.Start();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
			if (!Shutdown)
			{
				e.Cancel = true;
				Hide();
			}
        }

        private void Timer_Tick(object sender, EventArgs e)
		{
			if (_currentWorkshopID > 0)
			{
				CheckSubscribed();
			}
		}

		private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
	        WindowState = WindowState.Minimized;
        }

        private void maximizeButton_Click(object sender, RoutedEventArgs e)
        {
	        if (WindowState != WindowState.Maximized)
	        {
		        WindowState = WindowState.Maximized;
	        }
	        else
	        {
		        WindowState = WindowState.Normal;
	        }
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
	        Close();
        }

        private void BrowseBackBtn_OnClick(object sender, RoutedEventArgs e)
        {
            if (webView.CanGoBack)
            {
				webView.GoBack();
            }
        }

        private void BrowseForwardBtn_OnClick(object sender, RoutedEventArgs e)
        {
	        if (webView.CanGoForward)
	        {
                webView.GoForward();
	        }
        }

        private void WebView_OnNavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
			subscribeBtn.IsEnabled = false;
			unsubscribeBtn.IsEnabled = false;

	        subscribeBtn.Visibility = Visibility.Collapsed;
			unsubscribeBtn.Visibility = Visibility.Collapsed;

            _currentWorkshopID = 0;
			_subscribed = null;
        }

		private void WebView_OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            Title = webView.CoreWebView2.DocumentTitle;

			browseBackBtn.IsEnabled = webView.CanGoBack;
			browseForwardBtn.IsEnabled = webView.CanGoForward;

            subscribeBtn.Visibility = Visibility.Collapsed;

			if (webView.Source.AbsolutePath == "/sharedfiles/filedetails/")
			{
				string query = webView.Source.Query;
				if (query != null && query.Length > 0)
				{
					query = query.Substring(1);
					string[] queries = query.Split('&');
					foreach (string q in queries)
					{
						if (q.StartsWith("id="))
						{
							string idStr = q.Substring(3);
							if (ulong.TryParse(idStr, out _currentWorkshopID))
							{
								CheckSubscribed();
							}
						}
					}
				}
			}
		}

		private void SubscribeBtn_OnClick(object sender, RoutedEventArgs e)
		{
			if (_currentWorkshopID > 0)
			{
				webView.ExecuteScriptAsync($"SubscribeItem({_currentWorkshopID}, 457140);");
			}
		}

		private void UnsubscribeBtn_OnClick(object sender, RoutedEventArgs e)
		{
			if (_currentWorkshopID > 0)
			{
				webView.ExecuteScriptAsync($"SubscribeItem({_currentWorkshopID}, 457140);");
			}
		}

		private async void CheckSubscribed()
		{
			_subscribed = null;
			var response = await webView.CoreWebView2.ExecuteScriptAsync("$J(\"#SubscribeItemBtn\").hasClass(\"toggled\")");
			if (bool.TryParse(response, out bool subscribed))
			{
				_subscribed = subscribed;
				if (_subscribed == true)
				{
					SteamIntegration.Instance.AddManagedContentID(_currentWorkshopID);
				}
			}
			RefreshSubscriptionButtons();
		}

		private void RefreshSubscriptionButtons()
		{
			if (!Dispatcher.CheckAccess())
			{
				Dispatcher.Invoke(RefreshSubscriptionButtons);
				return;
			}

			subscribeBtn.IsEnabled = true;
			unsubscribeBtn.IsEnabled = true;

			subscribeBtn.Visibility = _subscribed == false ? Visibility.Visible : Visibility.Collapsed;
			unsubscribeBtn.Visibility = _subscribed == true ? Visibility.Visible : Visibility.Collapsed;
		}
    }
}
