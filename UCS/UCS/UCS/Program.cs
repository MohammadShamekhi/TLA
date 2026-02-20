using System;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace UCS
{
    public class FA
    {
        public string states { get; set; }
        public string input_symbols { get; set; }
        public Dictionary<string, Dictionary<string, string>> transitions { get; set; }
        public string initial_state { get; set; }
        public string final_states { get; set; }

        public List<string> Final = new List<string>();
        public Dictionary<string, Dictionary<string, List<string>>> Graph = new Dictionary<string, Dictionary<string, List<string>>>();
        public HashSet<string> Symbols = new HashSet<string>();
        public void Standard()
        {
            char[] Remove = new char[] { '{', '}', '\'' };
            Final = string.Concat(this.final_states.Split(Remove)).Split(',').ToList();
            Symbols = string.Concat(this.input_symbols.Split(Remove)).Split(',').ToHashSet();
            foreach (string state in transitions.Keys)
            {
                   Dictionary<string, List<string>> Value = new Dictionary<string, List<string>>();
                   foreach (string symbol in transitions[state].Keys)
                       Value[symbol] = string.Concat(transitions[state][symbol].Split(Remove)).Split(',').ToList();
                   Graph[state] = Value;
            }
        }
    }
    internal class Program
    {
        static FA Star(FA NFA)
        {
            NFA.Standard();
            FA Star_NFA = new FA();
            Star_NFA.input_symbols = NFA.input_symbols;
            Dictionary<string, string> Encode = new Dictionary<string, string>(NFA.Graph.Count);
            int i = 0;
            foreach (string state in NFA.Graph.Keys)
            {
                Encode[state] = "q" + i.ToString();
                i++;
            }
            foreach (string state in NFA.Graph.Keys)
            {
                Star_NFA.Graph[Encode[state]] = NFA.Graph[state];
                foreach (string symbol in Star_NFA.Graph[Encode[state]].Keys)
                    for (int j = 0; j < Star_NFA.Graph[Encode[state]][symbol].Count; j++)
                        Star_NFA.Graph[Encode[state]][symbol][j] = Encode[Star_NFA.Graph[Encode[state]][symbol][j]];
            }
            NFA.initial_state = Encode[NFA.initial_state];
            string IS = "q" + i.ToString();
            string FS = "q" + (i + 1).ToString();
            Star_NFA.Graph[IS] = new Dictionary<string, List<string>>();
            Star_NFA.Graph[FS] = new Dictionary<string, List<string>>();
            Star_NFA.Graph[IS][""] = new List<string>(2) { NFA.initial_state, FS };
            Star_NFA.Graph[FS][""] = new List<string>(1) { IS };
            foreach (string final in NFA.Final)
            {
                if (!Star_NFA.Graph[Encode[final]].ContainsKey(""))
                    Star_NFA.Graph[final][""] = new List<string>(1);
                Star_NFA.Graph[Encode[final]][""].Add(FS);
            }
            Star_NFA.initial_state = IS;
            Star_NFA.final_states = "{'" + FS + "'}";
            Star_NFA.states = "{" + string.Join(',', Star_NFA.Graph.Keys.Select(x => "'" + x + "'")) + "}";
            Star_NFA.transitions = new Dictionary<string, Dictionary<string, string>>();
            foreach(string state in Star_NFA.Graph.Keys)
            {
                Star_NFA.transitions[state] = new Dictionary<string, string>();
                foreach(string symbol in Star_NFA.Graph[state].Keys)
                    Star_NFA.transitions[state][symbol] = "{" + string.Join(',', Star_NFA.Graph[state][symbol].Select(x => "'" + x + "'")) + "}";
            }
            return Star_NFA;
        }
        static FA Concatenation(FA NFA1, FA NFA2)
        {
            NFA1.Standard();
            NFA2.Standard();
            FA Concat_NFA = new FA();
            Concat_NFA.Symbols = NFA1.Symbols;
            foreach (string symbol in NFA2.Symbols)
                Concat_NFA.Symbols.Add(symbol);
            Concat_NFA.input_symbols = "{" + string.Join(',', Concat_NFA.Symbols.Select(x => "'" + x + "'")) + "}";
            Dictionary<string, string> Encode1 = new Dictionary<string, string>(NFA1.Graph.Count);
            Dictionary<string, string> Encode2 = new Dictionary<string, string>(NFA2.Graph.Count);
            int i = 0;
            foreach(string state in NFA1.Graph.Keys)
            {
                Encode1[state] = "q" + i.ToString();
                i++;
            }
            foreach(string state in NFA2.Graph.Keys)
            {
                Encode2[state] = "q" + i.ToString();
                i++;
            }
            foreach(string state in NFA1.Graph.Keys)
            {
                Concat_NFA.Graph[Encode1[state]] = NFA1.Graph[state];
                foreach(string symbol in Concat_NFA.Graph[Encode1[state]].Keys)
                    for (int j = 0; j < Concat_NFA.Graph[Encode1[state]][symbol].Count; j++)
                        Concat_NFA.Graph[Encode1[state]][symbol][j] = Encode1[Concat_NFA.Graph[Encode1[state]][symbol][j]];
            }
            foreach (string state in NFA2.Graph.Keys)
            {
                Concat_NFA.Graph[Encode2[state]] = NFA2.Graph[state];
                foreach (string symbol in Concat_NFA.Graph[Encode2[state]].Keys)
                    for (int j = 0; j < Concat_NFA.Graph[Encode2[state]][symbol].Count; j++)
                        Concat_NFA.Graph[Encode2[state]][symbol][j] = Encode2[Concat_NFA.Graph[Encode2[state]][symbol][j]];
            }
            string FS = "q" + i.ToString();
            NFA1.initial_state = Encode1[NFA1.initial_state];
            NFA2.initial_state = Encode2[NFA2.initial_state];
            Concat_NFA.initial_state = NFA1.initial_state;
            foreach (string final in NFA1.Final)
                if (!Concat_NFA.Graph[Encode1[final]].ContainsKey(""))
                    Concat_NFA.Graph[Encode1[final]][""] = new List<string>() { NFA2.initial_state };
                else
                    Concat_NFA.Graph[Encode1[final]][""].Add(NFA2.initial_state);
            foreach (string final in NFA2.Final)
                if (!Concat_NFA.Graph[Encode2[final]].ContainsKey(""))
                    Concat_NFA.Graph[Encode2[final]][""] = new List<string>() { FS };
                else
                    Concat_NFA.Graph[Encode2[final]][""].Add(FS);
            Concat_NFA.Graph[FS] = new Dictionary<string, List<string>>();
            Concat_NFA.final_states = "{'" + FS + "'}";
            Concat_NFA.states = "{" + string.Join(',', Concat_NFA.Graph.Keys.Select(x => "'" + x + "'")) + "}";
            Concat_NFA.transitions = new Dictionary<string, Dictionary<string, string>>();
            foreach (string state in Concat_NFA.Graph.Keys)
            {
                Concat_NFA.transitions[state] = new Dictionary<string, string>();
                foreach (string symbol in Concat_NFA.Graph[state].Keys)
                    Concat_NFA.transitions[state][symbol] = "{" + string.Join(',', Concat_NFA.Graph[state][symbol].Select(x => "'" + x + "'")) + "}";
            }
            return Concat_NFA;
        }
        static FA Union(FA NFA1, FA NFA2)
        {
            NFA1.Standard();
            NFA2.Standard();
            FA Union_NFA = new FA();
            Union_NFA.Symbols = NFA1.Symbols;
            foreach (string symbol in NFA2.Symbols)
                Union_NFA.Symbols.Add(symbol);
            Union_NFA.input_symbols = "{" + string.Join(',', Union_NFA.Symbols.Select(x => "'" + x + "'")) + "}";
            Dictionary<string, string> Encode1 = new Dictionary<string, string>(NFA1.Graph.Count);
            Dictionary<string, string> Encode2 = new Dictionary<string, string>(NFA2.Graph.Count);
            int i = 0;
            foreach (string state in NFA1.Graph.Keys)
            {
                Encode1[state] = "q" + i.ToString();
                i++;
            }
            foreach (string state in NFA2.Graph.Keys)
            {
                Encode2[state] = "q" + i.ToString();
                i++;
            }
            foreach (string state in NFA1.Graph.Keys)
            {
                Union_NFA.Graph[Encode1[state]] = NFA1.Graph[state];
                foreach (string symbol in Union_NFA.Graph[Encode1[state]].Keys)
                    for (int j = 0; j < Union_NFA.Graph[Encode1[state]][symbol].Count; j++)
                        Union_NFA.Graph[Encode1[state]][symbol][j] = Encode1[Union_NFA.Graph[Encode1[state]][symbol][j]];
            }
            foreach (string state in NFA2.Graph.Keys)
            {
                Union_NFA.Graph[Encode2[state]] = NFA2.Graph[state];
                foreach (string symbol in Union_NFA.Graph[Encode2[state]].Keys)
                    for (int j = 0; j < Union_NFA.Graph[Encode2[state]][symbol].Count; j++)
                        Union_NFA.Graph[Encode2[state]][symbol][j] = Encode2[Union_NFA.Graph[Encode2[state]][symbol][j]];
            }
            NFA1.initial_state = Encode1[NFA1.initial_state];
            NFA2.initial_state = Encode2[NFA2.initial_state];
            string IS = "q" + i.ToString();
            string FS = "q" + (i + 1).ToString();
            Union_NFA.initial_state = IS;
            Union_NFA.final_states = "{'" + FS + "'}";
            foreach (string final in NFA1.Final)
                if (!Union_NFA.Graph[Encode1[final]].ContainsKey(""))
                    Union_NFA.Graph[Encode1[final]][""] = new List<string>() { FS };
                else
                    Union_NFA.Graph[Encode1[final]][""].Add(FS);
            foreach (string final in NFA2.Final)
                if (!Union_NFA.Graph[Encode2[final]].ContainsKey(""))
                    Union_NFA.Graph[Encode2[final]][""] = new List<string>() { FS };
                else
                    Union_NFA.Graph[Encode2[final]][""].Add(FS);
            Union_NFA.Graph[IS] = new Dictionary<string, List<string>>();
            Union_NFA.Graph[FS] = new Dictionary<string, List<string>>();
            Union_NFA.Graph[IS][""] = new List<string>() { NFA1.initial_state, NFA2.initial_state };
            Union_NFA.states = "{" + string.Join(',', Union_NFA.Graph.Keys.Select(x => "'" + x + "'")) + "}";
            Union_NFA.transitions = new Dictionary<string, Dictionary<string, string>>();
            foreach (string state in Union_NFA.Graph.Keys)
            {
                Union_NFA.transitions[state] = new Dictionary<string, string>();
                foreach (string symbol in Union_NFA.Graph[state].Keys)
                    Union_NFA.transitions[state][symbol] = "{" + string.Join(',', Union_NFA.Graph[state][symbol].Select(x => "'" + x + "'")) + "}";
            }
            return Union_NFA;
        }
        static void Main()
        {
            string input = Console.ReadLine();
            if(input == "s")
            {
                string address = Console.ReadLine();
                address = @"" + address;
                string text = File.ReadAllText(address);
                FA NFA1 = JsonSerializer.Deserialize<FA>(text);
                FA NFA = Star(NFA1);
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json_FA = JsonSerializer.Serialize(NFA, options);
                json_FA = System.Text.RegularExpressions.Regex.Unescape(json_FA);
                File.WriteAllText(@"E:\first-project_TLA\TLA01-Projects\RFA.json", json_FA);
            }
            if(input == "u")
            {
                string address = Console.ReadLine();
                address = @"" + address;
                string text = File.ReadAllText(address);
                string address2 = Console.ReadLine();
                address2 = @"" + address2;
                string text2 = File.ReadAllText(address2);
                FA NFA1 = JsonSerializer.Deserialize<FA>(text);
                FA NFA2 = JsonSerializer.Deserialize<FA>(text2);
                FA NFA = Union(NFA1, NFA2);
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json_FA = JsonSerializer.Serialize(NFA, options);
                json_FA = System.Text.RegularExpressions.Regex.Unescape(json_FA);
                File.WriteAllText(@"E:\first-project_TLA\TLA01-Projects\RFA.json", json_FA);
            }
            if(input == "c")
            {
                string address = Console.ReadLine();
                address = @"" + address;
                string text = File.ReadAllText(address);
                string address2 = Console.ReadLine();
                address2 = @"" + address2;
                string text2 = File.ReadAllText(address2);
                FA NFA1 = JsonSerializer.Deserialize<FA>(text);
                FA NFA2 = JsonSerializer.Deserialize<FA>(text2);
                FA NFA = Concatenation(NFA1, NFA2);
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json_FA = JsonSerializer.Serialize(NFA, options);
                json_FA = System.Text.RegularExpressions.Regex.Unescape(json_FA);
                File.WriteAllText(@"E:\first-project_TLA\TLA01-Projects\RFA.json", json_FA);
            }
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.WorkingDirectory = @"E:\first-project_TLA\TLA01-Projects";
            p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = false;
            p.StartInfo.RedirectStandardInput = true;
            p.Start();
            p.StandardInput.WriteLine(@"python main.py RFA.json");
            p.Close();
        }
    }
}
