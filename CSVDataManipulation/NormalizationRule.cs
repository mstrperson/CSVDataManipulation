using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

    public class MACAddressNormalizationRule : INormalizationRule
    {
        public bool Capitalize { get; set; }
        public enum MacSeparator
        {
            None = 0,
            Colon = 1,
            Dash = 2
        }

        private String[] separators = { "", ":", "-" };

        public MacSeparator Separator { get; set; }

        public void Normalize(ref List<Dictionary<string, string>> data, List<string> columns)
        {
            Regex macaddress = new Regex(MatchPattern);
            foreach(Dictionary<String, String> row in data)
            {
                foreach(String column in columns)
                {
                    if (macaddress.IsMatch(row[column]))
                        continue;

                    String stripped = "";
                    Regex digit = new Regex("[0-9a-fA-F]");
                    foreach(char ch in row[column])
                    {
                        if(digit.IsMatch("" + ch))
                        {
                            stripped += ch;
                        }
                    }

                    stripped = Capitalize ? stripped.ToUpperInvariant() : stripped.ToLowerInvariant();

                    if (stripped.Length != 12)
                        throw new FormatException("Invalid MAC Address");


                    String formatted = "";
                    for (int i = 0; i < 12; i+=2)
                    {
                        formatted += stripped[i];
                        formatted += stripped[i + 1];
                        if (i < 9) formatted += separators[(int)Separator];
                    }

                    row[column] = formatted;
                }
            }
        }

        protected string MatchPattern
        {
            get
            {
                string letter = Capitalize ? "A-F" : "a-F";
                if(Separator == MacSeparator.None)
                {
                    return "^[" + letter + "0-9]{12}$";
                }

                string separator = Separator == MacSeparator.Colon ? ":" : "-";

                return "^([" + letter + "0-9]{2}" + separator + "){5}[" + letter + "0-9]{2}$";
            }
        }
    }
}
