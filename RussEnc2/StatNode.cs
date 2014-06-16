using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RussEnc2
{
	public class StatNode
	{
		public long Count { get; set; }

		internal Dictionary<char, StatNode> Children
		{
			get { return _children ?? (_children = new Dictionary<char, StatNode>()); }
			set { _children = value; }
		}

		private Dictionary<char, StatNode> _children;

		public void MakeCode(StringBuilder output, string lang, long sum, int level = 0)
		{
			if (level == 0) MakeHeaderCode(output, lang);
			var keys = SortedChildrenKeys();
			foreach (var key in keys)
			{
				if (level == 0)
				{
					output.AppendLine(string.Format("  if (c1 == (char){0}) {{", (int) key));
					Children[key].MakeCode(output, lang, Count, 1);
					output.AppendLine("  }");
				}
				else if (level == 1)
				{
					long val = (100000*Children[key].Count)/sum;
					if (val > 0) output.AppendLine(string.Format("    if (c2 == (char){0}) return {1};", (int) key, val));
				}
			}
			output.AppendLine(string.Format("{0}return 0;", level == 1 ? "    " : "  "));
			if (level == 0) MakeFooterCode(output, lang);
		}

		private char[] SortedChildrenKeys()
		{
			var ret = Children.Keys.ToArray();
			if (ret.Length > 1)
			{
				// dumb sorting
				var stop = false;
				while (!stop)
				{
					stop = true;
					for (var i = 1; i < ret.Length; i++)
					{
						char k1 = ret[i - 1], k2 = ret[i];
						if (Children[k1].Count >= Children[k2].Count) continue;
						ret[i - 1] = k2;
						ret[i] = k1;
						stop = false;
					}
				}
			}
			return ret;
		}


		private void MakeFooterCode(StringBuilder output, string lang)
		{
			output.AppendLine("}");
			output.AppendLine();
		}

		private void MakeHeaderCode(StringBuilder output, string lang)
		{
			output.AppendLine(string.Format("protected static int check_{0}(char c1, char c2) {{", lang));
		}

		public void AddStat(char prev, char next, int delta = 1)
		{
			IncStat(prev, delta).AddStat(next, delta);
		}

		private StatNode IncStat(char key, int delta = 1)
		{
			Count += delta;
			return GetNode(key);
		}

		private void AddStat(char next, int delta = 1)
		{
			IncStat(next).Count += delta;
		}

		private StatNode GetNode(char key)
		{
			var nodes = Children;
			if (!nodes.ContainsKey(key)) nodes[key] = new StatNode();
			return nodes[key];
		}

		public void WriteStats(TextWriter wrt)
		{
			if (_children == null) return;
			foreach (var pair in _children)
			{
				var key = pair.Key;
				pair.Value.WriteStats(key, wrt);
			}
		}

		private void WriteStats(char key, TextWriter wrt)
		{
			if (_children == null) return;
			foreach (var pair in _children)
			{
				var key2 = pair.Key;
				var info = string.Format("{0};{1};{2}", key, key2, pair.Value.Count);
				wrt.WriteLine(info);
			}
		}
	}
}
