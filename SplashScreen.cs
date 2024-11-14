using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Face_Detection_Security_System
{
    public partial class SplashScreen : Form
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]

        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,
            int nTopRect,
            int RightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
        );
        public SplashScreen()
        {
            InitializeComponent();
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 25, 25));
            ProgressBar1.Value = 0;
        }

        private void SplashScreen_Load(object sender, EventArgs e)
        {

        }
        private void timer1_Tick_1(object sender, EventArgs e)
        {
            label4.Visible = false;
            label5.Visible = false;
            textBox1.Visible = false;
            textBox2.Visible = false;
            button1.Visible = false;
            ProgressBar1.Value += 4;
            ProgressBar1.Text = ProgressBar1.Value.ToString() + "%";
            if (ProgressBar1.Value == 100)
            {
                timer1.Enabled = false;
                //Survelliance obj = new Survelliance();
                //Register obj = new Register();
                ProgressBar1.Visible = false;
                label2.Visible = false;
                textBox1.Text = "";
                textBox2.Text = "";
                label4.Visible = true;
                label5.Visible = true;
                textBox1.Visible = true;
                textBox2.Visible = true;
                button1.Visible = true;
                
                //MainForm obj = new MainForm();
                //obj.Show();
                //this.Hide();
            }
        }

        private void ProgressBar1_Click(object sender, EventArgs e)
        {

        }

        private void SplashScreen_Load_1(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "admin" && textBox2.Text == "1234")
            {
                MainForm obj = new MainForm();
                obj.Show();
                this.Hide();
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
}
