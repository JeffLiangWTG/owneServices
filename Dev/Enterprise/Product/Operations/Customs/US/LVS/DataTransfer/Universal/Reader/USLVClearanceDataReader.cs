using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.DataTransfer.Universal
{
	public class USLVClearanceDataReader : ShipmentDataObjectReader<CusUSLVClearance>
	{
		public USLVClearanceDataReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
		: base(dataObject, logger, factory)
		{
		}

		protected override IMatchingBusinessEntityFinder<CusUSLVClearance> GetCombinedReferenceMatcher()
		{
			return null;
		}

		protected override CusUSLVClearance GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			CusUSLVClearance result = null;

			if (!MatchingKey.IsEmpty)
			{
				var query = new ZQuery(CusUSLVClearanceSchema.ULH_MatchingKey, MatchingKey);
				result = factory.LoadTop1<CusUSLVClearance>(query);
			}

			PreLoadConsignmentsWithHouseBill(result);
			return result;
		}

		protected override bool TryGetExistingBusinessObjectMatchedOnDataTargetKeyCore(ITopLevelDataObject universalItem, IXmlImportLogger logger, IDataTargetDataObject dataTarget, BusinessObjectFactory factory, out BusinessObject foundBusinessObject)
		{
			var result = base.TryGetExistingBusinessObjectMatchedOnDataTargetKeyCore(universalItem, logger, dataTarget, factory, out foundBusinessObject);
			if (result && foundBusinessObject is CusUSLVClearance shipment)
			{
				PreLoadConsignmentsWithHouseBill(shipment);
			}

			return result;
		}

		protected virtual ZString UseCode => ZString.Empty;

		protected virtual ZString MatchingKey => ZString.Empty;

		void PreLoadConsignmentsWithHouseBill(CusUSLVClearance matchingClearance)
		{
			duplicateBillNumberList = new List<ZString>();
			consignmentDict = new Dictionary<ZString, CusUSLVConsignment[]>();

			var subShipmentCollection = GetSubShipmentCollection();
			if (matchingClearance != null && subShipmentCollection?.Count > 0)
			{
				foreach (var subShipment in subShipmentCollection)
				{
					var houseBillNumber = GetHouseBillNumber(subShipment);
					if (!houseBillNumber.IsEmpty)
					{
						var matchedConsignments = matchingClearance.CusUSLVConsignments.OfType<CusUSLVConsignment>().Where(x => x.ULB_HouseBill.EqualsIgnoringCase(houseBillNumber)).ToArray();
						if (matchedConsignments?.Length > 0)
						{
							if (!consignmentDict.ContainsKey(houseBillNumber))
							{
								consignmentDict[houseBillNumber] = matchedConsignments;
							}
							else if (!duplicateBillNumberList.Contains(houseBillNumber))
							{
								duplicateBillNumberList.Add(houseBillNumber);
							}
						}
					}
					else
					{
						hasSubShipmentWithoutHouseBill = true;
						break;
					}
				}
			}
		}
		protected ZBool hasSubShipmentWithoutHouseBill;
		protected List<ZString> duplicateBillNumberList;
		protected Dictionary<ZString, CusUSLVConsignment[]> consignmentDict;

		ZString GetHouseBillNumber(Shipment shipment)
		{
			var billNumber = shipment.WayBillNumber.GetValueOrDefault();
			var billType = shipment.WayBillType.GetCodeAsUpperCase();
			return billType == WayBillTypeList.Codes.House ? billNumber : ZString.Empty;
		}

		protected virtual DataObjectList<Shipment> GetSubShipmentCollection()
		{
			return dataObject.SubShipmentCollection;
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(CusUSLVClearance targetBO)
		{
			var result = base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO);
			if (result.IsEmpty)
			{
				if (hasSubShipmentWithoutHouseBill)
				{
					result = Res.GetString("05948523-cd27-40fc-bace-14b7f13a9f6e", "There is at least one sub-shipment without house bill number in this universal shipment.");
				}
				else if (duplicateBillNumberList?.Count > 0)
				{
					result = Res.GetString("27a47dd3-280d-4ac3-890c-4c586cf4236c", "The sub-shipment collection in this universal shipment contains multiple records with same house bill number ({0})", string.Join(", ", duplicateBillNumberList));
				}
				else if (consignmentDict?.Count > 0)
				{
					var duplicateBillNumber = consignmentDict.Where(c => c.Value != null && c.Value.Length > 1).Select(x => x.Key).ToArray();
					if (duplicateBillNumber?.Length > 0)
					{
						result = Res.GetString("4d3c92cf-d951-4571-9702-4ceb13e4e8cb", "There are multiple bills with same bill number ({0}) matched in Low Value Entries {1}.", string.Join(", ", duplicateBillNumber), targetBO.ULH_JobNumber);
					}
				}
			}

			return result;
		}

		protected override CharacterCase StringValueCharacterCase => CharacterCase.Upper;

		protected override void PopulateBusinessObject(CusUSLVClearance targetBO)
		{
			var delaySetters = new Dictionary<string, ValueSetter>();
			SetValue(targetBO, CusUSLVClearanceSchema.ULH_UseCode, UseCode, delaySetters);
			SetValue(targetBO, CusUSLVClearanceSchema.ULH_MatchingKey, MatchingKey, delaySetters);
			SetValue(targetBO, CusUSLVClearanceSchema.ULH_TransportMode, dataObject.TransportMode.GetNullableCodeAsUpperCase(), delaySetters);

			if (dataObject.WayBillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.Master)
			{
				SetValue(targetBO, CusUSLVClearanceSchema.ULH_MasterBill, dataObject.WayBillNumber, delaySetters);
			}

			PopulateContainerMode(targetBO, delaySetters);
			SetValue(targetBO, CusUSLVClearanceSchema.ULH_RL_NKPortOfLoading, dataObject.PortOfLoading, delaySetters);
			SetValue(targetBO, CusUSLVClearanceSchema.ULH_RL_NKPortOfDischarge, dataObject.PortOfDischarge, delaySetters);
			SetValue(targetBO, CusUSLVClearanceSchema.ULH_PortOfEntry, GetPortOfEntryWithFallback(), delaySetters);
			SetValue(targetBO, CusUSLVClearanceSchema.ULH_ConveyanceName, dataObject.VesselName, delaySetters);
			SetValue(targetBO, CusUSLVClearanceSchema.ULH_VoyageFlightNo, dataObject.VoyageFlightNo, delaySetters);

			var branchPK = dataObject.GetBranchPK(factory.BOFactory);
			if (branchPK.IsValid)
			{
				SetValue(targetBO, CusUSLVClearanceSchema.ULH_GB, branchPK, delaySetters);
			}

			PopulateValueFromOrganizationAddressCollection(targetBO, delaySetters);
			PopulateValueFromAddInfoCollection(targetBO, delaySetters);
			PopulateValueFromDateCollection(targetBO, delaySetters);

			delaySetters.SetValueInSpecificOrder(GetClearanceDataSettingOrder(targetBO.PK));

			PopulateCusUSLVConsignmentCollection(targetBO);
		}

		ZString? GetPortOfEntryWithFallback()
		{
			var portOfEntry = dataObject.AddInfoCollection?.GetZStringValue(LVSConstants.AddInfoConstants.SchDEntry);

			if (!portOfEntry.HasValue && dataObject.PortOfFirstArrival.TryGetUNLOCOAsUpperCase(factory.BOFactory, out var portCode) && !portCode.IsEmpty)
			{
				var resolvedPortCode = USScheduleResolver.GetScheduleCode(MasterFiles.Business.Schedule.D, portCode, dataObject.TransportMode.GetNullableCodeAsUpperCase().GetValueOrDefault(), factory.BOFactory);
				if (!string.IsNullOrEmpty(resolvedPortCode))
				{
					portOfEntry = resolvedPortCode;
				}
			}

			return portOfEntry;
		}

		protected virtual void PopulateContainerMode(CusUSLVClearance targetBO, Dictionary<string, ValueSetter> delaySetters)
		{
			SetValue(targetBO, CusUSLVClearanceSchema.ULH_ContainerMode, dataObject.CustomsContainerMode.GetNullableCodeAsUpperCase(), delaySetters);
		}

		IEnumerable<ZString> GetClearanceDataSettingOrder(ZGuid pk)
		{
			yield return ColumnValueSetter.GetKey(pk, CusUSLVClearanceSchema.ULH_UseCode);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVClearanceSchema.ULH_MatchingKey);

			yield return ColumnValueSetter.GetKey(pk, CusUSLVClearanceSchema.ULH_TransportMode);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVClearanceSchema.ULH_ContainerMode);

			yield return ColumnValueSetter.GetKey(pk, CusUSLVClearanceSchema.ULH_PortOfLoading);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVClearanceSchema.ULH_PortOfDischarge);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVClearanceSchema.ULH_RL_NKPortOfLoading);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVClearanceSchema.ULH_RL_NKPortOfDischarge);

			yield return ColumnValueSetter.GetKey(pk, CusUSLVClearanceSchema.ULH_CarrierSCAC);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVClearanceSchema.ULH_MasterBillIssuerSCAC);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVClearanceSchema.ULH_MasterBill);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVClearanceSchema.ULH_OH_Importer);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVClearanceSchema.ULH_IORType);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVClearanceSchema.ULH_IORReference);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVClearanceSchema.ULH_PortOfEntry);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVClearanceSchema.ULH_RemoteLocationFiling);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVClearanceSchema.ULH_DischargeDate);
			yield return ColumnValueSetter.GetKey(pk, CusUSLVClearanceSchema.ULH_EntryDate);
		}

		void PopulateValueFromOrganizationAddressCollection(CusUSLVClearance clearance, Dictionary<string, ValueSetter> delaySetters)
		{
			var localClientAddress = GetMatchingOrgAddress(AddressTypes.SendersLocalClient);
			if (localClientAddress != null)
			{
				SetValue(clearance, CusUSLVClearanceSchema.ULH_OH_Client, localClientAddress.OA_OH, delaySetters);
			}

			var importerAddress = GetMatchingOrgAddress(nameof(DocAddressType.ImporterOfRecord));
			if (importerAddress != null)
			{
				SetValue(clearance, CusUSLVClearanceSchema.ULH_OH_Importer, importerAddress.OA_OH, delaySetters);
			}
		}

		protected virtual void PopulateValueFromDateCollection(CusUSLVClearance clearance, Dictionary<string, ValueSetter> delaySetters)
		{
			if (dataObject.DateCollection?.Count > 0)
			{
				var clearanceRow = GetColumnIndexer(clearance);

				FillDates(clearanceRow, dataObject.DateCollection, false, delaySetters, new DateTypeSchemaColumnMap(CusUSLVClearanceSchema.ULH_DepartureDate, DateType.LoadingDate), new DateTypeSchemaColumnMap(CusUSLVClearanceSchema.ULH_DischargeDate, DateType.DischargeDate));
			}
		}

		protected void PopulateCusUSLVConsignmentCollection(CusUSLVClearance clearance)
		{
			var subShipmentCollection = GetSubShipmentCollection();
			if (subShipmentCollection?.Count > 0)
			{
				foreach (var subShipment in subShipmentCollection)
				{
					var houseBillNumber = GetHouseBillNumber(subShipment);
					CusUSLVConsignment[] matchingConsignments;
					consignmentDict.TryGetValue(houseBillNumber, out matchingConsignments);
					GetConsignmentDataReader(subShipment, logger, factory, clearance, matchingConsignments?.FirstOrDefault()).ReadIntoBusinessObject();
				}
			}
		}

		protected virtual USLVConsignmentDataReader GetConsignmentDataReader(Shipment subShipment, IXmlImportLogger logger, UniversalObjectFactory factory, CusUSLVClearance clearance, CusUSLVConsignment existingConsignment)
		{
			return new USLVConsignmentDataReader(subShipment, logger, factory, clearance, existingConsignment);
		}

		void PopulateValueFromAddInfoCollection(CusUSLVClearance clearance, Dictionary<string, ValueSetter> delaySetters)
		{
			if (dataObject.AddInfoCollection?.Count > 0)
			{
				SetValue(clearance, CusUSLVClearanceSchema.ULH_EntryFilerCode, dataObject.AddInfoCollection.GetZStringValue(LVSConstants.AddInfoConstants.EntryFilerCode), delaySetters);
				SetValue(clearance, CusUSLVClearanceSchema.ULH_US_NKLocationOfGoods, dataObject.AddInfoCollection.GetZStringValue(LVSConstants.AddInfoConstants.US_NKLocationOfGoods), delaySetters);
				SetValue(clearance, CusUSLVClearanceSchema.ULH_US_NKCentralizedExamSite, dataObject.AddInfoCollection.GetZStringValue(LVSConstants.AddInfoConstants.US_NKCentralizedExamSite), delaySetters);
				SetValue(clearance, CusUSLVClearanceSchema.ULH_IORType, dataObject.AddInfoCollection.GetZStringValue(LVSConstants.AddInfoConstants.IORType), delaySetters);
				SetValue(clearance, CusUSLVClearanceSchema.ULH_IORReference, dataObject.AddInfoCollection.GetZStringValue(LVSConstants.AddInfoConstants.IORReference), delaySetters);
				SetValue(clearance, CusUSLVClearanceSchema.ULH_MasterBillIssuerSCAC, dataObject.AddInfoCollection.GetZStringValue(LVSConstants.AddInfoConstants.MasterWayBillIssuerSCAC), delaySetters);
				SetValue(clearance, CusUSLVClearanceSchema.ULH_CarrierSCAC, dataObject.AddInfoCollection.GetZStringValue(LVSConstants.AddInfoConstants.UI_NKCarrierSCAC), delaySetters);
				SetValue(clearance, CusUSLVClearanceSchema.ULH_PortOfLoading, dataObject.AddInfoCollection.GetZStringValue(LVSConstants.AddInfoConstants.SchDLoading), delaySetters);
				SetValue(clearance, CusUSLVClearanceSchema.ULH_PortOfDischarge, dataObject.AddInfoCollection.GetZStringValue(LVSConstants.AddInfoConstants.SchDArrival), delaySetters);
				SetValue(clearance, CusUSLVClearanceSchema.ULH_ContactName, dataObject.AddInfoCollection.GetZStringValue(LVSConstants.AddInfoConstants.FilerName), delaySetters);
				SetValue(clearance, CusUSLVClearanceSchema.ULH_ContactPhone, dataObject.AddInfoCollection.GetZStringValue(LVSConstants.AddInfoConstants.FilerPhoneNumber), delaySetters);
				SetValue(clearance, CusUSLVClearanceSchema.ULH_RemoteLocationFiling, dataObject.AddInfoCollection.GetZBoolValue(LVSConstants.AddInfoConstants.EntryMode), delaySetters);
				SetValue(clearance, CusUSLVClearanceSchema.ULH_EntryDate, dataObject.AddInfoCollection.GetZDateValue(LVSConstants.AddInfoConstants.EntryDate), delaySetters);
			}
		}

		OrgAddress GetMatchingOrgAddress(string addressType)
		{
			var organizationAddress = (dataObject.OrganizationAddressCollection?.FirstOrDefault(addressType)) ?? (dataObject.SubShipmentCollection?.FirstOrDefault()?.OrganizationAddressCollection?.FirstOrDefault(addressType));

			return organizationAddress != null ? new OrganisationDataObjectReader(organizationAddress, logger, factory).GetMatched() : null;
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.USCustomsLowValueEntriesClearance; }
		}
	}
}
