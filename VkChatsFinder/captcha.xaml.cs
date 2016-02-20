using System;
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
using System.Windows.Shapes;

namespace VkChatsFinder
{
	/// <summary>
	/// Логика взаимодействия для captcha.xaml
	/// </summary>
	public partial class captcha : Window
	{
		internal string user_cap;
		public captcha(string c_url)
		{
			InitializeComponent();
			cap_img.DataContext = c_url;
		}

		private void cap_go_Click(object sender, RoutedEventArgs e)
		{
			this.user_cap = cap_text.Text;
			this.Close();
		}

		private void cap_text_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				cap_go_Click(this, e);
			}
		}
	}
}
