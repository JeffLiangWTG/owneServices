using System;
using CargoWise.Types;
using Enterprise.Customs.SG.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business
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

		#region HazardousMaterialCodeQualifierList

		public override CodeDescriptionPairList HazardousMaterialCodeQualifierList
		{
			get { return Factory.GetCachedValue<DGIndicatorCodeList>(); }
		}

		#endregion

		#region UQ Lists

		public override CodeDescriptionPairList CustomsUQList
		{
			get { return Factory.GetCachedValue<UnitOfQuantityCodeList>(); }
		}

		public override CodeDescriptionPairList InvoiceUQList
		{
			get { return Factory.GetCachedValue<UnitOfQuantityCodeList>(); }
		}

		#endregion

		#region Dutiable UQ List

		public DutiableUQList DutiableUQList
		{
			get { return Factory.GetCachedValue<DutiableUQList>(); }
		}

		#endregion

		#region OriginCriterion List

		public ReadOnlyCodeDescriptionPairList OriginCriterionCodeList
		{
			get
			{
				return Factory.GetCachedValue("OriginCriterionCodeList", () => SGCustomsDataRegistry.Instance.OriginCriterion.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty));
			}
		}

		#endregion

		public CodeDescriptionPairList PreferenceList
		{
			get
			{
				var declarationType = ZString.Empty;
				var declaration = Parent.Declaration;
				if (declaration != null)
				{
					if (declaration.IsImportOnly)
					{
						declarationType = "I";
					}
					else if (declaration.IsExportOnly)
					{
						declarationType = "E";
					}
				}

				return Factory.GetCachedValue("SG|JobComInvoiceLineLookups|PreferenceList|" + declarationType, delegate
				{
					var result = new CodeDescriptionPairList();
					if (declarationType == "I")
					{
						result.AddPair(PreferentialIndicatorCodeList.Codes.PRF, PreferentialIndicatorCodeList.Descriptions.PRF);
						result.AddPair(PreferentialIndicatorCodeList.Codes.STD, PreferentialIndicatorCodeList.Descriptions.STD);
					}
					else if (declarationType == "E")
					{
						result.AddPair(PreferentialIndicatorCodeList.Codes.PRI, PreferentialIndicatorCodeList.Descriptions.PRI);
						result.AddPair(PreferentialIndicatorCodeList.Codes.STD, PreferentialIndicatorCodeList.Descriptions.STD);
					}
					return result;
				});
			}
		}
	}
}
