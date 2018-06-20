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

        public ExtendedCSV(Stream inputStream, List<string> uidfields = null) : base(inputStream)
        {
            UniqueFields = uidfields == null ? new List<string>() : uidfields;

        }
    }
}
