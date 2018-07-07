using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
using System.Net.Http;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
         
        }
        int TogMove;
        int MValx;
        int MValy;

        private void panel4_MouseDown(object sender,MouseEventArgs e)
        {
            TogMove = 1;
            MValx = e.X;
            MValy = e.Y;
        }

        private void panel4_MouseUp(object sender, MouseEventArgs e)
        {
            TogMove = 0;
        }

        private void panel4_MouseMove(object sender, MouseEventArgs e)
        {
            if (TogMove == 1)
            {
                this.SetDesktopLocation(MousePosition.X - MValx, MousePosition.Y - MValy);
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            getlevels();
            //

        }



        public void getlevels()
        {
            System.Net.HttpWebRequest myReg = (System.Net.HttpWebRequest)
            System.Net.WebRequest.Create("http://192.168.100.1/cgi-bin/status_cgi");
            System.Net.HttpWebResponse myResp = (System.Net.HttpWebResponse)myReg.GetResponse();
            Stream myStream = myResp.GetResponseStream();
            StreamReader myReader = new StreamReader(myStream);


            int counter = 0; //Counter to count the index size of the list [0]
            string templine = myReader.ReadLine(); //Reads the first line
            int linecounter = 0;
            double dssnr = 0;
            while (!myReader.EndOfStream)
            {
                string regularExpressionPattern = @"<td>([^<]+)<\/td><td>(\d+)<\/td><td>([^<]+)<\/td><td>([^<]+)<\/td><td>([^<]+)<\/td>";//Before <td> we had <tr>
                Regex re = new Regex(regularExpressionPattern);

                foreach (Match m in re.Matches(templine))
                {
                    Console.WriteLine(m.Value);
                    string result = Regex.Replace(m.Value, "((<td[^>]*>)|(</td>))", ""); //Between quotes we can put spacing if needed
                    listBox1.Items.Add(templine);
                    result = result.Substring(result.IndexOf('V') + 1);
                    result = result.Replace(" dB", "");
                    if (linecounter < 8) // Sums all the downstream channels
                    {
                        double x = Double.Parse(result);
                        dssnr = dssnr + x;
                    }
                    linecounter++;

                }
                Console.ReadLine();
                counter++;
                lbllines.Text = counter.ToString(); //Print Num of lines
                templine = myReader.ReadLine();
            }
            dssnr = dssnr / 8; //Calculate DDSNR (Divide downstream channel sum)
            double tempvalue = dssnr;
            textBox2.Text = dssnr.ToString();
            textBox2.Text.Trim();
            if (tempvalue > 35)
            {
                lblstatusdssnr.Text = "       ✔";
            }
            else if (tempvalue < 35)
            {
                lblstatusdssnr.Text = "HIGH DSSNR";
            }
            myResp.Close();

            get_upstream();


        }
        public void get_upstream()
        {

            int i = 0;
            string result = "";
            while (listBox1.Items.Count > i)
            {
                //Original string before mods <td>Upstream 1<\/td><td>\d+<\/td><td>([\d.]+) MHz<\/td><td>([\d.]+) dBmV<\/td>
                string regularExpressionPattern2 = @"<td>Upstream 1<\/td><td>\d+<\/td><td>([\d.]+) MHz<\/td><td>([\d.]+) dBmV<\/td>";
                Regex re = new Regex(regularExpressionPattern2);

                foreach (Match m in re.Matches(listBox1.Items[i].ToString()))
                {
                    result = Regex.Replace(m.Value, "((<td[^>]*>)|(</td>))", ""); //Between quotes we can put spacing if needed
                    string tempresult = result; // Will use that to get USPWER

                    //======================================================
                    //                   USSNR PRINT
                    //======================================================
                    result = result.Substring(result.IndexOf('0') + 1); // Will use this to set the pointer after UCID since UPSTREAM#1 UCID=10 and we use the last Char of 10 -> 0
                    result = Regex.Replace(result, "MHz+[^ ]+ dBmV", "");
                    textBox1.Text = result.ToString();
                    textBox1.Text.Trim();
                    Console.WriteLine(result);
                    double tempussnr = Convert.ToDouble(textBox1.Text);

                    if (tempussnr >= 32)
                    {
                        lblstatusussnr.Text = "      ✔";
                    }
                    else if (tempussnr < 32)
                    {
                        lblstatusussnr.Text = "HIGH USSNR";
                    }

                    //======================================================
                    //                   USPOWER PRINT
                    //======================================================
                    tempresult = tempresult.Substring(tempresult.IndexOf('z') + 1); // Will use this to find USPWER
                    tempresult = Regex.Replace(tempresult, " dBmV", "");
                    Console.WriteLine(tempresult);
                    txtboxuspwr.Text = tempresult.ToString();
                    txtboxuspwr.Text.Trim();

                    double tempuspwr = Convert.ToDouble(txtboxuspwr.Text);

                    if (tempuspwr > 51)
                    {
                        lblstatususpwr.Text = "LOW USPOWER";
                    }
                    else if (tempuspwr <= 51)
                    {
                        lblstatususpwr.Text = "       ✔";
                    }
                }
                i++;
            }
             
        }

        private void btnline_Click(object sender, EventArgs e)
        {
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
