using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace RussEnc2
{
	internal class DatProcessor : ProcessorBase, IFileAlgo
	{
		private static readonly Regex ReDat = new Regex(@"([a-z]{2})(\.dat)$", RegexOptions.IgnoreCase);

		public void BeforeProcess()
		{
		}

		public void AfterProcess()
		{
		}

		public string[] GetFilesList()
		{
			return Directory.GetFiles(Directory.GetCurrentDirectory(), "*.dat");
		}

		public Dictionary<string, object> ParseFileName(string file)
		{
			var ret = new Dictionary<string, object>();
			var match = ReDat.Match(file);
			ret["ok"] = match.Success;
			if (match.Success)
			{
				ret["lang"] = match.Groups[1].Value.ToUpper();
			}
			else
			{
				LastError = "not matched to mask";
			}
			return ret;
		}

		public bool CheckFile(string file, Dictionary<string, object> param)
		{
			return param.Get<bool>("ok");
		}

		public bool ProcessFile(string file, Dictionary<string, object> param)
		{
			try
			{
				var lang = param.Get<string>("lang");
				using (var rdr = new StreamReader(file, Encoding.UTF8))
				{
					var root = GetLangRoot(lang);
					var first = true;
					while (!rdr.EndOfStream)
					{
						var line = (rdr.ReadLine() ?? "").TrimEnd();
						if (String.IsNullOrEmpty(line)) continue;
						if (first)
							SetAplha(lang, line);
						else
						{
							var pp = line.Split(';');
							if (pp.Length == 3)
							{
								root.AddStat(pp[0][0], pp[1][0], Int32.Parse(pp[2]));
							}
						}
						first = false;
					}
				}
				return true;
			}
			catch (Exception e)
			{
				LastError = e.Message;
				return false;
			}
		}
	}
}