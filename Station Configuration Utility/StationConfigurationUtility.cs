/*
 * This class handles events and calls the methods needed to build and maintian winform functionality
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using System.Drawing;

namespace Station_Configuration_Utility
{
    public partial class StationConfigUtil : Form
    {
        //Instance variables
        
        /*
        These 4 data structures are what control and store data for the configurations
        */
        //Main List for address numbers
        private List<int> addressList = new List<int>();
        //Main List for model numbers
        private List<string> modelList = new List<string>();       
        //Main List for aliases   
        private List<string[]> aliasList = new List<string[]>();
        //Main List for asset numbers
        private List<string> assetList = new List<string>();

        private List<string> comboBox9500List = new List<string>();
        private string[] stationPaths;
        private string[] stationList;
        private string[] stationBoxResults;
        private string[] configResults;
        private string[] addressResults;
        private string[] modelResults;
        private string[] asset9500B = new string[4];        
        private string stationPath;
        private string lastChecked;
        private string lastConfig;
        private string path;
        private string lastCongfigNameSelected;
        private string model;
        private string alias;        
        private string directoryPath;
        private string manualAsset;
        private string port;
        private string section;
        private string currentLocationPath = "C:/Station Configs/System/last_checked.txt";
        private string lastConfigDir = "C:/Station Configs/System/last_config_dir.txt";
        private string lastConfigNamePath = "C:/Station Configs/System/last_config_name.txt";
        private string metcalConfigPath = "C:/ProgramData/Fluke/METCAL/config.dat";
        private string tempMetcalConfigPath = "M:/Station Configs/temp_config/temp.txt";
        private string warnPathStation = "C:/Station Configs/System/warning_standard_add.txt";
        private string warnPathAsset = "C:/Station Configs/System/warning_asset_add.txt";
        private string warnPathHead = "C:/Station Configs/System/warning_head_delete.txt";
        private string path9500B = "C:/Station Configs/System/9500B_config.txt";
        private string last9500BHeadConfigPath = "C:/Station Configs/System/last_9500B.txt";
        private string metcalLaunchPath = "C:/Program Files (x86)/Fluke/METCAL/mcrt.exe";
        private string editorLaunchPath = "C:/Program Files (x86)/Fluke/METCAL Editor/mce32.exe";
        private string lastDC = "C:/Station Configs/System/last_DC.txt";
        private string DCpath = "C:/Station Configs/System/flex_DC.txt";
        private string lastPM = "C:/Station Configs/System/last_PWRMTR.txt";
        private string PMpath = "C:/Station Configs/System/flex_PWRMTR.txt";
        private string lastLO = "C:/Station Configs/System/last_LO.txt";
        private string LOpath = "C:/Station Configs/System/flex_LO.txt";
        private string scrPath = "https://sharepoint.qualcomm.com/qces/Home/ESS/TestEquip/Lists/Calibration%20Software%20Change%20Request/active.aspx";
        private string myAppsPath = "https://myapps.qualcomm.com/Citrix/MYAPPSWeb/";
        private string breakScriptPath = "M:/Station Configs/System/AdvancedBreak.exe";
        private string scriptInstallPath = "M:/Station Configs/System/AutoHotkey112209_Install.exe";
        private string niMaxPath = "C:/Program Files (x86)/National Instruments/MAX/NIMax.exe";
        private string metteamPath = "http://metteam:35853/Authentication/LogOn?ReturnUrl=%2f";
        private string metcalIni = "C:/ProgramData/Fluke/metcal.ini";       
        private int index9500BSelect;
        private int address;        
        private int avalibleAddress;
        private int standardSelect;
        private int rowIndex;
        private ComboBox stationComboBox;
        private DirectoryHandler dh = new DirectoryHandler();
        private FileHandler fh = new FileHandler();
        private LoginBox login = new LoginBox();
        private WarningBox warn = new WarningBox();
        private StringReader sr;
        private bool addValidated;
        private bool oscopeCal;
        private bool DC;
        private bool PWRMTR;
        private bool LO;    
        private bool reset = false;
        private bool startMethods = false;
        private bool exit = false;

        public StationConfigUtil()
        {
            
            //Intialize Windows nondyanmic compenents
            InitializeComponent();

            //Set last state data and intialize Windows dynamic compenents
            try
            {
                setLastState();
                //Start splash screen with new thread
                Thread t = new Thread(startSplashScreen);
                t.Start();
                Thread.Sleep(5000);
                t.Abort();
                Application.Exit();
            }
            catch (Exception e)
            {
                MessageBox.Show("A problem occured lanching application, exiting app.\n\n Exception: " + e.GetBaseException(), "Error");
                Environment.Exit(1);
            }
            

           
        }
        /*
         * Event listener methods start here
         */

        //Open login window for directory changes with folder browser dialog
        private void defaultDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool loginAttempt;

            loginAttempt = login.loginInputBox("Login", "Enter Login and Password.", "login");

            if (loginAttempt)
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.SelectedPath = stationPath;
                // Set the text description for the FolderBrowserDialog.
                fbd.Description = "Select the directory that you want to use as the default.";
                DialogResult result = fbd.ShowDialog();

                string newStationPath = fbd.SelectedPath;

                fh.textWriter(directoryPath, newStationPath);
            }
        }

        //Write to metcal config file when station config file is chosen
        private void stationComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (startMethods)
            {
                //clear metcal config
                fh.clearText(metcalConfigPath);
                //get combo box selection index and set path
                int selectedIndex = stationComboBox.SelectedIndex;
                path = stationPaths[selectedIndex];
                //get selected value from selection and set last selection variable
                string selectedValue = stationComboBox.SelectedItem.ToString();
                lastCongfigNameSelected = selectedValue;
                //results from stationbox selection for config file       
                stationBoxResults = fh.textReaderAll(path);
                //write all lines to new config selection
                fh.textWriterAll(metcalConfigPath, stationBoxResults);
                //Update current config view
                currentConfigView.ForeColor = System.Drawing.SystemColors.HotTrack;
                currentConfigView.Text = stationComboBox.SelectedItem.ToString();
                //Clear last config path and name then update file with current selection
                fh.clearText(lastConfigDir);
                fh.textWriter(lastConfigDir, path);
                fh.clearText(lastConfigNamePath);
                fh.textWriter(lastConfigNamePath, lastCongfigNameSelected);
                lastConfig = fh.textReader(lastConfigDir);
                //Update avalible addresses to configure
                avalibleAddress = parseAddress(path);
                avalAddressView.Text = "" + avalibleAddress;
                //Asset number list capacity of address list
                assetList.Clear();
                assetList.TrimExcess();
                assetList.Capacity = addressList.Count;
                for (int i = 0; i < assetList.Capacity; i++)
                {
                    assetList.Add("");
                }
                //Parse model numbers
                parseRemoteModelNumbers(path);
                //Parse alias names
                parseAliasNames(path);
                //Set address available in text box
                addressTextBox.Text = addressSelectionExample();
                //Reset tabs
                tabControl.TabPages.Clear();
                tabControl.TabPages.Add(stationsTab);
                tabControl.TabPages.Add(addStandardTab);
                //Build tab for 9500B Configuration if needed
                tab9500B();
                //Build tab for Flexible Standard
                flexStdTabControl();
                //Set last FLexible standard if used
                string dc = fh.textReader(lastDC);
                string pm = fh.textReader(lastPM);
                string lo = fh.textReader(lastLO);
                setCurrentFlex(dc, pm, lo);
                //Add address, alias, and models to data grid
                addDataGridData("stationsTab");
                addDataGridData("addStandardTab");
            }
        }

        //Choose location qualcomm not being able to add asset numbers
        private void qualcommLocationRB_CheckedChanged(object sender, EventArgs e)
        {

            if (qualcommLocationRB.Checked == true)
            {
                if (reset)
                {
                    fh.clearText(currentLocationPath);
                    fh.textWriter(currentLocationPath, "qualcomm");
                    setLastState();
                    reset = false;
                    qualcommLocationRB.Checked = false;                    
                }
                else
                {
                    //Set directory path
                    directoryPath = "C:/Station Configs/System/default_directory.txt";
                    stationPath = dh.setDirectory(directoryPath);
                    stationPaths = dh.getDirectoryFilePaths(stationPath);
                    stationList = dh.getDirectoryFilesNames(stationPath);
                    if (stationComboBox != null)
                    {
                        //Destroy current comobo box
                        stationComboBox.Dispose();
                        //Unsubscribe event handler
                        stationComboBox.SelectedIndexChanged -= (stationComboBox_SelectedIndexChanged);
                        setStationComboBox();
                    }
                    //update status label
                    currentNameConvView.Text = "Qualcomm";
                    fh.clearText(currentLocationPath);
                    fh.textWriter(currentLocationPath, "qualcomm");
                    fh.clearText(lastConfigDir);
                    currentConfigView.ForeColor = System.Drawing.Color.Red;
                    currentConfigView.Text = "Asset configuration denied!\nSelect new configuration.";
                    removeAssetNumberColumn();
                    dataGridView1.Rows.Clear();
                    dataGridView2.Rows.Clear();
                    clearAssetButton1.Visible = false;
                    addManStdAssetNum.Visible = false;
                    avalAddressView.Text = "0";
                }
            }
        }

        //Choose location onsite for adding asset numbers standard 
        private void onsiteLocationRB_CheckedChanged(object sender, EventArgs e)
        {

            if (onsiteLocationRB.Checked == true)
            {
                bool loginAttempt;

                loginAttempt = login.loginInputBox("Onsite Login", "Enter Login and Password.", "login");

                if (loginAttempt)
                {
                    directoryPath = "C:/Station Configs/System/onsite_directory.txt";
                    stationPath = dh.setDirectory(directoryPath);
                    stationPaths = dh.getDirectoryFilePaths(stationPath);
                    stationList = dh.getDirectoryFilesNames(stationPath);
                    if (stationComboBox != null)
                    {
                        //Destroy current comobo box
                        stationComboBox.Dispose();
                        //Unsubscribe event handler
                        stationComboBox.SelectedIndexChanged -= (stationComboBox_SelectedIndexChanged);
                        setStationComboBox();
                    }
                    //update status label
                    currentNameConvView.Text = "Onsite";
                    fh.clearText(currentLocationPath);
                    fh.textWriter(currentLocationPath, "onsite");
                    currentConfigView.ForeColor = System.Drawing.Color.Green;
                    currentConfigView.Text = "Asset configuration granted!\nSelect new configuration!";
                    addAssetNumberColumn();
                    dataGridView1.Rows.Clear();
                    dataGridView2.Rows.Clear();
                    clearAssetButton1.Visible = true;
                    addManStdAssetNum.Visible = true;
                    avalAddressView.Text = "0";
                }
                else
                {
                    onsiteLocationRB.Checked = false;
                }               
            }
        }

        //Clear text from model textbox if clicked
        private void modelTextBox_Click(object sender, EventArgs e)
        {
            if (modelTextBox.Text.Equals("e.g. HP 53131A"))
            {
                modelTextBox.Text = "";
                modelTextBox.ForeColor = System.Drawing.SystemColors.WindowText;
            }         
        }

        //Clear text from address textbox if clicked
        private void addressTextBox_Click(object sender, EventArgs e)
        {
            string addresses = addressSelectionExample();
            if (addressTextBox.Text.Equals(addresses))
            {
                addressTextBox.Text = "";
                addressTextBox.ForeColor = System.Drawing.SystemColors.WindowText;
            }
                
        }

        //Clear text from alias text box if clicked
        private void aliasTextBox_Click(object sender, EventArgs e)
        {
            if (aliasTextBox.Text.Equals("Use known alias in current program"))
            {
                aliasTextBox.Text = "";
                aliasTextBox.ForeColor = System.Drawing.SystemColors.WindowText;
            }                
        }

        //Exit Application
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
            try
            {
                string selectedValue = stationComboBox.SelectedItem.ToString();
                if (selectedValue.Equals("") || selectedValue == null)
                {
                    MessageBox.Show("You must choose a station config from the station tab\nbefore Exiting the application!", "Error/Warning!");
                }
                else
                {
                    DialogResult dialog = MessageBox.Show(this, "Are you sure you want to close?", "Closing", MessageBoxButtons.YesNo);
                    if (dialog == DialogResult.Yes)
                    {
                        exit = true;
                        Application.Exit();
                    }                         
                    if(dialog == DialogResult.No)
                    {
                        //do nothing
                    }
                }               
            }
            catch (Exception)
            {
                MessageBox.Show("You must choose a station config from the station tab\nbefore Exiting the application!", "Error/Warning!");
            }          
        }

        //Event for adding remote standard
        private void addRemoteStdButton_Click(object sender, EventArgs e)
        {
            addValidated = validateRemoteEntries();
            int remoteManualSeperator =0;
            int count = 0;
            if (addValidated)
            {
                //Build new model string
                string addConfigLine = address + ", @" + alias + ": " + model;
                //Clear and trim model list array
                modelList.Clear();
                modelList.TrimExcess();
                //reload model list array and get position of blank line sperating remote and manual standards
                modelResults = fh.textReaderAll(metcalConfigPath);
                foreach (string line in modelResults)
                {
                    modelList.Add(line);
                    if (line == "" || line == null)
                    {
                        remoteManualSeperator = count;
                        count++;
                    }
                    count++;
                }
                //Add new instrement to arraylist
                modelList.Insert(remoteManualSeperator, addConfigLine);
                fh.clearText(tempMetcalConfigPath);
                //Load temp array and write to temp file
                string[] temp = new string[modelList.Count];
                for (int i = 0; i < temp.Length; i++)
                {
                    temp[i] = modelList[i];
                }
                fh.textWriterAll(tempMetcalConfigPath, temp);
                loadConfigFile(tempMetcalConfigPath);
                //Add new standard to config datagrid
                dataGridView2.Rows.Add(address, alias, model);
                dataGridView1.Rows.Add(address, alias, model);
                //Build warning box if not turned off
                string result = fh.textReader(warnPathStation);
                if (result.Equals("unchecked"))
                {
                    warn.warningBox("Warning!", "Once the configuration is changed added standards will be lost!", "Check to disable this message", "station");
                }
                addressTextBox.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
                parseAddress(tempMetcalConfigPath);
                avalibleAddress = 30 - addressList.Count;
                avalAddressView.Text = "" + avalibleAddress;
                addressTextBox.Text = addressSelectionExample();
                modelTextBox.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
                modelTextBox.Text = "e.g. HP 53131A";
                aliasTextBox.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
                aliasTextBox.Text = "Use known alias in current program";                                
            }
            else
            {
                MessageBox.Show("Entry Error! Please recheck entries", "Error!");                
            }
        }

        //Clear text address text box if tabbed into
        private void addressTextBox_Enter(object sender, EventArgs e)
        {
            string addresses = addressSelectionExample();
            if (addressTextBox.Text.Equals(addresses))
            {
                addressTextBox.Text = "";
                addressTextBox.ForeColor = System.Drawing.SystemColors.WindowText;
            }
        }

        //Clear text model text box if tabbed into
        private void modelTextBox_Enter(object sender, EventArgs e)
        {
            if (modelTextBox.Text.Equals("e.g. HP 53131A"))
            {
                modelTextBox.Text = "";
                modelTextBox.ForeColor = System.Drawing.SystemColors.WindowText;
            }
        }

        //Clear text alias text box if tabbed into
        private void aliasTextBox_Enter(object sender, EventArgs e)
        {
            if (aliasTextBox.Text.Equals("Use known alias in current program"))
            {
                aliasTextBox.Text = "";
                aliasTextBox.ForeColor = System.Drawing.SystemColors.WindowText;
            }
        }

        //Add example back in model text box if text is null
        private void modelTextBox_Leave(object sender, EventArgs e)
        {
            if (modelTextBox.Text.Equals(""))
            {
                modelTextBox.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
                modelTextBox.Text = "e.g. HP 53131A";
            }
        }

        //Add example back in model address box if text is null
        private void addressTextBox_Leave(object sender, EventArgs e)
        {
            if (addressTextBox.Text.Equals(""))
            {
                addressTextBox.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
                addressTextBox.Text = addressSelectionExample();
            }
        }

        //Add example back in alias text box if text is null
        private void aliasTextBox_Leave(object sender, EventArgs e)
        {
            if (aliasTextBox.Text.Equals(""))
            {
                aliasTextBox.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
                aliasTextBox.Text = "Use known alias in current program";
            }
        }

        //Change head configuration on event
        private void comboBox9500B_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (startMethods)
            {
                radio9510.Checked = false;
                radio9520.Checked = false;
                radio9530.Checked = false;
                radio9550.Checked = false;
                radio9560.Checked = false;
                radioNoHead.Checked = true;
                //Get selected index of combo box
                index9500BSelect = comboBox9500B.SelectedIndex;
                //Parse asset numbers of selected index 
                parse9500BHeadsAssetNumbers(path9500B, index9500BSelect);
                //Set the labels to reflect channel/asset setup
                set9500BHeads(asset9500B);
                //Clear config and rewrite saved state config
                fh.clearText(last9500BHeadConfigPath);
                string indexChange = "" + index9500BSelect;
                fh.textWriter(last9500BHeadConfigPath, indexChange);
                //Load config files
                load9500BConfig();
                //Parse model numbers
                //Update avalible addresses to configure
                parseAddress(metcalConfigPath);
                //Asset number list capacity of address list
                assetList.Clear();
                assetList.TrimExcess();
                assetList.Capacity = addressList.Count;
                for (int i = 0; i < assetList.Capacity; i++)
                {
                    assetList.Add("");
                }
                //Parse model numbers
                parseRemoteModelNumbers(metcalConfigPath);
                //Parse alias names
                parseAliasNames(metcalConfigPath);
                //Set address available in text box
                //Add address and models to data grid
                addDataGridData("stationsTab");
                addDataGridData("addStandardTab");                
            }
            
        }

        //Add Channel 5 9510
        private void radio9510_CheckedChanged(object sender, EventArgs e)
        {
            if (radio9510.Checked == true)
            {
                string[] results = fh.textReaderAll(path9500B);
                string line9500B = results[index9500BSelect];
                Array.Clear(results, 0, results.Length);
                results = fh.textReaderAll(metcalConfigPath);


                for (int i = 0; i < results.Length; i++)
                {
                    if (results[i].Contains("9500"))
                    {
                        results[i] = line9500B;
                        break;
                    }
                }
                fh.clearText(metcalConfigPath);
                fh.textWriterAll(metcalConfigPath, results);
                if (assetNumberTextBoxOpt.Text.Equals(""))                         
                    headLabel5.Text = "9510";
                else
                    headLabel5.Text = "9510 - " + assetNumberTextBoxOpt.Text;
                line9500B = "";
                results = fh.textReaderAll(metcalConfigPath);
                for (int i = 0; i < results.Length; i++)
                {
                    if (results[i].Contains("@9500"))
                    {
                        line9500B = results[i];
                        if (assetNumberTextBoxOpt.Text.Equals(""))
                            line9500B += ", Datron 9510 [5]";
                        else
                            line9500B += ", Datron 9510 { " + assetNumberTextBoxOpt.Text +" } [5]";
                        results[i] = line9500B;
                        fh.clearText(metcalConfigPath);
                        fh.textWriterAll(metcalConfigPath, results);                        
                        parseRemoteModelNumbers(metcalConfigPath);
                        //Add address and models to data grid
                        addDataGridData("stationsTab");
                        addDataGridData("addStandardTab");
                        break;
                    }
                }
            }
        }

        //Add Channel 5 9520
        private void radio9520_CheckedChanged(object sender, EventArgs e)
        {
            if (radio9520.Checked == true)
            {
                string[] results = fh.textReaderAll(path9500B);
                string line9500B = results[index9500BSelect];
                Array.Clear(results, 0, results.Length);
                results = fh.textReaderAll(metcalConfigPath);


                for (int i = 0; i < results.Length; i++)
                {
                    if (results[i].Contains("9500"))
                    {
                        results[i] = line9500B;
                        break;
                    }
                }
                fh.clearText(metcalConfigPath);
                fh.textWriterAll(metcalConfigPath, results);
                if (assetNumberTextBoxOpt.Text.Equals(""))
                    headLabel5.Text = "9520";
                else
                    headLabel5.Text = "9520 - " + assetNumberTextBoxOpt.Text;
                line9500B = "";
                results = fh.textReaderAll(metcalConfigPath);
                for (int i = 0; i < results.Length; i++)
                {
                    if (results[i].Contains("@9500"))
                    {
                        line9500B = results[i];
                        if (assetNumberTextBoxOpt.Text.Equals(""))
                            line9500B += ", Datron 9520 [5]";
                        else
                            line9500B += ", Datron 9520 { " + assetNumberTextBoxOpt.Text + " } [5]";
                        results[i] = line9500B;
                        fh.clearText(metcalConfigPath);
                        fh.textWriterAll(metcalConfigPath, results);                        
                        parseRemoteModelNumbers(metcalConfigPath);
                        //Add address and models to data grid
                        addDataGridData("stationsTab");
                        addDataGridData("addStandardTab");
                        break;
                    }
                }
            }
        }

        //Add Channel 5 9530
        private void radio9530_CheckedChanged(object sender, EventArgs e)
        {
            if (radio9530.Checked == true)
            {
                string[] results = fh.textReaderAll(path9500B);
                string line9500B = results[index9500BSelect];
                Array.Clear(results, 0, results.Length);
                results = fh.textReaderAll(metcalConfigPath);


                for (int i = 0; i < results.Length; i++)
                {
                    if (results[i].Contains("9500"))
                    {
                        results[i] = line9500B;
                        break;
                    }
                }
                fh.clearText(metcalConfigPath);
                fh.textWriterAll(metcalConfigPath, results);
                if (assetNumberTextBoxOpt.Text.Equals(""))
                    headLabel5.Text = "9530";
                else
                    headLabel5.Text = "9530 - " + assetNumberTextBoxOpt.Text;
                line9500B = "";
                results = fh.textReaderAll(metcalConfigPath);
                for (int i = 0; i < results.Length; i++)
                {
                    if (results[i].Contains("@9500"))
                    {
                        line9500B = results[i];
                        if (assetNumberTextBoxOpt.Text.Equals(""))
                            line9500B += ", Datron 9530 [5]";
                        else
                            line9500B += ", Datron 9530 { " + assetNumberTextBoxOpt.Text + " } [5]";
                        results[i] = line9500B;
                        fh.clearText(metcalConfigPath);
                        fh.textWriterAll(metcalConfigPath, results);
                        parseRemoteModelNumbers(metcalConfigPath);
                        //Add address and models to data grid
                        addDataGridData("stationsTab");
                        addDataGridData("addStandardTab");
                        break;
                    }
                }
            }
        }

        //Add Channel 5 9550
        private void radio9550_CheckedChanged(object sender, EventArgs e)
        {
            if (radio9550.Checked == true)
            {
                string[] results = fh.textReaderAll(path9500B);
                string line9500B = results[index9500BSelect];
                Array.Clear(results, 0, results.Length);
                results = fh.textReaderAll(metcalConfigPath);


                for (int i = 0; i < results.Length; i++)
                {
                    if (results[i].Contains("9500"))
                    {
                        results[i] = line9500B;
                        break;
                    }
                }
                fh.clearText(metcalConfigPath);
                fh.textWriterAll(metcalConfigPath, results);
                if (assetNumberTextBoxOpt.Text.Equals(""))
                    headLabel5.Text = "9550";
                else
                    headLabel5.Text = "9550 - " + assetNumberTextBoxOpt.Text;
                line9500B = "";
                results = fh.textReaderAll(metcalConfigPath);
                for (int i = 0; i < results.Length; i++)
                {
                    if (results[i].Contains("@9500"))
                    {
                        line9500B = results[i];
                        if (assetNumberTextBoxOpt.Text.Equals(""))
                            line9500B += ", Datron 9550 [5]";
                        else
                            line9500B += ", Datron 9550 { " + assetNumberTextBoxOpt.Text + " } [5]";
                        results[i] = line9500B;
                        fh.clearText(metcalConfigPath);
                        fh.textWriterAll(metcalConfigPath, results);
                        parseRemoteModelNumbers(metcalConfigPath);
                        //Add address and models to data grid
                        addDataGridData("stationsTab");
                        addDataGridData("addStandardTab");
                        break;
                    }
                }
            }
        }

        //Add Channel 5 9560
        private void radio9560_CheckedChanged(object sender, EventArgs e)
        {
            if (radio9560.Checked == true)
            {
                string[] results = fh.textReaderAll(path9500B);
                string line9500B = results[index9500BSelect];
                Array.Clear(results, 0, results.Length);
                results = fh.textReaderAll(metcalConfigPath);

                for (int i = 0; i < results.Length; i++)
                {
                    if (results[i].Contains("9500"))
                    {
                        results[i] = line9500B;
                        break;
                    }
                }
                fh.clearText(metcalConfigPath);
                fh.textWriterAll(metcalConfigPath, results);
                if (assetNumberTextBoxOpt.Text.Equals(""))
                    headLabel5.Text = "9560";
                else
                    headLabel5.Text = "9560 - " + assetNumberTextBoxOpt.Text;
                line9500B = "";
                results = fh.textReaderAll(metcalConfigPath);
                for (int i = 0; i < results.Length; i++)
                {
                    if (results[i].Contains("@9500"))
                    {
                        line9500B = results[i];
                        if (assetNumberTextBoxOpt.Text.Equals(""))
                            line9500B += ", Fluke 9560 [5]";
                        else
                            line9500B += ", Fluke 9560 { " + assetNumberTextBoxOpt.Text + " } [5]";
                        results[i] = line9500B;
                        fh.clearText(metcalConfigPath);
                        fh.textWriterAll(metcalConfigPath, results);
                        parseRemoteModelNumbers(metcalConfigPath);
                        //Add address and models to data grid
                        addDataGridData("stationsTab");
                        addDataGridData("addStandardTab");
                        break;
                    }
                }
            }
        }

        //No head on Ch 5
        private void radioNoHead_CheckedChanged(object sender, EventArgs e)
        {
            if (radioNoHead.Checked == true)
            {
                headLabel5.Text = "None";
                assetNumberTextBoxOpt.Text = "";
                string[] results = fh.textReaderAll(path9500B);
                string line9500B = results[index9500BSelect];
                results = fh.textReaderAll(metcalConfigPath);


                for (int i = 0; i < results.Length; i++)
                {
                    if (results[i].Contains("9500"))
                    {
                        results[i] = line9500B;
                        break;
                    }
                }
                fh.clearText(metcalConfigPath);
                fh.textWriterAll(metcalConfigPath, results);
                parseRemoteModelNumbers(metcalConfigPath);
                //Add address and models to data grid
                addDataGridData("stationsTab");
                addDataGridData("remoteTab");
            }
        }
        //Add new set to 9500B Configuration
        private void addSetButton_Click(object sender, EventArgs e)
        {
            List<string> addItems = new List<string>();
            bool valid = true;
            string selectedValue = "";
            try
            {
                selectedValue = addSetComboBox.SelectedItem.ToString();
            }
            catch (NullReferenceException)
            {
               //Used if statement for check
            }
            if (selectedValue.Equals(""))
            {
                MessageBox.Show("You must Select a Set from the Drop Down List!");
            }
            string ch1 = addSetTextBox1.Text;
            if (head9500Validation(ch1))
            {
                addItems.Add(addSetTextBox1.Text);
            }
            else
            {
                MessageBox.Show("Check asset number format for Ch 1", "Error");
                valid = false;
            }
            string ch2 = addSetTextBox2.Text;
            if (head9500Validation(ch2))
            {
                addItems.Add(addSetTextBox2.Text);
            }
            else
            {
                MessageBox.Show("Check asset number format for Ch 2", "Error");
                valid = false;
            }
            string ch3 = addSetTextBox3.Text;
            if (head9500Validation(ch3))
            {
                addItems.Add(addSetTextBox3.Text);
            }
            else
            {
                MessageBox.Show("Check asset number format for Ch 3", "Error");
                valid = false;
            }
            string ch4 = addSetTextBox4.Text;
            if (head9500Validation(ch4))
            {
                addItems.Add(addSetTextBox4.Text);
            }
            else
            {
                MessageBox.Show("Check asset number format for Ch 4", "Error");
                valid = false;
            }
            if(ch1.Equals("") && ch2.Equals("") && ch3.Equals("") && ch4.Equals(""))
            {
                valid = false;
                MessageBox.Show("No assets were entered!", "Error");
            }
            if (valid)
            {
                
            int count = 1;
            string line = "19, @9500 : Datron 9500 (C5,G3,B)";
            if (!selectedValue.Equals("9560"))
            {
                foreach (string asset in addItems)
                {
                    if (!asset.Equals(""))
                    {
                        line += ", Datron " + selectedValue + " { " + asset + " } [" + count + "]";
                        count++;
                    }                            
                }
            }
            else
            {
                foreach (string asset in addItems)
                {
                    if (!asset.Equals(""))
                    {
                        line += ", Fluke " + selectedValue + " { " + asset + " } [" + count + "]";
                        count++;
                    }                                
                }
            }
            addItems.Clear();
            addItems.TrimExcess();
            string[] results = fh.textReaderAll(path9500B);
            string[] newResults = new string[results.Length + 1];
            for (int i = 0; i < results.Length; i++)
            {
                addItems.Add(results[i]);
            }
            addItems.Add(line);
            int index = 0;
            foreach (string item in addItems)
            {
                newResults[index] = item;
                index++;
            }
            fh.clearText(path9500B);
            fh.textWriterAll(path9500B, newResults);
            load9500ComboBox(path9500B);
            comboBox9500B.SelectedIndex = index - 1;
            addSetComboBox.SelectedIndex = 0;
            addSetTextBox1.Text = "";
            addSetTextBox2.Text = "";
            addSetTextBox3.Text = "";
            addSetTextBox4.Text = "";         
           
        }
                
    }

        //Delete set from 9500b Configuration
        private void deleteSetButton_Click(object sender, EventArgs e)
        {
            string result = fh.textReader(warnPathHead);
            if (result.Equals("unchecked"))
            {
                warn.warningBox("Warning!", "Once the heads are deleted, the data will be lost unless rentered!", "Check to disable this message", "head");
            }

            List<string> removeItem = new List<string>();
           
            int selectedValue = comboBox9500B.SelectedIndex;        
            string[] results = fh.textReaderAll(path9500B);
            string[] newResults = new string[results.Length - 1];
            for (int i = 0; i < results.Length; i++)
            {
                removeItem.Add(results[i]);
            }
            removeItem.Remove(results[selectedValue]);
            int index = 0;
            foreach (string item in removeItem)
            {
                if (!item.Equals(""))
                {
                    newResults[index] = item;
                    index++;
                }                
            }
            fh.clearText(path9500B);
            fh.textWriterAll(path9500B, newResults);
            load9500ComboBox(path9500B);
            comboBox9500B.SelectedIndex = 0;           
        }

        //Launch Software Change Request
        private void launchSCR_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(scrPath);
        }

        //Launch Break Script
        private void lanuchBreak_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(breakScriptPath);
            }
            //open install program
            catch (Win32Exception)
            {
                MessageBox.Show("Required software to run scripts is not installed,\nnow opening installer application", "Install Software");
                try
                {
                    System.Diagnostics.Process.Start(scriptInstallPath);
                }
                catch (Win32Exception)
                {
                    MessageBox.Show("You decided not to install the software", "Notification");
                }
            }
        }

        //Launch Metcal
        private void luanchMetcal_Click(object sender, EventArgs e)
        {
            try
            {
                //Add code to delete recent used in temp folder and recent used local xml file
                System.Diagnostics.Process.Start(metcalLaunchPath);
            }
            catch(Win32Exception)
            {
                MessageBox.Show("File not Found!", "Error");
            }
        }

        //Launch Metcal Editor
        private void launchEditor_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(editorLaunchPath);
            }
            catch (Win32Exception)
            {
                MessageBox.Show("File not Found!", "Error");
            }
        }
        //Launch Metteam
        private void metteamButton_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(metteamPath);
            }
            catch (Win32Exception)
            {
                MessageBox.Show("File not Found!", "Error");
            }
        }

        //Launch My Apps
        private void launchMyApps_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(myAppsPath);
        }

        //Launch NI Max
        private void LaunchNiMax_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(niMaxPath);
            }
            catch (Win32Exception)
            {
                MessageBox.Show("File not Found!", "Error");
            }
        }

        //Add asset number to config file for onsite asset number column 1
        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (startMethods)
            {
                int column;
                int row;
                string assetNumber = "";
                string redoAsset = "";                
                bool addAsset = true;

                //Get cell, row and value from selected cell and add asset number to array
                column = e.ColumnIndex;
                row = e.RowIndex;
                try
                {
                    if(row <= modelList.Count)
                    assetNumber = dataGridView1[column, row].Value.ToString();
                    else
                    {
                        assetNumber = null;
                        dataGridView1[column, row].Value = "";
                    }
                }
                catch (Exception)
                {
                    modelResults = fh.textReaderAll(metcalConfigPath);
                    //Parse out asset number if asset number already exists
                    if (modelResults[row].Contains("{") || modelResults[row].Contains("}"))
                    {                        
                        redoAsset = clearAssetNumbers(modelResults[row]);
                        modelResults[row] = redoAsset;
                        redoAsset = "";
                    }
                    dataGridView1[column, row].Value = "";
                    dataGridView2[column, row].Value = "";
                    fh.clearText(metcalConfigPath);
                    fh.textWriterAll(metcalConfigPath, modelResults);
                    addAsset = false;
                    if (assetNumber.Equals("") && assetList.Count > row)
                    {
                        assetList[row] = "";
                    }
                }
                //Check against current entries
                if (assetList.Count > 0 && !assetNumber.Equals(""))
                {
                    for (int i = 0; i < assetList.Count; i++)
                    {
                        if (assetList[i].Equals(assetNumber) && i != row)
                        {
                            addAsset = false;
                            MessageBox.Show("You already entered that asset number!", "Error!");
                            dataGridView1[column, row].Value = "";
                            dataGridView2[column, row].Value = "";
                            assetList[row] = "";
                            break;
                        }
                    }
                }
                if (addAsset)
                {
                    //make assetList match current amount of addresses used if remote standards have been added
                    parseAddress(metcalConfigPath);
                    if (addressList.Count > assetList.Count)
                    {
                        int size = assetList.Count;
                        string[] tempArray = new string[size];
                        for (int i = 0; i < assetList.Count; i++)
                        {
                            tempArray[i] = assetList[i];
                        }
                        assetList.Clear();
                        assetList.TrimExcess();
                        assetList.Capacity = addressList.Count;
                        for (int i = 0; i < assetList.Capacity; i++)
                        {
                            assetList.Add("");
                        }

                        for (int i = 0; i < tempArray.Length; i++)
                        {
                            assetList[i] = tempArray[i];
                        }
                    }
                    assetList[row] = assetNumber;                    
                    //Get current config list
                    modelResults = fh.textReaderAll(metcalConfigPath);
                    //Parse out asset number if asset number already exists
                    if (modelResults[row].Contains("{") || modelResults[row].Contains("}"))
                    {
                        redoAsset = clearAssetNumbers(modelResults[row]);
                        modelResults[row] = redoAsset;
                        redoAsset = "";
                    }
                    //Add Asset Number to selected asset
                    modelResults[row] += " { " + assetNumber + " }";                        
                    fh.clearText(metcalConfigPath);
                    fh.textWriterAll(metcalConfigPath, modelResults);                    
                    string result = fh.textReader(warnPathAsset);
                    dataGridView2[column, row].Value = assetNumber;
                    dataGridView1.FirstDisplayedCell.Selected = true;
                    if (result.Equals("unchecked"))
                    {
                        warn.warningBox("Warning!", "Once the configuration is changed added asset number will be lost!", "Check to disable this message", "asset");
                    }
                }                            
            }
        }

        //Add asset number to config file for onsite asset number column 2
        private void dataGridView2_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (startMethods)
            {
                int column;
                int row;
                string assetNumber = "";
                string redoAsset = "";
                bool addAsset = true;

                //Get cell, row and value from selected cell and add asset number to array
                column = e.ColumnIndex;
                row = e.RowIndex;
                try
                {
                    if (row <= modelList.Count)
                        assetNumber = dataGridView2[column, row].Value.ToString();
                    else
                    {
                        assetNumber = null;
                        dataGridView2[column, row].Value = "";
                    }
                }
                catch (Exception)
                {
                    modelResults = fh.textReaderAll(metcalConfigPath);
                    //Parse out asset number if asset number already exists
                    if (modelResults[row].Contains("{") || modelResults[row].Contains("}"))
                    {
                        redoAsset = clearAssetNumbers(modelResults[row]);
                        modelResults[row] = redoAsset;
                        redoAsset = "";
                    }
                    dataGridView2[column, row].Value = "";
                    dataGridView1[column, row].Value = "";
                    fh.clearText(metcalConfigPath);
                    fh.textWriterAll(metcalConfigPath, modelResults);
                    addAsset = false;
                    if (assetNumber.Equals("") && assetList.Count > row)
                    {
                        assetList[row] = "";
                    }
                }
                //Check against current entries
                if (assetList.Count > 0 && !assetNumber.Equals(""))
                {
                    for (int i = 0; i < assetList.Count; i++)
                    {
                        if (assetList[i].Equals(assetNumber) && i != row)
                        {
                            addAsset = false;
                            MessageBox.Show("You already entered that asset number!", "Error!");
                            dataGridView2[column, row].Value = "";
                            dataGridView1[column, row].Value = "";
                            assetList[row] = "";
                            break;
                        }
                    }
                }
                if (addAsset)
                {
                    //make assetList match current amount of addresses used if remote standards have been added
                    parseAddress(metcalConfigPath);
                    if (addressList.Count > assetList.Count)
                    {
                        int size = assetList.Count;
                        string[] tempArray = new string[size];
                        for (int i = 0; i < assetList.Count; i++)
                        {
                            tempArray[i] = assetList[i];
                        }
                        assetList.Clear();
                        assetList.TrimExcess();
                        assetList.Capacity = addressList.Count;
                        for (int i = 0; i < assetList.Capacity; i++)
                        {
                            assetList.Add("");
                        }

                        for (int i = 0; i < tempArray.Length; i++)
                        {
                            assetList[i] = tempArray[i];
                        }
                    }
                    assetList[row] = assetNumber;
                    //Get current config list
                    modelResults = fh.textReaderAll(metcalConfigPath);
                    //Parse out asset number if asset number already exists
                    if (modelResults[row].Contains("{") || modelResults[row].Contains("}"))
                    {
                        redoAsset = clearAssetNumbers(modelResults[row]);
                        modelResults[row] = redoAsset;
                        redoAsset = "";
                    }
                    //Add Asset Number to selected asset
                    modelResults[row] += " { " + assetNumber + " }";
                    fh.clearText(metcalConfigPath);
                    fh.textWriterAll(metcalConfigPath, modelResults);
                    string result = fh.textReader(warnPathAsset);
                    dataGridView1[column, row].Value = assetNumber;
                    dataGridView2.FirstDisplayedCell.Selected = true;
                    if (result.Equals("unchecked"))
                    {
                        warn.warningBox("Warning!", "Once the configuration is changed added asset number will be lost!", "Check to disable this message", "asset");
                    }
                }
            }
        }

        //About
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Station Utility Configuration App\n\n" +
                "This Application was designed to help with the execution of\n" +
                "Fluke MetCal configuration files w/ added features\n\n" +
                "Author: David Kimery\n\nDate: 10-13-2016\nVersion: 1.3.0", "About");
        }

        //Add Manual Standard
        private void addManualStandard_Click(object sender, EventArgs e)
        {
            bool addManual = true;
            string[] results = fh.textReaderAll(metcalConfigPath);
            string manualStd = addManualTextbox.Text.ToString();
            if (manualStd == "" || manualStd == "e.g. HP 11667A")
            {
                MessageBox.Show("You must enter a model number!", "Error!");
                addManualTextbox.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
                addManualTextbox.Text = "e.g. HP 11667A";
                addManual = false;
            }
            //If in onsite location ask for asset number
            if (lastChecked.Equals("onsite"))
            {
                string  assetNum = Interaction.InputBox("Enter Asset Number:", "Manual Standard", "Asset Number", -1, -1);
                manualStd += " { " + assetNum + " }";
            }
            if (addManual)
            {
                //Build new model string
                string addConfigLine = ": " + manualStd;
                //Clear and trim model list array
                modelList.Clear();
                modelList.TrimExcess();
                //reload model list array
                modelResults = fh.textReaderAll(metcalConfigPath);
                foreach (string line in modelResults)
                {
                    modelList.Add(line);
                }
                modelList.Add(addConfigLine);                
                fh.clearText(tempMetcalConfigPath);
                //Load temp array and write to temp file
                string[] temp = new string[modelList.Count];
                for (int i = 0; i < temp.Length; i++)
                {
                    temp[i] = modelList[i];
                }
                fh.textWriterAll(tempMetcalConfigPath, temp);
                loadConfigFile(tempMetcalConfigPath);
                string result = fh.textReader(warnPathStation);
                if (result.Equals("unchecked"))
                {
                    warn.warningBox("Warning!", "Once the configuration is changed added standards will be lost!", "Check to disable this message", "station");
                }
                addManualTextbox.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
                addManualTextbox.Text = "e.g. HP 11667A";
            }
        }

        //Clear add manual text if clicked
        private void addManualTextbox_Click(object sender, EventArgs e)
        {
            if (addManualTextbox.Text.Equals("e.g. HP 11667A"))
            {
                addManualTextbox.Text = "";
                addManualTextbox.ForeColor = System.Drawing.SystemColors.WindowText;
            }
        }

        //Clear add manual text if tabbed to
        private void addManualTextbox_Enter(object sender, EventArgs e)
        {
            if (addManualTextbox.Text.Equals("e.g. HP 11667A"))
            {
                addManualTextbox.Text = "";
                addManualTextbox.ForeColor = System.Drawing.SystemColors.WindowText;
            }
        }

        //Reenter manual standard placeholder
        private void addManualTextbox_Leave(object sender, EventArgs e)
        {
            if (addManualTextbox.Text.Equals(""))
            {
                addManualTextbox.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
                addManualTextbox.Text = "e.g. HP 11667A";
            }
        }

        //View Manual standards
        private void viewManualStdButton_Click(object sender, EventArgs e)
        {
            string[] manualStds = parseManualStandards();
            string viewResults = "";
            foreach (string line in manualStds)
            {
                if (line != null)
                {
                    viewResults += line + "\n";
                }
            }
            MessageBox.Show("Current Manual Standards:\n\n" + viewResults, "Manual Standards");            
        }

        //Change  login and Password
        private void changePasswordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool loginAttempt = login.loginInputBox("Enter Login and Password", "Please enter login and password.", "login");
            if (loginAttempt)
            {
                login.loginInputBox("Enter New Login and Password", "Please enter new login and password.", "newLogin");                
            }
        }

        //Minimize Window 
        private void minimizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        //Email for troubleshooting 
        private void requestChangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Email email = new Email();
            email.createEmail("Change Request Email", "Enter Request Below:");
        }

        //Override close button to avoid files being over written with null text
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            
                if (exit) Application.Exit();
                else
                {
                try
                {
                    string selectedValue = stationComboBox.SelectedItem.ToString();
                    if (selectedValue.Equals("") || selectedValue == null)
                    {
                        MessageBox.Show("You must choose a station config from the station tab\nbefore Exiting the application!", "Error/Warning!");
                        e.Cancel = true;
                    }
                    base.OnFormClosing(e);

                    if (e.CloseReason == CloseReason.WindowsShutDown) return;

                    // Confirm user wants to close
                    switch (MessageBox.Show(this, "Are you sure you want to close?", "Closing", MessageBoxButtons.YesNo))
                    {
                        case DialogResult.No:
                            e.Cancel = true;
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("You must choose a station config from the station tab\nbefore Exiting the application!", "Error/Warning!");
                    e.Cancel = true;
                }
            }
                
        }

        //Clear all asset numbers when pushed column 1
        private void clearAssetButton1_Click(object sender, EventArgs e)
        {
            clearAssetNumbers();
        }
        //Clear all asset numbers when pushed column 2
        private void clearAssetButton2_Click(object sender, EventArgs e)
        {
            clearAssetNumbers();
        }

        //The following radio button events are to change the alias of flexible standards as needed
        private void RB5720_CheckedChanged(object sender, EventArgs e)
        {           
            if (RB5720.Checked == true)
            {
                file5720Update.Text = "";
                file5730Update.Text = "";
                file5522Update.Text = "";
                string[] results = fh.textReaderAll(metcalConfigPath);
                for (int i = 0; i < results.Length; i++)
                {
                    if (results[i].Contains("5720"))
                    {
                        results[i] = addressList[i] + ", @DVC, @5720, @5700A :Fluke 5720A";
                    }
                    if (results[i].Contains("5730"))
                    {
                        results[i] = addressList[i] + ", @5730 :Fluke 5730A";
                    }
                    if (results[i].Contains("5522"))
                    {
                        results[i] = addressList[i] + ", @5522A, @5520, @5520A, @5522, @5500, @5500A :Fluke 5522A (G1)";
                    }
                }               
                //Write to text files
                fh.clearText(lastDC);
                fh.textWriter(lastDC, "5720A");
                fh.clearText(metcalConfigPath);
                fh.textWriterAll(metcalConfigPath, results);
                //Update avalible addresses to configure
                avalibleAddress = parseAddress(metcalConfigPath);
                avalAddressView.Text = "" + avalibleAddress;
                //Parse model numbers
                parseRemoteModelNumbers(metcalConfigPath);
                //Parse alias names
                parseAliasNames(metcalConfigPath);                
                if (startMethods)
                {
                    file5720Update.Text = "Config File Updated";
                    //Build Data Grid
                    addDataGridData("stationsTab");
                    addDataGridData("addStandardTab");
                }                
            }                        
        }
       
        private void RB5730_CheckedChanged(object sender, EventArgs e)
        {            
            if (RB5730.Checked == true)
            {
                file5720Update.Text = "";
                file5730Update.Text = "";
                file5522Update.Text = "";
                string[] results = fh.textReaderAll(metcalConfigPath);
                for (int i = 0; i < results.Length; i++)
                {
                    if (results[i].Contains("5720"))
                    {
                        results[i] = addressList[i] + ", @5720, @5700A :Fluke 5720A";
                    }
                    if (results[i].Contains("5730"))
                    {
                        results[i] = addressList[i] + ", @DVC, @5730 :Fluke 5730A";
                    }
                    if (results[i].Contains("5522"))
                    {
                        results[i] = addressList[i] + ", @5522A, @5520, @5520A, @5522, @5500, @5500A :Fluke 5522A (G1)";
                    }
                }               
                //Write to text files
                fh.clearText(lastDC);
                fh.textWriter(lastDC, "5730A");
                fh.clearText(metcalConfigPath);
                fh.textWriterAll(metcalConfigPath, results);
                //Update avalible addresses to configure
                avalibleAddress = parseAddress(metcalConfigPath);
                avalAddressView.Text = "" + avalibleAddress;
                //Parse model numbers
                parseRemoteModelNumbers(metcalConfigPath);
                //Parse alias names
                parseAliasNames(metcalConfigPath);                
                if (startMethods)
                {
                    file5730Update.Text = "Config File Updated";
                    //Build Data Grid
                    addDataGridData("stationsTab");
                    addDataGridData("addStandardTab");
                }
            }                       
        }
        
        private void RB5522_CheckedChanged(object sender, EventArgs e)
        {            
            if (RB5522.Checked == true)
            {
                file5720Update.Text = "";
                file5730Update.Text = "";
                file5522Update.Text = "";
                string[] results = fh.textReaderAll(metcalConfigPath);
                for (int i = 0; i < results.Length; i++)
                {
                    if (results[i].Contains("5720"))
                    {
                        results[i] = addressList[i] + ", @5720, @5700A :Fluke 5720A";
                    }
                    if (results[i].Contains("5730"))
                    {
                        results[i] = addressList[i] + ", @5730 :Fluke 5730A";
                    }
                    if (results[i].Contains("5522"))
                    {
                        results[i] = addressList[i] + ", @DVC, @5522A, @5520, @5520A, @5522, @5500, @5500A :Fluke 5522A (G1)";
                    }
                }               
                //Write to text files
                fh.clearText(lastDC);
                fh.textWriter(lastDC, "5522A");
                fh.clearText(metcalConfigPath);
                fh.textWriterAll(metcalConfigPath, results);
                //Update avalible addresses to configure
                avalibleAddress = parseAddress(metcalConfigPath);
                avalAddressView.Text = "" + avalibleAddress;
                //Parse model numbers
                parseRemoteModelNumbers(metcalConfigPath);
                //Parse alias names
                parseAliasNames(metcalConfigPath);                
                if (startMethods)
                {
                    file5522Update.Text = "Config File Updated";
                    //Build Data Grid
                    addDataGridData("stationsTab");
                    addDataGridData("addStandardTab");
                }
            }            
        }

        private void RB4418x_CheckedChanged(object sender, EventArgs e)
        {
            if (RB4418x.Checked == true)
            {
                file4418xUpdate.Text = "";                
                file4419xUpdate.Text = "";               
                file1912Aupdate.Text = "";                                
                string[] results = fh.textReaderAll(metcalConfigPath);
                for (int i = 0; i < results.Length; i++)
                {
                    if (results[i].Contains("E4418x"))
                    {
                        results[i] = addressList[i] + ", @PWRMTR, @4418, @4418x :Agilent E4418x";
                    }
                    if (results[i].Contains("E4419x"))
                    {
                        results[i] = addressList[i] + ", @4419, @4419x :Agilent E4419x";
                    }
                    if (results[i].Contains("N1912A"))
                    {
                        results[i] = addressList[i] + ", @442,@E4419,@441x,@N1912A,@1912,@1912A,@N1912,@N1911,@N1911A,@1911,@1911A :Agilent N1912A";
                    }                    
                }               
                //Write to text files
                fh.clearText(lastPM);
                fh.textWriter(lastPM, "E4418x");
                fh.clearText(metcalConfigPath);
                fh.textWriterAll(metcalConfigPath, results);
                //Update avalible addresses to configure
                avalibleAddress = parseAddress(metcalConfigPath);
                avalAddressView.Text = "" + avalibleAddress;
                //Parse model numbers
                parseRemoteModelNumbers(metcalConfigPath);
                //Parse alias names
                parseAliasNames(metcalConfigPath);               
                if (startMethods)
                {
                    file4418xUpdate.Text = "Config File Updated";
                    //Build Data Grid
                    addDataGridData("stationsTab");
                    addDataGridData("addStandardTab");
                }
            }
        }       

        private void RB4419x_CheckedChanged(object sender, EventArgs e)
        {
            if (RB4419x.Checked == true)
            {
                file4418xUpdate.Text = "";
                file4419xUpdate.Text = "";
                file1912Aupdate.Text = "";               
                string[] results = fh.textReaderAll(metcalConfigPath);
                for (int i = 0; i < results.Length; i++)
                {
                    if (results[i].Contains("E4418x"))
                    {
                        results[i] = addressList[i] + ", @4418, @4418x :Agilent E4418x";
                    }
                    if (results[i].Contains("E4419x"))
                    {
                        results[i] = addressList[i] + ", @PWRMTR, @4419, @4419x :Agilent E4419x";
                    }
                    if (results[i].Contains("N1912A"))
                    {
                        results[i] = addressList[i] + ", @442,@E4419,@441x,@N1912A,@1912,@1912A,@N1912,@N1911,@N1911A,@1911,@1911A :Agilent N1912A";
                    }
                }               
                //Write to text files
                fh.clearText(lastPM);
                fh.textWriter(lastPM, "E4419x");
                fh.clearText(metcalConfigPath);
                fh.textWriterAll(metcalConfigPath, results);
                //Update avalible addresses to configure
                avalibleAddress = parseAddress(metcalConfigPath);
                avalAddressView.Text = "" + avalibleAddress;
                //Parse model numbers
                parseRemoteModelNumbers(metcalConfigPath);
                //Parse alias names
                parseAliasNames(metcalConfigPath);                
                if (startMethods)
                {
                    file4419xUpdate.Text = "Config File Updated";
                    //Build Data Grid
                    addDataGridData("stationsTab");
                    addDataGridData("addStandardTab");
                }
            }
        }

        private void RB1912A_CheckedChanged(object sender, EventArgs e)
        {
            if (RB1912A.Checked == true)
            {
                file4418xUpdate.Text = "";
                file4419xUpdate.Text = "";
                file1912Aupdate.Text = "";                
                string[] results = fh.textReaderAll(metcalConfigPath);
                for (int i = 0; i < results.Length; i++)
                {
                    if (results[i].Contains("E4418x"))
                    {
                        results[i] = addressList[i] + ", @4418, @4418x :Agilent E4418x";
                    }
                    if (results[i].Contains("E4419x"))
                    {
                        results[i] = addressList[i] + ", @4419, @4419x :Agilent E4419x";
                    }
                    if (results[i].Contains("N1912A"))
                    {
                        results[i] = addressList[i] + ", @PWRMTR, @442,@E4419,@441x,@N1912A,@1912,@1912A,@N1912,@N1911,@N1911A,@1911,@1911A :Agilent N1912A";
                    }
                }               
                //Write to text files
                fh.clearText(lastPM);
                fh.textWriter(lastPM, "N1912A");
                fh.clearText(metcalConfigPath);
                fh.textWriterAll(metcalConfigPath, results);
                //Update avalible addresses to configure
                avalibleAddress = parseAddress(metcalConfigPath);
                avalAddressView.Text = "" + avalibleAddress;
                //Parse model numbers
                parseRemoteModelNumbers(metcalConfigPath);
                //Parse alias names
                parseAliasNames(metcalConfigPath);               
                if (startMethods)
                {
                    file1912Aupdate.Text = "Config File Updated";
                    //Build Data Grid
                    addDataGridData("stationsTab");
                    addDataGridData("addStandardTab");
                }
            }
        }

        private void RB8247_CheckedChanged(object sender, EventArgs e)
        {
            if (RB8247.Checked == true)
            {
                file8247update.Text = "";
                file8257update.Text = "";
                file518xxUpdate.Text = "";
                file4438update.Text = "";
                bool found4438 = false;
                string[] results = fh.textReaderAll(metcalConfigPath);
                for (int i = 0; i < results.Length; i++)
                {
                    if (results[i].Contains("E8247C"))
                    {
                        results[i] = addressList[i] + ", @LO, @E8247 :Agilent E8247C";
                    }
                    if (results[i].Contains("E8257C"))
                    {
                        results[i] = addressList[i] + ", @E8257 :Agilent E8257C";
                    }
                    if (results[i].Contains("N518xx"))
                    {
                        results[i] = addressList[i] + ", @MXG :Agilent N518xx";
                    }
                    if (results[i].Contains("E4438") && found4438 == false)
                    {
                        results[i] = addressList[i] + ", @E4438, @4438 :Agilent E4438C";
                        found4438 = true;
                    }
                }
                //Write to text files
                fh.clearText(lastLO);
                fh.textWriter(lastLO, "E8247C");
                fh.clearText(metcalConfigPath);
                fh.textWriterAll(metcalConfigPath, results);
                //Update avalible addresses to configure
                avalibleAddress = parseAddress(metcalConfigPath);
                avalAddressView.Text = "" + avalibleAddress;
                //Parse model numbers
                parseRemoteModelNumbers(metcalConfigPath);
                //Parse alias names
                parseAliasNames(metcalConfigPath);
                if (startMethods)
                {
                    file8247update.Text = "Config File Updated";
                    //Build Data Grid
                    addDataGridData("stationsTab");
                    addDataGridData("addStandardTab");
                }
            }
        }

        private void RB8257_CheckedChanged(object sender, EventArgs e)
        {
            if (RB8257.Checked == true)
            {
                file8247update.Text = "";
                file8257update.Text = "";
                file518xxUpdate.Text = "";
                file4438update.Text = "";
                bool found4438 = false;
                string[] results = fh.textReaderAll(metcalConfigPath);
                for (int i = 0; i < results.Length; i++)
                {
                    if (results[i].Contains("E8247C"))
                    {
                        results[i] = addressList[i] + ", @E8247 :Agilent E8247C";
                    }
                    if (results[i].Contains("E8257C"))
                    {
                        results[i] = addressList[i] + ", @LO, @E8257 :Agilent E8257C";
                    }
                    if (results[i].Contains("N518xx"))
                    {
                        results[i] = addressList[i] + ", @MXG :Agilent N518xx";
                    }
                    if (results[i].Contains("E4438") && found4438 == false)
                    {
                        results[i] = addressList[i] + ", @E4438, @4438 :Agilent E4438C";
                        found4438 = true;
                    }
                }
                //Write to text files
                fh.clearText(lastLO);
                fh.textWriter(lastLO, "E8257C");
                fh.clearText(metcalConfigPath);
                fh.textWriterAll(metcalConfigPath, results);
                //Update avalible addresses to configure
                avalibleAddress = parseAddress(metcalConfigPath);
                avalAddressView.Text = "" + avalibleAddress;
                //Parse model numbers
                parseRemoteModelNumbers(metcalConfigPath);
                //Parse alias names
                parseAliasNames(metcalConfigPath);
                if (startMethods)
                {
                    file8257update.Text = "Config File Updated";
                    //Build Data Grid
                    addDataGridData("stationsTab");
                    addDataGridData("addStandardTab");
                }
            }
        }

        private void RB518xx_CheckedChanged(object sender, EventArgs e)
        {
            if (RB518xx.Checked == true)
            {
                file8247update.Text = "";
                file8257update.Text = "";
                file518xxUpdate.Text = "";
                file4438update.Text = "";
                bool found4438 = false;
                string[] results = fh.textReaderAll(metcalConfigPath);
                for (int i = 0; i < results.Length; i++)
                {
                    if (results[i].Contains("E8247C"))
                    {
                        results[i] = addressList[i] + ", @E8247 :Agilent E8247C";
                    }
                    if (results[i].Contains("E8257C"))
                    {
                        results[i] = addressList[i] + ", @E8257 :Agilent E8257C";
                    }
                    if (results[i].Contains("N518xx"))
                    {
                        results[i] = addressList[i] + ", @LO, @MXG :Agilent N518xx";
                    }
                    if (results[i].Contains("E4438") && found4438 == false)
                    {
                        results[i] = addressList[i] + ", @E4438, @4438 :Agilent E4438C";
                        found4438 = true;
                    }
                }
                //Write to text files
                fh.clearText(lastLO);
                fh.textWriter(lastLO, "N518xx");
                fh.clearText(metcalConfigPath);
                fh.textWriterAll(metcalConfigPath, results);
                //Update avalible addresses to configure
                avalibleAddress = parseAddress(metcalConfigPath);
                avalAddressView.Text = "" + avalibleAddress;
                //Parse model numbers
                parseRemoteModelNumbers(metcalConfigPath);
                //Parse alias names
                parseAliasNames(metcalConfigPath);
                if (startMethods)
                {
                    file518xxUpdate.Text = "Config File Updated";
                    //Build Data Grid
                    addDataGridData("stationsTab");
                    addDataGridData("addStandardTab");
                }
            }
        }

        private void RB4438_CheckedChanged(object sender, EventArgs e)
        {
            if (RB4438.Checked == true)
            {
                file8247update.Text = "";
                file8257update.Text = "";
                file518xxUpdate.Text = "";
                file4438update.Text = "";
                bool found4438 = false;
                string[] results = fh.textReaderAll(metcalConfigPath);
                for (int i = 0; i < results.Length; i++)
                {
                    if (results[i].Contains("E8247D"))
                    {
                        results[i] = addressList[i] + ", @E8247 :Agilent E8247C";
                    }
                    if (results[i].Contains("E8257C"))
                    {
                        results[i] = addressList[i] + ", @E8257 :Agilent E8257C";
                    }
                    if (results[i].Contains("N518xx"))
                    {
                        results[i] = addressList[i] + ", @MXG :Agilent N518xx";
                    }
                    if (results[i].Contains("E4438")  && found4438 == false)
                    {
                        results[i] = addressList[i] + ", @LO, @E4438, @4438 :Agilent E4438C";
                        found4438 = true;
                    }
                }
                //Write to text files
                fh.clearText(lastLO);
                fh.textWriter(lastLO, "E4438C");
                fh.clearText(metcalConfigPath);
                fh.textWriterAll(metcalConfigPath, results);
                //Update avalible addresses to configure
                avalibleAddress = parseAddress(metcalConfigPath);
                avalAddressView.Text = "" + avalibleAddress;
                //Parse model numbers
                parseRemoteModelNumbers(metcalConfigPath);
                //Parse alias names
                parseAliasNames(metcalConfigPath);
                if (startMethods)
                {
                    file4438update.Text = "Config File Updated";
                    //Build Data Grid
                    addDataGridData("stationsTab");
                    addDataGridData("addStandardTab");
                }
            }
        }

        //Add manual standard asset number
        private void addManStdAssetNum_Click(object sender, EventArgs e)
        {
            manualAsset = "";
            standardSelect = -1;
            int count= 0;
            string[] manualStds = parseManualStandards();
            bool remoteConfigLine = true;
            string replaceAsset = "";            
            DialogResult dialog = buildManualSelectInputBox(manualStds);
            if ((dialog == DialogResult.OK) && (manualAsset != null && standardSelect >= 0))
            {
                if (manualAsset == "")
                {
                    replaceAsset = manualStds[standardSelect];
                    for (int i = 0; i < manualStds[standardSelect].Length; i++)
                    {
                        if (replaceAsset[i] != 123)
                        {
                            count++;
                        }
                        else if (replaceAsset[i] == 123)
                        {
                            count--;
                            break;
                        }
                    }
                    manualStds[standardSelect] = replaceAsset.Substring(0, count);
                }
                else if (manualAsset != "" && !manualStds[standardSelect].Contains("{"))
                {
                    manualStds[standardSelect] += " { " + manualAsset + " }";
                }
                else if (manualAsset != "" && manualStds[standardSelect].Contains("{"))
                {
                    replaceAsset = manualStds[standardSelect];
                    for (int i = 0; i < manualStds[standardSelect].Length; i++)
                    {
                        if (replaceAsset[i] != 123)
                        {
                            count++;
                        }
                        else if (replaceAsset[i] == 123)
                        {
                            count--;
                            break;
                        }
                    }
                    manualStds[standardSelect] = replaceAsset.Substring(0, count);
                    manualStds[standardSelect] += " { " + manualAsset + " }";
                }
                //Clear and trim model list array
                modelList.Clear();
                modelList.TrimExcess();
                //reload model list array
                modelResults = fh.textReaderAll(metcalConfigPath);
                foreach (string remoteLine in modelResults)
                {
                    if (remoteLine == null || remoteLine == "")
                    {
                        remoteConfigLine = false;
                        modelList.Add(remoteLine);
                        break;
                    }
                    else if (remoteConfigLine && (remoteLine == null || remoteLine != ""))
                    {
                        modelList.Add(remoteLine);
                    }
                }
                foreach (string manualLine in manualStds)
                {
                    modelList.Add(": " + manualLine);
                }                
                //Load temp array and write to temp file
                string[] temp = new string[modelList.Count];
                for (int i = 0; i < temp.Length; i++)
                {
                    temp[i] = modelList[i];
                }
                fh.clearText(metcalConfigPath);
                fh.textWriterAll(metcalConfigPath, temp);
                string result = fh.textReader(warnPathAsset);
                dataGridView1.FirstDisplayedCell.Selected = true;
                if (result.Equals("unchecked"))
                {
                    warn.warningBox("Warning!", "Once the configuration is changed added asset number will be lost!", "Check to disable this message", "asset");
                }
            }                    
        }

        //Get data from selected row
        private void getRowData_Click(object sender, EventArgs e)
        {
            bool selected = true;
            DataGridViewRow row = new DataGridViewRow();
            rowIndex = dataGridView2.CurrentCell.RowIndex;
            try
            {
                row = this.dataGridView2.SelectedRows[0];
            }
            catch (Exception)
            {
                selected = false;
                MessageBox.Show("You need to select a row", "Error");
            }
            if (selected)
            {
                int address = (int)row.Cells["Address2"].Value;
                string aliases = (string)row.Cells["aliasNames2"].Value;
                string model = (string)row.Cells["ModelNumber2"].Value;
                editRemoteStdGroupBox.Enabled = true;
                editModelTextBox.Text = model;
                editAddressTextBox.Text = address.ToString();
                editAliasTextBox.Text = aliases.Replace(" ", "");
            }

        }

        //Submit changes for config row in datagrid view
        private void submitChangesButton_Click(object sender, EventArgs e)
        {
            //Parse out and verfiy changes
            int newAddress = 0;
            string[] newAliases;
            string newModel;
            bool submitChange = true;
            newModel = editModelTextBox.Text;
            if (newModel == "")
            {
                MessageBox.Show("You did not enter a model", "Error");              
                submitChange = false;
                editModelTextBox.Text = modelList[rowIndex];
            }
            foreach (string modelCheck in modelList)
            {
                if (newModel.Equals(modelCheck))
                {
                    if (newModel.Equals(modelList[rowIndex]))
                    {
                        break;
                    }
                    else
                    {
                        MessageBox.Show("You must enter an unused model number", "Error");
                        editModelTextBox.Text = modelList[rowIndex];
                        submitChange = false;
                        break;
                    }                    
                }
            }
            editAliasTextBox.Text = editAliasTextBox.Text.Replace(" ", "");           
            newAliases = editAliasTextBox.Text.Split(',');
            if (newAliases == null)
            {
                MessageBox.Show("You did not enter any aliases", "Error!");                
                submitChange = false;
                editAliasTextBox.Text = string.Join(",", aliasList[rowIndex]);

            }
            int count = 0;            
            foreach (string[] aliasArray in aliasList)
            {
                foreach (string aliasCheck in aliasArray)
                {
                    for (int i = 0; i < newAliases.Length; i++)
                    {
                        if (count == rowIndex)
                        {
                            //do nothing
                        }
                        else if (aliasCheck.Equals(newAliases[i]))
                        {
                            MessageBox.Show("You must enter an unused alias", "Error");
                            submitChange = false;
                            editAliasTextBox.Text = string.Join(",", aliasList[rowIndex]);
                        }
                    }
                }
                count++;       
            }
            try
            {
                newAddress = int.Parse(editAddressTextBox.Text);
                foreach (int index in addressList)
                {
                    if (newAddress == addressList[rowIndex])
                    {
                        break;
                    }
                    else if(index == newAddress)
                    {
                        submitChange = false;
                        MessageBox.Show("That address is already in use", "Error");
                        editAddressTextBox.Text = addressList[rowIndex].ToString();
                    }
                }
                
            }
            catch(Exception)
            {
                submitChange = false;
                MessageBox.Show("You did not enter a number", "Error");
                editAddressTextBox.Text = addressList[rowIndex].ToString();
            }
            //Display changes and verify with user that they want to make those changes
            if (submitChange)
            {
                foreach (string index in newAliases)
                {

                }
                DialogResult dialog = MessageBox.Show("Verify changes are correct:\n\nModel: " + newModel + "\n" + "Address: " + newAddress + "\n" + "Aliases: " + string.Join(",", newAliases),
                      "Please Verify", MessageBoxButtons.YesNo);
                switch (dialog)
                {
                    case DialogResult.Yes:
                        string[] results = fh.textReaderAll(metcalConfigPath);
                        string aliasConfig = "";
                        for (int i = 0; i < newAliases.Length; i++)
                        {
                            if(i == newAliases.Length - 1)
                            {
                                aliasConfig = aliasConfig + "@" + newAliases[i] + ": ";
                            }else
                            {
                                aliasConfig = aliasConfig + "@" + newAliases[i] + ", ";
                            }                            
                        }
                        string newConfigLine = newAddress + ", " + aliasConfig + newModel;
                        if(lastChecked.Equals("onsite"))
                        {
                            newConfigLine = newConfigLine + " { " + assetList[rowIndex] + " } ";
                        }
                        results[rowIndex] = newConfigLine;
                        fh.clearText(metcalConfigPath);
                        fh.textWriterAll(metcalConfigPath, results);
                        //Parse model numbers
                        parseRemoteModelNumbers(metcalConfigPath);
                        //Parse alias names
                        parseAliasNames(metcalConfigPath);
                        //Set address available in text box
                        addressTextBox.Text = addressSelectionExample();                       
                        //Add address, alias, and models to data grid
                        addDataGridData("stationsTab");
                        addDataGridData("addStandardTab");
                        editModelTextBox.Text = "";
                        editAddressTextBox.Text = "";
                        editAliasTextBox.Text = "";
                        editRemoteStdGroupBox.Enabled = false;
                        break;                                      
                    case DialogResult.No: break;
                }
            }
            
        }

        //Instructions for adding a new 9500B head's set
        private void instructions_button_Click(object sender, EventArgs e)
        {
            MessageBox.Show("When to use the Add New Set Fucntion:\n\n" +
                            "1) When a new set is purchased you can enter the asset\n" +
                            "   here to add a new set to the 9500 Configuratrion list.\n\n" +
                            "2) If a head is replaced in an old set with a new asset\n" +
                            "   number, delete the old set containg the no longer valid\n" +
                            "   head and add the old set back in the configuration with the\n" +
                            "   new head's asset number and the other old heads asset numbers.\n\n" +
                            "3) To delete an old configuration select the set from the 9500\n" +
                            "   Configuratrion and click the Delete Current Config button.");

        }

        //Cancel edit remote standard changes
        private void cancelChangesButton_Click(object sender, EventArgs e)
        {
            editModelTextBox.Text = "";
            editAddressTextBox.Text = "";
            editAliasTextBox.Text = "";
            editRemoteStdGroupBox.Enabled = false;
        }


        /// Helper methods start here ///


        //Build Station ComboBox
        private void setStationComboBox()
        {
            stationComboBox = new ComboBox();
            stationComboBox.FormattingEnabled = false;
            stationComboBox.Location = new System.Drawing.Point(90, 35);
            stationComboBox.Name = "stationComboBox";
            stationComboBox.Size = new System.Drawing.Size(133, 21);
            stationComboBox.TabIndex = 0;
            stationComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            //Add stationList array to combo box
            foreach (string name in stationList)
            {
                stationComboBox.Items.Add(name);
            }
            //Add event handler
            stationComboBox.SelectedIndexChanged += new EventHandler(stationComboBox_SelectedIndexChanged);
            stationTabGroupBox.Controls.Add(stationComboBox);
        }

        //Set last configuration state
        private void setLastState()
        {
            lastChecked = fh.textReader(currentLocationPath);
            lastCongfigNameSelected = fh.textReader(lastConfigNamePath);
            lastConfig = fh.textReader(lastConfigDir);
            tabControl.TabPages.Clear();
            tabControl.TabPages.Add(stationsTab);
            tabControl.TabPages.Add(addStandardTab);
            int stationIndex = 0;
            currentConfigView.ForeColor = System.Drawing.SystemColors.HotTrack;
            string textNumber = fh.textReader(last9500BHeadConfigPath);           
            try
            {
                index9500BSelect = Int32.Parse(textNumber);
            }
            catch (FormatException)
            {
                MessageBox.Show("There was an error parrsing data in file");
            }

            //Last setup from qualcomm
            if (lastChecked.Equals("qualcomm"))
            {
                //Set directory path
                directoryPath = "C:/Station Configs/System/default_directory.txt";
                stationPath = dh.setDirectory(directoryPath);
                stationPaths = dh.getDirectoryFilePaths(stationPath);
                stationList = dh.getDirectoryFilesNames(stationPath);
                for (int i = 0; i < stationList.Length; i++)
                {
                    if (stationList[i].Equals(lastCongfigNameSelected))
                    {
                        stationIndex = i;
                    }
                }
                //update status label
                currentNameConvView.Text = "Qualcomm";                         
                //Remove Asset Number Column
                removeAssetNumberColumn();
                //Hide remove asset number button
                clearAssetButton1.Visible = false;
                addManStdAssetNum.Visible = false;
                loadConfigFile(lastConfig);
                currentConfigView.Text = lastCongfigNameSelected;
                //Intialize Windows dynamic Compenents
                setStationComboBox();
                stationComboBox.SelectedIndex = stationIndex;
                //Parse addresses and get number of available addresses
                parseAddress(lastConfig);
                avalAddressView.Text = "" + avalibleAddress;
                //Asset number list capacity of address list
                assetList.Capacity = addressList.Count;
                for (int i = 0; i < assetList.Capacity; i++)
                {
                    assetList.Add("");
                }
                //Parse models numbers
                parseRemoteModelNumbers(lastConfig);
                //Parse alias names
                parseAliasNames(lastConfig);
                //Set address available in text box
                addressTextBox.Text = addressSelectionExample();
                //Build tab for 9500B Configuration if needed
                tab9500B();
                //Build tab for Flexible Standard if needed
                flexStdTabControl();
                //Set last FLexible standard if used
                string dc = fh.textReader(lastDC);
                string pm = fh.textReader(lastPM);
                string lo = fh.textReader(lastLO);
                setCurrentFlex(dc, pm, lo);
                //Build Data Grid
                addDataGridData("stationsTab");
                addDataGridData("addStandardTab");
                //Get metcal.ini data for proc spliting and serial port
                getIniData();
                //Start method calls
                startMethods = true;
            }

            //Last setup if onsite
            else if (lastChecked.Equals("onsite"))
            {
                bool loginAttempt;

                loginAttempt = login.loginInputBox("Onsite Login", "Enter Login and Password.", "login");

                if (loginAttempt)
                {
                    //Set directory path
                    directoryPath = "C:/Station Configs/System/onsite_directory.txt";
                    stationPath = dh.setDirectory(directoryPath);
                    stationPaths = dh.getDirectoryFilePaths(stationPath);
                    stationList = dh.getDirectoryFilesNames(stationPath);
                    for (int i = 0; i < stationList.Length; i++)
                    {
                        if (stationList[i].Equals(lastCongfigNameSelected))
                        {
                            stationIndex = i;
                        }
                    }
                    //update status label
                    currentNameConvView.Text = "Onsite";
                    //Add asset number column 
                    addAssetNumberColumn();
                    loadConfigFile(lastConfig);
                    currentConfigView.Text = lastCongfigNameSelected;
                    //Intialize Windows dynamic Compenents
                    setStationComboBox();
                    stationComboBox.SelectedIndex = stationIndex;
                    //Parse addresses and get number of available addresses
                    parseAddress(lastConfig);
                    avalAddressView.Text = "" + avalibleAddress;
                    //Asset number list capacity of address list
                    assetList.Capacity = addressList.Count;                   
                    for (int i = 0; i < assetList.Capacity; i++)
                    {
                        assetList.Add("");
                    }                   
                    //Parse models numbers
                    parseRemoteModelNumbers(lastConfig);
                    //Parse alias names
                    parseAliasNames(lastConfig);
                    //Set address available in text box
                    addressTextBox.Text = addressSelectionExample();
                    //Build tab for 9500B Configuration if needed
                    tab9500B();
                    //Build tab for Flexible Standard if needed
                    flexStdTabControl();
                    //Set last FLexible standard if used
                    string dc = fh.textReader(lastDC);
                    string pm = fh.textReader(lastPM);
                    string lo = fh.textReader(lastLO);
                    setCurrentFlex(dc, pm, lo);
                    //Build Data Grid
                    addDataGridData("stationsTab");
                    addDataGridData("addStandardTab");
                    //Get metcal.ini data for proc spliting and serial port
                    getIniData();
                    //Start method calls
                    startMethods = true;
                    //show remove asset number button
                    clearAssetButton1.Visible = true;
                    addManStdAssetNum.Visible = true;
                }
                else
                {
                    reset = true;                    
                    qualcommLocationRB.Checked = true;
                }
            }    
        }

        //Load last config selected to local station config file
        private void loadConfigFile(string path)
        {
            fh.clearText(metcalConfigPath);
            configResults = fh.textReaderAll(path);
            fh.textWriterAll(metcalConfigPath, configResults);
        }

        //Parse current config file addresses to array list
        private int parseAddress(string path)
        {
            addressList.Clear();
            addressList.TrimExcess();
            addressResults = fh.textReaderAll(path);
            char[] strArray;
            string str = "";
            int digit = 0;
            for (int i = 0; i < addressResults.Length; i++)
            {
                sr = new StringReader(addressResults[i]);
                strArray = new char[addressResults[i].Length];

                if (addressResults[i].Length != 0)
                {
                    sr.Read(strArray, 0, 4);
                    if (strArray[1] == 44)//ASCII character for a comma is 44
                    {
                        str = "" + strArray[0];
                        digit = Int32.Parse(str);
                        addressList.Add(digit);
                    }
                    else if (strArray[2] == 44)
                    {
                        str = "" + strArray[0] + strArray[1];
                        digit = Int32.Parse(str);
                        addressList.Add(digit);
                    }
                    else if (strArray[3] == 58) //ASCII for colon is 58
                    {
                        if (strArray[1].Equals(""))
                        {
                            str = "" + strArray[0];
                            digit = Int32.Parse(str);
                            addressList.Add(digit);
                        }
                        else
                        {
                            str = "" + strArray[0] + strArray[1];
                            digit = Int32.Parse(str);
                            addressList.Add(digit);
                        }
                    }
                }
                if (addressResults[i].Length == 0) break;
            }
            return avalibleAddress = 30 - addressList.Count;

        }

        //Parse current confit file model numbers to array list
        private void parseRemoteModelNumbers(string path)
        {
            char space = ' ';
            int index = 0;
            modelList.Clear();
            modelList.TrimExcess();
            modelResults = fh.textReaderAll(path);
            foreach (string line in modelResults)
            {
                modelResults[index] = line.Trim(space);
                index++;
            }
            char[] strArray;
            string modelNumber;
            string asset;
            string model9500B = "9500";            
            string [] dcArray = fh.textReaderAll(DCpath);
            string[] loArray = fh.textReaderAll(LOpath);
            string[] pmArray = fh.textReaderAll(PMpath);        
            int count;
            int colonIndex;
            int bracketIndex;
            int bracket = 123;
            bool foundColon;
            bool foundBracket;
            bool foundAsset;
            bool oscope9500configured = false;
            bool modelCheck = false;
            oscopeCal = false;
            DC = false;
            PWRMTR = false;
            LO = false;
            for (int i = 0; i < modelResults.Length; i++)
            {
                sr = new StringReader(modelResults[i]);
                strArray = new char[modelResults[i].Length + 1];
                foundColon = false;
                foundBracket = false;
                foundAsset = false;
                colonIndex = 0;
                bracketIndex = 0;
                asset = "";               
                count = 1;
                index = 1;
                modelNumber = "";
                modelCheck = modelResults[i].Contains(model9500B);
                if (modelResults[i].Length != 0 && !modelCheck)
                {
                    sr.Read(strArray, 0, strArray.Length);
                    while (!foundColon)
                    {
                        for (int j = 0; j < strArray.Length; j++)
                        {
                            if (strArray[j] == 58)//ASCII amount for colon
                            {
                                colonIndex = j;
                                foundColon = true;
                                break;
                            }
                        }
                    }
                    while (strArray[colonIndex + count] != 0 && foundBracket == false)
                    {
                        if (strArray[colonIndex + count] != 32 && strArray[colonIndex + count] != bracket)
                        {
                            modelNumber = modelNumber + strArray[colonIndex + count];
                            count++;
                        }
                        if (strArray[colonIndex + count] == 32)
                        {
                            modelNumber = modelNumber + " ";
                            count++;
                        }
                        if (strArray[colonIndex + count] == bracket)
                        {
                            foundBracket = true;
                            bracketIndex = colonIndex + count;
                            foundAsset = true;
                        }
                    }
                    //Add asset if found
                    while (foundAsset)
                    {
                            
                        if (strArray[bracketIndex + index] != bracket && strArray[bracketIndex + index] != 32)
                        {
                            asset += strArray[bracketIndex + index];
                            index++;
                        }

                        if (strArray[bracketIndex + index] == 32)
                        {
                            index++;
                        }
                        if (strArray[bracketIndex + index] == 125)
                        {
                            foundAsset = false;
                        }
                        
                    }
                    //Load needed arrays if asset number is found
                    if (!asset.Equals("") && asset != null && assetNumbers1.Visible == true)
                    {
                        assetList[i] = asset;
                        
                    }                                      
                }
                //Get config for last 9500 selected localy
                else if (modelCheck)
                {
                    string config9500 = "";
                    if (!oscope9500configured)
                    {
                        string[] results = fh.textReaderAll(path9500B);
                        config9500 = results[index9500BSelect];
                        while (!foundColon)
                        {
                            for (int j = 0; j < config9500.Length; j++)
                            {
                                if (config9500[j] == 58)//ASCII amount for colon
                                {
                                    colonIndex = j;
                                    foundColon = true;
                                    break;
                                }
                            }                           
                        }
                        modelNumber = config9500.Substring(colonIndex + count);
                        oscope9500configured = true;
                    }
                    else
                    {
                        config9500 = modelResults[i];
                        for (int j = 0; j < config9500.Length; j++)
                        {
                            if (config9500[j] == 58)//ASCII amount for colon
                            {
                                colonIndex = j;
                                foundColon = true;
                                break;
                            }
                        }
                        modelNumber = config9500.Substring(colonIndex + count);
                    }                  
                }         
                modelNumber = modelNumber.Trim(space);
                modelList.Add(modelNumber);
                //See if 9500B or flexible standards are present 
                modelCheck = modelNumber.Contains(model9500B);
                if (modelCheck)
                {
                    oscopeCal = true;
                }
                for (int j = 0; j < dcArray.Length; j++)
                {
                    modelCheck = modelNumber.Contains(dcArray[j]);
                    if (modelCheck)
                    {
                        DC = true;
                        break;
                    }                   
                }
                for (int j = 0; j < dcArray.Length; j++)
                {
                    modelCheck = modelNumber.Contains(pmArray[j]);
                    if (modelCheck)
                    {
                        PWRMTR = true;
                        break;
                    }
                }
                for (int j = 0; j < dcArray.Length; j++)
                {
                    modelCheck = modelNumber.Contains(loArray[j]);
                    if (modelCheck)
                    {
                        LO = true;
                        break;
                    }
                }                
                //else if for if not apart of model checks
                if (modelResults[i].Length == 0)
                {
                    break;
                }
            }
        }

        //Parse current config file alias names to array list
        private void parseAliasNames(string path)
        {
            int comma = 44;
            int atSign = 64;
            int colon = 58;
            int space = 32;
            char[] strArray;
            int size;
            string alias;
            bool foundAtSign;
            bool foundComma; 
            bool foundColon;
            aliasList.Clear();
            aliasList.TrimExcess();
            string[] results = fh.textReaderAll(path);
            for (int i = 0; i < results.Length; i++)
            {
                if(!results[i].Equals(""))
                {
                    sr = new StringReader(results[i]);
                    strArray = new char[results[i].Length];
                    sr.Read(strArray, 0, strArray.Length);
                    foundColon = false;
                    foundAtSign = false;
                    foundComma = false;
                    alias = "";
                    size = 0;
                    if (strArray[1] == comma || strArray[2] == comma)
                    {
                        for (int index = 0; index < strArray.Length; index++)
                        {

                            while (!foundAtSign && !foundColon)
                            {
                                if (strArray[index] == atSign)
                                {
                                    foundAtSign = true;
                                    foundComma = false;
                                    index++;
                                }
                                if (!foundAtSign) index++;
                            }
                            while (!foundComma && !foundColon)
                            {
                                if (strArray[index] != comma && strArray[index] != colon && strArray[index] != space)
                                {
                                    alias += strArray[index];
                                    index++;
                                }
                                if (strArray[index] == space)
                                {
                                    index++;
                                }
                                if (strArray[index] == comma)
                                {
                                    foundComma = true;
                                    foundAtSign = false;
                                    alias += strArray[index];
                                }
                                if (strArray[index] == colon)
                                {
                                    foundColon = true;
                                }
                            }
                        }
                    }
                    else if(strArray[0] != colon && strArray[0] != space)
                    {
                        alias = "None";
                    }
                    if (alias.Contains(","))
                    {
                        for (int l = 0; l < strArray.Length; l++)
                        {
                            if (strArray[l] == comma) size++;
                        }
                        string[] aliasResults = new string[size];
                        aliasResults = alias.Split(',');
                        aliasList.Add(aliasResults);
                    }
                    else if(!alias.Equals(""))
                    {
                        string[] aliasResults = new string[1];
                        aliasResults[0] = alias;
                        aliasList.Add(aliasResults);
                    }
                }              
            }
        }

        //Load data grid view with addresses and model numbers
        private void addDataGridData(string tabName)
        {
            string aliasStr;
            string[] aliasStrArray;

            if (tabName.Equals("stationsTab"))
            {
                dataGridView1.Rows.Clear();               
                for (int i = 0; i < aliasList.Count; i++)
                {
                aliasStrArray = aliasList[i];
                aliasStr = "";
                for (int j = 0; j < aliasList[i].Length; j++)
                    {
                        if (aliasStrArray.Length > 1)
                        {
                            if (j != aliasStrArray.Length - 1) aliasStr += aliasStrArray[j] + ", ";
                            else aliasStr += aliasStrArray[j];
                        }
                        else
                        {
                            aliasStr += aliasStrArray[j];
                        }
                    }
                    dataGridView1.Rows.Add(addressList[i], aliasStr, modelList[i] );                    
                }
                //if asset number column is visible load asset numbers
                if (assetNumbers1.Visible == true)
                {
                    addAssetNumberToRows();
                }
            }
            if (tabName.Equals("addStandardTab"))
            {
                dataGridView2.Rows.Clear();
                for (int i = 0; i < aliasList.Count; i++)
                {
                    aliasStrArray = aliasList[i];
                    aliasStr = "";
                    for (int j = 0; j < aliasList[i].Length; j++)
                    {
                        if (aliasStrArray.Length > 1)
                        {
                            if (j != aliasStrArray.Length - 1) aliasStr += aliasStrArray[j] + ", ";
                            else aliasStr += aliasStrArray[j];
                        }
                        else
                        {
                            aliasStr += aliasStrArray[j];
                        }
                    }
                    dataGridView2.Rows.Add(addressList[i], aliasStr, modelList[i]);
                }
            }
        }

        //Set address text box example
        public string addressSelectionExample()
        {
            string example = "No Addresses Available";
            string addressAval = avalAddressView.Text.ToString();
            if (addressList.Count != 30)
            {
                example = addressAval + " Available Addresses";
            }
            return example;
        }

        //Validate enties for automated standard
        private bool validateRemoteEntries()
        {
            bool valid = true;
            address = 0;
            if (modelTextBox.Text.Equals("e.g. HP 53131A")) modelTextBox.Text = "";
            if (aliasTextBox.Text.Equals("Use known alias in current program")) aliasTextBox.Text = "";
            if (addressTextBox.Text.Contains("Address")) addressTextBox.Text = "";
            model = modelTextBox.Text.ToString();
            alias = aliasTextBox.Text.ToString();

            if (model == "")
            {
                MessageBox.Show("You did not enter a model or alias!", "Error!");               
                modelTextBox.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
                modelTextBox.Text = "e.g. HP 53131A";                
                valid = false;
            }
            if (alias == "")
            {
                MessageBox.Show("You did not enter a model or alias!", "Error!");               
                aliasTextBox.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
                aliasTextBox.Text = "Use known alias in current program";
                valid = false;
            }
            foreach (string modelCheck in modelList)
            {
                if (model.Equals(modelCheck))
                {
                    MessageBox.Show("You must enter an unused model number!", "Error!");
                    modelTextBox.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
                    modelTextBox.Text = "e.g. HP 53131A";                    
                    valid = false;
                    break;
                }
            }
            foreach (string[] aliasArray in aliasList)
            {
                foreach (string aliasCheck in aliasArray)
                {
                    if (alias.Equals(aliasCheck))
                    {
                        MessageBox.Show("You must enter an unused alias!", "Error!");                        
                        aliasTextBox.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
                        aliasTextBox.Text = "Use known alias in current program";
                        valid = false;
                    }
                }
            }
            try
            {
                address = Int32.Parse(addressTextBox.Text.ToString());
            }
            catch (FormatException)
            {
                MessageBox.Show("You must type a number!", "Error!");
                addressTextBox.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
                addressTextBox.Text = addressSelectionExample();               
                valid = false;
            }
            if (address > 0 && address < 31)
            {
                foreach (int usedAddress in addressList)
                {
                    if (usedAddress == address)
                    {
                        MessageBox.Show("You must type and unused address!", "Error!" );
                        addressTextBox.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
                        addressTextBox.Text = addressSelectionExample();                        
                        valid = false;
                        break;
                    }
                }
            }
            else
            {
                MessageBox.Show("Address must be between 1 and 30!", "Error!");
                addressTextBox.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
                addressTextBox.Text = addressSelectionExample();               
                valid = false;
            }

            return valid;
        }

        //Add tab for flexible standard selection
        private void flexStdTabControl()
        {
            try
            {
                if ((DC || PWRMTR || LO) && tabControl.TabPages.Contains(flexStdTab) == false)
                {
                    tabControl.TabPages.Add(flexStdTab);

                }
                else if ((DC || PWRMTR || LO) && tabControl.TabPages.Contains(flexStdTab) == true)
                {
                    //Do nothing
                }
                else if (tabControl.TabPages.Contains(flexStdTab) == true)
                {
                    tabControl.TabPages.Clear();
                    tabControl.TabPages.Add(stationsTab);
                    tabControl.TabPages.Add(addStandardTab);
                }
            }
            //Catching exception from tab build/clearing and ignoring it to enbale tab build/clear on start up of app.
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);
                //Test and fixed any other exceptions that might have been thrown
            }
        }

        //Add tab, components, and data for 9500B Configuration 
        private void tab9500B()
        {           
            try
            {
                if (oscopeCal && tabControl.TabPages.Contains(tabFor9500B) == false)
                {
                    tabControl.TabPages.Add(tabFor9500B);
                   //Load combo box for 9500 config                                  
                    load9500ComboBox(path9500B);                   
                    //Parse 9500 Head asset numbers
                    parse9500BHeadsAssetNumbers(path9500B, index9500BSelect);
                    //Set Labels to show current configuration
                    set9500BHeads(asset9500B);
                }
                else if (oscopeCal && tabControl.TabPages.Contains(tabFor9500B) == true)
                {
                    //Do nothing
                }
                else if (tabControl.TabPages.Contains(tabFor9500B) == true)
                {
                    tabControl.TabPages.Clear();
                    tabControl.TabPages.Add(stationsTab);
                    tabControl.TabPages.Add(addStandardTab);

                }
            }
            //Catching exception from tab build/clearing and ignoring it to enbale tab build/clear on start up of app.
            catch(Exception e)
            {
                //MessageBox.Show(e.Message);
                //Test and fixed any other exceptions that might have been thrown
            }


        }

        //Set 9500B Config
        private void set9500BHeads(string[] headsArray)
        {
            int length = 0;
            for (int i = 0; i < asset9500B.Length; i++ )
            {
                if (asset9500B[i] != null) length++;
            }
            string value = "";
            string selectedValue = comboBox9500B.GetItemText(comboBox9500B.Items[index9500BSelect]);
            if (selectedValue.Contains("9510"))
            {
                value = "9510 - ";
            }
            if (selectedValue.Contains("9520"))
            {
                value = "9520 - ";
            }
            if (selectedValue.Contains("9530"))
            {
                value = "9530 - ";
            }
            if (selectedValue.Contains("9550"))
            {
                value = "9550 - ";
            }
            if (selectedValue.Contains("9560"))
            {
                value = "9560 - ";
            }
            switch (length)
            {
                case 4:
                    headLabel1.Text = value + asset9500B[0];
                    headLabel2.Text = value + asset9500B[1];
                    headLabel3.Text = value + asset9500B[2];
                    headLabel4.Text = value + asset9500B[3];
                    comboBox9500B.SelectedIndex = index9500BSelect;
                    break;
                case 3:
                    headLabel1.Text = value + asset9500B[0];
                    headLabel2.Text = value + asset9500B[1];
                    headLabel3.Text = value + asset9500B[2];
                    headLabel4.Text = "None";
                    comboBox9500B.SelectedIndex = index9500BSelect;
                    break;
                case 2:
                    headLabel1.Text = value + asset9500B[0];
                    headLabel2.Text = value + asset9500B[1];
                    headLabel3.Text = "None";
                    headLabel4.Text = "None";
                    comboBox9500B.SelectedIndex = index9500BSelect;
                    break;
                case 1:
                    headLabel1.Text = value + asset9500B[0];
                    headLabel2.Text = "None";
                    headLabel3.Text = "None";
                    headLabel4.Text = "None";
                    comboBox9500B.SelectedIndex = index9500BSelect;
                    break;
            }
            
        }

        //Load Combo Box with saved heads
        private void load9500ComboBox(string path)
        {
            int count = 1;
            int index = 0;
            string set;
            string[] results = fh.textReaderAll(path);
            int length = 0;
            comboBox9500B.Items.Clear();
            foreach (string result in results)
            {
                char[] strArray = new char[results[index].Length];
                sr = new StringReader(results[index]);
                sr.Read(strArray, 0, strArray.Length);
                if (result.Contains("9530"))
                {
                    foreach (char strChar in strArray)
                    {
                        if (strChar == 123 || strChar == 125)
                        {
                            length++;
                        }
                    }
                    length = length / 2;
                    set = "Set " + count + " - 9530 x " + length;
                    comboBox9500B.Items.Add(set);
                    length = 0;
                }
                else if (result.Contains("9560"))
                {
                    foreach (char strChar in strArray)
                    {
                        if (strChar == 123 || strChar == 125)
                        {
                            length++;
                        }
                    }
                    length = length / 2;
                    set = "Set " + count + " - 9560 x " + length;
                    comboBox9500B.Items.Add(set);
                    length = 0;
                }
                else if (result.Contains("9550"))
                {
                    foreach (char strChar in strArray)
                    {
                        if (strChar == 123 || strChar == 125)
                        {
                            length++;
                        }
                    }
                    length = length / 2;
                    set = "Set " + count + " - 9550 x " + length;
                    comboBox9500B.Items.Add(set);
                    length = 0;
                }
                else if (result.Contains("9520"))
                {
                    foreach (char strChar in strArray)
                    {
                        if (strChar == 123 || strChar == 125)
                        {
                            length++;
                        }
                    }
                    length = length / 2;
                    set = "Set " + count + " - 9520 x " + length;
                    comboBox9500B.Items.Add(set);
                    length = 0;
                }
                else if (result.Contains("9510"))
                {
                    foreach (char strChar in strArray)
                    {
                        if (strChar == 123 || strChar == 125)
                        {
                            length++;
                        }
                    }
                    length = length / 2;
                    set = "Set " + count + " - 9510 x " + length;
                    comboBox9500B.Items.Add(set);
                    length = 0;
                }
                count++;
                index++;
            }
        }

        //Parse 9500B asset numbers
        public void parse9500BHeadsAssetNumbers(string path, int index9500B)
        {
            Array.Clear(asset9500B, 0, asset9500B.Length);
            char[] strArray;
            int count;
            int assetCount = 0;
            int bracketIndex;
            bool foundBracket;
            string asset;
            string[] results = fh.textReaderAll(path);
            sr = new StringReader(results[index9500B]);
            strArray = new char[results[index9500B].Length];
            foundBracket = false;
            bracketIndex = 0;
            asset = "";
            int index = 0;
            sr.Read(strArray, 0, strArray.Length);
            int length = strArray.Length;
            if (length >= 144)
            {
                if (results[index9500B].Length != 0)
                {
                    while (assetCount < 4)
                    {
                        asset = "";
                        while (!foundBracket)
                        {
                            for (int j = index; index < strArray.Length; j++)
                            {
                                if (strArray[j] == 123)//ASCII amount for {
                                {
                                    bracketIndex = j;
                                    foundBracket = true;
                                    index++;
                                    break;
                                }
                                index++;
                            }
                        }
                        count = 1;
                        while (foundBracket)
                        {
                            if (strArray[bracketIndex + count] == 125)
                            {
                                foundBracket = false;//ASCII amount for }                            
                            }
                            if (strArray[bracketIndex + count] != 32 && strArray[bracketIndex + count] != 125)
                            {
                                asset = asset + strArray[bracketIndex + count];
                                count++;
                                index++;
                            }
                            if (strArray[bracketIndex + count] == 32)
                            {
                                count++;
                                index++;
                            }
                        }
                        asset9500B[assetCount] = asset;
                        assetCount++;
                    }
                }
            }
            else if (length >= 120 && length < 144)
            {
                if (results[index9500B].Length != 0)
                {
                    while (assetCount < 3)
                    {
                        asset = "";
                        while (!foundBracket)
                        {
                            for (int j = index; index < strArray.Length; j++)
                            {
                                if (strArray[j] == 123)//ASCII amount for {
                                {
                                    bracketIndex = j;
                                    foundBracket = true;
                                    index++;
                                    break;
                                }
                                index++;
                            }
                        }
                        count = 1;
                        while (foundBracket)
                        {
                            if (strArray[bracketIndex + count] == 125)
                            {
                                foundBracket = false;//ASCII amount for }                            
                            }
                            if (strArray[bracketIndex + count] != 32 && strArray[bracketIndex + count] != 125)
                            {
                                asset = asset + strArray[bracketIndex + count];
                                count++;
                                index++;
                            }
                            if (strArray[bracketIndex + count] == 32)
                            {
                                count++;
                                index++;
                            }
                        }
                        asset9500B[assetCount] = asset;
                        assetCount++;
                    }
                }                
            }       
            else if (length >= 91 && length < 120)
            {
                if (results[index9500B].Length != 0)
                {
                    while (assetCount < 2)
                    {
                        asset = "";
                        while (!foundBracket)
                        {
                            for (int j = index; index < strArray.Length; j++)
                            {
                                if (strArray[j] == 123)//ASCII amount for {
                                {
                                    bracketIndex = j;
                                    foundBracket = true;
                                    index++;
                                    break;
                                }
                                index++;
                            }
                        }
                        count = 1;
                        while (foundBracket)
                        {
                            if (strArray[bracketIndex + count] == 125)
                            {
                                foundBracket = false;//ASCII amount for }                            
                            }
                            if (strArray[bracketIndex + count] != 32 && strArray[bracketIndex + count] != 125)
                            {
                                asset = asset + strArray[bracketIndex + count];
                                count++;
                                index++;
                            }
                            if (strArray[bracketIndex + count] == 32)
                            {
                                count++;
                                index++;
                            }
                        }
                        asset9500B[assetCount] = asset;
                        assetCount++;
                    }
                }                
            }
            else if (length >= 62 && length < 91)
            {
                if (results[index9500B].Length != 0)
                {
                    while (assetCount < 1)
                    {
                        asset = "";
                        while (!foundBracket)
                        {
                            for (int j = index; index < strArray.Length; j++)
                            {
                                if (strArray[j] == 123)//ASCII amount for {
                                {
                                    bracketIndex = j;
                                    foundBracket = true;
                                    index++;
                                    break;
                                }
                                index++;
                            }
                        }
                        count = 1;
                        while (foundBracket)
                        {
                            if (strArray[bracketIndex + count] == 125)
                            {
                                foundBracket = false;//ASCII amount for }                            
                            }
                            if (strArray[bracketIndex + count] != 32 && strArray[bracketIndex + count] != 125)
                            {
                                asset = asset + strArray[bracketIndex + count];
                                count++;
                                index++;
                            }
                            if (strArray[bracketIndex + count] == 32)
                            {
                                count++;
                                index++;
                            }
                        }
                        asset9500B[assetCount] = asset;
                        assetCount++;
                    }
                }                
            }                    
        }

        //Load config file with new 9500B configuration
        public void load9500BConfig()
        {
            int loadConfigIndex = 0;
            string line9500B = "";
            string[] results = fh.textReaderAll(metcalConfigPath);
            for (int i = 0; i < results.Length; i++)
            {
                if (results[i].Contains("@9500"))
                {
                    loadConfigIndex = i;
                    break;
                }
            }
            int length = 0;
            for (int i = 0; i < asset9500B.Length; i++)
            {
                if (asset9500B[i] != null) length++;
            }
            string[] results_ = fh.textReaderAll(path9500B);
            switch (length){
                case 4:
                    for (int i = 0; i < results.Length; i++)
                    {
                        if (results_[index9500BSelect].Contains("9510"))
                        {
                            line9500B = "19, @9500 : Datron 9500 (C5,G3,B), Datron 9510 { " +
                                asset9500B[0] + " } [1], Datron 9510 { " +
                                asset9500B[1] + " } [2], Datron 9510 { " +
                                asset9500B[2] + " } [3], Datron 9510 { " +
                                asset9500B[3] + " } [4]";
                            break;
                        }
                        else if (results_[index9500BSelect].Contains("9520"))
                        {
                            line9500B = "19, @9500 : Datron 9500 (C5,G3,B), Datron 9520 { " +
                                asset9500B[0] + " } [1], Datron 9520 { " +
                                asset9500B[1] + " } [2], Datron 9520 { " +
                                asset9500B[2] + " } [3], Datron 9520 { " +
                                asset9500B[3] + " } [4]";
                            break;
                        }
                        else if (results_[index9500BSelect].Contains("9530"))
                        {
                            line9500B = "19, @9500 : Datron 9500 (C5,G3,B), Datron 9530 { " +
                                asset9500B[0] + " } [1], Datron 9530 { " +
                                asset9500B[1] + " } [2], Datron 9530 { " +
                                asset9500B[2] + " } [3], Datron 9530 { " +
                                asset9500B[3] + " } [4]";
                            break;
                        }
                        else if (results_[index9500BSelect].Contains("9550"))
                        {
                            line9500B = "19, @9500 : Datron 9500 (C5,G3,B), Datron 9550 { " +
                                asset9500B[0] + " } [1], Datron 9550 { " +
                                asset9500B[1] + " } [2], Datron 9550 { " +
                                asset9500B[2] + " } [3], Datron 9550 { " +
                                asset9500B[3] + " } [4]";
                            break;
                        }
                        else if (results_[index9500BSelect].Contains("9560"))
                        {
                            line9500B = "19, @9500 : Datron 9500 (C5,G3,B), Fluke 9560 { " +
                                asset9500B[0] + " } [1], Fluke 9560 { " +
                                asset9500B[1] + " } [2], Fluke 9560 { " +
                                asset9500B[2] + " } [3], Fluke 9560 { " +
                                asset9500B[3] + " } [4]";
                            break;
                        }
                    }
                    break;
                case 3:
                    for (int i = 0; i < results.Length; i++)
                    {
                        if (results_[index9500BSelect].Contains("9510"))
                        {
                            line9500B = "19, @9500 : Datron 9500 (C5,G3,B), Datron 9510 { " +
                                asset9500B[0] + " } [1], Datron 9510 { " +
                                asset9500B[1] + " } [2], Datron 9510 { " +
                                asset9500B[2] + " } [3]";
                            break;
                        }
                        else if (results_[index9500BSelect].Contains("9520"))
                        {
                            line9500B = "19, @9500 : Datron 9500 (C5,G3,B), Datron 9520 { " +
                                asset9500B[0] + " } [1], Datron 9520 { " +
                                asset9500B[1] + " } [2], Datron 9520 { " +
                                asset9500B[2] + " } [3]";
                            break;
                        }
                        else if (results_[index9500BSelect].Contains("9530"))
                        {
                            line9500B = "19, @9500 : Datron 9500 (C5,G3,B), Datron 9530 { " +
                                asset9500B[0] + " } [1], Datron 9530 { " +
                                asset9500B[1] + " } [2], Datron 9530 { " +
                                asset9500B[2] + " } [3]";
                            break;
                        }
                        else if (results_[index9500BSelect].Contains("9550"))
                        {
                            line9500B = "19, @9500 : Datron 9500 (C5,G3,B), Datron 9550 { " +
                                asset9500B[0] + " } [1], Datron 9550 { " +
                                asset9500B[1] + " } [2], Datron 9550 { " +
                                asset9500B[2] + " } [3]";
                            break;
                        }
                        else if (results_[index9500BSelect].Contains("9560"))
                        {
                            line9500B = "19, @9500 : Datron 9500 (C5,G3,B), Fluke 9560 { " +
                                asset9500B[0] + " } [1], Fluke 9560 { " +
                                asset9500B[1] + " } [2], Fluke 9560 { " +
                                asset9500B[2] + " } [3]";
                            break;
                        }
                    }
                    break;
                case 2:
                    for (int i = 0; i < results.Length; i++)
                    {
                        if (results_[index9500BSelect].Contains("9510"))
                        {
                            line9500B = "19, @9500 : Datron 9500 (C5,G3,B), Datron 9510 { " +
                                asset9500B[0] + " } [1], Datron 9510 { " +
                                asset9500B[1] + " } [2]";
                            break;
                        }
                        else if (results_[index9500BSelect].Contains("9520"))
                        {
                            line9500B = "19, @9500 : Datron 9500 (C5,G3,B), Datron 9520 { " +
                                asset9500B[0] + " } [1], Datron 9520 { " +
                                asset9500B[1] + " } [2]";
                            break;
                        }
                        else if (results_[index9500BSelect].Contains("9530"))
                        {
                            line9500B = "19, @9500 : Datron 9500 (C5,G3,B), Datron 9530 { " +
                                asset9500B[0] + " } [1], Datron 9530 { " +
                                asset9500B[1] + " } [2]";
                            break;
                        }
                        else if (results_[index9500BSelect].Contains("9550"))
                        {
                            line9500B = "19, @9500 : Datron 9500 (C5,G3,B), Datron 9550 { " +
                                asset9500B[0] + " } [1], Datron 9550 { " +
                                asset9500B[1] + " } [2]";
                            break;
                        }
                        else if (results_[index9500BSelect].Contains("9560"))
                        {
                            line9500B = "19, @9500 : Datron 9500 (C5,G3,B), Fluke 9560 { " +
                                asset9500B[0] + " } [1], Fluke 9560 { " +
                                asset9500B[1] + " } [2]";
                            break;
                        }
                    }
                    break;
                case 1:
                    for (int i = 0; i < results.Length; i++)
                    {
                        if (results_[index9500BSelect].Contains("9510"))
                        {
                            line9500B = "19, @9500 : Datron 9500 (C5,G3,B), Datron 9510 { " +
                                asset9500B[0] + " } [1]";
                            break;
                        }
                        else if (results_[index9500BSelect].Contains("9520"))
                        {
                            line9500B = "19, @9500 : Datron 9500 (C5,G3,B), Datron 9520 { " +
                                asset9500B[0] + " } [1]";
                            break;
                        }
                        else if (results_[index9500BSelect].Contains("9530"))
                        {
                            line9500B = "19, @9500 : Datron 9500 (C5,G3,B), Datron 9530 { " +
                                asset9500B[0] + " } [1]";
                            break;
                        }
                        else if (results_[index9500BSelect].Contains("9550"))
                        {
                            line9500B = "19, @9500 : Datron 9500 (C5,G3,B), Datron 9550 { " +
                                asset9500B[0] + " } [1]";
                            break;
                        }
                        else if (results_[index9500BSelect].Contains("9560"))
                        {
                            line9500B = "19, @9500 : Datron 9500 (C5,G3,B), Fluke 9560 { " +
                                asset9500B[0] + " } [1]";
                            break;
                        }
                    }
                    break;
            }            
            results[loadConfigIndex] = line9500B;
            fh.clearText(metcalConfigPath);
            fh.textWriterAll(metcalConfigPath, results);
        }

        //Add Asset Number Column
        public void addAssetNumberColumn()
        {
            assetNumbers1.Visible = true;
            dataGridView1.Width = 515;
            assetNumbers2.Visible = true;
            dataGridView2.Width = 515;

        }
        //Remove asset Number Column
        public void removeAssetNumberColumn()
        {
            assetNumbers1.Visible = false;
            dataGridView1.Width = 415;
            assetNumbers2.Visible = false;
            dataGridView2.Width = 415;
        }       

        //Parse Manual Standards
        public string[] parseManualStandards()
        {
            int colon = 58;
            char index = ' ';
            int count = 0;
            string newLine = "";
            string[] manualStds;
            string[] results = fh.textReaderAll(metcalConfigPath);
            string[] temp = new string[results.Length];
            foreach (string line in results)
            {
                if(results[count] != "" && results[count] != null && results[count] != " ")
                {
                    index = line[0];
                }
                if (index == colon)
                {
                    foreach(char a in line)
                    {
                        if (a != colon)
                        {
                            newLine += a;
                        }
                    }
                    temp[count] = newLine;
                }
                count++;
                newLine = "";
            }
            count = 0;
            foreach (string line in temp)
            {
                if(line != null)
                {                    
                    count++;
                }
            }
            manualStds = new string[count];
            count = 0;
            foreach (string line in temp)
            {
                if (line != null)
                {
                    newLine = line.Trim();
                    manualStds[count] = newLine;
                    count++;
                }
            }           
            return manualStds;
        }        

        //Add asset number to row if needed
        public void addAssetNumberToRows()
        {
            int index = 0;
            foreach (string line in assetList)
            {
                if(line != null && !line.Equals(""))
                {
                    dataGridView1.Rows[index].Cells[3].Value = line;
                    dataGridView2.Rows[index].Cells[3].Value = line;
                }
                index++;
            }
        }

        //Clear assets numbers
        public string clearAssetNumbers(string temp)
        {
            string redoAsset = "";
            char index;
            char space = ' ';
            for (int i = 0; i < temp.Length; i++)
                {
                    index = temp[i];
                    if (index != 123)
                    {
                        redoAsset += index;
                    }
                    if (index == 123)
                    {
                        break;
                    }
                }
                redoAsset = redoAsset.Trim(space);
          return redoAsset;            
        }

        //Start Splash Screen
        public void startSplashScreen()
        {
            Application.Run(new SplashScreen());
        }        

        //Set last flexible standard used
        public void setCurrentFlex(string dc, string pm, string lo)
        {
            if(DC)
            {
                if (dc == "5730A")
                {
                    RB5730.Checked = false;
                    RB5730.Checked = true;
                    DCCalGroupBox.Enabled = true;
                    file5730Update.Text = "Currently Selected";
                }
                if (dc == "5720A")
                {
                    RB5720.Checked = false;
                    RB5720.Checked = true;
                    DCCalGroupBox.Enabled = true;
                    file5720Update.Text = "Currently Selected";
                }
                if (dc == "5522A")
                {
                    RB5522.Checked = false;
                    RB5522.Checked = true;
                    DCCalGroupBox.Enabled = true;
                    file5522Update.Text = "Currently Selected";
                }
            }
            else
            {
                DCCalGroupBox.Enabled = false;
                RB5522.Checked = false;
                RB5720.Checked = false;
                RB5730.Checked = false;
                file5720Update.Text = "";
                file5730Update.Text = "";
                file5522Update.Text = "";
            }
            if(PWRMTR)
            {
                if (pm == "E4418x")
                {
                    RB4418x.Checked = false;
                    RB4418x.Checked = true;
                    PMGroupBox.Enabled = true;
                    file4418xUpdate.Text = "Currently Selected";
                }
                if (pm == "E4419x")
                {
                    RB4419x.Checked = false;
                    RB4419x.Checked = true;
                    PMGroupBox.Enabled = true;
                    file4419xUpdate.Text = "Currently Selected";
                }
                if (pm == "N1912A")
                {
                    RB1912A.Checked = false;
                    RB1912A.Checked = true;
                    PMGroupBox.Enabled = true;
                    file1912Aupdate.Text = "Currently Selected";
                }
            }
            else
            {
                PMGroupBox.Enabled = false;
                RB4418x.Checked = false;
                RB4419x.Checked = false;
                RB1912A.Checked = false;
                file4418xUpdate.Text = "";
                file4419xUpdate.Text = "";
                file1912Aupdate.Text = "";
            }
            if (LO)
            {
                if (lo == "E8247D")
                {
                    RB8247.Checked = false;
                    RB8247.Checked = true;
                    sigGenGroupBox.Enabled = true;
                    file8247update.Text = "Currently Selected";
                }
                if (lo == "E8257C")
                {
                    RB8257.Checked = false;
                    RB8257.Checked = true;
                    sigGenGroupBox.Enabled = true;
                    file8257update.Text = "Currently Selected";
                }
                if (lo == "N518xx")
                {
                    RB518xx.Checked = false;
                    RB518xx.Checked = true;
                    sigGenGroupBox.Enabled = true;
                    file518xxUpdate.Text = "Currently Selected";
                }
                if (lo == "E4438C")
                {
                    RB4438.Checked = false;
                    RB4438.Checked = true;
                    sigGenGroupBox.Enabled = true;
                    file4438update.Text = "Currently Selected";
                }
            }
            else
            {
                sigGenGroupBox.Enabled = false;
                RB8247.Checked = false;
                RB8257.Checked = false;
                RB518xx.Checked = false;
                file5720Update.Text = "";
                file5730Update.Text = "";
                file5522Update.Text = "";
            }

        }

        //Adding 9500B head validation
        public bool head9500Validation(string asset)
        {
            bool valid;
            int assetLen = asset.Length;
            if(assetLen == 6 || assetLen == 7 || assetLen == 9)
            {
                valid = true;
            }
            else
            {
                valid = false;
            }
            if (asset.Equals(""))
            {
                valid = true;
            }
            return valid;
        }

        //Build Input box with combo box and text box for manual standard asset number entry
        public DialogResult buildManualSelectInputBox(string[] manualStds)
        {
            
            Form form = new Form();
            Label titleLabel = new Label();
            Label standardLabel = new Label();
            Label assetLabel = new Label();
            ComboBox standardCombo = new ComboBox();
            TextBox assetTextBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = "Manual Standard Asset Number";
            titleLabel.Text = "Enter Manual Standard Asset Number";
            standardLabel.Text = "Standard:";
            assetLabel.Text = "Asset Number:";
            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            for (int i = 0; i < manualStds.Length; i++)
            {
                if (manualStds[i] != null || manualStds[i] != "") ;
                standardCombo.Items.Add(manualStds[i]);
            }

            titleLabel.SetBounds(10, 20, 375, 15);
            standardLabel.SetBounds(20, 55, 75, 20);
            standardCombo.SetBounds(95, 50, 300, 20);
            assetLabel.SetBounds(20, 80, 75, 20);
            assetTextBox.SetBounds(95, 75, 300, 20);
            buttonOk.SetBounds(230, 115, 75, 25);
            buttonCancel.SetBounds(310, 115, 75, 25);

            titleLabel.AutoSize = true;
            standardLabel.AutoSize = true;
            assetLabel.AutoSize = true;
            standardCombo.Anchor = standardCombo.Anchor | AnchorStyles.Right;
            assetTextBox.Anchor = assetTextBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(400, 150);
            form.Controls.AddRange(new Control[] { titleLabel, standardLabel, assetLabel, standardCombo, assetTextBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, titleLabel.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            manualAsset = assetTextBox.Text;
            standardSelect = standardCombo.SelectedIndex;
            return dialogResult;
        }

        //Get metcal.ini data for proc spliting and serial port
        public void getIniData()
        {
            string[] ini = fh.textReaderAll(metcalIni);
            char a;
            int len;
            int equals = 61;
            section = "Disabled";
            port = "None";
            foreach (string line in ini)
            {
                if (line.Contains("tag"))
                {
                    for (int i = 0; i < line.Length; i++)
                    {
                        a = line[i];
                        if (a == equals)
                        {
                            len = line.Length - (i + 1);
                            section = line.Substring(i + 1, len);
                            break;
                        }
                    }
                }
                if (line.Contains("port") && (line.Contains("COM") || line.Contains("NONE")))
                {
                    for (int i = 0; i < line.Length; i++)
                    {

                        a = line[i];
                        if (a == equals)
                        {
                            len = line.Length - (i + 1);
                            port = line.Substring(i + 1, len);
                            break;
                        }
                    }
                }
            }
            if (section.Contains("no"))
                procSplitView.Text = "Disabled";
            else if (section.Contains("yes"))
                procSplitView.Text = "Enabled";
            else
                procSplitView.Text = "Disabled";
            portView.Text = port;
        }

        //Clear all asset numbers from column
        public void clearAssetNumbers()
        {
            int rowCount = dataGridView1.Rows.Count;
            modelResults = fh.textReaderAll(metcalConfigPath);

            for (int i = 0; i < modelResults.Length; i++)
            {
                modelResults[i] = clearAssetNumbers(modelResults[i]);
            }
            fh.clearText(metcalConfigPath);
            fh.textWriterAll(metcalConfigPath, modelResults);
            for (int i = 0; i < rowCount; i++)
            {
                dataGridView1.Rows[i].Cells[3].Value = "";
                dataGridView2.Rows[i].Cells[3].Value = "";
            }
            assetList.Clear();
            assetList.Capacity = modelList.Count;
            for (int i = 0; i < assetList.Capacity; i++)
            {
                assetList.Add("");
            }
        }        
    }     
}


