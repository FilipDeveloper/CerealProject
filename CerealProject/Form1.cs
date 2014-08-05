using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace CerealProject
{
    public partial class Form1 : Form
    {
        public string indexUrl = "http://www.kashi.com";
        public Form1()
        {
            InitializeComponent();
        }

        static void Label1_TextChanged(string text)
        {

        }
        private void button1_Click(object sender, EventArgs e)
        {
            //Generate list of food types
            #region Generate list of food types

            label1.Text = "Generate list of food types...";
            this.Refresh();

            string pageSource = WorkerClass.getSourceCode(indexUrl + "/our-foods");
            int start = pageSource.IndexOf("submenu foods");
            int end = pageSource.IndexOf("</ul>", start);
            string TypeSource = pageSource.Substring(start, end - start);
            List<string> listFoodTypes = new List<string>();
            StringReader reader = new StringReader(TypeSource);
            while (true)
            {
                string line = reader.ReadLine();
                if (line != null)
                {
                    if (line.Contains("<a href"))
                    {
                        start = line.IndexOf("<a href") + 9;
                        end = line.IndexOf(" title") - 1;
                        string link = line.Substring(start, end - start);
                        listFoodTypes.Add(indexUrl + link);
                    }
                }
                else
                {
                    label1.Text = "";
                    break;
                }
            }
            #endregion
            //Download pages of all Kashi products
            #region Download pages

            List<string> listFilePaths = new List<string>();
            foreach (string link in listFoodTypes)
            {
                label1.Text = "Download pages of all Kashi products" + Environment.NewLine + Environment.NewLine + listFilePaths.Count.ToString() + " out of 87 pages downloaded";
                this.Refresh();

                string sourceCode = WorkerClass.getSourceCode(link);
                start = sourceCode.IndexOf("\"products-grid\"");
                end = sourceCode.IndexOf("</ul>", start);
                string productsGrid = sourceCode.Substring(start, end - start);
                reader = new StringReader(productsGrid);
                
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line != null)
                    {
                        if (line.Contains("<a href"))
                        {
                            start = line.IndexOf("<a href") + 9;
                            end = line.IndexOf("\">");
                            string productLink = line.Substring(start, end - start);
                            listFilePaths.Add(WorkerClass.downloadPage(indexUrl + productLink));
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            #endregion
            //Generate Kashi table
            #region Generate Kashi table

            label1.Text = "Generate Kashi table..." + Environment.NewLine + Environment.NewLine;
            this.Refresh();

            string delim = ",";
            
            //Title of columns
            string CSV_String = "Brand" + delim + "Manufacturer" + delim + "Type" + delim + "Name" + delim + "Serving Size" + delim + "Calories" + delim + "Total Fat"
                                + delim + "Sodium" + delim + "Fiber" + delim + "Sugar" + delim + "Protein" + delim + "Whole Grains" + delim + "Ingredients"
                                + delim + "Allegrens" + delim + "Nutrition Panle" + delim + "Product Image" + Environment.NewLine;

            //Entries
            OrderedDictionary CSV_Row = new OrderedDictionary();

            CSV_Row.Add("Brand", "");
            CSV_Row.Add("Manufacturer", "");
            CSV_Row.Add("Type", "");
            CSV_Row.Add("Name", "");
            CSV_Row.Add("Serving Size", "");
            CSV_Row.Add("Calories", "");
            CSV_Row.Add("Total Fat", "");
            CSV_Row.Add("Sodium", "");
            CSV_Row.Add("Fiber", "");
            CSV_Row.Add("Sugar", "");
            CSV_Row.Add("Protein", "");
            CSV_Row.Add("Whole Grains", "");
            CSV_Row.Add("Ingredients", "");
            CSV_Row.Add("Allergens", "");
            CSV_Row.Add("Nutrition Panel", "");
            CSV_Row.Add("Product Image", "");

            int rowCounter = 0; //to count and show how many rows are inserted in CSV string
            foreach (string filepath in listFilePaths)
            {
                generateCSVRow(CSV_Row, filepath);
                CSV_String = addCSVRow(CSV_Row, CSV_String);

                label1.Text = "Generate Kashi table..." + Environment.NewLine + Environment.NewLine + ++rowCounter;
            }

            //Saving CSV_String as CSV file

            if (WorkerClass.saveCSVfile("Kashi", CSV_String))
            {
                label1.Text = "Kashi table saved as Kashi.csv";
            }

            #endregion
        }

        

        private void generateCSVRow(OrderedDictionary CSV_Row, string filepath)
        {
            StreamReader stream_reader = new StreamReader(filepath);
            string WholePage = stream_reader.ReadToEnd();

            //Brand & Manufacturer

            CSV_Row["Brand"] = "Kashi";
            CSV_Row["Manufacturer"] = "Kashi";

            //Type

            int start = WholePage.IndexOf("og:url");
            int end = WholePage.IndexOf("/>", start);
            string strType = WholePage.Substring(start, end - start);
            start = strType.IndexOf("our-foods/") + 10;
            end = strType.IndexOf("/", start);
            CSV_Row["Type"] = strType.Substring(start, end - start).Replace('-',' ');

            //Name

            start = WholePage.IndexOf("<title>") + 7;
            end = WholePage.IndexOf("</title>", start);
            string value = "\"" + WholePage.Substring(start, end - start) + "\"";
            CSV_Row["Name"] = value;

            //Product Image

            start = WholePage.IndexOf("<img id=\"packImage\"");
            end = WholePage.IndexOf("/>", start);
            string strProductImage = WholePage.Substring(start, end - start);
            start = strProductImage.IndexOf("src=") + 5;
            end = strProductImage.IndexOf("\"", start);
            value = strProductImage.Substring(start, end - start);
            string column_title = "Product Image";
            CSV_Row[column_title] = value;

            //Nutrition Table

            start = WholePage.IndexOf("nutrition-wrapper");
            end = WholePage.IndexOf("</fieldset>", start);
            string nutritionTable = WholePage.Substring(start, end - start);

            StringReader string_reader = new StringReader(nutritionTable);
            int ind_li = 0;
            int ind_ing = 0;
            int ind_All = 0;
            int ind_npan = 0;
            string entry = "";
            while (true)
            {
                string line = string_reader.ReadLine();
                if (line == null)
                {
                    break;
                }
                if (line.Contains("<li>"))
                {
                    ind_li = 1;
                }
                if (ind_li == 1)
                {
                    entry += line;
                }
                if (line.Contains("</li>"))
                {
                    ind_li = 0;
                    start = entry.IndexOf("\">") + 2;
                    end = entry.IndexOf("<", start);
                    column_title = entry.Substring(start, end - start);
                    start = entry.IndexOf("</label>") + 8;
                    end = entry.IndexOf("</li>", start);
                    value = entry.Substring(start, end - start).Trim();
                    CSV_Row[column_title] = value;
                    entry = "";
                }

                //Nutrition Panel

                if (ind_npan == 0)
                {
                    if (line.Contains("img id = \"nutritionImage\""))
                    {
                        ind_npan = 1;
                        start = line.IndexOf("src=") + 5;
                        end = line.IndexOf("\" title");
                        column_title = "Nutrition Panel";
                        value = line.Substring(start, end - start);
                        CSV_Row[column_title] = value;
                    }
                }

                //Ingredients

                if (ind_ing == 0)
                {
                    if (line.Contains("<h4>Ingredients</h4>"))
                    {
                        ind_ing = 1;
                    }
                }
                if (ind_ing == 1)
                {
                    entry += line;
                    if (line.Contains("</div>"))
                    {
                        ind_ing = 2;
                        if (entry.Contains("<div>"))
                        {
                            start = entry.IndexOf("<div>") + 5;
                            end = entry.IndexOf("</div>", start);
                            CSV_Row["Ingredients"] = "\"" + entry.Substring(start, end - start) + "\"";
                            entry = "";
                        }
                        else
                        {
                            CSV_Row["Ingredients"] = "";
                            entry = "";
                        }
                    }
                }

                //Allergens

                if (ind_All == 0)
                {
                    if (line.Contains("<h4>Allergens</h4>"))
                    {
                        ind_All = 1;
                    }
                }
                if (ind_All == 1)
                {
                    entry += line;
                    if (line.Contains("</div>"))
                    {
                        ind_All = 2;
                        if (entry.Contains("<div>"))
                        {
                            start = entry.IndexOf("<div>") + 5;
                            end = entry.IndexOf("</div>", start);
                            CSV_Row["Allergens"] = "\"" + entry.Substring(start, end - start) + "\"";
                            entry = "";
                        }
                        else
                        {
                            CSV_Row["Allergens"] = "";
                            entry = "";
                        }
                    }
                }
            }

        }

        private string addCSVRow(OrderedDictionary CSV_Row, string CSV_String)
        {
            string delim = ",";
            CSV_String += CSV_Row["Brand"] + delim + CSV_Row["Manufacturer"] + delim + CSV_Row["Type"] + delim + CSV_Row["Name"]
                        + delim + CSV_Row["Serving Size"] + delim + CSV_Row["Calories"] + delim + CSV_Row["Total Fat"]
                        + delim + CSV_Row["Sodium"] + delim + CSV_Row["Fiber"] + delim + CSV_Row["Sugar"] + delim + CSV_Row["Protein"]
                        + delim + CSV_Row["Whole Grains"] + delim + CSV_Row["Ingredients"] + delim + CSV_Row["Allergens"]
                        + delim + CSV_Row["Nutrition Panel"] + delim + CSV_Row["Product Image"] + Environment.NewLine;
            return CSV_String;
        }
    }
}
