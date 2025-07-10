using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class AMSDocAddressDependentCollection : JobDocAddressDependentCollection
	{
		public AMSDocAddressDependentCollection(IDocAddresses parent)
			: base(parent)
		{ }

		protected override BusinessObject AddNewCore()
		{
			var result = (JobDocAddress)base.AddNewCore();
			result.AdditionalValidation = new ACEOceanManifestJobDocAddressValidation(result);
			return result;
		}
	}
}
