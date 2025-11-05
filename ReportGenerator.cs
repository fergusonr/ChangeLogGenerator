using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using System.Diagnostics;

using LibGit2Sharp;

namespace ChangeLogGenerator
{
	public class ReportGenerator
	{
		public enum OutputType { None, Txt, Rtf, Html, Md };

		public bool NoCredit { get; set; }

		internal class ChangeLog
		{
			internal string Name; // branch name
			internal Dictionary<Tag, List<string>> Commits = new Dictionary<Tag, List<string>>();
		}
		internal struct Tag
		{
			internal string Name;
			internal DateTime Date;
		}

		private readonly Repository _repo;
		private readonly Branch _branch;

		private readonly ChangeLog _changeLog = new ChangeLog();
		private const string _untagged = "Untagged";

		/// <summary>
		/// Git log generator
		/// </summary>
		public ReportGenerator(string repoPath, string namedBranch)
		{
			_repo = new Repository(repoPath);

			if (namedBranch == null)
				_branch = _repo.Branches.First(); // master/main
			else
			{
				_branch = _repo.Branches.FirstOrDefault(x => x.FriendlyName == namedBranch);

				if (_branch == null)
					throw new Exception($"Branch {namedBranch} not found");
			}

			_changeLog.Name = _branch.FriendlyName;

			Console.Error.WriteLine($"{_branch.FriendlyName} commits {_branch.Commits.Count():#,##0} tags {_repo.Tags.Count():#,##0}");

			var timer = new Stopwatch();
			timer.Start();

			var currentTag = new Tag();
			foreach (var commit in _branch.Commits.OrderByDescending(x => x.Author.When))
			{
				var tag = _repo.Tags.FirstOrDefault(x => commit.Sha == x.Reference.TargetIdentifier);

				if (tag != null)
				{
					currentTag.Name = tag.FriendlyName;
					currentTag.Date = commit.Author.When.Date;
				}

				if (currentTag.Date == DateTime.MinValue)
				{
						currentTag.Name = _untagged;
						currentTag.Date = commit.Author.When.Date;
				}

				if (!_changeLog.Commits.ContainsKey(currentTag))
					_changeLog.Commits[currentTag] = new List<string>();

				_changeLog.Commits[currentTag].Add(commit.Message.TrimEnd('\r', '\n'));
			}

			timer.Stop();

			Console.Error.WriteLine($"Duration {timer.Elapsed.Hours:00}:{timer.Elapsed.Minutes:00}:{timer.Elapsed.Seconds:00}");

			_repo.Dispose();
		}

		/// <summary>
		/// Generate log report
		/// </summary>
		public void Generate(OutputType outputType, TextWriter outStream)
		{
			const string text = "Generated with";
			string gitUrl = $"https://github.com/fergusonr/{typeof(ReportGenerator).Namespace}";

			// tag version banner
			var bCol = Color.DarkGreen;
			var bColU = Color.Orange;
			var fCol = Color.White;

			#region Html
			///
			/// Html
			///
			if (outputType == OutputType.Html)
			{
				outStream.WriteLine("<html>\n<body>");

				foreach (var tag in _changeLog.Commits.OrderByDescending(x => x.Key.Date))
				{
					var col = tag.Key.Name == _untagged ? bColU : bCol;

					outStream.WriteLine($"<b style=\"background-color:rgb({col.R},{col.G},{col.B});color:rgb({fCol.R},{fCol.G},{fCol.B})\">&nbsp;{tag.Key.Name}&nbsp;</b>");
					outStream.WriteLine($"<table>\n<tr><td><b>{tag.Key.Date.ToLongDateString()}</b></td></tr>");

					foreach (var message in tag.Value)
					{
						var messageMod = message.Replace("\n", "<br>\n&ensp;&ensp;");

						outStream.WriteLine($"<tr><td>&nbsp;&#x2022;&nbsp;{messageMod}</td></tr>");
					}

					outStream.WriteLine("</table>\n<br>");
				}

				outStream.WriteLine($"Branch: {_changeLog.Name}<br>");

				if (!NoCredit)
					outStream.WriteLine($@"{text}: <a href=""{gitUrl}"">{gitUrl}</a>");

				outStream.WriteLine("</body>\n</html>");
			}
			#endregion

			#region Markdown
			///
			/// Markdown
			///
			else if (outputType == OutputType.Md)
			{
				foreach (var tag in _changeLog.Commits.OrderByDescending(x => x.Key.Date))
				{
					var col = tag.Key.Name == _untagged ? bColU : bCol;

					outStream.WriteLine($"#### <span style=\"background-color:rgb({col.R},{col.G},{col.B});color:rgb({fCol.R},{fCol.G},{fCol.B})\">{tag.Key.Name}</span>\n**{tag.Key.Date.ToLongDateString()}**");

					foreach (var message in tag.Value)
					{
						var messageMod = message.Replace("\n", "  \n&ensp;");

						outStream.WriteLine($"- {messageMod}");
					}
				}

				outStream.WriteLine();

				outStream.WriteLine($"Branch: {_changeLog.Name}<br>");

				if (!NoCredit)
					outStream.WriteLine($"{text}: [{gitUrl}]({gitUrl})");
			}
			#endregion

			#region Rtf
			///
			/// Rtf
			///
			else if (outputType == OutputType.Rtf)
			{
				outStream.WriteLine(@"{\rtf1\ansi{\fonttbl\f0\fCourier New;}");
				outStream.WriteLine($@"{{\colortbl;\red{fCol.R}\green{fCol.G}\blue{fCol.B};\red{bCol.R}\green{bCol.G}\blue{bCol.B};\red{bColU.R}\green{bColU.G}\blue{bColU.B};}}");

				foreach (var tag in _changeLog.Commits.OrderByDescending(x => x.Key.Date))
				{
					var col = tag.Key.Name == _untagged ? 3 : 2;

					outStream.WriteLine($@"{{\pard\li0\highlight1\cf1\highlight{col}\b1  {tag.Key.Name} }}\line\b1 {tag.Key.Date.ToLongDateString()}\b0\par");

					outStream.WriteLine(@"{\pard\li400");

					foreach (var message in tag.Value)
					{
						var messageMod = message.Replace("\\", "\\'5c"); // Escape rtf identifiers...
						messageMod = messageMod.Replace("{", "\\'7b"); 
						messageMod = messageMod.Replace("}", "\\'7d");
						messageMod = messageMod.Count(x => x == '\n') > 1 ? messageMod.Replace("\n", "\\line\\tab\n") : messageMod;

						outStream.WriteLine($@"\bullet  {messageMod}\line");
					}

					outStream.WriteLine(@"\par}");
				}

				outStream.WriteLine($@"\fs20Branch: {_changeLog.Name}\line");

				if (!NoCredit)
					outStream.WriteLine($@"\fs20{text}: {{\field{{\*\fldinst HYPERLINK ""{gitUrl}""}}}}\line");

				outStream.WriteLine("}");
			}
			#endregion

			#region Text
			///
			/// Text
			///
			else
			{
				foreach (var tag in _changeLog.Commits.OrderByDescending(x => x.Key.Date))
				{
					Console.BackgroundColor = tag.Key.Name == _untagged ? ConsoleColor.DarkYellow : ConsoleColor.DarkGreen;
					Console.ForegroundColor = ConsoleColor.White;

					outStream.Write($" {tag.Key.Name} ");

					// https://stackoverflow.com/questions/31140768/console-resetcolor-is-not-resetting-the-line-after-completely
					Console.ResetColor();
					outStream.WriteLine();

					Console.BackgroundColor = ConsoleColor.DarkGray;
					Console.ForegroundColor = ConsoleColor.White;

					outStream.Write($" {tag.Key.Date.ToLongDateString()} ");

					Console.ResetColor();
					outStream.WriteLine();

					foreach (var message in tag.Value)
					{
						var messageMod = message.Replace("\n", "\n\t");

						outStream.WriteLine($"  {messageMod}");
					}

					outStream.WriteLine();
				}

				outStream.WriteLine($"Branch: {_changeLog.Name}");

				if (!NoCredit)
				{
					outStream.WriteLine();
					outStream.WriteLine($"{text}: {gitUrl}");
				}
			}
			#endregion
		}
	}
}
