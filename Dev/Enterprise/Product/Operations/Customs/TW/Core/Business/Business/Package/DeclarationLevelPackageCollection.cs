using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class DeclarationLevelPackageCollection : BaseDeclarationLevelPackageCollection<Package>
	{
		public DeclarationLevelPackageCollection(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override void SetDefaultsForFirstPackage(Package firstPackage)
		{
			base.SetDefaultsForFirstPackage(firstPackage);

			firstPackage.CW_NetWeightUQ = Core.Constants.Weight.Kilograms;
			firstPackage.CW_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			var declaration = firstPackage.Declaration;
			if (declaration != null)
			{
				firstPackage.CW_PackType = declaration.JE_TotalNoOfPacksPackType;
			}
		}
	}
}
