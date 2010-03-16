namespace GatherInstalledApplications {
    partial class Form1 {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.checkedComboBoxEditVendors = new DevExpress.XtraEditors.CheckedComboBoxEdit();
            this.groupControl1 = new DevExpress.XtraEditors.GroupControl();
            this.memoEditServers = new DevExpress.XtraEditors.MemoEdit();
            this.groupControl2 = new DevExpress.XtraEditors.GroupControl();
            this.progressBarControl1 = new DevExpress.XtraEditors.ProgressBarControl();
            this.simpleButtonStart = new DevExpress.XtraEditors.SimpleButton();
            this.simpleButtonQuit = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.checkedComboBoxEditVendors.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).BeginInit();
            this.groupControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.memoEditServers.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).BeginInit();
            this.groupControl2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.progressBarControl1.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // checkedComboBoxEditVendors
            // 
            this.checkedComboBoxEditVendors.EditValue = "";
            this.checkedComboBoxEditVendors.Location = new System.Drawing.Point(5, 25);
            this.checkedComboBoxEditVendors.Name = "checkedComboBoxEditVendors";
            this.checkedComboBoxEditVendors.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.checkedComboBoxEditVendors.Properties.Items.AddRange(new DevExpress.XtraEditors.Controls.CheckedListBoxItem[] {
            new DevExpress.XtraEditors.Controls.CheckedListBoxItem("Microsoft"),
            new DevExpress.XtraEditors.Controls.CheckedListBoxItem("eG"),
            new DevExpress.XtraEditors.Controls.CheckedListBoxItem("Citrix")});
            this.checkedComboBoxEditVendors.Size = new System.Drawing.Size(248, 20);
            this.checkedComboBoxEditVendors.TabIndex = 0;
            // 
            // groupControl1
            // 
            this.groupControl1.Controls.Add(this.checkedComboBoxEditVendors);
            this.groupControl1.Location = new System.Drawing.Point(12, 12);
            this.groupControl1.Name = "groupControl1";
            this.groupControl1.Size = new System.Drawing.Size(258, 55);
            this.groupControl1.TabIndex = 1;
            this.groupControl1.Text = "Vendors";
            // 
            // memoEditServers
            // 
            this.memoEditServers.Location = new System.Drawing.Point(5, 25);
            this.memoEditServers.Name = "memoEditServers";
            this.memoEditServers.Size = new System.Drawing.Size(247, 235);
            this.memoEditServers.TabIndex = 2;
            // 
            // groupControl2
            // 
            this.groupControl2.Controls.Add(this.memoEditServers);
            this.groupControl2.Location = new System.Drawing.Point(13, 73);
            this.groupControl2.Name = "groupControl2";
            this.groupControl2.Size = new System.Drawing.Size(257, 265);
            this.groupControl2.TabIndex = 3;
            this.groupControl2.Text = "Servers";
            // 
            // progressBarControl1
            // 
            this.progressBarControl1.Location = new System.Drawing.Point(12, 344);
            this.progressBarControl1.Name = "progressBarControl1";
            this.progressBarControl1.Size = new System.Drawing.Size(258, 18);
            this.progressBarControl1.TabIndex = 4;
            // 
            // simpleButtonStart
            // 
            this.simpleButtonStart.Location = new System.Drawing.Point(276, 12);
            this.simpleButtonStart.Name = "simpleButtonStart";
            this.simpleButtonStart.Size = new System.Drawing.Size(75, 23);
            this.simpleButtonStart.TabIndex = 5;
            this.simpleButtonStart.Text = "Start";
            this.simpleButtonStart.Click += new System.EventHandler(this.simpleButtonStart_Click);
            // 
            // simpleButtonQuit
            // 
            this.simpleButtonQuit.Location = new System.Drawing.Point(276, 41);
            this.simpleButtonQuit.Name = "simpleButtonQuit";
            this.simpleButtonQuit.Size = new System.Drawing.Size(75, 23);
            this.simpleButtonQuit.TabIndex = 6;
            this.simpleButtonQuit.Text = "Quit";
            this.simpleButtonQuit.Click += new System.EventHandler(this.simpleButtonQuit_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(360, 370);
            this.Controls.Add(this.simpleButtonQuit);
            this.Controls.Add(this.simpleButtonStart);
            this.Controls.Add(this.progressBarControl1);
            this.Controls.Add(this.groupControl2);
            this.Controls.Add(this.groupControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Installed Applications";
            ((System.ComponentModel.ISupportInitialize)(this.checkedComboBoxEditVendors.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl1)).EndInit();
            this.groupControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.memoEditServers.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).EndInit();
            this.groupControl2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.progressBarControl1.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.CheckedComboBoxEdit checkedComboBoxEditVendors;
        private DevExpress.XtraEditors.GroupControl groupControl1;
        private DevExpress.XtraEditors.MemoEdit memoEditServers;
        private DevExpress.XtraEditors.GroupControl groupControl2;
        private DevExpress.XtraEditors.ProgressBarControl progressBarControl1;
        private DevExpress.XtraEditors.SimpleButton simpleButtonStart;
        private DevExpress.XtraEditors.SimpleButton simpleButtonQuit;

    }
}

