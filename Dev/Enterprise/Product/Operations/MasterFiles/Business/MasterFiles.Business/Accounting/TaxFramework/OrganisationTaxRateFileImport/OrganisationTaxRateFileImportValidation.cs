using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrganisationTaxRateFileImportValidation : ZValidation
	{
		public OrganisationTaxRateFileImportValidation(OrganisationTaxRateFileImport parent)
				: base(parent)
		{
			Parent = parent;
		}

		protected readonly OrganisationTaxRateFileImport Parent;

		public override Type AutoValidationType => GetType();
		public override void ValidateAll()
		{
			ValidateTaxConfiguration();
			ValidateRateSource();
		}

		public void ValidateTaxConfiguration() => ValidateCalculatedProperty(Parent.TaxConfigurationInfo);
		public void ValidateRateSource() => ValidateCalculatedProperty(Parent.RateSourceInfo);

		protected void CheckTaxConfiguration()
		{
			MandatoryValidation.CheckEntered(Parent.TaxConfigurationInfo);
			if (!Parent.TaxConfigurationInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidPK(Parent.TaxConfigurationInfo);
			}
		}

		protected void CheckRateSource()
		{
			MandatoryValidation.CheckEntered(Parent.RateSourceInfo);
			if (!Parent.RateSourceInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(Parent.RateSourceInfo);
			}
		}
	}
}
