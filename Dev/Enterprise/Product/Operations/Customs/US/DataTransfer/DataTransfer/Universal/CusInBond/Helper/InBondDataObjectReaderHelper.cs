using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class InBondDataObjectReaderHelper : UniversalCommonReaderHelper
	{
		public InBondDataObjectReaderHelper(UniversalObjectFactory factory, string dataProviderForCodeMapping = null)
			: base(factory, Core.Constants.CountryCodes.UnitedStates, dataProviderForCodeMapping)
		{
		}

		public void MarkUnprocessedExistingBillsFor(CusInBondHeader header)
		{
			foreach (var bill in GetAllInBondBills(header))
			{
				if (!ExistingBillProcessingDictionary.ContainsKey(bill))
				{
					ExistingBillProcessingDictionary.Add(bill, false);
				}
			}
		}

		protected virtual IEnumerable<CusInBondBill> GetAllInBondBills(CusInBondHeader header)
		{
			header.Bills.Refresh();
			return header.Bills.OfType<CusInBondBill>();
		}

		public void MarkProcessed(CusInBondBill bill)
		{
			if (ExistingBillProcessingDictionary.ContainsKey(bill))
			{
				ExistingBillProcessingDictionary[bill] = true;
			}
		}

		public void DeleteUnprocessedBillsFor(CusInBondHeader header, IXmlImportLogger logger)
		{
			foreach (var pair in ExistingBillProcessingDictionary.ToArray())
			{
				if (!pair.Value && pair.Key.B0_BH == header.PK)
				{
					var bill = pair.Key;
					ExistingBillProcessingDictionary.Remove(bill);
					LogDelete(logger, bill);
					bill.Delete();
				}
			}
		}

		protected void LogDelete(IXmlImportLogger logger, BusinessObject bizObj)
		{
			logger.Log(LogType.Information, Res.GetString("B3B036E5-FF5D-40A2-BF2B-3338DC739658", "Deleted {0} from {1}.", bizObj.HumanReadableName, "UniversalShipment"));
		}

		Dictionary<CusInBondBill, bool> ExistingBillProcessingDictionary
		{
			get { return existingBillProcessingDictionary ?? (existingBillProcessingDictionary = new Dictionary<CusInBondBill, bool>()); }
		}
		Dictionary<CusInBondBill, bool> existingBillProcessingDictionary;

		public void CollectBillLink(CusInBondBill billBO, AdditionalBill billData)
		{
			if (billData.Link.HasValue && !linkToBillMap.ContainsKey(billData.Link.Value))
			{
				linkToBillMap.Add(billData.Link.Value, billBO);
			}
		}

		public CusInBondBill GetBill(ZInt? link)
		{
			CusInBondBill result = null;
			if (link.HasValue && linkToBillMap.ContainsKey(link.Value))
			{
				result = linkToBillMap[link.Value];
			}
			return result;
		}

		readonly Dictionary<int, CusInBondBill> linkToBillMap = new Dictionary<int, CusInBondBill>();

		public void CollectContainerPackingLineAndCommercialInvoiceLineDetails(Shipment shipment)
		{
			if (shipment != null)
			{
				CollectContainerDetails(shipment.ContainerCollection);
				CollectPackingLineDetails(shipment.PackingLineCollection);
				CollectCommercialInvoiceLineDetails(shipment.CommercialInfo);
			}
		}

		void CollectCommercialInvoiceLineDetails(UniversalCustoms.CommercialInfo commercialInfo)
		{
			linkToCommercialInvoiceLineMap.Clear();
			if (commercialInfo != null && commercialInfo.CommercialInvoiceCollection != null)
			{
				var commercialInvoiceHeaderData = commercialInfo.CommercialInvoiceCollection.FirstOrDefault();
				if (commercialInvoiceHeaderData != null && commercialInvoiceHeaderData.CommercialInvoiceLineCollection != null)
				{
					foreach (var commercialInvoiceLine in commercialInvoiceHeaderData.CommercialInvoiceLineCollection.Where(x => x.Link.HasValue))
					{
						var invoiceLineLink = commercialInvoiceLine.Link.Value;
						if (!linkToCommercialInvoiceLineMap.ContainsKey(invoiceLineLink))
						{
							linkToCommercialInvoiceLineMap.Add(invoiceLineLink, commercialInvoiceLine);
						}
					}
				}
			}
		}
		readonly Dictionary<int, UniversalCustoms.CommercialInvoiceLine> linkToCommercialInvoiceLineMap = new Dictionary<int, UniversalCustoms.CommercialInvoiceLine>();

		void CollectContainerDetails(IEnumerable<Container> containerCollection)
		{
			linkToContainerMap.Clear();
			if (containerCollection != null)
			{
				foreach (var container in containerCollection.Where(x => x.Link.HasValue))
				{
					var containerLink = container.Link.Value;
					if (!linkToContainerMap.ContainsKey(containerLink))
					{
						linkToContainerMap.Add(containerLink, container);
					}
				}
			}
		}
		readonly Dictionary<int, Container> linkToContainerMap = new Dictionary<int, Container>();

		void CollectPackingLineDetails(IEnumerable<PackingLine> packingLineCollection)
		{
			containerPackingLineMap.Clear();
			if (packingLineCollection != null)
			{
				foreach (var packingLine in packingLineCollection.Where(x => x.ContainerLink.HasValue))
				{
					var containerLink = packingLine.ContainerLink.Value;
					List<PackingLine> list;
					if (!containerPackingLineMap.TryGetValue(containerLink, out list))
					{
						list = new List<PackingLine>();
						containerPackingLineMap.Add(containerLink, list);
					}
					list.Add(packingLine);
				}
			}
		}
		readonly Dictionary<int, List<PackingLine>> containerPackingLineMap = new Dictionary<int, List<PackingLine>>();

		public UniversalCustoms.CommercialInvoiceLine GetCommercialInvoiceLine(ZInt? invoiceLineLink)
		{
			UniversalCustoms.CommercialInvoiceLine result = null;
			if (invoiceLineLink.HasValue && linkToCommercialInvoiceLineMap.ContainsKey(invoiceLineLink.Value))
			{
				result = linkToCommercialInvoiceLineMap[invoiceLineLink.Value];
			}
			return result;
		}

		public Container GetContainer(ZInt? containerLink)
		{
			Container result = null;
			if (containerLink.HasValue && linkToContainerMap.ContainsKey(containerLink.Value))
			{
				result = linkToContainerMap[containerLink.Value];
			}
			return result;
		}

		public bool IsPackingLineExist(ZInt? containerLink)
		{
			return containerLink.HasValue && containerPackingLineMap.ContainsKey(containerLink.Value);
		}

		public IEnumerable<PackingLine> GetPackingLineDetails(ZInt? containerLink)
		{
			var result = Enumerable.Empty<PackingLine>();
			if (IsPackingLineExist(containerLink))
			{
				result = containerPackingLineMap[containerLink.Value];
			}
			return result;
		}

		public void MarkUnprocessedExistingMovementsFor(CusInBondHeader header)
		{
			foreach (var moveHeader in GetAllInBondMoveHeaders(header))
			{
				if (!ExistingMovementProcessingDictionary.ContainsKey(moveHeader))
				{
					ExistingMovementProcessingDictionary.Add(moveHeader, false);
				}
			}
		}

		public void MarkProcessed(CusInBondMoveHeader moveHeader)
		{
			if (ExistingMovementProcessingDictionary.ContainsKey(moveHeader))
			{
				ExistingMovementProcessingDictionary[moveHeader] = true;
			}
		}

		public void DeleteUnprocessedMovementsFor(CusInBondHeader header, IXmlImportLogger logger)
		{
			foreach (var pair in ExistingMovementProcessingDictionary.ToArray())
			{
				if (!pair.Value && pair.Key.BM_BH == header.PK)
				{
					var moveHeader = pair.Key;
					ExistingMovementProcessingDictionary.Remove(moveHeader);
					LogDelete(logger, moveHeader);
					moveHeader.Delete();
				}
			}
		}

		Dictionary<CusInBondMoveHeader, bool> ExistingMovementProcessingDictionary
		{
			get { return existingMovementProcessingDictionary ?? (existingMovementProcessingDictionary = new Dictionary<CusInBondMoveHeader, bool>()); }
		}
		Dictionary<CusInBondMoveHeader, bool> existingMovementProcessingDictionary;

		protected virtual IEnumerable<CusInBondMoveHeader> GetAllInBondMoveHeaders(CusInBondHeader header)
		{
			return header.MovementHeaders.OfType<CusInBondMoveHeader>();
		}
	}
}
