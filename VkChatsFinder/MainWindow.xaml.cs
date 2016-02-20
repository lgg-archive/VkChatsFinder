using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VkChatsFinder
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	/// 

	/**
	 * 
	 * All info:
	 *		https://github.com/littleguga/VkChatsFinder/
	 * 
	 */

	public partial class MainWindow : Window
	{
		internal VkApi vkApi = new VkApi("3219730");
		internal int counter = 1, limit = 5000, offset = 200;
		internal List<Task> tasks = new List<Task>();

		public MainWindow()
		{
			InitializeComponent();

			HideScriptErrors(web_browser, true);

			changeVis(btn_openall, false);
			visBrowser(false);
			visTable(false);
			visLoader(false);
			visStart(false);
		}

		/* Action on login btn click */
		private void btn_login_Click(object sender, RoutedEventArgs e)
		{
			visLoader(true);

			if (String.IsNullOrEmpty(Properties.Settings.Default.access_token))
			{
				visBrowser(true);
				web_browser.Navigate(vkApi.getAuthUrl());
			}
			else
			{
				vkApi.getAccessTokenFromMem();
				logSuccess();
			}
			restoreDefault();
		}

		/* Start chat finder on search btn click */
		private void btn_search_Click(object sender, RoutedEventArgs e)
		{
			visStart(false);
			visLoader(true);

			Task.Factory.StartNew(
				() =>
				{
					while (this.counter < this.limit)
					{
						if (checkChat(counter))
						{
							break;
						}
						this.counter++;
					}
				}
			).ContinueWith((t) =>
				{
					logAction("Stopped, max id: " + this.counter);
					visLoader(false);
					visTable(true);
					generateOpenAllBtn();
				}, 
				TaskScheduler.FromCurrentSynchronizationContext()
			);
		}

		/* Reset btn */
		private void btn_clear_Click(object sender, RoutedEventArgs e)
		{
			clearToken(sender, e);
		}

		/* Function for creating url for all chats */
		private string generateAllChats(int min, int max)
		{
			string url = "https://vk.com/im?sel=c" +min + "&peers=c";
			for (int j = min; j < max; j++)
			{
				url += j + "_c";
			}
			return url;
		}

		private void generateOpenAllBtn()
		{
			//@TODO: fix this to not open unexistable chats
			List<string> urls = new List<string>();

			for (int j = 1; j < this.counter; j = j + 40)
			{
				urls.Add(generateAllChats(j, j + 40));
			}

			btn_openall.Tag = urls;

			changeVis(btn_openall, true);
		}

		private void btn_openall_Click(object sender, RoutedEventArgs e)
		{
			foreach (string url in btn_openall.Tag as List<string>)
			{
				//This needed to not open empty DELETED chat with chat_id 2000000
				string u = url;
				if (url.EndsWith("_c"))
				{
					u = url.Substring(0, url.Length - 2);
				}

				System.Diagnostics.Process.Start(u);
			}
		}

		/* When browser loaded web page - check if url contents access_token */
		private void web_browser_LoadCompleted(object sender, NavigationEventArgs e)
		{
			if (vkApi.getAccessToken(web_browser.Source.ToString()))
			{
				logSuccess();
			}
		}

		/* Ignore alerts about javascript errors in WebBrowser */
		internal void HideScriptErrors(WebBrowser wb, bool hide)
		{
			var fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
			if (fiComWebBrowser == null) return;
			var objComWebBrowser = fiComWebBrowser.GetValue(wb);
			if (objComWebBrowser == null)
			{
				wb.Loaded += (o, s) => HideScriptErrors(wb, hide); //In case we are to early
				return;
			}
			objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { hide });
		}

		/* Clear cookie and cache */
		//@TODO: refactor this
		private void clearToken(object sender, RoutedEventArgs e)
		{
			//Clear app properties
			Properties.Settings.Default.access_token = String.Empty;
			Properties.Settings.Default.Save();

			//Clear token in vkApi Class
			vkApi.getAccessTokenFromMem();

			//---------------------To remove this
			//Clear cookies(urghh...trying to clear)
			web_browser.Navigate("javascript:void((function(){var a,b,c,e,f;f=0;a=document.cookie.split('; ');for(e=0;e<a.length&&a[e];e++){f++;for(b='.'+location.host;b;b=b.replace(/^(?:%5C.|[^%5C.]+)/,'')){for(c=location.pathname;c;c=c.replace(/.$/,'')){document.cookie=(a[e]+'; domain='+b+'; path='+c+'; expires='+new Date((new Date()).getTime()-1e11).toGMTString());}}}})())");
			web_browser.Navigate("about:blank");

			string[] Cookies = System.IO.Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Cookies));
			foreach (string currentFile in Cookies)
			{
				try
				{
					System.IO.File.Delete(currentFile);
				}
				catch (Exception ex)
				{

				}
			}
			string[] InterNetCache = System.IO.Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache));
			foreach (string currentFile in InterNetCache)
			{
				try
				{
					System.IO.File.Delete(currentFile);
				}
				catch (Exception ex)
				{

				}
			}
			//---------------------To remove this

			//It seems that it is really working
			dynamic document = web_browser.Document;
			document.ExecCommand("ClearAuthenticationCache", false, null);

			//Clear and hide data grid
			data_table.Items.Clear();
			data_table.Items.Refresh();
			visTable(false);

			//Hide loader(to prevent bugs)
			visStart(false);
			visLoader(false);
			changeVis(btn_openall, false);

			logAction("Logged out");
			btn_login.Content = "Login";
			btn_login.Click -= clearToken;
			btn_login.Click += new RoutedEventHandler(btn_login_Click);

			restoreDefault();
		}

		/* Enable default browser open link in dataGrid */
		private void OnHyperlinkClick(object sender, RoutedEventArgs e)
		{
			System.Diagnostics.Process.Start((sender as TextBlock).Tag.ToString());
		}

		/* Restore default values on logout */
		private void restoreDefault()
		{
			this.counter = 1;
			this.limit = 5000;
			this.offset = 200;

			changeVis(btn_openall, false);
			visTable(false); //fixed bug: user start app -> press search btn -> then login btn(loader image over datagrid)
		}
	}
}
