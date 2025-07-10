using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class NMFSHarvestingDetailValidation : Customs.Business.MultiLineAddInfos.CusAddInfoValidation
	{
		public NMFSHarvestingDetailValidation(NMFSHarvestingDetail harvestingDetail)
			: base(harvestingDetail)
		{
		}

		new NMFSHarvestingDetail Parent
		{
			get { return (NMFSHarvestingDetail)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateUS_FirstLandingCountry();
		}

		public void ValidateUS_FirstLandingCountry()
		{
			ValidateCalculatedProperty(Parent.US_FirstLandingCountryInfo);
		}

		protected void CheckUS_FirstLandingCountry()
		{
			if (IsSIMPPgaRequiredValidation && NMFSLine.RequiresFullData)
			{
				if (Parent.US_FirstLandingCountry != "ZZ")
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.US_FirstLandingCountryInfo, Parent.AddInfoLookups.Countries);
				}

				if (!Parent.US_FirstLandingCountry_ReadOnly)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_FirstLandingCountryInfo);
				}
			}
		}

		NMFSLine NMFSLine
		{
			get
			{
				var harvestingDetail = Parent;
				return harvestingDetail == null ? null : harvestingDetail.Parent;
			}
		}

		bool IsSIMPPgaRequiredValidation
		{
			get
			{
				var result = false;

				var nmfsLine = NMFSLine;
				if (nmfsLine != null && nmfsLine.IsSIMProgramType)
				{
					var invoiceLine = nmfsLine.InvoiceLine;
					var declaration = invoiceLine != null ? invoiceLine.Declaration : null;
					result = declaration != null && declaration.IsPGAValidationOn();
				}
				return result;
			}
		}
	}
}
