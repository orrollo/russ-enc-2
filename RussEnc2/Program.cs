using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RussEnc2
{
	class Program
	{
		public static void Process(Type algoType, Dictionary<string,object> ctx)
		{
			var algo = Activator.CreateInstance(algoType) as IFileAlgo;
			if (algo == null) return;
			algo.SetContext(ctx);
			algo.BeforeProcess();
			var files = algo.GetFilesList();
			foreach (var file in files)
			{
				Console.Write("Processing <" + Path.GetFileName(file) + ">... ");
				var param = algo.ParseFileName(file);
				if (!algo.CheckFile(file, param))
				{
					Console.WriteLine(algo.GetLastError());
					continue;
				}
				Console.WriteLine(!algo.ProcessFile(file, param) ? algo.GetLastError() : "done.");
			}
			algo.AfterProcess();
		}

		static void Main(string[] args)
		{
			var ctx = new Dictionary<string, object>();
			Process(typeof(LangProcessor), ctx);
			Process(typeof(DatProcessor), ctx);
			Process(typeof(InpProcessor), ctx);
			Process(typeof(DataSaver), ctx);
		}
	}
}
