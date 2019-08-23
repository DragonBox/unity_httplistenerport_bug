using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
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

    [SerializeField]
    Text OpenedPort;


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

    public void DoGetPorts()
    {
        OpenedPort.text = "";
        List<Port> ports = GetNetStatPorts();
        foreach (Port port in ports)
        {
            if (port.pid == Process.GetCurrentProcess().Id)
            {
                OpenedPort.text = OpenedPort.text + port.port_number + " ";
            }
            //UnityEngine.Debug.Log("Ports : " + port.pid.ToString());
        }
    }

    // ===============================================
    // The Method That Parses The NetStat Output
    // And Returns A List Of Port Objects
    // ===============================================
    public static List<Port> GetNetStatPorts()
    {
        var Ports = new List<Port>();
        try
        {
            using (Process p = new Process())
            {

                ProcessStartInfo ps = new ProcessStartInfo();
                ps.Arguments = "-a -n -o";
                ps.FileName = "netstat.exe";
                ps.UseShellExecute = false;
                ps.WindowStyle = ProcessWindowStyle.Hidden;
                ps.RedirectStandardInput = true;
                ps.RedirectStandardOutput = true;
                ps.RedirectStandardError = true;

                p.StartInfo = ps;
                p.Start();

                StreamReader stdOutput = p.StandardOutput;
                StreamReader stdError = p.StandardError;

                string content = stdOutput.ReadToEnd() + stdError.ReadToEnd();
                string exitStatus = p.ExitCode.ToString();

                if (exitStatus != "0")
                {
                    // Command Errored. Handle Here If Need Be
                }

                //Get The Rows
                string[] rows = Regex.Split(content, "\r\n");
                foreach (string row in rows)
                {
                    //Split it baby
                    string[] tokens = Regex.Split(row, "\\s+");
                    if (tokens.Length > 4 && (tokens[1].Equals("UDP") || tokens[1].Equals("TCP")))
                    {
                        string localAddress = Regex.Replace(tokens[2], @"\[(.*?)\]", "1.1.1.1");
                        Ports.Add(new Port
                        {
                            protocol = localAddress.Contains("1.1.1.1") ? String.Format("{0}v6", tokens[1]) : String.Format("{0}v4", tokens[1]),
                            port_number = localAddress.Split(':')[1],
                            process_name = tokens[1] == "UDP" ? LookupProcess(Convert.ToInt16(tokens[4])) : LookupProcess(Convert.ToInt16(tokens[5])),
                            pid = Convert.ToInt16(tokens[5])
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return Ports;
    }

    public static string LookupProcess(int pid)
    {
        string procName;
        try { procName = Process.GetProcessById(pid).ProcessName; }
        catch (Exception) { procName = "-"; }
        return procName;
    }

    // ===============================================
    // The Port Class We're Going To Create A List Of
    // ===============================================
    public class Port
    {
        public string name
        {
            get
            {
                return string.Format("{0} ({1} port {2})", this.process_name, this.protocol, this.port_number);
            }
            set { }
        }
        public string port_number { get; set; }
        public string process_name { get; set; }
        public string protocol { get; set; }
        public int pid { get; set; }
    }
}
