namespace Graph_Simulator
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlView = new System.Windows.Forms.Panel();
            this.chkRandom = new System.Windows.Forms.CheckBox();
            this.cmbNodes = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtCost = new System.Windows.Forms.TextBox();
            this.btnCalc = new System.Windows.Forms.Button();
            this.btnAddNode = new System.Windows.Forms.Button();
            this.lstResult = new System.Windows.Forms.ListBox();
            this.chk2way = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // pnlView
            // 
            this.pnlView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlView.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.pnlView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlView.Location = new System.Drawing.Point(163, 38);
            this.pnlView.Name = "pnlView";
            this.pnlView.Size = new System.Drawing.Size(795, 374);
            this.pnlView.TabIndex = 21;
            this.pnlView.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlView_Paint);
            this.pnlView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlView_MouseDown);
            // 
            // chkRandom
            // 
            this.chkRandom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkRandom.AutoSize = true;
            this.chkRandom.Location = new System.Drawing.Point(269, 13);
            this.chkRandom.Name = "chkRandom";
            this.chkRandom.Size = new System.Drawing.Size(66, 17);
            this.chkRandom.TabIndex = 20;
            this.chkRandom.Text = "Random";
            this.chkRandom.UseVisualStyleBackColor = true;
            this.chkRandom.CheckedChanged += new System.EventHandler(this.chkRandom_CheckedChanged);
            // 
            // cmbNodes
            // 
            this.cmbNodes.FormattingEnabled = true;
            this.cmbNodes.Location = new System.Drawing.Point(94, 108);
            this.cmbNodes.Name = "cmbNodes";
            this.cmbNodes.Size = new System.Drawing.Size(63, 21);
            this.cmbNodes.TabIndex = 19;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(131, 13);
            this.label1.TabIndex = 17;
            this.label1.Text = "Weight added Connection";
            // 
            // txtCost
            // 
            this.txtCost.Location = new System.Drawing.Point(163, 9);
            this.txtCost.Name = "txtCost";
            this.txtCost.Size = new System.Drawing.Size(100, 20);
            this.txtCost.TabIndex = 16;
            this.txtCost.Text = "5";
            // 
            // btnCalc
            // 
            this.btnCalc.Location = new System.Drawing.Point(12, 106);
            this.btnCalc.Name = "btnCalc";
            this.btnCalc.Size = new System.Drawing.Size(76, 23);
            this.btnCalc.TabIndex = 14;
            this.btnCalc.Text = "Calculate";
            this.btnCalc.UseVisualStyleBackColor = true;
            this.btnCalc.Click += new System.EventHandler(this.btnCalc_Click);
            // 
            // btnAddNode
            // 
            this.btnAddNode.Location = new System.Drawing.Point(12, 36);
            this.btnAddNode.Name = "btnAddNode";
            this.btnAddNode.Size = new System.Drawing.Size(145, 23);
            this.btnAddNode.TabIndex = 15;
            this.btnAddNode.Text = "Add Node";
            this.btnAddNode.UseVisualStyleBackColor = true;
            this.btnAddNode.Click += new System.EventHandler(this.btnAddNode_Click);
            // 
            // lstResult
            // 
            this.lstResult.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lstResult.FormattingEnabled = true;
            this.lstResult.Location = new System.Drawing.Point(12, 159);
            this.lstResult.Name = "lstResult";
            this.lstResult.Size = new System.Drawing.Size(145, 251);
            this.lstResult.TabIndex = 22;
            this.lstResult.SelectedIndexChanged += new System.EventHandler(this.lstResult_SelectedIndexChanged);
            // 
            // chk2way
            // 
            this.chk2way.AutoSize = true;
            this.chk2way.Location = new System.Drawing.Point(354, 13);
            this.chk2way.Name = "chk2way";
            this.chk2way.Size = new System.Drawing.Size(69, 17);
            this.chk2way.TabIndex = 23;
            this.chk2way.Text = "Two way";
            this.chk2way.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(970, 433);
            this.Controls.Add(this.chk2way);
            this.Controls.Add(this.lstResult);
            this.Controls.Add(this.pnlView);
            this.Controls.Add(this.chkRandom);
            this.Controls.Add(this.cmbNodes);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtCost);
            this.Controls.Add(this.btnCalc);
            this.Controls.Add(this.btnAddNode);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pnlView;
        private System.Windows.Forms.CheckBox chkRandom;
        private System.Windows.Forms.ComboBox cmbNodes;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtCost;
        private System.Windows.Forms.Button btnCalc;
        private System.Windows.Forms.Button btnAddNode;
        private System.Windows.Forms.ListBox lstResult;
        private System.Windows.Forms.CheckBox chk2way;
    }
}

