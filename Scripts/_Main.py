# Main.py is called when OpenSMO has started.
# You shouldn't have to edit this script ever.
# To make a new script, just make a new *.py file.

import clr

clr.AddReferenceByPartialName("OpenSMO")
from OpenSMO import *
from OpenSMO.Func import *
from OpenSMO.Data import *
from System import *
from System.Diagnostics import *

import os
import random

Script = main.Scripting

execfile("Scripts/_Functions.py")

# Uncomment to spawn a simple Python shell in the server console.
#Script.Shell()

scripts = os.listdir("Scripts")
for script in scripts:
	if script[0] != "_":
		main.AddLog("Loading script: " + script)
		execfile("Scripts/" + script)