using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module
{
	public class ReconDeclarationFilterLookups
	{
		public ReconDeclarationFilterLookups(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		public ConsigneeCollection Importers => new ConsigneeCollection(factory);

		public ReconIssueCodeList IssueCodeList => factory.GetCachedValue<ReconIssueCodeList>();

		public ReconPortsList ReconPortsList => factory.GetCachedValue<ReconPortsList>();

		public PaymentTypeList PaymentTypeList => PaymentTypeList.GetCachedReconPaymentTypeList(factory);

		public GlbStaffCollection StaffList => new GlbStaffCollection(factory);

		public ReconMessageStatusList ReconMessageStatusList => ReconMessageStatusList.GetCachedReconMessageStatusList(factory);

		public GlbBranchCollection BranchList => new GlbBranchCollection(factory);

		public CodeDescriptionPairList DISStatusList
		{
			get
			{
				return factory.GetCachedValue<CodeDescriptionPairList>("Recon|DISStatusList", delegate
				{
					var result = new Common.US.DIS.StatusList();
					result.RemoveCode(Common.US.DIS.StatusList.Codes.MUL);
					return result;
				});
			}
		}

		public CodeDescriptionPairList EntrySummaryActionsList
		{
			get
			{
				return factory.GetCachedValue("EntrySummaryActionsList", delegate
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(DeclarationFilterConstants.ALL, "All Records");
					result.AddPair(DeclarationFilterConstants.Incomplete, "All with Action Incomplete");
					return result;
				});
			}
		}
	}
}
