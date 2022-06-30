using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task13ConsoleAPI
{
    public class Csv
    {
        public List<List<string>> Rows = new List<List<string>>();

        public char SeparatorChar;

        public int MaxRowsInOneFile = 30000;

        public Csv(char separatorChar = ',')
        {
            SeparatorChar = separatorChar;
        }

        public void FileOpen(string path, bool createIfNotExist = false)
        {
            Rows.Clear();

            if (File.Exists(path))
            {
                OpenSingleFile(path);
            }
            else if (createIfNotExist)
            {
                Csv csv = new Csv();
                csv.FileSave(path);
            }
            else
            {
                string searchPattern = Path.GetFileName(path).Replace(".csv", "*");
                string searchPath = Path.GetDirectoryName(path);

                string[] files = Directory.GetFiles(searchPath, searchPattern);

                foreach (var file in files)
                {
                    OpenSingleFile(file);
                }
            }
        }

        private void OpenSingleFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new Exception("File Not exist!");
            }

            using (TextFieldParser parser = new TextFieldParser(filePath))
            {
                parser.TextFieldType = FieldType.Delimited;

                parser.SetDelimiters(SeparatorChar.ToString());

                while (!parser.EndOfData)
                {
                    Rows.Add(parser.ReadFields().ToList());
                }
            }
        }

        public void FileSave(string path)
        {
            if (!path.ToLower().EndsWith(".csv"))
            {
                path += ".csv";
            }

            var correctPath = Directory.GetParent(path).ToString();

            Directory.CreateDirectory(correctPath);

            FileSave(path, false);
        }

        private void FileSave(string path, bool appendFile)
        {
            var files = SeparateCsvToFiles(Rows, path);

            foreach (var file in files)
            {
                using (StreamWriter writer = new StreamWriter(file.Key, appendFile, Encoding.UTF8))
                {
                    foreach (var row in file.Value)
                    {
                        writer.WriteLine(BuildCsvRow(row));
                    }
                }
            }
        }

        private List<KeyValuePair<string, List<List<string>>>> SeparateCsvToFiles(List<List<string>> rows, string originalPath)
        {
            List<List<string>> rowsCopy = new List<List<string>>(rows);

            var files = new List<KeyValuePair<string, List<List<string>>>>();

            int counter = 1;
            Func<string> newpath = () => { return originalPath.Replace(".csv", counter++ + ".csv"); };

            if (rowsCopy.Count < MaxRowsInOneFile)
            {
                files.Add(new KeyValuePair<string, List<List<string>>>(originalPath, rowsCopy));
            }
            else
            {
                List<List<string>> fileRows;

                while (rowsCopy.Count > MaxRowsInOneFile)
                {
                    fileRows = rowsCopy.GetRange(0, MaxRowsInOneFile);
                    rowsCopy.RemoveRange(0, MaxRowsInOneFile);

                    files.Add(new KeyValuePair<string, List<List<string>>>(newpath(), fileRows));
                }

                fileRows = rowsCopy.GetRange(0, rowsCopy.Count());

                files.Add(new KeyValuePair<string, List<List<string>>>(newpath(), fileRows));
            }

            return files;
        }

        public void AddRow(List<string> cells)
        {
            Rows.Add(cells);
        }

        public void AddRow(params string[] cells)
        {
            Rows.Add(new List<string>(cells));
        }


        public void FileAppend(string path)
        {
            FileSave(path, true);
        }

        private string BuildCsvRow(List<string> rowCells)
        {
            StringBuilder builder = new StringBuilder();

            bool firstColumn = true;

            foreach (string value in rowCells)
            {
                if (value != null)
                {
                    if (!firstColumn)
                        builder.Append(SeparatorChar);

                    if (value.IndexOfAny(new char[] { '"', SeparatorChar }) != -1)
                    {
                        builder.AppendFormat("\"{0}\"", value.Replace("\"", "\"\""));
                    }
                    else
                    {
                        builder.Append(value);
                    }

                    firstColumn = false;
                }
            }
            return builder.ToString();
        }
    }
}
