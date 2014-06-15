using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RussEnc2
{
	internal class DataSaver : ProcessorBase, IFileAlgo
	{
		public void BeforeProcess()
		{
			foreach (var pair in Langs)
			{
				var lang = pair.Key;
				var fileName = Path.Combine(Directory.GetCurrentDirectory(), String.Format("{0}.dat", lang));
				Console.Write("saving file: " + Path.GetFileName(fileName) + "... ");
				using (var wrt = new StreamWriter(fileName, false, Encoding.UTF8))
				{
					wrt.WriteLine(LangAlphas[lang]);
					GetLangRoot(lang).WriteStats(wrt);
					wrt.Flush();
				}
				Console.WriteLine("done.");
			}
		}

		public void AfterProcess()
		{
		}

		public string[] GetFilesList()
		{
			return new string[0];
		}

		public Dictionary<string, object> ParseFileName(string file)
		{
			throw new NotImplementedException();
		}

		public bool CheckFile(string file, Dictionary<string, object> param)
		{
			throw new NotImplementedException();
		}

		public bool ProcessFile(string file, Dictionary<string, object> param)
		{
			throw new NotImplementedException();
		}
	}
}