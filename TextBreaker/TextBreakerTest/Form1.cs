using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using LayoutFarm.TextBreaker;
using LayoutFarm.TextBreaker.Custom;

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

            LayoutFarm.TextBreaker.ICU.DictionaryData.LoadData("../../../icu58/brkitr/thaidict.dict");
        }

        CustomDic customDic;
        ThaiDictionaryBreakingEngine thaiDicBreakingEngine;
        private void cmdCustomBuild_Click(object sender, EventArgs e)
        {
            //test read dict data line
            //1. load dic
            if (customDic == null)
            {
                customDic = new CustomDic();
                customDic.LoadFromTextfile("../../../icu58/brkitr_src/dictionaries/thaidict.txt");
                thaiDicBreakingEngine = new ThaiDictionaryBreakingEngine();
                thaiDicBreakingEngine.SetDictionaryData(customDic);

            }
            //2. create dictionary based breaking engine
            CustomBreaker breaker1 = new CustomBreaker();
            breaker1.AddBreakingEngine(thaiDicBreakingEngine);
            char[] test = this.textBox1.Text.ToCharArray();
            breaker1.BreakWords(test, 0);
            this.listBox1.Items.Clear();
            foreach (var span in breaker1.GetBreakSpanIter())
            {
                string s = new string(test, span.startAt, span.len);
                this.listBox1.Items.Add(span.startAt + " " + s);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string test1= "ผู้ใหญ่หาผ้าใหม่ให้สะใภ้ใช้คล้องคอ ใฝ่ใจเอาใส่ห่อมิหลงใหลใครขอดู จะใคร่ลงเรือใบดูน้ำใสและปลาปู สิ่งใดอยู่ในตู้มิใช่อยู่ใต้ตั่งเตียง บ้าใบถือใยบัวหูตามัวมาให้เคียง เล่าเท่าอย่าละเลี่ยงยี่สิบม้วนจำจงดี";
            this.textBox1.Text = test1;
        }
    }
}
