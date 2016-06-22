using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Eto.Forms;
using Eto.Drawing;
using Eto.Threading;

namespace retailrenostesttoollib
{
    /// <summary>
    /// Your application's main form
    /// </summary>
    public class MainForm : Form
    {
        static ReferenceEPCs references;
        static RenosConnectionManager rcm;
        static DataExporter de;
        static Dictionary<int, TestRun> testRuns = new Dictionary<int, TestRun> ();

        bool renosConnected = false;
        bool runActive = false;

        int currentRunNumber = 0;

        string referenceTargetEPCFileName;
        string referenceStrayEPCFileName;

        Label targetEPCsLabel;
        Label strayEPCsLabel;

        Button targetEPCsButton;
        Button strayEPCsButton;

        TextBox renosAddressTextBox;
        Button renosConnectButton;
        Label renosConnectLabel;
        Label renosInfoLabel;

        Button startStopButton;
        Button deleteButton;

        TextBox runDescriptionTextBox;
        TextBox runLocationTextBox;
        TextBox runAisleWidthTextBox;
        TextBox runPowerTextBox;

        Label currentRunLabel;

        Label targetObserveEPCsLabel;
        Label targetMoveEPCsLabel;
        Label strayObserveEPCsLabel;
        Label strayMoveEPCsLabel;
        Label otherObserveEPCsLabel;
        Label otherMoveEPCsLabel;

        UITimer timer;

        public MainForm ()
        {
            Console.WriteLine("Starting Renos Test Toolie...");

            references = new ReferenceEPCs ();

            Title = "Renos Test Toolie";
            ClientSize = new Size (575, 675);

            var layout = new PixelLayout ();

            int verticalPixels = 0;

            this.KeyDown += MainForm_KeyDown;

            // #1 Reference files
            layout.Add (new Label { Text = "#1 Reference files", Font = new Font (FontFamilies.Sans, 24) }, 10, verticalPixels += 10);

            // add labels and button for target EPCs
            targetEPCsLabel = new Label { Text = "No target EPCs file loaded" };
            layout.Add (targetEPCsLabel, 10, verticalPixels += 40);

            targetEPCsButton = new Button { Text = "Load target EPCs file", Width = 250 };
            targetEPCsButton.Click += TargetEPCsButton_Click;
            layout.Add (targetEPCsButton, 300, verticalPixels);

            // add labels and button for stray EPCs
            strayEPCsLabel = new Label { Text = "No stray EPCs file loaded" };
            layout.Add (strayEPCsLabel, 10, verticalPixels += 30);

            strayEPCsButton = new Button { Text = "Load stray EPCs file", Width = 250 };
            strayEPCsButton.Click += StrayEPCsButton_Click;
            layout.Add (strayEPCsButton, 300, verticalPixels);

            // #2 Renos connection
            layout.Add (new Label { Text = "#2 Renos connection", Font = new Font (FontFamilies.Sans, 24) }, 10, verticalPixels += 40);

            // add labels to connect to the system
            renosAddressTextBox = new TextBox { Text = "192.168.133.1", Width = 250 };
            renosAddressTextBox.KeyDown += RenosAddressTextBox_KeyDown;
            layout.Add (renosAddressTextBox, 10, verticalPixels += 40);

            renosConnectButton = new Button { Text = "Connect to Renos", Width = 250 };
            renosConnectButton.Click += RenosConnectButton_Click;
            layout.Add (renosConnectButton, 300, verticalPixels);

            renosConnectLabel = new Label { Text = "Not connected", TextColor = Color.Parse ("red") };
            layout.Add (renosConnectLabel, 10, verticalPixels += 30);

            renosInfoLabel = new Label ();
            layout.Add (renosInfoLabel, 10, verticalPixels += 20);

            // #3 Run configuration
            layout.Add (new Label { Text = "#3 Run configuration", Font = new Font (FontFamilies.Sans, 24) }, 10, verticalPixels += 50);

            // show parameters that can be changed
            layout.Add (new Label { Text = "Description" }, 10, verticalPixels += 40);
            runDescriptionTextBox = new TextBox { PlaceholderText = "Dolley", Width = 250, Enabled = false };
            layout.Add (runDescriptionTextBox, 150, verticalPixels);

            layout.Add (new Label { Text = "Location" }, 10, verticalPixels += 30);
            runLocationTextBox = new TextBox { PlaceholderText = "Stock room", Width = 250, Enabled = false };
            layout.Add (runLocationTextBox, 150, verticalPixels);

            layout.Add (new Label { Text = "Aisle width/height" }, 10, verticalPixels += 30);
            runAisleWidthTextBox = new TextBox { PlaceholderText = "2.0m", Width = 250, Enabled = false };
            layout.Add (runAisleWidthTextBox, 150, verticalPixels);

            layout.Add (new Label { Text = "Power" }, 10, verticalPixels += 30);
            runPowerTextBox = new TextBox { PlaceholderText = "50%", Width = 250, Enabled = false };
            layout.Add (runPowerTextBox, 150, verticalPixels);

            startStopButton = new Button { Text = "Start", Width = 200, Height = 50 };
            startStopButton.Click += StartStopButton_Click;
            startStopButton.Enabled = false;
            layout.Add (startStopButton, 10, verticalPixels += 50);

            deleteButton = new Button {
                Text = "Stop and delete",
                Width = 150,
                Height = 50,
                TextColor = Color.Parse ("red")
            };
            deleteButton.Click += DeleteButton_Click;
            deleteButton.Enabled = false;
            layout.Add (deleteButton, 225, verticalPixels);

            currentRunLabel = new Label { Text = "Run -", Width = 150, Height = 100, Font = new Font (FontFamilies.Sans, 36) };
            layout.Add (currentRunLabel, 400, verticalPixels += 80);

            layout.Add (new Label { Text = "Observed", Font = new Font (FontFamilies.Sans, 12, FontStyle.Bold, 0) }, 150, verticalPixels);
            layout.Add (new Label { Text = "Moved", Font = new Font (FontFamilies.Sans, 12, FontStyle.Bold, 0) }, 300, verticalPixels);

            layout.Add (new Label { Text = "Target EPCs", Font = new Font (FontFamilies.Sans, 12, FontStyle.Bold, 0) }, 10, verticalPixels += 30);

            targetObserveEPCsLabel = new Label { Text = "-" };
            layout.Add (targetObserveEPCsLabel, 150, verticalPixels);
            targetMoveEPCsLabel = new Label { Text = "-" };
            layout.Add (targetMoveEPCsLabel, 300, verticalPixels);

            layout.Add (new Label { Text = "Stray EPCs", Font = new Font (FontFamilies.Sans, 12, FontStyle.Bold, 0) }, 10, verticalPixels += 30);

            strayObserveEPCsLabel = new Label { Text = "-" };
            layout.Add (strayObserveEPCsLabel, 150, verticalPixels);
            strayMoveEPCsLabel = new Label { Text = "-" };
            layout.Add (strayMoveEPCsLabel, 300, verticalPixels);

            layout.Add (new Label { Text = "Other EPCs", Font = new Font (FontFamilies.Sans, 12, FontStyle.Bold, 0) }, 10, verticalPixels += 30);

            otherObserveEPCsLabel = new Label { Text = "-" };
            layout.Add (otherObserveEPCsLabel, 150, verticalPixels);
            otherMoveEPCsLabel = new Label { Text = "-" };
            layout.Add (otherMoveEPCsLabel, 300, verticalPixels);

            layout.Add (new Label { Text = String.Format("Renos Test Toolie Version: {0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()), Font = new Font (FontFamilies.Sans, 8, FontStyle.None, 0) }, 10, verticalPixels+=50);

            timer = new UITimer();
            timer.Interval = 0.1;
            timer.Elapsed += Timer_Elapsed;

            Content = layout;
        }

        void RenosAddressTextBox_KeyDown (object sender, KeyEventArgs e)
        {
            string keyPressed = e.Key.ToString ();

            if (keyPressed.Equals ("Enter")) {
                renosConnectButton.PerformClick ();
            }
        }

        void MainForm_KeyDown (object sender, KeyEventArgs e)
        {
            string keyPressed = e.Key.ToString();

            if (keyPressed.Equals("PageUp")) {
                deleteButton.PerformClick();
            } else if (keyPressed.Equals("PageDown")) {
                startStopButton.PerformClick();
            }
        }

        void Timer_Elapsed (object sender, EventArgs e)
        {
            while (rcm.eventQueue.Count > 0) {
                RenosEvent renosEvent = rcm.eventQueue.Dequeue ();

                if (renosEvent.epcList == null)
                    continue;

                foreach (EpcList epcList in renosEvent.epcList) {
                    if (renosEvent.eventType == "rfid_observation") {
                        testRuns [currentRunNumber].ProcessObservation(epcList.epc, epcList.time);
                    } else if (renosEvent.eventType == "rfid_move") {
                        testRuns [currentRunNumber].ProcessMove(epcList.epc, epcList.time, renosEvent.direction);
                    }
                }
            }

            UpdateProgressBars ();           
        }

        void SetParametersEnabled (bool enabled)
        {
            runDescriptionTextBox.Enabled = enabled;
            runLocationTextBox.Enabled = enabled;
            runAisleWidthTextBox.Enabled = enabled;
            runPowerTextBox.Enabled = enabled;
        }

        void StartStopButton_Click (object sender, EventArgs e)
        {
            if (!runActive) {
                if (!rcm.StartGettingEvents ()) {
                    MessageBox.Show ("Cannot open socket connection to Renos", "Connection error");
                    return;
                }

                testRuns [currentRunNumber] = new TestRun ();
                testRuns [currentRunNumber].dateTime = DateTime.Now;
                testRuns [currentRunNumber].description = runDescriptionTextBox.Text;
                testRuns [currentRunNumber].testLocation = runLocationTextBox.Text;
                testRuns [currentRunNumber].aisleWidth = runAisleWidthTextBox.Text;
                testRuns [currentRunNumber].power = runPowerTextBox.Text;

                renosConnectButton.Enabled = false;
                deleteButton.Enabled = true;

                SetParametersEnabled (false);

                timer.Start();

                currentRunLabel.BackgroundColor = Color.Parse("Green");

                runActive = true;
                startStopButton.Text = "Stop and save";

            } else {
                rcm.StopGettingEvents ();

                timer.Stop();

                currentRunLabel.BackgroundColor = this.BackgroundColor;

                renosConnectButton.Enabled = true;
                deleteButton.Enabled = false;

                SetParametersEnabled (true);

                // write current run to file
                de.WriteRunDetails (testRuns [currentRunNumber], currentRunNumber);

                // write summary of all runs to file
                de.WriteAllRuns (testRuns, references);

                currentRunNumber++;

                runActive = false;
                startStopButton.Text = "Start";
            }
        }

        void DeleteButton_Click (object sender, EventArgs e)
        {
            if (runActive) {
                rcm.StopGettingEvents ();

                timer.Stop();

                renosConnectButton.Enabled = true;
                deleteButton.Enabled = false;

                SetParametersEnabled (true);

                currentRunLabel.BackgroundColor = this.BackgroundColor;

                // restore previous data
                currentRunNumber--;
                UpdateProgressBars ();
                currentRunNumber++;

                runActive = false;
                startStopButton.Text = "Start";
            }
        }

        void RenosConnectButton_Click (object sender, EventArgs e)
        {
            string IPHostname = renosAddressTextBox.Text;

            if (!renosConnected) {
                rcm = new RenosConnectionManager (IPHostname);
                if (!rcm.Connect ()) {
                    MessageBox.Show ("Cannot connect to Renos at hostname or IP " + IPHostname, "Connection error");
                    return;
                }

                renosConnectLabel.Text = "Connected";
                renosConnectLabel.TextColor = Color.Parse ("green");
                renosConnectButton.Text = "Disconnect";
                renosAddressTextBox.Enabled = false;
                renosInfoLabel.Text = String.Format ("Firmware: {0}, Role: {1}", rcm.systemVersion, rcm.systemRole);

                targetEPCsButton.Enabled = false;
                strayEPCsButton.Enabled = false;

                startStopButton.Enabled = true;

                SetParametersEnabled (true);

                // reset the run number
                currentRunNumber = 0;
                UpdateProgressBars();
                currentRunNumber++;

                // reset test run data
                testRuns = new Dictionary<int, TestRun> ();

                // set up new exporter and copy reference files to this folder
                de = new DataExporter ();
                if (references.GetNumberOfTargetEPCs () > 0)
                    de.StoreTargetEPCs (referenceTargetEPCFileName);
                if (references.GetNumberOfStrayEPCs () > 0)
                    de.StoreStrayEPCs (referenceStrayEPCFileName);
                de.WriteRenosInfoStatus (ref rcm);

                renosConnected = true;
            } else {
                renosConnectLabel.Text = "Not connected";
                renosConnectLabel.TextColor = Color.Parse ("red");
                renosConnectButton.Text = "Connect";
                renosAddressTextBox.Enabled = true;
                renosInfoLabel.Text = "";

                targetEPCsButton.Enabled = true;
                strayEPCsButton.Enabled = true;

                startStopButton.Enabled = false;

                SetParametersEnabled (false);

                renosConnected = false;

                return;
            }
        }

        void TargetEPCsButton_Click (object sender, EventArgs e)
        {
            var file = new OpenFileDialog ();
            file.MultiSelect = false;
            if (file.ShowDialog (this) == DialogResult.Ok) {
                referenceTargetEPCFileName = file.FileName;
                references.LoadTargetEPCs (referenceTargetEPCFileName);
                targetEPCsLabel.Text = String.Format ("{0} EPCs in target list", references.GetNumberOfTargetEPCs ());
            } else {
                targetEPCsLabel.Text = "File could not be loaded.";
            }
        }

        void StrayEPCsButton_Click (object sender, EventArgs e)
        {
            var file = new OpenFileDialog ();
            file.MultiSelect = false;
            if (file.ShowDialog (this) == DialogResult.Ok) {
                referenceStrayEPCFileName = file.FileName;
                references.LoadStrayEPCs (referenceStrayEPCFileName);
                strayEPCsLabel.Text = String.Format ("{0} EPCs in stray list", references.GetNumberOfStrayEPCs ());
            } else {
                strayEPCsLabel.Text = "File could not be loaded.";
            }
        }

        void UpdateProgressBars ()
        {
            if (currentRunNumber == 0)
                currentRunLabel.Text = "Run -";
            else
                currentRunLabel.Text = "Run " + currentRunNumber;

            if (!testRuns.ContainsKey (currentRunNumber))
                return;

            Dictionary<string, int> progress = references.CalculateProgress (testRuns [currentRunNumber]);

            int maxTargetEPCs = references.GetNumberOfTargetEPCs ();
            int maxStrayEPCs = references.GetNumberOfStrayEPCs ();

            decimal percentageTargetObserve = 0;
            decimal percentageTargetMove = 0;
            if (maxTargetEPCs > 0) {
                percentageTargetObserve = progress ["targetCurrentObservationQuantity"] / (decimal)maxTargetEPCs;
                percentageTargetMove = progress ["targetCurrentMoveQuantity"] / (decimal)maxTargetEPCs;
            }

            decimal percentageStrayObserve = 0;
            decimal percentageStrayMove = 0;
            if (maxStrayEPCs > 0) {
                percentageStrayObserve = progress ["strayCurrentObservationQuantity"] / (decimal)maxStrayEPCs;
                percentageStrayMove = progress ["strayCurrentMoveQuantity"] / (decimal)maxStrayEPCs;
            }

            targetObserveEPCsLabel.Text = String.Format ("{0} ({1:P1})", progress ["targetCurrentObservationQuantity"], percentageTargetObserve);
            targetMoveEPCsLabel.Text = String.Format ("{0} ({1:P1})", progress ["targetCurrentMoveQuantity"], percentageTargetMove);
            strayObserveEPCsLabel.Text = String.Format ("{0} ({1:P1})", progress ["strayCurrentObservationQuantity"], percentageStrayObserve);
            strayMoveEPCsLabel.Text = String.Format ("{0} ({1:P1})", progress ["strayCurrentMoveQuantity"], percentageStrayMove);
            otherObserveEPCsLabel.Text = progress ["otherCurrentObservationQuantity"].ToString ();
            otherMoveEPCsLabel.Text = progress ["otherCurrentMoveQuantity"].ToString ();
        }
    }
}
