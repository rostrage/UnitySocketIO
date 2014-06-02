using UnityEngine;
using System.Net;
using System;
using System.Collections;
using System.IO;
using WebSocketSharp;
/// <summary>
/// This class will serve as a wrapper around the websocket interface so that it works in both standalone and webplayer versions.
/// </summary>
public class WebSocketWrapper : MonoBehaviour {
	WebSocket ws;
	string sid;
	bool isAlive = false; //built-in websocket isAlive breaks socket.io
	void Start () {
		SocketIOHandShake ();
		//ws.Send ("BALUS");
	}

	void Update() {
	}


	//TODO: Make this async
	void SocketIOHandShake() {
		WebRequest wr = WebRequest.Create("http://worldmanager.rutgers.edu:3003/socket.io/1");
		wr.Method = "POST";
		WebResponse res = wr.GetResponse ();
		StreamReader sr = new StreamReader (res.GetResponseStream());
		string responseString = sr.ReadToEnd ();
		sid = responseString.Substring (0, responseString.IndexOf (':'));
		Debug.Log(responseString);
		int heartbeat = Convert.ToInt32(responseString.Substring (responseString.IndexOf (':')+1, 2));
		ws = new WebSocket ("ws://174.129.18.69:3003/socket.io/1/websocket/"+sid);
		ws.OnMessage += (sender, e) => {
			isAlive = true;
			Debug.Log ("Laputa says: " + e.Data);
		};
		ws.OnError += (object sender, WebSocketSharp.ErrorEventArgs e) =>  
			Debug.LogError(e.Message);
		ws.Connect ();
		StartCoroutine ("SendHeartbeat", heartbeat);
	}

	IEnumerator SendHeartbeat(int timeout) {
		for (;;) {
			yield return new WaitForSeconds (timeout/2);
			SendSocketIOMessage ("2", "", "", "");

		}
	}

	void SendSocketIOMessage(string messageType, string messageId, string messageEndpoint, string messageData) {
		if (ws != null) {
			string message;
			if(messageEndpoint!="") {
				message = messageType + ":" + messageId + ":" + messageEndpoint + ":" + messageData;
			} else {
				message = messageType + ":" + messageId + ":";
			}
			if(isAlive) {
				ws.Send (message);
			}
			else {
				Debug.Log("Conneciton lost");
			}
		}
	}
}
