using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ClearFileBranchTransfer
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();

            try
            {
                Manager.GetInstance();
                //dateLabel.Text = Manager.GetInstance().DTNow.ToString("yyyy-MM-dd");

                // 初始化UI
                LVInit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Environment.Exit(0);
            }
        }


        private void LVInit()
        {
            lvList.Items.Clear();
            lvList.BeginUpdate();
            int i = 0;
            foreach (ClearFile clearFile in Manager.GetInstance().ClearFileColl)
            {
                ListViewItem lvi = new ListViewItem((++i).ToString());
                lvi.SubItems.Add(clearFile.FilePath);
                lvi.SubItems.Add(clearFile.Market);
                lvi.SubItems.Add(clearFile.OldPrefix);
                lvi.SubItems.Add(clearFile.NewPrefix);
                lvi.SubItems.Add(clearFile.Procedure);
                lvi.SubItems.Add(clearFile.IsOK ? "√" : "×");
                lvi.SubItems.Add(clearFile.Status);

                lvi.Tag = clearFile;

                lvList.Items.Add(lvi);


                // 颜色
                if (clearFile.IsOK)
                    lvi.BackColor = SystemColors.Window;
                else
                    lvi.BackColor = Color.Pink;
            }

            lvList.Columns[0].Width = -1;
            lvList.Columns[1].Width = -1;
            //productListView.Columns[1].Width = -1;
            //productListView.Columns[2].Width = -1;
            //productListView.Columns[3].Width = -1;

            lvList.EndUpdate();
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {

        }
    }
}
