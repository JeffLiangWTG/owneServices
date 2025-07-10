using System.Collections.Generic;

namespace Enterprise.MasterFiles.Business.Xsd
{
	class XsdBuilder
	{
		public XsdBuilder()
		{
			this.internalList = new List<string>();
		}

		readonly List<string> internalList;
		int indentCount;

		public void AddBlankLine()
		{
			internalList.Add(string.Empty);
		}

		public void Add(string lineValue)
		{
			if (string.IsNullOrEmpty(lineValue))
			{
				AddBlankLine();
				return;
			}

			bool isEndElementLine = lineValue.StartsWith("</");
			if (isEndElementLine)
			{
				indentCount--;
			}

			internalList.Add("".PadRight(indentCount * 2, ' ') + lineValue);

			if (!isEndElementLine && !lineValue.EndsWith("/>"))
			{
				indentCount++;
			}
		}

		public void Add(string value, params object[] parameters)
		{
			Add(string.Format(value, parameters));
		}

		public string WriteOutXsd()
		{
			return string.Join("\r\n", internalList.ToArray());
		}
	}
}
