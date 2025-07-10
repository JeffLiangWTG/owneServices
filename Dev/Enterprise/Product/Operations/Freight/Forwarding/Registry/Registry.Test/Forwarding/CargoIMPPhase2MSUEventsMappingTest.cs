using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	[TestedType(typeof(CargoIMPPhase2MSUEventsMapping))]
	public class CargoIMPPhase2MSUEventsMappingTest : RegistryBusinessObjectTemplateTestCase<CargoIMPPhase2MSUEventsMapping>
	{
		public void TestCheckIsUniqueInCollectionForEventAndReference()
		{
			var errorMessage = "This event code is duplicated. For duplicate events, Event References must be unique and must not be blank.";
			var collection = new CargoIMPPhase2MSUEventsMappingCollection();

			var map1 = collection.AddNew();
			map1.EnterpriseEvent = AutoEvents.CargoCheckin.Code;
			map1.EnterpriseEventReference = string.Empty;

			map1.RunPreSaveValidation();
			AssertNoError(map1.EnterpriseEventInfo, errorMessage);
			AssertNoError(map1.EnterpriseEventReferenceInfo, errorMessage);

			var map2 = collection.AddNew();
			map2.EnterpriseEvent = AutoEvents.CargoCheckin.Code;
			map2.EnterpriseEventReference = string.Empty;

			map1.RunPreSaveValidation();
			AssertHasError(map1.EnterpriseEventInfo, errorMessage);
			AssertHasError(map1.EnterpriseEventReferenceInfo, errorMessage);

			map2.RunPreSaveValidation();
			AssertHasError(map2.EnterpriseEventInfo, errorMessage);
			AssertHasError(map2.EnterpriseEventReferenceInfo, errorMessage);

			map1.EnterpriseEventReference = "FAC=CFS";

			map1.RunPreSaveValidation();
			AssertHasError(map1.EnterpriseEventInfo, errorMessage);
			AssertHasError(map1.EnterpriseEventReferenceInfo, errorMessage);

			map2.RunPreSaveValidation();
			AssertNoError(map2.EnterpriseEventInfo, errorMessage);
			AssertNoError(map2.EnterpriseEventReferenceInfo, errorMessage);

			map2.EnterpriseEventReference = "LOC=AU";

			map1.RunPreSaveValidation();
			AssertNoError(map1.EnterpriseEventInfo, errorMessage);
			AssertNoError(map1.EnterpriseEventReferenceInfo, errorMessage);

			map2.RunPreSaveValidation();
			AssertNoError(map2.EnterpriseEventInfo, errorMessage);
			AssertNoError(map2.EnterpriseEventReferenceInfo, errorMessage);
		}

		public void TestValidateEnterpriseEvent()
		{
			var collection = new CargoIMPPhase2MSUEventsMappingCollection();
			var map = collection.AddNew();

			map.EnterpriseEvent = string.Empty;
			AssertHasError(map.EnterpriseEventInfo, "Please enter a value.");

			map.EnterpriseEvent = "INI";
			AssertHasError(map.EnterpriseEventInfo, "Enter a valid selection.");
			AssertNoError(map.EnterpriseEventInfo, "Please enter a value.");

			map.EnterpriseEvent = AutoEvents.GateInCFSReplacedByGINEventCode;
			AssertNoError(map.EnterpriseEventInfo, "Enter a valid selection.");
			AssertHasWarning(map.EnterpriseEventInfo, "This event code is no longer used.");

			map.EnterpriseEvent = AutoEvents.GateOutCFSReplacedByGOUEventCode;
			AssertHasWarning(map.EnterpriseEventInfo, "This event code is no longer used.");

			map.EnterpriseEvent = AutoEvents.CargoCheckin.Code;
			AssertNoWarning(map.EnterpriseEventInfo, "This event code is no longer used.");
		}

		public void TestValidateCargoIMPPhase2MSUEvent()
		{
			CargoIMPPhase2MSUEventsMapping map1 = new CargoIMPPhase2MSUEventsMapping();
			map1.CargoIMPPhase2MSUEvent = "INI";
			Assert(map1.HasErrors);
			map1.CargoIMPPhase2MSUEvent = CargoIMPPhase2MSUEventCodeList.Codes.DOC;
			Assert(!map1.HasErrors);

			map1.CargoIMPPhase2MSUEvent = "";
			Assert(map1.HasErrors);
		}

		public void TestEnterpriseEventDescription()
		{
			CargoIMPPhase2MSUEventsMapping map1 = new CargoIMPPhase2MSUEventsMapping();
			map1.EnterpriseEvent = Events.Attached.Code;
			AssertEquals(Events.Attached.Description, map1.EnterpriseEventDescription);
			map1.EnterpriseEvent = Events.AvailableTo.Code;
			AssertEquals(Events.AvailableTo.Description, map1.EnterpriseEventDescription);
		}

		#region Implementation

		protected override CargoIMPPhase2MSUEventsMapping GetBusinessObjectToClone()
		{
			return new CargoIMPPhase2MSUEventsMapping();
		}

		protected override CargoIMPPhase2MSUEventsMapping GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
