class Steps:
	def Init(self):
		Script.HookChatCommand("steps", self.StepsCommand)
		Script.HookPacket(NSCommand.NSCGON, self.NSCGON)

	def InitSteps(self, user):
		if not user.Meta.ContainsKey("Steps"):
			user.Meta["Steps"] = [0, 0, 0, 0, 0, 0]

	def StepsCommand(self, user, args):
		self.InitSteps(user)
		steps = user.Meta["Steps"]
		user.SendChatMessage(ChatColor("00aaaa") + "Steps this session:")
		user.SendChatMessage("Flawless: " + ChatColor("00aa00") + str(steps[5]))
		user.SendChatMessage("Perfect: " + ChatColor("00aa00") + str(steps[4]))
		user.SendChatMessage("Great: " + ChatColor("00aa00") + str(steps[3]))
		user.SendChatMessage("Good: " + ChatColor("00aa00") + str(steps[2]))
		user.SendChatMessage("Boo: " + ChatColor("00aa00") + str(steps[1]))
		user.SendChatMessage("Miss: " + ChatColor("00aa00") + str(steps[0]))
		return True

	def NSCGON(self, info):
		self.InitSteps(info.User)
		for i in range(3, 9):
			info.User.Meta["Steps"][i - 3] += info.User.Notes[i]

steps = Steps()
steps.Init()