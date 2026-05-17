using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TTG_Tools
{
    public partial class QuickToolsDialog : Form
    {
        private readonly FontEditor _owner;

        // Controls for Copy coordinates (groupBox1)
        private GroupBox groupBox1;
        private TextBox textBox8;
        private TextBox textBox9;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private CheckBox checkBox1;
        private CheckBox checkBox2;
        private Button buttonClear;
        private Button buttonCopyCoordinates;

        // Controls for Change char width (groupBox3)
        private GroupBox groupBox3;
        private TextBox textBox1;
        private Button button1;
        private RadioButton radioButtonXstart;
        private RadioButton radioButtonXend;
        private Label label5;

        public QuickToolsDialog(FontEditor owner)
        {
            _owner = owner;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Quick Tools";
            this.Size = new Size(240, 370);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ShowInTaskbar = false;

            // ======== groupBox1: Copy coordinates ========
            this.groupBox1 = new GroupBox();
            this.textBox8 = new TextBox();
            this.textBox9 = new TextBox();
            this.label1 = new Label();
            this.label2 = new Label();
            this.label3 = new Label();
            this.label4 = new Label();
            this.checkBox1 = new CheckBox();
            this.checkBox2 = new CheckBox();
            this.buttonClear = new Button();
            this.buttonCopyCoordinates = new Button();

            this.groupBox1.SuspendLayout();
            this.SuspendLayout();

            this.groupBox1.Controls.Add(this.buttonClear);
            this.groupBox1.Controls.Add(this.textBox8);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.checkBox1);
            this.groupBox1.Controls.Add(this.buttonCopyCoordinates);
            this.groupBox1.Controls.Add(this.checkBox2);
            this.groupBox1.Controls.Add(this.textBox9);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Location = new Point(12, 12);
            this.groupBox1.Size = new Size(210, 109);
            this.groupBox1.Text = "Copy coordinates";

            this.textBox8.Location = new Point(46, 20);
            this.textBox8.Size = new Size(72, 21);

            this.textBox9.Location = new Point(46, 46);
            this.textBox9.Size = new Size(72, 21);

            this.label1.AutoSize = true;
            this.label1.Location = new Point(123, 23);
            this.label1.Text = "(0)";

            this.label2.AutoSize = true;
            this.label2.Location = new Point(123, 48);
            this.label2.Text = "(0)";

            this.label3.AutoSize = true;
            this.label3.Location = new Point(10, 23);
            this.label3.Text = "From";

            this.label4.AutoSize = true;
            this.label4.Location = new Point(10, 49);
            this.label4.Text = "To";

            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = CheckState.Checked;
            this.checkBox1.Location = new Point(151, 22);
            this.checkBox1.Text = "x";

            this.checkBox2.AutoSize = true;
            this.checkBox2.Checked = true;
            this.checkBox2.CheckState = CheckState.Checked;
            this.checkBox2.Location = new Point(151, 48);
            this.checkBox2.Text = "y";

            this.buttonClear.Location = new Point(124, 72);
            this.buttonClear.Size = new Size(57, 23);
            this.buttonClear.Text = "Clear";
            this.buttonClear.Click += buttonClear_Click;

            this.buttonCopyCoordinates.Location = new Point(46, 72);
            this.buttonCopyCoordinates.Size = new Size(70, 23);
            this.buttonCopyCoordinates.Text = "Do it!";
            this.buttonCopyCoordinates.Click += buttonCopyCoordinates_Click;

            this.textBox8.TextChanged += textBox8_TextChanged;
            this.textBox9.TextChanged += textBox9_TextChanged;

            // ======== groupBox3: Change char width ========
            this.groupBox3 = new GroupBox();
            this.radioButtonXstart = new RadioButton();
            this.radioButtonXend = new RadioButton();
            this.label5 = new Label();
            this.textBox1 = new TextBox();
            this.button1 = new Button();

            this.groupBox3.SuspendLayout();

            this.groupBox3.Controls.Add(this.radioButtonXstart);
            this.groupBox3.Controls.Add(this.radioButtonXend);
            this.groupBox3.Controls.Add(this.textBox1);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.button1);
            this.groupBox3.Location = new Point(12, 130);
            this.groupBox3.Size = new Size(210, 125);
            this.groupBox3.Text = "Change char width to:";

            this.label5.AutoSize = true;
            this.label5.Location = new Point(7, 22);
            this.label5.Size = new Size(55, 13);
            this.label5.Text = "Value (+/-):";

            this.textBox1.Location = new Point(7, 42);
            this.textBox1.Size = new Size(58, 21);
            this.textBox1.Text = "0";

            this.radioButtonXstart.AutoSize = true;
            this.radioButtonXstart.Location = new Point(80, 20);
            this.radioButtonXstart.Size = new Size(66, 17);
            this.radioButtonXstart.Text = "X Start";
            this.radioButtonXstart.UseVisualStyleBackColor = true;

            this.radioButtonXend.AutoSize = true;
            this.radioButtonXend.Checked = true;
            this.radioButtonXend.Location = new Point(80, 42);
            this.radioButtonXend.Size = new Size(63, 17);
            this.radioButtonXend.Text = "X End";
            this.radioButtonXend.UseVisualStyleBackColor = true;

            this.button1.Location = new Point(7, 72);
            this.button1.Size = new Size(148, 23);
            this.button1.Text = "Change";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += button1_Click;

            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();

            // Add to form
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox3);

            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
        }

        // ======== Event Handlers ========

        private void buttonClear_Click(object sender, EventArgs e)
        {
            textBox8.Text = "";
            textBox9.Text = "";
            label1.Text = "(0)";
            label2.Text = "(0)";
            checkBox1.Checked = true;
            checkBox2.Checked = true;
        }

        private void buttonCopyCoordinates_Click(object sender, EventArgs e)
        {
            string ch1 = textBox8.Text;
            string ch2 = textBox9.Text;
            if (ch1.Length == ch2.Length)
            {
                for (int i = 0; i < ch1.Length; i++)
                {
                    int f = Convert.ToInt32(Encoding.GetEncoding(AppData.settings.ASCII_N).GetBytes(ch1[i].ToString())[0]);
                    int s = Convert.ToInt32(Encoding.GetEncoding(AppData.settings.ASCII_N).GetBytes(ch2[i].ToString())[0]);
                    int first = 0;
                    int second = 0;
                    for (int j = 0; j < _owner.dataGridViewWithCoord.RowCount; j++)
                    {
                        if (Convert.ToInt32(_owner.dataGridViewWithCoord[0, j].Value) == f)
                            first = j;
                        if (Convert.ToInt32(_owner.dataGridViewWithCoord[0, j].Value) == s)
                            second = j;
                    }

                    CopyDataInGrid(6, first, second);
                    CopyDataInGrid(7, first, second);
                    CopyDataInGrid(8, first, second);
                    CopyDataInGrid(9, first, second);
                    CopyDataInGrid(10, first, second);
                    CopyDataInGrid(11, first, second);
                    CopyDataInGrid(12, first, second);

                    if (checkBox1.Checked)
                    {
                        CopyDataInGrid(2, first, second);
                        CopyDataInGrid(3, first, second);
                    }
                    if (checkBox2.Checked)
                    {
                        CopyDataInGrid(4, first, second);
                        CopyDataInGrid(5, first, second);
                    }
                }
            }
            else if (ch1.Length == 1)
            {
                for (int i = 0; i < ch2.Length; i++)
                {
                    int f = Convert.ToInt32(Encoding.GetEncoding(AppData.settings.ASCII_N).GetBytes(ch1[i].ToString())[0]);
                    int s = Convert.ToInt32(Encoding.GetEncoding(AppData.settings.ASCII_N).GetBytes(ch2[i].ToString())[0]);
                    int first = 0;
                    int second = 0;
                    for (int j = 0; j < _owner.dataGridViewWithCoord.RowCount; j++)
                    {
                        if (Convert.ToInt32(_owner.dataGridViewWithCoord[0, j].Value) == f)
                            first = j;
                        if (Convert.ToInt32(_owner.dataGridViewWithCoord[0, j].Value) == s)
                            second = j;
                    }

                    CopyDataInGrid(6, first, second);
                    CopyDataInGrid(7, first, second);
                    CopyDataInGrid(8, first, second);
                    CopyDataInGrid(9, first, second);
                    CopyDataInGrid(10, first, second);
                    CopyDataInGrid(11, first, second);
                    CopyDataInGrid(12, first, second);

                    if (checkBox1.Checked)
                    {
                        CopyDataInGrid(2, first, second);
                        CopyDataInGrid(3, first, second);
                    }
                    if (checkBox2.Checked)
                    {
                        CopyDataInGrid(4, first, second);
                        CopyDataInGrid(5, first, second);
                    }
                }
            }
        }

        private void CopyDataInGrid(int column, int first, int second)
        {
            var dgv = _owner.dataGridViewWithCoord;
            if (dgv.RowCount > first && dgv.RowCount > second && dgv.ColumnCount > column)
            {
                var sourceCell = dgv[column, first];
                var destCell = dgv[column, second];
                if (sourceCell != null && destCell != null)
                {
                    destCell.Value = sourceCell.Value;
                    destCell.Style.BackColor = Color.Green;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Methods.IsNumeric(textBox1.Text))
            {
                int w = Convert.ToInt32(textBox1.Text);
                var dgv = _owner.dataGridViewWithCoord;
                for (int i = 0; i < dgv.RowCount; i++)
                {
                    if (radioButtonXend.Checked)
                        dgv[3, i].Value = Convert.ToInt32(dgv[3, i].Value) + w;
                    else
                        dgv[2, i].Value = Convert.ToInt32(dgv[2, i].Value) + w;
                    dgv[7, i].Value = Convert.ToInt32(dgv[7, i].Value) + w;
                    dgv[12, i].Value = Convert.ToInt32(dgv[12, i].Value) + w;
                }
            }
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            label1.Text = "(" + textBox8.Text.Length.ToString() + ")";
        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {
            label2.Text = "(" + textBox9.Text.Length.ToString() + ")";
        }
    }
}
