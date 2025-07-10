using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using CodeList = Enterprise.Customs.TW.Business;

namespace Enterprise.Customs.TW.Business
{
	public class CusTWControllingMessageHeaderLookups : AutoCusTWControllingMessageHeaderLookups
	{
		public CusTWControllingMessageHeaderLookups(AutoCusTWControllingMessageHeader parent) : base(parent)
		{
		}

		protected new CusTWControllingMessageHeader Parent => (CusTWControllingMessageHeader)base.Parent;

		JobDeclaration Declaration => Parent.EntryInstruction?.JobDeclaration;

		ZBool IsExport => Declaration?.IsExport ?? ZBool.False;

		ZBool IsImport => Declaration?.IsImport ?? ZBool.False;

		public CodeDescriptionPairList ControllingAgencyList => Factory.GetCachedValue($"Enterprise.Customs.TW.Business.CusTWControllingMessageHeaderLookups.ControllingAgencyList|{Parent.TW1_ControllingMessageType}", () =>
		{
			var controllingAgencys = TWRefCusCodeListTypes.GetControllingAgencyCollection(Factory, Parent.TW1_ControllingMessageType);
			var result = new CodeDescriptionPairList();
			controllingAgencys.ForEach(x => result.AddPair(x.ZZD_Code, x.ZZD_Description));
			result.Sort();
			return result;
		});

		public RefUNLOCOCollection PortList => new RefUNLOCOCollection(Factory);

		public CodeDescriptionPairList BusinessTypeList
		{
			get
			{
				var parent = Parent;
				return Factory.GetCachedValue($"Enterprise.Customs.TW.Business.CusTWControllingMessageHeaderLookups.BusinessTypeList|{parent.TW1_ControllingMessageType}|{IsExport}|{IsImport}", () =>
				{
					var result = new CodeDescriptionPairList();
					switch (parent.TW1_ControllingMessageType)
					{
						case CodeList.ControllingMessageTypeList.Codes.NX603:
							result.AddPair(CPT_111_BusinessTypeList.Codes.MedicalInstrument, CPT_111_BusinessTypeList.Descriptions.MedicalInstrument);
							break;
						case CodeList.ControllingMessageTypeList.Codes.NX301_DN:
							result.AddPair(CPT_111_BusinessTypeList.Codes.Inspection, CPT_111_BusinessTypeList.Descriptions.Inspection);
							result.AddPair(CPT_111_BusinessTypeList.Codes.Recheck, CPT_111_BusinessTypeList.Descriptions.Recheck);
							result.AddPair(CPT_111_BusinessTypeList.Codes.ExemptionFromInspection, CPT_111_BusinessTypeList.Descriptions.ExemptionFromInspection);
							break;
						case CodeList.ControllingMessageTypeList.Codes.NX401:
							if (IsExport)
							{
								result.AddPair(CPT_111_BusinessTypeList.Codes.QuarantineOfExportAnimal, CPT_111_BusinessTypeList.Descriptions.QuarantineOfExportAnimal);
								result.AddPair(CPT_111_BusinessTypeList.Codes.QuarantineOfExportPlant, CPT_111_BusinessTypeList.Descriptions.QuarantineOfExportPlant);
							}
							else if (IsImport)
							{
								result.AddPair(CPT_111_BusinessTypeList.Codes.QuarantineOfImportAnimal, CPT_111_BusinessTypeList.Descriptions.QuarantineOfImportAnimal);
								result.AddPair(CPT_111_BusinessTypeList.Codes.QuarantineOfImportPlant, CPT_111_BusinessTypeList.Descriptions.QuarantineOfImportPlant);
							}
							break;
						case CodeList.ControllingMessageTypeList.Codes.NX201_01:
							if (IsExport)
							{
								result.AddPair(NX902_TypeOfApplicationCodeList.Codes._0, NX902_TypeOfApplicationCodeList.Descriptions._0);
								result.AddPair(NX902_TypeOfApplicationCodeList.Codes._1, NX902_TypeOfApplicationCodeList.Descriptions._1);
							}
							else if (IsImport)
							{
								result.AddPair(NX902_TypeOfApplicationCodeList.Codes._0, NX902_TypeOfApplicationCodeList.Descriptions._0);
								result.AddPair(NX902_TypeOfApplicationCodeList.Codes._2, NX902_TypeOfApplicationCodeList.Descriptions._2);
								result.AddPair(NX902_TypeOfApplicationCodeList.Codes._3, NX902_TypeOfApplicationCodeList.Descriptions._3);
							}
							break;
						case CodeList.ControllingMessageTypeList.Codes.NX201_07:
							result.AddPair(NX902_TypeOfApplicationCodeList.Codes._0, NX902_TypeOfApplicationCodeList.Descriptions._0);
							result.AddPair(NX902_TypeOfApplicationCodeList.Codes._2, NX902_TypeOfApplicationCodeList.Descriptions._2);
							result.AddPair(NX902_TypeOfApplicationCodeList.Codes._3, NX902_TypeOfApplicationCodeList.Descriptions._3);
							break;
						default:
							break;
					}
					return result;
				});
			}
		}

		public virtual CodeDescriptionPairList ProcessingUnitList => TWRefCusCodeListTypes.GetProcessingUnitList(Factory, Parent.TW1_ControllingAgency, Parent.EntryInstruction?.DateOfValuation ?? ZDateTime.Today);

		public CodeDescriptionPairList CPT_116_PaymentMethodList => CodeList.CPT_116_PaymentMethodList.GetPaymentMethodList(base.Factory, Parent.TW1_ControllingAgency);

		public CodeDescriptionPairList AppointmentPeriodList => Factory.GetCachedValue($"Enterprise.Customs.TW.Business.AppointmentPeriodList|{Parent.TW1_ControllingMessageType}", () =>
		{
			var parent = Parent;
			var result = new CodeDescriptionPairList();
			if (parent.IsNX401 || parent.IsNX601 || parent.IsNX603 || parent.IsNX301_AX)
			{
				result.AddPair(CPT_113_AppointmentPeriodList.Codes.Between0800And1200, CPT_113_AppointmentPeriodList.Descriptions.Between0800And1200);
				result.AddPair(CPT_113_AppointmentPeriodList.Codes.Between1201And1800, CPT_113_AppointmentPeriodList.Descriptions.Between1201And1800);
				result.AddPair(CPT_113_AppointmentPeriodList.Codes.Between1801And2400, CPT_113_AppointmentPeriodList.Descriptions.Between1801And2400);
				result.AddPair(CPT_113_AppointmentPeriodList.Codes.Other, CPT_113_AppointmentPeriodList.Descriptions.Other);
			}
			else if (parent.IsNX301 || parent.IsNX301_DN)
			{
				result.AddPair(CPT_113_AppointmentPeriodList.Codes.Between0800And1200, CPT_113_AppointmentPeriodList.Descriptions.Between0800And1200);
				result.AddPair(CPT_113_AppointmentPeriodList.Codes.Between1201And1800, CPT_113_AppointmentPeriodList.Descriptions.Between1201And1800);
			}
			return result;
		});

		public PreviousImportedWineInspectionStatusList PreWineInspectionStatusList => Factory.GetCachedValue<PreviousImportedWineInspectionStatusList>();

		public CodeDescriptionPairList PurposeList => Factory.GetCachedValue($"Enterprise.Customs.TW.Business.CusTWControllingMessageHeaderLookups.PurposeList|{Parent.TW1_BusinessType}", () =>
		{
			var businessType = Parent.TW1_BusinessType;
			var result = new CodeDescriptionPairList();
			if (businessType == CPT_111_BusinessTypeList.Codes.Inspection || businessType == CPT_111_BusinessTypeList.Codes.Recheck)
			{
				result = new CPT_117_A_B_PurposeCodeList();
			}
			else if (businessType == CPT_111_BusinessTypeList.Codes.ExemptionFromInspection)
			{
				result = new CPT_117_C_PurposeCodeList();
			}
			return result;
		});

		public CodeDescriptionPairList ControllingMessageTypeList => Factory.GetCachedValue($"Enterprise.Customs.TW.Business.ControllingMessageTypeList|{IsExport}|{IsImport}", () =>
		{
			var result = new CodeDescriptionPairList();
			if (IsExport)
			{
				result.AddPair(CodeList.ControllingMessageTypeList.Codes.NX101, CodeList.ControllingMessageTypeList.Descriptions.NX101);
				result.AddPair(CodeList.ControllingMessageTypeList.Codes.X101, CodeList.ControllingMessageTypeList.Descriptions.X101);
				result.AddPair(CodeList.ControllingMessageTypeList.Codes.NX201_01, CodeList.ControllingMessageTypeList.Descriptions.NX201_01);
				result.AddPair(CodeList.ControllingMessageTypeList.Codes.NX401, CodeList.ControllingMessageTypeList.Descriptions.NX401);
			}
			else if (IsImport)
			{
				result.AddPair(CodeList.ControllingMessageTypeList.Codes.NX201_01, CodeList.ControllingMessageTypeList.Descriptions.NX201_01);
				result.AddPair(CodeList.ControllingMessageTypeList.Codes.NX201_07, CodeList.ControllingMessageTypeList.Descriptions.NX201_07);
				result.AddPair(CodeList.ControllingMessageTypeList.Codes.NX301, CodeList.ControllingMessageTypeList.Descriptions.NX301);
				result.AddPair(CodeList.ControllingMessageTypeList.Codes.NX301_AX, CodeList.ControllingMessageTypeList.Descriptions.NX301_AX);
				result.AddPair(CodeList.ControllingMessageTypeList.Codes.NX301_DN, CodeList.ControllingMessageTypeList.Descriptions.NX301_DN);
				result.AddPair(CodeList.ControllingMessageTypeList.Codes.NX401, CodeList.ControllingMessageTypeList.Descriptions.NX401);
				result.AddPair(CodeList.ControllingMessageTypeList.Codes.NX601, CodeList.ControllingMessageTypeList.Descriptions.NX601);
				result.AddPair(CodeList.ControllingMessageTypeList.Codes.NX603, CodeList.ControllingMessageTypeList.Descriptions.NX603);
			}
			return result;
		});

		public OrgHeaderCollection LocalProcessorOrganisations => fLocalProcessorOrganisations ?? (fLocalProcessorOrganisations = new OrgHeaderCollection(Factory));

		OrgHeaderCollection fLocalProcessorOrganisations;

		public virtual CodeDescriptionPairList CertificateTypeList => Factory.GetCachedValue<CertificateTypeList>();

		public CodeDescriptionPairList EUSteelDeclarationCodeList => Factory.GetCachedValue<EUSteelDeclarationCodeList>();

		public CodeDescriptionPairList EUSteelPhaseCodeList => Factory.GetCachedValue<EUSteelPhaseCodeList>();

		public CodeDescriptionPairList ManufacturerPrintingCodeList
		{
			get
			{
				var certificateType = Parent.TW1_CertificateType;
				return Factory.GetCachedValue($"Enterprise.Customs.TW.Business.CusTWControllingMessageHeaderLookups.ManufacturerPrintingCodeList|{certificateType}", () =>
				{
					CodeDescriptionPairList result;
					switch (certificateType)
					{
						case CodeList.CertificateTypeList.Codes.Code9:
						case CodeList.CertificateTypeList.Codes.Code11:
						case CodeList.CertificateTypeList.Codes.Code13:
						case CodeList.CertificateTypeList.Codes.Code14:
							result = new CPT_123_ManufacturerPrintingCodeList();
							break;
						case CodeList.CertificateTypeList.Codes.Code15:
							result = new CPT_123_15_ManufacturerPrintingCodeList();
							break;
						default:
							result = new CodeDescriptionPairList();
							break;
					}
					return result;
				});
			}
		}

		public CodeDescriptionPairList CPT_123_PrintingCodeList => Factory.GetCachedValue<CPT_123_PrintingCodeList>();

		public CodeDescriptionPairList CPT_127_GoodsReleaseReasonCodeList => Factory.GetCachedValue<CPT_127_GoodsReleaseReasonCodeList>();

		public CodeDescriptionPairList PortOfBulkCommodityList => Factory.GetCachedValue<PortOfBulkCommodityList>();

		public CodeDescriptionPairList LicensingStatusList
		{
			get
			{
				var controllingMessageType = Parent.TW1_ControllingMessageType;
				return Factory.GetCachedValue($"Enterprise.Customs.TW.Business.CusTWControllingMessageHeaderLookups.LicensingStatusList|{controllingMessageType}", () =>
				{
					var result = new CodeDescriptionPairList();
					switch (controllingMessageType)
					{
						case CodeList.ControllingMessageTypeList.Codes.NX101:
							result = Factory.GetCachedValue<NX102ResultCodeList>();
							break;
						case CodeList.ControllingMessageTypeList.Codes.NX201_01:
						case CodeList.ControllingMessageTypeList.Codes.NX201_07:
							result = Factory.GetCachedValue<CPT_120_202_Result>();
							break;
						case CodeList.ControllingMessageTypeList.Codes.NX301:
						case CodeList.ControllingMessageTypeList.Codes.NX301_AX:
						case CodeList.ControllingMessageTypeList.Codes.NX601:
						case CodeList.ControllingMessageTypeList.Codes.NX603:
							result = Factory.GetCachedValue<NX302NX602AuditResultCodeList>();
							break;
						case CodeList.ControllingMessageTypeList.Codes.NX301_DN:
							result = Factory.GetCachedValue<NX302_DNAuditResultCodeList>();
							break;
						case CodeList.ControllingMessageTypeList.Codes.NX401:
							result = Factory.GetCachedValue<CPT_104_QuarantineResultCodeList>();
							break;
						default:
							result = Factory.GetCachedValue<CodeDescriptionPairList>();
							break;
					}
					return result;
				});
			}
		}

		public CodeDescriptionPairList LicensingMessageStatusList => Factory.GetCachedValue<TWMessageStatusCodeList>();
	}
}
