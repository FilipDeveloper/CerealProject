using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Windows.Forms;

namespace CerealProject
{
    public class WorkerClass
    {
        public static string getSourceCode(string url)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            StreamReader reader = new StreamReader(resp.GetResponseStream());
            string sourceCode = reader.ReadToEnd();
            reader.Close();
            resp.Close();
            return sourceCode;
        }

        public static string downloadPage(string url)
        {
            string sourceCode = getSourceCode(url);
            string cwd = Directory.GetCurrentDirectory();
            int start = sourceCode.IndexOf("<title>") + 7;
            int end = sourceCode.IndexOf("</title>",start);
            
            if (!Directory.Exists(Path.Combine(cwd, "Kashi")))
            {
                Directory.CreateDirectory(Path.Combine(cwd, "Kashi"));
            }
            string filename = sourceCode.Substring(start, end - start);
            
            string path = Path.Combine(cwd, "Kashi", filename + ".html");
            StreamWriter sw = new StreamWriter(path);
            sw.Write(sourceCode);
            sw.Close();
            return path;
        }

        public static bool saveCSVfile(string cerealBrand, string CSV_String)
        {
            string cwd = Directory.GetCurrentDirectory();
            if (cerealBrand == "Kashi")
            {
                if (!Directory.Exists(Path.Combine(cwd, "Kashi")))
                {
                    Directory.CreateDirectory(Path.Combine(cwd, "Kashi"));
                }
                string path = Path.Combine(cwd, "Kashi", "Kashi.csv");

                StreamWriter sw = new StreamWriter(path);
                try
                {
                    sw.Write(CSV_String);
                }
                catch (IOException e)
                {
                    MessageBox.Show("Error occured while trying to save Kashi CSV file" + e.Source, "Error");
                }
                finally
                {
                    sw.Close();
                }
              
            }
            return true;
        }

    }
}
