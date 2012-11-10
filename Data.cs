using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenSMO {
    public static class Data {
        public static Hashtable GetSong(string Name, string Artist, string SubTitle) {
            Hashtable[] resCheck = MySql.Query("SELECT * FROM songs WHERE Name='" + Name + "' " +
                                             "AND Artist='" + Artist + "' " +
                                             "AND SubTitle\='" + SubTitle + "'");

            if (resCheck == null) return null;
            if (resCheck.Length == 1)
                return resCheck[0];
            else
                return null;
        }

        public static Hashtable AddSong(bool Start, User user) {
            if (user.CurrentRoom == null || user.CurrentRoom.CurrentSong == null || user.CurrentRoom.Reported)
                return null;

            string Name = MySql.AddSlashes(user.CurrentRoom.CurrentSong.Name);
            string Artist = MySql.AddSlashes(user.CurrentRoom.CurrentSong.Artist);
            string SubTitle = MySql.AddSlashes(user.CurrentRoom.CurrentSong.SubTitle);

            Hashtable song = Data.GetSong(Name, Artist, SubTitle);

            if (user.ShadowBanned) {
                if (song != null) {
                    return song;
                } else {
                    Hashtable ret = new Hashtable();
                    ret["ID"] = -1;
                    ret["Name"] = Name;
                    ret["Artist"] = Artist;
                    ret["SubTitle"] = SubTitle;
                    ret["Played"] = 0;
                    ret["Notes"] = 0;
                    return ret;
                }
            }

            if (song == null) {
                MySql.Query("INSERT INTO songs (Name,Artist,SubTitle) VALUES(" +
                                                          "'" + Name + "'," +
                                                          "'" + Artist + "'," +
                                                          "'" + SubTitle + "')");

                return MySql.Query("SELECT * FROM songs ORDER BY ID DESC LIMIT 0,1")[0];
            } else if (Start) {
                MySql.Query("UPDATE songs SET Played=Played+1 WHERE ID=" + song["ID"].ToString());
                user.CurrentRoom.Reported = true;
            }

            return song;
        }

        public static string BanUser(User user, int originID) {
            string IP = user.tcpClient.Client.RemoteEndPoint.ToString().Split(':')[0];
            MySql.Query("INSERT INTO bans (IP,From) VALUES(\"" + IP + "\"," + originID + ")");
            return IP;
        }

        public static bool IsBanned(string IP) {
            Hashtable[] res = MySql.Query("SELECT * FROM bans WHERE IP = \"" + MySql.AddSlashes(IP) + "\"");
            return res.Length != 0;
        }

        public static void AddStats(User user) {
            if (user.CurrentRoom == null)
                return;

            if (user.CurrentRoom.CurrentSong == null)
                return;

            user.CurrentRoom.Status = RoomStatus.Ready;

            string Name = MySql.AddSlashes(user.CurrentRoom.CurrentSong.Name);
            string Artist = MySql.AddSlashes(user.CurrentRoom.CurrentSong.Artist);
            string SubTitle = MySql.AddSlashes(user.CurrentRoom.CurrentSong.SubTitle);

            int songID = 0;
            Hashtable song = Data.GetSong(Name, Artist, SubTitle);

            if (song != null) {
                songID = (int)song["ID"];

                if (!user.ShadowBanned) {
                    double songTime = user.SongTime.Elapsed.TotalSeconds;
                    if (songTime > (int)song["Time"]) {
                        MySql.Query("UPDATE songs SET Time=" + songTime.ToString().Replace(',', '.') + " WHERE ID=" + song["ID"]);
                    }
                }

                string playerSettings = MySql.AddSlashes(user.GamePlayerSettings);

                // Big-ass query right there...
                if (!user.ShadowBanned) {
                    MySql.Query("INSERT INTO stats (User,PlayerSettings,Song,Feet,Difficulty,Grade,Score,MaxCombo," +
                        "Note_0,Note_1,Note_Mine,Note_Miss,Note_Barely,Note_Good,Note_Great,Note_Perfect,Note_Flawless,Note_NG,Note_Held) VALUES(" +
                        user.User_Table["ID"].ToString() + ",'" + playerSettings + "'," + songID.ToString() + "," + user.GameFeet.ToString() + "," + ((int)user.GameDifficulty).ToString() + "," + ((int)user.Grade).ToString() + "," + user.Score.ToString() + "," + user.MaxCombo.ToString() + "," +
                        user.Notes[0].ToString() + "," + user.Notes[1].ToString() + "," + user.Notes[2].ToString() + "," + user.Notes[3].ToString() + "," + user.Notes[4].ToString() + "," + user.Notes[5].ToString() + "," + user.Notes[6].ToString() + "," + user.Notes[7].ToString() + "," + user.Notes[8].ToString() + "," + user.Notes[9].ToString() + "," + user.Notes[10].ToString() + ")");
                }
            }

            // Give player XP
            int XP = 0;
            for (int i = 3; i <= 8; i++)
                XP += (i - 3) * user.Notes[i];
            XP /= 6;

            user.SendChatMessage("You gained " + Func.ChatColor("aaaa00") + XP.ToString() + Func.ChatColor("ffffff") + " XP!");

            if (!user.ShadowBanned)
                MySql.Query("UPDATE users SET XP=XP+" + XP.ToString() + " WHERE ID=" + user.User_ID.ToString());
        }
    }
}
