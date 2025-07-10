using System.Linq;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class TaxFrameworkAccTaxRateLookups : AccTaxRateLookups
	{
		public TaxFrameworkAccTaxRateLookups(TaxFrameworkAccTaxRate parent) : base(parent)
		{
		}

		public CodeDescriptionPairList TaxSystems
		{
			get
			{
				var types = new CodeDescriptionPairList();
				var taxSystemsConfigurations = AccountingMasterFilesRegistry.Instance.TaxSystems.Value.Cast<TaxSystemsConfiguration>().Where(x => x.Country == Parent.AT_RN_NKCountry);
				foreach (var taxSystemsConfiguration in taxSystemsConfigurations)
				{
					types.AddPair(taxSystemsConfiguration.Code, taxSystemsConfiguration.Name);
				}
				return types;
			}
		}

		public CodeDescriptionPairList RateSources => new AccountingMasterFilesTaxFrameworkConstants.TaxRateSources();

		new TaxFrameworkAccTaxRate Parent
		{
			get { return (TaxFrameworkAccTaxRate)base.Parent; }
		}
	}
}