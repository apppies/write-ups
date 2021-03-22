using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Numerics;

namespace NixuPlumbing
{

    class CommitInfo
    {
        public string Hash { get; set; }
        public string Author { get; set; }
        public string Comment { get; set; }
        public string Date { get; set; }
        public string Parent { get; set; }

        static public CommitInfo ParseInfo(string[] InfoLines)
        {
            var ret = new CommitInfo();
            ret.Hash = InfoLines[0].Substring(7);
            ret.Author = InfoLines[1].Substring(8);
            ret.Comment = InfoLines[4].Trim();
            var dateInfo = InfoLines[2].Split(' ');

            var xDate = DateTime.Parse($"{dateInfo[4]} {dateInfo[5]} {dateInfo[7]} {dateInfo[6]}");  // + " " + dateInfo[8];
            var unixTime = ((DateTimeOffset)xDate).AddHours(-1).ToUnixTimeSeconds();
            ret.Date = $"{unixTime} {dateInfo[8]}";
            return ret;
        }
    }

    class Program
    {
        static string Sha1StringFromString(string input)
        {
            var hash = new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Concat(hash.Select(b => b.ToString("x2")));
        }

        static byte[] Sha1FromByteArray(byte[] input)
        {
            var hash = new SHA1Managed().ComputeHash(input);
            return hash;
        }

        static byte[] GetGITFileHash(string content)
        {
            var input = $"blob {content.Length}\0" + content;
            var byteyinput = Encoding.UTF8.GetBytes(input);
            return Sha1FromByteArray(byteyinput);
        }

        static byte[] TreeHash(Dictionary<string,string> files)
        {
            //Construct bytes tree
            var tree = new List<byte>();
            foreach (var file in files)
            {
                var fileEntry = $"100644 {file.Key}\0";
                var fileHash = GetGITFileHash(file.Value);
                var leaf = Encoding.ASCII.GetBytes(fileEntry).ToList();
                leaf.AddRange(fileHash);
                tree.AddRange(leaf);
            }
            var header = Encoding.ASCII.GetBytes($"tree {tree.Count}\0").ToList();
            header.AddRange(tree);

            return header.ToArray();
        }

        static void Main(string[] args)
        {
            var gitlogText = System.IO.File.ReadAllLines(@"C:\dump\gitlog.txt");
            var commits = new List<CommitInfo>();

            for (int i = 0; i < gitlogText.Length; i+=6)
            {
                commits.Add(CommitInfo.ParseInfo(gitlogText.Skip(i).Take(6).ToArray()));
            }
            commits.Reverse();

            for (int i = 1; i < commits.Count; i++)
            {
                commits[i].Parent = commits[i - 1].Hash;
            }

            bool run = true;
            bool found;
            int currentCommit = 1;
            string flag = "";
            string flagOriginal = "";
            int sides = 9;
            while (run)
            {
                found = false;
                for (int i = 1; i < sides + 1; i++)
                {
                    flag = flagOriginal + i.ToString();
                    var commitHash = CommitHash(commits[currentCommit], flag);
                    
                    if (commits[currentCommit].Hash == commitHash)
                    {
                        Debug.WriteLine($"{i}: {commits[currentCommit].Hash} - {commitHash}");
                        Debug.WriteLine($"Commit: {currentCommit}, Found: {i}, Flag: {flag}");
                        flagOriginal = flag;
                        currentCommit++;
                        found = true;
                        break;
                    }
                }

                //no solution, try different dice
                if (!found)
                {
                    if (sides == 10000000)
                    {
                        run = false;
                    }
                    else
                    {
                        if (sides == 9)
                            sides = 100;
                        else
                            sides = 10000000;

                        Debug.WriteLine($"New die, sides: {sides}");
                    }

                }
            }
            
            Debug.WriteLine("done throwing dice");

            flag = "689676399780243364502385099687264621047025008848296702816833952558388938389548485877060426128011030487330314";
            currentCommit = 26;
            Debug.WriteLine(CommitHash(commits[currentCommit], flag) + " - " + commits[currentCommit].Hash);

            BigInteger bigInt = BigInteger.Parse(flag);
            flag = bigInt.ToString("x");
            Debug.WriteLine(flag);
            Debug.WriteLine(CommitHash(commits[currentCommit], flag) + " - " + commits[currentCommit].Hash);

            flag = "4b2d656e6c0a787679620a50756e657976720a717279676e0a7a62776e0a476e61620a6776666e0a666e6f6e0a";
            currentCommit = 27;

            var toBytes = new List<byte>();
            for (int i = 0; i < flag.Length; i+=2)
            {
                var sub = $"{flag[i]}{flag[i + 1]}";
                var subnum = Convert.ToByte(sub, 16);
                toBytes.Add(subnum);
            }
            string bitString = Encoding.ASCII.GetString(toBytes.ToArray());


            Debug.WriteLine(bitString);
            Debug.WriteLine(CommitHash(commits[currentCommit], bitString) + " - " + commits[currentCommit].Hash);

            flag = "K-enl\nxvyb\nPuneyvr\nqrygn\nzbwn\nGnab\ngvfn\nfnon\n";
            currentCommit = 28;

            flag = Rot13(flag);
            Debug.WriteLine(CommitHash(commits[currentCommit], flag) + " - " + commits[currentCommit].Hash);


            flag = "X-ray\nkilo\nCharlie\ndelta\nmoja\nTano\ntisa\nsaba\n";
            currentCommit = 29;
            flag = "X-ray\nkilo\nCharlie\ndelta\none\nFive\nnine\nseven\n";
            Debug.WriteLine(CommitHash(commits[currentCommit], flag) + " - " + commits[currentCommit].Hash);

            flag = "xkcd1597";
            currentCommit = 30;
            Debug.WriteLine(CommitHash(commits[currentCommit], flag) + " - " + commits[currentCommit].Hash);

            flag = "NIXU{xkcd1597}";
            currentCommit = 31;
            Debug.WriteLine(CommitHash(commits[currentCommit], flag) + " - " + commits[currentCommit].Hash);

        }

        static string CommitHash(CommitInfo commit, string FlagContent)
        {
            var tree = TreeHash(new Dictionary<string, string>() { { "flag.txt", FlagContent } });

            var treeHash = string.Concat(Sha1FromByteArray(tree).Select(b => b.ToString("x2")));

            var commitInfo = "";
            if (commit.Parent != "")
            {
                commitInfo = $"tree {treeHash}\nparent {commit.Parent}\nauthor {commit.Author} {commit.Date}\ncommitter {commit.Author} {commit.Date}\n\n{commit.Comment}\n";
            }
            else
            {
                commitInfo = $"tree {treeHash}\nauthor {commit.Author} {commit.Date}\ncommitter {commit.Author} {commit.Date}\n\n{commit.Comment}\n";
            }

            var commitString = $"commit {commitInfo.Length}\0{commitInfo}";

            var commitHash = Sha1StringFromString(commitString);

            return commitHash;
        }

        public static string Rot13(string value)
        {
			// https://www.dotnetperls.com/rot13
            char[] array = value.ToCharArray();
            for (int i = 0; i < array.Length; i++)
            {
                int number = (int)array[i];

                if (number >= 'a' && number <= 'z')
                {
                    if (number > 'm')
                    {
                        number -= 13;
                    }
                    else
                    {
                        number += 13;
                    }
                }
                else if (number >= 'A' && number <= 'Z')
                {
                    if (number > 'M')
                    {
                        number -= 13;
                    }
                    else
                    {
                        number += 13;
                    }
                }
                array[i] = (char)number;
            }
            return new string(array);
        }
    }
}
