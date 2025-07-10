using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class AddInfoJobComInvoiceLineLookups : USAddInfoLookups
	{
		public AddInfoJobComInvoiceLineLookups(AddInfoJobComInvoiceLine parent)
			: base(parent)
		{
		}

		public new AddInfoJobComInvoiceLine Parent
		{
			get { return (AddInfoJobComInvoiceLine)base.Parent; }
		}

		public JobComInvoiceLine InvoiceLine
		{
			get { return (JobComInvoiceLine)Parent.Parent; }
		}

		public CodeDescriptionPairList US_HazMatQualifierList
		{
			get { return Factory.GetCachedValue<HazMatQualifierList>(); }
		}

		public CodeDescriptionPairList TaxCodeList
		{
			get { return GetTaxCodeList(Parent.InvoiceLine.ImportTariff); }
		}

		public CodeDescriptionPairList TaxRateList
		{
			get
			{
				var invoiceLine = Parent.InvoiceLine;
				if (invoiceLine.IsCBMAProductClaimAndIsNotCBMA23Effective)
				{
					return CBMATaxRateList;
				}
				else
				{
					return GetTaxRateList(invoiceLine.ImportTariff, invoiceLine.US_TaxCode, invoiceLine.US_TaxRateT);
				}
			}
		}

		public CodeDescriptionPairList CBMATaxRateList => Business.CBMATaxRateList.GetList(Factory, Parent.US_TaxCode, Parent.US_TaxRateS);

		public CodeDescriptionPairList OrigTaxCodeList
		{
			get { return GetTaxCodeList(Parent.InvoiceLine.OriginalImportTariff); }
		}

		public CodeDescriptionPairList OrigTaxRateList
		{
			get { return GetTaxRateList(Parent.InvoiceLine.OriginalImportTariff, Parent.InvoiceLine.US_R_OrigTaxCode, Parent.InvoiceLine.US_R_OrigTaxRateT); }
		}

		public IBusinessObjectCollection ADDCaseNumberList
		{
			get
			{
				var invoiceLine = Parent.InvoiceLine;
				if (invoiceLine.IsCombinedLine())
				{
					return new ACEADD_CVDListLoader(Factory).GetCaseNumberList(invoiceLine.US_UC_NKCountryOfOrigin, invoiceLine.US_ADDCaseNo, "A", invoiceLine.JI_Tariff);
				}
				else
				{
					return new ACEADD_CVDListLoader(Factory).GetCaseNumberList(invoiceLine.US_UC_NKCountryOfOrigin, invoiceLine.US_ADDCaseNo, "A", invoiceLine.JI_Tariff, invoiceLine.US_SupTariff);
				}
			}
		}

		public IBusinessObjectCollection CVDCaseNumberList
		{
			get
			{
				var invoiceLine = Parent.InvoiceLine;
				if (invoiceLine.IsCombinedLine())
				{
					return new ACEADD_CVDListLoader(Factory).GetCaseNumberList(invoiceLine.US_UC_NKCountryOfOrigin, invoiceLine.US_CVDCaseNo, "C", invoiceLine.JI_Tariff);
				}
				else
				{
					return new ACEADD_CVDListLoader(Factory).GetCaseNumberList(invoiceLine.US_UC_NKCountryOfOrigin, invoiceLine.US_CVDCaseNo, "C", invoiceLine.JI_Tariff, invoiceLine.US_SupTariff);
				}
			}
		}

		public CodeDescriptionPairList ReconOrigSPIList
		{
			get
			{
				SPILine spiLine = SPILine.NewReconOrig(Parent.InvoiceLine);

				return Parent.Factory.GetCachedValue(spiLine.GetKeyForSPIList(), delegate
					{
						return SPICompleteList.GetRelevantListFor(spiLine);
					}
				);
			}
		}

		public CodeDescriptionPairList ReconFeeAndChargeList
		{
			get
			{
				IReconOriginalChargeParent parent = Parent.InvoiceLine;
				return parent.FeeAndChargeList;
			}
		}

		public CodeDescriptionPairList SetIndicatorList
		{
			get { return Factory.GetCachedValue<SetIndicatorList>(); }
		}

		public CodeDescriptionPairList SPIList
		{
			get
			{
				var invoiceLine = Parent.InvoiceLine;
				if (invoiceLine.IsCombinedLine())
				{
					var parentLine = invoiceLine.ParentTariffLine ?? invoiceLine;
					return Parent.Factory.GetCachedValue(GetKeyForCombinedLinesSPIList(parentLine), delegate
					{
						var spiList = SPICompleteList.GetRelevantListFor(SPILine.New(parentLine));
						foreach (JobComInvoiceLine childLine in parentLine.ChildLines)
						{
							var childSPIList = SPICompleteList.GetRelevantListFor(SPILine.New(childLine));
							foreach (CodeDescriptionPair item in childSPIList)
							{
								if (!spiList.ContainsCode(item.Code))
								{
									spiList.AddPair(item.Code, item.Description);
								}
							}
						}
						return spiList;
					});
				}
				else
				{
					var spiLine = SPILine.New(invoiceLine);
					return Parent.Factory.GetCachedValue(spiLine.GetKeyForSPIList(), delegate
					{
						return SPICompleteList.GetRelevantListFor(spiLine);
					});
				}
			}
		}

		ZString GetKeyForCombinedLinesSPIList(JobComInvoiceLine parentLine)
		{
			var result = new ZStringBuilder();
			if (parentLine != null)
			{
				result.Append(SPILine.New(parentLine).GetKeyForSPIList());

				foreach (JobComInvoiceLine childLine in parentLine.ChildLines)
				{
					result.Append(SPILine.New(childLine).GetKeyForSPIList());
				}
			}
			return result.ToString();
		}

		public CodeDescriptionPairList NMFS370DisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonList(InvoiceLine.OGARequirementCalculator.NMFS370RequirementCode, Parent.Factory, ShouldCheckEntryType, EntryType); }
		}

		public CodeDescriptionPairList NMFSAMRDisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonList(InvoiceLine.OGARequirementCalculator.NMFSAMRRequirementCode, Parent.Factory, ShouldCheckEntryType, EntryType); }
		}

		public CodeDescriptionPairList NMFSHMSDisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonList(InvoiceLine.OGARequirementCalculator.NMFSHMSRequirementCode, Parent.Factory, ShouldCheckEntryType, EntryType); }
		}

		public CodeDescriptionPairList PGADisclaimReasonList
		{
			get { return Factory.GetCachedValue<PGADisclaimReasonList>(); }
		}

		public CodeDescriptionPairList TTBDisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonList(InvoiceLine.OGARequirementCalculator.TTBRequirementCode, Factory, ShouldCheckEntryType, EntryType); }
		}

		public CodeDescriptionPairList CPSCDisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonList(InvoiceLine.OGARequirementCalculator.CPSCRequirementCode, Factory, ShouldCheckEntryType, EntryType); }
		}

		public CodeDescriptionPairList APHISDisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonList(InvoiceLine.OGARequirementCalculator.APHISRequirementCode, Factory, ShouldCheckEntryType, EntryType); }
		}

		public CodeDescriptionPairList FWSDisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonList(InvoiceLine.OGARequirementCalculator.FWSRequirementCode, Factory, ShouldCheckEntryType, EntryType, GovernmentAgencyProgramCodeList.Codes.FWS); }
		}

		public CodeDescriptionPairList OMCDisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonList(InvoiceLine.OGARequirementCalculator.OMCRequirementCode, Factory, ShouldCheckEntryType, EntryType); }
		}

		public CodeDescriptionPairList FDADisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonList(InvoiceLine.OGARequirementCalculator.ACEFDARequirementCode, Factory, ShouldCheckEntryType, EntryType); }
		}

		public CodeDescriptionPairList AMSDisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonList(InvoiceLine.OGARequirementCalculator.AMSRequirementCode, Factory, ShouldCheckEntryType, EntryType); }
		}

		public CodeDescriptionPairList NOPDisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonList(InvoiceLine.OGARequirementCalculator.NOPRequirementCode, Factory, ShouldCheckEntryType, EntryType); }
		}

		public CodeDescriptionPairList DEADisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonList(InvoiceLine.OGARequirementCalculator.DEARequirementCode, Factory, ShouldCheckEntryType, EntryType); }
		}

		public CodeDescriptionPairList PSTDisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonList(InvoiceLine.OGARequirementCalculator.PSTRequirementCode, Parent.Factory, ShouldCheckEntryType, EntryType); }
		}

		public CodeDescriptionPairList HFCDisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonList(InvoiceLine.OGARequirementCalculator.HFCRequirementCode, Parent.Factory, ShouldCheckEntryType, EntryType); }
		}

		public CodeDescriptionPairList VNEDisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonList(InvoiceLine.OGARequirementCalculator.VNERequirementCode, Parent.Factory, ShouldCheckEntryType, EntryType); }
		}

		public CodeDescriptionPairList ODSDisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonList(InvoiceLine.OGARequirementCalculator.ODSRequirementCode, Parent.Factory, ShouldCheckEntryType, EntryType); }
		}

		public CodeDescriptionPairList FSISDisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonList(InvoiceLine.OGARequirementCalculator.FSISRequirementCode, Parent.Factory, ShouldCheckEntryType, EntryType); }
		}

		public CodeDescriptionPairList TSCADisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonList(InvoiceLine.OGARequirementCalculator.TSCARequirementCode, Parent.Factory, ShouldCheckEntryType, EntryType); }
		}

		public CodeDescriptionPairList NHTSADisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonList(InvoiceLine.OGARequirementCalculator.NHTSARequirementCode, Parent.Factory, ShouldCheckEntryType, EntryType); }
		}

		public CodeDescriptionPairList LaceyDisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonList(InvoiceLine.OGARequirementCalculator.ACELaceyRequirementCode, Parent.Factory, ShouldCheckEntryType, EntryType); }
		}

		bool ShouldCheckEntryType
		{
			get
			{
				return InvoiceLine.InvoiceHeader?.IsAttachedToPersistentDeclaration ?? false;
			}
		}

		ZString EntryType
		{
			get
			{
				return InvoiceLine.Declaration?.US_EntryType ?? string.Empty;
			}
		}

		public CodeDescriptionPairList EPANetQtyUQList
		{
			get
			{
				return Factory.GetCachedValue("EPANetQtyUQ",
					delegate
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(Core.Constants.Weight.Kilograms, "Kilograms");
						result.AddPair(Core.Constants.Volume.Litre, "Litre");
						return result;
					}
				);
			}
		}

		public USCarrierCombinedCollection USCarrierList
		{
			get { return new USCarrierCombinedCollection(Factory); }
		}

		public CodeDescriptionPairList ManifestUQList
		{
			get
			{
				var isConsumptionFTZ = InvoiceLine?.Declaration?.IsConsumptionFTZ ?? false;
				if (isConsumptionFTZ)
				{
					return new RefPackTypeCollection(Factory).GetAsCodeDescriptionPairWithStandardUnits();
				}

				return Factory.GetCachedValue<ShippingOrPackingingUnitList>();
			}
		}

		public CodeDescriptionPairList ProductExclusionList
		{
			get
			{
				return Factory.GetCachedValue("ProductExclusionList" + InvoiceLine.JI_Tariff, () =>
				{
					var result = new CodeDescriptionPairList();
					if (InvoiceLine.IsOnlySteelProductAvailable)
					{
						result.AddPair(AdditionalDeclarationTypeCodeList.Codes._02, AdditionalDeclarationTypeCodeList.Descriptions._02);
					}
					else if (InvoiceLine.IsOnlyAluminumProductAvailable)
					{
						result.AddPair(AdditionalDeclarationTypeCodeList.Codes._03, AdditionalDeclarationTypeCodeList.Descriptions._03);
					}
					else
					{
						result.AddPair(AdditionalDeclarationTypeCodeList.Codes._02, AdditionalDeclarationTypeCodeList.Descriptions._02);
						result.AddPair(AdditionalDeclarationTypeCodeList.Codes._03, AdditionalDeclarationTypeCodeList.Descriptions._03);
					}
					return result;
				});
			}
		}

		[System.Xml.Serialization.XmlIgnore]
		public ICollection US_LicenseType_List
		{
			get
			{
				if (InvoiceLine.IsFTZAdmission)
				{
					return US_LicenseType_ListForFTZ;
				}
				return US_LicenseType_ListForEXP;
			}
		}

		public CodeDescriptionPairList US_LicenseType_ListForFTZ
		{
			get
			{
				var date = ZDate.Today;
				var uS_LicenseType_ListForFTZ = Factory.GetCachedValue("US_LicenseType_ListForFTZ|" + date.ToString(), () =>
				{
					var codeDescriptionPairList = new CodeDescriptionPairList();
					var licenseTypeList = new USFTZLicenseTypeCollection(Factory, date);
					licenseTypeList.Load();
					foreach (var license in licenseTypeList.Cast<ZZRefCusCodeListCombined>())
					{
						codeDescriptionPairList.AddPair(license.ZZD_Code, license.ZZD_Description);
					}
					return codeDescriptionPairList;
				});
				return uS_LicenseType_ListForFTZ;
			}
		}

		public static USAESLicenseCodeCollection GetUS_LicenseType_ListForEXPCollection(BusinessObjectFactory factory, ZDateTime date)
		{
			var uS_LicenseType_ListForEXP = new USAESLicenseCodeCollection(factory);
			uS_LicenseType_ListForEXP.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.EffectiveDate, "Property1", date));
			return uS_LicenseType_ListForEXP;
		}

		public USAESLicenseCodeCollection US_LicenseType_ListForEXP
		{
			get
			{
				return GetUS_LicenseType_ListForEXPCollection(Factory, InvoiceLine.ExportDateForLicenseType);
			}
		}

		public CodeDescriptionPairList ControlledGroupNames
		{
			get
			{
				var declaration = Parent.InvoiceLine.Declaration;
				return Factory.GetCachedValue($"ControlledGroupNames|{declaration?.Importer?.OH_Code ?? ZString.Empty}", () =>
				{
					var result = new CodeDescriptionPairList();
					if (declaration?.ImporterWrapper is OrgHeaderWrapper importerWrapper)
					{
						foreach (ImportersControlledGroupName controlledGroupName in importerWrapper.ImportersControlledGroupNames)
						{
							var groupName = controlledGroupName.US_GroupName.Left(AutoUSAddInfo.Schema.US_ControlledGroupNameMaxLength);
							if (!groupName.IsEmpty)
							{
								result.AddPairIfNotExist(groupName, groupName);
							}
						}
					}

					return result;
				});
			}
		}

		public CodeDescriptionPairList ForeignProducerIdentifiers
		{
			get
			{
				var declaration = InvoiceLine.Declaration;
				var isCBMA23Effective = InvoiceLine.IsCBMA23Effective;
				var shouldShowOldFPIForCBMA = EntryTypeList.IsExWarehouseTypeOrFTZ(InvoiceLine.ImportEntryType);
				return Factory.GetCachedValue($"ForeignProducerIdentifiers|{declaration?.Importer?.OH_Code ?? ZString.Empty}|{InvoiceLine.JI_OA_ManufacturerAddress}|{isCBMA23Effective}|{shouldShowOldFPIForCBMA}", () =>
				{
					var result = new CodeDescriptionPairList();

					if (declaration?.ImporterWrapper is OrgHeaderWrapper importerWrapper)
					{
						foreach (AllocationQuantityPerFPI allocationQty in importerWrapper.AllocationQuantityPerFPIs)
						{
							var foreignProducerIdentifier = allocationQty.US_ForeignProducerIdentifier.Left(AutoUSAddInfo.Schema.US_FPIMaxLength);

							if (!foreignProducerIdentifier.IsEmpty && allocationQty.US_OA_ManufacturerAddress == Parent.InvoiceLine.JI_OA_ManufacturerAddress)
							{
								if (!isCBMA23Effective || shouldShowOldFPIForCBMA)
								{
									result.AddPairIfNotExist(foreignProducerIdentifier, foreignProducerIdentifier);
								}
							}
						}
					}

					if (isCBMA23Effective && InvoiceLine.ManufacturerAddress is OrgAddress manufacturerAddress)
					{
						var identifierFromManufacturer = manufacturerAddress.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.ForeignProducerIdentifier, Core.Constants.CountryCodes.UnitedStates).Left(AutoUSAddInfo.Schema.US_FPIMaxLength);
						if (!identifierFromManufacturer.IsEmpty)
						{
							result.AddPairIfNotExist(identifierFromManufacturer, identifierFromManufacturer);
						}
					}

					return result;
				});
			}
		}

		public CodeDescriptionPairList TaxRateTypeList
		{
			get
			{
				var line = InvoiceLine;
				return GetTaxRateTypeList(line.ImportTariff, line.US_TaxCode, line.TaxRateT_ReadOnly);
			}
		}

		public CodeDescriptionPairList OrigTaxRateTypeList
		{
			get
			{
				var line = InvoiceLine;
				return GetTaxRateTypeList(line.OriginalImportTariff, line.US_R_OrigTaxCode, line.OrigTaxRateT_ReadOnly);
			}
		}

		CodeDescriptionPairList GetTaxRateTypeList(USCTariff tariff, ZString taxCode, ZBool taxRateT_ReadOnly)
		{
			return Factory.GetCachedValue((tariff?.PK ?? ZGuid.Empty).ToString() + taxCode + taxRateT_ReadOnly.ToString(), () =>
			{
				var result = new CodeDescriptionPairList();
				if (tariff == null || taxCode.IsEmpty || taxRateT_ReadOnly)
				{
					result = new RateTypeList();
				}
				else
				{
					var taxRate = tariff.DutyRates.GetRateForTaxFeeClassCode(taxCode);
					result.AddPair(RateTypeList.Codes.Primary, RateTypeList.Descriptions.Primary + " - " + taxRate.UD_TaxFeeSpecificRate);
					result.AddPair(RateTypeList.Codes.Secondary, RateTypeList.Descriptions.Secondary + " - " + taxRate.UD_TaxFeeAdvalorem);
				}
				return result;
			});
		}

		public USAESECCNNumberCollection US_ECCNList
		{
			get
			{
				return Factory.GetCachedValue($"ECCNList|{InvoiceLine.US_LicenseType}|{ZDateTime.Today.ToISO8601ShortDateString()}", delegate
				{
					var uS_ECCNList = new USAESECCNNumberCollection(Parent.Factory, InvoiceLine.US_LicenseType);
					uS_ECCNList.FilterBusinessObjectDefaults.RemoveAll();
					uS_ECCNList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.EffectiveDate, "Property1", ZDateTime.Today));
					return uS_ECCNList;
				});
			}
		}
	}
}
