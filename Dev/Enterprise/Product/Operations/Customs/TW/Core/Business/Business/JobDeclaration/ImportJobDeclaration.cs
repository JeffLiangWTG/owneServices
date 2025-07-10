using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class ImportJobDeclaration : Customs.Business.ImportJobDeclaration
	{
		public ImportJobDeclaration(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override void PerformCountrySpecificImporting()
		{
			base.PerformCountrySpecificImporting();

			if (GlbBranch.CurrentBranch.OrgProxy?.MainAddress is OrgAddress address && address.IsInDatabase)
			{
				ImportedDeclaration.JE_OA_DeclarantAddress = address.PK;
			}
		}
	}
}
