using System;
using System.Collections.Generic;

namespace CSVDataManipulation
{
    public interface IConflictRule
    {
        String Resolve(Dictionary<String, String> dataA, Dictionary<String, String> dataB, String conflictColumn);
    }

    public class UserResolveConflictRule : IConflictRule
    {
        public string Resolve(Dictionary<string, string> dataA, Dictionary<string, string> dataB, string conflictColumn)
        {
            Console.WriteLine("Conflicting Data in column '{0}'.  Choose which data to keep.", conflictColumn);
            Console.WriteLine("1) {0}\n2) {1}\n3) *something else*", dataA[conflictColumn], dataB[conflictColumn]);
            Console.Write(">> ");

            String choice = Console.ReadLine();
            if (choice.Equals("1")) return dataA[conflictColumn];
            else if (choice.Equals("2")) return dataB[conflictColumn];
            else
            {
                Console.WriteLine("Enter a value to replace.");
                Console.Write(">> ");
                return Console.ReadLine();
            }
        }
    }

    public class YesBeatsNoConflictRule : IConflictRule
    {
        /// <summary>
        /// Check to see if Yes or No, and Yes wins!
        /// otherwise, hand off to a UserResolveConflictRule
        /// </summary>
        /// <returns>The resolve.</returns>
        /// <param name="dataA">Data a.</param>
        /// <param name="dataB">Data b.</param>
        /// <param name="conflictColumn">Conflict column.</param>
        public string Resolve(Dictionary<string, string> dataA, Dictionary<string, string> dataB, string conflictColumn)
        {
            if (dataA[conflictColumn].ToLower().Equals("n") && dataB[conflictColumn].ToLower().Equals("y")) return "y";
            if (dataB[conflictColumn].ToLower().Equals("n") && dataA[conflictColumn].ToLower().Equals("y")) return "y";

            UserResolveConflictRule rule = new UserResolveConflictRule();
            return rule.Resolve(dataA, dataB, conflictColumn);
        }
    }
}
