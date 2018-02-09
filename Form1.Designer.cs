using CaseLocator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace CaseDownloader
{
  public class frmCaseDownloader : Form
  {
    private IContainer components = (IContainer) null;
    private WindowsFormsSynchronizationContext mUiContext;
    private Button btnDownload;
    private TextBox txtRefNum;
    private Label label2;
    private TextBox txtUserName;
    private Label label1;
    private Label label3;
    private TextBox txtPassword;
    private NumericUpDown numThreads;
    private Label label4;
    private Label lblStart;
    private Label lblFinish;
    private Label lblCompleted;
    private Label lblTotal;
    private Label label5;
    private Label label6;
    private Label lblempCaseCount;
    private Label label7;
    private Label lblErrorCase;
    public DataGridView grdCases;
    public TextBox txtConsole;
    private DataGridViewTextBoxColumn colRefNum;
    private DataGridViewTextBoxColumn colCasesComp;
    private DataGridViewTextBoxColumn colCaseCount;
    private DataGridViewTextBoxColumn colDocs;
    private DataGridViewTextBoxColumn linkDocs;
    private DataGridViewTextBoxColumn empDocs;
    private DataGridViewTextBoxColumn totDocs;
    private DataGridViewTextBoxColumn allDocs;
    private DataGridViewTextBoxColumn colStatus;
    private CheckBox chkResume;
    private List<string> noload_log = new List<string>();
    private List<string> empty_log = new List<string>();

    public frmCaseDownloader()
    {
      this.InitializeComponent();
      frmCaseDownloader.CheckForIllegalCrossThreadCalls = false;
    }

    private void btnDownload_Click(object sender, EventArgs e)
    {
      this.mUiContext = new WindowsFormsSynchronizationContext();
      if (this.txtRefNum.Text == "")
      {
        int num1 = (int) MessageBox.Show("Enter Case Number");
      }
      else if (this.txtUserName.Text == "")
      {
        int num2 = (int) MessageBox.Show("Enter User Name");
      }
      else if (this.txtPassword.Text == "")
      {
        int num3 = (int) MessageBox.Show("Enter Password");
      }
      else
      {
        FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
        string path;
        if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
          path = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\cases\\";
        else  path = Path.GetFullPath(folderBrowserDialog.SelectedPath);
        this.btnDownload.Enabled = false;
        this.txtPassword.Enabled = false;
        this.txtUserName.Enabled = false;
        this.txtRefNum.Enabled = false;
        this.numThreads.Enabled = false;
        this.txtConsole.Visible = true;
        this.chkResume.Enabled = false;
        List<string> lstRefNums = new List<string>();
        string upper = this.txtRefNum.Text.ToUpper();
        if (upper.Contains(","))
          lstRefNums = ((IEnumerable<string>) upper.Split(',')).ToList<string>();
        else if (upper.Contains("-"))
        {
          List<int> intList = new List<int>();
          lstRefNums = ((IEnumerable<string>) upper.Split('-')).ToList<string>();
          string[] strArray1 = new string[0];
          for (int index = 0; index < lstRefNums.Count; ++index)
          {
            string str1 = lstRefNums[index];
            char[] chArray1 = new char[1]{ 'C' };
            foreach (string str2 in str1.Split(chArray1))
            {
              char[] chArray2 = new char[1]{ ' ' };
              string[] strArray2 = str2.Split(chArray2);
              if (strArray2[0] != "")
                intList.Add(Convert.ToInt32(strArray2[0]));
            }
          }
          for (int index = intList[0]; index <= intList[1]; ++index)
            lstRefNums.Add("C" + (object) index);
        }
        else
          lstRefNums.Add(upper);
        lstRefNums = lstRefNums.Distinct<string>().ToList<string>();
        lstRefNums = lstRefNums.OrderBy<string, string>((Func<string, string>) (x => x)).ToList<string>();
        if (lstRefNums.Count > 100000)
        {
          int num4 = (int) MessageBox.Show("You are trying to download " + lstRefNums.Count.ToString() + " cases.  Please select 100000 or less.");
        }
        else
        {
          this.lblempCaseCount.Text = "0";
          this.lblErrorCase.Text = "0";        
          this.lblCompleted.Text = "0";
          this.lblTotal.Text = "/  " + lstRefNums.Count.ToString();
          Thread.Sleep(100);
          this.grdCases.Visible = true;
          this.grdCases.Rows.Clear();
          foreach (string str in lstRefNums)
            this.grdCases.Rows.Add(new object[1]
            {
              (object) str
            });
          this.grdCases.Refresh();
          DateTime start = DateTime.Now;
          TimeSpan timeSpan1 = new TimeSpan();
          this.lblStart.Text = "Started at: " + start.ToShortTimeString();
          this.lblFinish.Text = "";
          this.txtConsole.Text = "Starting with " + this.numThreads.Value.ToString() + " threads.  Need to download " + lstRefNums.Count.ToString() + " cross reference numbers. (" + start.ToShortTimeString() + ")";
          //string path;
          Task.Factory.StartNew<ParallelLoopResult>((Func<ParallelLoopResult>) (() =>
          {
            List<string> stringList = lstRefNums;
            ParallelOptions parallelOptions = new ParallelOptions();
            parallelOptions.MaxDegreeOfParallelism = Convert.ToInt32(this.numThreads.Value);
            Action<string> body =  ((Action<string>) (refNum =>
            {
              DataGridViewRow dataGridViewRow = this.grdCases.Rows.Cast<DataGridViewRow>().Where<DataGridViewRow>((Func<DataGridViewRow, bool>) (r => r.Cells[0].Value.ToString().Equals(refNum))).First<DataGridViewRow>();
              TimeSpan timeSpan = new TimeSpan();
              string[] str1 = { (string)null, (string)null };
              while (str1[0] != "0")
              {
                 WindowsFormsSynchronizationContext mUiContext = this.mUiContext;
                SendOrPostCallback d = new SendOrPostCallback(this.UpdateGUIConsole);
                string str2 = "Processing ";
                string str3 = refNum.ToString();
                string str4 = " on thread ";
                int managedThreadId = Thread.CurrentThread.ManagedThreadId;
                string str5 = managedThreadId.ToString();
                string str6 = str2 + str3 + str4 + str5;
                mUiContext.Post(d, (object) str6);
                if (dataGridViewRow.Cells[8].Value == null)
                {
                  DataGridViewCell cell = dataGridViewRow.Cells[8];
                  string str7 = "Processing on thread ";
                  managedThreadId = Thread.CurrentThread.ManagedThreadId;
                  string str8 = managedThreadId.ToString();
                  string str9 = str7 + str8;
                  cell.Value = (object) str9;
                }
                else
                {
                  DataGridViewCell cell = dataGridViewRow.Cells[8];
                  string str7 = "Re-Processing on thread ";
                  managedThreadId = Thread.CurrentThread.ManagedThreadId;
                  string str8 = managedThreadId.ToString();
                  string str9 = str7 + str8;
                  cell.Value = (object) str9;
                }
                this.mUiContext.Post(new SendOrPostCallback(this.UpdateGUI), (object) null);
                DateTime now = DateTime.Now;
                Locate locate = new Locate();
                locate.userName = this.txtUserName.Text;
                locate.password = this.txtPassword.Text;
                locate.UserDefinedPath = path;
                str1 = str1[0] != null ? locate.LocateCase(refNum, this.grdCases, this.txtConsole, !this.chkResume.Checked) : locate.LocateCase(refNum, this.grdCases, this.txtConsole, !this.chkResume.Checked);
                timeSpan = DateTime.Now - now;
                if (str1[0] != "0")
                {
                  dataGridViewRow.Cells[8].Value = (object) "Error Processing";
                  this.mUiContext.Post(new SendOrPostCallback(this.UpdateGUI), (object) null);
                  this.mUiContext.Post(new SendOrPostCallback(this.UpdateGUIConsole), (object) ("Error Processing " + refNum.ToString() + ":"));
                  this.mUiContext.Post(new SendOrPostCallback(this.UpdateGUIConsole), (object) str1);
                }
              }
                //if (str1[1] == "Cannot load this page") { this.lblErrorCase.Text = Convert.ToString((int)Convert.ToInt16(this.lblErrorCase.Text) + 1); }
                this.Updatestate(str1[1],refNum);
                Thread.Sleep(100);
              dataGridViewRow.Cells[8].Value = (object) (str1[1] +" in "+ (object) timeSpan.Minutes + " minutes " + (object) timeSpan.Seconds + " seconds");
              this.mUiContext.Post(new SendOrPostCallback(this.UpdateGUIConsole), (object) (str1[1] +" "+ refNum + " in " + (object) timeSpan.Minutes + " minutes " + (object) timeSpan.Seconds + " seconds"));
              this.mUiContext.Post(new SendOrPostCallback(this.UpdateGUIComplete), (object) null);
              this.mUiContext.Post(new SendOrPostCallback(this.UpdateGUI), (object) null);             
              Thread.Sleep(100);
            }));
            return Parallel.ForEach<string>((IEnumerable<string>) stringList, parallelOptions, body);
          })).ContinueWith((Action<Task<ParallelLoopResult>>) (tsk => this.EndTweets((Task) tsk, start)));
        }
      }
    }

    public void EndTweets(Task tsk, DateTime start)
    {
      TimeSpan timeSpan = DateTime.Now - start;
      this.mUiContext.Post(new SendOrPostCallback(this.UpdateGUI), (object) null);
      this.mUiContext.Post(new SendOrPostCallback(this.UpdateFinish), (object) ("Processing complete. Total time: " + (object) timeSpan.Hours + " hours " + (object) timeSpan.Minutes + " minutes " + (object) timeSpan.Seconds + " seconds"));
      this.mUiContext.Post(new SendOrPostCallback(this.UpdateGUIConsole), (object) ("Processing complete. Total time: " + (object) timeSpan.Hours + " hours " + (object) timeSpan.Minutes + " minutes " + (object) timeSpan.Seconds + " seconds"));
    }

    public void UpdateFinish(object userData)
    {
      this.lblFinish.Text = userData.ToString();
      this.btnDownload.Enabled = true;
      this.txtPassword.Enabled = true;
      this.txtUserName.Enabled = true;
      this.txtRefNum.Enabled = true;
      this.numThreads.Enabled = true;
      this.chkResume.Enabled = true;
      this.create_log();
    }

    public void UpdateGUI(object userData)
    {
      this.grdCases.Refresh();
    }

    public void UpdateGUIComplete(object userData)
    {
      if (this.lblCompleted.Text == "")
        this.lblCompleted.Text = "1";
      else
        this.lblCompleted.Text = Convert.ToString((int) Convert.ToInt16(this.lblCompleted.Text) + 1);
    }
    public void Updatestate(object userData,string casename)
    {
        if(userData.ToString() == "Cannot load this page") {
            if (this.lblErrorCase.Text == "")
                this.lblErrorCase.Text = "1";
            else
                this.lblErrorCase.Text = Convert.ToString((int)Convert.ToInt16(this.lblErrorCase.Text) + 1);
                noload_log.Add(casename);
        }else if(userData.ToString()== "Empty case!")
        {
            if (this.lblempCaseCount.Text == "")
                this.lblempCaseCount.Text = "1";
            else
                this.lblempCaseCount.Text = Convert.ToString((int)Convert.ToInt16(this.lblempCaseCount.Text) + 1);
                empty_log.Add(casename);
        }
            
    }
    public void UpdateGUIConsole(object userData)
    {
      TextBox txtConsole = this.txtConsole;
      string str = txtConsole.Text + Environment.NewLine + userData.ToString();
      txtConsole.Text = str;
      this.txtConsole.SelectionStart = this.txtConsole.TextLength;
      this.txtConsole.ScrollToCaret();
    }

    private void grdCases_CellContentClick(object sender, DataGridViewCellEventArgs e)
    {
    }

    private void txtRefNum_TextChanged(object sender, EventArgs e)
    {
    }

    private void frmCaseDownloader_Load(object sender, EventArgs e)
    {
      this.lblCompleted.Text = "";
      this.lblFinish.Text = "";
      this.lblStart.Text = "";
      this.lblTotal.Text = "";
      this.lblempCaseCount.Text = "";
      this.lblErrorCase.Text = "";
      this.txtConsole.Visible = true;
      frmCaseDownloader.CheckForIllegalCrossThreadCalls = false;
    }
    private void create_log()
    {
            DateTime now = DateTime.Now;
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)+ "\\LOG";
            DirectoryInfo directoryInfocase = new DirectoryInfo(filePath+"\\LOG");
            if (!directoryInfocase.Exists)
            {
                new DirectoryInfo(filePath).Create();
            }
            string path = filePath+"\\log -" + now.Day.ToString() +".txt";
            string time_UTC = now.ToString();
            File.AppendAllText(@path,"\r\n --- This log gernerated in "+ time_UTC +"---"+ Environment.NewLine);
            int i = 0;
            bool exitflag = false;
            if (noload_log.Count > 0) File.AppendAllText(@path, "\r\n (1) Can not download this page \r\n" + Environment.NewLine);
            while (true)
            {
                string text = " ";
                for (int j = 0; j <= 3; j++)
                {

                    if (4 * i + j < noload_log.Count)
                    {
                        text = text + noload_log[4 * i + j] + ", ";
                    }
                    else
                    {
                        File.AppendAllText(@path, text + Environment.NewLine);
                        exitflag = true;
                        break;
                    }
                    if (j == 3)
                    {
                        File.AppendAllText(@path, text + Environment.NewLine);
                        break;
                    }
                }
                i++;
                if (exitflag) break;
            }
            i = 0;
            exitflag = false;
            if (empty_log.Count > 0) File.AppendAllText(@path, "\r\n (2) Empty Case this has not documents! \r\n" + Environment.NewLine);
            while (true)
            {
                string text = " ";
                for (int j = 0; j <= 3; j++)
                {                    
                    if (4 * i + j < empty_log.Count)
                    {
                        text = text + empty_log[4 * i + j] + ", ";
                    }
                    else
                    {
                        File.AppendAllText(@path, text + Environment.NewLine);
                        exitflag = true;
                        break;
                    }
                    if (j == 3)
                    {
                        File.AppendAllText(@path, text + Environment.NewLine);
                        break;
                    }
                }
                i++;
                if (exitflag) break;
            }
            empty_log.Clear();
            noload_log.Clear();
    }

        protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.btnDownload = new Button();
      this.txtRefNum = new TextBox();
      this.label2 = new Label();
      this.txtUserName = new TextBox();
      this.label1 = new Label();
      this.label3 = new Label();
      this.txtPassword = new TextBox();
      this.numThreads = new NumericUpDown();
      this.label4 = new Label();
      this.grdCases = new DataGridView();
      this.lblStart = new Label();
      this.lblFinish = new Label();
      this.lblCompleted = new Label();
      this.lblTotal = new Label();
      this.txtConsole = new TextBox();
      this.label5 = new Label();
      this.label6 = new Label();
      this.lblempCaseCount = new Label();
      this.label7 = new Label();
      this.lblErrorCase = new Label();
      this.colRefNum = new DataGridViewTextBoxColumn();
      this.colCasesComp = new DataGridViewTextBoxColumn();
      this.colCaseCount = new DataGridViewTextBoxColumn();
      this.colDocs = new DataGridViewTextBoxColumn();
      this.linkDocs = new DataGridViewTextBoxColumn();
      this.empDocs = new DataGridViewTextBoxColumn();
      this.totDocs = new DataGridViewTextBoxColumn();
      this.allDocs = new DataGridViewTextBoxColumn();
      this.colStatus = new DataGridViewTextBoxColumn();
      this.chkResume = new CheckBox();
      this.numThreads.BeginInit();
      ((ISupportInitialize) this.grdCases).BeginInit();
      this.SuspendLayout();
      this.btnDownload.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold, GraphicsUnit.Point, (byte) 0);
      this.btnDownload.Location = new Point(852, 12);
      this.btnDownload.Name = "btnDownload";
      this.btnDownload.Size = new Size(106, 23);
      this.btnDownload.TabIndex = 0;
      this.btnDownload.Text = "&Download";
      this.btnDownload.UseVisualStyleBackColor = true;
      this.btnDownload.Click += new EventHandler(this.btnDownload_Click);
      this.txtRefNum.Enabled = true;
      this.txtRefNum.Location = new Point(91, 13);
      this.txtRefNum.Name = "txtRefNum";
      this.txtRefNum.Size = new Size(178, 20);
      this.txtRefNum.TabIndex = 1;
      this.txtRefNum.Text = "";
      this.txtRefNum.TextChanged += new EventHandler(this.txtRefNum_TextChanged);
      this.label2.AutoSize = true;
      this.label2.Font = new Font("Microsoft Sans Serif", 8.25f);
      this.label2.Location = new Point(12, 15);
      this.label2.Name = "label2";
      this.label2.Size = new Size(73, 13);
      this.label2.TabIndex = 12;
      this.label2.Text = "Cross Ref. No";
      this.txtUserName.Enabled = true;
      this.txtUserName.Location = new Point(344, 12);
      this.txtUserName.Name = "txtUserName";
      this.txtUserName.Size = new Size(103, 20);
      this.txtUserName.TabIndex = 13;
      this.txtUserName.Text = "LRGTAC_admin";
      this.label1.AutoSize = true;
      this.label1.Font = new Font("Microsoft Sans Serif", 8.25f);
      this.label1.Location = new Point(275, 15);
      this.label1.Name = "label1";
      this.label1.Size = new Size(63, 13);
      this.label1.TabIndex = 14;
      this.label1.Text = "User Name:";
      this.label3.AutoSize = true;
      this.label3.Font = new Font("Microsoft Sans Serif", 8.25f);
      this.label3.Location = new Point(453, 15);
      this.label3.Name = "label3";
      this.label3.Size = new Size(56, 13);
      this.label3.TabIndex = 16;
      this.label3.Text = "Password:";
      this.txtPassword.Enabled = true;
      this.txtPassword.Location = new Point(515, 11);
      this.txtPassword.Name = "txtPassword";
      this.txtPassword.Size = new Size(118, 20);
      this.txtPassword.TabIndex = 15;
      this.txtPassword.Text = "tacannie1";
      this.txtPassword.UseSystemPasswordChar = true;
      this.numThreads.Enabled = true;
      this.numThreads.Location = new Point(724, 12);
      this.numThreads.Maximum = new Decimal(new int[4]
      {
        20,
        0,
        0,
        0
      });
      this.numThreads.Minimum = new Decimal(new int[4]
      {
        1,
        0,
        0,
        0
      });
      this.numThreads.Name = "numThreads";
      this.numThreads.Size = new Size(42, 20);
      this.numThreads.TabIndex = 17;
      this.numThreads.Value = new Decimal(new int[4]
      {
        12,
        0,
        0,
        0
      });
      this.label4.AutoSize = true;
      this.label4.Font = new Font("Microsoft Sans Serif", 8.25f);
      this.label4.Location = new Point(639, 14);
      this.label4.Name = "label4";
      this.label4.Size = new Size(79, 13);
      this.label4.TabIndex = 18;
      this.label4.Text = "Threads (1-20):";
      this.grdCases.AllowUserToAddRows = false;
      this.grdCases.AllowUserToDeleteRows = false;
      this.grdCases.AllowUserToResizeColumns = false;
      this.grdCases.AllowUserToResizeRows = false;
      this.grdCases.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.grdCases.Columns.AddRange((DataGridViewColumn) this.colRefNum, (DataGridViewColumn) this.colCasesComp, (DataGridViewColumn) this.colCaseCount, (DataGridViewColumn)this.colDocs, (DataGridViewColumn)this.linkDocs, (DataGridViewColumn)this.empDocs, (DataGridViewColumn)this.totDocs, (DataGridViewColumn)this.allDocs, (DataGridViewColumn) this.colStatus);
      this.grdCases.Location = new Point(15, 68);
      this.grdCases.MultiSelect = false;
      this.grdCases.Name = "grdCases";
      this.grdCases.ReadOnly = true;
      this.grdCases.RowHeadersVisible = false;
      this.grdCases.Size = new Size(600, 575);
      this.grdCases.TabIndex = 19;
      this.grdCases.Visible = true;
      this.grdCases.CellContentClick += new DataGridViewCellEventHandler(this.grdCases_CellContentClick);
      this.lblStart.AutoSize = true;
      this.lblStart.Font = new Font("Microsoft Sans Serif", 8.25f);
      this.lblStart.Location = new Point(392, 46);
      this.lblStart.Name = "lblStart";
      this.lblStart.Size = new Size(55, 13);
      this.lblStart.TabIndex = 20;
      this.lblStart.Text = "Start Time";
      this.lblFinish.AutoSize = true;
      this.lblFinish.Font = new Font("Microsoft Sans Serif", 8.25f);
      this.lblFinish.Location = new Point(512, 46);
      this.lblFinish.Name = "lblFinish";
      this.lblFinish.Size = new Size(48, 13);
      this.lblFinish.TabIndex = 21;
      this.lblFinish.Text = "End time";
      this.lblCompleted.AutoSize = true;
      this.lblCompleted.Font = new Font("Microsoft Sans Serif", 8.25f);
      this.lblCompleted.Location = new Point(89, 46);
      this.lblCompleted.Name = "lblCompleted";
      this.lblCompleted.Size = new Size(30, 13);
      this.lblCompleted.TabIndex = 22;
      this.lblCompleted.Text = "lblCo";
      this.lblTotal.AutoSize = true;
      this.lblTotal.Font = new Font("Microsoft Sans Serif", 8.25f);
      this.lblTotal.Location = new Point(121, 46);
      this.lblTotal.Name = "lblTotal";
      this.lblTotal.Size = new Size(33, 13);
      this.lblTotal.TabIndex = 23;
      this.lblTotal.Text = "lblTot";
      this.label6.AutoSize = true;
      this.label6.Font = new Font("Microsoft Sans Serif", 8.25f);
      this.label6.Location = new Point(180, 46);
      this.label6.Name = "label6";
      this.label6.Size = new Size(60, 13);
      this.label6.TabIndex = 25;
      this.label6.Text = "Empty :";
      this.lblempCaseCount.AutoSize = true;
      this.lblempCaseCount.Font = new Font("Microsoft Sans Serif", 8.25f);
      this.lblempCaseCount.Location = new Point(220, 46);
      this.lblempCaseCount.Name = "lblempCaseCount";
      this.lblempCaseCount.Size = new Size(30, 13);
      this.lblempCaseCount.TabIndex = 22;
      this.lblempCaseCount.Text = "";
      this.label7.AutoSize = true;  
      this.label7.Font = new Font("Microsoft Sans Serif", 8.25f);
      this.label7.Location = new Point(270, 46);
      this.label7.Name = "label7";
      this.label7.Size = new Size(50, 13);
      this.label7.TabIndex = 25;
      this.label7.Text = "Error :";
      this.lblErrorCase.AutoSize = true;
      this.lblErrorCase.Font = new Font("Microsoft Sans Serif", 8.25f);
      this.lblErrorCase.Location = new Point(320, 46);
      this.lblErrorCase.Name = "lblErrorCase";
      this.lblErrorCase.Size = new Size(33, 13);
      this.lblErrorCase.TabIndex = 23;
      this.lblErrorCase.Text = "";
      this.txtConsole.Location = new Point(620, 68);
      this.txtConsole.Multiline = true;
      this.txtConsole.Visible = true;
      this.txtConsole.Name = "txtConsole";
      this.txtConsole.ReadOnly = true;
      this.txtConsole.ScrollBars = ScrollBars.Vertical;
      this.txtConsole.Size = new Size(336, 575);
      this.txtConsole.TabIndex = 24;
      this.label5.AutoSize = true;
      this.label5.Font = new Font("Microsoft Sans Serif", 8.25f);
      this.label5.Location = new Point(12, 46);
      this.label5.Name = "label5";
      this.label5.Size = new Size(71, 13);
      this.label5.TabIndex = 25;
      this.label5.Text = "Case Ref No:";
      this.colRefNum.HeaderText = "Cross Ref. No";
      this.colRefNum.Name = "colRefNum";
      this.colRefNum.ReadOnly = true;
      this.colRefNum.Width = 70;
      this.colCasesComp.HeaderText = "Completed Cases";
      this.colCasesComp.Name = "colCasesComp";
      this.colCasesComp.ReadOnly = true;
      this.colCasesComp.Width = 60;
      this.colCaseCount.HeaderText = "Total Cases";
      this.colCaseCount.Name = "colCaseCount";
      this.colCaseCount.ReadOnly = true;
      this.colCaseCount.Width = 50;
      this.colDocs.HeaderText = "Completed Counts";
      this.colDocs.ReadOnly = true;
      this.colDocs.Width = 65;
      this.colDocs.Name = "colDocs";
      this.linkDocs.HeaderText = "Link Count";
      this.linkDocs.ReadOnly = true;
      this.linkDocs.Width = 50;
      this.linkDocs.Name = "linkDocs";
      this.empDocs.HeaderText = "Generated PDF";
      this.empDocs.ReadOnly = true;
      this.empDocs.Width = 50;
      this.empDocs.Name = "empDocs";
      this.totDocs.HeaderText = "Documents Downloaded";
      this.totDocs.ReadOnly = true;
      this.totDocs.Width = 65;
      this.totDocs.Name = "totDocs";
      this.allDocs.HeaderText = "Total Count";
      this.allDocs.ReadOnly = true;
      this.allDocs.Width = 50;
      this.allDocs.Name = "allDocs";
      this.colStatus.HeaderText = "Status";
      this.colStatus.MinimumWidth = 210;
      this.colStatus.Name = "colStatus";
      this.colStatus.ReadOnly = true;
      this.colStatus.Width = 210;
      this.chkResume.AutoSize = true;
      this.chkResume.Enabled = false;
      this.chkResume.Location = new Point(781, 14);
      this.chkResume.Name = "chkResume";
      this.chkResume.Size = new Size(65, 17);
      this.chkResume.TabIndex = 26;
      this.chkResume.Text = "Resume";
      this.chkResume.UseVisualStyleBackColor = true;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(971, 655);
      this.Controls.Add((Control) this.chkResume);
      this.Controls.Add((Control) this.label5);
      this.Controls.Add((Control) this.txtConsole);
      this.Controls.Add((Control) this.lblTotal);
      this.Controls.Add((Control) this.lblCompleted);
      this.Controls.Add((Control) this.lblFinish);
      this.Controls.Add((Control)this.label6);
      this.Controls.Add((Control)this.lblempCaseCount);
      this.Controls.Add((Control)this.label7);
      this.Controls.Add((Control)this.lblErrorCase);
      this.Controls.Add((Control) this.lblStart);
      this.Controls.Add((Control) this.grdCases);
      this.Controls.Add((Control) this.label4);
      this.Controls.Add((Control) this.numThreads);
      this.Controls.Add((Control) this.label3);
      this.Controls.Add((Control) this.txtPassword);
      this.Controls.Add((Control) this.label1);
      this.Controls.Add((Control) this.txtUserName);
      this.Controls.Add((Control) this.label2);
      this.Controls.Add((Control) this.txtRefNum);
      this.Controls.Add((Control) this.btnDownload);
      this.Name = nameof (frmCaseDownloader);
      this.Text = "Case Downloader";
    //  this.TopMost = true;
      this.Load += new EventHandler(this.frmCaseDownloader_Load);
      this.numThreads.EndInit();
      ((ISupportInitialize) this.grdCases).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
