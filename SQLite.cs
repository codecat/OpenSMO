using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Mono.Data.Sqlite;
using System.Diagnostics;

namespace OpenSMO {
    public class Sql {
        public static string Filename;
        public static int Version;
        public static bool Compress;
        public static bool UseCommit;

        private static SqliteConnection conn;
        private static SqliteCommand cmd;

        public static void Connect() {
            UseCommit = bool.Parse(MainClass.Instance.ServerConfig.Get("Database_UseCommit"));

            conn = new SqliteConnection("Data Source=" + Filename + ";Version=" + Version.ToString() + ";New=False;Compress=" + Compress.ToString() + ";Journal Mode=Off;");
            try { conn.Open(); } catch (Exception ex) {
                MainClass.AddLog("Couldn't open SQLite database: " + ex.Message, true);
            }

            if (UseCommit)
                Query("BEGIN TRANSACTION");
        }

        public static bool Connected {
            get { return conn != null; }
        }

        public static string AddSlashes(string str) {
            return str.Replace("'", "''");
        }

        public static void Close() {
            conn.Close();
        }

        private static int commitTimer;
        public static void Update() {
            if (UseCommit) {
                if (++commitTimer >= MainClass.Instance.FPS * int.Parse(MainClass.Instance.ServerConfig.Get("Database_CommitTime"))) {
                    commitTimer = 0;
                    Query("COMMIT TRANSACTION");
                    Query("BEGIN TRANSACTION");
                }
            }
        }

        public static Hashtable[] Query(string qry) {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            SqliteDataReader reader = null;

            cmd = conn.CreateCommand();
            cmd.CommandText = qry;

            try { reader = cmd.ExecuteReader(); } catch (Exception ex) {
                MainClass.AddLog("Query error: '" + ex.Message + "'", true);
                MainClass.AddLog("Query was: '" + qry + "'", true);
                return null;
            }

            List<Hashtable> ret = new List<Hashtable>();

            while (reader.Read()) {
                Hashtable row = new Hashtable();

                for (int i = 0; i < reader.FieldCount; i++) {
                    if (reader[i].GetType() == typeof(Int64)) {
                        if ((long)reader[i] > int.MaxValue)
                            row[reader.GetName(i)] = (long)reader[i];
                        else
                            row[reader.GetName(i)] = (int)(long)reader[i];
                    } else
                        row[reader.GetName(i)] = reader[i];
                }

                ret.Add(row);
            }

            sw.Stop();

            if (sw.ElapsedMilliseconds >= 1000)
                MainClass.AddLog("SQL Query took very long: " + sw.ElapsedMilliseconds + "ms", true);

            return ret.ToArray();
        }
    }
}
