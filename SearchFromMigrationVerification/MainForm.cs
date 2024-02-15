using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace SearchFromReport
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var openFileDialog1 = new OpenFileDialog
            {
                Title = @"Browse Report File",
                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "csv",
                Filter = @"CSV Files (*.csv)|*.csv",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var openFileDialog1 = new OpenFileDialog
            {
                Title = @"Browse csv File",
                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "csv",
                Filter = @"CSV Files (*.csv)|*.csv",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = openFileDialog1.FileName;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();
        }

        private static List<string> FindIdInHtml(List<string> idList, ICollection<string> idFromFileFromMigrationVerification)
        {
            var listOfItemId = new List<string>();
            foreach (var item in idList)
            {
                var foundItem = idFromFileFromMigrationVerification.Contains(item);

                if (!foundItem)
                {
                    listOfItemId.Add(item);
                }
            }

            return listOfItemId;
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var idFilePath = textBox1.Text;
            var migrationVerificationFile = textBox2.Text;

            if (!File.Exists(migrationVerificationFile) || !File.Exists(idFilePath) || Path.GetExtension(idFilePath).ToLower() != ".csv" || Path.GetExtension(migrationVerificationFile).ToLower() != ".csv")
                return;
            var idFromFile = new List<string>();
            var idFromFileFromMigrationVerification = new List<string>();
            using (var reader = new StreamReader(idFilePath))
            {
                while (!reader.EndOfStream)
                {
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');
                        var s = values.FirstOrDefault();
                        idFromFile.Add(s);
                    }
                }
            }

            using (var reader = new StreamReader(migrationVerificationFile))
            {
                while (!reader.EndOfStream)
                {
                    {
                        try
                        {
                            var line = reader.ReadLine();
                            var values = line.Split('\"');
                            var s = values.FirstOrDefault();
                            var type = values[13];
                            if (type == "0")
                                continue;
                            var itemId = values[19];
                            idFromFileFromMigrationVerification.Add(itemId);
                        }
                        catch (Exception)
                        {
                            //throw;
                        }
                    }
                }
            }

            var failedItems = FindIdInHtml(idFromFile, idFromFileFromMigrationVerification);
            var fileName = Path.GetFileNameWithoutExtension(migrationVerificationFile);
            string newFileWithExtension = fileName + " FailedItems.csv";
            var stringValue = string.Join(Environment.NewLine, failedItems.ToArray());

            using (var sw = new StreamWriter(newFileWithExtension))
            {
                sw.WriteLine(stringValue);
            }

            MessageBox.Show(@"Created csv file for the failed items.", this.Name, MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            //cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Close();
        }
    }
}