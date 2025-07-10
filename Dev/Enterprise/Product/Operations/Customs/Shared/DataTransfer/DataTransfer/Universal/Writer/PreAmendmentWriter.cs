using System.Collections.Generic;
using System.Globalization;
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
	class PreAmendmentWriter : ITopLevelDataObjectWriter
	{
		public PreAmendmentWriter(IDataWritingManager manager, RecipientRoleType recipientRoleType)
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
			var warehouseIntegrationSupporter = (IWarehouseIntegrationSupporter)sourceBO;
			var sourceBOManager = sourceBO.GetUniversalDataContextManager() as IShipmentDataContextManager;
			var writer = sourceBOManager.GetShipmentDataObjectWriter(manager);
			var dataObject = (Shipment)writer.GetDataObject(sourceBO);
			dataObject.DataContext.SetCompanyAndDataProviderDetails(warehouseIntegrationSupporter.Company);
			var lastClearedOutwardShipment = warehouseIntegrationSupporter.GetLastClearedUniversalShipment(recipientRoleType);
			return lastClearedOutwardShipment == null ? dataObject : GetMergeDataObjectForPreAmendment(dataObject, lastClearedOutwardShipment);
		}

		Shipment GetMergeDataObjectForPreAmendment(Shipment dataObject, Shipment lastClearedOutwardShipment)
		{
			var result = dataObject;
			var factory = manager.Action != null ? manager.Action.FactoryForProcessing : null;
			if (factory != null)
			{
				var previousLineDetails = lastClearedOutwardShipment.GetWarehouseCustomsLineDetails(lastClearedOutwardShipment.DataContext);
				if (previousLineDetails != null && previousLineDetails.Any())
				{
					var currentLineDetails = dataObject.GetWarehouseCustomsLineDetails(dataObject.DataContext);
					if (currentLineDetails != null && currentLineDetails.Any())
					{
						var invoice = new CommercialInvoiceHeader(manager.WriterStrategy);
						invoice.SetCommercialInvoiceLineCollection(() =>
						{
							var commercialInvoiceLineCollection = new DataObjectList<CommercialInvoiceLine>();
							var mergedPreviousLinesData = GetMergedData(previousLineDetails);
							var mergedCurrentLinesData = GetMergedData(currentLineDetails);
							foreach (var mergedPreviousLineDataPair in mergedPreviousLinesData)
							{
								var mergedPreviousLineData = mergedPreviousLineDataPair.Value;
								CommercialInvoiceLine mergedCurrentData;
								if (mergedCurrentLinesData.TryGetValue(mergedPreviousLineDataPair.Key, out mergedCurrentData))
								{
									if (mergedCurrentData.BondedWarehouseQuantity.GetValueOrDefault() < mergedPreviousLineData.BondedWarehouseQuantity.GetValueOrDefault())
									{
										mergedCurrentData = mergedPreviousLineData;
									}
									mergedCurrentLinesData.Remove(mergedPreviousLineDataPair.Key);
								}
								else
								{
									mergedCurrentData = mergedPreviousLineData;
								}
								commercialInvoiceLineCollection.Add(mergedCurrentData);
							}
							commercialInvoiceLineCollection.AddRange(mergedCurrentLinesData.Values);
							return commercialInvoiceLineCollection;
						});
						var commercialInfo = result.CommercialInfo;
						commercialInfo.SubGroupCollection = null;
						commercialInfo.CommercialChargeCollection = null;
						commercialInfo.CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[] { invoice });
					}
					else
					{
						result = lastClearedOutwardShipment;
					}
				}
			}
			return result;
		}

		Dictionary<string, CommercialInvoiceLine> GetMergedData(IEnumerable<IWarehouseCustomsLineDetails> lineDetails)
		{
			var result = new Dictionary<string, CommercialInvoiceLine>();
			foreach (var lineDetail in lineDetails)
			{
				string key = GetKey(lineDetail);
				CommercialInvoiceLine invoiceLine;
				if (result.TryGetValue(key, out invoiceLine))
				{
					invoiceLine.BondedWarehouseQuantity = invoiceLine.BondedWarehouseQuantity.GetValueOrDefault() + lineDetail.InvoiceLine.BondedWarehouseQuantity.GetValueOrDefault();
				}
				else
				{
					result.Add(key, lineDetail.InvoiceLine);
				}
			}
			return result;
		}

		string GetKey(IWarehouseCustomsLineDetails lineDetail)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}_{1}_{2}", lineDetail.PreviousEntryNumber, lineDetail.PreviousEntryLineNumber, lineDetail.InvoiceLine.PartNo.GetValueOrDefault());
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
