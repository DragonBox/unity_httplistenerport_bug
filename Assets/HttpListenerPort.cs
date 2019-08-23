using System;
using System.Diagnostics;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

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



    void Start()
    {
        URI.text = "http://127.0.0.1:12345/";

        Pid.text = "Pid : " + Process.GetCurrentProcess().Id.ToString();
        UnityEngine.Debug.Log(Pid.text);
    }

    public void DoStart()
    {
        listener = new HttpListener();
        string uriprefix = URI.text;
        /* if (uriprefix.Length == 0)
			uriprefix =  ;*/
        UnityEngine.Debug.Log("Starting with prefix: " + uriprefix);
        listener.Prefixes.Add(uriprefix);
        listener.Start();
        UnityEngine.Debug.Log("IsListening: " + listener.IsListening);

        IPEndPoint endpoint = CreateListenerRequest(listener, uriprefix);
        UnityEngine.Debug.Log("Using port : " + endpoint.Port);
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
        request.GetRequestStream().Write(new byte[] { (byte)'a' }, 0, 1);
        request.GetRequestStream().Dispose();

        // Send request, socket is created or reused.
        var response = request.GetResponse();

        UnityEngine.Debug.Log("HI: " + response.ResponseUri);

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
            UnityEngine.Debug.Log("Stopping listener");
            listener.Stop();
        }
        listener = null;
    }
}
