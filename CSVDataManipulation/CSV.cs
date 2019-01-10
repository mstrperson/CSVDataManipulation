using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;

/// <summary>
/// Summary description for CSV
/// </summary>
namespace CSVDataManipulation
{
    public class CSV : IEnumerable<Dictionary<String, String>>
    {
        /// <summary>
        /// A title or heading for the table.  If you want one...
        /// </summary>
        public String Heading
        { get; set; }

        protected List<Dictionary<String, String>> _Data;

        /// <summary>
        /// Readonly access to all of the rows of this table.
        /// </summary>
        public List<Dictionary<String, String>> Data
        {
            get { return _Data; }
        }

        /// <summary>
        /// Gets the Enumerator for iterrating over this CSV.
        /// Enumerates the Rows of this CSV.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Dictionary<string, string>> GetEnumerator()
        {
            return ((IEnumerable<Dictionary<string, string>>)Data).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Dictionary<string, string>>)Data).GetEnumerator();
        }

        /// <summary>
        /// Readonly access to the data in this table by row number.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Dictionary<String,String> this[int index]
        {
            get
            {
                return this.Data[index];
            }
        }

        protected static Regex Quoted = new Regex("^\"[^\"]*\"$");

        /// <summary>
        /// Initialize a blank CSV.
        /// </summary>
        /// <param name="heading"></param>
        public CSV(String heading = "")
        {
            Heading = heading;
            _Data = new List<Dictionary<string, string>>();
        }

        /// <summary>
        /// Initialize a CSV from a List of Dictionaries.
        /// </summary>
        /// <param name="data"></param>
        public CSV(List<Dictionary<String, String>> data)
        {
            Heading = "";
            _Data = data;
        }

        /// <summary>
        /// Open a CSV from a stream.
        /// </summary>
        /// <param name="inputStream"></param>
        public CSV(Stream inputStream)
        {
            Heading = "";
            _Data = new List<Dictionary<string, string>>();
            StreamReader reader = new StreamReader(inputStream);
            String line = reader.ReadLine();
            String[] headers = line.Split(',');
            while (!reader.EndOfStream)
            {
                Dictionary<String, String> row = new Dictionary<string, string>();
                line = reader.ReadLine();
                String[] values = line.Split(',');
                for (int i = 0; i < headers.Length && i < values.Length; i++)
                {
                    if (Quoted.IsMatch(values[i]))
                    {
                        values[i] = values[i].Substring(1, values[i].Length - 2);
                    }

                    while (row.ContainsKey(headers[i])) headers[i] = headers[i] + " ";

                    row.Add(headers[i], values[i]);
                }

                _Data.Add(row);
            }
        }

        /// <summary>
        /// Add a row to this CSV.
        /// Resets the AllKeys field.
        /// </summary>
        /// <param name="row"></param>
        public void Add(Dictionary<String, String> row)
        {
            _Data.Add(row);
            _AllKeys = new List<string>();
        }


        /// <summary>
        /// Check to see if a row is in this CSV.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public bool Contains(Dictionary<String, String> row)
        {
            for (int i = 0; i < _Data.Count; i++)
            {
                bool match = true;
                foreach (String key in row.Keys)
                {
                    if (!_Data[i].ContainsKey(key))
                    {
                        match = false;
                        break;
                    }

                    if (!_Data[i][key].Equals(row[key]))
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get a CSV containing all the entries of this CSV which do not correspond to entries in the Other CSV.
        /// </summary>
        /// <param name="Other"></param>
        /// <returns></returns>
        public CSV NotIn(CSV Other)
        {
            CSV newCSV = new CSV();

            List<String> CommonKeys = new List<string>();
            foreach (String key in AllKeys)
            {
                if (Other.AllKeys.Contains(key))
                {
                    CommonKeys.Add(key);
                }
            }

            foreach (Dictionary<String, String> row in _Data)
            {
                Dictionary<String, String> strippedRow = new Dictionary<string, string>();
                foreach (String key in CommonKeys)
                {
                    strippedRow.Add(key, row[key]);
                }

                if (!Other.Contains(strippedRow))
                {
                    newCSV.Add(row);
                }
            }

            return newCSV;
        }

        /// <summary>
        /// Remove a row from this CSV.
        /// </summary>
        /// <param name="row"></param>
        public void Remove(Dictionary<String, String> row)
        {
            #region Search
            int index = -1;
            bool foundMatch = false;
            do
            {
                foundMatch = false;
                for (int i = 0; i < _Data.Count; i++)
                {
                    bool match = true;
                    foreach (String key in row.Keys)
                    {
                        if (!_Data[i].ContainsKey(key))
                        {
                            match = false;
                            break;
                        }

                        if (!_Data[i][key].Equals(row[key]))
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                    {
                        index = i;
                        foundMatch = true;
                        break;
                    }
                }

                if (index != -1)
                    _Data.RemoveAt(index);
            } while (foundMatch);
            #endregion

            _AllKeys = new List<string>();
        }

        private List<String> _AllKeys;

        /// <summary>
        /// Get the list of all keys in this CSV.
        /// Not every row is guaranteed to have a value for every key.
        /// </summary>
        public List<String> AllKeys
        {
            get
            {
                if (_AllKeys == null || _AllKeys.Count == 0)
                {
                    _AllKeys = new List<string>();

                    foreach (Dictionary<String, String> row in _Data)
                    {
                        foreach (String key in row.Keys)
                        {
                            if (!_AllKeys.Contains(key))
                            {
                                _AllKeys.Add(key);
                            }
                        }
                    }
                }
                return _AllKeys;
            }
        }


        /// <summary>
        /// How many Columns are in this CSV?
        /// </summary>
        public int ColCount
        {
            get
            {
                return AllKeys.Count;
            }
        }

        /// <summary>
        /// How many rows are in this CSV?
        /// </summary>
        public int RowCount
        {
            get
            {
                return Data.Count;
            }
        }

        /// <summary>
        /// Save the CSV to a file.
        /// This method will delete an existing file with this name.
        /// </summary>
        /// <param name="fileName"></param>
        public void Save(String fileName)
        {
            if (File.Exists(fileName)) File.Delete(fileName);
            this.Save(new FileStream(fileName, FileMode.OpenOrCreate));
        }

        /// <summary>
        /// Save the CSV to a stream.
        /// </summary>
        /// <param name="output"></param>
        public void Save(Stream output)
        {
            StreamWriter writer = new StreamWriter(output);
            writer.AutoFlush = true;

            if (!Heading.Equals(""))
                writer.WriteLine(Heading);

            if (AllKeys.Count <= 0)
            {
                return;
            }

            writer.Write(AllKeys[0]);
            for (int i = 1; i < AllKeys.Count; i++)
            {
                writer.Write(",{0}", AllKeys[i]);
            }
            writer.WriteLine();

            foreach (Dictionary<String, String> row in _Data)
            {
                if (row.ContainsKey(AllKeys[0]))
                    writer.Write(row[AllKeys[0]]);
                for (int i = 1; i < AllKeys.Count; i++)
                {
                    writer.Write(",");
                    if (row.ContainsKey(AllKeys[i]))
                    {
                        writer.Write(row[AllKeys[i]]);
                    }
                }
                writer.WriteLine();
            }


            writer.Close();
        }

        /// <summary>
        /// Bulk add data from another CSV object.
        /// </summary>
        /// <param name="other"></param>
        public void Add(CSV other)
        {
            foreach (Dictionary<String, String> row in other.Data)
            {
                this.Add(row);
            }
        }

        /// <summary>
        /// Get all of the data in the requested column of the table.
        /// if an invalid header is given, throws an ArgumentOutOfRangeException.
        /// </summary>
        /// <param name="header">Column Header from this table.</param>
        /// <returns></returns>
        public List<String> GetColumn(String header)
        {
            if (!AllKeys.Contains(header)) throw new ArgumentOutOfRangeException("Invalid Header Name");

            List<String> column = new List<string>();
            foreach (Dictionary<String, String> row in Data)
            {
                column.Add(row[header]);
            }

            return column;
        }

        /// <summary>
        /// Get the first row matching the given headr, key pair.
        /// if no row under the given header contains the given key, 
        /// throws an ArgumentOutOfRangeException.
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetRow(String header, String key)
        {
            foreach (Dictionary<string, string> row in Data)
            {
                if (row[header].Equals(key))
                {
                    return row;
                }
            }

            throw new ArgumentOutOfRangeException(String.Format("{0} was not found under the {1} header.", key, header));
        }
    }
}
