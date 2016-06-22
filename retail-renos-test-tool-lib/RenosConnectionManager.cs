using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using WebSocketSharp;
using System.Net;
using RestSharp;
using Newtonsoft.Json;
using Eto.Forms;
using Eto.Drawing;

namespace retailrenostesttoollib
{
    public class RenosConnectionManager
    {
        RestClient client;

        string URI;

        WebSocket ws;

        public string systemID;
        public string systemVersion;
        public string systemRole;

        public string unreachableUnits;
        public string blockedIRBeamSensors;
        public string RFIDErrors;
        public string deviceManagementConnectionErrror;

        public Queue<RenosEvent> eventQueue;

        public RenosConnectionManager (string URI)
        {
            this.URI = URI;

            eventQueue = new Queue<RenosEvent>();
        }

        public bool Connect ()
        {
            // initiate client connection
            Console.WriteLine("Connecting to Renos at {0}", URI);
            client = new RestClient("http://" + URI);

            // get info
            var infoRequest = new RestRequest("/api/v2/info", Method.GET);
            var infoResult = client.Execute(infoRequest);
            if (infoResult.ResponseStatus == ResponseStatus.Error)
                return false;

            Dictionary<string, string> info = JsonConvert.DeserializeObject<Dictionary<string, string>>(infoResult.Content);

            systemID = info["system_id"];
            systemVersion = info["system_version"];
            systemRole = info["system_role"];

            // get status
            var statusRequest = new RestRequest("/api/v2/status", Method.GET);
            var statusResult = client.Execute(statusRequest);
            if (statusResult.ResponseStatus == ResponseStatus.Error)
                return false;
            Dictionary<string, string> status = JsonConvert.DeserializeObject<Dictionary<string, string>>(client.Execute(statusRequest).Content);

            unreachableUnits = status["unreachable_units"];
            blockedIRBeamSensors = status["blocked_ir_beam_sensors"];
            RFIDErrors = status["rfid_errors"];
            deviceManagementConnectionErrror = status["device_management_connection_error"];

            return true;
        }

        public bool StartGettingEvents ()
        {
            ws = new WebSocket ("ws://" + URI + "/api/v2/events");
            ws.OnMessage += (sender, e) => {
                if (e.Type == Opcode.Text) {
                    Console.WriteLine ("Renos says: " + e.Data);
                    RenosEvent item = JsonConvert.DeserializeObject<RenosEvent> (e.Data);
                    eventQueue.Enqueue(item);
                } 
            };

            ws.Connect ();
            if (ws.ReadyState == WebSocketState.Open) {
                ws.Send ("{\"request\": \"subscribe\",\"event_types\": [\"rfid_observation\", \"rfid_move\"]}");
                return true;
            }
            return false;
        }

        public void StopGettingEvents ()
        {
            eventQueue.Clear();

            ws.Close();
        }

    }

    public class EpcList
    {
        public string epc { get; set; }
        public DateTime time { get; set; }
    }

    public class RenosEvent : EventArgs
    {
        [JsonProperty("event")]
        public string eventType { get; set; }
        public string id { get; set; }
        public string direction { get; set; }
        public DateTime time { get; set; }
        [JsonProperty("epc_list")]
        public IList<EpcList> epcList { get; set; }
    }
}