using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusInvPackLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUnitTypeList()
		{
			AssertEquals("UnitTypeList", typeof(CodeDescriptionPairList), lookups.UnitTypeList.GetType());
		}

		public void TestTypeOfDifferenceList_MatchesDBConstraint()
		{
			AssertEquals("Codes as string", "DEC, DIF, MIS, NEW", lookups.TypeOfDifferenceList.CodesAsString);
		}

		public void TestTypeOfDifferenceList_ValuesValidForDBConstraint()
		{
			cusInvPack.B5_ParentTableCode = "JE";
			foreach (var code in lookups.TypeOfDifferenceList.GetAllCodes())
			{
				cusInvPack.B5_TypeOfDifference = code;
				AssertNoExceptionThrown($"The database constraint for 'B5_TypeOfDifference' does not allow the value '{code}'. Consider updating the database constraint or revising the valid values in the lookup list.", Factory.Save);
			}
		}

		public void TestTypeOfDifferenceList_Cached()
		{
			var list = lookups.TypeOfDifferenceList;
			AssertSame(list, lookups.TypeOfDifferenceList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusInvPack = Factory.New<CusInvPackForTest>();
			lookups = new CusInvPackLookups(cusInvPack);
		}
		CusInvPack cusInvPack;
		CusInvPackLookups lookups;

		class CusInvPackForTest : CusInvPack<DummyBusinessObject, CusInvPackLookups, CusInvPackValidation>
		{
			public CusInvPackForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
		}
	}
}
