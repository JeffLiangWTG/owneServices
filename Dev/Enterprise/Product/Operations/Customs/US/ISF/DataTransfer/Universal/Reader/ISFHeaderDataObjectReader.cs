using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Customs.US.ISF.Business.ISFConstants;

namespace Enterprise.Customs.US.ISF.DataTransfer.Universal.Reader
{
	class ISFHeaderDataObjectReader : ShipmentDataObjectReader<CusISFHeader>
	{
		public ISFHeaderDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
		}

		protected override IMatchingBusinessEntityFinder<CusISFHeader> GetCombinedReferenceMatcher()
		{
			return null;
		}

		public override DataContextType DataContextType
		{
			get { return DataContextType.USImporterSecurityFiling; }
		}

		protected override CusISFHeader GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			AddWayBillToAdditionalBillCollectionIfNotExists();

			CusISFHeader header = null;

			EntryNumber transactionNumberObject = null;
			if (dataObject.EntryNumberCollection != null)
			{
				transactionNumberObject = dataObject.EntryNumberCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == Constants.EntryNumberConstants.ISF && x.CountryOfIssue.GetCodeAsUpperCase() == Core.Constants.CountryCodes.UnitedStates);
			}
			if (transactionNumberObject != null && !transactionNumberObject.Number.GetValueOrDefault().IsEmpty)
			{
				header = FindMatchingISFHeaderUsingTransactionNumber(transactionNumberObject.Number.GetValueOrDefault());
			}
			else if (dataObject.AddInfoCollection != null)
			{
				var orgAddress = ImporterAddress;
				if (orgAddress != null)
				{
					header = FindMatchingISFHeaderUsingImporterAndBills(orgAddress.OA_OH);
				}
			}

			return header;
		}
		ZString reasonForNotAbleToUpdate;

		OrgAddress ImporterAddress
		{
			get
			{
				if (!hasAttemptImporterMatch)
				{
					hasAttemptImporterMatch = true;
					var importer = dataObject.OrganizationAddressCollection.FindBestImporterMatch();
					if (importer != null)
					{
						var reader = new OrganisationDataObjectReader(importer, logger, factory);
						importerAddress = reader.GetMatched();
					}
				}
				return importerAddress;
			}
		}
		OrgAddress importerAddress;
		bool hasAttemptImporterMatch;

		void AddWayBillToAdditionalBillCollectionIfNotExists()
		{
			var wayBillNumber = dataObject.WayBillNumber.GetValueOrDefault();
			var billType = GetMappedBillTypeFromWayBillType(dataObject.WayBillType.GetCodeAsUpperCase());

			if (!wayBillNumber.IsEmpty && !billType.IsEmpty)
			{
				dataObject.SetAdditionalBillCollection(() =>
				{
					var billCollection = dataObject.AdditionalBillCollection ?? new List<AdditionalBill>();

					if (!billCollection.Any(bill => string.Compare(bill.BillType.GetCodeAsUpperCase(), billType, true) == 0 && string.Compare(bill.BillNumber.GetValueOrDefault(), wayBillNumber, true) == 0))
					{
						var additionBill = new AdditionalBill(DefaultDataObjectWriterStrategy.Instance)
						{
							BillType = new WayBillType() { Code = billType },
							BillNumber = wayBillNumber,
						};
						billCollection.Add(additionBill);
					}

					return billCollection.Count == 0 ? null : billCollection;
				});
			}
		}

		ZString GetMappedBillTypeFromWayBillType(ZString wayBillType)
		{
			switch (wayBillType)
			{
				case WayBillTypeList.Codes.Master:
					return BillTypeList.Codes.OceanBillOfLading;
				case WayBillTypeList.Codes.MasterHouse:
					return BillTypeList.Codes.MasterBillOfLading;
				case WayBillTypeList.Codes.House:
					return BillTypeList.Codes.HouseBillOfLading;
				default:
					return ZString.Empty;
			}
		}

		CusISFHeader FindMatchingISFHeaderUsingTransactionNumber(ZString transactionNumber)
		{
			var headers = factory.Load<CusISFHeader>(new ZQuery(CusISFHeaderSchema.BF_CustomsReference, transactionNumber));
			if (headers.Length > 1)
			{
				reasonForNotAbleToUpdate = MultipleISFMatchingTransactionNumber;
			}
			else if (headers.Length == 1)
			{
				return headers[0];
			}

			return null;
		}
		internal const string MultipleISFMatchingTransactionNumber = "Cannot import the existing data as there are multiple ISF Jobs matching the Transaction Number.";

		CusISFHeader FindMatchingISFHeaderUsingImporterAndBills(ZGuid importerPK)
		{
			var additionalBillCollectionHasValues = dataObject.AdditionalBillCollection != null && dataObject.AdditionalBillCollection.Count > 0;
			var ownerRef = dataObject.OwnerRef.GetValueOrDefault();
			if (additionalBillCollectionHasValues || !ownerRef.IsEmpty)
			{
				var importerQuery = new ZQuery(CusISFHeaderSchema.BF_OH_Importer, importerPK);

				bool? isMatchedOnBills = null;
				if (additionalBillCollectionHasValues)
				{
					var houseBillNumberList = new List<ZString>();
					var oceanBillNumberList = new List<ZString>();

					foreach (var billObject in dataObject.AdditionalBillCollection)
					{
						var billNumber = billObject.BillNumber.GetValueOrDefault();
						if (!billNumber.IsEmpty)
						{
							switch (billObject.BillType.GetCodeAsUpperCase())
							{
								case BillTypeList.Codes.HouseBillOfLading:
									houseBillNumberList.Add(billNumber);
									break;
								case BillTypeList.Codes.OceanBillOfLading:
									oceanBillNumberList.Add(billNumber);
									break;
							}
						}
					}

					if (houseBillNumberList.Count > 0 || oceanBillNumberList.Count > 0)
					{
						var headerQuery = new ZDBOnlyQuery(typeof(CusISFHeader));
						if (houseBillNumberList.Count > 0)
						{
							var houseBillSubQuery = new ZDBOnlySubQuery(typeof(CusISFBill), CusISFBillSchema.BB_BF);
							houseBillSubQuery.AddToFilter(CusISFBillSchema.BB_BillType, BillTypeList.Codes.HouseBillOfLading);
							houseBillSubQuery.AddToFilter(CusISFBillSchema.BB_BillNum, houseBillNumberList);
							headerQuery.AddSubQuery(houseBillSubQuery, JoinCondition.And);
						}
						if (oceanBillNumberList.Count > 0)
						{
							var oceanBillSubQuery = new ZDBOnlySubQuery(typeof(CusISFBill), CusISFBillSchema.BB_BF);
							oceanBillSubQuery.AddToFilter(CusISFBillSchema.BB_BillType, BillTypeList.Codes.OceanBillOfLading);
							oceanBillSubQuery.AddToFilter(CusISFBillSchema.BB_BillNum, oceanBillNumberList);
							headerQuery.AddSubQuery(oceanBillSubQuery, JoinCondition.And);
						}

						headerQuery.AddToFilter(GetBillDateFilter());
						headerQuery.AddToFilter(importerQuery);

						var headers = factory.Load<CusISFHeader>(headerQuery);
						isMatchedOnBills = headers.Length != 0;
						if (headers.Length > 1)
						{
							reasonForNotAbleToUpdate = MultipleISFMatchingEitherHouseBillOrOreanBill;
						}
						else if (headers.Length == 1)
						{
							return headers[0];
						}
					}
				}

				if ((!isMatchedOnBills.HasValue || !isMatchedOnBills.Value) && !ownerRef.IsEmpty)
				{
					var query = new ZQuery(GetBillDateFilter());
					query.AddToFilter(CusISFHeaderSchema.BF_OwnerReference, ownerRef);
					query.AddToFilter(importerQuery);

					var headers = factory.Load<CusISFHeader>(query);
					if (headers.Length > 1)
					{
						reasonForNotAbleToUpdate = MultipleISFMatchingOwnerReference;
					}
					else if (headers.Length == 1)
					{
						return headers[0];
					}
				}
			}

			return null;
		}
		internal const string MultipleISFMatchingEitherHouseBillOrOreanBill = "Cannot import the existing data as there are multiple ISF Job matching the Importer and either the House Bill of Lading or the Ocean Bill of Lading.";
		internal const string MultipleISFMatchingOwnerReference = "Cannot import the existing data as there are multiple ISF Job matching Importer and the Owner Reference.";

		ZQuery GetBillDateFilter()
		{
			ZQuery dateMatchQuery = new ZQuery(CusISFHeaderSchema.BF_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Now.AddDays(NoOfDaysOldAllowedInMatchingBill));
			dateMatchQuery.AddToFilter(JoinCondition.Or, CusISFHeaderSchema.BF_SystemCreateTimeUtc, ZDateTime.Empty);
			return dateMatchQuery;
		}
		public const int NoOfDaysOldAllowedInMatchingBill = -360;

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(CusISFHeader targetBO)
		{
			if (!reasonForNotAbleToUpdate.IsEmpty)
			{
				return reasonForNotAbleToUpdate;
			}
			return base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO);
		}

		protected override void PopulateBusinessObject(CusISFHeader targetBO)
		{
			var helper = new ISFDataObjectHelper(targetBO);
			var headerRow = GetColumnIndexer(targetBO);
			var delaySetters = IsDefaultingEnabled ? new Dictionary<string, ValueSetter>() : null;
			SetValue(headerRow, CusISFHeaderSchema.BF_TransportMode, GetTransportMode(), delaySetters);

			ZGuid branchPK = dataObject.GetBranchPK(factory.BOFactory);
			if (branchPK.IsValid)
			{
				SetValue(headerRow, CusISFHeaderSchema.BF_GB, branchPK, delaySetters);
			}
			SetValue(headerRow, CusISFHeaderSchema.BF_OwnerReference, dataObject.OwnerRef, delaySetters);
			SetValue(headerRow, CusISFHeaderSchema.BF_RL_NKPortOfUnload, dataObject.PortOfDischarge, delaySetters);
			SetValue(headerRow, CusISFHeaderSchema.BF_RL_NKPlaceOfDelivery, dataObject.PortOfDestination, delaySetters);
			SetValue(headerRow, CusISFHeaderSchema.BF_EstimatedValue, dataObject.GoodsValue, delaySetters);
			SetValue(headerRow, CusISFHeaderSchema.BF_EstimatedQuantity, dataObject.TotalNoOfPacks, delaySetters);
			SetValue(headerRow, CusISFHeaderSchema.BF_EstimatedQuantityUQ, dataObject.TotalNoOfPacksPackageType, delaySetters);
			if (dataObject.TotalWeight != null && dataObject.TotalWeight.HasValue)
			{
				SetValue(headerRow, CusISFHeaderSchema.BF_EstimatedWeight, dataObject.TotalWeight.Value.ToZInt(), delaySetters);
			}
			SetValue(headerRow, CusISFHeaderSchema.BF_EstimatedWeightUQ, dataObject.TotalWeightUnit, delaySetters);

			FillAddInfos(targetBO, delaySetters);
			FillOrganizationAddresses(targetBO);
			FillCustomizedFields(targetBO);
			FillAdditionalBills(targetBO);
			FillISFLines(targetBO, helper);
			FillEquips(targetBO);
			FillTransportLegs(targetBO);
			FillNotes(targetBO);

			delaySetters.SetValueInSpecificOrder(GetHeaderSettingOrder(targetBO));
		}

		IEnumerable<ZString> GetHeaderSettingOrder(CusISFHeader header)
		{
			yield return ColumnValueSetter.GetKey(header.PK, CusISFHeaderSchema.BF_EntryType);
			yield return ColumnValueSetter.GetKey(header.PK, CusISFHeaderSchema.BF_ShipmentType);
			yield return ColumnValueSetter.GetKey(header.PK, CusISFHeaderSchema.BF_TransportMode);
			yield return ColumnValueSetter.GetKey(header.PK, CusISFHeaderSchema.BF_GB);
			yield return ColumnValueSetter.GetKey(header.PK, CusISFHeaderSchema.BF_SCAC);
			yield return ColumnValueSetter.GetKey(header.PK, CusISFHeaderSchema.BF_OH_Importer);
			yield return ColumnValueSetter.GetKey(header.PK, CusISFHeaderSchema.BF_OwnerReference);
			yield return ColumnValueSetter.GetKey(header.PK, CusISFHeaderSchema.BF_ActionReasonCode);
			yield return ColumnValueSetter.GetKey(header.PK, CusISFHeaderSchema.BF_ImporterCodeType);
			yield return ColumnValueSetter.GetKey(header.PK, CusISFHeaderSchema.BF_ImporterCode);
			yield return ColumnValueSetter.GetKey(header.PK, CusISFHeaderSchema.BF_DateOfBirth);
			yield return ColumnValueSetter.GetKey(header.PK, CusISFHeaderSchema.BF_CountryOfIssue);
			yield return ColumnValueSetter.GetKey(header.PK, CusISFHeaderSchema.BF_ConsigneeCodeType);
			yield return ColumnValueSetter.GetKey(header.PK, CusISFHeaderSchema.BF_ConsigneeCode);
			yield return ColumnValueSetter.GetKey(header.PK, CusISFHeaderSchema.BF_ConsigneeFullName);
			yield return ColumnValueSetter.GetKey(header.PK, CusISFHeaderSchema.BF_ConsigneeDateOfBirth);
			yield return ColumnValueSetter.GetKey(header.PK, CusISFHeaderSchema.BF_ConsigneeCountryOfIssue);
			yield return ColumnValueSetter.GetKey(header.PK, CusISFHeaderSchema.BF_BondNumberOrHolder);
			yield return ColumnValueSetter.GetKey(header.PK, CusISFHeaderSchema.BF_BondActivityCode);
			yield return ColumnValueSetter.GetKey(header.PK, CusISFHeaderSchema.BF_BondType);
			yield return ColumnValueSetter.GetKey(header.PK, CusISFHeaderSchema.BF_RL_NKPortOfUnload);
			yield return ColumnValueSetter.GetKey(header.PK, CusISFHeaderSchema.BF_RL_NKPlaceOfDelivery);
			yield return ColumnValueSetter.GetKey(header.PK, CusISFHeaderSchema.BF_SendEquipment);
			yield return ColumnValueSetter.GetKey(header.PK, CusISFHeaderSchema.BF_ShipmentSubType);
			yield return ColumnValueSetter.GetKey(header.PK, CusISFHeaderSchema.BF_EstimatedValue);
			yield return ColumnValueSetter.GetKey(header.PK, CusISFHeaderSchema.BF_EstimatedQuantity);
			yield return ColumnValueSetter.GetKey(header.PK, CusISFHeaderSchema.BF_EstimatedQuantityUQ);
			yield return ColumnValueSetter.GetKey(header.PK, CusISFHeaderSchema.BF_EstimatedWeight);
			yield return ColumnValueSetter.GetKey(header.PK, CusISFHeaderSchema.BF_EstimatedWeightUQ);
		}

		ZString? GetTransportMode()
		{
			ZString? result = null;

			if (dataObject.TransportMode != null && dataObject.TransportMode.GetCodeAsUpperCase() == TransportTypeList.Codes.Sea && dataObject.CustomsContainerMode != null)
			{
				var isContainerized = dataObject.CustomsContainerMode.GetCodeAsUpperCase() == ContainerModeList.Codes.Containerized;
				result = isContainerized ? TransportModeCodes.Codes.OceanVesselContainerized : TransportModeCodes.Codes.OceanVesselNonContainerized;
			}

			return result;
		}

		void FillAddInfos(IColumnIndexer targetBO, Dictionary<string, ValueSetter> delaySetters)
		{
			if (dataObject.AddInfoCollection != null && dataObject.AddInfoCollection.Count > 0)
			{
				SetValue(targetBO, CusISFHeaderSchema.BF_EntryType, dataObject.AddInfoCollection.GetZStringValue(Constants.AddInfoConstants.EntryType), delaySetters);
				SetValue(targetBO, CusISFHeaderSchema.BF_ShipmentType, dataObject.AddInfoCollection.GetZStringValue(Constants.AddInfoConstants.ISFShipmentType), delaySetters);
				SetValue(targetBO, CusISFHeaderSchema.BF_SCAC, dataObject.AddInfoCollection.GetZStringValue(Constants.AddInfoConstants.CarrierSCAC), delaySetters);
				SetValue(targetBO, CusISFHeaderSchema.BF_ActionReasonCode, dataObject.AddInfoCollection.GetZStringValue(Constants.AddInfoConstants.ActionReason), delaySetters);
				SetValue(targetBO, CusISFHeaderSchema.BF_ImporterCodeType, dataObject.AddInfoCollection.GetZStringValue(Constants.AddInfoConstants.ImporterIDType), delaySetters);
				SetValue(targetBO, CusISFHeaderSchema.BF_ImporterCode, dataObject.AddInfoCollection.GetZStringValue(Constants.AddInfoConstants.ImporterID), delaySetters);
				SetValue(targetBO, CusISFHeaderSchema.BF_DateOfBirth, dataObject.AddInfoCollection.GetZDateTimeValue(Constants.AddInfoConstants.ImporterDOB), delaySetters);
				SetValue(targetBO, CusISFHeaderSchema.BF_CountryOfIssue, dataObject.AddInfoCollection.GetZStringValue(Constants.AddInfoConstants.ImporterIssueCountry), delaySetters);
				SetValue(targetBO, CusISFHeaderSchema.BF_ConsigneeCodeType, dataObject.AddInfoCollection.GetZStringValue(Constants.AddInfoConstants.ConsigneeIDType), delaySetters);
				SetValue(targetBO, CusISFHeaderSchema.BF_ConsigneeCode, dataObject.AddInfoCollection.GetZStringValue(Constants.AddInfoConstants.ConsigneeID), delaySetters);
				SetValue(targetBO, CusISFHeaderSchema.BF_ConsigneeFullName, dataObject.AddInfoCollection.GetZStringValue(Constants.AddInfoConstants.ConsigneeName), delaySetters);
				SetValue(targetBO, CusISFHeaderSchema.BF_ConsigneeDateOfBirth, dataObject.AddInfoCollection.GetZDateTimeValue(Constants.AddInfoConstants.ConsigneeDOB), delaySetters);
				SetValue(targetBO, CusISFHeaderSchema.BF_ConsigneeCountryOfIssue, dataObject.AddInfoCollection.GetZStringValue(Constants.AddInfoConstants.ConsigneeIssueCountry), delaySetters);
				SetValue(targetBO, CusISFHeaderSchema.BF_BondNumberOrHolder, dataObject.AddInfoCollection.GetZStringValue(Constants.AddInfoConstants.ISFBondHolder), delaySetters);
				SetValue(targetBO, CusISFHeaderSchema.BF_BondActivityCode, dataObject.AddInfoCollection.GetZStringValue(Constants.AddInfoConstants.ISFBondActivityCode), delaySetters);
				SetValue(targetBO, CusISFHeaderSchema.BF_BondType, dataObject.AddInfoCollection.GetZStringValue(Constants.AddInfoConstants.ISFBondType), delaySetters);
				SetValue(targetBO, CusISFHeaderSchema.BF_ShipmentSubType, dataObject.AddInfoCollection.GetZStringValue(Constants.AddInfoConstants.ISFShipmentSubType), delaySetters);
				SetValue(targetBO, CusISFHeaderSchema.BF_SendEquipment, dataObject.AddInfoCollection.GetZStringValue(Constants.AddInfoConstants.SendEquipment), delaySetters);
			}
		}

		void FillOrganizationAddresses(CusISFHeader targetBO)
		{
			if (dataObject.OrganizationAddressCollection != null)
			{
				FillImporterDocumentaryAddress(targetBO);
				var supportedAddressTypes = ((IDocAddresses)targetBO).SupportedAddressTypes.Where(x => x != DocAddressType.Manufacturer).ToList();
				var sequenceDictionary = new Dictionary<DocAddressType, int>();
				var existingDocAddress = new List<ISFDocAddress>(factory.Load<ISFDocAddress>(new ZQuery(JobDocAddressSchema.E2_ParentID, targetBO.PK)));
				var processedAddressTypes = new List<DocAddressType>();
				foreach (var orgAddressDataObject in dataObject.OrganizationAddressCollection)
				{
					var addressType = orgAddressDataObject.AddressType.GetValueOrDefault();
					DocAddressType docAddressType;
					if (Enum.TryParse(addressType, false, out docAddressType) && supportedAddressTypes.Contains(docAddressType))
					{
						if (!processedAddressTypes.Contains(docAddressType))
						{
							processedAddressTypes.Add(docAddressType);
						}
						int sequence;
						if (!sequenceDictionary.TryGetValue(docAddressType, out sequence))
						{
							sequence = -1;
							sequenceDictionary.Add(docAddressType, 0);
						}
						sequenceDictionary[docAddressType] = ++sequence;
						var docAddress = FillOrganizationAddress(targetBO, docAddressType, orgAddressDataObject, (ZByte)sequence);
						existingDocAddress.Remove(docAddress);
					}
				}
				existingDocAddress.Where(x => processedAddressTypes.Contains(x.DocAddressType)).DeleteAll();
			}
		}

		ZString GetDocAddressTypeCode(DocAddressType docAddressType)
		{
			return DocAddressTypes.GetCode(factory.BOFactory, docAddressType);
		}

		internal const string ManufacturerNumberExceedsMaximum = "Cannot import ISF data because Universal Shipment contains over 256 distinct Manufacturer addresses.";

		internal ISFDocAddress FillManufacturerOrganization(CusISFHeader targetBO, OrganizationAddress manufacturerAddress, int countOfAddresses)
		{
			ZByte sequence;
			if (countOfAddresses < 256)
			{
				sequence = ZByte.ParseSafe(countOfAddresses.ToString(), ZByte.Zero);
			}
			else
			{
				throw new MessageProcessingBusinessFailureException(ManufacturerNumberExceedsMaximum);
			}

			return FillOrganizationAddress(targetBO, DocAddressType.Manufacturer, manufacturerAddress, sequence);
		}

		ISFDocAddress FillOrganizationAddress(CusISFHeader targetBO, DocAddressType docAddressType, OrganizationAddress orgAddressDataObject, ZByte sequence)
		{
			var addressType = GetDocAddressTypeCode(docAddressType);
			var query = new ZQuery(JobDocAddressSchema.E2_AddressType, addressType);
			query.AddToFilter(JobDocAddressSchema.E2_AddressSequence, sequence);
			query.AddToFilter(JobDocAddressSchema.E2_ParentID, targetBO.PK);
			query.FetchOnlyFromLocalCache = true;
			var docAddress = factory.LoadTop1<ISFDocAddress>(query);
			if (docAddress == null)
			{
				docAddress = factory.New<ISFDocAddress>();
				docAddress.E2_ParentID = targetBO.PK;
				docAddress.E2_ParentTableCode = CusISFHeaderSchema.Constants.Prefix;
				docAddress.E2_AddressType = addressType;
				docAddress.E2_AddressSequence = sequence;
			}
			var reader = new OrganisationDataObjectReader(orgAddressDataObject, logger, factory);
			var orgAddress = reader.GetMatched();
			if (orgAddress != null && orgAddress.OA_OH == OrgHeader.UnmatchedOrganisationPK)
			{
				orgAddress = null;
				var docAddresRow = GetColumnIndexer(docAddress);
				SetValue(docAddresRow, JobDocAddressSchema.E2_OA_Address, ZGuid.Empty);
				SetValue(docAddresRow, JobDocAddressSchema.E2_AddressOverride, ZBool.True);
			}
			reader.PopulateJobDocAddress(orgAddress, docAddress);
			return docAddress;
		}

		void FillImporterDocumentaryAddress(CusISFHeader targetBO)
		{
			var orgAddress = ImporterAddress;
			if (orgAddress != null)
			{
				SetValue(targetBO, CusISFHeaderSchema.BF_OH_Importer, orgAddress.OA_OH);
			}
		}

		void FillCustomizedFields(CusISFHeader targetBO)
		{
			if (dataObject.CustomizedFieldCollection != null && dataObject.CustomizedFieldCollection.Count > 0)
			{
				var usedCustomFields = new List<(string, DataType?)>();
				var attributeOne = dataObject.CustomizedFieldCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.CustomizedFieldConstants.CustomAttribOne && x.DataType.GetValueOrDefault() == DataType.String);
				if (attributeOne != null)
				{
					targetBO.CustomAttribute1 = attributeOne.Value.GetValueOrDefault();
					usedCustomFields.Add((Constants.CustomizedFieldConstants.CustomAttribOne, DataType.String));
					usedCustomFields.Add((CusISFHeader.Schema.CustomAttribute1, DataType.String));
				}

				var attributeTwo = dataObject.CustomizedFieldCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == Constants.CustomizedFieldConstants.CustomAttribTwo && x.DataType.GetValueOrDefault() == DataType.String);
				if (attributeTwo != null)
				{
					targetBO.CustomAttribute2 = attributeTwo.Value.GetValueOrDefault();
					usedCustomFields.Add((Constants.CustomizedFieldConstants.CustomAttribTwo, DataType.String));
					usedCustomFields.Add((CusISFHeader.Schema.CustomAttribute2, DataType.String));
				}

				PopulateWorkflowCustomFields(targetBO, dataObject, usedCustomFields.ToArray());
			}
		}

		void FillAdditionalBills(CusISFHeader targetBO)
		{
			var billQuery = new ZQuery();
			billQuery.AddToFilter(CusISFBillSchema.BB_BF, targetBO.PK);
			var billTypeToBeDeletedList = new List<ZString>();
			var billsImported = new List<CusISFBill>();
			if (dataObject.AdditionalBillCollection != null && dataObject.AdditionalBillCollection.Count > 0)
			{
				foreach (var billObject in dataObject.AdditionalBillCollection)
				{
					var billType = billObject.BillType.GetCodeAsUpperCase();
					if (billType == BillTypeList.Codes.MasterBillOfLading || billType == BillTypeList.Codes.HouseBillOfLading || billType == BillTypeList.Codes.OceanBillOfLading)
					{
						ImportBillData(new ISFAdditionalBillDataObjectReader(billObject, logger, factory, targetBO), billTypeToBeDeletedList, billsImported);
					}
				}
			}

			if (dataObject.EntryNumberCollection != null)
			{
				var ensObject = dataObject.EntryNumberCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == Constants.EntryNumberConstants.ENS);
				if (ensObject != null)
				{
					ImportBillData(new ISFEntryNumberBillDataObjectReader(ensObject, logger, factory, targetBO), billTypeToBeDeletedList, billsImported);
				}
			}

			if (dataObject.AdditionalReferenceCollection != null && dataObject.AdditionalReferenceCollection.Count > 0)
			{
				foreach (var additionalRef in dataObject.AdditionalReferenceCollection)
				{
					var billType = additionalRef.Type.GetCodeAsUpperCase();
					if (!billType.IsEmpty && billType != BillTypeList.Codes.MasterBillOfLading && billType != BillTypeList.Codes.HouseBillOfLading
						&& billType != BillTypeList.Codes.OceanBillOfLading && billType != BillTypeList.Codes.USCBPEntryNumber)
					{
						ImportBillData(new ISFAdditionalReferenceBillDataObjectReader(additionalRef, logger, factory, targetBO), billTypeToBeDeletedList, billsImported);
					}
				}
			}

			billQuery.AddToFilter(CusISFBillSchema.BB_BillType, billTypeToBeDeletedList);
			var existingBills = new List<CusISFBill>(factory.Load<CusISFBill>(billQuery));
			RemoveUnRelatedBills(existingBills, billsImported);
		}

		void ImportBillData(IReferenceDataObjectReader reader, List<ZString> typeToBeDeletedList, List<CusISFBill> billsImportedList)
		{
			var bill = reader.BillBO;
			var billType = reader.BillType;
			if (!billType.IsEmpty && !typeToBeDeletedList.Contains(billType))
			{
				typeToBeDeletedList.Add(billType);
			}
			if (!billsImportedList.Contains(bill))
			{
				billsImportedList.Add(bill);
			}
		}

		void RemoveUnRelatedBills(List<CusISFBill> billsToBeDeleted, List<CusISFBill> billsImported)
		{
			billsImported.ForEach(bill => billsToBeDeleted.Remove(bill));
			var billsCannotBeDeleted = billsToBeDeleted.Where(x =>
						(x.BB_BillType == BillTypeList.Codes.OceanBillOfLading || x.BB_BillType == BillTypeList.Codes.HouseBillOfLading) &&
						!(x.BB_CustomsStatus.IsEmpty && x.BB_FirstMatchedDate.IsEmpty && x.BB_MatchDate.IsEmpty)).ToList();
			billsCannotBeDeleted.ForEach(bill => billsToBeDeleted.Remove(bill));
			billsToBeDeleted.DeleteAll();
		}

		void FillISFLines(CusISFHeader targetBO, ISFDataObjectHelper helper)
		{
			if (dataObject.CommercialInfo != null)
			{
				try
				{
					var existingLines = new List<CusISFLine>(factory.Load<CusISFLine>(new ZQuery(CusISFLineSchema.BL_BF, targetBO.GetValue(CusISFHeaderSchema.PK))));
					targetBO.SuspendDeleteProductLines = true;
					existingLines.DeleteAll();
					var existingManufacturerAddress = targetBO.ManufacturerAddresses.ToList();
					if (dataObject.CommercialInfo.CommercialInvoiceCollection != null)
					{
						foreach (var commercialInvoiceHeader in dataObject.CommercialInfo.CommercialInvoiceCollection)
						{
							if (commercialInvoiceHeader.CommercialInvoiceLineCollection != null)
							{
								foreach (var lineDataObject in commercialInvoiceHeader.CommercialInvoiceLineCollection)
								{
									var lineReader = new ISFLineDataObjectReader(lineDataObject, logger, factory, targetBO, this).ReadIntoBusinessObject();
									existingManufacturerAddress.Remove(lineReader.ManufacturerDocAddress);
								}
							}
						}
					}
					existingManufacturerAddress.DeleteAll();
				}
				finally
				{
					targetBO.SuspendDeleteProductLines = false;
				}
			}
		}

		void FillEquips(CusISFHeader targetBO)
		{
			if (dataObject.ContainerCollection != null)
			{
				var existingEquips = new List<CusISFEquip>(factory.Load<CusISFEquip>(new ZQuery(CusISFEquipSchema.BE_BF, targetBO.GetValue(CusISFHeaderSchema.PK))));
				foreach (var containerObject in dataObject.ContainerCollection)
				{
					var equip = new ISFContainerDataObjectReader(containerObject, logger, factory, targetBO).ReadIntoBusinessObject();
					existingEquips.Remove(equip);
				}
				existingEquips.DeleteAll();
			}
		}

		void FillTransportLegs(CusISFHeader targetBO)
		{
			if (dataObject.TransportLegCollection != null)
			{
				var existingTransports = new List<Transport>(new TypedEnumerable<Transport>(targetBO.Transports));
				foreach (var transportLegObject in dataObject.TransportLegCollection)
				{
					var transport = new TransportLegDataObjectReader(transportLegObject, logger, factory, targetBO).ReadIntoBusinessObject();
					targetBO.Transports.Add(transport);
					existingTransports.Remove(transport);
				}
				existingTransports.DeleteAll();
			}
		}

		void FillNotes(CusISFHeader targetBO)
		{
			if (dataObject.NoteCollection != null)
			{
				new NotesCollectionReader(dataObject.NoteCollection, logger, factory, targetBO).ReadIntoCollection();
			}
		}
	}
}
