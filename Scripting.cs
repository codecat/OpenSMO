using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace OpenSMO {
    public class HookInfo {
        public User User;
        public int SubCommand;
    }

    public class Scripting {
        public ScriptRuntime Host;
        public ScriptScope Scope;
        public ScriptEngine Engine;

        public delegate void UpdateHookCall();
        public delegate void PacketHookCall(HookInfo info);
        public delegate bool ChatHookCall(User user, string message);
        public delegate bool ChatCommandHookCall(User user, string args);
        public delegate string NameFormatHookCall(User user, string current);
        public delegate int PlayerXPHookCall(User user, int current);

        public Dictionary<NSCommand, List<PacketHookCall>> PacketHooks;
        public List<UpdateHookCall> UpdateHooks;
        public List<ChatHookCall> ChatHooks;
        public Dictionary<string, List<ChatCommandHookCall>> ChatCommandHooks;
        public List<NameFormatHookCall> NameFormatHooks;
        public List<PlayerXPHookCall> PlayerXPHooks;

        public Scripting() {
            Host = Python.CreateRuntime();
            Scope = Host.CreateScope();
            Engine = Host.GetEngine("Python");

            PacketHooks = new Dictionary<NSCommand, List<PacketHookCall>>();
            UpdateHooks = new List<UpdateHookCall>();
            ChatHooks = new List<ChatHookCall>();
            ChatCommandHooks = new Dictionary<string, List<ChatCommandHookCall>>();
            NameFormatHooks = new List<NameFormatHookCall>();
            PlayerXPHooks = new List<PlayerXPHookCall>();
        }

        public void HookPacket(NSCommand command, PacketHookCall call, bool priority = false) {
            if (!PacketHooks.ContainsKey(command))
                PacketHooks.Add(command, new List<PacketHookCall>());

            if (priority) PacketHooks[command].Insert(0, call);
            else PacketHooks[command].Add(call);
        }

        public void HookUpdate(UpdateHookCall call, bool priority = false) {
            if (priority) UpdateHooks.Insert(0, call);
            else UpdateHooks.Add(call);
        }

        public void HookChat(ChatHookCall call, bool priority = false) {
            if (priority) ChatHooks.Insert(0, call);
            else ChatHooks.Add(call);
        }

        public void HookChatCommand(string command, ChatCommandHookCall call, bool priority = false) {
            if (!ChatCommandHooks.ContainsKey(command))
                ChatCommandHooks.Add(command, new List<ChatCommandHookCall>());

            if (priority) ChatCommandHooks[command].Insert(0, call);
            else ChatCommandHooks[command].Add(call);
        }

        public void HookNameFormat(NameFormatHookCall call, bool priority = false) {
            if (priority) NameFormatHooks.Insert(0, call);
            else NameFormatHooks.Add(call);
        }

        public void HookPlayerXP(PlayerXPHookCall call, bool priority = false) {
            if (priority) PlayerXPHooks.Insert(0, call);
            else PlayerXPHooks.Add(call);
        }

        public void Start() {
            try {
                Engine.ExecuteFile("Scripts/_Main.py", Scope);
            } catch (Exception ex) { HandleError(ex); }
        }

        public void HandleError(Exception ex) {
            MainClass.AddLog("Scripting error: " + Engine.GetService<ExceptionOperations>().FormatException(ex), true);
        }

        public void Shell() {
            while (true) {
                Console.Write(">>> ");
                try {
                    string input = Console.ReadLine();
                    if (input.Trim().ToLower() == "exit") return;
                    Console.WriteLine(Engine.Execute("str(" + input + ")", Scope));
                } catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
