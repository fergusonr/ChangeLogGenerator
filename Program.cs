using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace ChangeLogGenerator
{
	public static class Program
	{
		static void Main(string[] args)
		{
			#region commandline args
			var type = ReportGenerator.OutputType.None;

			if (args.ArgBool("html"))
				type = ReportGenerator.OutputType.Html;
			if (args.ArgBool("rtf"))
				type = ReportGenerator.OutputType.Rtf;
			if (args.ArgBool("md"))
				type = ReportGenerator.OutputType.Md;
			if (args.ArgBool("txt") || args.ArgBool("text"))
				type = ReportGenerator.OutputType.Txt;

			var repoPath = args.Arg("repo") ?? ".";
			var branch = args.Arg("branch");
			var noCredit = args.ArgBool("nocredit");
			var showVersion = args.ArgBool("version");

			var outFile = args.Arg("output");

			if(args.Length == 0)
			{
				ShowUsage();
				return;
			}

			// validate
			if (!args.Check(out string message))
			{
				Console.Error.WriteLine($"Error: {message}");
				ShowUsage();
				return;
			}

			void ShowUsage()
			{
				Console.Error.WriteLine($"Usage: {typeof(Program).Namespace} --txt | --rtf | --md | --html [--nocredit] [--repo path] [--branch name] [--output filename]");
			}

			if (showVersion)
			{
				var name = Assembly.GetExecutingAssembly().GetName();
				Console.Error.WriteLine($"{name.Name} {name.Version.Major}.{name.Version.Build}.{name.Version.Minor}.{name.Version.MinorRevision}");
				return;
			}

			if (type == ReportGenerator.OutputType.None)
			{
				Console.Error.WriteLine("Error: Specify file format --txt | --rtf | --md | --html");
				return;
			}

			if(outFile != null)
			{
				var ext = Path.GetExtension(outFile);

				if (ext == string.Empty)
				{
					outFile += "." + type.ToString().ToLower();
					Console.Error.WriteLine($"Outfile: {outFile}");
				}
				else if(!ext.Substring(1).Equals(type.ToString(), StringComparison.InvariantCultureIgnoreCase))
				{
					Console.Error.WriteLine($"Invalid extension {ext}");
					return;
				}
			}
			#endregion

			try
			{
				var parser = new ReportGenerator(repoPath, branch) { NoCredit = noCredit };

				using (TextWriter outStream = outFile == null ?  Console.Out : new StreamWriter(outFile))
				{
					parser.Generate(type, outStream);
				}
			}
			catch (Exception e)
			{
				Console.Error.WriteLine($"Error: {e.Message}");
			}
		}
	}

	internal static class ArgsExtensions
	{
		private static readonly Dictionary<string, (bool Mandatory, bool HasValue)> _known = new Dictionary<string, (bool, bool)>();

		// for	"-searchKey value"	return "value"
		internal static string Arg(this string[] args, string name, bool mandatory = false)
		{
			_known.Add($"--{name}", (mandatory, true));
			var index = Array.IndexOf(args, $"--{name}");
			return index != -1 && index + 1 < args.Length ? args[index + 1] : null;
		}

		// Simple boolean arg		"-someoption"	return true
		internal static bool ArgBool(this string[] args, string name, bool mandatory = false)
		{
			_known.Add($"--{name}", (mandatory, false));
			return args.Contains($"--{name}");
		}

		/// <summary>
		/// Check
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public static bool Check(this string[] args, out string message)
		{
			var unknown = args.Where(x => x.StartsWith("-")).Except(_known.Keys);

			if (unknown.Any())
			{
				message = $"Unknown argument(s): {string.Join(", ", unknown)}";
				return false;
			}

			var missing = _known.Where(x => x.Value.Mandatory).Select(x => x.Key).Except(args);

			if (missing.Any())
			{
				message = $"Missing argument(s): {string.Join(", ", missing)}";
				return false;
			}

			message = "";
			return true;
		}
	}
}
