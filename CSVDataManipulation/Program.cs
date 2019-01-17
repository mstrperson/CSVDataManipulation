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
            ExtendedCSV jamf = new ExtendedCSV(new FileStream("Z:\\Jason\\iPads\\jamf.csv", FileMode.Open), new List<string> { "Serial Number" })
            {
                ConflictRule = new PickTheFirstConflictRule()
            };
            CSV wasp = new CSV(new FileStream("Z:\\Jason\\iPads\\wasp.csv", FileMode.Open));

            jamf.Merge(wasp);
            jamf.Save("Z:\\Jason\\iPads\\jamfExtended.csv");

            CSV misingFromJamf = new CSV();

            foreach(string serial in wasp["Serial Number"])
            {
                if(!String.IsNullOrWhiteSpace(serial) && !jamf["Serial Number"].Contains(serial))
                {
                    Console.WriteLine("Jamf does not contain {0}.", serial);
                    misingFromJamf.Add(wasp["Serial Number", serial]);
                }
            }

            misingFromJamf.Save("Z:\\Jason\\iPads\\missingFromJamf.csv");

            Console.WriteLine("Done!");
            Console.ReadKey();
        }

        public static void CleanMissing()
        {
            ExtendedCSV extended = new ExtendedCSV(new FileStream("/Users/jcox/Documents/missing_lenovo.csv", FileMode.Open), new List<string>() { "WASP" });
            ExtendedCSV other = new ExtendedCSV(new FileStream("/Users/jcox/Documents/employee_lenovo.csv", FileMode.Open), new List<string>() { "WASP" });

            foreach (Dictionary<String, String> row in other.Data)
            {
                extended.Remove(row);
            }

            extended.Save("/Users/jcox/Documents/missing_lenovo_cleaned.csv");
        }

        public static void SerialNumberComparison(String missingFileName, String outputFileName)
        {
            ExtendedCSV extended = new ExtendedCSV(
                new FileStream(missingFileName, FileMode.Open), new List<string>() { "Serial No" });

            ExtendedCSV other = new ExtendedCSV(
                new FileStream("/Users/jcox/Documents/employee_laptop_serials.csv", FileMode.Open), new List<string>() { "Serial No" })
            {
                ConflictRule = new PickTheFirstConflictRule()
            };

            other.NormalizeColumns(new SerialNumberNormalizationRule() { Capitalize = true }, new List<String>() { "Serial No" });

            other = new ExtendedCSV(other.FlattenRows(), new List<string>() { "Serial No" });
            other.Save("/Users/jcox/Documents/employee_laptop_serials.csv");
            ExtendedCSV output = new ExtendedCSV(extended.PullRowsMatchingPrimaryKeysWith(other), new List<String>() { "Serial No" });
            output.GetDataColumnsFrom(other, new List<String>() { "First", "Last" });
            output.Save(outputFileName);
        }

        public static void ConsolidateData(String handsOnFileName, String exportedFileName, String mergedFileName)
        {

            ExtendedCSV extended = new ExtendedCSV(
                new FileStream(handsOnFileName, FileMode.Open), new List<string>() { "WASP" }
            );

            ExtendedCSV other = new ExtendedCSV(
                new FileStream(exportedFileName, FileMode.Open), new List<string>() { "WASP" }
            );

            extended.GetDataColumnsFrom(other, new List<string>() { "Serial No", "Model" });

            extended.Save(mergedFileName);
        }

        public static void Combine()
        {
            List<String> files = 
                Directory.EnumerateFiles("/Users/jcox/Documents/toCombine/").Where(
                    fn => fn.EndsWith(".csv", StringComparison.InvariantCultureIgnoreCase)).ToList();

            CSV output = new CSV();
            foreach(string fileName in files)
            {
                CSV toAdd = new CSV(new FileStream(fileName, FileMode.Open));
                output.Add(toAdd);
            }

            output.Save("/Users/jcox/Documents/combined.csv");
        }

        public static void Compare(String handsOnFileName, String exportedFileName, String missingFileName, String newFileName)
        {

            ExtendedCSV extended = new ExtendedCSV(
                new FileStream(handsOnFileName, FileMode.Open), new List<string>() { "WASP" }
            );

            CSV other = new CSV(new FileStream(exportedFileName, FileMode.Open));

            extended.GetMissingRowsFrom(other).Save(missingFileName);
            extended.GetExtraRowsFrom(other).Save(newFileName);
        }

        public static void Normalize()
        {
            ExtendedCSV extended = new ExtendedCSV(
                new FileStream("/Users/jcox/Documents/loanerLaptops.csv", FileMode.Open), new List<String>() { "WASP" });

            extended.NormalizeColumns(
                new MACAddressNormalizationRule()
                { Capitalize = false, Separator = MACAddressNormalizationRule.MacSeparator.None },
                new List<String>() { "Wifi", "Bluetooth", "Ethernet" });
            extended.NormalizeColumns(
                new SerialNumberNormalizationRule() { Capitalize = true },
                new List<String>() { "Serial", "Model" }
            );

            extended.Save("/Users/jcox/Documents/loanersCleaned.csv");
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

                ExtendedCSV extendedCSV = new ExtendedCSV(fileStream, uniqueFields)
                {
                    ConflictRule = new YesBeatsNoConflictRule() 
                                        { 
                                            NextRule = new UserResolveConflictRule() 
                                        }
                };

                CSV output = extendedCSV.FlattenRows();
                string outfilename = fileName.Replace("toFlatten", "flattened");
                output.Save(outfilename);
                Console.WriteLine("Flattened: {0}", outfilename);
            }

            Console.WriteLine("Done!");
            Console.ReadKey();
        }
    }
}
