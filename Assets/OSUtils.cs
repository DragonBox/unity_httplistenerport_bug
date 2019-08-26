using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace WWTK.OSUtils
{
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
		public int port_number { get; set; }
		public string process_name { get; set; }
		public string protocol { get; set; }
		public int pid { get; set; }
	}
	class OSUtils
	{

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

					UnityEngine.Debug.Log(content);

					if (exitStatus != "0")
					{
						UnityEngine.Debug.LogError("Command failed " + exitStatus);
						UnityEngine.Debug.Log(content);
						throw new System.Exception("Failed running Netstats " + exitStatus);
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
								port_number = int.Parse(localAddress.Split(':')[1]),
								process_name = tokens[1] == "UDP" ? LookupProcess(Convert.ToInt32(tokens[4])) : LookupProcess(Convert.ToInt32(tokens[5])),
								pid = Convert.ToInt32(tokens[5])
							});
						}
					}
				}
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
			}
			return Ports;
		}

		public static string LookupProcess(int pid)
		{
			string procName;
			try { procName = System.Diagnostics.Process.GetProcessById(pid).ProcessName; } catch (Exception) { procName = "-"; }
			return procName;
		}
	}
}