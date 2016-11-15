using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using LayoutFarm.TextBreaker;
using LayoutFarm.TextBreaker.CustomBreaker;

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

            DictionaryData.LoadData("../../../icu58/brkitr/thaidict.dict");
        }

        private void cmdCustomBuild_Click(object sender, EventArgs e)
        {
            //test read dict data line
            CustomDic customDic = new CustomDic();
            customDic.LoadFromTextfile("../../../icu58/brkitr_src/dictionaries/thaidict.txt");
            CustomBreaker breaker1 = new CustomBreaker();
            breaker1.AddDic(customDic);
            breaker1.BreakWords("ผู้มาใหญ่หาผ้าใหม่ให้สะใภ้ใช้คล้องคอ ใฝ่ใจเอาใส่ห่อมิหลงใหลใครขอดู จะใคร่ลงเรือใบดูน้ำใสและปลาปู สิ่งใดอยู่ในตู้มิใช่อยู่ใต้ตั่งเตียง บ้าใบถือใยบัวหูตามัวมาให้เคียง เล่าเท่าอย่าละเลี่ยงยี่สิบม้วนจำจงดี");

        }
    }
}
