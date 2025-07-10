using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Rating.Services.Test
{
	[TestedType(typeof(UniversalChargeCodeMapBizo))]
	sealed class UniversalChargeCodeMapBizoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			var list = new UniversalChargeCodeMapBizoCollection(Factory);
			var biz1 = list.AddNew("CODE", "DESC");
			AssertEquals("CODE", biz1.Code);
			AssertEquals("DESC", biz1.Description);
			AssertEquals(ZGuid.Empty, biz1.GlobalChargeCodePk);
			AssertEquals(ZGuid.Empty, biz1.LocalChargeCodePk);
		}

		public void TestLocalChargeCodePk()
		{
			var mappedChargeCode = CreateChargeCode("ZZZ", false, "UNI");
			var unmappedChargeCode = CreateChargeCode("ZZ2", false, "");
			var globalUnmapped = CreateChargeCode("FOO", true, "");
			Factory.Save();

			var list = new UniversalChargeCodeMapBizoCollection(Factory);
			var biz1 = list.AddNew("UNI", "Uni Desc");
			var biz2 = list.AddNew("UNI2", "Uni Two");
			AssertNoErrors("new record is valid", biz1.LocalChargeCodePkInfo);

			biz1.LocalChargeCodePk = ZGuid.NewZGuid();
			AssertHasErrors("must be valid charge code pk", biz1.LocalChargeCodePkInfo);

			biz1.LocalChargeCodePk = mappedChargeCode.PK;
			AssertNoErrors("Charge codes can have multiple mappings", biz1.LocalChargeCodePkInfo);

			biz1.LocalChargeCodePk = globalUnmapped.PK;
			AssertHasErrors("global charge is invalid", biz1.LocalChargeCodePkInfo);

			biz1.LocalChargeCodePk = unmappedChargeCode.PK;
			AssertNoErrors("unmapped charge is OK", biz1.LocalChargeCodePkInfo);

			biz1.LocalChargeCodePk = ZGuid.Empty;
			biz1.GlobalChargeCodePk = ZGuid.Empty;
			AssertHasErrors("no selection is invalid", biz1.LocalChargeCodePkInfo);

			biz1.GlobalChargeCodePk = unmappedChargeCode.PK;
			biz1.LocalChargeCodePk = ZGuid.Empty;
			AssertNoErrors("local charge empty is OK if global charge is selected", biz1.LocalChargeCodePkInfo);

			biz1.GlobalChargeCodePk = ZGuid.Empty;
			biz1.LocalChargeCodePk = unmappedChargeCode.PK;
			AssertNoErrors("PRE", biz1.LocalChargeCodePkInfo);

			biz1.LocalChargeCodePk = unmappedChargeCode.PK;
			biz2.LocalChargeCodePk = unmappedChargeCode.PK;
			AssertNoErrors("duplicate mapping is valid", biz2.LocalChargeCodePkInfo);
			AssertNoErrors("duplicate mapping is valid", biz1.LocalChargeCodePkInfo);
		}

		public void TestGlobalChargeCodePk()
		{
			var mappedChargeCode = CreateChargeCode("ZZZ", true, "UNI");
			var unmappedChargeCode = CreateChargeCode("ZZ2", true, "");
			var localUnmapped = CreateChargeCode("FOO", false, "");
			Factory.Save();

			var list = new UniversalChargeCodeMapBizoCollection(Factory);
			var biz1 = list.AddNew("UNI", "Uni Desc");
			var biz2 = list.AddNew("UNI2", "Uni Two");
			AssertNoErrors("new record is valid", biz1.GlobalChargeCodePkInfo);

			biz1.GlobalChargeCodePk = ZGuid.NewZGuid();
			AssertHasErrors("must be valid charge code pk", biz1.GlobalChargeCodePkInfo);

			biz1.GlobalChargeCodePk = mappedChargeCode.PK;
			AssertNoErrors("Charge codes can have multiple mappings", biz1.GlobalChargeCodePkInfo);

			biz1.GlobalChargeCodePk = localUnmapped.PK;
			AssertHasErrors("local charge is invalid", biz1.GlobalChargeCodePkInfo);

			biz1.GlobalChargeCodePk = unmappedChargeCode.PK;
			AssertNoErrors("unmapped charge is OK", biz1.GlobalChargeCodePkInfo);

			biz1.GlobalChargeCodePk = ZGuid.Empty;
			biz1.LocalChargeCodePk = ZGuid.Empty;
			AssertHasErrors("no selection is invalid", biz1.GlobalChargeCodePkInfo);

			biz1.LocalChargeCodePk = unmappedChargeCode.PK;
			biz1.GlobalChargeCodePk = ZGuid.Empty;
			AssertNoErrors("global charge empty is OK if local charge is selected", biz1.GlobalChargeCodePkInfo);

			biz1.LocalChargeCodePk = ZGuid.Empty;
			biz1.GlobalChargeCodePk = unmappedChargeCode.PK;
			AssertNoErrors("PRE", biz1.GlobalChargeCodePkInfo);

			biz2.GlobalChargeCodePk = unmappedChargeCode.PK;
			AssertNoErrors("duplicate mapping is valid", biz2.GlobalChargeCodePkInfo);
			AssertNoErrors("duplicate mapping is valid", biz1.GlobalChargeCodePkInfo);
		}

		#region Helpers

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

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var list = new UniversalChargeCodeMapBizoCollection(Factory);
			var biz = list.AddNew("CODE", "DESC");
			return biz;
		}

		#endregion
	}

	[TestedType(typeof(UnmappedGlobalChargeCodeCollection))]
	public class UnmappedGlobalChargeCodeCollectionTest : ActiveBusinessObjectCollectionTestCase<UnmappedGlobalChargeCodeCollection>
	{
		protected override UnmappedGlobalChargeCodeCollection GetCollectionToTest()
			=> new UnmappedGlobalChargeCodeCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var elem = AccChargeCode.CreateGlobalChargeCode(Factory);
			//			elem.AC_UniversalChargeCodeMap = ZString.Empty;
			return elem;
		}
	}

	[TestedType(typeof(UnmappedChargeCodeCollection))]
	public class UnmappedChargeCodeCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
			=> new UnmappedChargeCodeCollection(Factory);
	}
}
