/*
This class will be used for reading and writing to files
*/

using System.IO;
using System.Windows.Forms;

namespace Station_Configuration_Utility
{
    class FileHandler
    {
        private string result;
        private string[] results;

        //read one line of text
        public string textReader(string txtFile)
        {
           try
            {
                StreamReader reader = new StreamReader(txtFile);
                result = reader.ReadLine();
                reader.Close();
                return result;
            }catch(FileNotFoundException)
            {
                MessageBox.Show("File Not Found!");
                return null;
            }
        }

        //write one line of text
        public void textWriter(string txtFile, string text)
        {
            try
            {
                StreamWriter writter = new StreamWriter(txtFile);
                writter.WriteLine(text);
                writter.Close();
                
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("File Not Found!");
            }
        }

        //clear all text
        public void clearText(string txtFile)
        {
            try
            {
                File.Create(txtFile).Close();
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("File Not Found!");
            }
        }

        //read all line of text
        public string[] textReaderAll(string txtFile)
        {
            try
            {
                results = File.ReadAllLines(txtFile);
                return results;
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("File Not Found!");
                return null;
            }
        }

        //write all lines of text
        public void textWriterAll(string txtFile, string[] text)
        {
            try
            {
                File.WriteAllLines(txtFile, text);               
            }
            catch (FileNotFoundException fnfe)
            {
                MessageBox.Show("File Not Found!");
            }

        }
    }
}
