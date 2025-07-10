using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.CNReferenceData.Services;

namespace CargoWise.RefDbRepo.CNReferenceData.Business
{
	public class RateFormulaParser
	{
		readonly string pattern;

		public RateFormulaParser(string pattern)
		{
			this.pattern = pattern;
		}

		public string GetFormula(string raw)
		{
			var corrected = Correct(raw);
			var tree = Parse(corrected);
#pragma warning disable CA1031 // Do not catch general exception types
			try
			{
				var formula = Traverse(tree);
				return new string(formula.Where(c => c >= 32 && c <= 126).ToArray());
			}
			catch (Exception e)
			{
				GlobalOption.Instance.Log.Error($"Failed Parsing Formula '{raw}' with the exception: ", e);
				return string.Empty;
			}
#pragma warning restore CA1031 // Do not catch general exception types
		}

		public static string Correct(string str)
		{
			if (str.Contains("暂缓实施") || str == "零国别关税配额税率" || str == "待更新")
			{
				str = "0";
			}
			else if (str.Replace(" ", "").Contains("配额税率"))
			{
				var index = str.IndexOfAny(new[] { ':', '：' }) + 1;
				if (index > 0)
				{
					str = str.Substring(index, str.Length - index);
				}
				if (str != "0" && !str.EndsWith("%", StringComparison.InvariantCultureIgnoreCase))
				{
					str += "%";
				}
			}

			var greaterMatchGroups = new Regex(@"(\S*)在(\d*)(\S*)或(\d*)(\S*)及以上(\S*)").Match(str).Groups;
			if (greaterMatchGroups.Count > 6)
			{
				str = $"{greaterMatchGroups[1]}>={greaterMatchGroups[2]}{greaterMatchGroups[3]}{greaterMatchGroups[6]};{greaterMatchGroups[1]}>={greaterMatchGroups[4]}{greaterMatchGroups[5]}{greaterMatchGroups[6]}";
			}
			else
			{
				greaterMatchGroups = new Regex(@"(\S*)在(\d*)(\S*)及以上(\S*)").Match(str).Groups;
				if (greaterMatchGroups.Count > 4)
				{
					str = $"{greaterMatchGroups[1]}>={greaterMatchGroups[2]}{greaterMatchGroups[3]}{greaterMatchGroups[4]}";
				}
			}

			return str
				.Replace(" ", "")
				.Replace("％", "%")
				.Replace("≥", ">=")
				.Replace("≤", "<=")
				.Replace("＜", "<")
				.Replace("＞", ">")
				.Replace("。", "")
				.Replace("，加", "+")
				.Replace("的麦芽酿造啤酒", "")
				.Replace("\"", "")
				.Replace("“", "")
				.Replace("”", "")
				.Replace("，两者从低", "->MIN")
				.Replace("；两者从低执行", "->MIN")
				.Replace("两者从低执行", "->MIN")
				.Replace("；从低执行", "->MIN")
				.Replace("从低执行", "->MIN")
				.Replace("时，税率为", ":")
				.Replace("的,税率为", ":")
				.Replace("的税率为", ":")
				.Replace("，税率为", ":")
				.Replace(",税率为", ":")
				.Replace("，完税价", ";CV")
				.Replace("，", ":")
				.Replace("：", ":")
				.Replace("或", "与")
				.Replace("；", ";")
				.Replace("进口关税完税价格", "CV")
				.Replace("进口完税价格", "CV")
				.Replace("完税价格", "CV")
				.Replace("完税价", "CV")
				.Replace("价格", "CV")
				.Replace("不高于", "<=")
				.Replace("高于", ">")
				.Replace("大于等于", ">=")
				.Replace("美元", "[UOM=][USD]")
				.Replace("元人民币", "[UOM=][CNY]")
				.Replace("人民币", "[UOM=][CNY]")
				.Replace("元", "[UOM=][CNY]")
				.Replace("yuan", "[UOM=][CNY]")
				.Replace("片(张)", "件")
				.Replace("片（张）", "件")
				.Replace("毫升(克)", "克")
				.Replace("毫升（克）", "克")
				.Replace("每标准条", "标准条每")
				.Replace("L", "升")
				.Replace("㎡", "平方米")
				.Trim('）', '。');
		}

		static Node Parse(string str)
		{
			foreach (var op in new[] { "->", "与", ";", ":", "每", "<=", ">=", ">", "<", "+", "%", "[UOM=]", "/", })
			{
				if (str.Contains(op))
				{
					var pair = str.Split(new[] { op }, StringSplitOptions.None);
					if (pair.Length == 2)
					{
						var node = new BinaryNode
						{
							Operator = op,
							LeftChild = Parse(pair[0]),
							RightChild = Parse(pair[1])
						};
						node.LeftChild.Parent = node;
						node.RightChild.Parent = node;
						return node;
					}
				}
			}

			return new TerminalNode
			{
				Leaf = str
			};
		}

#pragma warning disable CA1502 // Avoid excessive complexity
		string Traverse(Node node)
#pragma warning restore CA1502 // Avoid excessive complexity
		{
			switch (node)
			{
				case TerminalNode t:
					{
						switch (t.Leaf)
						{
							case "CV" when t.Parent is BinaryNode parent && Find(parent, l => l.Leaf == "[USD]"):
								return "CVINUSD";
							case "CV":
								return "CV";
							case "[USD]":
							case "[CNY]":
								return "";
							case "标准箱":
								return "MAX(1, ROUND([143] / 50, 0))";
							case "标准条":
								return "MAX(1, ROUND([143] / 0.2, 0))";
							default:
								{
									var code = UnitOfMeasurementHelper.Instance.GetValueOrDefault(t.Leaf);
									return !string.IsNullOrEmpty(code) ? $"[{code}]" : t.Leaf;
								}
						}
					}
				case BinaryNode b:
					switch (b.Operator)
					{
						case "->":
							return Traverse(b.RightChild) + "(" + Traverse(b.LeftChild) + ")";
						case "与":
							return Traverse(b.LeftChild) + ", " + Traverse(b.RightChild);
						case ";":
							return "IF(" + Traverse(b.LeftChild) + ", " + "IF(" + Traverse(b.RightChild) + ", 0))";
						case ":":
							var result = Traverse(b.LeftChild) + ", " + Traverse(b.RightChild);
							if (b.Parent == null)
							{
								result = "IF(" + result + ", 0))";
							}
							return result;
						case "<=":
						case ">=":
						case ">":
						case "<":
						case "+":
							return Traverse(b.LeftChild) + " " + b.Operator + " " + Traverse(b.RightChild);
						case "%" when b.LeftChild is TerminalNode liftLeaf:
							{
								var r = (decimal.Parse(liftLeaf.Leaf, CultureInfo.InvariantCulture) / 100m).ToString(CultureInfo.InvariantCulture);
								return pattern.Replace("{{rate}}", r);
							}
						case "[UOM=]":
							return Traverse(b.LeftChild) + Traverse(b.RightChild);
						case "/":
							return " * " + Traverse(b.LeftChild) + Traverse(b.RightChild);
						case "每":
							return Traverse(b.RightChild) + " * " + Traverse(b.LeftChild);
						default:
							return "";
					}
				default:
					return "";
			}
		}

		static bool Find(Node node, Func<TerminalNode, bool> func)
		{
			switch (node)
			{
				case TerminalNode t:
					return func.Invoke(t);
				case BinaryNode b:
					return Find(b.LeftChild, func) || Find(b.RightChild, func);
				default:
					return false;
			}
		}

		public static string GetRateFormula(string tariff, string rateType, string formulaDerivedFrom, ILog logger)
		{
			string formula;

			formulaDerivedFrom = RemoveRedundantDescription(formulaDerivedFrom);

			if (formulaDerivedFrom == "0")
			{
				formula = "0";
			}
			else
			{
				var pattern = GetRateFormulaPattern(rateType);

				if (decimal.TryParse(formulaDerivedFrom, out decimal rateValue))
				{
					rateValue /= 100;
					formula = pattern.Replace("{{rate}}", rateValue.ToString(CultureInfo.InvariantCulture));
				}
				else
				{
					formula = new RateFormulaParser(pattern).GetFormula(formulaDerivedFrom);

					if (string.IsNullOrEmpty(formula))
					{
						logger?.Error($"Special Rate Parse Error for {tariff}: {formulaDerivedFrom} => {formula}");
					}
					else
					{
						logger?.Debug($"Special Rate for {tariff}: {formulaDerivedFrom} => {formula}");
					}
				}
			}

			return formula;
		}

		static string RemoveRedundantDescription(string formula)
		{
			return formula.Replace("国别关税配额税率：", "").Replace("国别关税配额税率:", "").Trim();
		}

		public static string RemoveNewLines(string str)
		{
			return string.IsNullOrEmpty(str) ? str : str.Replace("&#xD;", "").Replace("&#xA;", "").Replace("\r", "").Replace("\n", "").Trim();
		}

		static string GetRateFormulaPattern(string rateType)
		{
			string pattern;

			switch (rateType)
			{
				case "EXP":
					pattern = "ROUND(VFD / (1 + {{rate}}), 0) * {{rate}}";
					break;
				case "EXC":
					pattern = "VFD / (1 - {{rate}}) * {{rate}}";
					break;
				default:
					pattern = "VFD * {{rate}}";
					break;
			}

			return pattern;
		}
	}

	class BinaryNode : Node
	{
		public Node LeftChild;
		public Node RightChild;
		public string Operator = string.Empty;
	}

	class TerminalNode : Node
	{
		public string Leaf = string.Empty;
	}

	class Node
	{
		public Node Parent;
	}
}
