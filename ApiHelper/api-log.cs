using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillCity {
	public static partial class Api {
		public static readonly string LOG_TAG = "api";

		private class LogHandler : ILogHandler {
			public static List<string> nodes;
			public LogHandler() {
				if (nodes == null) {
					nodes = new List<string>();
				}
			}

			public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args) {
				Debug.unityLogger.logHandler.LogFormat(logType, context, format, args);
				nodes.Add($"<{System.DateTime.Now.ToString()}> [{logType.ToString()}]\t {System.String.Format(format, args)} {(context == null ? "" : "<context:" + context.ToString() + ">")}");
			}

			public void LogException(System.Exception exception, UnityEngine.Object context) {
				Debug.unityLogger.LogException(exception, context);
				nodes.Add($"<{System.DateTime.Now.ToString()}> [{LogType.Exception.ToString()}]\t {exception} {(context == null ? "" : "<context:" + context.ToString() + ">")}");
			}
		}

		public static List<string> log => LogHandler.nodes;

		private static Logger _log;
		public static Logger logger { get {
			if (_log == null) {
				_InitializeLogging();
			}
			return _log;
		} }

		private static void _InitializeLogging() {
			Debug.Log("initializing API logging");
			_log = new Logger(new LogHandler());
		}

		public static void SaveLog() {
			// TODO:
		}
	}
}