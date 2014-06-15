using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RussEnc2
{
	class Program
	{
		static void Main(string[] args)
		{
			ProcessLngFiles();
			ProcessDatFiles();
			ProcessInpFiles();
			SaveDatFiles();
		}

		private static void SaveDatFiles()
		{
			foreach (var pair in Langs)
			{
				var lang = pair.Key;
				var fileName = Path.Combine(Directory.GetCurrentDirectory(), string.Format("{0}.dat", lang));
				Console.Write("saving file: " + fileName + "... ");
				using (var wrt = new StreamWriter(fileName, false, Encoding.UTF8))
				{
					wrt.WriteLine(LangAlphas[lang]);
					GetLangRoot(lang).WriteStats(wrt);
					wrt.Flush();
				}
				Console.WriteLine("done.");
			}
		}

		private static void ProcessDatFiles()
		{
			var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.dat");
			foreach (var fileName in files)
			{
				Console.Write("Processing data: " + Path.GetFileName(fileName) + "... ");
				var ret = ProcessFileName(ReDat, fileName);
				if (ret == null)
				{
					Console.WriteLine("not matched to mask, skip");
					continue;
				}
				LoadData(ret[0], fileName);
			}
		}

		private static void StoreHashFile()
		{
			using (var wrt = new StreamWriter(GetHashFileName(), false, Encoding.UTF8))
			{
				foreach (var pair in Hashes) wrt.WriteLine(pair.Key + ";" + pair.Value);
				wrt.Flush();
			}
		}

		private static void ReadHashFile()
		{
			var fileName = GetHashFileName();
			if (!File.Exists(fileName)) return;
			using (var rdr = new StreamReader(fileName, Encoding.UTF8))
			{
				while (!rdr.EndOfStream)
				{
					var line = (rdr.ReadLine() ?? "").Trim();
					if (string.IsNullOrEmpty(line)) continue;
					var pp = line.Split(';');
					if (pp.Length == 2) Hashes.Add(pp[0], ulong.Parse(pp[1]));
				}
			}
		}

		private static string GetHashFileName()
		{
			return Path.Combine(Directory.GetCurrentDirectory(), "hash.lst");
		}

		private static void LoadData(string lang, string fileName)
		{
			using (var rdr = new StreamReader(fileName, Encoding.UTF8))
			{
				var root = GetLangRoot(lang);
				var first = true;
				while (!rdr.EndOfStream)
				{
					var line = (rdr.ReadLine() ?? "").TrimEnd();
					if (string.IsNullOrEmpty(line)) continue;
					if (first) 
						SetAplha(lang, line);
					else
					{
						var pp = line.Split(';');
						if (pp.Length == 3)
						{
							root.AddStat(pp[0][0], pp[1][0], int.Parse(pp[2]));
						}
					}
					first = false;
				}
			}
			Console.WriteLine("processing finished.");
		}

		private static void ProcessLngFiles()
		{
			var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.lng");
			foreach (var fileName in files)
			{
				Console.Write("Processing language def: " + Path.GetFileName(fileName) + "... ");
				var ret = ProcessFileName(ReLng, fileName);
				if (ret == null)
				{
					Console.WriteLine("not matched to mask, skip");
					continue;
				}
				var enc = ParseEncodingName(ret[1]);
				if (enc == null)
				{
					Console.WriteLine("unable to get encoding <" + ret[1] + ">, skip");
					continue;
				}
				ReadLng(ret, fileName, enc);
			}
		}

		private static void ReadLng(string[] ret, string fileName, Encoding enc)
		{
			using (var rdr = new StreamReader(fileName, enc))
			{
				var first = true;
				while (!rdr.EndOfStream)
				{
					string line = (rdr.ReadLine() ?? "").TrimEnd();
					if (string.IsNullOrEmpty(line)) continue;
					if (first) 
						SetAplha(ret[0], line);
					else 
						AddEncCandidate(ret[0], line);
					first = false;
				}
			}
			Console.WriteLine("processing finished.");
		}

		private static void AddEncCandidate(string lang, string encName)
		{
			var enc = ParseEncodingName(encName);
			if (enc == null)
			{
				Console.WriteLine("encoding <" + encName + "> is unknown");
				return;
			}
			if (!LangEncs.ContainsKey(lang)) LangEncs.Add(lang, new List<string>());
			LangEncs[lang].Add(encName);
		}

		private static void SetAplha(string lang, string alpha)
		{
			LangAlphas[lang] = alpha;
		}

		private static void ProcessInpFiles()
		{
			ReadHashFile();
			var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.inp");
			foreach (var fileName in files)
			{
				Console.Write("Processing data: " + Path.GetFileName(fileName) + "... ");
				if (IsProcessed(fileName))
				{
					Console.WriteLine("already processed, skip");
					continue;
				}
				var ret = ProcessFileName(ReInp, fileName);
				if (ret == null)
				{
					Console.WriteLine("not matched to mask, skip");
					continue;
				}
				var alpha = GetAlpha(ret[0]);
				if (string.IsNullOrEmpty(alpha))
				{
					Console.WriteLine("chars set is undefined, skip");
					continue;
				}
				var enc = ParseEncodingName(ret[1]);
				if (enc == null)
				{
					Console.WriteLine("unable to get encoding <" + ret[1] + ">, skip");
					continue;
				}
				GetStats(ret, fileName, enc, alpha);
			}
			StoreHashFile();
		}

		private static ulong[] table = null;

		private static bool IsProcessed(string fileName)
		{
			var shortName = GetShortName(fileName);
			if (!Hashes.ContainsKey(shortName)) return false;
			return Hashes[shortName] == CalculateHash(fileName);
		}

		private static string GetShortName(string fileName)
		{
			return Path.GetFileName(fileName).ToUpper();
		}

		private static ulong CalculateHash(string fileName)
		{
			ulong hash = 0;
			using (var stream = File.OpenRead(fileName))
			{
				var buf = new byte[65000];
				if (table == null) CreateTable();
				while (true)
				{
					var cnt = stream.Read(buf, 0, buf.Length);
					for (var i = 0; i < cnt; i++)
					{
						var t = (byte) (((hash >> (7 + (i%16))) & 0xff) ^ buf[i]);
						hash += table[t];
					}
					if (cnt < buf.Length) break;
				}
			}
			return hash;
		}

		private static void CreateTable()
		{
			ulong cur = 0;
			table = new ulong[256];
			for (int idx = 0; idx < 256; idx++)
			{
				cur = cur*93 + 11;
				for (var n = (cur%20) + 10; n > 0; n--) cur = cur*93 + 11;
				table[idx] = cur;
			}
		}

		private static void GetStats(string[] ret, string fileName, Encoding enc, string alpha)
		{
			var root = GetLangRoot(ret[0]);
			using (var rdr = new StreamReader(fileName, enc))
			{
				var prev = (char) 0;
				var first = true;
				while (!rdr.EndOfStream)
				{
					var next = (char) rdr.Read();
					if (!first && !Skip(prev, alpha) && !Skip(next, alpha))
					{
						root.AddStat(prev, next);
					}
					prev = next;
					first = false;
				}
			}
			Console.WriteLine("processing finished.");
			AddToProcessed(fileName);
		}

		private static void AddToProcessed(string fileName)
		{
			var shortName = GetShortName(fileName);
			Hashes[shortName] = CalculateHash(fileName);
		}

		private static bool Skip(char key, string alpha)
		{
			return alpha.IndexOf(key) == -1;
		}

		private static string GetAlpha(string lang)
		{
			return LangAlphas.ContainsKey(lang) ? LangAlphas[lang] : string.Empty;
		}

		private static Encoding ParseEncodingName(string encName)
		{
			try
			{
				int encCode;
				return int.TryParse(encName, out encCode)
					      ? Encoding.GetEncoding(encCode)
						  : Encoding.GetEncoding(encName);
			}
			catch (Exception e)
			{
				return null;
			}
		}

		private static string[] ProcessFileName(Regex re, string fileName)
		{
			var match = re.Match(fileName);
			string[] ret = null;
			if (match.Success)
			{
				ret = new string[] {match.Groups[1].Value.ToUpper(), match.Groups[2].Value};
			}
			return ret;
		}

		private static readonly Dictionary<string,StatNode> Langs = new Dictionary<string, StatNode>();
		private static readonly Dictionary<string, List<string>> LangEncs = new Dictionary<string, List<string>>();
		private static readonly Dictionary<string, string> LangAlphas = new Dictionary<string, string>();
		private static readonly Dictionary<string, ulong> Hashes = new Dictionary<string, ulong>();

		private static readonly Regex ReInp = new Regex(@"([a-z]{2})-([^\\.]+)\.inp$", RegexOptions.IgnoreCase);
		private static readonly Regex ReLng = new Regex(@"([a-z]{2})-([^\\.]+)\.lng$", RegexOptions.IgnoreCase);
		private static readonly Regex ReDat = new Regex(@"([a-z]{2})(\.dat)$", RegexOptions.IgnoreCase);

		private static StatNode GetLangRoot(string lang)
		{
			if (!Langs.ContainsKey(lang)) Langs[lang] = new StatNode();
			return Langs[lang];
		}
	}
}
