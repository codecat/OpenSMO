using System;
using System.Collections;
using System.IO;

namespace OpenSMO {
    public class Config {
        public string Filename;
        public Hashtable Data;

        public Config(string Filename) {
            this.Filename = Filename;

            Data = new Hashtable();
            string[] lines = File.ReadAllLines(Filename);
            foreach (string line in lines) {
                if (line.Trim().StartsWith("//") || line.Trim() == "")
                    continue;

                string[] parse = line.Trim().Split(new char[] { '=' }, 2);

                if (parse.Length != 2)
                    continue;

                Data[parse[0].Trim()] = parse[1].Trim();
            }
        }

        public bool Contains(string Key) {
            return Data.ContainsKey(Key);
        }

        public string Get(string Key) {
            if (Contains(Key))
                return (string)Data[Key];
            else
                return "";
        }
    }
}
