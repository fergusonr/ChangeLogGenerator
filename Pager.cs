using System;
using System.IO;
using System.Text;

namespace Helpers.Pagers
{
	/// <summary>
	/// Text Pager (More)
	/// </summary>
	public class Pager : TextWriter
	{
		public Pager()
		{
			Encoding = Console.Out.Encoding;
		}

		public required int Length
		{
			get;
			set
			{
				field = value;
				_total = 0;
				_window = 0;
			}
		}

		int _total;
		int _window;

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
		private void Print(string line, bool crlf)
		{
			if (_total == Length)
				return;

			if (_total < Length && _window < Console.WindowHeight)
			{
				if (crlf)
				{
					if (string.IsNullOrEmpty(line))
						Console.WriteLine();
					else
						Console.WriteLine(line);

					_total++;
					_window++;
				}
				else
					Console.Write(line);
			}
			else
			{
				_window = 0;

				if (Console.IsOutputRedirected)
					return;

				Console.ResetColor();
				Console.Write($"-- More ({Math.Round((double)_total / Length * 100)}%) -- ");

				var key = Console.ReadKey(intercept: true);
				if(key.KeyChar == 'q' || key.KeyChar == 'Q')
				{
					_total = Length;
					return;
				}
			}
		}
		#endregion
	}
}
