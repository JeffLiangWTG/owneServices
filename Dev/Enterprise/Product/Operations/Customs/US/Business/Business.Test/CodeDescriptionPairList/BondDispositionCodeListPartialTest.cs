using System.Collections.Generic;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class BondDispositionCodeListTest : NUnit.Framework.TestCase
	{
		public void TestGetCodeDescriptionPairList()
		{
			var list = new BondDispositionCodeList();
			AssertEquals(66, list.Count);
			foreach (var code in despositionCodes)
			{
				AssertEquals(true, list.ContainsCode(code));
			}
		}

		readonly List<string> despositionCodes = new List<string>()
		{
			"BNI",
			"C22", "C98", "CAB", "CAR", "CBE", "CCB", "CEB", "CNB", "CPA", "CTB", "CPD", "CPX", "CRF", "CTR", "CTS", "CUA", "CUB", "CUD", "CVB", "CVI",
			"EAB", "EAD", "EAR", "EBM", "ECB", "EDB", "EEB", "ENB", "ERB", "ESA", "ETR", "ESR", "ETS", "EUA", "EUB", "EUD", "EVB", "EVI",
			"INS", "LCL", "LEX", "LLQ", "LLV", "LRL", "LSP",
			"PEA", "PEB", "PEC", "PEN", "PEM", "PEO", "PER", "PFD", "PFR", "POD", "POR", "PPC", "PPR", "PQ1", "PQ2", "PQ3", "PQA", "PQR", "PQV",
			"SUF"
		};
	}
}
