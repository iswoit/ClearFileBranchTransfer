using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
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


        private void LVUpdate()
        {
            lvList.BeginUpdate();
            // 进度列表
            try
            {
                for (int i = 0; i < lvList.Items.Count; i++)
                {
                    ClearFile clearFile = (ClearFile)lvList.Items[i].Tag;   // 配置对象
                    lvList.Items[i].SubItems[5].Text = clearFile.Procedure;
                    lvList.Items[i].SubItems[6].Text = clearFile.IsOK ? "√" : "×";              // 标志到齐
                    lvList.Items[i].SubItems[7].Text = clearFile.Status.ToString();             // 状态


                    if (clearFile.IsRunning)
                    {
                        lvList.Items[i].BackColor = Color.LightBlue;
                        lvList.Items[i].EnsureVisible();
                    }
                    else
                    {
                        if (clearFile.IsOK)
                            lvList.Items[i].BackColor = SystemColors.Window;
                        else
                            lvList.Items[i].BackColor = Color.Pink;
                    }
                }

                if (Manager.GetInstance().IsAllOK)
                {
                    lbIsAllOK.Text = "是";
                    lbIsAllOK.ForeColor = Color.Green;
                }
                else
                {
                    lbIsAllOK.Text = "否";
                    lbIsAllOK.ForeColor = Color.Red;
                }
            }
            catch (Exception)
            {
                // ui异常过滤
            }

            lvList.EndUpdate();
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            if (!bgWorker.IsBusy)
            {
                btnExecute.Text = "执行中...";
                bgWorker.RunWorkerAsync();
                Manager.GetInstance().IsRunning = true;
                lbStatus.Text = "正在运行...";
            }
            else
            {
                btnExecute.Text = "执行";
                bgWorker.CancelAsync();
            }
        }

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgWorker = sender as BackgroundWorker;
            UserState us;

            // 遍历每个ClearFile
            foreach (ClearFile clearFile in Manager.GetInstance().ClearFileColl)
            {
                try
                {
                    // 1.重置状态
                    clearFile.ResetStatus();
                    clearFile.IsRunning = true;
                    clearFile.Status = "运行中...";
                    bgWorker.ReportProgress(1);
                    Thread.Sleep(50);

                    // 2.文件是否存在
                    try
                    {
                        if (!File.Exists(clearFile.FilePath))
                        {
                            clearFile.IsRunning = false;
                            clearFile.Status = "文件不存在";
                            us = new UserState(true, string.Format(@"文件[{0}]文件不存在", clearFile.FilePath));
                            bgWorker.ReportProgress(1, us);
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        clearFile.IsRunning = false;
                        clearFile.Status = "异常";
                        us = new UserState(true, string.Format(@"文件[{0}]处理异常: {1}", clearFile.FilePath, ex.Message));
                        bgWorker.ReportProgress(1, us);
                        continue;
                    }

                    // 3.遍历每个股东号进行修改
                    // 进行update操作
                    int updateCnt = 0;
                    string connectString = @"Provider=VFPOLEDB.1;Data Source=D:\;Collating Sequence=MACHINE";    // 连接串
                    try
                    {
                        using (OleDbConnection connection = new OleDbConnection(connectString))
                        {
                            connection.Open();
                            using (OleDbCommand command = new OleDbCommand())
                            {
                                command.Connection = connection;
                                command.CommandType = CommandType.Text;

                                List<string> tmpList = new List<string>(clearFile.AccList.Keys);
                                for (int i = 0; i < tmpList.Count; i++)
                                {
                                    command.CommandText = string.Format(@"update {0} set {1}=substr({1},1,{8})+'{2}'+substr({1},{9})  WHERE substr({1},{3},{4})='{5}' and {6}='{7}'",
                                                                            clearFile.FilePath,        // 文件
                                                                            clearFile.ContractCol,     // 合同列
                                                                            clearFile.NewPrefix,       // 新合同号
                                                                            clearFile.ContractStart,    // 合同号起始
                                                                            clearFile.ContractLength,  // 合同号长度
                                                                            clearFile.OldPrefix,         // 旧合同前缀
                                                                            clearFile.AccountCol,       // 股东代码列
                                                                            tmpList[i],                  // 股东号
                                                                            clearFile.ContractStart - 1,    // 前缀长度
                                                                            clearFile.ContractStart + clearFile.ContractLength   // 后缀开始
                                                                        );

                                    int iUpdateCnt = command.ExecuteNonQuery();
                                    updateCnt += iUpdateCnt;
                                    clearFile.AccList[tmpList[i]] = true;
                                    bgWorker.ReportProgress(1);
                                }
                            }
                        }

                        us = new UserState(true, string.Format(@"文件[{0}]共更新了[{1}]行.", clearFile.FilePath, updateCnt));
                        bgWorker.ReportProgress(1, us);
                    }
                    catch (Exception ex)
                    {
                        clearFile.IsRunning = false;
                        clearFile.Status = "异常";
                        us = new UserState(true, string.Format(@"文件[{0}]处理异常: {1}", clearFile.FilePath, ex.Message));
                        bgWorker.ReportProgress(1, us);
                        continue;
                    }


                    // 4.结束
                    clearFile.IsRunning = false;
                    clearFile.Status = "完成";
                    bgWorker.ReportProgress(1);

                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }//eof foreach
        }

        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // 如果有错误日志，输出
            if (e.UserState != null)
            {
                UserState us = (UserState)e.UserState;
                if (us.HasError)
                    Print_Message(us.ErrorMsg);
            }

            // 更新listView
            try
            {
                LVUpdate();
            }
            catch (Exception ex)
            {
                Print_Message(ex.Message);
            }
        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)    // 未处理的异常，需要弹框
            {
                Print_Message(e.Error.Message);
            }
            else if (e.Cancelled)
            {
                Print_Message("任务被手工取消");

            }
            else
            {
                Print_Message("执行完毕");
            }

            btnExecute.Text = "执行";
            lbStatus.Text = "运行完毕";
            Manager.GetInstance().IsRunning = false;

            LVUpdate();
        }


        private void Print_Message(string message)
        {
            tbLog.Text = string.Format("{0}:{1}", DateTime.Now.ToString("HH:mm:ss"), message) + System.Environment.NewLine + tbLog.Text;
        }
    }
}
