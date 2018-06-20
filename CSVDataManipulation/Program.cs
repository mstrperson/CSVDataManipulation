using System;
using System.Collections.Generic;
using System.IO;

namespace CSVDataManipulation
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            FileStream fileStream = new FileStream("/Users/jcox/Documents/test.csv", FileMode.Open);
            ExtendedCSV extendedCSV = new ExtendedCSV(fileStream, new List<string>() { "id" });

            CSV output = extendedCSV.SquashRows();
            output.Save("/Users/jcox/Documents/testoutput.csv");

        }
    }
}
