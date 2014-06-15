using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace RussEnc2
{
	internal class LangProcessor : ProcessorBase, IFileAlgo
	{
		private static readonly Regex ReLng = new Regex(@"([a-z]{2})-([^\\.]+)\.lng$", RegexOptions.IgnoreCase);

		public void BeforeProcess()
		{
		}

		public void AfterProcess()
		{
		}

		public string[] GetFilesList()
		{
			return Directory.GetFiles(Directory.GetCurrentDirectory(), "*.lng");
		}

		public Dictionary<string, object> ParseFileName(string file)
		{
			var ret = new Dictionary<string, object>();
			var match = ReLng.Match(file);
			ret["ok"] = match.Success;
			if (match.Success)
			{
				ret["lang"] = match.Groups[1].Value.ToUpper();
				ret["encName"] = match.Groups[2].Value;
			}
			else
			{
				LastError = "not matched to mask";
			}
			return ret;
		}

		public bool CheckFile(string file, Dictionary<string, object> param)
		{
			if (!param.Get<bool>("ok")) return false;
			param["enc"] = ParseEncoding(param.Get<string>("encName"));
			return param.Get<object>("enc") != null;
		}

		public bool ProcessFile(string file, Dictionary<string, object> param)
		{
			var enc = param.Get<Encoding>("enc");
			var lang = param.Get<string>("lang");
			using (var rdr = new StreamReader(file, enc))
			{
				var first = true;
				while (!rdr.EndOfStream)
				{
					string line = (rdr.ReadLine() ?? "").TrimEnd();
					if (String.IsNullOrEmpty(line)) continue;
					if (first)
						SetAplha(lang, line);
					else
						AddEncCandidate(lang, line);
					first = false;
				}
			}
			return true;
		}

		protected void AddEncCandidate(string lang, string encName)
		{
			if (ParseEncoding(encName) != null)
			{
				if (!LangEncs.ContainsKey(lang)) LangEncs.Add(lang, new List<string>());
				LangEncs[lang].Add(encName);
			}
		}
	}
}