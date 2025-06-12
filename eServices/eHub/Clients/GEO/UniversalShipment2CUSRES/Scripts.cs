using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CargoWise.eHub.Clients.GEO.Tests
{
	public class Scripts
	{
		public System.Collections.Generic.List<string> SWAPIndicatorList = new System.Collections.Generic.List<string>();

		public int CountSWAPIndicatorList()
		{
			return SWAPIndicatorList.Count;
		}

		public bool AddToSWAPIndicatorList(string indicator)
		{
			if (!SWAPIndicatorList.Contains(indicator))
			{
				SWAPIndicatorList.Add(indicator);
				return true;
			}
			else
			{
				return false;
			}
		}

		public string ReplaceCRWithSpace(string input)
		{
			string replacedString = input.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Trim();
			return replacedString.Substring(0, System.Math.Min(70, replacedString.Length));
		}

		public int InvoiceSequenceNumber = 0;

		public int GetNextInvoiceSequenceNumber()
		{
			return ++InvoiceSequenceNumber;
		}

		public string GetValueOfInvoiceNumberAndValueList(string invoiceNumberAndValueList, string invoiceNumber, int indexOfValue)
		{
			string result = string.Empty;
			int indexOfInvoiceNumber = invoiceNumberAndValueList.IndexOf(invoiceNumber);

			if (indexOfInvoiceNumber != -1)
			{
				int startIndexOfValue = indexOfInvoiceNumber + invoiceNumber.Length + 1;
				if (startIndexOfValue <= invoiceNumberAndValueList.Length)
				{
					string subString = invoiceNumberAndValueList.Substring(startIndexOfValue);
					int indexOfSemiComma = subString.IndexOf(';');

					if (indexOfSemiComma != -1)
					{
						string valueList = subString.Substring(0, indexOfSemiComma);
						string[] valueArray = valueList.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
						if (valueArray.Length > indexOfValue)
							result = valueArray[indexOfValue];
					}
				}
			}

			return result;
		}
	}
}
