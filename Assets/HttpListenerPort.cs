using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using WWTK.OSUtils;

public class HttpListenerPort : MonoBehaviour
{

	HttpListener listener;

	[SerializeField]
	Button StartButton;
	[SerializeField]
	Button StopButton;
	[SerializeField]
	Text URI;
	[SerializeField]
	Text Status;
	[SerializeField]
	Text Pid;

	[SerializeField]
	Text OpenedPort;

	void Start()
	{
		int pid = System.Diagnostics.Process.GetCurrentProcess().Id;
		Pid.text = "Pid : " + pid.ToString();
		Debug.Log(Pid.text);
	}

	public void DoStart()
	{
		listener = new HttpListener();
		string uriprefix = URI.text;
		/* if (uriprefix.Length == 0)
			uriprefix =  ;*/
		Debug.Log("Starting with prefix: " + uriprefix);
		listener.Prefixes.Add(uriprefix);
		listener.Start();
		Debug.Log("IsListening: " + listener.IsListening);

		if (listener.IsListening)
		{
			try
			{
				IPEndPoint endpoint = CreateListenerRequest(listener, uriprefix);
				Debug.Log("Using port : " + endpoint.Port);
			}
			catch (WebException e)
			{
				Debug.Log("Couldn't connect to port: " + e);
				Debug.Log("HttpListener port not found. Trying to detect it...");
				int port = FindOneOpenPort();
				if (port != -1)
				{
					uriprefix = System.Text.RegularExpressions.Regex.Replace(uriprefix, @"(.*//.*:)([0-9]+)(/.*)", m => m.Groups[1].Value + port + m.Groups[3].Value);
					Debug.Log("uri is " + uriprefix);
				}
			}
		}
	}
	public IPEndPoint CreateListenerRequest(HttpListener listener, string uri)
	{
		IPEndPoint ipEndPoint = null;
		var mre = new System.Threading.ManualResetEvent(false);
		listener.BeginGetContext(result =>
		{
			ipEndPoint = ListenerCallback(result);
			mre.Set();
		}, listener);

		var request = (HttpWebRequest)WebRequest.Create(uri);
		request.Method = "POST";

		// We need to write something
		request.GetRequestStream().Write(new byte[] {
			(byte) 'a' }, 0, 1);
		request.GetRequestStream().Dispose();

		// Send request, socket is created or reused.
		var response = request.GetResponse();

		Debug.Log("HI: " + response.ResponseUri);

		// Close response so socket can be reused.
		response.Close();

		mre.WaitOne();

		return ipEndPoint;
	}

	public static IPEndPoint ListenerCallback(IAsyncResult result)
	{
		var listener = (HttpListener)result.AsyncState;
		var context = listener.EndGetContext(result);
		var clientEndPoint = context.Request.RemoteEndPoint;

		// Disposing InputStream should not avoid socket reuse
		context.Request.InputStream.Dispose();

		// Close OutputStream to send response
		context.Response.OutputStream.Close();

		return clientEndPoint;
	}

	void Update()
	{
		if (listener != null && listener.IsListening)
		{
			Status.text = "Started";
		}
		else
		{
			Status.text = "Stopped";
		}
		StartButton.gameObject.SetActive(listener == null || !listener.IsListening);
		StopButton.gameObject.SetActive(listener != null && listener.IsListening);
	}

	public void DoStop()
	{
		if (listener != null)
		{
			Debug.Log("Stopping listener");
			listener.Stop();
		}
		listener = null;
	}

	public void DoGetPorts()
	{
		OpenedPort.text = "";
		int ProcessID = System.Diagnostics.Process.GetCurrentProcess().Id;
#if UNITY_EDITOR
		ProcessID = 47056;
		List<Port> ports = new List<Port>();
		string output = Resources.Load<TextAsset>("NetCatOutput").text;
		OSUtils.ParseNetcatOutput(output, ports);
#else
		List<Port> ports = OSUtils.GetNetStatPorts();
#endif
		foreach (Port port in ports)
		{
			if (port.pid == ProcessID && port.state == "LISTENING")
			{
				OpenedPort.text = OpenedPort.text + port.port_number + " ";
			}
			Debug.Log("Ports : " + port.name);
		}
	}


	int FindOneOpenPort()
	{
		if (Application.platform == RuntimePlatform.WindowsPlayer)
		{
			int ProcessID = System.Diagnostics.Process.GetCurrentProcess().Id;
			List<Port> ports = OSUtils.GetNetStatPorts();
			List<int> portids = new List<int>();
			foreach (Port port in ports)
			{
				if (port.pid == ProcessID && port.state == "LISTENING")
				{
					portids.Add(port.port_number);
				}
			}
			if (portids.Count == 1)
			{
				return portids[0];
			}
		}
		return -1;
	}
}