using System;
using System.IO;
using MonoDevelop.Core.Execution;
using Mono.Debugging.Backend;
using Mono.Debugging.Client;
using MonoDevelop.Debugger;
using System.Collections.Generic;

namespace MonoDevelop.HaxeBinding
{
	public class HxcppDebuggerFactory: IDebuggerEngine
	{
		public HxcppDebuggerFactory ()
		{
		}

		struct FileData {
			public DateTime LastCheck;
			public bool IsExe;
		}

		Dictionary<string,FileData> fileCheckCache = new Dictionary<string, FileData> ();

		public bool CanDebugCommand (ExecutionCommand command)
		{
			NativeExecutionCommand cmd = command as NativeExecutionCommand;
			if (cmd == null)
				return false;

			string file = FindFile (cmd.Command);
			if (!File.Exists (file)) {
				// The provided file is not guaranteed to exist. If it doesn't
				// we assume we can execute it because otherwise the run command
				// in the IDE will be disabled, and that's not good because that
				// command will build the project if the exec doesn't yet exist.
				return true;
			}

			file = Path.GetFullPath (file);
			DateTime currentTime = File.GetLastWriteTime (file);

			FileData data;
			if (fileCheckCache.TryGetValue (file, out data)) {
				if (data.LastCheck == currentTime)
					return data.IsExe;
			}
			data.LastCheck = currentTime;
			try {
				data.IsExe = IsExecutable (file);
			} catch {
				data.IsExe = false;
			}
			fileCheckCache [file] = data;
			return data.IsExe;
		}

		public DebuggerStartInfo CreateDebuggerStartInfo (ExecutionCommand command)
		{
			NativeExecutionCommand pec = (NativeExecutionCommand) command;
			DebuggerStartInfo startInfo = new DebuggerStartInfo ();
			startInfo.Command = pec.Command;
			startInfo.Arguments = pec.Arguments;
			startInfo.WorkingDirectory = pec.WorkingDirectory;
			if (pec.EnvironmentVariables.Count > 0) {
				foreach (KeyValuePair<string,string> val in pec.EnvironmentVariables)
					startInfo.EnvironmentVariables [val.Key] = val.Value;
			}
			return startInfo;
		}

		public bool IsExecutable (string file)
		{
			// HACK: this is a quick but not very reliable way of checking if a file
			// is a native executable. Actually, we are interested in checking that
			// the file is not a script.
			using (StreamReader sr = new StreamReader (file)) {
				char[] chars = new char[3];
				int n = 0, nr = 0;
				while (n < chars.Length && (nr = sr.ReadBlock (chars, n, chars.Length - n)) != 0)
					n += nr;
				if (nr != chars.Length)
					return true;
				if (chars [0] == '#' && chars [1] == '!')
					return false;
			}
			return true;
		}

		public DebuggerSession CreateSession ()
		{
			HxcppDbgSession ds = new HxcppDbgSession ();
			return ds;
		}

		public ProcessInfo[] GetAttachableProcesses ()
		{
			List<ProcessInfo> procs = new List<ProcessInfo> ();
			foreach (string dir in Directory.GetDirectories ("/proc")) {
				int id;
				if (!int.TryParse (Path.GetFileName (dir), out id))
					continue;
				try {
					File.ReadAllText (Path.Combine (dir, "sessionid"));
				} catch {
					continue;
				}
				string cmdline = File.ReadAllText (Path.Combine (dir, "cmdline"));
				cmdline = cmdline.Replace ('\0',' ');
				ProcessInfo pi = new ProcessInfo (id, cmdline);
				procs.Add (pi);
			}
			return procs.ToArray ();
		}

		string FindFile (string cmd)
		{
			if (Path.IsPathRooted (cmd))
				return cmd;
			string pathVar = Environment.GetEnvironmentVariable ("PATH");
			string[] paths = pathVar.Split (Path.PathSeparator);
			foreach (string path in paths) {
				string file = Path.Combine (path, cmd);
				if (File.Exists (file))
					return file;
			}
			return cmd;
		}
	}
}

