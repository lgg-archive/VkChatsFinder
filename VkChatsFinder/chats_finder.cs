using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VkChatsFinder
{
	public partial class MainWindow
	{
		/* Generates interval of chats_id */
		private string generateInterval(int i, int offset)
		{
			int j = i + offset;
			string result = "";
			while (i < j)
			{
				result += i + ",";
				i++;
			}
			result = result.Remove(result.Length - 1); //Remove last ","
			return result;
		}

		/* Main function for chats search */
		private bool checkChat(int chat_id)
		{
			bool isResponseOk = false;
			JObject chats = vkApi.get(
				"messages.getChat", 
				"chat_ids=" + generateInterval(chat_id, this.offset)
			);

			//Captcha 
			try
			{
				if (Convert.ToInt32(chats["error"]["error_code"]) == 14)
				{
					string c_url = chats["error"]["captcha_img"].ToString();
					string c_sid = chats["error"]["captcha_sid"].ToString();

					//captcha_sid
					//captcha_key
					chats = vkApi.get(
						"messages.getChat",
						"chat_ids=" + generateInterval(chat_id, this.offset) +
						"&captcha_key=" + showCaptcha(c_url) +
						"&captcha_sid=" + c_sid
					);
				}
			}
			catch{ }

			try
			{
				isResponseOk = Convert.ToBoolean((chats["response"][0]["id"]));
			}
			catch
			{
				isResponseOk = false;
			}

			if (this.offset == 1)
			{
				try
				{
					if (Convert.ToBoolean((chats["response"][0]["id"])))
					{
						addData(
							new {
								tchat_id = chats["response"][0]["id"].ToString(),
								tchat_title = chats["response"][0]["title"].ToString(),
								chat_link = "https://vk.com/im?sel=c" + chats["response"][0]["id"].ToString()
							}
						);
						return false;
					}
					else
					{
						//There is no chats
						return true;
					}
				}
				catch
				{
					//There is no chats
					return true;
				}
			}
			else
			{
				if (isResponseOk)
				{
					int c_c = chats["response"].Count();
					int k = 0;
					while (k < c_c)
					{
						addData(
							new{
								tchat_id = chats["response"][k]["id"].ToString(),
								tchat_title = chats["response"][k]["title"].ToString(),
								chat_link = "https://vk.com/im?sel=c" + chats["response"][k]["id"].ToString()
							}
						);
						k++;
					}

					this.counter = this.counter + this.offset - 1;
					return false;
				}
				else
				{
					//Divide offset on 2 and try again
					this.offset = this.offset / 2;
					return checkChat(chat_id);
				}
			}
		}
	}
}
