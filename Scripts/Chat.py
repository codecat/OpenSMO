class Chat:
  def Init(self):
    # Hook chat commands
    Script.HookChatCommand("me",            self.Me)
    Script.HookChatCommand("time",          self.Time)
    Script.HookChatCommand("spectate",      self.Spectate)
    Script.HookChatCommand("free",          self.Free)
    Script.HookChatCommand("kick",          self.Kick)
    Script.HookChatCommand("ghost",         self.Ghost)
    Script.HookChatCommand("op",            self.Op)
    Script.HookChatCommand("deop",          self.DeOp)
    Script.HookChatCommand("title",         self.Title)
    Script.HookChatCommand("desc",          self.Desc)
    Script.HookChatCommand("pass",          self.Pass)
    Script.HookChatCommand("secret",        self.Secret)
    Script.HookChatCommand("ban",           self.Ban)
    Script.HookChatCommand("online",        self.Online)
    Script.HookChatCommand("reload",        self.Reload)
    Script.HookChatCommand("restartserver", self.RestartServer)
    Script.HookChatCommand("stopserver",    self.StopServer)
    Script.HookChatCommand("help",          self.Help)
    Script.HookChatCommand("eval",          self.Eval)

    # Hook user name formatting
    Script.HookNameFormat(self.NameFormat, True)
    Script.HookWebFormat(self.WebFormat, True)

  def Me(self, user, args):
    if args != "":
      main.SendChatAll(user.NameFormat() + " " + args, user.CurrentRoom)
    
    return True

  def Time(self, user, args):
    user.SendChatMessage("You've played for " + ChatColor("00aa00") \
                          + str(user.PlayTime.Elapsed.Hours) + " hours, " \
                          + str(user.PlayTime.Elapsed.Minutes) + " minutes" \
                          + ChatColor("ffffff") + " this session.")
    
    return True

  def Spectate(self, user, args):
    if parseBool(config.Get("Allow_Spectators")):
      user.Spectating = not user.Spectating

      specstr = ChatColor("00aa00") + "not spectating"
      if user.Spectating:
        specstr = ChatColor("aa0000") + "spectating"
      main.SendChatAll(user.NameFormat() + " is now " + specstr, user.CurrentRoom)
    else:
      user.SendChatMessage(ChatColor("aa0000") + "Spectator mode is disabled on this server.")

    return True

  def Free(self, user, args):
    if user.CurrentRoom != None and user.CanChangeRoomSettings():
      user.CurrentRoom.Free = not user.CurrentRoom.Free

      freestr = ChatColor("aa0000") + "disabled"
      if user.CurrentRoom.Free:
        freestr = ChatColor("00aa00") + "enabled"
      main.SendChatAll("Free mode has been " + freestr + ChatColor("ffffff") + " by " + user.NameFormat(), user.CurrentRoom)
    return True

  def Kick(self, user, args):
    if user.CurrentRoom == None:
      if user.IsModerator():
        for u in main.Users.ToArray():
          if u.User_Name.lower() == args.lower():
            u.Kick()
      else:
        user.SendChatMessage("You're not a moderator!")
    else:
      if user.CanChangeRoomSettings():
        roomUsers = user.CurrentRoom.Users.ToArray()
        for u in roomUsers:
          if u.User_Name.lower() == args.lower():
            user.CurrentRoom.Users.Remove(u)
            u.CurrentRoom = None
            u.SendToRoom()
            u.SendChatMessage(ChatColor("aa0000") + "Kicked from room!")
        for u in roomUsers:
          u.SendRoomPlayers()
      else:
        user.SendChatMessage("You're not a room operator!")

    return True

  def Ghost(self, user, args):
    if user.IsModerator():
      for u in main.Users.ToArray():
        if u.User_Name.lower() == args.lower():
          u.Kick()
    else:
      user.SendChatMessage("You're not a moderator!")

    return True

  def Op(self, user, args):
    if user.CurrentRoom != None and user.CanChangeRoomSettings():
      roomUsers = user.CurrentRoom.Users.ToArray()
      for u in roomUsers:
        if u.User_Name.lower() == args.lower():
          if u.CurrentRoomRights == RoomRights.Player:
            u.CurrentRoomRights = RoomRights.Operator
            main.SendChatAll(u.NameFormat() + " is now a room operator.", user.CurrentRoom)

    return True

  def DeOp(self, user, args):
    if user.CurrentRoom != None and user.CanChangeRoomSettings():
      roomUsers = user.CurrentRoom.Users.ToArray()
      for u in roomUsers:
        if u.User_Name.lower() == args.lower():
          if u.CurrentRoomRights == RoomRights.Operator:
            u.CurrentRoomRights = RoomRights.Player
            main.SendChatAll(u.NameFormat() + " is no longer a room operator.", user.CurrentRoom)

    return True

  def Title(self, user, args):
    if user.CanChangeRoomSettings():
      user.CurrentRoom.Name = args
      main.SendChatAll("Room title updated: " + ChatColor("00aa00") + args, user.CurrentRoom)
    else:
      user.SendChatMessage("You're not a room operator!")

    return True

  def Desc(self, user, args):
    if user.CanChangeRoomSettings():
      user.CurrentRoom.Description = args
      main.SendChatAll("Room description updated: " + ChatColor("00aa00") + args, user.CurrentRoom)
    else:
      user.SendChatMessage("You're not a room operator!")

    return True

  def Pass(self, user, args):
    if user.CanChangeRoomSettings():
      user.CurrentRoom.Password = args
      main.SendChatAll("Room password changed!", user.CurrentRoom)
    else:
      user.SendChatMessage("You're not a room operator!")

    return True

  def Secret(self, user, args):
    if user.CanChangeRoomSettings():
      user.CurrentRoom.Secret = not user.CurrentRoom.Secret
      secretstr = ChatColor("00aa00") + "public"
      if user.CurrentRoom.Secret:
        secretstr = ChatColor("aa0000") + "secret"
      main.SendChatAll("Room is now " + secretstr + ChatColor("ffffff") + "!", user.CurrentRoom)
    else:
      user.SendChatMessage("You're not a room operator!")

    return True

  def Ban(self, user, args):
    if user.IsModerator():
      for u in main.Users.ToArray():
        if u.User_Name.lower() == args.lower():
          IP = BanUser(u, user.User_ID)
          if parseBool(config.Get("Game_ShadowBan")):
            u.ShadowBanned = True
            user.SendChatMessage("IP " + ChatColor("aa0000") + IP + ChatColor("ffffff") + " has been shadowbanned.")
          else:
            user.SendChatMessage("IP " + ChatColor("aa0000") + IP + ChatColor("ffffff") + " has been banned.")
    else:
      user.SendChatMessage("You're not a moderator!")

    return True

  def Online(self, user, args):
    userCount = len(main.Users)

    if userCount != 1:
      user.SendChatMessage("There are " + str(userCount) + " users online!")
    else:
      user.SendChatMessage("You're the only one online.")

    return True

  def Reload(self, user, args):
    if user.IsAdmin():
      main.AddLog("Reload called")
      main.ReloadScripts()
      user.SendChatMessage(ChatColor("00aa00") + "Scripts reloaded")
    else:
      user.SendChatMessage("You're not an admin!")

    return True

  def RestartServer(self, user, args):
    if user.IsAdmin():
      main.AddLog("Server restart issued by " + user.User_Name, True)

      Process.Start("StepServer.exe")
      Process.GetCurrentProcess().Kill()
    else:
      user.SendChatMessage("You're not an admin!")

    return True

  def StopServer(self, user, args):
    if user.IsAdmin():
      main.AddLog("Server stopped by " + user.User_Name, true)

      Process.GetCurrentProcess().Kill()
    else:
      user.SendChatMessage("You're not an admin!")
    
    return True

  def Help(self, user, args):
    user.SendChatMessage(ChatColor("aaaa00") + "Commands:")

    user.SendChatMessage("-  /me <message>")
    user.SendChatMessage("-  /time")

    if user.CurrentRoom != None:
      if parseBool(config.Get("Allow_Spectators")):
        user.SendChatMessage("-  /spectate")

      if user.CurrentRoomRights == RoomRights.Owner:
        user.SendChatMessage(ChatColor("9d009d") + "-  /op <user>")
        user.SendChatMessage(ChatColor("9d009d") + "-  /deop <user>")

      if user.CanChangeRoomSettings():
        user.SendChatMessage(ChatColor("00aaaa") + "-  /free")
        user.SendChatMessage(ChatColor("00aaaa") + "-  /title <new title>")
        user.SendChatMessage(ChatColor("00aaaa") + "-  /desc <new description>")
        user.SendChatMessage(ChatColor("00aaaa") + "-  /pass <new password>")
        user.SendChatMessage(ChatColor("00aaaa") + "-  /secret")
        user.SendChatMessage(ChatColor("00aaaa") + "-  /kick")

    if user.IsModerator():
      user.SendChatMessage(ChatColor("00aa00") + "-  /ban <user>")
      user.SendChatMessage(ChatColor("00aa00") + "-  /ghost <user>")

    if user.IsAdmin():
      user.SendChatMessage(ChatColor("aa0000") + "-  /restartserver")
      user.SendChatMessage(ChatColor("aa0000") + "-  /stopserver")

    return True

  def Eval(self, user, args):
    if user.IsAdmin():
      user.SendChatMessage(str(eval(args)))
      return True

  def NameFormat(self, user, current):
    pre = ""
    post = ChatColor("ffffff")

    if user.CurrentRoom != None:
      if user.CurrentRoomRights == RoomRights.Operator:
        pre = ChatColor("00aaaa") + "+"
      elif user.CurrentRoomRights == RoomRights.Owner:
        pre = ChatColor("9d009d") + "~"

    if user.User_Rank == UserRank.User:
      pre += ChatColor("aaaaaa")
    elif user.User_Rank == UserRank.Moderator:
      pre += ChatColor("00aa00")
    elif user.User_Rank  == UserRank.Admin:
      pre += ChatColor("aa0000")

    return pre + current + post

  def WebFormat(self, user, current):
    pre = ChatColor("cccccc") + "(Web)"
    post = ChatColor("ffffff")

    if user["Rank"] == "0":
      pre += ChatColor("aaaaaa")
    elif user["Rank"] == "1":
      pre += ChatColor("00aa00")
    elif user["Rank"] == "2":
      pre += ChatColor("aa0000")

    return pre + current + post

chat = Chat()
chat.Init()