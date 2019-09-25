using System;
using System.Net;
using Microsoft.Win32;

namespace HttpListenerBugConsole
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Get45PlusFromRegistry(); //https://docs.microsoft.com/en-us/dotnet/framework/migration-guide/how-to-determine-which-versions-are-installed

			//code from example: https://docs.microsoft.com/en-us/dotnet/api/system.net.httplistener?view=netframework-4.8
			if (!HttpListener.IsSupported)
			{
				Console.WriteLine ("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
				return;
			}

			HttpListener listener = new HttpListener();
			string uriprefix = "http://127.0.0.1:12345/";
			Console.WriteLine("Starting with prefix: " + uriprefix);
			listener.Prefixes.Add(uriprefix);
			listener.Start();

			Console.WriteLine("Listening...");
			// Note: The GetContext method blocks while waiting for a request. 
			HttpListenerContext context = listener.GetContext();
			HttpListenerRequest request = context.Request;
			// Obtain a response object.
			HttpListenerResponse response = context.Response;
			// Construct a response.
			string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
			// Get a response stream and write the response to it.
			response.ContentLength64 = buffer.Length;
			System.IO.Stream output = response.OutputStream;
			output.Write(buffer,0,buffer.Length);
			// You must close the output stream.
			output.Close();
			listener.Stop();
		}

		private static void Get45PlusFromRegistry()
		{
			const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";

			using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
			{
				if (ndpKey != null && ndpKey.GetValue("Release") != null) {
					Console.WriteLine(".NET Framework Version:"+ CheckFor45PlusVersion((int) ndpKey.GetValue("Release")));
				}
				else 
				{
					Console.WriteLine(".NET Framework Version 4.5 or later is not detected.");
				} 
			}
		}

		// Checking the version using >= enables forward compatibility.
		private static string CheckFor45PlusVersion(int releaseKey)
		{
			if (releaseKey >= 528040)
				return "4.8 or later";
			if (releaseKey >= 461808)
				return "4.7.2";
			if (releaseKey >= 461308)
				return "4.7.1";
			if (releaseKey >= 460798)
				return "4.7";
			if (releaseKey >= 394802)
				return "4.6.2";
			if (releaseKey >= 394254)
				return "4.6.1";      
			if (releaseKey >= 393295)
				return "4.6";      
			if (releaseKey >= 379893)
				return "4.5.2";      
			if (releaseKey >= 378675)
				return "4.5.1";      
			if (releaseKey >= 378389)
				return "4.5";      
			// This code should never execute. A non-null release key should mean
			// that 4.5 or later is installed.
			return "No 4.5 or later version detected";
		}


	}
}
