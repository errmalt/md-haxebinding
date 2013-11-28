using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MonoDevelop.HaxeBinding.Tools
{
	public class HaxelibTools
	{
		private HaxelibTools ()
		{
		}

		static Dictionary<string, List<string>> cache = new Dictionary<string, List<string>>();

		public static bool IsLibraryInstalled(string library) {
			return GetLibraryPath (library).ToArray ().Length > 0;
		}

		public static List<string> GetLibraryPath(string library)
		{
			if (cache.ContainsKey (library)) {
				return cache [library];
			}
			ProcessStartInfo info = new ProcessStartInfo ();

			info.FileName = "haxe";
			info.Arguments = "--run tools.haxelib.Main path " + library;
			info.UseShellExecute = false;
			info.RedirectStandardOutput = true;
			info.RedirectStandardError = true;
			//info.WindowStyle = ProcessWindowStyle.Hidden;
			info.CreateNoWindow = true;
			string data;
			using (Process process = Process.Start (info))
			{
				data = process.StandardOutput.ReadToEnd ();
				process.WaitForExit ();
			}
			var libsPaths = new List<string> ();
			var dataList = data.Split (Environment.NewLine.ToCharArray());
			foreach (string line in dataList) {
				line = line.Trim ();
				if (!line.StartsWith ("-D ") && !line.StartsWith ("-L ") && !line.StartsWith("Library " + library + " is not installed")) {
					libsPaths.Add (line);
				}
			}
			cache.Add (library, libsPaths);
			return libsPaths;
		}
	}
}

