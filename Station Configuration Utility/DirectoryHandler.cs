/*
This class deals with all directory handling
*/

using System.Windows.Forms;
using System.IO;

namespace Station_Configuration_Utility
{
    class DirectoryHandler
    {
        FileHandler fh = new FileHandler();

        //Sets default directory
        public string setDirectory(string filePath)
        {            
            string result = fh.textReader(filePath);
            return result;
        }

        //Gets an array of file paths and get returns the names without extensions
        public string[] getDirectoryFilesNames(string filePath)
        {
            string fileName;
            string[] filePaths;
            
                filePaths = getDirectoryFilePaths(filePath);
                int size = filePaths.Length;
                string[] fileNames;
                fileNames = new string[size];
                int count = 0;
                foreach (string file in filePaths)
                {
                    fileName = Path.GetFileNameWithoutExtension(file);
                    fileNames[count] = fileName;
                    count++;
                }
                return fileNames;
        }

        //Return an array of file paths
        public string[] getDirectoryFilePaths(string filePath)
        {
            
            string[] filePaths;
            try
            {
                filePaths = Directory.GetFiles(filePath);
                
                return filePaths;
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("File Not Found");
            }
            return null;
        }
    }
}
