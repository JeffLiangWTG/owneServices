using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DeliveryOrderHazmat))]
	sealed class DeliveryOrderHazmatTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<DeliveryOrderHazmat>
	{
		public void TestUpdateHazmatDetails()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "2643";
			subs.DG_Variant = "Z";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs.DG_Class = "1.3";
			subs.DG_PG = "I";
			subs.DG_PSN = "PROP DUMMY BLAH";
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.DeliveryOrderHeaders.AddNew();
			var hazmat = header.DeliveryOrderHazmats.AddNew();
			hazmat.US_UNNumber = ZString.Empty;
			AssertEquals(ZString.Empty, hazmat.US_HazardClass);
			AssertEquals(ZString.Empty, hazmat.US_ProperShippingName);
			AssertEquals(ZString.Empty, hazmat.US_PackingGroup);
			hazmat.US_UNNumber = "ZZSDF";
			AssertEquals(ZString.Empty, hazmat.US_HazardClass);
			AssertEquals(ZString.Empty, hazmat.US_ProperShippingName);
			AssertEquals(ZString.Empty, hazmat.US_PackingGroup);
			hazmat.US_UNNumber = "2643Z";
			AssertEquals("1.3", hazmat.US_HazardClass);
			AssertEquals("PROP DUMMY BLAH", hazmat.US_ProperShippingName);
			AssertEquals("I", hazmat.US_PackingGroup);
		}

		public void TestDisplayUNNumber()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "2643";
			subs.DG_Variant = "Z";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs.DG_Class = "1.3";
			subs.DG_PG = "I";
			subs.DG_PSN = "PROP DUMMY BLAH";
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.DeliveryOrderHeaders.AddNew();
			var hazmat = header.DeliveryOrderHazmats.AddNew();
			hazmat.US_UNNumber = "ZZSDF";
			AssertEquals("ZZSDF", hazmat.DisplayUNNumber);
			hazmat.US_UNNumber = "2643Z";
			AssertEquals("2643", hazmat.DisplayUNNumber);
		}

		public void TestDefault()
		{
			var line = Factory.New<DeliveryOrderHazmat>();
			AssertEquals(CusAddInfoSchema.Constants.Prefix, line.B7_ParentTableCode);
			AssertEquals(CusAddInfoTypeAttribute.Codes.USDeliveryOrderHazmat, line.B7_Type);
		}

		public void TestDeleteWhenEmpty()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.DeliveryOrderHeaders.AddNew();
			var hazmat = header.DeliveryOrderHazmats.AddNew();
			hazmat.US_UNNumber = "2003";
			Factory.Save();
			AssertEquals(false, hazmat.IsDeleted);
			hazmat.US_UNNumber = ZString.Empty;
			Factory.Save();
			AssertEquals(true, hazmat.IsDeleted);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var header = declaration.DeliveryOrderHeaders.AddNew();
			var hazmat = header.DeliveryOrderHazmats.AddNew();
			hazmat.US_UNNumber = "2003";
			return hazmat;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<DeliveryOrderHazmat>();
		}
	}
}
