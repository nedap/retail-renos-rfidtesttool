using System;
using System.Collections;
using System.Collections.Generic;

namespace retailrenostesttoollib
{
    public class TestRun
    {
        public string description;
        public string testLocation;
        public string aisleWidth;
        public string power;
        public DateTime dateTime;
        public List<TestRunObservation> epcObservations;
        public List<TestRunMove> epcMoves;

        public TestRun ()
        {
            epcObservations = new List<TestRunObservation> ();
            epcMoves = new List<TestRunMove> ();
        }

        public void ProcessObservation (string epc, DateTime time)
        {
            if (!ContainsObservation (epc)) {
                TestRunObservation o = new TestRunObservation();
                o.epc = epc;
                o.time = time;
                epcObservations.Add(o);
            }
        }

        public void ProcessMove (string epc, DateTime time, string direction)
        {
            if (!ContainsMove (epc, direction)) {
                TestRunMove o = new TestRunMove();
                o.epc = epc;
                o.time = time;
                o.direction = direction;
                epcMoves.Add(o);
            }
        }

        public bool ContainsObservation (string epcToCompare)
        {
            foreach (TestRunObservation o in epcObservations) {
                if (o.epc.Equals(epcToCompare))
                    return true;
            }

            return false;
        }

        public bool ContainsMove (string epcToCompare)
        {
            foreach (TestRunMove m in epcMoves) {
                if (m.epc.Equals (epcToCompare))
                    return true;
            }
            return false;
        }

        public bool ContainsMove (string epcToCompare, string directionToCompare)
        {
            foreach (TestRunMove m in epcMoves) {
                if ((m.epc.Equals (epcToCompare)) && (m.direction.Equals (directionToCompare)))
                    return true;
            }
            return false;
        }

        public List<string> UniqueObservationEPCs ()
        {
            List<string> epcs = new List<string> ();

            foreach (TestRunObservation o in epcObservations) {
                if (!epcs.Contains(o.epc))
                    epcs.Add(o.epc);
            }

            return epcs;
        }

        public List<string> UniqueMoveEPCs ()
        {
            List<string> epcs = new List<string> ();

            foreach (TestRunMove m in epcMoves) {
                if (!epcs.Contains(m.epc))
                    epcs.Add(m.epc);
            }

            return epcs;
        }
    }

    public class TestRunObservation
    {
        public string epc;
        public DateTime time;
    }

    public class TestRunMove
    {
        public string epc;
        public DateTime time;
        public string direction;
    }
}

