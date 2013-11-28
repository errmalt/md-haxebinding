using System;
using System.IO;
using MonoDevelop.HaxeBinding.Projects;
using MonoDevelop.HaxeBinding.Tools;
using System.Collections.Generic;

namespace MonoDevelop.HaxeBinding.Tools {

	public class HxmlParser {

		public HxmlParser () {
		}

		// Main class
		public string Main;

		// out file or folder
		public string Out;

		// target
		public HaxeTarget? Target;

		// inline haxe args
		public string Args;

		public List<string> Libs;
		public List<string> ClassPaths;
		public Dictionary<string, string> Defines;

		string[] hxmlArgs;
		int ArgIndex;

		public void Parse(string hxmlFileContent) {

			Main = null;
			Out = null;
			Target = null;
			Libs = new List<string>();
			ClassPaths = new List<string> ();
			Defines = new Dictionary<string, string> ();

			string hxml = hxmlFileContent.Replace (Environment.NewLine, " ");
			hxmlArgs = hxml.Split (' ');
			Args = String.Join (" ", hxmlArgs);
			ArgIndex = 0;

			bool readOut = false;
			string line;
			while ((line = nextLine()) != null) {
				switch (line) {
				case "-D":
					string define = nextLine ();
					string[] split = define.Split ('=');
					if (split.Length > 1)
						Defines.Add (split [0], split [1]);
					else
						Defines.Add (split [0], split [0]);
					break;
				case "-cp":
					ClassPaths.Add (nextLine ());
					break;
				case "-lib":
					Libs.Add (nextLine ());
					break;
				case "-main":
					Main = nextLine();
					break;
				case "-swf":
					Target = HaxeTarget.Flash;
					readOut = true;
					break;
				case "-js":
					Target = HaxeTarget.Js;
					readOut = true;
					break;
				case "-neko":
					Target = HaxeTarget.Neko;
					readOut = true;
					break;
				case "-php":
					Target = HaxeTarget.Php;
					readOut = true;
					break;
				case "-cpp":
					Target = HaxeTarget.Cpp;
					readOut = true;
					break;
				case "-java":
					Target = HaxeTarget.Java;
					readOut = true;
					break;
				case "-cs":
					Target = HaxeTarget.Cs;
					readOut = true;
					break;
				}
				if (readOut) {
					Out = nextLine();
					readOut = false;
				}
			}
		}

		string nextLine() {
			string line = null;
			do {
				if (ArgIndex >= hxmlArgs.Length) return null;
				line = hxmlArgs [ArgIndex++];
			} while (line.Length == 0);
			if (line == "--next")
				return null;
			return line;
		}

		public string GetProjectPath(HaxeProject project, bool createOutFolder = true) {

			string path = Path.GetDirectoryName (Out);
			if (path.Length == 0)
				path = ".";
			path = Path.GetFullPath (path);
			if (!Directory.Exists (path)) {
				path = Path.Combine (project.BaseDirectory, Out);
				if (createOutFolder && !Directory.Exists (Path.GetDirectoryName (path))) {
					Directory.CreateDirectory (Path.GetDirectoryName (path));
				}
			}
			return path;
		}
	}


}

