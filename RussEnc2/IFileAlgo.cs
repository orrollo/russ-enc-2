using System.Collections.Generic;

namespace RussEnc2
{
	internal interface IFileAlgo
	{
		void BeforeProcess();
		void AfterProcess();
		string[] GetFilesList();
		Dictionary<string, object> ParseFileName(string file);
		bool CheckFile(string file, Dictionary<string, object> param);
		string GetLastError();
		bool ProcessFile(string file, Dictionary<string, object> param);
		void SetContext(Dictionary<string, object> context);
	}
}