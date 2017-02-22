using System;
using System.Windows.Forms;

namespace Station_Configuration_Utility
{
    public partial class SplashScreen : Form
    {
        public SplashScreen()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            progressBar1.Increment(1);
            if (progressBar1.Value > 1 && progressBar1.Value < 10)
            {
                label1.Text = "Files Loading..";
            }
            if (progressBar1.Value > 10 && progressBar1.Value < 20)
            {
                label1.Text = "Files Loading....";
            }
            if (progressBar1.Value > 20 && progressBar1.Value < 30)
            {
                label1.Text = "Files Loading.....";
            }
            if (progressBar1.Value > 30 && progressBar1.Value < 40)
            {
                label1.Text = "Modules Loading..";
            }
            if (progressBar1.Value > 40 && progressBar1.Value < 50)
            {
                label1.Text = "Modules Loading....";
            }
            if (progressBar1.Value > 50  && progressBar1.Value < 60)
            {
                label1.Text = "Modules Loading.....";
            }
            if (progressBar1.Value > 60 && progressBar1.Value < 70)
            {
                label1.Text = "User Interface Loading..";
            }
            if (progressBar1.Value > 70 && progressBar1.Value < 80)
            {
                label1.Text = "User Interface Loading....";
            }
            if (progressBar1.Value > 80 && progressBar1.Value < 99)
            {
                label1.Text = "User Interface Loading.....";
            }
            if (progressBar1.Value == 100)
            {
                label1.Text = "Successfully Loaded!";
                timer1.Stop();
            }
        }
    }
}
