using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Registry
{
	[XmlSerializerAssembly("Enterprise.Freight.Forwarding.Registry.XmlSerializers")]
	public class CargoIMPPhase2MSUEventsMappingCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new CargoIMPPhase2MSUEventsMapping this[int i]
		{
			get { return (CargoIMPPhase2MSUEventsMapping)Elements[i]; }
		}

		public new CargoIMPPhase2MSUEventsMapping AddNew()
		{
			return (CargoIMPPhase2MSUEventsMapping)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CargoIMPPhase2MSUEventsMappingCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CargoIMPPhase2MSUEventsMapping();
		}

		public static CargoIMPPhase2MSUEventsMappingCollection GetDefault()
		{
			var result = new CargoIMPPhase2MSUEventsMappingCollection();

			var mapping = result.AddNew();
			mapping.CargoIMPPhase2MSUEvent = CargoIMPPhase2MSUEventCodeList.Codes.REW;
			mapping.EnterpriseEvent = AutoEvents.CargoReceivedAtDepot.Code;

			mapping = result.AddNew();
			using (mapping.GetValidationSuspender()) //Suppression for resources string test case - TestAccessingValuesOnlyDoesNotDemandResourceStrings
			{
				mapping.CargoIMPPhase2MSUEvent = CargoIMPPhase2MSUEventCodeList.Codes.DEW;
				mapping.EnterpriseEvent = AutoEvents.GateOutCFSReplacedByGOUEvent.Code;
			}

			mapping = result.AddNew();
			mapping.CargoIMPPhase2MSUEvent = CargoIMPPhase2MSUEventCodeList.Codes.DEW;
			mapping.EnterpriseEvent = AutoEvents.GateOut.Code;
			mapping.EnterpriseEventReference = DefaultRefForGINAndGOU;

			mapping = result.AddNew();
			mapping.CargoIMPPhase2MSUEvent = CargoIMPPhase2MSUEventCodeList.Codes.DOC;
			mapping.EnterpriseEvent = AutoEvents.CargoCheckin.Code;

			mapping = result.AddNew();
			using (mapping.GetValidationSuspender()) //Suppression for resources string test case - TestAccessingValuesOnlyDoesNotDemandResourceStrings
			{
				mapping.CargoIMPPhase2MSUEvent = CargoIMPPhase2MSUEventCodeList.Codes.RIW;
				mapping.EnterpriseEvent = AutoEvents.GateInCFSReplacedByGINEvent.Code;
			}

			mapping = result.AddNew();
			mapping.CargoIMPPhase2MSUEvent = CargoIMPPhase2MSUEventCodeList.Codes.RIW;
			mapping.EnterpriseEvent = AutoEvents.GateIn.Code;
			mapping.EnterpriseEventReference = DefaultRefForGINAndGOU;

			mapping = result.AddNew();
			mapping.CargoIMPPhase2MSUEvent = CargoIMPPhase2MSUEventCodeList.Codes.OFD;
			mapping.EnterpriseEvent = AutoEvents.Delivered.Code;

			mapping = result.AddNew();
			mapping.CargoIMPPhase2MSUEvent = CargoIMPPhase2MSUEventCodeList.Codes.POD;
			mapping.EnterpriseEvent = AutoEvents.DeliveryCartageCompleteFinalised.Code;

			return result;
		}

		static readonly ZString DefaultRefForGINAndGOU = "FAC=CFS"; // Default Event Reference For GIN And GOU

		public ZString GetCargoIMPPhase2MSUEventFromEnterpriseEvent(string entEvent, string reference)
		{
			foreach (CargoIMPPhase2MSUEventsMapping map in this)
			{
				if (map.EnterpriseEvent == entEvent && TriggerConditionEvaluator.IsConditionValueMatchReference(map.EnterpriseEventReference, reference))
				{
					return map.CargoIMPPhase2MSUEvent;
				}
			}

			return ZString.Empty;
		}
	}
}
