using System.Globalization;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USCusUQList : CodeDescriptionPairList
	{
		public USCusUQList()
		{
			var aesList = new AESUnitOfMeasureList();
			var abiList = new ABIUnitOfMeasureList();
			foreach (CodeDescriptionPair pair in abiList)
			{
				string code = pair.Code;
				string description = string.Format(CultureInfo.InvariantCulture, "{0} ({1})", pair.Description, (aesList.ContainsCode(code)) ? "Common" : "Import Only");
				AddPair(code, description);
			}

			foreach (CodeDescriptionPair pair in aesList)
			{
				string code = pair.Code;
				if (!ContainsCode(code))
				{
					string description = pair.Description + " (Export Only)";
					AddPair(code, description);
				}
			}
			SortByDescription();
		}
	}
}
