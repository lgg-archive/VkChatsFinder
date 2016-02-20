using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using System.IO;
using System.Web;
using System.Net;

using System.Windows;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VkChatsFinder
{
	internal class VkApi
	{
		private string auth_url = "https://oauth.vk.com/authorize?display=page&redirect_uri=https://oauth.vk.com/blank.html&scope=messages&response_type=token&v=";
		private string app_id;
		private string api_ver = "5.45";
		private string access_token;

		/* Set app id on Class create */
		internal VkApi(string app_id)
		{
			this.app_id = app_id;
		}

		/* Generates auth url */
		internal string getAuthUrl()
		{
			return this.auth_url +
				this.api_ver +
				"&client_id=" + this.app_id +
				"&refreshToken=" + Guid.NewGuid().ToString(); //this needed because I'm fucking tired of clearing cache/cookie problems
		}

		/* Search for access_token in web_url */
		internal bool getAccessToken(string web_url)
		{
			if (web_url.Contains("access_token"))
			{
				var match = Regex.Match(web_url, @"=(.*?)&");
				this.access_token = match.Groups[1].ToString();

				Properties.Settings.Default.access_token = this.access_token;
				Properties.Settings.Default.Save();

				return true;
			}
			else
			{
				return false;
			}
		}

		/* Load access_token from app settings */
		internal void getAccessTokenFromMem()
		{
			this.access_token = Properties.Settings.Default.access_token;
		}

		/* Requests vk api */
		internal JObject get(string method, string m_params)
		{
			string vkApiGetUrl = "https://api.vk.com/method/" + method + "?access_token=" + this.access_token + "&" + m_params + "&v=" + this.api_ver;
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(vkApiGetUrl);
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			using (StreamReader stream = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
			{
				var vkApiResponse = stream.ReadToEnd();
				return JObject.Parse(vkApiResponse);
			}
		}
	}
}
