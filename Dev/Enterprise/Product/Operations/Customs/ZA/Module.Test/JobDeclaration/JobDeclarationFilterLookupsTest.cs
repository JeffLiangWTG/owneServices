using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Customs.Common.ZA;

namespace Enterprise.Customs.ZA.Module.Testing
{
	sealed class JobDeclarationFilterLookupsTest : TestCaseWithFactory
	{
		public void TestMessageTypeList()
		{
			AssertEquals("MessageTypeList of correct type", typeof(ZAJobMessageTypeList), lookups.MessageTypeList.GetType());
			AssertEquals("EXW, EXP, IMP, IMX, MSC", lookups.MessageTypeList.CodesAsString);
		}

		public void TestTransportTypeList()
		{
			AssertEquals("TransportTypeList", ", AIR, SEA, ROA, RAI, MAI, FIX, OTH", lookups.TransportTypeList.CodesAsString);
		}

		public void TestContainerModeList()
		{
			AssertEquals("CNT, BBK, BLK, LQD", lookups.ContainerModeList.CodesAsString);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1820:Test for empty strings using string length", Justification = "Need to retain reference to constant member for test")]
		public void TestMessageStatusList()
		{
			var list1 = lookups.MessageStatusList();
			var list2 = filterBizObj.Factory.GetCachedValue<ZAMessageStatusList>();
			AssertEquals(list1.Count, list2.Count);
			foreach (ICodeDescription pair in list2)
			{
				var code = pair.Code;
				if (code == ZAMessageStatusList.Codes.NotSent)
				{
					code = DeclarationFilterConstants.EntryStatus.NotSentForFilter;
				}

				AssertEquals(code, pair.Description, list1.GetDescriptionFromCode(code));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			filterBizObj = new JobDeclarationFilterBusinessObject();
			lookups = filterBizObj.Lookups;
		}

		JobDeclarationFilterBusinessObject filterBizObj;
		JobDeclarationFilterLookups lookups;
	}
}
