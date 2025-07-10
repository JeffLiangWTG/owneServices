using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal.AddInfoExtensions;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using EntryNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.EntryNumber;
using GovernmentAgencyProgramCodeList = Enterprise.Customs.US.Business.GovernmentAgencyProgramCodeList;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;
using UniversalShipment = Enterprise.Customs.DataTransfer.Universal;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class DeclarationDataObjectWriter : UniversalShipment.DeclarationDataObjectWriter
	{
		internal DeclarationDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}

		protected override void MergeCountrySpecificRelatedData(ZBool isTopLevelContextCustomsDeclaration, BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			MergeDataObjectWriterManager.AddBizObjData(writeManager, declarationData, ((JobDeclaration)declarationBO).InBondHeader as BusinessObject, includeParent: true, includeChildren: true);
		}

		protected override UniversalShipment.UniversalDataObjectWriterHelper CreateNewUniversalDataObjectWriterHelper(BaseJobDeclaration declarationBO)
		{
			return new UniversalDataObjectWriterHelper(declarationBO.Factory);
		}

		protected override UniversalShipment.CustomsEntryHeaderDataObjectWriter GetNewCustomsEntryHeaderDataObjectWriter()
		{
			return new CustomsEntryHeaderDataObjectWriter(writeManager, helper);
		}

		protected override UniversalShipment.CommercialInvoiceHeaderDataObjectWriter GetNewCommercialInvoiceHeaderDataObjectWriter(Customs.Business.CusEntryHeader relatedEntry)
		{
			return new CommercialInvoiceHeaderDataObjectWriter(writeManager, helper, landedCostDataWriter, relatedEntry);
		}

		protected sealed override void AddTableFetchHintCreators(IExternalFetchHintSupporter externalFetchHintSupporter)
		{
			ExternalFetchHintSupporter = externalFetchHintSupporter;
			CalculateFlagsForFetchHint();
			base.AddTableFetchHintCreators(externalFetchHintSupporter);
			var declarationPK = DeclarationRowForFetchHint.GetValue(JobDeclarationSchema.PK);
			externalFetchHintSupporter.AddFetchHint(new FetchHint(CusAddInfoSchema.B7_ParentID, declarationPK));
			externalFetchHintSupporter.AddFetchHint(new FetchHint(CusCodeDataSchema.CY_ParentID, declarationPK));
			externalFetchHintSupporter.AddFetchHint(new FetchHint(CusEntryNumSchema.CE_ParentID, declarationPK));
			externalFetchHintSupporter.AddFetchHint(new FetchHint(GenAddOnColumnSchema.XA_ParentID, declarationPK));
			externalFetchHintSupporter.AddTableFetchHintCreator(CusAddInfoSchema.Instance, GetCusAddInfoRelatedFetchHints);
			externalFetchHintSupporter.AddTableFetchHintCreator(CusCodeDataSchema.Instance, GetCusCodeDataRelatedFetchHints);
		}
		IExternalFetchHintSupporter ExternalFetchHintSupporter { get; set; }

		void CalculateFlagsForFetchHint()
		{
			var messageType = DeclarationRowForFetchHint.GetValue(JobDeclarationSchema.JE_MessageType);
			isImport = Enterprise.Customs.US.Business.JobMessageTypeList.IsImport(messageType);
			isExport = messageType == Enterprise.Customs.US.Business.JobMessageTypeList.Codes.Export;
			var addInfos = DeclarationRowForFetchHint.GetAddInfos(JobDeclarationSchema.JE_AddInfo);
			useHTS = addInfos.GetValue(USAddInfoSchema.US_TariffType) == TariffTypeList.Codes.HTS;
			effectiveDateForDutyRate = DeclarationRowForFetchHint.GetAddInfos(JobDeclarationSchema.JE_AddInfo).GetValue(USAddInfoSchema.US_DateOfExport);
		}
		bool isImport;
		bool isExport;
		bool useHTS;
		ZDateTime effectiveDateForDutyRate;

		protected virtual IEnumerable<IFetchHint> GetCusAddInfoRelatedFetchHints(IColumnIndexer row)
		{
			var addInfoPK = row.GetValue(CusAddInfoSchema.PK);
			yield return new FetchHint(CusAddInfoSchema.B7_ParentID, addInfoPK);
			yield return new FetchHint(CusCodeDataSchema.CY_ParentID, addInfoPK);
			yield return new FetchHint(GenPivotSchema.XX_Relation1ID, addInfoPK);
		}

		protected virtual IEnumerable<IFetchHint> GetCusCodeDataRelatedFetchHints(IColumnIndexer row)
		{
			var codeDataPK = row.GetValue(CusCodeDataSchema.PK);
			yield return new FetchHint(CusAddInfoSchema.B7_ParentID, codeDataPK);
			yield return new FetchHint(CusCodeDataSchema.CY_ParentID, codeDataPK);
		}

		protected override IEnumerable<IFetchHint> GetCusEntryHeaderRelatedFetchHints(IColumnIndexer row)
		{
			foreach (var fetchHint in base.GetCusEntryHeaderRelatedFetchHints(row))
			{
				yield return fetchHint;
			}
			var entryHeaderPK = row.GetValue(CusEntryHeaderSchema.PK);
			yield return new FetchHint(CusAddInfoSchema.B7_ParentID, entryHeaderPK);
			yield return new FetchHint(CusCodeDataSchema.CY_ParentID, entryHeaderPK);
		}

		protected override IEnumerable<IFetchHint> GetCusEntryLineRelatedFetchHints(IColumnIndexer row)
		{
			foreach (var fetchHint in base.GetCusEntryLineRelatedFetchHints(row))
			{
				yield return fetchHint;
			}
			var entryLinePK = row.GetValue(CusEntryLineSchema.PK);
			yield return new FetchHint(CusUnderbondDecSchema.BU_CL, entryLinePK);

			yield return new FetchHint(CusCodeDataSchema.CY_ParentID, entryLinePK);
		}

		protected override IEnumerable<IFetchHint> GetCusDecHouseContainerPivotRelatedFetchHints(IColumnIndexer row)
		{
			foreach (var fetchHint in base.GetCusDecHouseContainerPivotRelatedFetchHints(row))
			{
				yield return fetchHint;
			}
		}

		protected override IEnumerable<IFetchHint> GetCusDecHouseBillRelatedFetchHints(IColumnIndexer row)
		{
			foreach (var fetchHint in base.GetCusDecHouseBillRelatedFetchHints(row))
			{
				yield return fetchHint;
			}
			var billPK = row.GetValue(CusDecHouseBillSchema.PK);
			yield return new FetchHint(CusAddInfoSchema.B7_ParentID, billPK);
			yield return new FetchHint(CusCodeDataSchema.CY_ParentID, billPK);
			yield return new ZQueryFetchHint(GenPivotSchema.Instance, GetGenPivotFetchHintQueryOfFDA(billPK));
		}

		ZQuery GetGenPivotFetchHintQueryOfFDA(ZGuid relation2ID)
		{
			var query = new ZQuery(GenPivotSchema.XX_Relation2ID, relation2ID);
			query.AddToFilter(GenPivotSchema.XX_RelationType, (ZString)FDARelatedBillsGenPivot.RelationType);
			return query;
		}

		protected override IEnumerable<IFetchHint> GetJobComInvoiceHeaderRelatedFetchHints(IColumnIndexer row)
		{
			foreach (var fetchHint in base.GetJobComInvoiceHeaderRelatedFetchHints(row))
			{
				yield return fetchHint;
			}
			var invoicePK = row.GetValue(JobComInvoiceHeaderSchema.PK);
			yield return new FetchHint(CusAddInfoSchema.B7_ParentID, invoicePK);
			yield return new FetchHint(CusCodeDataSchema.CY_ParentID, invoicePK);
		}

		protected override IEnumerable<IFetchHint> GetJobComInvoiceLineRelatedFetchHints(IColumnIndexer row)
		{
			foreach (var fetchHint in base.GetJobComInvoiceLineRelatedFetchHints(row))
			{
				yield return fetchHint;
			}
			var invoiceLinePK = row.GetValue(JobComInvoiceLineSchema.PK);
			yield return new FetchHint(CusAddInfoSchema.B7_ParentID, invoiceLinePK);
			yield return new FetchHint(CusCodeDataSchema.CY_ParentID, invoiceLinePK);
			yield return new FetchHint(JobComInvoiceLineSchema.JI_ParentID, invoiceLinePK);
			yield return new FetchHint(CusUnderbondDecSchema.BU_JI, invoiceLinePK);
			var addInfos = row.GetAddInfos(JobComInvoiceLineSchema.JI_AddInfo);
			var fetchHint1 = GetFetchHintIfNotEmpty(JobComInvoiceLineSchema.PK, addInfos, USAddInfoSchema.US_JI_ParentProduct);
			if (fetchHint1 != null)
			{
				yield return fetchHint1;
			}

			var tariff = row.GetValue(JobComInvoiceLineSchema.JI_Tariff);
			if (isExport)
			{
				if (!tariff.IsEmpty)
				{
					var factory = (BusinessObjectFactory)ExternalFetchHintSupporter;
					if (factory != null)
					{
						if (useHTS)
						{
							yield return new FetchHint(TariffViewSchema.ZZ1_TariffCode, tariff);
							var zQueryFetchHint = new ZQueryFetchHint(TariffViewSchema.Instance, Customs.Universal.TariffView.Loader.GetEffectiveTariffFilter(factory, Core.Constants.CountryCodes.UnitedStates, Customs.Universal.Constants.TariffTypes.Export, tariff, effectiveDateForDutyRate));
							if (zQueryFetchHint != null)
							{
								yield return zQueryFetchHint;
							}
						}
						else
						{
							yield return new FetchHint(TariffViewSchema.ZZ1_TariffCode, tariff);
							var zQueryFetchHint = new ZQueryFetchHint(TariffViewSchema.Instance, Customs.Universal.TariffView.Loader.GetEffectiveTariffFilter(factory, Core.Constants.CountryCodes.UnitedStates, Customs.Universal.Constants.TariffTypes.ScheduleB, tariff, effectiveDateForDutyRate));
							if (zQueryFetchHint != null)
							{
								yield return zQueryFetchHint;
							}
						}
					}
				}
			}
			else if (isImport)
			{
				if (!tariff.IsEmpty)
				{
					yield return new FetchHint(USCTariffSchema.UE_Tariff, tariff);
					yield return new ZQueryFetchHint(USCTariffRuleSchema.Instance, USCTariffRule.Loader.GetTariffRange(tariff));
				}
				fetchHint1 = GetFetchHintIfNotEmpty(USCTariffSchema.UE_Tariff, addInfos, USAddInfoSchema.US_SupTariff);
				if (fetchHint1 != null)
				{
					yield return fetchHint1;
				}
			}

			fetchHint1 = GetFetchHintIfNotEmpty(USCCountrySchema.UC_Code, addInfos, USAddInfoSchema.US_UC_NKCountryOfOrigin);
			if (fetchHint1 != null)
			{
				yield return fetchHint1;
			}
		}

		protected new UniversalDataObjectWriterHelper helper
		{
			get { return (UniversalDataObjectWriterHelper)base.helper; }
		}

		protected override void PopulateExtraOrganisation(BaseJobDeclaration declarationBO, Shipment declarationData, bool keepExistingData)
		{
			base.PopulateExtraOrganisation(declarationBO, declarationData, keepExistingData);
			var declaration = declarationBO as JobDeclaration;
			var organizationAddressCollection = declarationData.OrganizationAddressCollection;
			if (declaration != null && organizationAddressCollection != null)
			{
				var usHelper = helper;

				var dclarantAddress = organizationAddressCollection.FirstOrDefault(AddressTypes.Declarant);
				organizationAddressCollection.Remove(dclarantAddress);

				if (declaration.IsImport)
				{
					declarationData.AddOrgAddress(writeManager, declaration.IOR, Constants.AddressType.ImporterOfRecord);
					declarationData.AddOrgAddress(writeManager, declaration.FDASubmitter, Constants.AddressType.FDASubmitter);

					declarationData.AddOrgAddress(writeManager, declaration.Seller, Constants.AddressType.Seller);
					declarationData.AddOrgAddress(writeManager, declaration.InvoicerAddress, Constants.AddressType.Invoicer);
					declarationData.AddOrgAddress(writeManager, declaration.NotifyParty, DocAddressType.NotifyParty);
					declarationData.AddOrgAddress(writeManager, declaration.ShipToParty, Constants.AddressType.ShipToParty);
				}
				else if (declaration.IsDrawback)
				{
					declarationData.AddOrgAddress(writeManager, declaration.NotifyParty, DocAddressType.NotifyParty);
					declarationData.AddOrgAddress(writeManager, usHelper.Load<OrgHeader>(declaration.US_DRWTransferee), Constants.AddressType.Transferee);
				}
				else if (declaration.IsExport)
				{
					declarationData.AddOrgAddress(writeManager, declaration.FPPI, Constants.AddressType.ForeignPrincipalPartyInInterest);
				}

				if (declaration.IsInsuranceFunctionEnable && !declaration.US_InsuranceAgent.IsEmpty)
				{
					var address = GetAddressForEBond(declaration);
					organizationAddressCollection.Add(address);
				}
			}
		}

		OrganizationAddress GetAddressForEBond(JobDeclaration declaration)
		{
			var result = new OrganizationAddress(writeManager.WriterStrategy)
			{
				AddressType = Constants.AddressType.BondContact,
				Contact = declaration.CurrentUserFullName,
				Email = declaration.CurrentUserEmailAddress,
				Phone = declaration.CurrentUserWorkPhone
			};

			var currentCompany = GlbCompany.CurrentCompany;

			var districtPortCode = USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var entryFilter = USCustomsDataRegistry.Instance.EntryFiler.GetFallBackValueAtAllLevels(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty)?.EntryFilerCode ?? ZString.Empty;
			var officeCode = USCustomsDataRegistry.Instance.BRecordOfficeCode.GetFallBackValueAtAllLevels(currentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

			var secondaryNotifyParty = string.Concat(districtPortCode, entryFilter, officeCode);

			if (!string.IsNullOrWhiteSpace(secondaryNotifyParty))
			{
				result.SetRegistrationNumberCollection(() => new List<RegistrationNumber>
				{
					new RegistrationNumber
					{
						Value = secondaryNotifyParty,
						CountryOfIssue = Country.New(currentCompany.Country),
						Type = new RegistrationNumberType
						{
							Code = "SNP", Description = "Secondary Notify Party"
						}
					}
				});
			}

			return result;
		}

		protected override void PopulateManufacturer(BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			if (declarationBO.IsImport)
			{
				base.PopulateManufacturer(declarationBO, declarationData);
			}
		}

		protected override void PopulateSoldToPartyAddress(BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			if (declarationBO.IsImport)
			{
				base.PopulateSoldToPartyAddress(declarationBO, declarationData);
			}
		}

		protected override void PopulateBuyingAgent(BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			if (declarationBO.IsImport)
			{
				base.PopulateBuyingAgent(declarationBO, declarationData);
			}
		}

		protected override void PopulateSellingAgent(BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			if (declarationBO.IsImport)
			{
				base.PopulateSellingAgent(declarationBO, declarationData);
			}
		}

		protected override void PopulateConsigneeAddress(BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			if (declarationBO.IsImport)
			{
				base.PopulateConsigneeAddress(declarationBO, declarationData);
			}
		}

		protected override void PopulateConsignee(BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			if (declarationBO.IsExport)
			{
				base.PopulateConsignee(declarationBO, declarationData);
			}
		}

		protected override void AddAdditionalParentBillDetails(Customs.Business.Bill billBO, AdditionalBill additionalBill)
		{
			ZString? parentBillNumber = null;
			var usBill = (Business.Bill)billBO;
			var parentBill = usBill.ParentBill;
			if (parentBill != null)
			{
				parentBillNumber = parentBill.CU_BillNum;
				additionalBill.SetAddInfoCollection(() => additionalBill.AddInfoCollection.AddSafe(new UniversalDataBuss.DataObjects.Universal.AddInfo() { Key = Constants.AddInfoKeys.AdditionalBill.ParentBillIssuerSCAC, Value = parentBill.US_UI_NKBillIssuerSCAC }));
				var parentParentBill = parentBill.ParentBill;
				if (parentParentBill != null)
				{
					additionalBill.SetAddInfoCollection(() => additionalBill.AddInfoCollection.AddSafe(new UniversalDataBuss.DataObjects.Universal.AddInfo() { Key = Constants.AddInfoKeys.AdditionalBill.ParentMasterBillNumber, Value = parentParentBill.CU_BillNum }));
					additionalBill.SetAddInfoCollection(() => additionalBill.AddInfoCollection.AddSafe(new UniversalDataBuss.DataObjects.Universal.AddInfo() { Key = Constants.AddInfoKeys.AdditionalBill.ParentMasterBillIssuerSCAC, Value = parentParentBill.US_UI_NKBillIssuerSCAC }));
				}
			}
			additionalBill.ParentBillNumber = parentBillNumber;
		}

		protected override List<UniversalDataBuss.DataObjects.Universal.AddInfo> CreateDeclarationAddInfo(BaseJobDeclaration declarationBO, Shipment declarationData)
		{
			var addInfoCollection = base.CreateDeclarationAddInfo(declarationBO, declarationData);
			var usDeclaration = (JobDeclaration)declarationBO;
			var wayBillIssuerSCAC = ZString.Empty;
			if (usDeclaration.JE_HouseBill.IsEmpty)
			{
				if (!usDeclaration.JE_MasterBill.IsEmpty)
				{
					wayBillIssuerSCAC = usDeclaration.JE_MasterBillIssuerSCAC;
				}
			}
			else
			{
				wayBillIssuerSCAC = usDeclaration.JE_HouseBillIssuerSCAC;
				helper.Update(addInfoCollection, Constants.AddInfoKeys.Declaration.MasterWayBillNumber, usDeclaration.JE_MasterBill);
				helper.Update(addInfoCollection, Constants.AddInfoKeys.Declaration.MasterWayBillIssuerSCAC, usDeclaration.JE_MasterBillIssuerSCAC);
			}
			helper.Update(addInfoCollection, Constants.AddInfoKeys.Declaration.WayBillIssuerSCAC, wayBillIssuerSCAC);
			helper.Update(addInfoCollection, Constants.AddInfoKeys.Declaration.BondDesignationCode, (ZString)BondDesignationCodeList.Codes.BasicBond);
			PopulateStatementDetails(usDeclaration, addInfoCollection, helper);
			return addInfoCollection;
		}

		void PopulateStatementDetails(JobDeclaration declarationBO, List<UniversalDataBuss.DataObjects.Universal.AddInfo> addInfoCollection, UniversalDataObjectWriterHelper declarationHelper)
		{
			declarationHelper.Update(addInfoCollection, Constants.AddInfoKeys.Declaration.StatementNumber, declarationBO.StatementNo);
			declarationHelper.Update(addInfoCollection, Constants.AddInfoKeys.Declaration.StatementStatus, declarationBO.StatementStatus);
			declarationHelper.Update(addInfoCollection, Constants.AddInfoKeys.Declaration.StatementPaidDate, declarationBO.StatementPaidDate);
			declarationHelper.Update(addInfoCollection, Constants.AddInfoKeys.Declaration.StatementPaymentStatus, declarationBO.PaymentStatus);
		}

		protected override List<EntryNumber> PopulateCusDisposition(BaseJobDeclaration declarationBO)
		{
			var result = base.PopulateCusDisposition(declarationBO);
			var declaration = declarationBO as JobDeclaration;
			if (declaration != null)
			{
				var governmentAgenciesCodeList = new GovernmentAgencyProgramCodeList();
				foreach (CusDisposition dispositions in declaration.EntryPGACusDispositions)
				{
					var entryNumberData = new EntryNumber()
					{
						Number = declaration.US_EntryFilerCode + "-" + declaration.DecEntryNumber,
						Type = new EntryType() { Code = dispositions.CDI_StatusKey, Description = governmentAgenciesCodeList.GetDescriptionFromCode(dispositions.CDI_StatusKey) },
						EntryStatus = new EntryStatus() { Code = dispositions.CDI_Status, Description = dispositions.StatusDescription },
						IssueDate = dispositions.CDI_StatusDate,
						EntryIsSystemGenerated = true
					};
					result.Add(entryNumberData);
				}
			}
			return result;
		}

		protected override void PopulateNoOfPacksAndPackType(BaseJobDeclaration declarationBO, Shipment declarationData, bool keepExistingData)
		{
			if (declarationBO.IsImport)
			{
				declarationData.TotalNoOfPacks = PopulateValue(declarationData.TotalNoOfPacks, keepExistingData, () => declarationBO.JE_TotalNoOfPacks);
				declarationData.TotalNoOfPacksPackageType = PopulateValue(declarationData.TotalNoOfPacksPackageType, keepExistingData, () => ListHelper.GetWithDescription<PackageType>(declarationBO.JE_TotalNoOfPacksPackType, declarationBO.Lookups.JE_TotalNoOfPacksPackType_List));
			}
			else
			{
				base.PopulateNoOfPacksAndPackType(declarationBO, declarationData, keepExistingData);
			}
		}
	}
}
