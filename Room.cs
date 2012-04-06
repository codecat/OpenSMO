using System;
using System.Collections.Generic;
using System.Collections;

namespace OpenSMO {
    public enum RoomStatus : int { Ready, Closed, Locked }

    public class Room {
        public MainClass mainClass;

        public string ID = "";
        public string Name;
        public string Description;
        public string Password;
        public string Style = "dance";
        public RoomStatus Status = RoomStatus.Closed;
        public bool Free;
        public bool AllPlaying = false;
        public bool Secret = false;
        public int SendStatsTimer = 0;
        public Hashtable Meta = new Hashtable();

        public User Owner;
        public List<User> Users {
            get {
                List<User> ret = new List<User>();
                foreach (User user in mainClass.Users) {
                    if (user.CurrentRoom == this)
                        ret.Add(user);
                }
                return ret;
            }
        }
        public int UserCount { get { return Users.Count; } }

        public Song CurrentSong = new Song();
        public bool Reported;

        private string _ChatBuffer = "";
        public string ChatBuffer {
            get {
                string[] lines = this._ChatBuffer.Split('\n');
                string ret = "";

                int lineLimit = int.Parse(mainClass.ServerConfig.Get("RTS_ChatLines"));
                int lineCount = lines.Length;
                int startCount = Math.Max(0, lineCount - lineLimit);

                for (int i = startCount; i < lineCount; i++)
                    ret += lines[i] + '\n';
                return ret.Trim('\n');
            }
        }

        public void AddChatBuffer(string str) {
            _ChatBuffer += str + "\n";
        }

        public Room(MainClass mainClass, User Owner) {
            this.mainClass = mainClass;
            this.Owner = Owner;
            this.ID = MainClass.RandomString(5);
        }

        public void Update() {
            if (++SendStatsTimer == mainClass.FPS / 2) {
                foreach (User user in Users) {
                    if (user.Playing)
                        user.SendGameStatus();
                }
                SendStatsTimer = 0;
            }

            if (UserCount == 0) {
                MainClass.AddLog("Room '" + Name + "' removed.");
                mainClass.Rooms.Remove(this);
            }
        }
    }
}
