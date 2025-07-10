using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.US.LVS.DataTransfer.Universal
{
	public class eTailUSLVClearanceDataObjectReader : USLVClearanceDataReader
	{
		public eTailUSLVClearanceDataObjectReader(UniversalShipment masterShipmentDataObject, UniversalShipment shipmentDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(masterShipmentDataObject, logger, factory)
		{
			this.shipmentDataObject = shipmentDataObject;
			this.forwardingShipmentJobNumber = shipmentDataObject.DataContext?.DataSourceCollection?.FirstOrDefault(s => s.Type.GetValueOrDefault() == nameof(DataContextType.ForwardingShipment))?.Key ?? ZString.Empty;
		}

		readonly UniversalShipment shipmentDataObject;
		readonly ZString forwardingShipmentJobNumber;

		#region Override

		protected override ZString MatchingKey => forwardingShipmentJobNumber;

		protected override ZString UseCode => LVSConstants.ETailUseCode;

		protected override DataObjectList<UniversalShipment> GetSubShipmentCollection()
		{
			var forwardingShipmentDataObject = dataObject.SubShipmentCollection.SingleOrDefault();
			return forwardingShipmentDataObject?.SubShipmentCollection;
		}

		protected override void PopulateContainerMode(CusUSLVClearance clearance, Dictionary<string, ValueSetter> delaySetters)
		{
			var containerMode = ContainerModes.Containerised;
			var transportMode = dataObject.TransportMode.GetCodeAsUpperCase();
			if (transportMode.Equals(TransportModes.Sea) && dataObject.ContainerMode.GetCodeAsUpperCase().Equals(ContainerModes.LCL)
				|| transportMode.Equals(TransportModes.Air))
			{
				containerMode = ContainerModes.NonContainerised;
			}

			SetValue(clearance, CusUSLVClearanceSchema.ULH_ContainerMode, containerMode, delaySetters);
		}

		protected override void PopulateValueFromDateCollection(CusUSLVClearance clearance, Dictionary<string, ValueSetter> delaySetters)
		{
			if (shipmentDataObject.DateCollection != null)
			{
				FillDates(clearance,
					shipmentDataObject.DateCollection,
					false,
					delaySetters,
					new DateTypeSchemaColumnMap(CusUSLVClearanceSchema.ULH_DepartureDate, DateType.Departure),
					new DateTypeSchemaColumnMap(CusUSLVClearanceSchema.ULH_DischargeDate, DateType.Arrival));
			}

			SetValue(clearance, CusUSLVClearanceSchema.ULH_EntryDate, CargoWise.Types.ZDate.Today, delaySetters);
		}

		protected override USLVConsignmentDataReader GetConsignmentDataReader(UniversalShipment subShipment, IXmlImportLogger logger, UniversalObjectFactory factory, CusUSLVClearance clearance, CusUSLVConsignment existingConsignment)
		{
			return new eTailUSLVConsignmentDataObjectReader(subShipment, logger, factory, clearance, existingConsignment);
		}

		#endregion
	}
}
