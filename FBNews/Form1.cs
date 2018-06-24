using FBNews.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
        string token = "EAACEdEose0cBAFZCnQNbjlo3GgFpWH77upQyG5UfemInZBofGuQZA1OPf4dwVB6fPMRq1ZBOJQWJKgFhnabr9RYAxNvJm8K3y7g0L9E5DhdqWtgN7mU2SpZBVbaAcPQqu0LxhiLuVHnxAfwDX6Q5qlqe78sUZC4qDNcZBUqzOUM6jYopDmVrXdwmg6Qxnm1UW4ZD";
       // string Url = "https://graph.facebook.com/155869377766434/posts?limit=100&access_token=";
        List<page> pages;
        FBNewsDbEntities fbEntities;
        string[] keywords;
        string path;
        

        public Form1()
        {
            InitializeComponent();
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

            var criterias=  fbEntities.criterias.ToList();
            foreach (criteria criteria in criterias)
            {
                string[] arrcriterias = new string[4];
                arrcriterias[0] = criteria.id+"" ;
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
            listView2.Columns.Add("ID", 100);
            listView2.Columns.Add("Page Name", 100);

            var pages = fbEntities.pages.Where(c=>c.criteriaId==criteriaId).ToList();
            foreach (page page in pages)
            {
                string[] arrpages = new string[4];
                arrpages[0] = page.id + "";
                arrpages[1] = page.pagename;
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
            listView3.Columns.Add("Page Name", 100);

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


        private void button4_Click(object sender, EventArgs e)
        {
            int postcount = 0;
            fbEntities = new FBNewsDbEntities();
            label6.Text = "Loading";
            var criterias = fbEntities.criterias.ToList();
          //  Url = Url + token;



            foreach (criteria criteria in criterias)
            {

                pages = fbEntities.pages.Where(c => c.criteriaId == criteria.id).ToList() ;
                keywords = fbEntities.keywords.Where(c=>c.cirteriaId==criteria.id). Select(c => c.keywords).ToArray();

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
                                    if (message.ToLower().Contains(keyword.ToLower()) && !(fbEntities.FBNews.Any(c=>c.post_id==id)))
                                    {
                                        FBNew fb = new FBNew();
                                        fb.created_at = createdate;
                                        fb.message = message;
                                        fb.pagename = page.pagename;
                                        fb.post_id = id;
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

            MessageBox.Show("Done ! We Found  " +postcount+ "  new posts");
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

        private void button6_Click(object sender, EventArgs e)
        {
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

        private void button5_Click(object sender, EventArgs e)
        {
            string pageid = "";
            string pagename = "";
            int criteriaId = int.Parse( listView1.SelectedItems[0].SubItems[0].Text);

            page page = new page();

            if (string.IsNullOrWhiteSpace(textBox3.Text) ||string.IsNullOrWhiteSpace(textBox5.Text))
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
            page.pagename=pagename;
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

        private void button7_Click(object sender, EventArgs e)
        {

            fbEntities = new FBNewsDbEntities();
            criteria criteria = new criteria();
            criteria.criterianame = "Test";
            fbEntities.criterias.Add(criteria);
            fbEntities.SaveChanges();
            getcriterias();


           
        }

        private void button8_Click(object sender, EventArgs e)
        {
            getpages();
            getKeywords();

        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listView3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


    }
}
