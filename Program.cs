using System;
using System.CommandLine;
using System.IO;

namespace ChangeLogGenerator
{
	public static class Program
	{
		static void Main(string[] args)
		{
			#region commandline args
			var oRepoPath = new Option<string>("--repo") { Description = "Repo path", DefaultValueFactory = r => "." };
			oRepoPath.AcceptLegalFilePathsOnly();
			var oBranch = new Option<string>("--branch") { Description = "Branch name" };
			var oType = new Option<ReportGenerator.OutputType>("--type") { Description = "Output format", Required = true };
			var oNoCredit = new Option<bool>("--nocredit") { Description = "No credit" };
			var oOutput = new Option<string>("--output") { Description = "Output file" };
			oOutput.AcceptLegalFilePathsOnly();

			var rootCommand = new RootCommand(nameof(ChangeLogGenerator)) { oRepoPath, oBranch, oType, oNoCredit, oOutput };
			rootCommand.TreatUnmatchedTokensAsErrors = true;
		
			var results = rootCommand.Parse(args);
			results.Invoke();

			if ((results.Action?.Terminating) ?? false)
				return;

			var outFile = results.GetValue(oOutput);
			var type = results.GetValue(oType);

			if (outFile != null)
			{
				var ext = Path.GetExtension(outFile);

				if (ext == string.Empty)
				{
					outFile += "." + type.ToString().ToLower();
					Console.Error.WriteLine($"Output file: {outFile}");
				}
				else if(!ext[1..].Equals(type.ToString(), StringComparison.InvariantCultureIgnoreCase))
				{
					Console.Error.WriteLine($"Invalid extension {ext}");
					return;
				}
			}
			#endregion

			try
			{
				var parser = new ReportGenerator(results.GetValue(oRepoPath), results.GetValue(oBranch)) { NoCredit = results.GetValue(oNoCredit) };

				using (TextWriter outStream = outFile == null ? Console.Out : new StreamWriter(outFile))
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
}
