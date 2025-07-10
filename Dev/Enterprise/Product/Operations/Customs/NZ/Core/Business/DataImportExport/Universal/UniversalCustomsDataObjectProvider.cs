using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using UniversalShipment = Enterprise.Customs.DataTransfer.Universal;

namespace Enterprise.Customs.NZ.Business.Data.Universal
{
	public class UniversalCustomsDataObjectProvider : IUniversalCustomsDataObjectProvider
	{
		#region IUniversalCustomsDataObjectProvider Members

		public ICodeDescriptionPairList TableSpecificCusSupportingInfoTypeList(ZString tableCode, string dataContext = "")
		{
			return null;
		}

		public ICodeDescriptionPairList TableSpecificCusReferenceTypeList(ZString tableCode, string dataContext) => null;

		public ICodeDescriptionPairList TableSpecificCusAddInfoTypeList(ZString tableCode, string dataContext = "")
		{
			return CusAddInfoTypeListProvider.TableSpecificCusAddInfoTypeList(tableCode);
		}

		public ICodeDescriptionPairList TableSpecificAddInfoGroupTypesNeedInsertedToOtherTableList(ZString tableCode, string dataContext)
		{
			return null;
		}

		public ICodeDescriptionPairList TableSpecificCusCodeDataCodeList(ZString tableCode, string dataContext = "")
		{
			return null;
		}

		public ICodeDescriptionPairList TableSpecificCusCodeDataTypeList(ZString tableCode, string dataContext = "")
		{
			return null;
		}

		public ITopLevelDataObjectWriter GetNewDeclarationDataObjectWriter(IDataWritingManager manager)
		{
			return new DeclarationDataObjectWriter(manager);
		}

		public ITopLevelDataObjectReader GetNewJobDeclarationDataObjectReader(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment)
		{
			return new JobDeclarationDataObjectReader(declarationDataObject, logger, factory, shipment);
		}

		public IEnumerable<ITopLevelDataObjectReader> GetNewAirManifestDataObjectReaders(Shipment mawbDataObject, Shipment hvlvShipment, IXmlImportLogger logger, UniversalObjectFactory factory, bool singleHAWBCheck)
		{
			var shipmentDataObject = hvlvShipment ?? mawbDataObject;
			var isHVLV = mawbDataObject.IsHVLV();

			var maximumHAWBsAllowed = GetMaximumHouseBillsAllowed(mawbDataObject, CusMAWB.CheckIsImport);
			var incomingWaybillNumbers = GetIncomingWaybillNumbers(shipmentDataObject);
			var shipmentPK = GetShipmentPK(factory, shipmentDataObject);

			var existingMawbs = isHVLV
				? GetExistingHVLVMAWBs(mawbDataObject, hvlvShipment, factory, shipmentPK, maximumHAWBsAllowed, incomingWaybillNumbers)
				: new CusMAWB.Loader(factory.BOFactory).FindMatchingMAWBs(mawbDataObject, hvlvShipment);

			if (!RequireMultipleReader(existingMawbs.Length, mawbDataObject, maximumHAWBsAllowed, () => existingMawbs[0].ChildBills.Select(hb => hb.CS_MasterHouseBill).ToList()))
			{
				yield return CreateMAWBDataObjectReader(existingMawbs.FirstOrDefault(), null);
				yield break;
			}

			foreach (var mawb in existingMawbs.Cast<CusMAWB>())
			{
				var reader = CreateMAWBDataObjectReader(mawb, null);

				if (!reader.IsUpdatable(mawb))
				{
					yield return reader;
					yield break;
				}
			}

			(var remainingWaybillNumbers, var mawbCapacityMap) = CalculateCapacityForExistingBillsAndGetRemainingWaybillNumbers(existingMawbs, incomingWaybillNumbers, isHVLV, shipmentPK);

			foreach (var reader in CreateReadersForExistingCustomsJobs(remainingWaybillNumbers, mawbCapacityMap, maximumHAWBsAllowed, CreateMAWBDataObjectReader))
			{
				yield return reader;
			}

			foreach (var reader in CreateReadersForRemainingWaybillNumbers(remainingWaybillNumbers, maximumHAWBsAllowed, (addedWaybillNumbers) => CreateMAWBDataObjectReader(null, addedWaybillNumbers)))
			{
				yield return reader;
			}

			CusMAWBDataObjectReader CreateMAWBDataObjectReader(CusMAWB existingMawb, List<ZString> addedWaybillNumbers)
			{
				return new CusMAWBDataObjectReader(existingMawb, addedWaybillNumbers, mawbDataObject, hvlvShipment, logger, factory, shipmentPK, singleHAWBCheck);
			}
		}

		public IEnumerable<ITopLevelDataObjectReader> GetNewCusSCAOceanBillDataObjectReaders(Shipment dataObject, Shipment subShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			var shipmentDataObject = dataObject ?? subShipment;
			var isHVLV = dataObject.IsHVLV();

			var maximumHouseBillsAllowed = GetMaximumHouseBillsAllowed(dataObject, CusSCAOceanBill.CheckIsImport);
			var incomingWaybillNumbers = GetIncomingWaybillNumbers(shipmentDataObject);
			var shipmentPK = GetShipmentPK(factory, shipmentDataObject);

			var existingOceanBills = isHVLV
				? GetExistingHVLVOceanBills(dataObject, subShipment, factory, shipmentPK, maximumHouseBillsAllowed, incomingWaybillNumbers)
				: new CusSCAOceanBill.Loader(factory.BOFactory).FindMatchingSCAOceanBills(dataObject, subShipment);

			if (!RequireMultipleReader(existingOceanBills.Length, dataObject, maximumHouseBillsAllowed, () => existingOceanBills[0].HouseBills.Select(hb => hb.CA_HouseBill).ToList()))
			{
				yield return CreateOceanBillDataObjectReader(existingOceanBills.FirstOrDefault(), null);
				yield break;
			}

			foreach (var oceanBill in existingOceanBills.Cast<CusSCAOceanBill>())
			{
				var reader = CreateOceanBillDataObjectReader(oceanBill, null);
				if (!reader.IsUpdatable(oceanBill))
				{
					yield return reader;
					yield break;
				}
			}

			(var remainingWaybillNumbers, var oceanBillCapacityMap) = CalculateCapacityForExistingOceanBillsAndGetRemainingWaybillNumbers(existingOceanBills, incomingWaybillNumbers, isHVLV, shipmentPK);

			foreach (var reader in CreateReadersForExistingCustomsJobs(remainingWaybillNumbers, oceanBillCapacityMap, maximumHouseBillsAllowed, CreateOceanBillDataObjectReader))
			{
				yield return reader;
			}

			foreach (var reader in CreateReadersForRemainingWaybillNumbers(remainingWaybillNumbers, maximumHouseBillsAllowed, (addedWaybillNumbers) => CreateOceanBillDataObjectReader(null, addedWaybillNumbers)))
			{
				yield return reader;
			}

			CusSCAOceanBillDataObjectReader CreateOceanBillDataObjectReader(CusSCAOceanBill existingOceanBill, List<ZString> addedWaybillNumbers)
			{
				return new CusSCAOceanBillDataObjectReader(existingOceanBill, addedWaybillNumbers, dataObject, subShipment, logger, factory);
			}
		}

		CusMAWB[] GetExistingHVLVMAWBs(Shipment topLevelDataObject, Shipment subShipment, UniversalObjectFactory factory, ZGuid shipmentPK, int maxHAWBsAllowed, List<ZString> incomingWaybillNumbers)
		{
			var existingMawbs = GetHVLVBillsFromGenPivotCollection<CusMAWB>(
				subShipment, CusMAWBSchema.Constants.Prefix, factory);

			var matchingMawbs = new CusMAWB.Loader(factory.BOFactory).FindMatchingMAWBs(topLevelDataObject, subShipment);
			if (existingMawbs.Length == 0)
			{
				if (matchingMawbs.Length > 0 && HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.Value)
				{
					existingMawbs = GetLatestBillForMerging(subShipment, matchingMawbs, CusMAWBSchema.Constants.Prefix, bill => ((CusHAWB)bill.ChildBills.FirstOrDefault())?.Shipment, bill => maxHAWBsAllowed - bill.ChildBills.Count);
				}
				else
				{
					existingMawbs = matchingMawbs;
				}
			}

			CheckNoDuplicateWaybillsIfRequired(
				topLevelDataObject,
				() => matchingMawbs.SelectMany(mawb => mawb.ChildBills
													 .Where(hawb => hawb.CS_JS != shipmentPK)
													 .Select(hawb => hawb.CS_HAWB)),
				incomingWaybillNumbers);

			return existingMawbs;
		}

		CusSCAOceanBill[] GetExistingHVLVOceanBills(Shipment topLevelDataObject, Shipment subShipment, UniversalObjectFactory factory, ZGuid shipmentPK, int maxHouseBillsAllowed, List<ZString> incomingWaybillNumbers)
		{
			var existingOceanBills = GetHVLVBillsFromGenPivotCollection<CusSCAOceanBill>(
				subShipment, CusSCAOceanBillSchema.Constants.Prefix, factory);

			var matchingOceanBills = new CusSCAOceanBill.Loader(factory.BOFactory).FindMatchingSCAOceanBills(topLevelDataObject, subShipment);
			if (existingOceanBills.Length == 0)
			{
				if (matchingOceanBills.Length > 0 && HVLVDataRegistry.Instance.EnableHVLVMultiShipmentNZICRCRE.Value)
				{
					existingOceanBills = GetLatestBillForMerging(subShipment, matchingOceanBills, CusSCAOceanBillSchema.Constants.Prefix, bill => ((CusSCAHouse)bill.HouseBills.FirstOrDefault())?.Shipment, bill => maxHouseBillsAllowed - bill.HouseBills.Count);
				}
				else
				{
					existingOceanBills = matchingOceanBills;
				}
			}

			CheckNoDuplicateWaybillsIfRequired(
				topLevelDataObject,
				() => matchingOceanBills.SelectMany(waybillNumber => waybillNumber.HouseBills
													 .Where(houseBill => houseBill.CA_JS != shipmentPK)
													 .Select(houseBill => houseBill.CA_HouseBill)),
				incomingWaybillNumbers);

			return existingOceanBills;
		}

		(List<ZString>, Dictionary<CusMAWB, int>) CalculateCapacityForExistingBillsAndGetRemainingWaybillNumbers(IEnumerable<CusMAWB> existingMawbs, List<ZString> incomingWaybillNumbers, bool isHVLV, ZGuid shipmentPK)
		{
			var remainingWaybillNumbers = new List<ZString>(incomingWaybillNumbers.OrderBy(x => x));
			var mawbCapacityMap = new Dictionary<CusMAWB, int>();

			foreach (var mawb in existingMawbs.Cast<CusMAWB>())
			{
				var retainableHawbs = mawb.ChildBills
					.Cast<CusHAWB>()
					.Count(hawb => ShouldKeepHAWB(hawb, isHVLV, shipmentPK, incomingWaybillNumbers));

				remainingWaybillNumbers.RemoveAll(waybillNumber => mawb.ChildBills
					.OfType<CusHAWB>()
					.Any(hawb => hawb.CS_HAWB.Equals(waybillNumber)));

				mawbCapacityMap[mawb] = retainableHawbs;
			}

			return (remainingWaybillNumbers, mawbCapacityMap);
		}

		(List<ZString>, Dictionary<CusSCAOceanBill, int>) CalculateCapacityForExistingOceanBillsAndGetRemainingWaybillNumbers(IEnumerable<CusSCAOceanBill> existingOceanBills, List<ZString> incomingWaybillNumbers, bool isHVLV, ZGuid shipmentPK)
		{
			var remainingWaybillNumbers = new List<ZString>(incomingWaybillNumbers.OrderBy(x => x));
			var oceanBillCapacityMap = new Dictionary<CusSCAOceanBill, int>();

			foreach (var oceanBill in existingOceanBills.Cast<CusSCAOceanBill>())
			{
				var houseBillsToKeep = oceanBill.HouseBills
					.Cast<CusSCAHouse>()
					.Count(houseBill => ShouldKeepHouseBill(houseBill, isHVLV, shipmentPK, incomingWaybillNumbers));

				remainingWaybillNumbers.RemoveAll(waybillNumber => oceanBill.HouseBills
					.OfType<CusSCAHouse>()
					.Any(houseBill => houseBill.CA_HouseBill.Equals(waybillNumber)));

				oceanBillCapacityMap[oceanBill] = houseBillsToKeep;
			}

			return (remainingWaybillNumbers, oceanBillCapacityMap);
		}

		bool ShouldKeepHAWB(CusHAWB hawb, bool isHVLV, ZGuid shipmentPK, List<ZString> incomingWaybillNumbers)
		{
			var hawbNumber = hawb.CS_HAWB.ToUpper();
			return !hawb.CanDelete
				   || incomingWaybillNumbers.Contains(hawbNumber)
				   || (isHVLV && !hawb.ShouldMarkBillAsUnprocessed_HVLV(shipmentPK));
		}

		public ITopLevelDataObjectWriter GetNewAirManifestDataObjectWriter(IDataWritingManager manager)
		{
			return new CusMAWBDataObjectWriter(manager);
		}

		public ITopLevelDataObjectWriter GetNewAirManifestLineDataObjectWriter(IDataWritingManager manager, UniversalShipment.AirManifest.AirManifestDataObjectWriterHelper helper)
		{
			return new CusHAWBDataObjectWriter(manager, helper == null ? null : new AirManifestDataObjectWriterHelper(helper));
		}

		public UniversalShipment.UniversalDataObjectReaderHelper GetNewUniversalDataObjectReaderHelper(UniversalObjectFactory factory, string sourceCountryCode, string dataProviderForCodeMapping = null)
		{
			return new UniversalDataObjectReaderHelper(factory, sourceCountryCode);
		}

		public ITopLevelDataObjectReader GetNewStandaloneCommercialInvoiceDataObjectReader(Shipment shipmentDataObject, UniversalDataBuss.DataObjects.Universal.Customs.CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return null;
		}

		public StandaloneCommercialInvoiceDataObjectWriter GetNewStandaloneCommercialInvoiceDataObjectWriter(IDataWritingManager manager)
		{
			return null;
		}

		bool ShouldKeepHouseBill(CusSCAHouse houseBill, bool isHVLV, ZGuid shipmentPK, List<ZString> incomingWaybills)
		{
			var houseBillNumber = houseBill.CA_HouseBill.ToUpper();
			return !houseBill.CanDelete
				   || incomingWaybills.Contains(houseBillNumber)
				   || (isHVLV && !houseBill.ShouldDeleteUnprocessedHouseBill_HVLV(() => shipmentPK));
		}

		public ITopLevelDataObjectWriter GetNewCusSCAOceanBillDataObjectWriter(IDataWritingManager manager)
		{
			return new CusSCAOceanBillDataObjectWriter(manager);
		}

		#endregion

		#region HVLV

		IEnumerable<ITopLevelDataObjectReader> CreateReadersForExistingCustomsJobs<T>(List<ZString> remainingWaybillNumbers, Dictionary<T, int> capacityMap, int maxHouseBillsAllowed, Func<T, List<ZString>, ITopLevelDataObjectReader> createReader) where T : BusinessObject
		{
			foreach (var existingJob in capacityMap.OrderByDescending(pair => pair.Value).Select(pair => pair.Key))
			{
				var availableSlots = maxHouseBillsAllowed - capacityMap[existingJob];
				var addedWaybillNumbers = TakeWaybillNumbersFromRemaining(remainingWaybillNumbers, availableSlots);

				yield return createReader(existingJob, addedWaybillNumbers);
			}
		}

		IEnumerable<ITopLevelDataObjectReader> CreateReadersForRemainingWaybillNumbers(List<ZString> remainingWaybillNumbers, int maxHouseBillsAllowed, Func<List<ZString>, ITopLevelDataObjectReader> createReader)
		{
			while (remainingWaybillNumbers.Count > 0)
			{
				var addedWaybillNumbers = TakeWaybillNumbersFromRemaining(remainingWaybillNumbers, maxHouseBillsAllowed);
				yield return createReader(addedWaybillNumbers);
			}
		}

		T[] GetHVLVBillsFromGenPivotCollection<T>(Shipment subShipment, string hvlvBillPrefix, UniversalObjectFactory factory) where T : BusinessObject
		{
			var sourceShipment = LoadShipmentFromDataSource(subShipment, factory);
			return LoadGenPivotCollection(sourceShipment, hvlvBillPrefix)
				.Where(pivot => pivot.Relation2Object is T)
				.Select(pivot => pivot.Relation2Object as T)
				.ToArray();
		}

		T[] GetLatestBillForMerging<T>(Shipment subShipment, T[] matchingBills, string genPivotRelation2TableCode, Func<T, ForwardingShipment> originShipmentGetter, Func<T, int> capacityGetter) where T : BusinessObject
		{
			var result = Array.Empty<T>();
			var latestBill = matchingBills[0];
			var originShipment = originShipmentGetter(latestBill);
			var capacity = capacityGetter(latestBill);
			if (LoadGenPivotCollection(originShipment, genPivotRelation2TableCode).Length <= 1
				&& subShipment.GetLowestLevelShipment().Take(capacity + 1).Count() <= capacity)
			{
				result = [latestBill];
			}

			return result;
		}

		ForwardingShipment LoadShipmentFromDataSource(Shipment universalShipment, UniversalObjectFactory factory)
		{
			var result = default(ForwardingShipment);
			var sourceShipmentDataContext = universalShipment.DataContext.DataSourceCollection.First();
			if (sourceShipmentDataContext != null)
			{
				result = sourceShipmentDataContext.GetLoadedJobFromDataContextType(universalShipment, factory.BOFactory) as ForwardingShipment;
			}

			return result;
		}

		GenPivot[] LoadGenPivotCollection(ForwardingShipment shipment, string relation2TableCode)
		{
			var result = Array.Empty<GenPivot>();
			if (shipment != null && shipment.HVLVConsignmentHeader != null)
			{
				var zQuery = new ZQuery(GenPivotSchema.XX_RelationType, GenPivotTypes.HighVolumeLowValue);
				zQuery.AddToFilter(GenPivotSchema.XX_Relation1ID, shipment.HVLVConsignmentHeader.PK);
				zQuery.AddToFilter(GenPivotSchema.XX_Relation1TableCode, HVLVConsignmentHeaderSchema.Constants.Prefix);
				zQuery.AddToFilter(GenPivotSchema.XX_Relation2TableCode, relation2TableCode);
				result = shipment.Factory.Load<GenPivot>(zQuery);
			}

			return result;
		}

		int GetMaximumHouseBillsAllowed(Shipment dataObject, Func<ZString, ZString, bool> checkIsImport)
		{
			var isImport = checkIsImport(dataObject.PortOfLoading?.Code ?? ZString.Empty, dataObject.PortOfDischarge?.Code ?? ZString.Empty);
			return isImport
				? NZCustomsDataRegistry.Instance.MaxNumberOfECIManifestLinesAccepted.Value
				: NZCustomsDataRegistry.Instance.MaxNumberOfECIManifestLinesAcceptedCRE.Value;
		}

		List<ZString> GetIncomingWaybillNumbers(Shipment shipment) => shipment.GetLowestLevelShipment()
			.Select(x => x.WayBillNumber.GetValueOrDefault().ToUpper())
			.ToList();

		List<ZString> TakeWaybillNumbersFromRemaining(List<ZString> remainingWaybillNumbers, int count)
		{
			var result = remainingWaybillNumbers.Take(Math.Min(count, remainingWaybillNumbers.Count)).ToList();
			remainingWaybillNumbers.RemoveRange(0, result.Count);
			return result;
		}

		ZGuid GetShipmentPK(UniversalObjectFactory factory, Shipment shipment)
		{
			var shipmentKey = shipment?.DataContext?.GetMatchingDataSource(DataContextType.ForwardingShipment)?.Key ?? ZString.Empty;
			return shipmentKey.IsEmpty ? ZGuid.Empty : factory.LoadFromUniqueKey<ForwardingShipment>(JobShipmentSchema.JS_UniqueConsignRef, shipmentKey)?.PK ?? ZGuid.Empty;
		}

		void CheckNoDuplicateWaybillsIfRequired(Shipment mainDataObject, Func<IEnumerable<ZString>> existingWaybillsToCheck, IEnumerable<ZString> incomingWaybills)
		{
			var existingBills = existingWaybillsToCheck().Select(x => x.ToUpper());
			if (mainDataObject.IsHVLV() && existingBills.Any())
			{
				var duplicateLimitToSearch = 10;
				var hashset = new HashSet<ZString>(incomingWaybills.Select(x => x.ToUpper()));

				var firstTenDuplicates = existingBills.Where(x => hashset.Remove(x)).Take(duplicateLimitToSearch).ToList();

				if (firstTenDuplicates.Count > 0)
				{
					var builder = new ZStringBuilder();
					builder.AppendLine(Res.GetString("ff5b7f65-d696-458b-8b2c-7a4411242d4e", "Unable to merge this shipment into existing ICR/CRE because some House Bills already exist. Please fix these duplications and try again:"));
					foreach (var duplicateWaybill in firstTenDuplicates)
					{
						builder.AppendLine(duplicateWaybill);
					}

					if (firstTenDuplicates.Count == duplicateLimitToSearch)
					{
						builder.AppendLine(Res.GetString("2e43f132-1484-46ef-804a-a4f7b2a85e6d", "More duplicates may exist."));
					}

					throw new DataObjectValidationException(builder.ToString());
				}
			}
		}

		#endregion

		bool RequireMultipleReader(int billsCount, Shipment dataObject, int maximumNoOfHBsAllowed, Func<List<ZString>> houseBillsWayBillNumberCollectionGetter)
		{
			if (billsCount > 1)
			{
				return true;
			}
			else if (billsCount == 0)
			{
				return dataObject.GetLowestLevelShipment().Take(maximumNoOfHBsAllowed + 1).Count() > maximumNoOfHBsAllowed;
			}
			else
			{
				var houseBills = houseBillsWayBillNumberCollectionGetter();
				var capacity = maximumNoOfHBsAllowed - houseBills.Count;
				var incomingHouseBillCount = 0;
				foreach (var incomingHouseBill in dataObject.GetLowestLevelShipment())
				{
					if (!houseBills.Contains(incomingHouseBill.WayBillNumber ?? ZString.Empty))
					{
						incomingHouseBillCount++;
						if (incomingHouseBillCount > capacity)
						{
							return true;
						}
					}
				}

				return false;
			}
		}
	}
}
