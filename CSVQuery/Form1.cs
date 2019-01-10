using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSVDataManipulation;
using System.IO;

namespace CSVQuery
{
    public partial class Form1 : Form
    {
        CSV data;

        public Form1()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog() { DefaultExt = "csv", Title = "Open CSV", Multiselect = false };
            if(openFile.ShowDialog() == DialogResult.OK)
            {
                data = new CSV(new FileStream(openFile.FileName, FileMode.Open));
                fileNameLabel.Text = openFile.FileName;
                columnSelect.DataSource = data.AllKeys;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CSV output = new CSV();

            foreach(Dictionary<string, string> row in data.Data)
            {
                if(row[((String)columnSelect.SelectedItem)].Contains(valueInput.Text))
                {
                    output.Add(row);
                }
            }

            SaveFileDialog save = new SaveFileDialog() { DefaultExt = "csv", AddExtension = true };
            if (save.ShowDialog() == DialogResult.OK)
            {
                output.Save(save.OpenFile());
            }
        }
    }
}
