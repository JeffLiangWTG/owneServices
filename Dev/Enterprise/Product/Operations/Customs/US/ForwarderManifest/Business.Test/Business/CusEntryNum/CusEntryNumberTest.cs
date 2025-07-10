using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	[TestedType(typeof(CusEntryNumber))]
	sealed class CusEntryNumberTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var cusEntryNum = (CusEntryNumber)base.GetNewBusinessObjectForDeleteTest(factory);
			cusEntryNum.CE_ParentTable = "JobDeclaration";

			return cusEntryNum;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var cusEntryNum = (CusEntryNumber)base.GetBusinessObjectForFetchForLoad();
			cusEntryNum.CE_ParentTable = "JobDeclaration";

			return cusEntryNum;
		}

		public void TestDefaultValues()
		{
			AssertEquals(false, entryNumber.CE_EntryIsSystemGenerated);
			AssertEquals(Core.Constants.CountryCodes.UnitedStates, entryNumber.CE_RN_NKCountryCode);
		}

		#region Implementation

		CusEntryNumber entryNumber;

		protected override void SetUp()
		{
			base.SetUp();
			entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_ParentTable = "JobDeclaration";
		}

		#endregion
	}
}
