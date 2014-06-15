using System;
using System.Collections.Generic;
using System.Text;

namespace RussEnc2
{
	internal class ProcessorBase
	{
		protected string LastError;
		protected Dictionary<string, object> Context;

		protected string GetAlpha(string lang)
		{
			return LangAlphas.ContainsKey(lang) ? LangAlphas[lang] : String.Empty;
		}

		protected StatNode GetLangRoot(string lang)
		{
			if (!Langs.ContainsKey(lang)) Langs[lang] = new StatNode();
			return Langs[lang];
		}

		protected Dictionary<string, StatNode> Langs
		{
			get
			{
				if (!Context.ContainsKey("langs")) Context["langs"] = new Dictionary<string, StatNode>();
				return Context.Get<Dictionary<string, StatNode>>("langs");
			}
		}

		protected Dictionary<string,List<string>> LangEncs
		{
			get
			{
				if (!Context.ContainsKey("langEncs")) Context["langEncs"] = new Dictionary<string, List<string>>();
				return Context.Get<Dictionary<string, List<string>>>("langEncs");
			}
		}

		protected Dictionary<string,string> LangAlphas
		{
			get
			{
				if (!Context.ContainsKey("langAlphas")) Context["langAlphas"] = new Dictionary<string, string>();
				return Context.Get<Dictionary<string, string>>("langAlphas");
			}
		}

		public virtual string GetLastError()
		{
			return LastError;
		}

		public Encoding ParseEncoding(string encName)
		{
			try
			{
				int code;
				return int.TryParse(encName, out code) ? Encoding.GetEncoding(code) : Encoding.GetEncoding(encName);
			}
			catch (Exception)
			{
				LastError = "unable to get encoding <" + encName + ">";
				return null;
			}
		}

		public void SetContext(Dictionary<string, object> context)
		{
			Context = context;
		}

		protected void SetAplha(string lang, string alpha)
		{
			LangAlphas[lang] = alpha;
		}

		protected bool Error(string error = null)
		{
			if (!string.IsNullOrEmpty(error)) LastError = error;
			return false;
		}
	}
}