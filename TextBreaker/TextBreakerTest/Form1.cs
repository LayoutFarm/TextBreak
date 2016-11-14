using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using LayoutFarm.TextBreaker;

namespace TextBreakerTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //break
            EnTextBreaker enTextBreaker = new EnTextBreaker();
            string text = this.textBox1.Text;
            listBox1.Items.Clear();
            char[] textBuffer = text.ToCharArray();
            enTextBreaker.DoBreak(textBuffer, bounds =>
            {
                listBox1.Items.Add(
                    new string(textBuffer, bounds.startIndex, bounds.length));


            });
        }

        private void cmdReadDict_Click(object sender, EventArgs e)
        {

        }


    }
}
