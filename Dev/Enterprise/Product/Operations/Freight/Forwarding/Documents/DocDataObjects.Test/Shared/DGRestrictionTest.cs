using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.DataTransfer.Universal.CodeLists;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(DGRestriction))]
	sealed class DGRestrictionTests : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DGRestriction
			{
				EmergencyScheduleFire = new CodeDescription(new EmergencyScheduleFireCodes()) { Code = "F-A" },
				EmergencyScheduleSpillage = new CodeDescription(new EmergencyScheduleSpillageCodes()) { Code = "S-A" },
				Standard = "IMO",
				ExceptedQuantityCode = "E0",
				FlashPoint = "25 cc",
				IMOClass = "6.1",
				MarinePollutantCode = "Y",
				PackedInLimitedQuantity = false,
				PackingGroup = "I",
				ProperShippingName = "CHOROACETONE, STABLIZED",
				State = "L",
				SubLabel1 = "3",
				SubLabel2 = "8",
				Code = "1695",
				Unno = "1695",
				Variant = ""
			};
		}
	}
}
