using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	class DtbConsignmentRunSheetDataObjectWriter : TopLevelDataObjectWriter<DtbConsignmentRunSheet, UniversalShipment>
	{
		public DtbConsignmentRunSheetDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.TransportConsignmentRunSheet;
		}

		protected override void PopulateDataObject(DtbConsignmentRunSheet runSheet, UniversalShipment dataObject)
		{
			var recipientRoleCodes = writeManager.Action.RecipientRoleDetails.Select(r => r.Type);
			var sendingToPickupDepot = recipientRoleCodes.Contains(RecipientRoleType.DTW);
			var sendingToDeliveryDepot = recipientRoleCodes.Contains(RecipientRoleType.ATW);

			if (sendingToPickupDepot || sendingToDeliveryDepot)
			{
				var actionType = sendingToPickupDepot ? ActionTypes.Codes.PickUp : ActionTypes.Codes.Delivery;
				var depotInstructions = runSheet.RunSheetInstructions.Where(i => ConsignmentRunSheetHelper.IsDepotInstruction(i, actionType)).ToArray();

				if (depotInstructions.Length == 1)
				{
					var writer = new DtbConsignmentRunSheetInstructionDataObjectWriter(writeManager);

					var instructionDataObject = writer.GetDataObject(depotInstructions.Single());
					dataObject.VoyageFlightNo = TruckRegistrationNumber(runSheet);
					dataObject.SetContainerCollection(() => instructionDataObject.ContainerCollection);
					dataObject.SetSubShipmentCollection(() => instructionDataObject.SubShipmentCollection);
					dataObject.SetOrganizationAddressCollection(() => instructionDataObject.OrganizationAddressCollection);
				}
			}
		}

		protected override IEnumerable<IPropertyValue> GetUserDefinedValues(DtbConsignmentRunSheet runSheet)
		{
			var helper = ObjectFactory.Get<ICustomValuesHelper>();

			return helper.GetUserDefinedValues(runSheet);
		}

		static ZString TruckRegistrationNumber(DtbConsignmentRunSheet runSheet)
		{
			var dynamicCollection = new DynamicBusinessObjectCollection(runSheet.Factory);
			var sqlText = Invariant($@"
SELECT TOP 1 RQ_ShortCode as ShortCode
FROM dbo.DtbConsignmentRunSheet
JOIN dbo.DtbEquipmentItem on LTE_ParentID = KG_PK and LTE_ParentTableCode = '{DtbConsignmentRunSheetSchema.Constants.Prefix}'
JOIN dbo.RefEquipment on RQ_PK = LTE_RQ_Equipment
WHERE RQ_IsVehicle = 1 and KG_PK = @RunSheetPK
ORDER BY RQ_ShortCode");

			var parameters = new ZSqlParameterCollection
			{
				{ "@RunSheetPK", runSheet.PK, DtbConsignmentRunSheetSchema.PK }
			};

			dynamicCollection.Load(sqlText, parameters);

			return dynamicCollection.Any() ? (ZString)dynamicCollection[0]["ShortCode"] : runSheet.KG_AdHocTruckRegistration;
		}
	}
}


