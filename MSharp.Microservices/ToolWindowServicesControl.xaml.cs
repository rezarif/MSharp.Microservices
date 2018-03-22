namespace MSharp.Microservices
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Json;
    using System.Windows;
    using System.Windows.Controls;
    using System.Data;
    using System.Linq;

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
                txtSolutionName.Text = "Solution: " + solution.Replace('"',' ');
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
                        dr["port"] = ApplicationUrl.Substring(ApplicationUrl.LastIndexOf(":")+1);
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
            var msg = "Is Nothing";

            Button mnuItem = (Button)sender;
            if (mnuItem.CommandParameter != null)
            {
                string solutionPath = mnuItem.CommandParameter.ToString().Trim();
                if (solutionPath.Length != 0)
                {
                    msg = solutionPath;
                    //MessageBox.Show(solutionPath);
                }
            }

            MessageBox.Show(msg);
        }

    }
}