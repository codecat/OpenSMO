# OpenSMO gag chat

class Gag:
	GagChars = "afwAFW"

	def Init(self):
		Script.HookChatCommand("gag", self.GagCommand)
		Script.HookChat(self.Chat)

	def GagText(self, orig):
		ret = ""
		for i in range(0, len(orig)):
			char = orig[i]
			if char == " ":
				ret += " "
			else:
				ret += self.GagChars[random.randint(0, len(self.GagChars) - 1)]
		return ret

	def Gagged(self, user):
		return user.Meta.ContainsKey("Gagged") and user.Meta["Gagged"]

	def GagCommand(self, user, args):
		if user.IsModerator():
			for u in main.Users:
				if u.User_Name.lower() == args.lower():
					u.Meta["Gagged"] = not self.Gagged(u)
					gagstr = ChatColor("00aa00") + "ungagged"
					if u.Meta["Gagged"]:
						gagstr = ChatColor("aa0000") + "gagged"
					main.SendChatAll(u.NameFormat() + " got " + gagstr + ChatColor("ffffff") + "!")

		return True

	def Chat(self, user, message):
		if self.Gagged(user):
			main.SendChatAll(user.NameFormat() + ": " + self.GagText(message))
			return True

gag = Gag()
gag.Init()