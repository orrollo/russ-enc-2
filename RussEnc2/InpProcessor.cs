using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace RussEnc2
{
	internal class InpProcessor : ProcessorBase, IFileAlgo
	{
		private static readonly Regex ReInp = new Regex(@"([a-z]{2})-([^\\.]+)\.inp$", RegexOptions.IgnoreCase);
		protected readonly Dictionary<string, ulong> Hashes = new Dictionary<string, ulong>();

		public void BeforeProcess()
		{
			var fileName = GetHashFileName();
			if (!File.Exists(fileName)) return;
			using (var rdr = new StreamReader(fileName, Encoding.UTF8))
			{
				while (!rdr.EndOfStream)
				{
					var line = (rdr.ReadLine() ?? "").Trim();
					if (String.IsNullOrEmpty(line)) continue;
					var pp = line.Split(';');
					if (pp.Length == 2) Hashes.Add(pp[0], UInt64.Parse(pp[1]));
				}
			}
		}

		private static string GetHashFileName()
		{
			return Path.Combine(Directory.GetCurrentDirectory(), "hash.lst");
		}

		public void AfterProcess()
		{
			using (var wrt = new StreamWriter(GetHashFileName(), false, Encoding.UTF8))
			{
				foreach (var pair in Hashes) wrt.WriteLine(pair.Key + ";" + pair.Value);
				wrt.Flush();
			}
		}

		public string[] GetFilesList()
		{
			return Directory.GetFiles(Directory.GetCurrentDirectory(), "*.inp");
		}

		public Dictionary<string, object> ParseFileName(string file)
		{
			var ret = new Dictionary<string, object>();
			var match = ReInp.Match(file);
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
			param["hashValue"] = CalculateHash(file);
			if (IsProcessed(file, param.Get<ulong>("hashValue"))) 
				return Error("already processed, skip");
			if (string.IsNullOrEmpty(GetAlpha(param.Get<string>("lang"))))
				return Error("chars set is undefined, skip");
			param["enc"] = ParseEncoding(param.Get<string>("encName"));
			return param.Get<object>("enc") != null;
		}

		private bool IsProcessed(string fileName, ulong hashValue)
		{
			var shortName = GetShortName(fileName);
			if (!Hashes.ContainsKey(shortName)) return false;
			return Hashes[shortName] == hashValue;
		}

		private static string GetShortName(string fileName)
		{
			var name = Path.GetFileName(fileName);
			return !string.IsNullOrEmpty(name) ? name.ToUpper() : null;
		}

		private static ulong[] _hashTable = null;

		private static ulong[] CreateTable()
		{
			ulong cur = 0;
			var table = new ulong[256];
			for (var idx = 0; idx < 256; idx++)
			{
				cur = cur * 93 + 11;
				for (var n = (cur % 20) + 10; n > 0; n--) cur = cur * 93 + 11;
				table[idx] = cur;
			}
			return table;
		}

		private static ulong CalculateHash(string fileName)
		{
			ulong hash = 0;
			using (var stream = File.OpenRead(fileName))
			{
				var buf = new byte[65000];
				_hashTable = _hashTable ?? CreateTable();
				while (true)
				{
					var cnt = stream.Read(buf, 0, buf.Length);
					for (var i = 0; i < cnt; i++)
					{
						var t = (byte)(((hash >> (7 + (i % 16))) & 0xff) ^ buf[i]);
						hash += _hashTable[t];
					}
					if (cnt < buf.Length) break;
				}
			}
			return hash;
		}

		public bool ProcessFile(string file, Dictionary<string, object> param)
		{
			var lang = param.Get<string>("lang");
			var alpha = GetAlpha(lang);
			var root = GetLangRoot(lang);
			var enc = param.Get<Encoding>("enc");
			using (var rdr = new StreamReader(file, enc))
			{
				var prev = (char)0;
				var first = true;
				while (!rdr.EndOfStream)
				{
					var next = (char)rdr.Read();
					if (!first && !Skip(prev, alpha) && !Skip(next, alpha))
					{
						root.AddStat(prev, next);
					}
					prev = next;
					first = false;
				}
			}
			Console.WriteLine("processing finished.");
			AddToProcessed(file, param.Get<ulong>("hashValue"));
			return true;
		}

		private static bool Skip(char prev, string alpha)
		{
			return alpha.IndexOf(prev) == -1;
		}

		private void AddToProcessed(string fileName, ulong hashValue)
		{
			Hashes[GetShortName(fileName)] = hashValue;
		}

	}
}