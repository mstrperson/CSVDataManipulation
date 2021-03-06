﻿using System;
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
        /// Initializes a new instance of the <see cref="T:CSVDataManipulation.ExtendedCSV"/> class.
        /// </summary>
        /// <param name="toExtend">To extend.</param>
        /// <param name="uidfields">Uidfields.</param>
        public ExtendedCSV(CSV toExtend, List<string> uidfields) : base(toExtend.Data)
        {
            this.UniqueFields = uidfields;
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
        /// Find the *first* row with specified primaryKey.
        /// Ideally, this should be the *unique* row with the given primaryKey,
        /// but this method does not check that.
        /// 
        /// throws <see cref="System.Collections.Generic.KeyNotFoundException"/> if no match is found.
        /// throws <see cref="InvalidDataException"/> if the primary key does not contain the appropriate unique fields.
        /// </summary>
        /// <returns>The the row that matches the given primary key or throws <see cref="System.Collections.Generic.KeyNotFoundException"/>.</returns>
        /// <param name="primaryKey">Primary key.</param>
        public Dictionary<string, string> Find(Dictionary<string, string> primaryKey)
        {
            foreach(Dictionary<string, string> row in this._Data)
            {
                bool match = true;
                foreach(String pk in UniqueFields)
                {
                    if (!primaryKey.ContainsKey(pk)) 
                        throw new InvalidDataException("Provided primaryKey does not contain a required field.");
                    
                    if(!row[pk].Equals(primaryKey[pk]))
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                    return row;
            }

            throw new KeyNotFoundException("No row corresponds to the given primary key.");
        }

        new public void Remove(Dictionary<String, String> primaryKey)
        {
            try
            {
                Dictionary<string, string> match = this.Find(primaryKey);
                this._Data.Remove(match);
            }
            catch (KeyNotFoundException)
            {
                
            }
            catch(InvalidDataException)
            {
                
            }
        }


        #region flatten file
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

        #endregion // flatten

        #region merge documents

        /// <summary>
        /// Merge this ExtendedCSV with another CSV.  
        /// The merger looks for matching unique field rows in the /other/ csv and merges them into this one.
        /// 
        /// </summary>
        /// <param name="other">CSV with additional data.</param>
        public void Merge(CSV other)
        {
            foreach (Dictionary<string, string> rowA in this.Data)
            {
                foreach (Dictionary<string, string> rowB in other.Data)
                {
                    if(CompareUniqueFields(rowA, rowB))
                    {
                        foreach(String column in rowB.Keys)
                        {
                            if(!rowA.ContainsKey(column))
                            {
                                // add the missing data to this sheet.
                                rowA.Add(column, rowB[column]);
                            }
                            else if(String.IsNullOrWhiteSpace(rowA[column]))
                            {
                                rowA[column] = rowB[column];
                            }
                            else if(!String.IsNullOrWhiteSpace(rowB[column]) && !rowA[column].Equals(rowB[column]))
                            {
                                // check for conflicting data.
                                rowA[column] = ConflictRule.Resolve(rowA, rowB, column);
                            }
                        }
                    }
                }
            }
        }

        public void GetDataColumnsFrom(ExtendedCSV other, List<String> columnsToPull)
        {
            foreach(Dictionary<string, string> sourceRow in this._Data)
            {
                Dictionary<string, string> otherRow;
                try
                {
                    otherRow = other.Find(sourceRow);
                }
                catch(KeyNotFoundException)
                {
                    // ignore

                    // TODO:  Don't ignore this...
                    continue;
                }

                foreach(string column in columnsToPull)
                {
                    if (!otherRow.ContainsKey(column))
                        continue;

                    if (sourceRow.ContainsKey(column))
                        sourceRow[column] = otherRow[column];
                    else
                        sourceRow.Add(column, otherRow[column]);
                }
            }
        }

        #endregion // merge documents

        #region Comparison

        /// <summary>
        /// Compares the other csv to this csv and returns a CSV containing all 
        /// of the rows that are present in the Other but are missing from this csv.
        /// </summary>
        /// <returns>The missing rows from.</returns>
        /// <param name="other">Other.</param>
        public CSV GetMissingRowsFrom(CSV other)
        {
            CSV output = new CSV();

            foreach(Dictionary<String, String> row in other.Data)
            {
                bool found = false;
                foreach(Dictionary<string, string> checkRow in this.Data)
                {
                    if(CompareUniqueFields(checkRow, row))
                    {
                        found = true;
                        break;
                    }
                }

                if(!found)
                {
                    output.Add(row);
                }
            }

            return output;
        }


        /// <summary>
        /// Gets the extra rows from this csv which are not present in the other csv.
        /// </summary>
        /// <returns>The extra rows from.</returns>
        /// <param name="other">Other.</param>
        public CSV GetExtraRowsFrom(CSV other)
        {
            CSV output = new CSV();
            foreach (Dictionary<String, String> row in this.Data)
            {
                bool found = false;
                foreach (Dictionary<string, string> checkRow in other.Data)
                {
                    if (CompareUniqueFields(checkRow, row))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    output.Add(row);
                }
            }

            return output;
        }

        public CSV PullRowsMatchingPrimaryKeysWith(CSV other)
        {
            CSV output = new CSV();

            foreach(Dictionary<String, String> row in other.Data)
            {
                try
                {
                    output.Add(this.Find(row));
                }
                catch(KeyNotFoundException)
                {
                    continue;
                }
                catch(InvalidDataException)
                {
                    continue;
                }
            }

            return output;
        }

        #endregion // Comparison


        public void NormalizeColumns(INormalizationRule rule, List<String> columns)
        {
            rule.Normalize(ref _Data, columns);
        }

    }
}
