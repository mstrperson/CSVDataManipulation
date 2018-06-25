using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace CSVDataManipulation
{

    /// <summary>
    /// Extention of the CSV class which can be used for various data manipulation.
    /// 
    /// Currently implements flattening of CSV files given a list of primary keys.
    /// </summary>
    public class ExtendedCSV : CSV
    {
        /// <summary>
        /// Gets or sets the list of unique fields.
        /// This might be the primary keys for the table.
        /// </summary>
        /// <value>The unique fields.</value>
        public List<String> UniqueFields
        {
            get;
            set;
        }

        /// <summary>
        /// The conflict rule.
        /// </summary>
        public IConflictRule ConflictRule;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:CSVDataManipulation.ExtendedCSV"/> class.
        /// </summary>
        /// <param name="inputStream">Input stream.</param>
        /// <param name="uidfields">Uidfields.</param>
        public ExtendedCSV(Stream inputStream, List<string> uidfields = null) : base(inputStream)
        {
            UniqueFields = uidfields == null ? new List<string>() : uidfields;

        }

        /// <summary>
        /// Compares the unique fields.
        /// </summary>
        /// <returns><c>true</c>, if unique fields was compared, <c>false</c> otherwise.</returns>
        /// <param name="rowA">Row a.</param>
        /// <param name="rowB">Row b.</param>
        protected bool CompareUniqueFields(Dictionary<string, string> rowA, Dictionary<String, String> rowB)
        {
            foreach (String uid in UniqueFields)
            {
                if (!rowA.ContainsKey(uid) || !rowB.ContainsKey(uid))
                    return false;
                if (!rowA[uid].Equals(rowB[uid]))
                    return false;

            }

            return true;
        }

        /// <summary>
        /// Squashs the rows.
        /// </summary>
        /// <returns>The rows.</returns>
        public CSV FlattenRows()
        {
            List<Dictionary<String, String>> output = new List<Dictionary<string, string>>();

            for (int i = 0; i < this.Data.Count; i++)
            {
                bool merged = false;
                for (int j = 0; j < output.Count; j++)
                {
                    if(CompareUniqueFields(this.Data[i], output[j]))
                    {
                        output[j] = Merge(output[j], this.Data[i]);
                        merged = true;
                    }
                }

                if (!merged)
                    output.Add(this.Data[i]);
            }

            return new CSV(output);
        }


        /// <summary>
        /// Merge the specified destination and row.
        /// </summary>
        /// <returns>The merge.</returns>
        /// <param name="destination">Destination.</param>
        /// <param name="row">Row.</param>
        protected Dictionary<String,String> Merge(Dictionary<string, string> destination, Dictionary<string, string> row)
        {
            List<string> keys = new List<string>();

            foreach (string key in destination.Keys)
            {
                keys.Add(key);
            }
            foreach(string key in keys)
            {
                if(!UniqueFields.Contains(key))
                {
                    if (String.IsNullOrWhiteSpace(destination[key]))
                        destination[key] = row[key];
                    if (!destination[key].Equals(row[key]) && !String.IsNullOrWhiteSpace(row[key]))
                        destination[key] = ConflictRule.Resolve(destination, row, key);
                    
                }
            }

            return destination;
        }

       
    }
}
