using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class HazardousMaterialTest : TestCaseWithFactory
	{
		public void TestIHazardousMaterialMembers()
		{
			var contact = Factory.New<OrgContact>();
			contact.OC_ContactName = "BOB THE BUILDER";
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "1!10";
			subs.DG_Variant = "c";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			subs.DG_Class = "2.1";
			subs.DG_PSN = "PSN BLAB";
			var dataItem = Factory.New<UNDGDataItem>();
			var hazardous = new HazardousMaterial(dataItem);
			IHazardousMaterial line = hazardous;
			AssertEquals(ZDecimal.Zero, line.FlashPointTemp);
			AssertEquals(ZString.Empty, line.ContactName);
			AssertEquals("", line.HazMatClass);
			AssertEquals("", line.HazMatClassificationDesc);
			AssertEquals("", line.HazMatCode);
			AssertEquals("", line.HazMatDesc);
			AssertEquals(HazMatQualifierList.Codes.IMOCode, line.HazMatQualifier);
			AssertEquals(ZBool.False, line.IsFlashPointTempRelevant);
			AssertEquals(ZBool.False, line.IsHazRelevant);
			dataItem.LinkDefault(subs);
			dataItem.DI_DGFlashPoint = 120m;
			dataItem.DI_TechnicalName = "THIS IS A TECHICAL NAME";
			AssertEquals(120m, line.FlashPointTemp);
			dataItem.DI_OC_DGContact = contact.PK;
			AssertEquals("BOB THE BUILDER", line.ContactName);
			AssertEquals("2.1", line.HazMatClass);
			AssertEquals("THIS IS A TECHICAL NAME", line.HazMatClassificationDesc);
			AssertEquals("UN1!10", line.HazMatCode);
			AssertEquals("PSN BLAB", line.HazMatDesc);
			AssertEquals(HazMatQualifierList.Codes.IMOCode, line.HazMatQualifier);
			AssertEquals(ZBool.False, line.IsFlashPointTempRelevant);
			AssertEquals(ZBool.True, line.IsHazRelevant);
			dataItem.Substance.DG_FlashPoint = "2 cc";
			AssertEquals(ZBool.True, line.IsFlashPointTempRelevant);
		}
	}
}
