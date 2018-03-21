namespace MSharp.Microservices
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Json;
    using System.Windows;
    using System.Windows.Controls;
    using System.Data;

    /// <summary>
    /// Interaction logic for ToolWindowServicesControl.
    /// </summary>
    public partial class ToolWindowServicesControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToolWindowServicesControl"/> class.
        /// </summary>
        public ToolWindowServicesControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
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
                // Open document 
                filename = dlg.FileName;
                string text = File.ReadAllText(filename);
                JsonValue value = JsonValue.Parse(text);
                var solution = value[0]["Solution"]["FullName"].ToString();
                txtSolutionName.Text = "Solution: " + solution.Replace('"',' ');
                var services = value[0]["Services"];

                var a = JsonArray.Parse(services.ToString());
                DataSet1.tblServiceDataTable tblServices = new DataSet1.tblServiceDataTable();
                foreach (var item in a)
                {
                    DataRow dr = tblServices.NewRow();

                    var pair = (System.Collections.Generic.KeyValuePair<string, JsonValue>)item;
                    var k = pair.Key;
                    var v = pair.Value;
                    dr["service"] = k;
                    JsonValue srvValue = JsonValue.Parse(v.ToString());
                    dr["open_live"] = srvValue["LiveUrl"].ToString().Replace('"', ' ');
                    dr["open_uat"] = srvValue["UatUrl"].ToString().Replace('"', ' ');


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
    }
}