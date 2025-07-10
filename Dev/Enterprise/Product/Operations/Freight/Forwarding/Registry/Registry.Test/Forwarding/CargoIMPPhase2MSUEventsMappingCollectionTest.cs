using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	[TestedType(typeof(CargoIMPPhase2MSUEventsMappingCollection))]
	public class CargoIMPPhase2MSUEventsMappingCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<CargoIMPPhase2MSUEventsMappingCollection>
	{
		public void TestGetDefault()
		{
			var defaultValue = CargoIMPPhase2MSUEventsMappingCollection.GetDefault();
			AssertEquals(8, defaultValue.Count);

			foreach (CargoIMPPhase2MSUEventsMapping mapping in defaultValue)
			{
				if (mapping.CargoIMPPhase2MSUEvent == CargoIMPPhase2MSUEventCodeList.Codes.REW)
				{
					AssertEquals(AutoEvents.CargoReceivedAtDepot.Code, mapping.EnterpriseEvent);
				}
				else if (mapping.CargoIMPPhase2MSUEvent == CargoIMPPhase2MSUEventCodeList.Codes.DEW)
				{
					var isGOC = mapping.EnterpriseEvent == AutoEvents.GateOutCFSReplacedByGOUEvent.Code && mapping.EnterpriseEventReference.IsEmpty;
					var iSGOU = mapping.EnterpriseEvent == AutoEvents.GateOut.Code && mapping.EnterpriseEventReference == "FAC=CFS";

					Assert("should be GOC or GOU", isGOC || iSGOU);
				}
				else if (mapping.CargoIMPPhase2MSUEvent == CargoIMPPhase2MSUEventCodeList.Codes.DOC)
				{
					AssertEquals(AutoEvents.CargoCheckin.Code, mapping.EnterpriseEvent);
				}
				else if (mapping.CargoIMPPhase2MSUEvent == CargoIMPPhase2MSUEventCodeList.Codes.RIW)
				{
					var isGIC = mapping.EnterpriseEvent == AutoEvents.GateInCFSReplacedByGINEvent.Code && mapping.EnterpriseEventReference.IsEmpty;
					var isGIN = mapping.EnterpriseEvent == AutoEvents.GateIn.Code && mapping.EnterpriseEventReference == "FAC=CFS";

					Assert("should be GIC or GIN", isGIC || isGIN);
				}
				else if (mapping.CargoIMPPhase2MSUEvent == CargoIMPPhase2MSUEventCodeList.Codes.OFD)
				{
					AssertEquals(AutoEvents.Delivered.Code, mapping.EnterpriseEvent);
				}
				else if (mapping.CargoIMPPhase2MSUEvent == CargoIMPPhase2MSUEventCodeList.Codes.POD)
				{
					AssertEquals(AutoEvents.DeliveryCartageCompleteFinalised.Code, mapping.EnterpriseEvent);
				}
				else
				{
					Fail("Code is not allowed");
				}
			}
		}

		public void TestGetCargoIMPPhase2MSUEventFromEnterpriseEvent()
		{
			var value = CargoIMPPhase2MSUEventsMappingCollection.GetDefault();

			var msuEvent = value.GetCargoIMPPhase2MSUEventFromEnterpriseEvent(AutoEvents.GateIn.Code, string.Empty);
			AssertEquals(string.Empty, msuEvent);

			msuEvent = value.GetCargoIMPPhase2MSUEventFromEnterpriseEvent(AutoEvents.GateIn.Code, "|FAC=CFS");
			AssertEquals(CargoIMPPhase2MSUEventCodeList.Codes.RIW, msuEvent);

			msuEvent = value.GetCargoIMPPhase2MSUEventFromEnterpriseEvent(AutoEvents.GateIn.Code, "|FAC=CFS|LOC=US");
			AssertEquals(CargoIMPPhase2MSUEventCodeList.Codes.RIW, msuEvent);

			msuEvent = value.GetCargoIMPPhase2MSUEventFromEnterpriseEvent(AutoEvents.GateIn.Code, "|FAC=CTO");
			AssertEquals(string.Empty, msuEvent);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override CargoIMPPhase2MSUEventsMappingCollection GetCollectionToTest()
		{
			return new CargoIMPPhase2MSUEventsMappingCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CargoIMPPhase2MSUEventsMapping();
		}

		#endregion
	}
}
