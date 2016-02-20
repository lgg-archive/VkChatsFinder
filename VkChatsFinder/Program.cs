using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Windows;

namespace VkChatsFinder
{
	internal static class Program
	{
		[STAThread]
		private static void Main(params string[] args)
		{
			AppDomain.CurrentDomain.AssemblyResolve += (sender, e) => loadEmbeddedAssembly(e.Name);

			VkChatsFinder.App app = new VkChatsFinder.App();
			app.InitializeComponent();
			app.Run();
		}

		private static Assembly loadEmbeddedAssembly(string name)
		{

			if (name.EndsWith("Retargetable=Yes"))
			{
				return Assembly.Load(new AssemblyName(name));
			}

			var container = Assembly.GetExecutingAssembly();
			var path = new AssemblyName(name).Name + ".dll";



			using (var stream = container.GetManifestResourceStream(path))
			{
				if (stream == null)
				{
					return null;
				}

				var bytes = new byte[stream.Length];
				stream.Read(bytes, 0, bytes.Length);
				return Assembly.Load(bytes);
			}
		}
	}
}
