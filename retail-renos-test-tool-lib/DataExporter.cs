using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using CsvHelper;

namespace retailrenostesttoollib
{
    public class DataExporter
    {
        string folderURI;

        public DataExporter ()
        {
            string path = Environment.GetFolderPath (Environment.SpecialFolder.Desktop);
            folderURI = path + Path.DirectorySeparatorChar + "results_" + DateTime.Now.ToString ("yyyy-MM-dd_HHmm") + Path.DirectorySeparatorChar;
            if (!Directory.Exists (folderURI))
                Directory.CreateDirectory (folderURI);
        }

        public void StoreTargetEPCs (string URI)
        {
            string target = folderURI + "target_epcs.csv";
            if (!File.Exists (target))
                File.Copy (URI, target);
        }

        public void StoreStrayEPCs (string URI)
        {
            string target = folderURI + "stray_epcs.csv";
            if (!File.Exists (target))
                File.Copy (URI, target);
        }

        public void WriteRenosInfoStatus (ref RenosConnectionManager rcm)
        {
            using (TextWriter writer = File.CreateText (folderURI + "renos_info_status.txt")) {
                writer.WriteLine ("# Renos info");
                writer.WriteLine ("Firmware version: " + rcm.systemVersion);
                writer.WriteLine ("System ID: " + rcm.systemID);
                writer.WriteLine ("Role: " + rcm.systemRole);
                writer.WriteLine ("RFID errors: " + rcm.RFIDErrors);
            }
        }

        public void WriteRunDetails (TestRun run, int runNumber)
        {
            using (TextWriter writer = File.CreateText (folderURI + "run_" + runNumber + "_observations.csv")) {
                var csv = new CsvWriter (writer);
                csv.Configuration.Delimiter = ";";
                csv.WriteField ("epc");
                csv.WriteField ("time");
                csv.NextRecord ();
                foreach (TestRunObservation o in run.epcObservations) {
                    csv.WriteField (o.epc);
                    csv.WriteField (o.time);
                    csv.NextRecord ();
                }
            }
            using (TextWriter writer = File.CreateText (folderURI + "run_" + runNumber + "_moves.csv")) {
                var csv = new CsvWriter (writer);
                csv.Configuration.Delimiter = ";";
                csv.WriteField ("epc");
                csv.WriteField ("time");
                csv.WriteField ("direction");
                csv.NextRecord ();
                foreach (TestRunMove m in run.epcMoves) {
                    csv.WriteField (m.epc);
                    csv.WriteField (m.time);
                    csv.WriteField (m.direction);
                    csv.NextRecord ();
                }
            }
        }

        public void WriteAllRuns (Dictionary<int, TestRun> testRuns, ReferenceEPCs references)
        {
            List<string> fields = new List<string> ();
            fields.Add ("run_number");
            fields.Add ("run_time");
            fields.Add ("run_description");
            fields.Add ("run_location");
            fields.Add ("run_aisle_width");
            fields.Add ("run_power");
            fields.Add ("#_target");
            fields.Add ("#_stray");
            fields.Add ("#_observed");
            fields.Add ("#_moved");
            fields.Add ("%_observed_target");
            fields.Add ("%_moved_target");
            fields.Add ("%_observed_stray");
            fields.Add ("%_moved_stray");
            fields.Add ("#_observed_target");
            fields.Add ("#_observed_stray");
            fields.Add ("#_observed_other");
            fields.Add ("#_moved_target");
            fields.Add ("#_moved_stray");
            fields.Add ("#_moved_other");

            // add the fields per category
            foreach (string c in references.categories) {
                fields.Add ("#_target_" + c);
                fields.Add ("#_stray_" + c);
                fields.Add ("%_observed_target_" + c);
                fields.Add ("%_moved_target_" + c);
                fields.Add ("%_observed_stray_" + c);
                fields.Add ("%_moved_stray_" + c);
                fields.Add ("#_observed_target_" + c);
                fields.Add ("#_observed_stray_" + c);
                fields.Add ("#_observed_other_" + c);
                fields.Add ("#_moved_target_" + c);
                fields.Add ("#_moved_stray_" + c);
                fields.Add ("#_moved_other_" + c);
            }

            List<Dictionary< string, string>> results = new List<Dictionary<string, string>> ();

            foreach (KeyValuePair<int, TestRun> kvp in testRuns) {
                Dictionary<string, string> result = new Dictionary<string, string> ();
                result ["run_number"] = kvp.Key.ToString ();
                result ["run_time"] = kvp.Value.dateTime.ToLongDateString () + " " + kvp.Value.dateTime.ToLongTimeString ();
                result ["run_description"] = kvp.Value.description;
                result ["run_location"] = kvp.Value.testLocation;
                result ["run_aisle_width"] = kvp.Value.aisleWidth;
                result ["run_power"] = kvp.Value.power;

                int target = references.GetNumberOfTargetEPCs ();
                int stray = references.GetNumberOfStrayEPCs ();

                result ["#_target"] = target.ToString ();
                result ["#_stray"] = stray.ToString ();
                result ["#_observed"] = kvp.Value.epcObservations.Count.ToString ();
                result ["#_moved"] = kvp.Value.epcMoves.Count.ToString ();

                Dictionary<string, int> calculationResult = references.CalculateProgress (kvp.Value);

                int observedTarget = calculationResult ["targetCurrentObservationQuantity"];
                int observedStray = calculationResult ["strayCurrentObservationQuantity"];
                int observedOther = calculationResult ["otherCurrentObservationQuantity"];
                int movedTarget = calculationResult ["targetCurrentMoveQuantity"];
                int movedStray = calculationResult ["strayCurrentMoveQuantity"];
                int movedOther = calculationResult ["otherCurrentMoveQuantity"];

                result ["#_observed_target"] = observedTarget.ToString ();
                result ["#_observed_stray"] = observedStray.ToString ();
                result ["#_observed_other"] = observedOther.ToString ();
                result ["#_moved_target"] = movedTarget.ToString ();
                result ["#_moved_stray"] = movedStray.ToString ();
                result ["#_moved_other"] = movedOther.ToString ();

                if (target > 0) {
                    decimal observedTargetPercentage = (decimal)observedTarget / (decimal)target;
                    decimal movedTargetPercentage = (decimal)movedTarget / (decimal)target;
                    result ["%_observed_target"] = String.Format ("{0:P1}", observedTargetPercentage);
                    result ["%_moved_target"] = String.Format ("{0:P1}", movedTargetPercentage);
                }
                if (stray > 0) {
                    decimal observedStrayPercentage = (decimal)observedStray / (decimal)stray;
                    decimal movedStrayPercentage = (decimal)movedStray / (decimal)stray;
                    result ["%_observed_stray"] = String.Format ("{0:P1}", observedStrayPercentage);
                    result ["%_moved_stray"] = String.Format ("{0:P1}", movedStrayPercentage);
                }

                // now calculate the same for each category
                foreach (string c in references.categories) {
                    int targetCategory = references.GetNumberOfTargetEPCs(c);
                    int strayCategory = references.GetNumberOfStrayEPCs(c);

                    result ["#_target_" + c] = targetCategory.ToString();
                    result ["#_stray_" + c] = strayCategory.ToString();

                    Dictionary<string, int> calculationResultCategory = references.CalculateProgress (kvp.Value, c);

                    int observedTargetCategory = calculationResultCategory ["targetCurrentObservationQuantity"];
                    int observedStrayCategory = calculationResultCategory ["strayCurrentObservationQuantity"];
                    int observedOtherCategory = calculationResultCategory ["otherCurrentObservationQuantity"];
                    int movedTargetCategory = calculationResultCategory ["targetCurrentMoveQuantity"];
                    int movedStrayCategory = calculationResultCategory ["strayCurrentMoveQuantity"];
                    int movedOtherCategory = calculationResultCategory ["otherCurrentMoveQuantity"];

                    result ["#_observed_target_" + c] = observedTargetCategory.ToString ();
                    result ["#_observed_stray_" + c] = observedStrayCategory.ToString ();
                    result ["#_observed_other_" + c] = observedOtherCategory.ToString ();
                    result ["#_moved_target_" + c] = movedTargetCategory.ToString ();
                    result ["#_moved_stray_" + c] = movedStrayCategory.ToString ();
                    result ["#_moved_other_" + c] = movedOtherCategory.ToString ();

                    if (targetCategory > 0) {
                        decimal observedTargetPercentageCategory = (decimal)observedTargetCategory / (decimal)targetCategory;
                        decimal movedTargetPercentageCategory = (decimal)movedTargetCategory / (decimal)targetCategory;
                        result ["%_observed_target_" + c] = String.Format ("{0:P1}", observedTargetPercentageCategory);
                        result ["%_moved_target_" + c] = String.Format ("{0:P1}", movedTargetPercentageCategory);
                    }
                    if (strayCategory > 0) {
                        decimal observedStrayPercentageCategory = (decimal)observedStrayCategory / (decimal)strayCategory;
                        decimal movedStrayPercentageCategory = (decimal)movedStrayCategory / (decimal)strayCategory;
                        result ["%_observed_stray_" + c] = String.Format ("{0:P1}", observedStrayPercentageCategory);
                        result ["%_moved_stray_" + c] = String.Format ("{0:P1}", movedStrayPercentageCategory);
                    }
                }

                results.Add (result);
            }

            using (TextWriter writer = File.CreateText (folderURI + "runs_summary.csv")) {
                var csv = new CsvWriter (writer);
                csv.Configuration.Delimiter = ";";

                // write headers
                foreach (string s in fields)
                    csv.WriteField (s);

                csv.NextRecord ();

                // write contents
                foreach (Dictionary<string, string> dic in results) {
                    foreach (string s in fields) {
                        if (dic.ContainsKey(s))
                            csv.WriteField(dic[s]);
                        else
                            csv.WriteField("");
                    }
                    csv.NextRecord();
                }
            }
        }
    }
}

