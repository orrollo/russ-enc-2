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
