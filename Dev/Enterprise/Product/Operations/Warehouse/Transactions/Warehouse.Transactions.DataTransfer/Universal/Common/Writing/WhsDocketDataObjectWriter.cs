using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public abstract class WhsDocketDataObjectWriter<T> : TopLevelDataObjectWriter<T, UniversalShipment>
		where T : WhsDocket
	{
		protected WhsDocketDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override ZString GetEDIMessageSubType() => EDIMessageSubTypeList.Codes.XmlUniversalShipment;

		#region Export Job

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
		protected override void PopulateDataObject(T docket, UniversalShipment shipmentDataObject)
		{
			var orderDataObject = new Order(writeManager.WriterStrategy);
			orderDataObject.OrderNumber = docket.WD_ExternalReference;
			orderDataObject.Status = GetDocketStatus(docket);

			var warehouse = docket.Warehouse;
			if (warehouse != null && !warehouse.WW_WarehouseCode.IsEmpty)
			{
				orderDataObject.Warehouse = new UniversalDataBuss.DataObjects.Universal.Warehouse { Code = warehouse.WW_WarehouseCode, Name = warehouse.WW_WarehouseName };
			}

			shipmentDataObject.Order = orderDataObject;
			ExportRelatedEntities(docket, shipmentDataObject);
			CustomLabelsCustomizedFieldDataObjectWriter.Write(WhsDocketSchema.Instance, docket, shipmentDataObject, new WhsDocket.CustomLabelsProvider(docket));
		}

		void ExportRelatedEntities(T docket, UniversalShipment shipmentDataObject)
		{
			var notes = docket.Notes.GetAllNotesVisibleToCurrentCompany().OrderBy(x => x.ST_Description);
			shipmentDataObject.SetNoteCollection(() => ProcessCollection(notes, new NoteDataObjectWriter(writeManager), CollectionContent.Partial));
			shipmentDataObject.AddOrgAddress(writeManager, docket.Client, DocAddressType.ConsignorDocumentaryAddress);
		}

		protected override IEnumerable<IPropertyValue> GetUserDefinedValues(T docket) => docket.GetUserDefinedValues();

		protected virtual CodeDescriptionPair GetDocketStatus(T docket) => ListHelper.GetWithDescription<CodeDescriptionPair>(docket.WD_DocketStatus, docket.Statuses);

		#endregion

		protected ZInt? ToZIntSafely(ZDecimal value, string fieldName)
		{
			var result = (ZInt?)null;

			if (value >= new ZDecimal(int.MinValue) && value <= new ZDecimal(int.MaxValue))
			{
				result = (ZInt)value;
			}
			else if (writeManager.Action.Notifications != null)
			{
				var warningMessage = Res.GetString("b22def86-56a3-4865-a86f-8c9b931bdb73",
					"Unable to convert {0} to an integer, as it has value {1}.",
					fieldName,
					value);
				writeManager.Action.Notifications.AddWarning(warningMessage);
			}

			return result;
		}
	}
}
