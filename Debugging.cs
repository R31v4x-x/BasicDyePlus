using System;
using Terraria;
using Terraria.Chat;
using Terraria.Localization;

namespace BasicDyePlus {
	internal static class Debugging {
		internal static void ChatOverhead(string message, int duration = 5) {
#if DEBUG
			if (Main.dedServ) return;
			Main.LocalPlayer.chatOverhead.NewMessage(message, duration);
#endif
		}
		internal static void ChatOverhead(object message, int duration = 5) {
			ChatOverhead(message?.ToString() ?? "", duration);
		}
		internal static void ChatMessage(string message, bool all = false) {
#if DEBUG
			if (Main.dedServ) return;
			if (all) {
				ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message), Color.White);
			} else {
				Main.NewText(message);
			}
#endif
		}
		internal static void ChatMessage(object message, bool all = false) {
			ChatMessage(message.ToString(), all);
		}
		internal static void Assert(bool value, Exception exception) {
			if (!value) throw exception;
		}
		static bool firstFirstUpdate = true;
		internal static bool firstUpdate = false;
		public static void LogFirstRun(string name, bool isGameLoop = false) {
			if (firstFirstUpdate && isGameLoop) {
				firstUpdate = true;
				firstFirstUpdate = false;
			}
			if (firstUpdate) {
				BasicDyePlus.instance.Logger.Info($"Running {name} for the first time");
			}
		}
		public static void LogFirstRun(Delegate del, bool isGameLoop = false) => LogFirstRun($"{del.Method.DeclaringType.Name}.{del.Method.Name}", isGameLoop);
	}
}
