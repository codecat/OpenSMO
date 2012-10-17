using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Data;
using System.Diagnostics;

namespace OpenSMO
{
  public enum UserRank : int { User, Moderator, Admin }
  public enum RoomRights : int { Player, Operator, Owner }

  public class User
  {
    public MainClass mainClass;
    public bool Connected = true;
    public bool ShadowBanned = false;

    public int User_ID = 0;
    public string User_Name = "";
    public string User_IP = "";
    public UserRank User_Rank = UserRank.User;
    public Hashtable User_Table = null;

    public int User_Protocol = 0;
    public string User_Game = "";

    private Room _CurrentRoom = null;
    public Room CurrentRoom
    {
      get { return _CurrentRoom; }
      set
      {
        Room oldRoom = _CurrentRoom;
        _CurrentRoom = value;

        User[] lobbyUsers = GetUsersInRoom();

        if (value == null) {
          if (oldRoom == null) return;

          User[] users = oldRoom.Users.ToArray();
          if (users.Length == 0) {
            if (!oldRoom.Fixed) {
              MainClass.AddLog("Removing room '" + oldRoom.Name + "'");
              mainClass.Rooms.Remove(oldRoom);

              foreach (User user in lobbyUsers)
                user.SendRoomList();
            }
          } else {
            if (oldRoom.AllPlaying) {
              bool shouldStart = true;
              foreach (User user in users) {
                if (!user.Synced) {
                  shouldStart = false;
                  break;
                }
              }

              if (shouldStart) {
                SendSongStartTo(users);
                oldRoom.AllPlaying = false;
              }
            }

            mainClass.SendChatAll(NameFormat() + " left the room.", oldRoom, this);

            foreach (User user in users)
              user.SendRoomPlayers();

            if (users.Length > 0) {
              if (CurrentRoomRights == RoomRights.Owner) {
                User newOwner;
                int tmout = 0;
                do {
                  newOwner = users[MainClass.rnd.Next(users.Length)];
                  if (++tmout == 15) return;
                } while (newOwner == this);
                newOwner.CurrentRoomRights = RoomRights.Owner;
                newOwner.CurrentRoom.Owner = newOwner;
                mainClass.SendChatAll(newOwner.NameFormat() + " is now room owner.", newOwner.CurrentRoom);
              }
            } else {
              if (!oldRoom.Fixed) {
                MainClass.AddLog("Removing room '" + oldRoom.Name + "'");
                mainClass.Rooms.Remove(oldRoom);
                oldRoom = null;
              }
            }
            CurrentRoomRights = RoomRights.Player;
          }

          foreach (User user in lobbyUsers)
            user.SendRoomPlayers();
        }
      }
    }
    public RoomRights CurrentRoomRights = RoomRights.Player;
    public NSScreen CurrentScreen = NSScreen.Black;

    public TcpClient tcpClient;
    public BinaryWriter tcpWriter;
    public BinaryReader tcpReader;

    public Ez ez;

    public bool CanPlay = true;
    public bool Spectating = false;
    public bool Synced = false;
    public bool SyncNeeded = false;
    public bool Playing = false;
    public int[] Notes;
    public int NoteCount
    {
      get
      {
        if (Notes == null) return 0;
        int ret = 0;
        for (int i = 3; i <= 8; i++)
          ret += Notes[i];
        return ret;
      }
    }
    public int Score = 0;
    public int Combo = 0;
    public int MaxCombo = 0;
    private NSGrades _Grade = 0;
    public NSGrades Grade
    {
      get { return _Grade; }
      set
      {
        if (bool.Parse(mainClass.ServerConfig.Get("Game_FullComboIsAA"))) {
          if (value >= NSGrades.A && FullCombo) {
            _Grade = NSGrades.AA;
            return;
          }
        }
        _Grade = value;
      }
    }

    public NSNotes NoteHit = NSNotes.Miss;
    public double NoteOffset = 0d;
    public ushort NoteOffsetRaw = 0;

    public int GameFeet = 0;
    public NSDifficulty GameDifficulty = NSDifficulty.Beginner;
    public string GamePlayerSettings = "";
    public string CourseTitle = "";
    public string SongOptions = "";
    public Hashtable Meta = new Hashtable();

    public Stopwatch PlayTime = new Stopwatch();
    public Stopwatch SongTime = new Stopwatch();

    public int SMOScore
    {
      get
      {
        if (Notes == null) return 0;

        int ret = 0;
        for (int i = 4; i <= 8; i++)
          ret += Notes[i] * (i - 3);
        return ret;
      }
    }
    public bool FullCombo
    {
      get
      {
        int badCount = 0;
        for (int i = 0; i <= 5; i++)
          badCount += Notes[i];
        return badCount == 0;
      }
    }

    public User(MainClass mainClass, TcpClient tcpClient)
    {
      this.mainClass = mainClass;
      this.tcpClient = tcpClient;

      this.User_IP = tcpClient.Client.RemoteEndPoint.ToString().Split(':')[0];

      NetworkStream stream = tcpClient.GetStream();
      stream.ReadTimeout = int.Parse(mainClass.ServerConfig.Get("Server_ReadTimeout"));
      this.tcpWriter = new BinaryWriter(stream);
      this.tcpReader = new BinaryReader(stream);

      ez = new Ez(this);
    }

    ~User()
    {
      CurrentRoom = null;
    }

    public bool RequiresAuthentication()
    {
      if (User_Name == "") {
        Kick();
        return false;
      }
      return true;
    }

    public bool RequiresRoom()
    {
      if (CurrentRoom == null) {
        Kick();
        return false;
      }
      return false;
    }

    public void Kick()
    {
      MainClass.AddLog("Client '" + this.User_Name + "' kicked.");
      if (this.CurrentRoom != null) this.CurrentRoom = null;
      this.Disconnect();
    }

    public void Ban()
    {
      Data.BanUser(this, 0);
    }

    public void Ban(int originID)
    {
      Data.BanUser(this, originID);
    }

    public void KickBan()
    {
      this.Ban();
      this.Kick();
    }

    public void KickBan(int originID)
    {
      this.Ban(originID);
      this.Kick();
    }

    public void Disconnect()
    {
      MainClass.AddLog("Client '" + this.User_Name + "' disconnected.");
      if (this.CurrentRoom != null)
        this.CurrentRoom.Users.Remove(this);
      this.tcpClient.Close();
      this.Connected = false;
    }

    public string NameFormat()
    {
      string current = User_Name;

      for (int i = 0; i < mainClass.Scripting.NameFormatHooks.Count; i++) {
        try {
          current = mainClass.Scripting.NameFormatHooks[i](this, current);
        } catch (Exception ex) { mainClass.Scripting.HandleError(ex); }
      }

      return current + Func.ChatColor("ffffff");
    }

    public void SendChatMessage(string Message)
    {
      ez.Write1((byte)(mainClass.ServerOffset + NSCommand.NSCCM));
      ez.WriteNT(" " + (Message.StartsWith("|c0") ? "" : Func.ChatColor("ffffff")) + Message + " ");
      ez.SendPack();
    }

    public void SendRoomList()
    {
      ez.Write1((byte)(mainClass.ServerOffset + NSCommand.NSCSMOnline));
      ez.Write1((byte)1);
      ez.Write1((byte)1);

      if (ShadowBanned) {
        ez.Write1((byte)0);
      } else {
        byte visibleRoomCount = 0;
        foreach (Room r in mainClass.Rooms) {
          if (r.Owner == null || !r.Owner.ShadowBanned)
            visibleRoomCount++;
        }
        ez.Write1(visibleRoomCount);

        foreach (Room room in mainClass.Rooms) {
          if (room.Owner == null || !room.Owner.ShadowBanned) {
            ez.WriteNT(room.Name);
            ez.WriteNT(room.Description);
          }
        }

        foreach (Room room in mainClass.Rooms) {
          if (room.Owner == null || !room.Owner.ShadowBanned)
            ez.Write1((byte)room.Status);
        }

        foreach (Room room in mainClass.Rooms) {
          if (room.Owner == null || !room.Owner.ShadowBanned)
            ez.Write1((byte)(room.Password != "" ? 1 : 0));
        }
      }

      ez.SendPack();
    }

    public void SendToRoom()
    {
      SyncNeeded = false;
      CanPlay = true;

      if (CurrentRoom != null) {
        SendRoomList();

        ez.Write1((byte)(mainClass.ServerOffset + NSCommand.NSCSMOnline));
        ez.Write1(1);
        ez.Write1(0);
        ez.WriteNT(CurrentRoom.Name);
        ez.WriteNT(CurrentRoom.Description);
        ez.Write1(1); // If this is 0, it won't change the players' screen
        ez.SendPack();

        foreach (User user in mainClass.Users)
          user.SendRoomPlayers();

        mainClass.SendChatAll(NameFormat() + Func.ChatColor("ffffff") + " joined the room.", CurrentRoom);

        if (CurrentRoom.Fixed) {
          if (CurrentRoom.FixedMotd != "") {
            SendChatMessage(Func.ChatColor("00aa00") + CurrentRoom.FixedMotd);
          }

          if (CurrentRoom.FixedOperators.Contains(User_ID)) {
            SendChatMessage(Func.ChatColor("0000aa") + "You are an operator in this fixed room.");
            CurrentRoomRights = RoomRights.Operator;
          }
        }
      } else
        MainClass.AddLog("Not supported: Kicking from room. Fixme! User::SendToRoom", true);
    }

    public User[] GetUsersInRoom()
    {
      List<User> ret = new List<User>();
      foreach (User user in mainClass.Users) {
        if (user.CurrentRoom == this.CurrentRoom)
          ret.Add(user);
      }
      return ret.ToArray();
    }

    public void SendRoomPlayers()
    {
      User[] users = GetUsersInRoom();

      ez.Write1((byte)(mainClass.ServerOffset + NSCommand.NSCUUL));
      ez.Write1(mainClass.ServerMaxPlayers); // Not used clientside

      if (ShadowBanned) {
        ez.Write1((byte)1);
        ez.Write1(1);
        ez.WriteNT(User_Name);
      } else {
        ez.Write1((byte)users.Length);

        foreach (User user in users) {
          ez.Write1(1); // status
          ez.WriteNT(user.User_Name);
        }
      }

      ez.SendPack();
    }

    public void SendSong(bool Start)
    {
      if (Start) {
        Playing = true;
        CurrentRoom.Status = RoomStatus.Closed;

        // Reset
        Notes = new int[(int)NSNotes.NUM_NS_NOTES];
        Score = 0;
        Combo = 0;
        MaxCombo = 0;
        Grade = NSGrades.AAAA;
      }

      ez.Write1((byte)(mainClass.ServerOffset + NSCommand.NSCRSG));
      ez.Write1(Start ? (byte)2 : (byte)1);
      ez.WriteNT(CurrentRoom.CurrentSong.Name);
      ez.WriteNT(CurrentRoom.CurrentSong.Artist);
      ez.WriteNT(CurrentRoom.CurrentSong.SubTitle);
      ez.SendPack();
    }

    public void SendGameStatusColumn(byte ColumnID)
    {
      ez.Write1((byte)(mainClass.ServerOffset + NSCommand.NSCGSU));
      ez.Write1(ColumnID);

      User[] origColumnUsers = GetUsersInRoom();
      User[] columnUsers = (from user in origColumnUsers where user.Playing orderby user.SMOScore descending select user).ToArray();
      ez.Write1((byte)columnUsers.Length);

      switch (ColumnID) {
        case 0: // Positions
          for (int i = 0; i < columnUsers.Length; i++) {
            for (int j = 0; j < origColumnUsers.Length; j++) {
              if (origColumnUsers[j] == columnUsers[i]) {
                ez.Write1((byte)j);
                break;
              }
            }
          }
          break;

        case 1: // Combo
          foreach (User user in columnUsers)
            ez.Write2((short)user.Combo);
          break;

        case 2: // Grade
          foreach (User user in columnUsers)
            ez.Write1((byte)user.Grade);
          break;
      }

      ez.SendPack();
    }

    public void SendGameStatus()
    {
      SendGameStatusColumn(0);
      SendGameStatusColumn(1);
      SendGameStatusColumn(2);
    }

    public bool IsModerator()
    {
      return User_Rank >= UserRank.Moderator;
    }

    public bool IsAdmin()
    {
      return User_Rank >= UserRank.Admin;
    }

    public bool CanChangeRoomSettings()
    {
      if (CurrentRoom != null)
        return CurrentRoomRights >= RoomRights.Operator || IsModerator();
      return false;
    }

    public void SendAttack(string modifiers, int ms)
    {
      ez.Write1((byte)(mainClass.ServerOffset + NSCommand.NSCAttack));
      ez.Write1(0); // iPlayerNumber <-- Most of the times 0.
      ez.Write4(ms); // fSecsRemaining / 1000.0f <-- Thus, in milliseconds.
      ez.WriteNT(modifiers); // "300% wave, *4 -300% beat" <-- Deadly.
      ez.SendPack();
    }

    public void SendSongStartTo(User[] checkSyncPlayers)
    {
      foreach (User user in checkSyncPlayers) {
        user.Synced = false;
        user.SongTime.Restart();

        user.ez.Write1((byte)(mainClass.ServerOffset + NSCommand.NSCGSR));
        user.ez.SendPack();
      }
    }

    // Ping packet sent by client, generally not sent by client unless server requests so.
    public void NSCPing()
    {
      ez.Write1((byte)(mainClass.ServerOffset + NSCommand.NSCPingR));
      ez.Discard(); // Just to be sure.
    }

    // Client sent a ping response.
    public void NSCPingR()
    {
      pingTimeout = 5;
      ez.Discard(); // Just to be sure.
    }

    // Client handshakes with the server
    public void NSCHello()
    {
      User_Protocol = ez.Read1();
      User_Game = ez.ReadNT().Replace("\n", "|");

      MainClass.AddLog(User_IP + " is using SMOP v" + User_Protocol.ToString() + " in " + User_Game);
      PlayTime.Start();

      ez.Write1((byte)(mainClass.ServerOffset + NSCommand.NSCHello));
      ez.Write1(mainClass.ServerVersion);
      ez.WriteNT(mainClass.ServerConfig.Get("Server_Name"));
      ez.SendPack();
    }

    // Client switches menu screen
    public void NSCSMS()
    {
      NSScreen oldScreen = CurrentScreen;
      NSScreen newScreen = (NSScreen)ez.Read1();

      if (newScreen == NSScreen.Lobby) {
        CurrentRoom = null;

        SendRoomList();
        SendRoomPlayers();
      } else if (newScreen == NSScreen.Room) {
        List<User> users = CurrentRoom.Users;
        // find people waiting for synchronization
        List<User> usersToSendPacketTo = new List<User>();
        foreach (User user in users) {
          if (user.SyncNeeded) {
            usersToSendPacketTo.Add(user);
          }
        }

        // send packet those people
        SendSongStartTo(usersToSendPacketTo.ToArray());
      }

      CurrentScreen = newScreen;
    }

    // Client requests to start the game.
    public void NSCGSR()
    {
      if (CurrentRoom == null) {
        ez.Discard();
        return;
      }

      if (!RequiresAuthentication()) return;

      GameFeet = ez.Read1() / 16;
      GameDifficulty = (NSDifficulty)(ez.Read1() / 16);

      Synced = ez.Read1() == 16;

      CurrentRoom.CurrentSong.Name = ez.ReadNT();
      CurrentRoom.CurrentSong.SubTitle = ez.ReadNT();
      CurrentRoom.CurrentSong.Artist = ez.ReadNT();

      this.CourseTitle = ez.ReadNT();
      this.SongOptions = ez.ReadNT();

      string newPlayerSettings = "";
      do {
        newPlayerSettings += ez.ReadNT() + " ";
      } while (ez.LastPacketSize > 0);
      GamePlayerSettings = newPlayerSettings.Trim();

      CurrentRoom.AllPlaying = true;
      User[] checkSyncPlayers = GetUsersInRoom();
      foreach (User user in checkSyncPlayers) {
        if (user.SyncNeeded && user.CanPlay && !user.Synced)
          CurrentRoom.AllPlaying = false;
      }

      if (!Synced || CurrentRoom.AllPlaying) {
        ez.Write1((byte)(mainClass.ServerOffset + NSCommand.NSCGSR));
        ez.SendPack();

        if (CurrentRoom.AllPlaying) {
          SendSongStartTo(checkSyncPlayers);
          CurrentRoom.AllPlaying = false;
        }
      }
    }

    // User sends a game status update (game passes a step)
    public void NSCGSU()
    {
      if (!RequiresAuthentication()) return;

      if (Playing && !Spectating) {
        NSNotes gsuCtr;
        NSGrades gsuGrade;
        int gsuScore, gsuCombo, gsuLife;
        double gsuOffset;

        gsuCtr = (NSNotes)ez.Read1();
        gsuGrade = (NSGrades)(ez.Read1() / 16);
        gsuScore = ez.Read4();
        gsuCombo = ez.Read2();
        gsuLife = ez.Read2();
        NoteOffsetRaw = ez.ReadU2();
        gsuOffset = NoteOffsetRaw / 2000d - 16.384d;

        if (User_Protocol == 2)
          gsuCtr += 2;

        NoteHit = gsuCtr;
        NoteOffset = gsuOffset;

        Notes[(int)gsuCtr]++;
        Grade = gsuGrade;
        Score = gsuScore;
        Combo = gsuCombo;

        if (gsuCombo > MaxCombo)
          MaxCombo = gsuCombo;
      } else
        ez.Discard();
    }

    // Client exited game.
    public void NSCGON()
    {
      if (!RequiresAuthentication()) return;

      if (Playing && !Spectating) {
        Playing = false;

        if (CurrentRoom != null) { // Required for SMOP v2
          CurrentRoom.Reported = false;

          User[] columnUsers = GetUsersInRoom();

          // Post evaluation data
          ez.Write1((byte)(mainClass.ServerOffset + NSCommand.NSCGON));
          ez.Write1((byte)columnUsers.Length);

          // Name index
          for (int i = 0; i < columnUsers.Length; i++) ez.Write1((byte)i);
          // Score
          for (int i = 0; i < columnUsers.Length; i++) ez.Write4(columnUsers[i].Score);
          // Grade
          for (int i = 0; i < columnUsers.Length; i++) ez.Write1((byte)columnUsers[i].Grade);
          // Difficulty
          for (int i = 0; i < columnUsers.Length; i++) ez.Write1((byte)columnUsers[i].GameDifficulty);

          // Flawless to Miss
          for (int j = 0; j < 6; j++) {
            for (int i = 0; i < columnUsers.Length; i++) ez.Write2((short)columnUsers[i].Notes[(int)NSNotes.Flawless - j]);
          }
          // Held
          for (int i = 0; i < columnUsers.Length; i++) ez.Write2((short)columnUsers[i].Notes[(int)NSNotes.Held]);
          // Max combo
          for (int i = 0; i < columnUsers.Length; i++) ez.Write2((short)columnUsers[i].MaxCombo);

          // Player settings
          for (int i = 0; i < columnUsers.Length; i++) ez.WriteNT(columnUsers[i].GamePlayerSettings);

          ez.SendPack();
        }

        if (NoteCount > 0) {
          if (FullCombo) SendChatMessage(Func.ChatColor("00aa00") + "FULL COMBO!!");
          Data.AddStats(this);
        }
      } else {
        if (Spectating)
          SendChatMessage(Func.ChatColor("aa0000") + "Spectator mode activated, no stats gained.");
      }
    }

    // Client selects a song.
    public void NSCRSG()
    {
      if (CurrentRoom == null) {
        ez.Discard();
        return;
      }

      if (!RequiresAuthentication()) return;

      byte pickResponseStatus = ez.Read1();

      string pickName = ez.ReadNT();
      string pickArtist = ez.ReadNT();
      string pickAlbum = ez.ReadNT();

      switch (pickResponseStatus) {
        case 0: // Player has song
          CanPlay = SyncNeeded = true;
          ez.Discard();
          return;

        case 1: // Player does not have song
          CanPlay = SyncNeeded = false;
          mainClass.SendChatAll(NameFormat() + " does " + Func.ChatColor("aa0000") + "not" + Func.ChatColor("ffffff") + " have that song!", CurrentRoom);
          ez.Discard();
          return;
      }

      User[] pickUsers = GetUsersInRoom();

      bool canStart = true;
      string cantStartReason = "";

      Song currentSong = CurrentRoom.CurrentSong;
      bool isNewSong = currentSong.Artist != pickArtist ||
        currentSong.Name != pickName ||
        currentSong.SubTitle != pickAlbum;

      foreach (User user in pickUsers) {
        if (user.CurrentScreen != NSScreen.Room) {
          canStart = false;
          cantStartReason = user.NameFormat() + " is not ready yet!";
          break;
        } else if (!user.CanPlay && !isNewSong) {
          canStart = false;
          cantStartReason = user.NameFormat() + " is unable to participate!";
          break;
        }
      }

      if (CurrentRoom.Free || CanChangeRoomSettings()) {
        if (CurrentRoom.CurrentSong.Name == pickName &&
            CurrentRoom.CurrentSong.Artist == pickArtist &&
            CurrentRoom.CurrentSong.SubTitle == pickAlbum) {

          if (canStart) {
            foreach (User user in pickUsers) {
              Data.AddSong(true, this);
              user.SendSong(true);
              user.SendGameStatus();
            }
          } else
            mainClass.SendChatAll(cantStartReason, CurrentRoom);
        } else {
          if (canStart) {
            Song newSong = new Song();

            newSong.Name = pickName;
            newSong.Artist = pickArtist;
            newSong.SubTitle = pickAlbum;

            CurrentRoom.CurrentSong = newSong;

            int pickSongPlayed = 0;
            Hashtable pickSongRow = Data.AddSong(false, this);
            if (pickSongRow != null) {
              pickSongPlayed = (int)pickSongRow["Played"];
              newSong.Time = (int)pickSongRow["Time"];
            }

            mainClass.SendChatAll(NameFormat() + " selected " + Func.ChatColor("00aa00") + pickName + Func.ChatColor("ffffff") + ", which has " + (pickSongPlayed == 0 ? "never been played." : (pickSongPlayed > 1 ? "been played " + pickSongPlayed.ToString() + " times." : "been played only once.")), CurrentRoom);

            foreach (User user in pickUsers) {
              user.SendSong(false);
              user.SongTime.Reset();
            }
          } else
            mainClass.SendChatAll(cantStartReason, CurrentRoom);
        }
      } else {
        ez.Discard();
        SendChatMessage("You are not the room owner. Ask " + CurrentRoom.Owner.NameFormat() + " for /free");
      }
    }

    // Client sent user options
    public void NSCUPOpts()
    {
      ez.Discard(); // This contains a string with user options, but we don't really care about that too much for now.
    }

    byte packetCommandSub = 0;
    // Client sent SMO packet (note subpacket comments)
    public void NSCSMOnline()
    {
      packetCommandSub = ez.Read1();
      byte packetCommandSubSub = ez.Read1();

      if (packetCommandSub == 0) { // Login
        ez.Read1(); // Reserved byte

        string smoUsername = ez.ReadNT();
        string smoPassword = ez.ReadNT();

        if (!new Regex("^([A-F0-9]{32})$").Match(smoPassword).Success) {
          ez.Write1((byte)(mainClass.ServerOffset + NSCommand.NSCSMOnline));
          ez.Write2(1);
          ez.WriteNT("Login failed! Invalid password.");
          ez.SendPack();

          MainClass.AddLog("Invalid password hash given!", true);
          return;
        }

        Hashtable[] smoLoginCheck = Sql.Query("SELECT * FROM \"users\" WHERE \"Username\"='" + Sql.AddSlashes(smoUsername) + "'");
        if (smoLoginCheck.Length == 1 && smoLoginCheck[0]["Password"].ToString() == smoPassword) {
          MainClass.AddLog(smoUsername + " logged in.");

          User_Table = smoLoginCheck[0];
          User_ID = (int)User_Table["ID"];
          User_Name = (string)User_Table["Username"];
          User_Rank = (UserRank)User_Table["Rank"];

          ez.Write1((byte)(mainClass.ServerOffset + NSCommand.NSCSMOnline));
          ez.Write2(0);
          ez.WriteNT("Login success!");
          ez.SendPack();

          SendChatMessage(mainClass.ServerConfig.Get("Server_MOTD"));
          SendRoomList();

          User[] users = GetUsersInRoom();
          foreach (User user in users)
            user.SendRoomPlayers();

          return;
        } else if (smoLoginCheck.Length == 0) {
          if (bool.Parse(mainClass.ServerConfig.Get("Allow_Registration"))) {
            Sql.Query("INSERT INTO main.users (\"Username\",\"Password\",\"Email\",\"Rank\",\"XP\") VALUES(\"" + Sql.AddSlashes(smoUsername) + "\",\"" + Sql.AddSlashes(smoPassword) + "\",\"\",0,0)");
            MainClass.AddLog(smoUsername + " is now registered");

            User_Table = Sql.Query("SELECT * FROM \"users\" WHERE \"Username\"='" + Sql.AddSlashes(smoUsername) + "' AND \"Password\"='" + Sql.AddSlashes(smoPassword) + "'")[0];
            User_ID = (int)User_Table["ID"];
            User_Name = (string)User_Table["Username"];
            User_Rank = (UserRank)User_Table["Rank"];

            ez.Write1((byte)(mainClass.ServerOffset + NSCommand.NSCSMOnline));
            ez.Write2(0);
            ez.WriteNT("Login success!");
            ez.SendPack();

            SendChatMessage(mainClass.ServerConfig.Get("Server_MOTD"));
            SendRoomList();

            User[] users = GetUsersInRoom();
            foreach (User user in users)
              user.SendRoomPlayers();

            return;
          }
        }

        MainClass.AddLog(smoUsername + " tried logging in but failed");

        ez.Write1((byte)(mainClass.ServerOffset + NSCommand.NSCSMOnline));
        ez.Write2(1);
        ez.WriteNT("Login failed! Invalid password.");
        ez.SendPack();
      } else if (packetCommandSub == 01) { // Join room
        if (!RequiresAuthentication()) return;

        if (ez.LastPacketSize == 0)
          return;

        string joinRoomName = ez.ReadNT();
        string joinRoomPass = "";

        if (ez.LastPacketSize > 0)
          joinRoomPass = ez.ReadNT();

        foreach (Room room in mainClass.Rooms) {
          if (room.Name == joinRoomName && (room.Password == joinRoomPass || IsModerator())) {
            CurrentRoomRights = RoomRights.Player;
            CurrentRoom = room;
            SendToRoom();
            break;
          }
        }
      } else if (packetCommandSub == 02) { // New room
        if (!RequiresAuthentication()) return;

        string newRoomName = ez.ReadNT();
        string newRoomDesc = ez.ReadNT();
        string newRoomPass = "";

        if (ez.LastPacketSize > 0)
          newRoomPass = ez.ReadNT();

        MainClass.AddLog(User_Name + " made a new room '" + newRoomName + "'");

        Room newRoom = new Room(mainClass, this);

        newRoom.Name = newRoomName;
        newRoom.Description = newRoomDesc;
        newRoom.Password = newRoomPass;

        mainClass.Rooms.Add(newRoom);

        User[] users = GetUsersInRoom();
        foreach (User user in users)
          user.SendRoomList();

        CurrentRoom = newRoom;
        CurrentRoomRights = RoomRights.Owner;
        SendToRoom();

        SendChatMessage("Welcome to your room! Type /help for a list of commands.");
      } else {
        // This is probably only for command sub 3, which is information you get when you hover over a room in the lobby.
        // TODO: Make NSCSMOnline sub packet 3 (room info on hover)
        //MainClass.AddLog( "Discarded unknown sub-packet " + packetCommandSub.ToString() + " for NSCSMOnline" );
        ez.Discard();
      }
    }

    // Client sent style update
    public void NSCSU()
    {
      // We don't really care about this packet yet
      ez.Discard();
    }

    // Client sent chat message
    public void NSCCM()
    {
      if (!RequiresAuthentication()) return;

      string cmMessage = ez.ReadNT();
      try {
        if (cmMessage[0] == '/') {
          string[] cmdParse = cmMessage.Split(new char[] { ' ' }, 2);
          string cmdName = cmdParse[0].Substring(1);
          bool handled = false;

          if (mainClass.Scripting.ChatCommandHooks.ContainsKey(cmdName)) {
            for (int i = 0; i < mainClass.Scripting.ChatCommandHooks[cmdName].Count; i++) {
              bool subHandled = mainClass.Scripting.ChatCommandHooks[cmdName][i](this, cmdParse.Length == 2 ? cmdParse[1] : "");
              if (!handled) handled = subHandled;
            }
          }

          if (!handled)
            SendChatMessage("Unknown command. Type /help for a list of commands.");
        } else {
          bool cmHandled = false;
          for (int i = 0; i < mainClass.Scripting.ChatHooks.Count; i++)
            cmHandled = mainClass.Scripting.ChatHooks[i](this, cmMessage);
          if (!cmHandled)
            mainClass.SendChatAll(NameFormat() + ": " + cmMessage, CurrentRoom);
        }
      } catch (Exception ex) { mainClass.Scripting.HandleError(ex); }
    }

    void couldntReadData()
    {
      MainClass.AddLog("Couldn't read data from " + this.User_Name + ", user disconnecting");
      if (CurrentRoom != null) CurrentRoom = null;
      mainClass.Users.Remove(this);
      this.tcpClient.Close();
    }

    int pingTimer = 0;
    int pingTimeout = 5;
    public void Update()
    {
      if (++pingTimer == mainClass.FPS) {
        if (pingTimeout > 0) {
          pingTimer = 0;
          pingTimeout = 5;

          ez.Write1((byte)(mainClass.ServerOffset + NSCommand.NSCPing));
          ez.SendPack();
        } else {
          if (pingTimeout == 0) {
            MainClass.AddLog("Ping timeout for " + this.User_Name + ", user disconnecting");
            if (CurrentRoom != null) CurrentRoom = null;
            mainClass.Users.Remove(this);
            this.tcpClient.Close();
            return;
          } else {
            MainClass.AddLog("Timeout " + pingTimeout + " for user " + this.User_Name);
            pingTimeout--;
          }
        }
      }

      try {
        int a = tcpClient.Available;
      } catch {
        MainClass.AddLog("Socket closed.");
        if (CurrentRoom != null) CurrentRoom = null;
        mainClass.Users.Remove(this);
        return;
      }

      if (tcpClient.Available > 0) {
        try {
          if (ez.ReadPack() == -1) return;
        } catch {
          this.couldntReadData();
          return;
        }

        NSCommand packetCommand;
        try {
          packetCommand = (NSCommand)ez.Read1();
        } catch {
          this.couldntReadData();
          return;
        }

        switch (packetCommand) {
          case NSCommand.NSCPing: this.NSCPing(); break;
          case NSCommand.NSCPingR: this.NSCPingR(); break;
          case NSCommand.NSCHello: this.NSCHello(); break;
          case NSCommand.NSCSMS: this.NSCSMS(); break;
          case NSCommand.NSCGSR: this.NSCGSR(); break;
          case NSCommand.NSCGSU: this.NSCGSU(); break;
          case NSCommand.NSCGON: this.NSCGON(); break;
          case NSCommand.NSCRSG: this.NSCRSG(); break;
          case NSCommand.NSCUPOpts: this.NSCUPOpts(); break;
          case NSCommand.NSCSMOnline: this.NSCSMOnline(); break;
          case NSCommand.NSCSU: this.NSCSU(); break;
          case NSCommand.NSCCM: this.NSCCM(); break;
          default:
            MainClass.AddLog("Packet " + packetCommand.ToString() + " discarded!");
            ez.Discard();
            break;
        }

        if (mainClass.Scripting.PacketHooks.ContainsKey(packetCommand)) {
          for (int i = 0; i < mainClass.Scripting.PacketHooks[packetCommand].Count; i++) {
            try {
              mainClass.Scripting.PacketHooks[packetCommand][i](new HookInfo() { User = this, SubCommand = packetCommandSub });
            } catch (Exception ex) { mainClass.Scripting.HandleError(ex); }
          }
        }
      }
    }
  }
}
