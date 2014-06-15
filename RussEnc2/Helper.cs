using System.Collections.Generic;

namespace RussEnc2
{
	public static class Helper
	{
		public static T Get<T>(this Dictionary<string, object> dic, string name, T def)
		{
			return dic.ContainsKey(name) ? (T) dic[name] : def;
		}

		public static T Get<T>(this Dictionary<string, object> dic, string name)
		{
			return dic.ContainsKey(name) ? (T) dic[name] : default(T);
		}
	}
}