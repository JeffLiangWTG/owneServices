using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbExternalPasswordLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPasswordStatusList()
		{
			var externalPassword = Factory.New<GlbExternalPassword>();
			var list1 = externalPassword.Lookups.PasswordStatusList;
			var list2 = externalPassword.Lookups.PasswordStatusList;
			AssertEquals(true, object.ReferenceEquals(list1, list2));
			AssertEquals(7, list1.Count);
			AssertEquals(PasswordStatusList.Descriptions.PasswordOK, list1.GetDescriptionFromCode("OK"));
			AssertEquals(PasswordStatusList.Descriptions.Deactivated, list1.GetDescriptionFromCode("INA"));
			AssertEquals(PasswordStatusList.Descriptions.Invalid, list1.GetDescriptionFromCode("INV"));
			AssertEquals(PasswordStatusList.Descriptions.Valid, list1.GetDescriptionFromCode("VAL"));
			AssertEquals(PasswordStatusList.Descriptions.Expired, list1.GetDescriptionFromCode("EXP"));
			AssertEquals(PasswordStatusList.Descriptions.Pending, list1.GetDescriptionFromCode("PEN"));
			AssertEquals(PasswordStatusList.Descriptions.Error, list1.GetDescriptionFromCode("FAL"));
		}

		public void TestPasswordTypesList()
		{
			var externalPassword = Factory.New<GlbExternalPassword>();
			var list1 = externalPassword.Lookups.PasswordTypeList;
			var list2 = Factory.GetCachedValue<PasswordTypesList>();
			AssertEquals(true, object.ReferenceEquals(list1, list2));
			(string description, string code)[] expectedList = new[]
			{
				(PasswordTypesList.Descriptions.ESB, "ESB"),
				(PasswordTypesList.Descriptions.AUB, "AUB"),
				(PasswordTypesList.Descriptions.ITA, "ITA"),
				(PasswordTypesList.Descriptions.ITB, "ITB"),
				(PasswordTypesList.Descriptions.ITM, "ITM"),
				(PasswordTypesList.Descriptions.ITX, "ITX"),
				(PasswordTypesList.Descriptions.NTP, "NTP"),
				(PasswordTypesList.Descriptions.NZB, "NZB"),
				(PasswordTypesList.Descriptions.SG4, "SG4"),
				(PasswordTypesList.Descriptions.SGA, "SGA"),
				(PasswordTypesList.Descriptions.NUT, "NUT"),
				(PasswordTypesList.Descriptions.NXM, "NXM"),
				(PasswordTypesList.Descriptions.CDS, "CDS"),
				(PasswordTypesList.Descriptions.CEP, "CEP"),
				(PasswordTypesList.Descriptions.EBD, "EBD"),
				(PasswordTypesList.Descriptions.TVA, "TVA"),
				(PasswordTypesList.Descriptions.TVF, "TVF"),
				(PasswordTypesList.Descriptions.UVC, "UVC"),
				(PasswordTypesList.Descriptions.TRK, "TRK"),
				(PasswordTypesList.Descriptions.FPC, "FPC"),
				(PasswordTypesList.Descriptions.CCT, "CCT"),
				(PasswordTypesList.Descriptions.TRU, "TRU"),
				(PasswordTypesList.Descriptions.INC, "INC"),
				(PasswordTypesList.Descriptions.INX, "INX"),
				(PasswordTypesList.Descriptions.INT, "INT"),
				(PasswordTypesList.Descriptions.INS, "INS"),
				(PasswordTypesList.Descriptions.HUI, "HUI"),
				(PasswordTypesList.Descriptions.UTB, "UTB"),
				(PasswordTypesList.Descriptions.MXB, "MXB"),
				(PasswordTypesList.Descriptions.PLB, "PLB"),
				(PasswordTypesList.Descriptions.EIM, "EIM"),
				(PasswordTypesList.Descriptions.BRS, "BRS"),
				(PasswordTypesList.Descriptions.ARB, "ARB"),
				(PasswordTypesList.Descriptions.CHD, "CHD"),
				(PasswordTypesList.Descriptions.CHC, "CHC"),
				(PasswordTypesList.Descriptions.CHT, "CHT"),
				(PasswordTypesList.Descriptions.IER, "IER"),
				(PasswordTypesList.Descriptions.IEM, "IEM"),
				(PasswordTypesList.Descriptions.KRB, "KRB"),
				(PasswordTypesList.Descriptions.PHA, "PHA"),
				(PasswordTypesList.Descriptions.PHU, "PHU"),
				(PasswordTypesList.Descriptions.PLN, "PLN"),
				(PasswordTypesList.Descriptions.PLC, "PLC"),
				(PasswordTypesList.Descriptions.IC2, "IC2"),
				(PasswordTypesList.Descriptions.BEC, "BEC"),
				(PasswordTypesList.Descriptions.NCB, "NCB"),
				(PasswordTypesList.Descriptions.ILC, "ILC"),
				(PasswordTypesList.Descriptions.CHR, "CHR"),
				(PasswordTypesList.Descriptions.MXL, "MXL"),
				(PasswordTypesList.Descriptions.NOD, "NOD"),
				(PasswordTypesList.Descriptions.ILS, "ILS"),
			};
			CombineAssertions(() =>
			{
				var list = new PasswordTypesList();
				foreach ((string description, string code) in expectedList)
				{
					AssertEquals(code, description, list.GetDescriptionFromCode(code));
					list.RemoveCode(code);
				}
				AssertEquals("Untested codes", string.Empty, list.CodesAsString);
			});
		}
	}
}
