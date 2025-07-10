using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.US.Business
{
	public abstract class USFSISLine : AutoUSFSISLine
	{
		public USFSISLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			RefreshWhenElectronicallyExchangedChanged();
		}

		public override ZString US_UC_NKCertificateIssuerCountry
		{
			get { return base.US_UC_NKCertificateIssuerCountry; }
			set
			{
				var isElectronicallyCertificatedOld = IsElectronicallyCertificated;
				base.US_UC_NKCertificateIssuerCountry = value;
				if (isElectronicallyCertificatedOld != IsElectronicallyCertificated)
				{
					ClearPropertiesWhenElectronicallyCertificated();
					RefreshWhenElectronicallyExchangedChanged();
				}
			}
		}

		void ClearPropertiesWhenElectronicallyCertificated()
		{
			if (IsElectronicallyCertificated)
			{
				US_ExportingEstNo = ZString.Empty;
				US_ProductID = ZString.Empty;
				US_ProductIDQualifier = ZString.Empty;
				US_IntendedUseCode = ZString.Empty;
			}
		}

		void RefreshWhenElectronicallyExchangedChanged()
		{
			US_ExportingEstNoInfo.RefreshBinding();
			US_ProductIDInfo.RefreshBinding();
			US_ProductIDQualifierInfo.RefreshBinding();
			US_IntendedUseCodeInfo.RefreshBinding();
		}

		public new USFSISLineValidation Validation
		{
			get { return (USFSISLineValidation)base.Validation; }
		}

		protected override CusAddInfoValidation GetNewValidation()
		{
			return new USFSISLineValidation(this);
		}

		protected override ZString HumanReadableNameCore
		{
			get { return "Certificate"; }
		}

		ZBool GetCertificatedCountryWithChile(string countryCode)
		{
			return countryCode switch
			{
				Core.Constants.CountryCodes.Australia or Core.Constants.CountryCodes.NewZealand or Core.Constants.CountryCodes.Netherlands or Core.Constants.CountryCodes.Chile => ZBool.True,
				_ => ZBool.False,
			};
		}

		ZBool GetCertificatedCountry(string countryCode)
		{
			return countryCode switch
			{
				Core.Constants.CountryCodes.Australia or Core.Constants.CountryCodes.NewZealand or Core.Constants.CountryCodes.Netherlands => ZBool.True,
				_ => ZBool.False,
			};
		}

		public ZBool IsElectronicallyCertificated => ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Universal.Constants.FunctionalityTypes.USCLeCERT, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today) ? GetCertificatedCountryWithChile(US_UC_NKCertificateIssuerCountry) : GetCertificatedCountry(US_UC_NKCertificateIssuerCountry);

		protected void ClearValuesAfterClone(USFSISLine result)
		{
			result.US_AdditionalSpecies = ZString.Empty;
			result.US_DateOfInspection = ZDateTime.Empty;
			result.US_HealthCertificateNumber = ZString.Empty;
			result.US_SealNumbers = ZString.Empty;
		}
	}
}
