using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Diagnostics;

namespace NFAtoDFA
{
    public class FA
    {
        public string states { get; set; }
        public string input_symbols { get; set; }
        public Dictionary<string, Dictionary<string, string>> transitions { get; set; }
        public string initial_state { get; set; }
        public string final_states { get; set; }
        public FA NFAtoDFA(Dictionary<string, Dictionary<string, List<string>>> NFA_Graph, string Symbols, HashSet<string> NFA_FinalStates)
        {
            HashSet<string> Check_NFA = new HashSet<string>(NFA_Graph.Count);
            Queue<string> V_NFA = new Queue<string>(NFA_Graph.Count);
            foreach (string V in NFA_Graph.Keys)
            {
                if (NFA_Graph[V].ContainsKey(""))
                {
                    Check_NFA = new HashSet<string>(NFA_Graph.Count);
                    Check_NFA.Add(V);
                    foreach (string v in NFA_Graph[V][""])
                    {
                        V_NFA.Enqueue(v);
                        Check_NFA.Add(v);
                    }
                    while (V_NFA.Count > 0)
                    {
                        string start = V_NFA.Dequeue();
                        if (NFA_Graph[start].ContainsKey(""))
                            foreach (string v in NFA_Graph[start][""])
                                if (!Check_NFA.Contains(v))
                                {
                                    V_NFA.Enqueue(v);
                                    Check_NFA.Add(v);
                                    NFA_Graph[V][""].Add(v);
                                }
                    }
                }
            }

            FA DFA = new FA();
            if (NFA_Graph[this.initial_state].ContainsKey(""))
                DFA.initial_state = this.initial_state + string.Join("", NFA_Graph[this.initial_state][""].OrderBy(x => x));////////////
            else
                DFA.initial_state = this.initial_state;
            List<string> DFA_FinalStates = new List<string>();
            List<string> DFA_States = new List<string>();

            Dictionary<string, Dictionary<string, string>> DFA_Graph = new Dictionary<string, Dictionary<string, string>>();
            List<string> DFA_State = new List<string>(NFA_Graph.Count);
            DFA_State.Add(this.initial_state);
            if (NFA_Graph[initial_state].ContainsKey(""))
                foreach (string v in NFA_Graph[initial_state][""])
                    DFA_State.Add(v);
            HashSet<string> Check_DFA = new HashSet<string>();
            Check_DFA.Add(string.Join("", DFA_State.OrderBy(x => x)));
            Queue<List<string>> V_DFA = new Queue<List<string>>();
            V_DFA.Enqueue(DFA_State);
            while (V_DFA.Count > 0)
            {
                DFA_State = V_DFA.Dequeue();
                DFA_States.Add("'" + string.Join("", DFA_State.OrderBy(x => x)) + "'");
                foreach (string V in DFA_State)
                    if (NFA_FinalStates.Contains(V))
                    {
                        DFA_FinalStates.Add("'" + string.Join("", DFA_State.OrderBy(x => x)) + "'");
                        break;
                    }
                DFA_Graph[string.Join("", DFA_State.OrderBy(x => x))] = new Dictionary<string, string>(Symbols.Length);
                foreach (char symbol in Symbols)
                {
                    HashSet<string> Next_DFA_State = new HashSet<string>(NFA_Graph.Count);
                    foreach (string V in DFA_State)
                        if (NFA_Graph[V].ContainsKey(symbol.ToString()))
                            foreach (string v in NFA_Graph[V][symbol.ToString()])
                                Next_DFA_State.Add(v);
                    if (Next_DFA_State.Count > 0)
                    {
                        List<string> Added_States = new List<string>(NFA_Graph.Count);
                        foreach (string V in Next_DFA_State)
                        {
                            if (NFA_Graph[V].ContainsKey(""))
                                foreach (string v in NFA_Graph[V][""])
                                    Added_States.Add(v);
                        }
                        foreach (string s in Added_States)
                            Next_DFA_State.Add(s);
                        string Next_State = string.Join("", Next_DFA_State.OrderBy(x => x));
                        if (!Check_DFA.Contains(Next_State))
                        {
                            Check_DFA.Add(Next_State);
                            V_DFA.Enqueue(Next_DFA_State.ToList());
                        }
                        DFA_Graph[string.Join("", DFA_State.OrderBy(x => x))][symbol.ToString()] = Next_State;
                    }
                    else
                    {
                        DFA_Graph[string.Join("", DFA_State.OrderBy(x => x))][symbol.ToString()] = "TRAP";
                        if (!DFA_Graph.ContainsKey("TRAP"))
                        {
                            DFA_States.Add("'" + "TRAP" + "'");
                            DFA_Graph["TRAP"] = new Dictionary<string, string>(Symbols.Length);
                            foreach (char s in Symbols)
                                DFA_Graph["TRAP"][s.ToString()] = "TRAP";
                        }
                    }
                }
            }
            DFA.input_symbols = this.input_symbols;
            DFA.final_states = string.Join(',', DFA_FinalStates);
            DFA.final_states = "{" + DFA.final_states + "}";
            DFA.states = string.Join(',', DFA_States);
            DFA.states = "{" + DFA.states + "}";
            DFA.transitions = DFA_Graph;
            return DFA;
        }
        public FA Standard_input()
        {
            char[] Remove = new char[] { '{', '}', ',', '\'' };
            char[] Remove2 = new char[] { '{', '}', '\'' };
            string Symbols = string.Concat(this.input_symbols.Split(Remove));
            Dictionary<string, Dictionary<string, List<string>>> NFA_Graph = new Dictionary<string, Dictionary<string, List<string>>>(transitions.Count);
            foreach (string state in transitions.Keys)
            {
                if (transitions[state].Count > 0)
                {
                    Dictionary<string, List<string>> Value = new Dictionary<string, List<string>>(Symbols.Length + 1);
                    foreach (string symbol in transitions[state].Keys)
                        Value[symbol] = string.Concat(transitions[state][symbol].Split(Remove2)).Split(',').ToList();
                    NFA_Graph[state] = Value;
                }
            }
            //check when DFA doesn't have final state
            HashSet<string> NFA_FinalStates = string.Concat(this.final_states.Split(Remove2)).Split(',').ToHashSet();
            return NFAtoDFA(NFA_Graph, Symbols, NFA_FinalStates);
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            string address = Console.ReadLine();
            address = @"" + address;
            string text = File.ReadAllText(address);
            FA NFA = JsonSerializer.Deserialize<FA>(text);
            FA DFA = NFA.Standard_input();
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json_DFA = JsonSerializer.Serialize(DFA, options);
            json_DFA = System.Text.RegularExpressions.Regex.Unescape(json_DFA);
            File.WriteAllText(@"E:\first-project_TLA\TLA01-Projects\DFA.json", json_DFA);
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.WorkingDirectory = @"E:\first-project_TLA\TLA01-Projects";
            p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = false;
            p.StartInfo.RedirectStandardInput = true;
            p.Start();
            p.StandardInput.WriteLine(@"python main.py DFA.json");
            p.Close();
        }
    }
}
