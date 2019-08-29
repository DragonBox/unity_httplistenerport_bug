using System;
using System.Net;

namespace HttpListenerBugConsole
{
	class MainClass
	{
		public static void Main (string[] args)
		{
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
	}
}
