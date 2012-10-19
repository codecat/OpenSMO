# OpenSMO hardcore mode

class Hardcore:
	def Init(self):
		Script.HookPacket(NSCommand.NSCGSU, self.NSCGSU)
		Script.HookPacket(NSCommand.NSCSMOnline, self.NSCSMOnline)
		Script.HookChatCommand("hc", self.HCCommand)

	def HCCommand(self, user, args):
		if user.CanChangeRoomSettings():
			user.CurrentRoom.Meta["HC_Enabled"] = not user.CurrentRoom.Meta["HC_Enabled"]

			hcstr = ChatColor("00aa00") + "disabled"
			if user.CurrentRoom.Meta["HC_Enabled"]:
				hcstr = ChatColor("aa0000") + "enabled"
			main.SendChatAll("Hardcore mode has been " + hcstr + ChatColor("ffffff") + "!", user.CurrentRoom)
		else:
			user.SendChatMessage("You're not a room operator!")

		return True

	def NSCGSU(self, info):
		if info.User.CurrentRoom.Meta["HC_Enabled"]:
			if info.User.Combo > 0 and info.User.Combo % 50 == 0:
				roomUsers = info.User.CurrentRoom.Users
				if len(roomUsers) == 1:
					return

				attackUser = roomUsers[random.randint(0, len(roomUsers) - 1)]
				while attackUser == info.User:
					attackUser = roomUsers[random.randint(0, len(roomUsers) - 1)]

				attackUser.SendAttack(str(info.User.Combo) + "% Expand, " + str(info.User.Combo) + "% Bumpy, " + str(info.User.Combo) + "% Wave", info.User.Combo * 2)

	def NSCSMOnline(self, info):
		if info.SubCommand == 1:
			if info.User.CurrentRoom != None:
				if info.User.CurrentRoom.Meta["HC_Enabled"]:
					info.User.SendChatMessage(ChatColor("aa0000") + "WARNING: Hardcore mode enabled!")
		elif info.SubCommand == 2:
			info.User.CurrentRoom.Meta["HC_Enabled"] = False

hardcore = Hardcore()
hardcore.Init()