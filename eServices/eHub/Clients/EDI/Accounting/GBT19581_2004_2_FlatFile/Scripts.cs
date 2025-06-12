using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CargoWise.eHub.Clients.EDI.Accounting.Transforms.GBT19581_2004_2_GBT19581_2004_FlatFile
{
	public class Scripts
	{
		public string ConvertBooleanToInt(string input)
		{
			return ConvertBooleanToIntRaw(input);
		}

		public string ConvertBooleanToIntRaw(string input)
		{
			if (input.ToLower() == "true") return "1";
			else return "0";
		}
	}
}
