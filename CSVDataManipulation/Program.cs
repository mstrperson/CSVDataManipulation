using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CSVDataManipulation
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            ExtendedCSV ext = new ExtendedCSV(new FileStream("C:\\temp\\test1.csv", FileMode.Open), new List<string>() { "id" });
            CSV other = new CSV(new FileStream("C:\\temp\\test2.csv", FileMode.Open));

            ext.Merge(other);
            ext.Save("C:\\temp\\testOut.csv");
            Console.WriteLine("Done!");
            Console.ReadKey();
        }

        public static void Flatten()
        {
            List<String> files = Directory.EnumerateFiles(String.Format("/Users/{0}/Documents/toFlatten/", Environment.UserName)).ToList();

            if (!Directory.Exists(String.Format("/Users/{0}/Documents/flattened", Environment.UserName)))
                Directory.CreateDirectory(String.Format("/Users/{0}/Documents/flattened", Environment.UserName));

            List<String> uniqueFields = new List<string>();

            if (File.Exists(String.Format("/Users/{0}/Documents/toFlatten/.flatten.conf", Environment.UserName)))
            {
                StreamReader reader =
                    new StreamReader(
                        new FileStream(
                            String.Format("/Users/{0}/Documents/toFlatten/.flatten.conf", Environment.UserName),
                            FileMode.Open));

                String fields = reader.ReadLine();
                uniqueFields.AddRange(fields.Split('\t'));
            }
            else
            {
                // ask the user.
                Console.WriteLine("What are the unique fields?  [Enter one at a time.  '*' to End.");
                String field = "";
                while (true)
                {
                    Console.Write(">> ");
                    field = Console.ReadLine();
                    if (field.Equals("*"))
                        break;
                    else
                        uniqueFields.Add(field);
                }
            }

            foreach (string fileName in files)
            {
                if (!fileName.EndsWith(".csv", StringComparison.CurrentCultureIgnoreCase)) continue;
                Console.WriteLine(fileName);
                FileStream fileStream = new FileStream(fileName, FileMode.Open);
                ExtendedCSV extendedCSV = new ExtendedCSV(fileStream, uniqueFields);
                CSV output = extendedCSV.SquashRows();
                string outfilename = fileName.Replace("toSquash", "flattened");
                output.Save(outfilename);
                Console.WriteLine("Flattened: {0}", outfilename);
            }

            Console.WriteLine("Done!");
            Console.ReadKey();
        }
    }
}
