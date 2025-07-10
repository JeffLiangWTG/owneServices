using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.CusReconBase;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public sealed class ConsolidatedDeclarationEntry : AutoCusReconEntry
	{
		public const string EntryTypeForConsolidateDeclaration = "CSD";

		public ConsolidatedDeclarationEntry(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CRE_EntryType = EntryTypeForConsolidateDeclaration;
			CRE_GB_Branch = GlbBranch.CurrentBranch.PK;
		}
	}
}
