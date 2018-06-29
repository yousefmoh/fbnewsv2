using FBNews.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FBNews
{
    public partial class Form1 : Form
    {
        string token = "EAACEdEose0cBAOvN2HMaxFcJlNsyz3ZCXOFAC3JKUwgmtIMnQc0pYPhQYveoHBFQEfjtfSlmmPj25nCZAhVrGkdiS7fbt7vqvrYn9iEWZBZBzIPgkUiZCdpYcZCgMplWT8gHppwhzwBha8zBGdRxyFqVskIAGxNRja7tjYQ8ZCCZBQZDZD";
        List<page> pages;
        FBNewsDbEntities fbEntities;
        string[] keywords;
        string path;

        public Form1()
        {
            InitializeComponent();
      //       FormBorderStyle = FormBorderStyle.None;
        //    WindowState = FormWindowState.Normal;

            getcriterias();

        }

        private void getcriterias()
        {
            fbEntities = new FBNewsDbEntities();
            listView1.View = View.Details;
            listView1.GridLines = true;
            listView1.FullRowSelect = true;
            ListViewItem itm;

            listView1.Clear();

            //Add column header
            listView1.Columns.Add("ID", 100);
            listView1.Columns.Add("Cratiria Name", 100);

            var criterias = fbEntities.criterias.ToList();
            foreach (criteria criteria in criterias)
            {
                string[] arrcriterias = new string[4];
                arrcriterias[0] = criteria.id + "";
                arrcriterias[1] = criteria.criterianame;
                itm = new ListViewItem(arrcriterias);
                listView1.Items.Add(itm);
            }





        }


        private void getpages()
        {
            int criteriaId = int.Parse(listView1.SelectedItems[0].SubItems[0].Text);

            fbEntities = new FBNewsDbEntities();
            listView2.View = View.Details;
            listView2.GridLines = true;
            listView2.FullRowSelect = true;
            ListViewItem itm;

            listView2.Clear();

            //Add column header
            listView2.Columns.Add("ID", 50);
            listView2.Columns.Add("Page Name", 80);
            listView2.Columns.Add("Page Category", 80);
            listView2.Columns.Add("Page Location", 80);
            listView2.Columns.Add("Page Url", 150);



            var pages = fbEntities.pages.Where(c => c.criteriaId == criteriaId).ToList();
            foreach (page page in pages)
            {
                string[] arrpages = new string[5];
                arrpages[0] = page.id + "";
                arrpages[1] = page.pagename;
                arrpages[2] = page.category;
                arrpages[3] = page.location;
                arrpages[4] = page.pageurl;

                itm = new ListViewItem(arrpages);
                listView2.Items.Add(itm);
            }





        }
        private void getKeywords()
        {
            int criteriaId = int.Parse(listView1.SelectedItems[0].SubItems[0].Text);

            fbEntities = new FBNewsDbEntities();
            listView3.View = View.Details;
            listView3.GridLines = true;
            listView3.FullRowSelect = true;
            ListViewItem itm;

            listView3.Clear();

            //Add column header
            listView3.Columns.Add("ID", 100);
            listView3.Columns.Add("Keyword Name", 100);

            var keywords = fbEntities.keywords.Where(c => c.cirteriaId == criteriaId).ToList();
            foreach (keyword keyword in keywords)
            {
                string[] arrkeywords = new string[4];
                arrkeywords[0] = keyword.id + "";
                arrkeywords[1] = keyword.keywords;
                itm = new ListViewItem(arrkeywords);
                listView3.Items.Add(itm);
            }





        }



        //Search
        private void button4_Click(object sender, EventArgs e)
        {
            int postcount = 0;
            fbEntities = new FBNewsDbEntities();
            label6.Text = "Loading";
            var criterias = fbEntities.criterias.ToList();

            //Task f = Task.Factory.StartNew(() =>
            //{
                
           


            foreach (criteria criteria in criterias)
            {

                pages = fbEntities.pages.Where(c => c.criteriaId == criteria.id).ToList();
                keywords = fbEntities.keywords.Where(c => c.cirteriaId == criteria.id).Select(c => c.keywords).ToArray();

                if (pages.Count == 0 || keywords.Length == 0)
                {
                    continue;
                }

                foreach (page page in pages)
                {



                    path = "https://graph.facebook.com/" + page.pagefbid + "/posts?limit=100&access_token=" + token;
                    var clientRepsonse = "";
                    try
                    {
                        using (var client = new WebClient())
                        {
                            clientRepsonse = client.DownloadString(path);
                        }
                        int count = 0;

                        dynamic deserialize_post = JsonConvert.DeserializeObject(clientRepsonse);
                        if (deserialize_post.data != null)
                        {

                            // check if posts exist
                            var listresponse = deserialize_post.data;
                            DateTime lastCheckDate = listresponse[0].created_time;

                            for (int i = 0; i < listresponse.Count; i++)
                            {
                                count = listresponse.Count;
                                string message = listresponse[i].message;
                                DateTime createdate = listresponse[i].created_time;
                                string id = listresponse[i].id;



                                if (!string.IsNullOrEmpty(message))
                                {
                                    foreach (string keyword in keywords)
                                    {
                                        if (message.ToLower().Contains(keyword.ToLower()) && !(fbEntities.FBNews.Any(c => c.post_id == id)))
                                        {

                                            string [] url = id.Split('_');
                                            FBNew fb = new FBNew();
                                            fb.created_at = createdate;
                                            fb.message = message;
                                            fb.pagename = page.pagename;
                                            fb.post_id = id;
                                           
                                            fb.location = page.location;
                                            fb.category = page.category;
                                            fb.keyword = keyword;
                                            fb.url = page.pageurl+url[1];
                                            fbEntities.FBNews.Add(fb);
                                            fbEntities.SaveChanges();
                                            postcount++;

                                        }
                                    }

                                }
                            }
                            page.lastcheckdate = lastCheckDate;
                            fbEntities.SaveChanges();
                        }
                        // MessageBox.Show(count + "");

                        // Do further processing.
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message + "");
                    }
                }
            }
            //});
            MessageBox.Show("Done ! We Found  " + postcount + "  new posts");
            label6.Text = "";



        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }



        //Add Keyword
        private void button6_Click(object sender, EventArgs e)
        {


            if (listView1.SelectedItems.Count==0)
            {
                MessageBox.Show("Please choose  criteria");
                return;
            }
            string key = "";
            int criteriaId = int.Parse(listView1.SelectedItems[0].SubItems[0].Text);

            

            keyword keyword = new keyword();
            if (textBox4.Text == null)
            {
                MessageBox.Show("Please Empty values not allowed");
                return;
            }

            key = textBox4.Text;
            keyword.keywords = key;
            keyword.cirteriaId = criteriaId;

            if (fbEntities.keywords.Any(c => c.keywords.Trim().ToLower() == key.ToLower().Trim()))
            {
                MessageBox.Show("This Keyword already exist !");
                return;

            }

            fbEntities.keywords.Add(keyword);
            if (fbEntities.SaveChanges() == 1)
                MessageBox.Show("Added Sucessfully ");
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }



        //Add page
        private void button5_Click(object sender, EventArgs e)
        {



            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please choose  criteria");
                return;
            }



            if (
                   string.IsNullOrWhiteSpace(textBox3.Text)
                || string.IsNullOrWhiteSpace(textBox5.Text)
                || string.IsNullOrWhiteSpace(textBox7.Text)
                || string.IsNullOrWhiteSpace(textBox8.Text)
                || string.IsNullOrWhiteSpace(textBox9.Text)

                )
            {
                MessageBox.Show("Please make sure that you inserted all values");
                return;
            }
           

            string pageid = "";
            string pagename = "";
            int criteriaId = int.Parse(listView1.SelectedItems[0].SubItems[0].Text);

            page page = new page();

            if (string.IsNullOrWhiteSpace(textBox3.Text) || string.IsNullOrWhiteSpace(textBox5.Text))
            {
                MessageBox.Show("Please Empty values not allowed");
                return;
            }
            if (fbEntities.pages.Any(c => c.pagefbid.Trim().ToLower() == textBox3.Text.ToLower().Trim()))
            {
                MessageBox.Show("This Page already exist !");
                return;

            }
            pageid = textBox3.Text;
            pagename = textBox5.Text;
            page.pagefbid = pageid;
            page.pagename = pagename;
            page.category = textBox7.Text;
            page.location = textBox8.Text;
            page.pageurl = textBox9.Text;
            page.criteriaId = criteriaId;
            


            fbEntities.pages.Add(page);
            if (fbEntities.SaveChanges() == 1)
                MessageBox.Show("Added Sucessfully ");



        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }



        //Add Criteria
        private void button7_Click(object sender, EventArgs e)
        {

            fbEntities = new FBNewsDbEntities();
            criteria criteria = new criteria();
            criteria.criterianame = textBox6.Text;
            fbEntities.criterias.Add(criteria);
            fbEntities.SaveChanges();
            getcriterias();



        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please choose  criteria");
                return;
            }
            getpages();
            getKeywords();

        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listView3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please choose criteria");
                return;
            }
            deletecriteria();
        }


        private void deletecriteria()
        {
            int criteriaId = int.Parse(listView1.SelectedItems[0].SubItems[0].Text);
            fbEntities = new FBNewsDbEntities();
            var dcriteria = fbEntities.criterias.Where(c => c.id == criteriaId).SingleOrDefault();
            var dkeywords = fbEntities.keywords.Where(c => c.cirteriaId == criteriaId).ToList();
            var dpages = fbEntities.pages.Where(c => c.criteriaId == criteriaId).ToList();
            foreach(keyword keyword in dkeywords)
            {
                if (keyword != null)
                {
                    fbEntities.keywords.Remove(keyword);
                    fbEntities.SaveChanges();
                }
            }

            foreach (page page in dpages)
            {
                if (page != null)
                {
                    fbEntities.pages.Remove(page);
                    fbEntities.SaveChanges();
                }
            }

            
            if (dcriteria != null)
            {
                fbEntities.criterias.Remove(dcriteria);
                fbEntities.SaveChanges();
            }
            getcriterias();
            clearlist();
            
        }

        private void clearlist()
        {

            listView2.View = View.Details;
            listView2.GridLines = true;
            listView2.FullRowSelect = true;
            listView2.Clear();
            listView3.View = View.Details;
            listView3.GridLines = true;
            listView3.FullRowSelect = true;
            listView3.Clear();

        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please choose page to delete ");
                return;
            }
            int pageid = int.Parse(listView2.SelectedItems[0].SubItems[0].Text);
            var page = fbEntities.pages.Where(c => c.id == pageid).SingleOrDefault();
            fbEntities.pages.Remove(page);
            fbEntities.SaveChanges();
            getpages();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (listView3.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please choose keyword to delete ");
                return;
            }
            int keywordid = int.Parse(listView3.SelectedItems[0].SubItems[0].Text);
            var keyword = fbEntities.keywords.Where(c => c.id == keywordid).SingleOrDefault();
            fbEntities.keywords.Remove(keyword);
            fbEntities.SaveChanges();
            getKeywords();
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {

        }

        private void button12_Click(object sender, EventArgs e)
        {


            string filePath = string.Empty;
            string fileExt = string.Empty;
            OpenFileDialog file = new OpenFileDialog(); //open dialog to choose file  
            if (file.ShowDialog() == System.Windows.Forms.DialogResult.OK) //if there is a file choosen by the user  
            {
                filePath = file.FileName; //get the path of the file  
                fileExt = Path.GetExtension(filePath); //get the file extension  
                if (fileExt.CompareTo(".xls") == 0 || fileExt.CompareTo(".xlsx") == 0)
                {
                    try
                    {
                        DataTable dtExcel = new DataTable();
                    
                        ReadExcel(filePath, fileExt); //read excel file  
                      //  dataGridView1.Visible = true;
                      //  dataGridView1.DataSource = dtExcel;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message.ToString());
                    }
                }
                else
                {
                    MessageBox.Show("Please choose .xls or .xlsx file only.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error); //custom messageBox to show error  
                }
            }  
        }

        public void ReadExcel(string fileName, string fileExt)
        {
            int count = 0;
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please choose  criteria");
                return;
            }

            int criteriaId = int.Parse(listView1.SelectedItems[0].SubItems[0].Text);
         
            
            bool first=true;
            string conn = string.Empty;
            string exceltype = string.Empty;
            DataTable dtexcel = new DataTable();
            if (fileExt.CompareTo(".xls") == 0)
            {
                conn = @"provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileName + ";Extended Properties='Excel 8.0;HRD=Yes;IMEX=1';"; //for below excel 2007  
                exceltype = "b2007";
            }
            else
            {
                conn = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileName + ";Extended Properties='Excel 12.0;HDR=NO';"; //for above excel 2007  
                exceltype = "a2007";
            }
            using (OleDbConnection con = new OleDbConnection(conn))
            {
                try
                {
                    OleDbDataAdapter oleAdpt = new OleDbDataAdapter("select * from [Sheet1$]", con); //here we read data from sheet1  
                    oleAdpt.Fill(dtexcel); //fill excel data into dataTable  
                    foreach (DataRow row in dtexcel.Rows)
                    {
                        if(exceltype=="a2007"&&first)
                        {
                            first = false;
                            continue;
                          
                       
                        }
                        var  pageid= row[0].ToString() ;
                        var pagename = row[1].ToString();
                        var location = row[2].ToString();
                        var category = row[3].ToString();
                        var pageurl = row[4].ToString();
                        Addpage(pageid, pagename, category, location, pageurl, criteriaId);

                        //criteriaId	location	category	pageurl

                    }
                    //MessageBox.Show(dtexcel.Columns + "");
                }
                catch (Exception ex) {
                    
                    count++;
                }
            }
            if( count==0)
            MessageBox.Show(" All Pages Added Sucessfully ");
            else
                MessageBox.Show(" Some Pages Added Sucessfully ");

           // return dtexcel;
        }
  
        public void Addpage(string pageid,string pagename,string pagecategory,string pagelocation,string pageurl,int criteriaId)
        {
            page page = new page();

           
           
            page.pagefbid = pageid;
            page.pagename = pagename;
            page.category = pagecategory;
            page.location = pagelocation;
            page.pageurl = pageurl;
            page.criteriaId = criteriaId;



            fbEntities.pages.Add(page);
            fbEntities.SaveChanges();
           // if (fbEntities.SaveChanges() == 1)
              //  MessageBox.Show("Added Sucessfully ");
        }


    }
}
