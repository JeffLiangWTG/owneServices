using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DataTransfer.Universal
{
	public class LastHoldWriter : ITopLevelDataObjectWriter
	{
		public LastHoldWriter(IDataWritingManager manager, RecipientRoleType recipientRoleType)
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
			var shipment = GetUniversalShipment(declaration, recipientRoleType);
			if (shipment == null)
			{
				var sourceBOManager = sourceBO.GetUniversalDataContextManager() as IShipmentDataContextManager;
				var writer = sourceBOManager.GetShipmentDataObjectWriter(manager);
				shipment = (Shipment)writer.GetDataObject(sourceBO);
			}
			else
			{
				ReplaceEntryNumberPlaceHolder(shipment, declaration.EntryNumber);
				declaration.UpdateHoldData(shipment, recipientRoleType);
			}

			return shipment;
		}

		protected virtual Shipment GetUniversalShipment(IWarehouseIntegrationSupporter declaration, RecipientRoleType recipientRoleType)
		{
			return declaration.GetLastHoldUniversalShipment(recipientRoleType);
		}

		void ReplaceEntryNumberPlaceHolder(Shipment shipment, ZString entryNumber)
		{
			if (!entryNumber.IsEmpty)
			{
				ReplaceEntryNumberPlaceHolder(shipment.CommercialInfo, entryNumber);
			}
		}

		void ReplaceEntryNumberPlaceHolder(CommercialInfo commercialInfo, ZString entryNumber)
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
							foreach (var commercialInvoiceLine in commercialInvoiceLineCollection.Where(x => x != null && x.EntryNumber.GetValueOrDefault() == Constants.EntryNumberPlaceHolder))
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
						ReplaceEntryNumberPlaceHolder(subGroup, entryNumber);
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
