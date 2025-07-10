using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class JobDeclarationDataObjectReader : JobDeclarationDataObjectReader<JobDeclaration, Bill, CusContainer, JobComInvoiceGroupHeader>
	{
		public JobDeclarationDataObjectReader(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingShipment shipment = null)
			: base(declarationDataObject, logger, factory, shipment)
		{
		}

		protected new UniversalDataObjectReaderHelper Helper
		{
			get { return (UniversalDataObjectReaderHelper)base.Helper; }
		}

		protected override void ImportCountrySpecificRelatedData(JobDeclaration declaration)
		{
			if (Shipment == null && dataObject.SubShipmentCollection != null)
			{
				var inBondData = dataObject.SubShipmentCollection.FirstOrDefault(x => x.GetMatchingDataTarget(UniversalDataBuss.Integration.DataContextType.InBond) != null);
				if (inBondData != null)
				{
					var provider = ObjectFactory.Get<ICustomsInBondDataObjectReaderProvider>();
					var reader = provider.GetReader(inBondData, logger, factory, declaration);
					BusinessObject inBondHeaderBO = null;
					reader.ReadIntoBusinessObject(ref inBondHeaderBO);
				}
			}

			if (declaration.IsReconMessageType && !declaration.IsInDatabase && declaration.ReconDeclaration == null)
			{
				declaration.ReconDeclaration = new ReconDeclaration(declaration);
			}

			SynchroniseFIRMSCode(declaration);

			if (isFromHVLV && string.IsNullOrEmpty(declaration.US_EntryType))
			{
				declaration.US_EntryType = EntryTypeList.Codes.LowValue;
				declaration.US_7501IOR = true;
			}

			var wayBillNumber = WayBillNumber;
			if (!wayBillNumber.IsEmpty && IsConsumptionFTZ && !FTZNumber.HasValue)
			{
				declaration.US_FTZNo = wayBillNumber.Left(declaration.US_FTZNoInfo.MaxLength);
			}

			declaration.DefaultFDADateOnDeclarationLevel();
		}

		void SynchroniseFIRMSCode(JobDeclaration declaration)
		{
			if (declaration != null && declaration.US_US_NKLocationOfGoods.IsEmpty && declaration.IsImport)
			{
				if (!declaration.DepotDocAddress.IsEmpty && declaration.DepotDocAddress.Organisation != null)
				{
					declaration.US_US_NKLocationOfGoods = declaration.DepotDocAddress.Organisation.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.FIRMSCode, Core.Constants.CountryCodes.UnitedStates);
				}
				else if (!declaration.ContainerTerminalOperatorDocAddress.IsEmpty && declaration.ContainerTerminalOperatorDocAddress.Organisation != null)
				{
					declaration.US_US_NKLocationOfGoods = declaration.ContainerTerminalOperatorDocAddress.Organisation.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.FIRMSCode, Core.Constants.CountryCodes.UnitedStates);
				}
				else if (!declaration.ContainerYardDocAddress.IsEmpty && declaration.ContainerYardDocAddress.Organisation != null)
				{
					declaration.US_US_NKLocationOfGoods = declaration.ContainerYardDocAddress.Organisation.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.FIRMSCode, Core.Constants.CountryCodes.UnitedStates);
				}
			}
		}

		bool IsEntryNumberUsed(ZString entryNumber, ZString entryFilterCode)
		{
			var existDeclration = factory.Load<JobDeclaration>(GetDeclarationQuery(entryNumber, CusEntryHeaderMessageTypeList.Codes.EntrySummary));
			return existDeclration.Any(x => x.US_EntryFilerCode == entryFilterCode);
		}

		bool IsFTZAdmissionNumberUsed()
		{
			return factory.Load<JobDeclaration>(GetDeclarationQuery(FTZAdmissionNumber, CusEntryHeaderMessageTypeList.Codes.ForeignTradeZone)).Length > 0;
		}

		protected override CommercialInvoiceHeaderDataObjectReader<JobComInvoiceGroupHeader> CreateNewCommercialInvoiceHeaderDataObjectReader(JobComInvoiceGroupHeader groupHeader, UniversalCustoms.CommercialInvoiceHeader invoiceData, Shipment dataObject, ILandedCostDataReader landedCostDataReader)
		{
			return new CommercialInvoiceHeaderDataObjectReader(invoiceData, logger, Helper, groupHeader, dataObject, landedCostDataReader);
		}

		protected override void FillOrganizationsCore(List<OrganizationAddress> organizationAddressCollection, JobDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			base.FillOrganizationsCore(organizationAddressCollection, declaration, delaySetters);

			var docAddressTypes = new Dictionary<string, DocAddressType>();

			docAddressTypes.Add(Constants.AddressType.CBPBroker, DocAddressType.CBPBroker);
			docAddressTypes.Add(Constants.AddressType.FDASubmitter, DocAddressType.FDASubmitter);
			ReadOrgAddress(declaration, delaySetters, docAddressTypes, organizationAddressCollection);

			var declarationRow = GetColumnIndexer(declaration);

			if (GetFPPIPK(declaration) is ZGuid fppiPK && fppiPK.IsValid)
			{
				var usDeclarationRow = GetColumnIndexer(declaration.USDeclaration);
				SetValue(usDeclarationRow, JobUSDeclarationSchema.USD_OH_ForeignPrincipalParty, fppiPK, delaySetters);
			}

			if (delaySetters == null)
			{
				Customs.Business.IAddInfoManager manager = declaration;

				var addInfo = manager.AddInfo;
				var addInfos = declarationRow.GetAddInfos(JobDeclarationSchema.JE_AddInfo);

				FillUSOrganization((SchemaGuidColumn column, Func<JobDeclaration, ZGuid?> getOrganisationPK) =>
				{
					var pkValue = getOrganisationPK(declaration);
					if (pkValue.HasValue)
					{
						var key = addInfo.GetKey(column.Name);
						addInfos.Update(key, pkValue.Value);
					}
				});

				SetValue(declaration, JobDeclaration.Schema.JE_OA_InvoicerAddress, GetInvoicerAddressPK(declaration));
				SetValue(declarationRow, JobDeclarationSchema.JE_AddInfo, AddInfoParser.Serialise(addInfos));
				SetValue(declarationRow, JobDeclarationSchema.JE_OH_NotifyParty, GetNotifyPartyPK(declaration));
			}
			else
			{
				FillUSOrganization((SchemaGuidColumn column, Func<JobDeclaration, ZGuid?> getOrganisationPK) =>
				{
					SetValueWithDelay(declaration, column, () => getOrganisationPK(declaration), delaySetters, JobDeclarationSchema.PK);
				});

				SetValueWithDelay(declaration, JobDeclaration.Schema.JE_OA_InvoicerAddress, () => GetInvoicerAddressPK(declaration), delaySetters);
				SetValueWithDelay(declaration, JobDeclarationSchema.JE_OH_NotifyParty, () => GetNotifyPartyPK(declaration), delaySetters, JobDeclarationSchema.PK);
			}
		}

		void ReadOrgAddress(JobDeclaration declaration, Dictionary<string, ValueSetter> delaySetters, Dictionary<string, DocAddressType> docAddressTypes, List<OrganizationAddress> organizationAddressCollection)
		{
			foreach (string docAddressType in docAddressTypes.Keys)
			{
				var orgAddress = organizationAddressCollection.FirstOrDefault(add => add.AddressType == (ZString?)docAddressType);
				if (orgAddress != null)
				{
					OrganisationDataObjectReader.MatchedOrNew(declaration, orgAddress, logger, factory, delaySetters, docAddressTypes[docAddressType]);
				}
			}
		}

		protected override void FillManufacturer(List<OrganizationAddress> orgAddresses, JobDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			if (JobMessageTypeList.IsImport(MessageTypeCode))
			{
				base.FillManufacturer(orgAddresses, declaration, delaySetters);
			}
		}

		protected override void FillSoldToPartyAddress(List<OrganizationAddress> orgAddresses, JobDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			if (JobMessageTypeList.IsImport(MessageTypeCode))
			{
				base.FillSoldToPartyAddress(orgAddresses, declaration, delaySetters);
			}
		}

		protected override void FillSellingAgent(List<OrganizationAddress> orgAddresses, JobDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			if (JobMessageTypeList.IsImport(MessageTypeCode))
			{
				base.FillSellingAgent(orgAddresses, declaration, delaySetters);
			}
		}

		protected override void FillBuyingAgent(List<OrganizationAddress> orgAddresses, JobDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			if (JobMessageTypeList.IsImport(MessageTypeCode))
			{
				base.FillBuyingAgent(orgAddresses, declaration, delaySetters);
			}
		}

		protected override void FillConsigneeAddress(List<OrganizationAddress> orgAddresses, JobDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			if (JobMessageTypeList.IsImport(MessageTypeCode))
			{
				base.FillConsigneeAddress(orgAddresses, declaration, delaySetters);
			}
		}

		protected override void FillConsignee(List<OrganizationAddress> orgAddresses, JobDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			if (!JobMessageTypeList.IsImport(MessageTypeCode))
			{
				base.FillConsignee(orgAddresses, declaration, delaySetters);
			}
		}

		protected override void FillDeclarant(List<OrganizationAddress> organizationAddressCollection, JobDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			if (JobMessageTypeList.IsImport(MessageTypeCode))
			{
				var declarationRow = GetColumnIndexer(declaration);
				var addressPk = GetAddressPK(declaration, Constants.AddressType.ImporterOfRecord, OrganisationTypes.Consignee);
				SetValue(declarationRow, JobDeclarationSchema.JE_OA_DeclarantAddress, addressPk, delaySetters);
			}
		}

		protected override void FillEntryNumbers(JobDeclaration declaration)
		{
			base.FillEntryNumbers(declaration);

			var dic = declaration.GetAddInfos(JobDeclarationSchema.JE_AddInfo);
			var entryFilerCode = dic.GetValue(USAddInfoSchema.US_EntryFilerCode);
			if (!entryFilerCode.IsEmpty && declaration.ImportEntryNumber.IsEmpty && IsImportMessageMode)
			{
				var entryNumber = GetEntryNumberFromDataObject();
				if (!entryNumber.IsEmpty)
				{
					if (IsEntryNumberUsed(entryNumber, entryFilerCode))
					{
						logger.Log(LogType.Warning, Res.GetString("6259F79F-222C-4244-9D88-F77325E14557", "This entry number '{0}' will not be imported as it is already used.", entryNumber));
					}
					else
					{
						declaration.ImportEntryNumber = entryNumber;
					}
				}
			}
			else if (IsFTZWithAdmissionNumber && declaration.FTZControlNumber.IsEmpty)
			{
				if (IsFTZAdmissionNumberUsed())
				{
					logger.Log(LogType.Warning, Res.GetString("69CC56AD-69DB-4ADA-A7D4-5A532AC70131", "This FTZ Admission number '{0}' will not be imported as it is already used.", FTZAdmissionNumber));
				}
				else
				{
					declaration.FTZAdmissionNumber = FTZAdmissionNumber;
				}
			}
		}

		protected override Customs.DataTransfer.Universal.CustomsEntryHeaderDataObjectReader CreateCustomsEntryHeaderDataObjectReader(UniversalCustoms.EntryHeader entryHeaderDataObject, JobDeclaration declaration, List<ZString> matchingKeys = null)
		{
			return new CustomsEntryHeaderDataObjectReader(entryHeaderDataObject, logger, Helper, declaration, ZGuid.Empty, GetAllInvoiceLineMatchingKeysFromDataObject(dataObject, Helper));
		}

		bool CheckMessageTypeBeforePopulateDeclaration(JobDeclaration declaration)
		{
			var result = true;
			if (declaration.IsInDatabase && declaration.DeclarationMessagesHaveBeenSent())
			{
				var dataObject = shipmentDataObject ?? this.dataObject;
				var messageType = CalculateMessageCode(dataObject, declaration);
				result = messageType.IsEmpty || declaration.JE_MessageType == messageType;
			}
			return result;
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(JobDeclaration declaration)
		{
			if (declaration != null && !CheckMessageTypeBeforePopulateDeclaration(declaration))
			{
				return Res.GetString("14AFB52E-2963-480B-9139-F0C2ADBEBBB7", "Cannot change declaration message type once messaging has started.");
			}
			if (isInvalidEntryNumber)
			{
				return Res.GetString("DE424438-5376-4CA1-AD58-FAEF13FAC88E", "The data import will not proceed because the entry number in this file is not valid.");
			}
			if (isMultipleDeclarationsMatchedWithEntryNumber)
			{
				return Res.GetString("6BDDE7F2-D89C-42BD-BE75-15649C857DD8", "A search by Entry Number has located multiple declarations. The data import will not proceed because the system is unable to identify a unique target for update based on the Entry Number Specified");
			}
			if (isMultipleDeclarationsMatchedWithFTZAdmissionNumber)
			{
				return Res.GetString("36924582-60E2-4B0B-A172-CBFCF9948E76", "A search by FTZ Admission Number has located multiple declarations. The data import will not proceed because the system is unable to identify a unique target for update based on the FTZ Admission Number Specified.");
			}
			if (!noDeclarationSatisfiesConditions.IsEmpty)
			{
				return noDeclarationSatisfiesConditions;
			}
			return base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(declaration);
		}

		protected override bool ShouldCreateDocAddress(JobDeclaration declaration, DocAddressType addressType)
		{
			return DocAddressesCreationHelper.ShouldBeCreated(declaration, addressType);
		}

		protected override List<ZString> GetAddressTypesHandleSeparately()
		{
			var result = base.GetAddressTypesHandleSeparately();
			result.Add(Customs.DataTransfer.Universal.Constants.AddressTypes.IntermediateConsignee);
			result.Add(nameof(DocAddressType.UltimateConsignee));
			result.Add(Constants.AddressType.ImporterOfRecord);
			result.Add(Constants.AddressType.Exporter);
			result.Add(nameof(DocAddressType.Manufacturer));
			result.Add(Constants.AddressType.Seller);
			result.Add(Customs.DataTransfer.Universal.Constants.AddressTypes.SellingAgent);
			result.Add(Constants.AddressType.Invoicer);
			result.Add(nameof(DocAddressType.BuyerDocumentaryAddress));
			result.Add(Customs.DataTransfer.Universal.Constants.AddressTypes.BuyingAgent);
			result.Add(Constants.AddressType.SoldToParty);
			result.Add(Constants.AddressType.Transferee);
			result.Add(Constants.AddressType.ForeignPrincipalPartyInInterest);
			return result;
		}

		void FillUSOrganization(Action<SchemaGuidColumn, Func<JobDeclaration, ZGuid?>> setValue)
		{
			// Drawback Data
			setValue(USAddInfoSchema.US_DRWTransferee, GetDRWTransfereePK);
		}

		ZGuid? GetFPPIPK(JobDeclaration declaration)
		{
			ZGuid? result = null;
			if (declaration.IsExport)
			{
				result = GetOrganisationPK(declaration, Constants.AddressType.ForeignPrincipalPartyInInterest, OrganisationTypes.None);
			}
			return result;
		}

		ZGuid? GetDRWTransfereePK(JobDeclaration declaration)
		{
			ZGuid? result = null;
			if (declaration.IsDrawback)
			{
				result = GetOrganisationPK(declaration, Constants.AddressType.Transferee, OrganisationTypes.Services);
			}
			return result;
		}

		ZGuid? GetNotifyPartyPK(JobDeclaration declaration)
		{
			ZGuid? result = null;
			if (declaration.IsImport || declaration.IsDrawback)
			{
				result = GetOrganisationPK(declaration, nameof(DocAddressType.NotifyParty), OrganisationTypes.None);
			}
			return result;
		}

		ZGuid? GetInvoicerAddressPK(JobDeclaration declaration)
		{
			ZGuid? result = null;
			if (declaration.IsImport)
			{
				result = GetAddressPK(declaration, Constants.AddressType.Invoicer, OrganisationTypes.Consignor);
			}
			return result;
		}

		ZGuid? GetOrganisationPK(JobDeclaration declaration, ZString addressType, OrganisationTypes orgCategory)
		{
			ZGuid? result = null;
			ZGuid organisationPK;
			ZGuid addressPK;
			if (this.TryGetMatchedOrganisationData(out organisationPK, out addressPK, dataObject, declaration, addressType, orgCategory))
			{
				result = organisationPK;
			}
			return result;
		}

		ZGuid? GetAddressPK(JobDeclaration declaration, ZString addressType, OrganisationTypes orgCategory)
		{
			ZGuid? result = null;
			ZGuid organisationPK;
			ZGuid addressPK;
			if (this.TryGetMatchedOrganisationData(out organisationPK, out addressPK, dataObject, declaration, addressType, orgCategory))
			{
				result = addressPK;
			}
			return result;
		}

		protected override AddInfoDataObjectReader GetNewAddInfoDataObjectReaderForDeclaration(JobDeclaration declaration)
		{
			return new AddInfoDataObjectReader<JobDeclaration>(logger, Helper, JobDeclarationSchema.JE_AddInfo, USAddInfoSchema.Instance, GetAddInfosThatShouldNotBeImported(declaration));
		}

		string[] GetAddInfosThatShouldNotBeImported(JobDeclaration declaration)
		{
			string[] result = null;
			if (declaration.IsImport && declaration.DeclarationMessagesHaveBeenSent())
			{
				result = new string[]
				{
					JobDeclaration.Schema.US_CheckNo.Substring(3),
					JobDeclaration.Schema.US_PaymentDueDate.Substring(3),
					JobDeclaration.Schema.US_PaymentDate.Substring(3),
					JobDeclaration.Schema.US_IsAIIRequested.Substring(3)
				};
			}
			return result;
		}

		protected override AdditionalBillDataObjectReader<Bill> GetNewAdditionalBillDataObjectReader(AdditionalBill additionalBillDataObject, AdditionalBillDataProvider<Bill> additionalBillDataProvider, BillDetail primaryMasterBillDetail, BillDetail primaryHouseBillDetail)
		{
			return new AdditionalBillDataObjectReader(additionalBillDataObject, logger, Helper, additionalBillDataProvider, primaryMasterBillDetail, primaryHouseBillDetail);
		}

		protected override CustomsContainerDataObjectReader<JobDeclaration, CusContainer> GetNewCustomsContainerDataObjectReader(Container containerDataObject, JobDeclaration declaration, ILandedCostDataReader landedCostDataReader)
		{
			return new CustomsContainerDataObjectReader<JobDeclaration, CusContainer>(containerDataObject, logger, Helper, declaration, landedCostDataReader);
		}

		protected override void PopulateNoOfPacksAndPackType(JobDeclaration declaration, Shipment dataObject, Dictionary<string, ValueSetter> delaySetters)
		{
			var localDataObject = dataObject;
			var declarationRow = GetColumnIndexer(declaration);
			SetValueWithDelay(declarationRow, JobDeclarationSchema.JE_TotalNoOfPacks, () => ShouldUseInnerPacks(declaration, localDataObject) ? localDataObject.TotalNoOfPacks : localDataObject.OuterPacks, delaySetters);
			SetValueWithDelay(declarationRow, JobDeclarationSchema.JE_TotalNoOfPacksPackType, () => Helper.GetCustomsUnitForPackType(ShouldUseInnerPacks(declaration, localDataObject) ? localDataObject.TotalNoOfPacksPackageType : localDataObject.OuterPacksPackageType), delaySetters);
			SetValue(declarationRow, JobDeclarationSchema.JE_TotalNoOfPacksDecimal, localDataObject.TotalNoOfPacksDecimal, delaySetters);
		}

		bool ShouldUseInnerPacks(JobDeclaration declaration, Shipment dataObject)
		{
			return declaration.IsImport && dataObject.TotalNoOfPacks.HasValue && dataObject.TotalNoOfPacks.Value > ZInt.Zero;
		}

		protected override void FillCountrySpecificDetails(JobDeclaration declaration, Shipment dataObject, Dictionary<string, ValueSetter> delaySetters)
		{
			base.FillCountrySpecificDetails(declaration, dataObject, delaySetters);

			var transportMode = dataObject.TransportMode != null ? dataObject.TransportMode.Code : null;
			if (!transportMode.HasValue && declaration.IsInDatabase)
			{
				transportMode = declaration.JE_TransportMode;
			}
			var transportReferenceKey = USAddInfoSchema.US_TransportReference.Name.Substring(3);
			if (MessageTypeCode == JobMessageTypeList.Codes.Export
				&& dataObject.CFSReference.HasValue
				&& transportMode.GetValueOrDefault().EqualsIgnoringCase(Core.Constants.TransportModes.Sea)
				&& (dataObject.AddInfoCollection == null || !dataObject.AddInfoCollection.GetZStringValue(transportReferenceKey).HasValue)
				&& !declaration.ShouldSynchroniseWithShipment())
			{
				var declarationRow = GetColumnIndexer(declaration);
				if (delaySetters == null)
				{
					var addInfos = declarationRow.GetAddInfos(JobDeclarationSchema.JE_AddInfo);
					addInfos.Update(transportReferenceKey, dataObject.CFSReference.Value);
					SetValue(declarationRow, JobDeclarationSchema.JE_AddInfo, AddInfoParser.Serialise(addInfos));
				}
				else
				{
					SetValue(declarationRow, USAddInfoSchema.US_TransportReference, dataObject.CFSReference, delaySetters, JobDeclarationSchema.PK);
				}
			}
		}

		protected override IEnumerable<ZString> GetSettingOrder(JobDeclaration declaration)
		{
			return SettingOrderDeterminer.GetSettingOrder(declaration);
		}

		protected override BillDetail GetPrimaryMasterBillCore(Shipment dataObject)
		{
			BillDetail result = null;
			if (dataObject != null)
			{
				var billType = dataObject.WayBillType.GetCodeAsUpperCase();
				if (billType == WayBillTypeList.Codes.Master)
				{
					result = CreateBillDetailFrom(dataObject);
				}
				else if (billType == WayBillTypeList.Codes.House)
				{
					if (dataObject.AdditionalBillCollection != null)
					{
						var masterWayBillNumber = dataObject.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.Declaration.MasterWayBillNumber);
						if (masterWayBillNumber.HasValue)
						{
							result = new BillDetail() { BillNumber = masterWayBillNumber };
						}
						var masterWayBillIssuerSCAC = dataObject.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.Declaration.MasterWayBillIssuerSCAC);
						if (masterWayBillIssuerSCAC.HasValue)
						{
							result = result ?? new BillDetail();
							result.AddInfoCollection = new List<UniversalDataBuss.DataObjects.Universal.AddInfo>(new[] { Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo.New(Constants.AddInfoKeys.Declaration.WayBillIssuerSCAC, masterWayBillIssuerSCAC.Value) });
						}
					}
					if (result == null && dataObject.WayBillNumber.HasValue)
					{
						var wayBillIssuerSCAC = dataObject.AddInfoCollection == null ? null : dataObject.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.Declaration.WayBillIssuerSCAC);
						var billNumber = dataObject.WayBillNumber.Value;
						var additionalBill = dataObject.AdditionalBillCollection.GetAdditionalBill(billNumber, billType, (x) =>
							{
								var issuerSCACKey = Bill.Schema.US_UI_NKBillIssuerSCAC.Substring(3);
								return !wayBillIssuerSCAC.HasValue || (x.AddInfoCollection != null && x.AddInfoCollection.GetZStringValue(issuerSCACKey).GetValueOrDefault() == wayBillIssuerSCAC.Value);
							});
						if (additionalBill != null)
						{
							result = new BillDetail() { BillNumber = additionalBill.ParentBillNumber };
							var parentBillIssuerSCAC = additionalBill.AddInfoCollection == null ? null : additionalBill.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.AdditionalBill.ParentBillIssuerSCAC);
							if (parentBillIssuerSCAC.HasValue)
							{
								result.AddInfoCollection = new List<UniversalDataBuss.DataObjects.Universal.AddInfo>(new[] { Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo.New(Constants.AddInfoKeys.Declaration.WayBillIssuerSCAC, parentBillIssuerSCAC.Value) });
							}
						}
					}
				}
			}
			return result;
		}

		protected override DataRow FindFirstCusDecHouseBillByBillNumberAndType(ZGuid declarationPK, ZString billNumber, ZString billType, List<UniversalDataBuss.DataObjects.Universal.AddInfo> addInfoCollection)
		{
			var result = base.FindFirstCusDecHouseBillByBillNumberAndType(declarationPK, billNumber, billType, addInfoCollection);

			if (result == null && (billNumber.Length == 16 || StartWithSCAC(billNumber, billType, addInfoCollection)))
			{
				result = base.FindFirstCusDecHouseBillByBillNumberAndType(declarationPK, billNumber.SubstringSafe(4), billType, addInfoCollection);
			}
			return result;
		}

		bool StartWithSCAC(ZString billNumber, ZString billType, List<UniversalDataBuss.DataObjects.Universal.AddInfo> addInfoCollection)
		{
			var result = false;
			if (billNumber.Length > 4 && addInfoCollection != null)
			{
				var scacKey = ZString.Empty;
				switch (billType)
				{
					case Enterprise.Customs.Business.BillTypeList.Codes.HouseBill:
						scacKey = Constants.AddInfoKeys.Declaration.WayBillIssuerSCAC;
						break;
					case Enterprise.Customs.Business.BillTypeList.Codes.MasterBill:
						scacKey = Constants.AddInfoKeys.Declaration.MasterWayBillIssuerSCAC;
						break;
				}
				var scac = addInfoCollection.GetZStringValue(scacKey).GetValueOrDefault();
				result = !scac.IsEmpty && scac.Length == 4 && billNumber.StartsWith(scac, StringComparison.CurrentCulture);
			}
			return result;
		}

		protected override BillDetail GetPrimaryHouseBillDetailCore(Shipment dataObject)
		{
			BillDetail result = null;
			if (dataObject != null)
			{
				var billType = dataObject.WayBillType.GetCodeAsUpperCase();
				if (billType == WayBillTypeList.Codes.House)
				{
					result = CreateBillDetailFrom(dataObject);
				}
			}
			return result;
		}

		BillDetail CreateBillDetailFrom(Shipment shipmentDataObject)
		{
			var result = new BillDetail() { BillNumber = shipmentDataObject.WayBillNumber };
			var wayBillIssuerSCACAddInfo = shipmentDataObject.AddInfoCollection == null ? null : shipmentDataObject.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoKeys.Declaration.WayBillIssuerSCAC);
			if (wayBillIssuerSCACAddInfo != null)
			{
				result.AddInfoCollection = new List<UniversalDataBuss.DataObjects.Universal.AddInfo>(new[] { wayBillIssuerSCACAddInfo });
				var wayBillIssuerSCAC = wayBillIssuerSCACAddInfo.Value.GetValueOrDefault();
				if (!wayBillIssuerSCAC.IsEmpty && dataObject.AdditionalBillCollection != null)
				{
					var wayBillNumber = shipmentDataObject.WayBillNumber.GetValueOrDefault();
					var scac = wayBillNumber.Left(wayBillIssuerSCAC.Length);
					if (scac == wayBillIssuerSCAC)
					{
						var issuerSCACKey = Bill.Schema.US_UI_NKBillIssuerSCAC.Substring(3);
						wayBillNumber = wayBillNumber.SubstringSafe(scac.Length);
						if (dataObject.AdditionalBillCollection.FirstOrDefault(x => x.AddInfoCollection != null && x.BillNumber.GetValueOrDefault() == wayBillNumber
							&& x.AddInfoCollection.GetZStringValue(issuerSCACKey).GetValueOrDefault() == scac) != null)
						{
							result.BillNumber = wayBillNumber;
						}
					}
				}
			}
			return result;
		}

		protected override JobDeclaration[] FilterUsingCountrySpecificBusinessRules(JobDeclaration[] declarations, BillDetail masterBillDetail)
		{
			var result = declarations;

			if (declarations == null || declarations.Length == 0)
			{
				return result;
			}

			if (IsFTZWithAdmissionNumber)
			{
				result = declarations.Where(declaration => declaration.FTZControlNumber.IsEmpty).ToArray();
			}

			var wayBillIssuerSCACAddInfo = masterBillDetail.AddInfoCollection?.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.AddInfoKeys.Declaration.WayBillIssuerSCAC);
			if (wayBillIssuerSCACAddInfo != null && wayBillIssuerSCACAddInfo.Value.HasValue && !wayBillIssuerSCACAddInfo.Value.Value.IsEmpty)
			{
				result = result.Where(d => d.MasterBillIssuerSCACCode.Equals(wayBillIssuerSCACAddInfo.Value)).ToArray();
			}

			return result;
		}

		protected override bool CanPopulateDeclaration(JobDeclaration declaration)
		{
			return string.IsNullOrEmpty(declaration.GetReasonForNotAbleToUpdate()) && !declaration.HasWHSTransaction && CheckMessageTypeBeforePopulateDeclaration(declaration);
		}

		protected override bool CanMatchByCountrySpecificBusinessRules
		{
			get
			{
				var entryNumber = GetEntryNumberFromDataObject();
				var entryFilerCode = GetEntryFilerCodeFromDataObject();
				var entryType = GetEntryTypeFromDataObject();
				return !entryNumber.IsEmpty && !entryFilerCode.IsEmpty && entryType == EntryTypeList.Codes.ConsumptionFTZ && IsImportMessageMode || IsFTZWithAdmissionNumber;
			}
		}

		bool HasValidlengthOfEntryNumber(ZString entryNum)
		{
			if (!entryNum.IsEmpty && entryNum.Length > MQEDIMessage.USEntryNumberPlaceHolder.Length)
			{
				return false;
			}

			return true;
		}

		bool IsImportMessageMode
		{
			get
			{
				return dataObject != null && dataObject.MessageType != null &&
				(dataObject.MessageType.Code.ToString() == JobMessageTypeList.Codes.Import || dataObject.MessageType.Code.ToString() == JobMessageTypeList.Codes.ImportByExternalBroker);
			}
		}

		protected override JobDeclaration GetExistingBusinessObjectUsingCountrySpecificBusinessRules()
		{
			var entryNumber = GetEntryNumberFromDataObject();
			var entryFilerCode = GetEntryFilerCodeFromDataObject();
			var entryType = GetEntryTypeFromDataObject();

			if (IsFTZWithAdmissionNumber)
			{
				var query = GetDeclarationQuery(FTZAdmissionNumber, CusEntryHeaderMessageTypeList.Codes.ForeignTradeZone);
				var jobDeclarations = factory.Load<JobDeclaration>(query);
				if (jobDeclarations.Length > 0)
				{
					var activeDeclarations = jobDeclarations.Where(dec => !dec.JE_IsCancelled).ToArray();
					switch (activeDeclarations.Length)
					{
						case 0:
							noDeclarationSatisfiesConditions = Res.GetString("694AC7BC-AFB2-4160-94FC-681D13EDCEF5", "An FTZ Declaration with a matching FTZ Admission number exists, however this Declaration is canceled. It is for this reason that the data import will not proceed.");
							return null;
						case 1:
							return activeDeclarations[0];
						default:
							isMultipleDeclarationsMatchedWithFTZAdmissionNumber = true;
							return null;
					}
				}

				return GetExistingBusinessObjectUsingBills();
			}

			//matching by entry number should occur only if xml has an entry type 06
			if (!entryNumber.IsEmpty && !entryFilerCode.IsEmpty && entryType == EntryTypeList.Codes.ConsumptionFTZ)
			{
				var declarationQuery = GetDeclarationQuery(entryNumber, CusEntryHeaderMessageTypeList.Codes.EntrySummary);

				var loadedDeclarations = factory.Load<JobDeclaration>(declarationQuery);

				var matchedDeclarations = loadedDeclarations.Where(dec => dec.US_EntryFilerCode == entryFilerCode);
				if (matchedDeclarations.Any()) //if there is a match in our system on entry number and entry filer code
				{
					var importDeclarations = matchedDeclarations.Where(dec => dec.US_EntryType == entryType && (dec.JE_MessageType == JobMessageTypeList.Codes.Import || dec.JE_MessageType == JobMessageTypeList.Codes.ImportByExternalBroker));
					if (importDeclarations.Any())
					{
						var ftzNotCancelledDeclarations = importDeclarations.Where(dec => !dec.JE_IsCancelled);
						if (ftzNotCancelledDeclarations.Count() > 1)
						{
							isMultipleDeclarationsMatchedWithEntryNumber = true;
						}
						else if (!ftzNotCancelledDeclarations.Any())
						{
							noDeclarationSatisfiesConditions = Res.GetString("2C5F8781-8BE1-4C09-8271-6673DEF51544", "An Import Declaration with Entry Type 06 with a matching Entry Filer Code and Entry Number exists, however these Declarations are canceled. It is for this reason that the data import will not proceed.");
						}
						else if (ftzNotCancelledDeclarations.Count() == 1)
						{
							return ftzNotCancelledDeclarations.FirstOrDefault();
						}
					}
					else
					{
						noDeclarationSatisfiesConditions = Res.GetString("A1D2F92A-4016-4F31-98BE-09C364BB6B6E", "At least one Declaration with a matching Entry Filer Code and Entry Number exists, however this Declaration does not have Shipment Type = IMP/IMX or it is not a 06 entry. It is for this reason that the data import will not proceed.");
					}
				}
			}
			return null;
		}

		bool IsFTZWithAdmissionNumber
		{
			get
			{
				return dataObject.MessageType != null && dataObject.MessageType.GetCodeAsUpperCase() == USJobMessageTypeList.Codes.FTZ && !FTZAdmissionNumber.IsEmpty;
			}
		}

		ZString FTZAdmissionNumber
		{
			get
			{
				if (!fFTZAdmissionNumber.HasValue)
				{
					var entryNumber = dataObject.EntryNumberCollection?.FirstOrDefault(number => number.Type.GetCodeAsUpperCase() == OrgConstants.NumberFountains.Code.FTZAdmissionControlNumber);
					fFTZAdmissionNumber = entryNumber?.Number ?? ZString.Empty;
				}
				return fFTZAdmissionNumber.Value;
			}
		}
		ZString? fFTZAdmissionNumber;

		ZString WayBillNumber
		{
			get
			{
				return dataObject.WayBillType.GetCodeAsUpperCase() == WayBillTypeList.Codes.Master ? (dataObject.WayBillNumber ?? ZString.Empty) : ZString.Empty;
			}
		}

		ZString? FTZNumber
		{
			get
			{
				return dataObject.AddInfoCollection?.FirstOrDefault(x => x.Key.GetValueOrDefault() == JobDeclaration.Schema.US_FTZNo.Substring(3))?.Value;
			}
		}

		ZBool IsConsumptionFTZ
		{
			get
			{
				return JobMessageTypeList.IsImport(MessageTypeCode) && GetEntryTypeFromDataObject() == EntryTypeList.Codes.ConsumptionFTZ;
			}
		}

		ZBool isMultipleDeclarationsMatchedWithEntryNumber;
		ZBool isInvalidEntryNumber;
		ZBool isMultipleDeclarationsMatchedWithFTZAdmissionNumber;
		ZString noDeclarationSatisfiesConditions;

		ZDBOnlyQuery GetDeclarationQuery(ZString entryNumber, ZString entryType)
		{
			var declarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));

			var declarationBranchQuery = new ZQuery(JobDeclarationSchema.JE_GB, GlbCompany.CurrentCompany.Branches.GetPKs());
			declarationQuery.AddToFilter(declarationBranchQuery);

			var entryNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.UnitedStates);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, entryNumber);
			declarationQuery.AddSubQuery(entryNumberQuery, JoinCondition.And);
			declarationQuery.IgnoreActiveFilter = true;
			return declarationQuery;
		}

		ZString GetEntryNumberFromDataObject()
		{
			if (!entryNumberCached.HasValue)
			{
				var entryNumber = ZString.Empty;
				if (dataObject != null && dataObject.EntryNumberCollection != null)
				{
					var entryNumberObj = dataObject.EntryNumberCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == CusEntryHeaderMessageTypeList.Codes.EntrySummary);
					entryNumber = entryNumberObj != null ? entryNumberObj.Number.Value : ZString.Empty;
				}
				isInvalidEntryNumber = !HasValidlengthOfEntryNumber(entryNumber);
				entryNumberCached = entryNumber;
			}
			return entryNumberCached.Value;
		}
		ZString? entryNumberCached;

		ZString GetEntryFilerCodeFromDataObject()
		{
			if (!entryFilerCodeCached.HasValue)
			{
				entryFilerCodeCached = dataObject?.AddInfoCollection?.FirstOrDefault(x => x.Key.GetValueOrDefault() == USAddInfoSchema.US_EntryFilerCode.Name.Substring(3))?.Value.GetValueOrDefault() ?? ZString.Empty;
			}
			return entryFilerCodeCached.Value;
		}
		ZString? entryFilerCodeCached;

		ZString GetEntryTypeFromDataObject()
		{
			if (!entryTypeCached.HasValue)
			{
				entryTypeCached = dataObject?.AddInfoCollection?.FirstOrDefault(x => x.Key.GetValueOrDefault() == JobDeclaration.Schema.US_EntryType.Substring(3))?.Value.GetValueOrDefault() ?? ZString.Empty;
			}
			return entryTypeCached.Value;
		}
		ZString? entryTypeCached;

		protected override JobDeclaration GetDeclarationFromShipment()
		{
			if (CanMatchByCountrySpecificBusinessRules)
			{
				var declaration = GetExistingBusinessObjectUsingCountrySpecificBusinessRules();
				if (declaration != null && declaration.JE_JS != Shipment.PK)
				{
					noDeclarationSatisfiesConditions = Res.GetString("52C39421-FA57-4D13-8C7C-FF3300A6CB5D", "At least one Import Declaration with a matching Entry Filer Code and Entry Number exists. It is for this reason that the data import will not proceed.");
					return null;
				}
				return declaration;
			}
			else
			{
				return base.GetDeclarationFromShipment();
			}
		}

		protected override void AddFetchHintsRelatedToCommerialInvoiceLineTariff(JobDeclaration declaration, List<ZString> harmonisedCodes)
		{
			base.AddFetchHintsRelatedToCommerialInvoiceLineTariff(declaration, harmonisedCodes);
			var tariffCodes = harmonisedCodes.Select(code => code.Replace(".", ""));
			if (declaration.UseScheduleB)
			{
				factory.BOFactory.AddFetchHint(TariffViewSchema.Instance,
					GetQueryForExportFetch(tariffCodes, Customs.Universal.Constants.TariffTypes.ScheduleB));
			}
			else
			{
				if (declaration.IsExport)
				{
					factory.BOFactory.AddFetchHint(TariffViewSchema.Instance,
						GetQueryForExportFetch(tariffCodes, Customs.Universal.Constants.TariffTypes.Export));
				}
				else
				{
					foreach (var tariffCode in tariffCodes)
					{
						factory.BOFactory.AddFetchHint(USCTariffSchema.UE_Tariff, tariffCode);
						factory.BOFactory.AddFetchHint(USCTariffRuleSchema.Instance, USCTariffRule.Loader.GetTariffRange(tariffCode));
					}
					var tariffs = factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, tariffCodes));
					factory.BOFactory.AddFetchHint(USCTariffDutyRateSchema.Instance, new ZQuery(USCTariffDutyRateSchema.UD_UE, tariffs.Select(t => t.PK)));
				}
			}
		}
		ZQuery GetQueryForExportFetch(IEnumerable<ZString> tariffCodes, string tariffType)
		{
			var query = new ZQuery();
			var type = Customs.Universal.RefCusTariffType.Loader.Load(factory.BOFactory, Core.Constants.CountryCodes.UnitedStates, tariffType);
			if (type != null)
			{
				query.AddToFilter(TariffViewSchema.ZZ1_TariffCode, tariffCodes);
				query.AddToFilter(TariffViewSchema.ZZ1_ZZI_TariffType, type.PK);
			}
			else
			{
				query.IsNoResultQuery = true;
			}
			return query;
		}

		protected override IEnumerable<ZString> GetDeclarationPropertiesToSuspendSetting()
		{
			foreach (var propertyName in base.GetDeclarationPropertiesToSuspendSetting())
			{
				yield return propertyName;
			}
			var dataObject = shipmentDataObject ?? this.dataObject;
			isFromHVLV = dataObject.GetMatchingDataSource(DataContextType.HVLVConsignment) != null
				&& dataObject.ShipmentType.GetCodeAsUpperCase() == Core.Constants.ShipmentTypes.HighVolumeLowValue;

			if (!isFromHVLV)
			{
				yield return JobDeclaration.SuspendKey_DefaultEntrySummaryAndCargoReleaseForImport;
			}
		}

		bool isFromHVLV;

		protected override void PopulateApplicationCode(Shipment dataObject, JobDeclaration declaration, Dictionary<string, ValueSetter> delaySetters)
		{
			var applicationCode = dataObject.MessagingApplicationCode.GetCodeAsUpperCase();
			if (!applicationCode.IsEmpty)
			{
				var declarationRow = GetColumnIndexer(declaration);
				SetValue(declarationRow, JobDeclarationSchema.JE_ApplicationCode, applicationCode, delaySetters);
			}
		}
	}
}
