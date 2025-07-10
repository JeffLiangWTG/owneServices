using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	[TestedType(typeof(CusUSLVItemPGADisclaimOption))]
	public class CusUSLVItemPGADisclaimOptionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPGADisclaimReasonList()
		{
			var defaultOption = new CusUSLVItemPGADisclaimOption(Factory, "TST");
			AssertEquals("Default option should contain 7 disclaim reasons", 7, defaultOption.PGADisclaimReasonList.Count);
			AssertEquals("Default option should contain disclaim reasons A to G", "A, B, C, D, E, F, G", defaultOption.PGADisclaimReasonList.CodesAsString);

			var aphisOption = new CusUSLVItemPGADisclaimOption(Factory, "APHIS");
			AssertEquals("APHIS option should contain 3 disclaim reasons", 3, aphisOption.PGADisclaimReasonList.Count);
			AssertEquals("APHIS option should contain disclaim reasons A, B, and D", "A, B, D", aphisOption.PGADisclaimReasonList.CodesAsString);

			var fdaOption = new CusUSLVItemPGADisclaimOption(Factory, "FDA");
			AssertEquals("FDA option should contain 1 disclaim reason", 1, fdaOption.PGADisclaimReasonList.Count);
			AssertEquals("FDA option should contain disclaim reason A", "A", fdaOption.PGADisclaimReasonList.CodesAsString);

			var fsisOption = new CusUSLVItemPGADisclaimOption(Factory, "FSIS");
			AssertEquals("FSIS option should contain 4 disclaim reasons", 4, fsisOption.PGADisclaimReasonList.Count);
			AssertEquals("FSIS option should contain disclaim reasons A to D", "A, B, C, D", fsisOption.PGADisclaimReasonList.CodesAsString);

			var amsOption = new CusUSLVItemPGADisclaimOption(Factory, "AMS");
			AssertEquals("AMS option should contain 2 disclaim reasons", 2, amsOption.PGADisclaimReasonList.Count);
			AssertEquals("AMS option should contain disclaim reasons A and B", "A, B", amsOption.PGADisclaimReasonList.CodesAsString);

			var nopOption = new CusUSLVItemPGADisclaimOption(Factory, "NOP");
			AssertEquals("NOP option should contain 2 disclaim reasons", 4, nopOption.PGADisclaimReasonList.Count);
			AssertEquals("NOP option should contain disclaim reasons A to D", "A, B, C, D", nopOption.PGADisclaimReasonList.CodesAsString);

			var fwsOption = new CusUSLVItemPGADisclaimOption(Factory, "FWS");
			AssertEquals("FWS option should contain 3 disclaim reasons", 3, fwsOption.PGADisclaimReasonList.Count);
			AssertEquals("FWS option should contain disclaim reasons C to E", "C, D, E", fwsOption.PGADisclaimReasonList.CodesAsString);

			var laceyOption = new CusUSLVItemPGADisclaimOption(Factory, "Lacey");
			AssertEquals("Lacey option should contain 4 disclaim reasons", 5, laceyOption.PGADisclaimReasonList.Count);
			AssertEquals("Lacey option should contain disclaim reasons A to D, and G", "A, B, C, D, G", laceyOption.PGADisclaimReasonList.CodesAsString);

			var odsOption = new CusUSLVItemPGADisclaimOption(Factory, "ODS");
			AssertEquals("ODS option should contain 4 disclaim reasons", 4, odsOption.PGADisclaimReasonList.Count);
			AssertEquals("ODS option should contain disclaim reasons A to D", "A, B, C, D", odsOption.PGADisclaimReasonList.CodesAsString);

			var pstOption = new CusUSLVItemPGADisclaimOption(Factory, "PST");
			AssertEquals("PST option should contain 3 disclaim reasons", 3, pstOption.PGADisclaimReasonList.Count);
			AssertEquals("PST option should contain disclaim reasons A, C, and D", "A, C, D", pstOption.PGADisclaimReasonList.CodesAsString);

			var vneOption = new CusUSLVItemPGADisclaimOption(Factory, "VNE");
			AssertEquals("VNE option should contain 4 disclaim reasons", 4, vneOption.PGADisclaimReasonList.Count);
			AssertEquals("VNE option should contain disclaim reasons A to D", "A, B, C, D", vneOption.PGADisclaimReasonList.CodesAsString);

			var nmf370Option = new CusUSLVItemPGADisclaimOption(Factory, "370");
			AssertEquals("370 option should contain 2 disclaim reasons", 2, nmf370Option.PGADisclaimReasonList.Count);
			AssertEquals("370 option should contain disclaim reasons A and B", "A, B", nmf370Option.PGADisclaimReasonList.CodesAsString);

			var tscaOption = new CusUSLVItemPGADisclaimOption(Factory, "TSCA");
			AssertEquals("TSCA option should contain 4 disclaim reasons", 4, tscaOption.PGADisclaimReasonList.Count);
			AssertEquals("TSCA option should contain disclaim reasons A to D", "A, B, C, D", tscaOption.PGADisclaimReasonList.CodesAsString);

			var amrOption = new CusUSLVItemPGADisclaimOption(Factory, "AMR");
			AssertEquals("AMR option should contain 2 disclaim reasons", 2, amrOption.PGADisclaimReasonList.Count);
			AssertEquals("AMR option should contain disclaim reasons A and B", "A, B", amrOption.PGADisclaimReasonList.CodesAsString);

			var omcOption = new CusUSLVItemPGADisclaimOption(Factory, "OMC");
			AssertEquals("OMC option should contain 1 disclaim reasons", 1, omcOption.PGADisclaimReasonList.Count);
			AssertEquals("OMC option should contain disclaim reasons A", "A", omcOption.PGADisclaimReasonList.CodesAsString);

			var hmsOption = new CusUSLVItemPGADisclaimOption(Factory, "HMS");
			AssertEquals("HMS option should contain 2 disclaim reasons", 2, hmsOption.PGADisclaimReasonList.Count);
			AssertEquals("HMS option should contain disclaim reasons A and B", "A, B", hmsOption.PGADisclaimReasonList.CodesAsString);

			var nhtsaOption = new CusUSLVItemPGADisclaimOption(Factory, "NHTSA");
			AssertEquals("NHTSA option should contain 1 disclaim reasons", 1, nhtsaOption.PGADisclaimReasonList.Count);
			AssertEquals("NHTSA option should contain disclaim reasons A", "A", nhtsaOption.PGADisclaimReasonList.CodesAsString);

			var ttbOption = new CusUSLVItemPGADisclaimOption(Factory, "TTB");
			AssertEquals("TTB option should contain 2 disclaim reasons", 2, ttbOption.PGADisclaimReasonList.Count);
			AssertEquals("TTB option should contain disclaim reasons A and C", "A, C", ttbOption.PGADisclaimReasonList.CodesAsString);

			var cpscOption = new CusUSLVItemPGADisclaimOption(Factory, "CPSC");
			AssertEquals("CPSC option should contain 2 disclaim reasons", 2, cpscOption.PGADisclaimReasonList.Count);
			AssertEquals("CPSC option should contain disclaim reasons A and B", "A, B", cpscOption.PGADisclaimReasonList.CodesAsString);

			var deaOption = new CusUSLVItemPGADisclaimOption(Factory, "DEA");
			AssertEquals("DEA option should contain 1 disclaim reasons", 1, deaOption.PGADisclaimReasonList.Count);
			AssertEquals("DEA option should contain disclaim reasons A", "A", deaOption.PGADisclaimReasonList.CodesAsString);

			var hfcOption = new CusUSLVItemPGADisclaimOption(Factory, "HFC");
			AssertEquals("HFC option should contain 4 disclaim reasons", 4, hfcOption.PGADisclaimReasonList.Count);
			AssertEquals("HFC option should contain disclaim reasons A to D", "A, B, C, D", hfcOption.PGADisclaimReasonList.CodesAsString);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CusUSLVItemPGADisclaimOption(Factory, "TST");
		}
	}
}
