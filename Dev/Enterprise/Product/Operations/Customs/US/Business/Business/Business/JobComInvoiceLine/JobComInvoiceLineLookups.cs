using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class JobComInvoiceLineLookups : Customs.Business.JobComInvoiceLineLookups
	{
		public JobComInvoiceLineLookups(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public new JobComInvoiceLine InvoiceLine
		{
			get { return (JobComInvoiceLine)base.InvoiceLine; }
		}

		protected new JobComInvoiceLine Parent
		{
			get { return (JobComInvoiceLine)base.Parent; }
		}

		public CodeDescriptionPairList ParentIDList
		{
			get
			{
				var parentIdList = new CodeDescriptionPairList();
				var header = Parent.InvoiceHeader;
				if (header != null)
				{
					Func<JobComInvoiceHeader, IEnumerable<ZGuid>, IEnumerable<JobComInvoiceLine>> parentGetter = (invoiceHeader, parentIDs) =>
					{
						return invoiceHeader.JobComInvoiceLines
						  .Find(new ZQuery(JobComInvoiceLineSchema.JI_ParentID, parentIDs))
						  .Where(line => line.PK != Parent.PK)
						  .Cast<JobComInvoiceLine>();
					};

					var xParentList = parentGetter(header, null);
					xParentList = xParentList.Concat(parentGetter(header, xParentList.Select(x => x.PK)).Where(line => line.IsSetXLine || line.IsSetVLine));
					xParentList.ForEach(line => parentIdList.AddPair(line.PK, header.JZ_InvoiceNumber + " - " + line.JI_LineNo, line.JI_Description));
				}
				return parentIdList;
			}
		}

		public OrgHeaderCollection Organisations
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		#region Tariffs

		public ICollection SupTariffsList
		{
			get
			{
				return Parent.SupTariffFormattedFieldType == nameof(FieldType.TextDropEdit) ? Parent.ApplicableSupTariffList : Tariffs;
			}
		}

		public BusinessObjectCollection Tariffs
		{
			get
			{
				if (Parent.UseScheduleB)
				{
					return GetScheduleBTariffs(Parent.JI_Tariff);
				}
				else if (Parent.UseHTSForExport)
				{
					return GetHTSExportTariffs(Parent.JI_Tariff);
				}
				else
				{
					return ImportTariffs;
				}
			}
		}

		public USCTariffCollection ImportTariffs
		{
			get
			{
				var htsTariffs = new USCTariffCollection(Factory);
				if (!Parent.JI_FormattedTariff.IsEmpty)
				{
					htsTariffs.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(USCTariff.FilterSchema.Tariff, "Property", Parent.JI_FormattedTariff));
				}

				return htsTariffs;
			}
		}

		public BusinessObjectCollection ExportTariffs
		{
			get
			{
				if (Parent.UseScheduleB)
				{
					return GetScheduleBTariffs(Parent.US_ExportTariff);
				}
				else
				{
					return GetHTSExportTariffs(Parent.US_ExportTariff);
				}
			}
		}

		internal Universal.TariffViewCollection GetScheduleBTariffs(ZString tariffCode)
			=> GetTariffsCalculatingDate(Universal.Constants.TariffTypes.ScheduleB, tariffCode);

		internal Universal.TariffViewCollection GetHTSExportTariffs(ZString tariffCode)
		 => GetTariffsCalculatingDate(Universal.Constants.TariffTypes.Export, tariffCode);

		public Universal.TariffViewCollection GetTariffsCalculatingDate(ZString tariffType, ZString tariffCode)
		{
			var effectiveDate = ZDate.Empty;
			var declaration = Parent?.Declaration;
			if (declaration != null && declaration.IsDrawback && Parent.US_DRWExportDate.IsValid)
			{
				effectiveDate = Parent.US_DRWExportDate.Date;
			}
			else
			{
				effectiveDate = Parent.EffectiveDateForDutyRate;
			}
			var result = Factory.GetTariffs(tariffType, tariffCode, effectiveDate);
			return result;
		}

		public TariffTypeList US_TariffTypeList
		{
			get { return Factory.GetCachedValue<TariffTypeList>(); }
		}

		#endregion

		public BusinessObjectCollection GlobalEntryLineKeys
		{
			get { return globalEntryLineKeys ?? (globalEntryLineKeys = new GlobalCusEntryLineCollection(Factory, InvoiceLine)); }
		}
		BusinessObjectCollection globalEntryLineKeys;

		public override CodeDescriptionPairList InvoiceUQList
		{
			get
			{
				if (Parent.IsExport)
				{
					return Factory.GetCachedValue("US InvoiceLine InvoiceUQList-Export", delegate
					{
						var result = new CodeDescriptionPairList();
						result.AddRange(base.InvoiceUQList);
						result.AddRangeOverwriteIfExists(Factory.GetCachedValue<AESUnitOfMeasureList>());
						result.Sort();
						return result;
					});
				}
				else if (Parent.Declaration != null && Parent.Declaration.IsACEDrawback)
				{
					return Factory.GetCachedValue<ACEDrawbackUnitOfMeasureList>();
				}
				else
				{
					return Factory.GetCachedValue<ABIUnitOfMeasureList>();
				}
			}
		}

		protected override ZString GetClassificationType()
		{
			var result = ZString.Empty;
			if (Parent.IsExport)
			{
				if (Parent.UseScheduleB)
				{
					result = ClassificationTypeList.Codes.SHB;
				}
				else
				{
					result = ClassificationTypeList.Codes.HTE;
				}
			}
			else
			{
				result = ClassificationTypeList.Codes.HTI;
			}
			return result;
		}

		public override CodeDescriptionPairList CustomsUQList
		{
			get
			{
				if (Parent.IsExport)
				{
					return Factory.GetCachedValue<AESUnitOfMeasureList>();
				}
				else
				{
					return Factory.GetCachedValue<ABIUnitOfMeasureList>();
				}
			}
		}

		public override IBaseClassificationCollection<BaseCusClassification> ClassificationList
		{
			get
			{
				if (Parent.UseScheduleB)
				{
					return new ExportClassificationCollection(Factory);
				}
				else
				{
					return new ImportClassificationCollection(Factory);
				}
			}
		}

		public LimitedReportingExportInformationCodeList LimitedReportingExportCodeList
		{
			get { return Factory.GetCachedValue<LimitedReportingExportInformationCodeList>(); }
		}

		protected override Customs.Business.OrgSupplierPartCollection GetNewPartCollection()
		{
			return Parent.Declaration != null && !Parent.Declaration.IsDrawback ?
					new OrgSupplierPartCollection(Factory, Parent, Parent.InvoiceHeader?.IsExport ?? ZBool.False) :
					new OrgSupplierPartCollection(Factory);
		}

		protected override void SetOrAdjustPartsListProperties(Customs.Business.OrgSupplierPartCollection partsList)
		{
			base.SetOrAdjustPartsListProperties(partsList);
			if (Parent.Declaration != null && Parent.Declaration.IsDrawback)
			{
				Parent.AdjustPartsListPropertiesForDrawback(partsList);
			}
		}

		public USCCountryCollection USCountryOfOrigins
		{
			get { return new USCCountryCollection(Factory); }
		}

		public DepositRateIndicatorList AntidumpingDutyDepositRates
		{
			get
			{
				return new DepositRateIndicatorList(AntidumpingUSCACCaseRate);
			}
		}

		public DepositRateIndicatorList CountervailingDutyDepositRates
		{
			get
			{
				return new DepositRateIndicatorList(CountervailingUSCACCaseRate);
			}
		}
		public USCACCaseRate AntidumpingUSCACCaseRate
		{
			get
			{
				return Parent.GetUSCACCaseRate(Parent.AntidumpingDutyCase);
			}
		}

		public USCACCaseRate CountervailingUSCACCaseRate
		{
			get
			{
				return Parent.GetUSCACCaseRate(Parent.CountervailingDutyCase);
			}
		}

		public CodeDescriptionPairList DRWCMCDIndicatorCodeList
		{
			get
			{
				return Factory.GetCachedValue("DRWCMCDIndicatorCodeList", // 'DRWCMCDIndicatorCodeList' is not a database field
					delegate
					{
						CodeDescriptionPairList result = new CodeDescriptionPairList();
						result.AddPair("E", "Neither Certificate Issued");
						result.AddPair("D", "Certificate of Delivery (CD) Issued");
						result.AddPair("M", "Certificate of Manufacture (CM) Issued");
						return result;
					}
				);
			}
		}

		public CodeDescriptionPairList DRWExportActionList
		{
			get
			{
				var declaration = Parent.Declaration;
				return Factory.GetCachedValue("DRWExportActionList" + (declaration != null ? declaration.JE_ApplicationCode : ZString.Empty), // 'DRWExportActionList' is not a database field
					delegate
					{
						var result = new CodeDescriptionPairList();

						if (declaration == null || !declaration.IsACEDrawback)
						{
							result.AddPair("D", "Destroyed");
							result.AddPair("F", "FTZ Transfer");
							result.AddPair("L", "Laded as Supplies");
							result.AddPair("M", "Mail Shipment");
							result.AddPair("G", "Government Exports");
							result.AddPair("V", "Vessels or Aircraft");
						}
						else
						{
							result.AddPair("D", "Destroy");
							result.AddPair("E", "Export");
						}

						return result;
					}
				);
			}
		}

		public IList<string> ValidCMPorts
		{
			get { return new string[] { "0401", "1001", "2002", "3901", "5201", "5301", "2704", "2809" }; }
		}

		public override CodeDescriptionPairList HazardousMaterialCodeQualifierList
		{
			get { return Parent.AddInfoLookups.US_HazMatQualifierList; }
		}

		public ACEDrawbackActionCodeList ACEDrawbackActionList
		{
			get { return Factory.GetCachedValue<ACEDrawbackActionCodeList>(); }
		}

		public ACEDrawbackClaimBasisList DrawbackClaimBasisList
		{
			get { return Factory.GetCachedValue<ACEDrawbackClaimBasisList>(); }
		}

		public DrawbackAccountingMethodCodeList DrawbackAccountingCodes
		{
			get
			{
				var declaration = Parent.Declaration;
				var isTFTEAProvision = declaration != null && new ZString("51,53,54,55,56,57,58,64,65,67,68,69,70,71").OccurrencesIgnoringCase(declaration.US_EntryType) == 1;
				return Factory.GetCachedValue("IsTFTEAProvision" + isTFTEAProvision, () =>
				{
					var result = new DrawbackAccountingMethodCodeList();
					if (isTFTEAProvision)
					{
						result.RemoveCode(DrawbackAccountingMethodCodeList.Codes._00);
					}
					else
					{
						result.RemoveCode(DrawbackAccountingMethodCodeList.Codes._01);
						result.RemoveCode(DrawbackAccountingMethodCodeList.Codes._02);
						result.RemoveCode(DrawbackAccountingMethodCodeList.Codes._03);
						result.RemoveCode(DrawbackAccountingMethodCodeList.Codes._04);
						result.RemoveCode(DrawbackAccountingMethodCodeList.Codes._05);
						result.RemoveCode(DrawbackAccountingMethodCodeList.Codes._06);
						result.RemoveCode(DrawbackAccountingMethodCodeList.Codes._07);
						result.RemoveCode(DrawbackAccountingMethodCodeList.Codes._08);
					}

					return result;
				});
			}
		}

		public CodeDescriptionPairList LicencePermitTypes
		{
			get { return LicencePermitTypeList.GetLicencePermitTypeList(Factory); }
		}
	}
}
