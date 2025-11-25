using System;
using System.IO;
using System.Text;
using System.Linq;

namespace Helpers.Pagers
{
	/// <summary>
	/// Unix style "more" Text Pager
	/// </summary>
	public class Pager : TextWriter
	{
		/// <summary>
		/// Total incoming line count. Set to zero if unknown
		/// </summary>
		public required int LineCount
		{
			get;
			set
			{
				field = value <= 0 ? int.MaxValue : value;
				_total = 0;
				_window = 1;
			}
		}

		public override Encoding Encoding { get; }

		public override void WriteLine(string line)
		{
			Print(line, crlf: true);
		}
		public override void WriteLine()
		{
			Print(null, crlf: true);
		}
		public override void Write(string line)
		{
			Print(line, crlf: false);
		}

		#region Private
		int _total;
		int _window;
		string _lostline;

		private void Print(string line, bool crlf)
		{
			if (_total == LineCount)
				return;

			if (_total < LineCount && _window < Console.WindowHeight)
			{
				if (crlf)
				{
					if (string.IsNullOrEmpty(line))
						Console.WriteLine();
					else
					{
						if (_lostline != null)
						{
							Console.WriteLine(_lostline);
							_lostline = null;
							_window++;
						}

						Console.WriteLine(line);
						_window += line.Count(x => x == '\n');
					}

					_window++;
					_total++;
				}
				else
					Console.Write(line);
			}
			else
			{
				_window = 1;

				if (Console.IsOutputRedirected)
					return;

				Console.ResetColor();

				if(LineCount == int.MaxValue)
					Console.Write("-- More -- ");
				else
					Console.Write($"-- More ({Math.Round((double)_total / LineCount * 100)}%) -- ");

				while(true)
				{ 
					var key = Console.ReadKey(intercept: true);
					if(key.Key == ConsoleKey.Q)
					{
						Console.WriteLine();
						_total = LineCount;
						return;
					}

					_lostline = line; // Because of "-- More (29%) --" being written

					if (key.Key == ConsoleKey.Spacebar) // increment page
					{
						break;
					}

					if(key.Key == ConsoleKey.Enter) // increment (bug 2!) lines
					{
						_window = Console.WindowHeight - 1;
						_total++;
						break;
					}
				}

				Console.CursorLeft = 0;
				Console.Write(new string(' ', 20));
				Console.CursorLeft = 0;
			}
		}
		#endregion
	}
}
