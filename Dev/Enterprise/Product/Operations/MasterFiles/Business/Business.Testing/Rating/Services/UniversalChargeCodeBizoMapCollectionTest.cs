using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Rating.Services.Test
{
	[TestedType(typeof(UniversalChargeCodeMapBizoCollection))]
	public class UniversalChargeCodeMapBizoCollectionTest : NonPersistentBusinessObjectCollectionTestCase<UniversalChargeCodeMapBizoCollection>
	{
		public void TestRequiredSecurityRights()
		{
			var globalChargeCode = CreateChargeCode("ZZZ", true);
			var localChargeCode = CreateChargeCode("LOC", false);
			Factory.Save();
			var localLinkedChargeCode = LoadLocalChargeCode("ZZZ");

			var list = new UniversalChargeCodeMapBizoCollection(Factory);
			var biz1 = list.AddNew("C1", "C1");
			var biz2 = list.AddNew("C2", "C2");
			var biz3 = list.AddNew("C3", "C3");
			AssertMissingSecurityRights(list, false, false, false);

			biz1.LocalChargeCodePk = localChargeCode.PK;
			AssertMissingSecurityRights(list, false, false, true);

			biz1.LocalChargeCodePk = localLinkedChargeCode.PK;
			AssertMissingSecurityRights(list, false, true, false);

			biz1.LocalChargeCodePk = ZGuid.Empty;
			biz1.GlobalChargeCodePk = globalChargeCode.PK;
			AssertMissingSecurityRights(list, true, false, false);

			biz2.GlobalChargeCodePk = globalChargeCode.PK;
			AssertMissingSecurityRights(list, true, false, false);

			biz3.LocalChargeCodePk = localLinkedChargeCode.PK;
			AssertMissingSecurityRights(list, true, true, false);

			biz2.LocalChargeCodePk = localChargeCode.PK;
			AssertMissingSecurityRights(list, true, true, true);
		}

		void AssertMissingSecurityRights(UniversalChargeCodeMapBizoCollection list, bool expectGlobal, bool expectLinked, bool expectBasic)
		{
			var rights = list.RequiredSecurityRights(Env.Security);
			AssertEquals("global", expectGlobal ? 1 : 0, rights.Count(x => x == Env.Security.GlobalChargeCodesModify));
			AssertEquals("global", expectLinked ? 1 : 0, rights.Count(x => x == Env.Security.ChargeCodesLTGModify));
			AssertEquals("global", expectBasic ? 1 : 0, rights.Count(x => x == Env.Security.ChargeCodesModify));
		}

		protected override UniversalChargeCodeMapBizoCollection GetCollectionToTest()
		{
			return new UniversalChargeCodeMapBizoCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new UniversalChargeCodeMapBizo(Factory);
		}

		AccChargeCode CreateChargeCode(string code, bool isGlobal, string universalChargeCode = "")
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = code;
			chargeCode.AC_Desc = code + " Description";
			chargeCode.AC_ChargeType = Enterprise.Core.Constants.ChargeType.Disbursement;

			if (!string.IsNullOrWhiteSpace(universalChargeCode))
			{
				var mapping = chargeCode.UniversalChargeCodeMappingsCollection.AddNew();
				mapping.AUP_Code = universalChargeCode;
			}

			chargeCode.AC_GC = isGlobal ? ZGuid.Empty : Env.CurrentCompanyPK;
			return chargeCode;
		}

		AccChargeCode LoadLocalChargeCode(string code)
		{
			var query = new ZQuery(AccChargeCodeSchema.AC_Code, code);
			query.AddToFilter(AccChargeCodeSchema.AC_GC, Env.CurrentCompanyPK);
			return Factory.LoadTop1<AccChargeCode>(query);
		}
	}
}
