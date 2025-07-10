using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgPartUnit))]
	sealed class OrgPartUnitTest : EnterpriseBusinessObjectTestCase
	{
		#region TestQuantityInParent

		public void TestQuantityInParent()
		{
			var unit = Factory.New<OrgPartUnit>();
			unit.OF_QuantityInParent = 1;
			Assert(unit.OF_QuantityInParentInfo.GetErrors().Count() == 0);

			unit.OF_QuantityInParent = 0;
			Assert(unit.OF_QuantityInParentInfo.GetErrors().Count() != 0);

			unit.OF_QuantityInParent = -1;
			Assert(unit.OF_QuantityInParentInfo.GetErrors().Count() != 0);
		}

		#endregion

		#region TestMarkAsNeedValidation

		public void TestMarkAsNeedValidation()
		{
			var markAsNeedingValidationHitCount = 0;
			var part = Factory.New<OrgSupplierPart>();
			var unit = part.PartUnits.AddNew();
			Factory.MarkedAsNeedingValidation += (bizO) => markAsNeedingValidationHitCount++;
			unit.OF_PackType = "NEW";
			AssertEquals("When setting OF_PackType should Call MarkedAsNeedingValidation", 1, markAsNeedingValidationHitCount);

			unit.OF_ParentPackType = "NEW";
			AssertEquals("When setting OF_ParentPackType should Call MarkedAsNeedingValidation", 2, markAsNeedingValidationHitCount);

			unit.OF_OP = ZGuid.NewZGuid();
			AssertEquals("When setting OF_OP should call MarkedAsNeedingValidation", 3, markAsNeedingValidationHitCount);

			unit.OF_OP = part.PK;
			unit.Delete();
			AssertEquals("Calling delete on OrgPartUnit should call MarkedAsNeedingValidation", 5, markAsNeedingValidationHitCount);
		}

		#endregion

		#region TestCanDelete

		public void TestCanDelete()
		{
			var partUnit = OrgPartUnitValidationTest.GetBoxPartUnitForTestingPendingUOMPicks(Factory);
			Assert(!partUnit.CanDelete);
			AssertEquals(OrgPartUnitValidation.PendingUOMPicksDetected, partUnit.ReasonForNotAbleToDelete);

			var anotherPart = partUnit.SupplierPart.PartUnits.AddNew();
			anotherPart.OF_PackType = "UNT";
			anotherPart.OF_QuantityInParent = 15;
			anotherPart.OF_ParentPackType = "PLT";
			Assert("This part can be deleted as it is not in database yet", anotherPart.CanDelete);
			anotherPart.Delete();

			var weightPart = partUnit.SupplierPart.PartUnits.AddNew();
			weightPart.OF_PackType = "UNT";
			weightPart.OF_QuantityInParent = 10;
			weightPart.OF_ParentPackType = "KG";

			var weightPartInverted = partUnit.SupplierPart.PartUnits.AddNew();
			weightPartInverted.OF_PackType = "G";
			weightPartInverted.OF_QuantityInParent = 0.01m;
			weightPartInverted.OF_ParentPackType = "UNT";

			var volumeUnit = partUnit.SupplierPart.PartUnits.AddNew();
			volumeUnit.OF_PackType = "UNT";
			volumeUnit.OF_QuantityInParent = 1;
			volumeUnit.OF_ParentPackType = "M3";

			var volumeUnitInverted = partUnit.SupplierPart.PartUnits.AddNew();
			volumeUnitInverted.OF_PackType = "D3";
			volumeUnitInverted.OF_QuantityInParent = 0.001;
			volumeUnitInverted.OF_ParentPackType = "UNT";

			Factory.Save();

			Assert(weightPart.CanDelete);
			Assert(weightPartInverted.CanDelete);
			Assert(volumeUnit.CanDelete);
			Assert(volumeUnitInverted.CanDelete);

			// now lets change packtype for original partUnit to weight to attempt trick CanDelete check
			partUnit.OF_PackType = "G";
			partUnit.OF_ParentPackType = "KG";
			Assert("Still shouldn't be able to delete this part unit as original values are packs", !partUnit.CanDelete);
			AssertEquals(OrgPartUnitValidation.PendingUOMPicksDetected, partUnit.ReasonForNotAbleToDelete);
		}

		#endregion
	}

	#region DeferrableTriggerTest

	[TestedType(typeof(OrgPartUnit))]
	class OrgPartUnitDeferrableTriggerTest : DeferrableTriggerTestCase<OrgPartUnit>
	{
	}

	#endregion
}
