using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Minimization
{
    class DFA
    {
        public string states { get; set; }
        public string input_symbols { get; set; }
        public Dictionary<string, Dictionary<string, string>> transitions { get; set; }
        public string initial_state { get; set; }
        public string final_states { get; set; }
        public DFA Minimize_DFA()
        {
            char[] Remove = new char[] { '{', '}', '\'' };
            HashSet<string> FinalStates = string.Concat(this.final_states.Split(Remove)).Split(',').ToHashSet();
            Dictionary<string, Dictionary<string, string>> DFA_Graph = transitions;
            //Remove non-reachable states with BFS
            HashSet<string> Reachable = new HashSet<string>(DFA_Graph.Count);
            Queue<string> BFS = new Queue<string>(DFA_Graph.Count);
            BFS.Enqueue(initial_state);
            Reachable.Add(initial_state);
            while (BFS.Count > 0)
            {
                string Start = BFS.Dequeue();
                foreach(string symbol in DFA_Graph[Start].Keys)
                    if (!Reachable.Contains(DFA_Graph[Start][symbol]))
                    {
                        BFS.Enqueue(DFA_Graph[Start][symbol]);
                        Reachable.Add(DFA_Graph[Start][symbol]);
                    }
            }

            Dictionary<string, HashSet<string>> Set = new Dictionary<string, HashSet<string>>(2);
            foreach(string State in Reachable)
            {
                if (FinalStates.Contains(State))
                {
                    if (!Set.ContainsKey("2"))//final
                        Set["2"] = new HashSet<string>(DFA_Graph.Count);
                    Set["2"].Add(State);
                }
                else
                {
                    if (!Set.ContainsKey("1"))//for non-finlal
                        Set["1"] = new HashSet<string>(DFA_Graph.Count);
                    Set["1"].Add(State);
                }
            }
            //start algorithm    
            bool finish = false;
            while (!finish)
            {
                Dictionary<string, HashSet<string>> Update_Set = new Dictionary<string, HashSet<string>>(DFA_Graph.Count);
                foreach(string Group in Set.Keys)
                {
                    foreach (string State in Set[Group])
                    {
                        StringBuilder NameOfNewSet = new StringBuilder();
                        NameOfNewSet.Append(Group);
                        foreach (string Symbol in DFA_Graph[State].Keys)
                            foreach (string set in Set.Keys)
                                if (Set[set].Contains(DFA_Graph[State][Symbol]))
                                {
                                    NameOfNewSet.Append(set);
                                    break;
                                }
                        if (!Update_Set.ContainsKey(NameOfNewSet.ToString()))
                            Update_Set[NameOfNewSet.ToString()] = new HashSet<string>(DFA_Graph.Count);
                        Update_Set[NameOfNewSet.ToString()].Add(State);
                    }
                }
                if (Update_Set.Count == Set.Count)
                {
                    finish = true;
                    Set.Clear();
                    foreach (HashSet<string> value_set in Update_Set.Values)
                        Set[string.Join("", value_set.OrderBy(x => x))] = value_set;
                }
                else
                {
                    Set.Clear();
                    int Number_Set = 1;
                    foreach (HashSet<string> value_set in Update_Set.Values)
                    {
                        Set[Number_Set.ToString()] = value_set;
                        Number_Set++;
                    }
                }
            }
            //for creating object of Min DFA
            DFA MinDFA = new DFA();
            MinDFA.transitions = new Dictionary<string, Dictionary<string, string>>(this.transitions.Count);
            MinDFA.input_symbols = input_symbols;
            List<string> Min_States = new List<string>(Set.Count);
            List<string> Min_Final = new List<string>(Set.Count);
            foreach(string set in Set.Keys)
            {
                MinDFA.transitions[set] = new Dictionary<string, string>(this.transitions.Values.Count);
                Min_States.Add("'" + set + "'");
                if (Set[set].Contains(this.initial_state))
                    MinDFA.initial_state = set;
                foreach (string state in FinalStates)
                    if (Set[set].Contains(state))
                    {
                        Min_Final.Add("'" + set + "'");
                        break;
                    }
                string First_State = Set[set].First();
                foreach(string symbol in DFA_Graph[First_State].Keys)
                    foreach (string group in Set.Keys)
                        if (Set[group].Contains(DFA_Graph[First_State][symbol]))
                        {
                            MinDFA.transitions[set][symbol] = group;
                            break;
                        }
            }
            MinDFA.states = "{" + string.Join(",", Min_States.OrderBy(x => x)) + "}";
            MinDFA.final_states = "{" + string.Join(",", Min_Final.OrderBy(x => x)) + "}";
            return MinDFA;
        }
    }
    internal class Program
    {
        static void Main()
        {
            string address = Console.ReadLine();
            address = @"" + address;
            string text = File.ReadAllText(address);
            DFA dfa = JsonSerializer.Deserialize<DFA>(text);
            DFA Min_dfa = dfa.Minimize_DFA();
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json_DFA = JsonSerializer.Serialize(Min_dfa, options);
            json_DFA = System.Text.RegularExpressions.Regex.Unescape(json_DFA);
            File.WriteAllText(@"E:\first-project_TLA\TLA01-Projects\SDFA.json", json_DFA);
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.WorkingDirectory = @"E:\first-project_TLA\TLA01-Projects";
            p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = false;
            p.StartInfo.RedirectStandardInput = true;
            p.Start();
            p.StandardInput.WriteLine(@"python main.py SDFA.json");
            p.Close();
        }
    }
}
