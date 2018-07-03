using System;
using System.Collections.Generic;

namespace CSVDataManipulation
{
    public interface INormalizationRule
    {
        void Normalize(ref List<Dictionary<String, String>> data, List<String> columns);
    }


    public class SerialNumberNormalizationRule : INormalizationRule
    {
        public bool Capitalize { get; set; }

        /// <summary>
        /// Normalize the specified columns of the given data.
        /// Standardizes data as all caps or all lowercase depending on the status of the Capitalize property.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <param name="columns">Columns.</param>
        public void Normalize(ref List<Dictionary<string, string>> data, List<string> columns)
        {
            foreach(Dictionary<string, string> row in data)
            {
                foreach(String column in columns)
                {
                    row[column] = Capitalize ? row[column].ToUpperInvariant() : row[column].ToLowerInvariant();
                }
            }
        }
    }
}
