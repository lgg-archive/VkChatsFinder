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

namespace VkChatsFinder
{
	public partial class MainWindow
	{
		private void logSuccess()
		{
			logAction("Logged in");

			visBrowser(false);
			visLoader(false);
			visStart(true);

			Dispatcher.Invoke(
				new Action(() =>
				{
					btn_login.Content = "Logout";
					btn_login.Click -= btn_login_Click;
					btn_login.Click += new RoutedEventHandler(clearToken);
				})
			);
		}

		private void logAction(string text)
		{
			Dispatcher.Invoke(
				new Action(() =>
				{
					login_alert.Content = text;
				})
			);
		}

		/* yep, this is stupid :D */
		private void visStart(bool visible)
		{
			changeVis(img_start, visible);
		}

		private void visLoader(bool visible)
		{
			changeVis(img_loader, visible);
		}

		private void visBrowser(bool visible)
		{
			changeVis(web_browser, visible);
		}

		private void visTable(bool visible)
		{
			changeVis(data_table, visible);
		}

		private void changeVis(UIElement obj, bool visible)
		{
			Dispatcher.Invoke(
				new Action(() =>
				{
					if (visible)
					{
						obj.Visibility = Visibility.Visible;
					}
					else
					{
						obj.Visibility = Visibility.Hidden;
					}
				})
			);
		}

		private void addData(object data)
		{
			Dispatcher.Invoke(
				new Action(() =>
				{
					data_table.Items.Add(data);
				})
			);
		}

		private string showCaptcha(string c_url)
		{
			captcha captchaForm = new captcha(c_url);
			this.IsEnabled = false;
			captchaForm.ShowDialog();
			this.IsEnabled = true;
			return captchaForm.user_cap;
		}
	}
}
