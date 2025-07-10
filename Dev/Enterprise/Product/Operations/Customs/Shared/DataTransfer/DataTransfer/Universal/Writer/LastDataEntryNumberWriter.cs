using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DataTransfer.Universal
{
	class LastDataEntryNumberWriter : ITopLevelDataObjectWriter
	{
		public LastDataEntryNumberWriter(IDataWritingManager manager, RecipientRoleType recipientRoleType)
		{
			this.manager = Argument.NotNull(manager, "manager");
			this.recipientRoleType = recipientRoleType;
		}
		readonly IDataWritingManager manager;
		readonly RecipientRoleType recipientRoleType;

		#region ITopLevelDataObjectWriter Members

		public ZString EDIMessageSubType
		{
			get { return EDIMessageSubTypeList.Codes.XmlUniversalShipment; }
		}

		public ITopLevelDataObject GetDataObject(BusinessObject sourceBO)
		{
			var declaration = (IWarehouseIntegrationSupporter)sourceBO;
			var shipment = declaration.GetLastUniversalShipmentFromDataExport(recipientRoleType, null);
			shipment?.SetWriterStrategy(manager.WriterStrategy);
			if (shipment == null)
			{
				var sourceBOManager = sourceBO.GetUniversalDataContextManager() as IShipmentDataContextManager;
				var writer = sourceBOManager.GetShipmentDataObjectWriter(manager);
				shipment = (Shipment)writer.GetDataObject(sourceBO);
			}
			else
			{
				AddEntryNumberOverrideAndReplaceExistingEntryNumber(shipment, declaration.EntryNumber);
			}

			return shipment;
		}

		void AddEntryNumberOverrideAndReplaceExistingEntryNumber(Shipment shipment, ZString entryNumber)
		{
			if (!entryNumber.IsEmpty && shipment.SetEntryNumberCollection(() => shipment.EntryNumberCollection ?? new System.Collections.Generic.List<UniversalDataBuss.DataObjects.Universal.EntryNumber>()))
			{
				var entryNumberData = shipment.EntryNumberCollection.FirstOrDefault(x => x.IsEntryNumberPlaceHolderType());
				if (entryNumberData == null)
				{
					shipment.EntryNumberCollection.Add(new UniversalDataBuss.DataObjects.Universal.EntryNumber() { Number = entryNumber, Type = new EntryType() { Code = Constants.EntryNumberPlaceHolderType } });
				}
				else
				{
					entryNumberData.Number = entryNumber;
				}
				ReplaceEntryNumber(shipment.CommercialInfo, entryNumber);
			}
		}

		void ReplaceEntryNumber(CommercialInfo commercialInfo, ZString entryNumber)
		{
			if (commercialInfo != null)
			{
				var commercialInvoiceCollection = commercialInfo.CommercialInvoiceCollection;
				if (commercialInvoiceCollection != null)
				{
					foreach (var commercialInvoice in commercialInvoiceCollection.WhereNotNull())
					{
						var commercialInvoiceLineCollection = commercialInvoice.CommercialInvoiceLineCollection;
						if (commercialInvoiceLineCollection != null)
						{
							foreach (var commercialInvoiceLine in commercialInvoiceLineCollection)
							{
								commercialInvoiceLine.EntryNumber = entryNumber;
							}
						}
					}
				}
				var subGroupCollection = commercialInfo.SubGroupCollection;
				if (subGroupCollection != null)
				{
					foreach (var subGroup in subGroupCollection)
					{
						ReplaceEntryNumber(subGroup, entryNumber);
					}
				}
			}
		}

		public ZString RootElementName
		{
			get { return (NoResString)"Shipment"; }
		}

		public DataContextType TopLevelDataContextType
		{
			get { return DataContextType.CustomsDeclaration; }
		}

		#endregion
	}
}
