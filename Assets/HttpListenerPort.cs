using System;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class HttpListenerPort : MonoBehaviour {

	HttpListener listener;

	[SerializeField]
	Button StartButton;
	[SerializeField]
	Button StopButton;
	[SerializeField]
	Text URI;
	[SerializeField]
	 Text Status;
	
	void Start()
	{
		URI.text = "http://127.0.0.1:12345/";
	}

	public void DoStart () {
		listener = new HttpListener();
		string uriprefix = URI.text;
		/* if (uriprefix.Length == 0)
			uriprefix =  ;*/
		Debug.Log("Starting with prefix: " + uriprefix);
		listener.Prefixes.Add(uriprefix);
		listener.Start();
		Debug.Log("IsListening: " + listener.IsListening);
	}
	void Update() {
		if (listener != null && listener.IsListening) {
			Status.text = "Started";
		}
		else {
			Status.text = "Stopped";
		}
		StartButton.gameObject.SetActive(listener == null || !listener.IsListening);
		StopButton.gameObject.SetActive(listener != null && listener.IsListening);
	}
	
	public void DoStop() {
		if (listener != null) {
			Debug.Log("Stopping listener");
			listener.Stop();
		}
		listener = null;
	}
}
