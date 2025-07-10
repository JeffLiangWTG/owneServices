using CargoWise.EntityFramework;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class CusClassPartPivotLookups : Customs.Business.CusClassPartPivotLookups
	{
		public CusClassPartPivotLookups(CusClassPartPivot parent)
			: base(parent)
		{
		}

		public new CusClassPartPivot Parent
		{
			get { return (CusClassPartPivot)base.Parent; }
		}

		public override CodeDescriptionPairList ClassificationTypes
		{
			get
			{
				CodeDescriptionPairList result;

				if (Parent.CI_CI_Parent.IsEmpty)
				{
					result = Factory.GetCachedValue<ClassificationTypeList>();
				}
				else
				{
					result = Factory.GetCachedValue<ClassificationChildTypeList>();
				}

				return result;
			}
		}

		public ZoneStatusList ZoneStatuses => Factory.GetCachedValue<ZoneStatusList>();

		public ConsignorCollection Manufacturers
		{
			get { return new ConsignorCollection(Factory); }
		}

		public ConsignorCollection Exporters
		{
			get { return new ConsignorCollection(Factory); }
		}

		public USCACCaseCollection ADDCaseNumberList
		{
			get { return new ACEADD_CVDListLoader(Parent.Factory).GetCaseNumberList(Parent.CD_UC_NKCountryOfOrigin, Parent.CD_ADDCaseNo, "A", Parent.TariffNumber, Parent.CI_SupplementalTariff); }
		}

		public USCACCaseCollection CVDCaseNumberList
		{
			get { return new ACEADD_CVDListLoader(Parent.Factory).GetCaseNumberList(Parent.CD_UC_NKCountryOfOrigin, Parent.CD_CVDCaseNo, "C", Parent.TariffNumber, Parent.CI_SupplementalTariff); }
		}

		public Customs.Business.BaseClassificationCollection<CusClassification> Classifications
		{
			get
			{
				return Parent.UseHTSClassification
				? HTSClassificationList
				: SCHBClassificationList;
			}
		}

		#region GuidFindBox Collection

		ExportClassificationCollection SCHBClassificationList => new ExportClassificationCollection(Factory);

		ImportClassificationCollection HTSClassificationList => new ImportClassificationCollection(Factory);

		#endregion

		#region Tariffs

		public override BusinessObjectCollection Tariffs
		{
			get
			{
				if (Parent.UseHTSClassification)
				{
					if (Parent.IsHTE)
					{
						return HTEList;
					}
					else
					{
						return HTSList;
					}
				}
				else
				{
					return SHBList;
				}
			}
		}

		USCTariffCollection HTSList
		{
			get
			{
				var htsList = new USCTariffCollection(Factory);
				if (!Parent.CI_FormattedTariffNum.IsEmpty)
				{
					htsList.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(USCTariff.FilterSchema.Tariff, "Property", Parent.CI_FormattedTariffNum));
				}

				return htsList;
			}
		}

		Universal.TariffViewCollection SHBList => Factory.GetTariffs(Universal.Constants.TariffTypes.ScheduleB, Parent.CI_TariffNum, Parent.EffectiveDate);

		Universal.TariffViewCollection HTEList => Factory.GetTariffs(Universal.Constants.TariffTypes.Export, Parent.CI_TariffNum, Parent.EffectiveDate);

		#endregion

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

		public CodeDescriptionPairList AMSDisclaimProgramList
		{
			get
			{
				return Factory.GetCachedValue("AMSDisclaimProgramList",
					delegate
					{
						var result = new CodeDescriptionPairList();
						result.AddPair(AMSProgramList.Codes.MO8, AMSProgramList.Descriptions.MO8);
						return result;
					}
				);
			}
		}
		public CodeDescriptionPairList PSTDisclaimProgramList
		{
			get { return Factory.GetCachedValue<PSTProductTypeList>(); }
		}

		public CodeDescriptionPairList NMFS370DisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonListWithoutTransactionalCodes(Parent.OGARequirementCalculator.NMFS370RequirementCode, Parent.Factory, false, string.Empty); }
		}

		public CodeDescriptionPairList NMFSAMRDisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonListWithoutTransactionalCodes(Parent.OGARequirementCalculator.NMFSAMRRequirementCode, Parent.Factory, false, string.Empty); }
		}

		public CodeDescriptionPairList NMFSHMSDisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonListWithoutTransactionalCodes(Parent.OGARequirementCalculator.NMFSHMSRequirementCode, Parent.Factory, false, string.Empty); }
		}

		public CodeDescriptionPairList TTBDisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonListWithoutTransactionalCodes(Parent.OGARequirementCalculator.TTBRequirementCode, Factory, false, string.Empty); }
		}

		public CodeDescriptionPairList CPSCDisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonListWithoutTransactionalCodes(Parent.OGARequirementCalculator.CPSCRequirementCode, Factory, false, string.Empty); }
		}

		public CodeDescriptionPairList APHISDisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonListWithoutTransactionalCodes(Parent.OGARequirementCalculator.APHISRequirementCode, Factory, false, string.Empty); }
		}

		public CodeDescriptionPairList FWSDisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonListWithoutTransactionalCodes(Parent.OGARequirementCalculator.FWSRequirementCode, Factory, false, string.Empty); }
		}

		public CodeDescriptionPairList OMCDisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonListWithoutTransactionalCodes(Parent.OGARequirementCalculator.OMCRequirementCode, Factory, false, string.Empty); }
		}

		public CodeDescriptionPairList FDADisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonListWithoutTransactionalCodes(Parent.OGARequirementCalculator.ACEFDARequirementCode, Factory, false, string.Empty); }
		}

		public CodeDescriptionPairList NOPDisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonListWithoutTransactionalCodes(Parent.OGARequirementCalculator.NOPRequirementCode, Factory, false, string.Empty); }
		}

		public CodeDescriptionPairList AMSDisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonListWithoutTransactionalCodes(Parent.OGARequirementCalculator.AMSRequirementCode, Factory, false, string.Empty); }
		}

		public CodeDescriptionPairList DEADisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonListWithoutTransactionalCodes(Parent.OGARequirementCalculator.DEARequirementCode, Factory, false, string.Empty); }
		}

		public CodeDescriptionPairList PSTDisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonListWithoutTransactionalCodes(Parent.OGARequirementCalculator.PSTRequirementCode, Parent.Factory, false, string.Empty); }
		}

		public CodeDescriptionPairList HFCDisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonListWithoutTransactionalCodes(Parent.OGARequirementCalculator.HFCRequirementCode, Parent.Factory, false, string.Empty); }
		}

		public CodeDescriptionPairList VNEDisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonListWithoutTransactionalCodes(Parent.OGARequirementCalculator.VNERequirementCode, Parent.Factory, false, string.Empty); }
		}

		public CodeDescriptionPairList ODSDisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonListWithoutTransactionalCodes(Parent.OGARequirementCalculator.ODSRequirementCode, Parent.Factory, false, string.Empty); }
		}

		public CodeDescriptionPairList FSISDisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonListWithoutTransactionalCodes(Parent.OGARequirementCalculator.FSISRequirementCode, Parent.Factory, false, string.Empty); }
		}

		public CodeDescriptionPairList TSCADisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonListWithoutTransactionalCodes(Parent.OGARequirementCalculator.TSCARequirementCode, Parent.Factory, false, string.Empty); }
		}

		public CodeDescriptionPairList NHTSADisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonListWithoutTransactionalCodes(Parent.OGARequirementCalculator.NHTSARequirementCode, Parent.Factory, false, string.Empty); }
		}

		public CodeDescriptionPairList LaceyDisclaimReasonList
		{
			get { return Enterprise.Customs.US.Business.PGADisclaimReasonList.GetDisclaimReasonListWithoutTransactionalCodes(Parent.OGARequirementCalculator.ACELaceyRequirementCode, Parent.Factory, false, string.Empty); }
		}
	}
}
