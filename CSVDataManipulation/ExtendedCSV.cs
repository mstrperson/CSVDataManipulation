using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace CSVDataManipulation
{
    public class ExtendedCSV : CSV
    {
        public List<String> UniqueFields
        {
            get;
            set;
        }

        protected IConflictRule conflictRule = new UserResolveConflictRule();

        public ExtendedCSV(Stream inputStream, List<string> uidfields = null) : base(inputStream)
        {
            UniqueFields = uidfields == null ? new List<string>() : uidfields;

        }

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

        public CSV SquashRows()
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

        public Dictionary<String,String> Merge(Dictionary<string, string> destination, Dictionary<string, string> row)
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
                        destination[key] = conflictRule.Resolve(destination, row, key);
                    
                }
            }

            return destination;
        }

        public CSV SquashRowsBad()
        {
            Dictionary<Dictionary<string, string>, Dictionary<string, string>> SquashedData = new Dictionary<Dictionary<string, string>, Dictionary<string, string>>();
            foreach(Dictionary<string, string> row in this.Data)
            {
                Dictionary<String, String> uid = new Dictionary<string, string>();
                Dictionary<String, String> data = new Dictionary<string, string>();

                // copy data from each of the UniqueFields into an identifying block of data
                foreach(String hdr in UniqueFields)
                {
                    uid.Add(hdr, row.ContainsKey(hdr) ? row[hdr] : "");
                }

                // copy the remaining data columns into the other half of the data block so that it can be compared/collapsed.
                foreach(string hdr in row.Keys)
                {
                    // only want the remaining data.
                    if(!UniqueFields.Contains(hdr))
                    {
                        data.Add(hdr, row[hdr]);
                    }
                }

                // if the uid data is new, just add this to the SquashedData list.
                if (!ContainsDictionaryKey(SquashedData, uid)) 
                    SquashedData.Add(uid, data);
                // Otherwise.... Do the squashing!
                else
                {
                    foreach(String key in data.Keys)
                    {
                        if (!SquashedData[uid].ContainsKey(key) && !String.IsNullOrWhiteSpace(data[key]))
                            SquashedData[uid].Add(key, data[key]);
                        else if (String.IsNullOrWhiteSpace(SquashedData[uid][key]))
                            SquashedData[uid][key] = data[key];
                        else if(!String.IsNullOrWhiteSpace(data[key]) && !data[key].Equals(SquashedData[uid][key]))
                        {
                            // Handle the conflict
                            SquashedData[uid][key] = conflictRule.Resolve(SquashedData[uid], data, key);
                        }
                    }
                }
            }

            // Recombine the squashed data
            List<Dictionary<String, String>> combined = new List<Dictionary<string, string>>();
            foreach(Dictionary<string, string> uid in SquashedData.Keys)
            {
                Dictionary<String, String> row = new Dictionary<string, string>();
                foreach(string key in uid.Keys)
                {
                    row.Add(key, uid[key]);
                }
                foreach (string key in SquashedData[uid].Keys)
                {
                    row.Add(key, SquashedData[uid][key]);
                }
                combined.Add(row);
            }

            return new CSV(combined);
        }

        private bool ContainsDictionaryKey(Dictionary<Dictionary<string, string>, Dictionary<string, string>> dict, Dictionary<string, string> key)
        {
            
            foreach(Dictionary<string, string> dictkey in dict.Keys )
            {
                bool isMatch = true;
                foreach(string subkey in dictkey.Keys)
                {
                    if (!key.ContainsKey(subkey) || !key[subkey].Equals(dictkey[subkey])) isMatch = false;
                }
                foreach (string subkey in key.Keys)
                {
                    if (!dictkey.ContainsKey(subkey) || !key[subkey].Equals(dictkey[subkey])) isMatch = false;
                }

                if (isMatch) return true;
            }

            return false;
        }
    }
}
