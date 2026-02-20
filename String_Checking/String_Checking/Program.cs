using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Linq;
using System.Text;

namespace String_Checking
{
    class FA
    {
        public string states { get; set; }
        public string input_symbols { get; set; }
        public Dictionary<string, Dictionary<string, string>> transitions { get; set; }
        public string initial_state { get; set; }
        public string final_states { get; set; }

        static Dictionary<string, Dictionary<string, List<string>>> Graph;
        public static string input;
        static HashSet<string> Final;
        static Dictionary<string, HashSet<int>> Check = new Dictionary<string, HashSet<int>>();
        public void standardization()
        {
            char[] Remove = new char[] { '{', '}', ',', '\'' };
            char[] Remove2 = new char[] { '{', '}', '\'' };
            string Symbols = string.Concat(this.input_symbols.Split(Remove));
            Graph = new Dictionary<string, Dictionary<string, List<string>>>(transitions.Count);
            foreach (string state in transitions.Keys)
            {
                if (transitions[state].Count > 0)
                {
                    Dictionary<string, List<string>> Value = new Dictionary<string, List<string>>(Symbols.Length + 1);
                    foreach (string symbol in transitions[state].Keys)
                        Value[symbol] = string.Concat(transitions[state][symbol].Split(Remove2)).Split(',').ToList();
                    Graph[state] = Value;
                }
            }
            //check when DFA doesn't have final state
            Final = string.Concat(this.final_states.Split(Remove2)).Split(',').ToHashSet();
        }
        public bool Accept(int index, string S)
        {
            if (!Check.ContainsKey(S))
            {
                Check[S] = new HashSet<int>();
                Check[S].Add(index);
            }
            else
            {
                if (Check[S].Contains(index))
                    return false;
                else
                    Check[S].Add(index);
            }
            if (index == input.Length)
            {
                if (Final.Contains(S))
                    return true;
            }
            else
            {
                if (Graph[S].ContainsKey(input[index].ToString()))
                {
                    foreach (string s in Graph[S][input[index].ToString()])
                        if (Accept(index + 1, s))
                            return true;
                }
            }
            if (!Graph[S].ContainsKey(""))
                return false;
            foreach (string s in Graph[S][""])
                if (Accept(index, s))
                    return true;
            return false;
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            string address = Console.ReadLine();
            address = @"" + address;
            string text = File.ReadAllText(address);
            FA fa = JsonSerializer.Deserialize<FA>(text);
            FA.input = Console.ReadLine();
            fa.standardization();
            if (fa.Accept(0, fa.initial_state))
                Console.WriteLine("Accepted");
            else
                Console.WriteLine("Rejected");
        }
    }
}
