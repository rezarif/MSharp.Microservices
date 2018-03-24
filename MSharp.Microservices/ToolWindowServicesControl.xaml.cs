namespace MSharp.Microservices
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Json;
    using System.Windows;
    using System.Windows.Controls;
    using System.Data;
    using System.Linq;
    using System.Runtime.InteropServices;
    using EnvDTE;
    using System;
    using System.Runtime.InteropServices.ComTypes;

    /// <summary>
    /// Interaction logic for ToolWindowServicesControl.
    /// </summary>
    public partial class ToolWindowServicesControl : UserControl
    {
        public ToolWindowServicesControl()
        {
            this.InitializeComponent();
        }

        private string applicationUrl(FileInfo solutionFile)
        {
            string result = "";
            var strFilePath = solutionFile.Directory.FullName + @"\Website\Properties\launchsettings.json";
            if (File.Exists(strFilePath))
            {
                //FileInfo launchSettingFile = new FileInfo(strFilePath);
                string text = File.ReadAllText(strFilePath);
                JsonValue value = JsonValue.Parse(text);
                result = value["profiles"]["Website"]["applicationUrl"].ToString().Replace('"', ' ').Trim();
            }

            return result;
        }

        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);

        public static DTE GetDTE(int processId)
        {
            string progId = "!VisualStudio.DTE.15.0:" + processId.ToString();
            object runningObject = null;

            IBindCtx bindCtx = null;
            IRunningObjectTable rot = null;
            IEnumMoniker enumMonikers = null;

            try
            {
                Marshal.ThrowExceptionForHR(CreateBindCtx(reserved: 0, ppbc: out bindCtx));
                bindCtx.GetRunningObjectTable(out rot);
                rot.EnumRunning(out enumMonikers);

                IMoniker[] moniker = new IMoniker[1];
                IntPtr numberFetched = IntPtr.Zero;
                while (enumMonikers.Next(1, moniker, numberFetched) == 0)
                {
                    IMoniker runningObjectMoniker = moniker[0];

                    string name = null;

                    try
                    {
                        if (runningObjectMoniker != null)
                        {
                            runningObjectMoniker.GetDisplayName(bindCtx, null, out name);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Do nothing, there is something in the ROT that we do not have access to.
                    }

                    if (!string.IsNullOrEmpty(name) && string.Equals(name, progId, StringComparison.Ordinal))
                    {
                        Marshal.ThrowExceptionForHR(rot.GetObject(runningObjectMoniker, out runningObject));
                        break;
                    }
                }
            }
            finally
            {
                if (enumMonikers != null)
                {
                    Marshal.ReleaseComObject(enumMonikers);
                }

                if (rot != null)
                {
                    Marshal.ReleaseComObject(rot);
                }

                if (bindCtx != null)
                {
                    Marshal.ReleaseComObject(bindCtx);
                }
            }

            return (DTE)runningObject;
        }
        private DTE SolutionDte(string solutionPath)
        {
            DTE resultDte = null;

            System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcessesByName("devenv");
            foreach (System.Diagnostics.Process p in processes)
            {
                var foundDte = GetDTE(p.Id);
                if (foundDte != null)
                {
                    if (foundDte.Solution.FullName == solutionPath)
                    {
                        resultDte = foundDte;
                    }
                }

            }

            if (resultDte == null)
            {
                Type typeDTE = Type.GetTypeFromProgID("VisualStudio.DTE.15.0");
                resultDte = (DTE)Activator.CreateInstance(typeDTE, true);
                resultDte.UserControl = true;
                resultDte.Solution.Open(solutionPath);
                //resultDte.MainWindow.Activate();
                //Marshal.ReleaseComObject(resultDte);
            }
            return resultDte;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        private void button1_Click(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".json";
            dlg.Filter = "Services Files (Services.json)|*.json";

            var msg = "";
            string filename = "empty";
            // Get the selected file name and display in a TextBox 
            if (dlg.ShowDialog() == true)
            {
                filename = dlg.FileName;
                FileInfo fInf = new FileInfo(filename);
                var parentDir = fInf.Directory.Parent;

                string text = File.ReadAllText(filename);
                JsonValue value = JsonValue.Parse(text);
                var solution = value[0]["Solution"]["FullName"].ToString();
                txtSolutionName.Text = "Solution: " + solution.Replace('"', ' ');
                var services = value[0]["Services"];

                var arrServices = JsonArray.Parse(services.ToString());
                DataSet1.tblServiceDataTable tblServices = new DataSet1.tblServiceDataTable();
                foreach (var srvc in arrServices)
                {
                    DataRow dr = tblServices.NewRow();
                    var srvcPair = (System.Collections.Generic.KeyValuePair<string, JsonValue>)srvc;
                    var ServiceName = srvcPair.Key;
                    var ServiceValue = srvcPair.Value;
                    dr["service"] = ServiceName;
                    JsonValue srvJValue = JsonValue.Parse(ServiceValue.ToString());
                    dr["open_live"] = srvJValue["LiveUrl"].ToString().Replace('"', ' ');
                    dr["open_uat"] = srvJValue["UatUrl"].ToString().Replace('"', ' ');
                    var solutionFile = parentDir.GetFiles(ServiceName + ".sln", SearchOption.AllDirectories).FirstOrDefault<FileInfo>();
                    if (solutionFile != null)
                    {
                        dr["solution_path"] = solutionFile.FullName;
                        var ApplicationUrl = applicationUrl(solutionFile);
                        dr["application_url"] = ApplicationUrl;
                        dr["port"] = ApplicationUrl.Substring(ApplicationUrl.LastIndexOf(":") + 1);
                    }

                    tblServices.Rows.Add(dr);
                }
                gridSerives.DataContext = tblServices.DefaultView;
            }
            //MessageBox.Show(msg);
        }

        private void HandleCheck(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Button is Checked");
        }

        private void HandleUnchecked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Button is unchecked.");
        }

        private void open_click(object sender, RoutedEventArgs e)
        {
            MenuItem mnuItem = (MenuItem)sender;
            if (mnuItem.CommandParameter != null)
            {
                string url = mnuItem.CommandParameter.ToString().Trim();
                if (url.Length != 0)
                {
                    System.Diagnostics.Process.Start(url);
                }
            }

            MessageBox.Show("Open menu is clicked. " + mnuItem.CommandParameter);
        }

        private void code_click(object sender, RoutedEventArgs e)
        {
            Button mnuItem = (Button)sender;
            if (mnuItem.CommandParameter != null)
            {
                string solutionPath = mnuItem.CommandParameter.ToString().Trim();
                if (solutionPath.Length != 0)
                {
                    var resultDte = SolutionDte(solutionPath);

                    //DTE resultDte = null;

                    //System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcessesByName("devenv");
                    //foreach (System.Diagnostics.Process p in processes)
                    //{
                    //    var foundDte = GetDTE(p.Id);
                    //    if (foundDte.Solution.FullName == solutionPath)
                    //    {
                    //        resultDte = foundDte;
                    //        //resultDte.MainWindow.Activate();
                    //    }
                    //}

                    //if (resultDte == null)
                    //{
                    //    Type typeDTE = Type.GetTypeFromProgID("VisualStudio.DTE.15.0");
                    //    resultDte = (DTE)Activator.CreateInstance(typeDTE, true);
                    //    resultDte.UserControl = true;
                    //    resultDte.Solution.Open(solutionPath);
                    //    //resultDte.MainWindow.Activate();
                    //    //Marshal.ReleaseComObject(resultDte);
                    //}

                    if (resultDte != null)
                    {
                        resultDte.MainWindow.Activate();

                    }
                }
            }
        }

    }
}