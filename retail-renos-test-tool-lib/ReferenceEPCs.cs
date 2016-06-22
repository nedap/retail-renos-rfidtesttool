using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CsvHelper;

namespace retailrenostesttoollib
{
    public class ReferenceEPCs
    {
        public Dictionary<string, List<string>> targetEPCsByCategory = new Dictionary<string, List<string>> ();
        public Dictionary<string, string> targetEPCsByEPC = new Dictionary<string, string> ();
        public Dictionary<string, List<string>> strayEPCsByCategory = new Dictionary<string, List<string>> ();
        public Dictionary<string, string> strayEPCsByEPC = new Dictionary<string, string> ();

        public List<string> categories = new List<string>();

        public ReferenceEPCs ()
        {
        }

        public void LoadTargetEPCs (string filename)
        {
            LoadEPCs (filename, ref targetEPCsByCategory, ref targetEPCsByEPC);
        }

        public void LoadStrayEPCs (string filename)
        {
            LoadEPCs (filename, ref strayEPCsByCategory, ref strayEPCsByEPC);
        }

        void LoadEPCs (string filename, ref Dictionary<string, List<string>> byCategory, ref Dictionary<string, string> byEPC)
        {
            byCategory = new Dictionary<string, List<string>> ();
            byEPC = new Dictionary<string, string> ();

            StreamReader tr = new StreamReader (filename);
            var csv = new CsvReader (tr);
            csv.Configuration.TrimFields = true;
            csv.Configuration.TrimHeaders = true;

            while (csv.Read ()) {
                string epc = "", epcTemp;
                if (csv.TryGetField("epc", out epcTemp))
                    epc = epcTemp;
                if (csv.TryGetField("hex", out epcTemp))
                    epc = epcTemp;
                epc.ToUpper();

                if (epc == "")
                    continue;

                string category = "", categoryTemp;
                if (csv.TryGetField("category", out categoryTemp))
                    category = categoryTemp;

                if (!byEPC.ContainsKey (epc)) {
                    byEPC.Add (epc, category);
                    if (!byCategory.ContainsKey (category))
                        byCategory.Add (category, new List<string> ());
                    byCategory [category].Add (epc);

                    // registers category
                    if (category != "")
                        if (!categories.Contains(category))
                            categories.Add(category);
                }
            }
            tr.Close ();
        }

        public int GetNumberOfTargetEPCs ()
        {
            return targetEPCsByEPC.Count;
        }

        public int GetNumberOfTargetEPCs (string category)
        {
            if (targetEPCsByCategory.ContainsKey(category))
                return targetEPCsByCategory[category].Count;

            return 0;
        }

        public int GetNumberOfStrayEPCs ()
        {
            return strayEPCsByEPC.Count;
        }

        public int GetNumberOfStrayEPCs (string category)
        {
            if (strayEPCsByCategory.ContainsKey(category))
                return strayEPCsByCategory[category].Count;

            return 0;
        }

        public Dictionary<string, int> CalculateProgress (TestRun run)
        {
            return CalculateProgress (run, new List<string>(targetEPCsByEPC.Keys), new List<string>(strayEPCsByEPC.Keys));
        }

        public Dictionary<string, int> CalculateProgress (TestRun run, string category)
        {
            Dictionary<string, int> progress = new Dictionary<string, int> ();

            // filter out whether both target and stray file is present, or just one of both
            if (targetEPCsByCategory.ContainsKey (category) && strayEPCsByCategory.ContainsKey (category)) {
                return CalculateProgress (run, targetEPCsByCategory[category], strayEPCsByCategory[category]);
            } else if (!targetEPCsByCategory.ContainsKey (category) && strayEPCsByCategory.ContainsKey (category)) {
                return CalculateProgress (run, new List<string>(), strayEPCsByCategory[category]);
            } else if (targetEPCsByCategory.ContainsKey (category) && !strayEPCsByCategory.ContainsKey (category)) {
                return CalculateProgress (run, targetEPCsByCategory[category], new List<string>());
            } else {
                return progress;
            }
        }

        public Dictionary<string, int> CalculateProgress (TestRun run, List<string> targetEPCs, List<string> strayEPCs)
        {
            Dictionary<string, int> progress = new Dictionary<string, int> ();

            int targetCurrentObservationQuantity = 0;
            int strayCurrentObservationQuantity = 0;
            int otherCurrentObservationQuantity = 0;
            int targetCurrentMoveQuantity = 0;
            int strayCurrentMoveQuantity = 0;
            int otherCurrentMoveQuantity = 0;

            foreach (string epc in run.UniqueObservationEPCs()) {
                if (targetEPCs.Contains(epc))
                    targetCurrentObservationQuantity++;
                else if (strayEPCs.Contains(epc))
                    strayCurrentObservationQuantity++;
                else
                    otherCurrentObservationQuantity++;
            }

            foreach (string epc in run.UniqueMoveEPCs()) {
                if (targetEPCs.Contains (epc))
                    targetCurrentMoveQuantity++;
                else if (strayEPCs.Contains (epc))
                    strayCurrentMoveQuantity++;
                else
                    otherCurrentMoveQuantity++;
            }

            progress ["targetCurrentObservationQuantity"] = targetCurrentObservationQuantity;
            progress ["strayCurrentObservationQuantity"] = strayCurrentObservationQuantity;
            progress ["otherCurrentObservationQuantity"] = otherCurrentObservationQuantity;
            progress ["targetCurrentMoveQuantity"] = targetCurrentMoveQuantity;
            progress ["strayCurrentMoveQuantity"] = strayCurrentMoveQuantity;
            progress ["otherCurrentMoveQuantity"] = otherCurrentMoveQuantity;

            return progress;
        }
    }
}