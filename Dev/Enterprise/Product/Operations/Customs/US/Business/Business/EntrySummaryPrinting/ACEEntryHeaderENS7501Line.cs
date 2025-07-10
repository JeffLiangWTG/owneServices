using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.EntrySummaryPrinting
{
	public class ACEEntryHeaderENS7501Line : EntryHeaderENS7501Line
	{
		public ACEEntryHeaderENS7501Line(CusEntryLine line, bool printInvoiceHeading, bool printInvoiceDetails)
			: base(line, printInvoiceHeading, printInvoiceDetails)
		{
		}

		protected override string GetADCVDRateDescriptionFromCaseRecord(ZString caseNo, string rateType, ZDecimal depositRateOverride)
		{
			var result = string.Empty;
			var adCase = (USCACCase)GetADCVDCaseRecord(caseNo);
			var adCaseRate = DepositRateIndicatorList.GetUSCACCaseRate(adCase, DateForAD_CVD);
			if (!DepositRateIndicatorList.DepositRateIndContainOverride(rateType))
			{
				result = DepositRateIndicatorList.GetRateDescriptionFromCaseRateRecord(adCaseRate, rateType);
			}
			else
			{
				result = DepositRateIndicatorList.GetDepositRateDescription(rateType, null, depositRateOverride, adCaseRate);
			}

			return result;
		}

		protected override IACCase GetADCVDCaseRecord(ZString caseNo)
		{
			return Factory.LoadFromNaturalKey<USCACCase>(USCACCaseSchema.U5_CaseNumber, caseNo);
		}

		public override ZString SPIAndOrSecondarySPI
		{
			get
			{
				var result = base.SPIAndOrSecondarySPI;

				var articleSetIndicator = ((IACECusEntryLine)line).ArticleSetIndicator;
				if (!articleSetIndicator.IsEmpty)
				{
					result += (!result.IsEmpty ? "," : "") + articleSetIndicator;
				}
				return result;
			}
		}

		bool ShouldAdjustLinePrice
		{
			get
			{
				var aceCusEntryLine = line as IACECusEntryLine;
				return aceCusEntryLine != null
					&& !aceCusEntryLine.SecondaryTariffLines.Any(x => x.IsCombineLine)
					&& aceCusEntryLine.IsSupLine
					&& SecondaryTariffLine
					&& new SecondaryTariffLineWrapper(secondaryTariffLine1).ValueInUSD.Round(0).IsEmpty;
			}
		}

		public override ZDecimal TotalLinePriceInLocalCurrencyRounded
		{
			get
			{
				if (ShouldAdjustLinePrice)
				{
					return ZDecimal.Zero;
				}
				else
				{
					return base.TotalLinePriceInLocalCurrencyRounded;
				}
			}
		}

		public override ZDecimal SecondaryLine1TotalLinePriceInLocalCurrencyRounded
		{
			get
			{
				if (ShouldAdjustLinePrice)
				{
					return base.TotalLinePriceInLocalCurrencyRounded;
				}
				else
				{
					return base.SecondaryLine1TotalLinePriceInLocalCurrencyRounded;
				}
			}
		}

		public override ZString LicenseNumber
		{
			get
			{
				if (!licenseNumberCached.HasValue)
				{
					licenseNumberCached = string.Join("\r\n", GetAllLicenceAndPermits(line.RandomLine).Where(x => !x.CY_Data.IsEmpty).Select(x => x.CY_Code + "-" + x.CY_Data.ToUpper()));
				}
				return licenseNumberCached.Value;
			}
		}
		ZString? licenseNumberCached;

		ZString GetLicenseNumber(bool hasTariffLine, CusEntryLine tariffLine)
		{
			var result = ZString.Empty;
			if (hasTariffLine && tariffLine.IsSupLineOrNormalLine)
			{
				result = tariffLine.RandomLine.LicenceAndPermits.Cast<Messaging.Business.MessageBuildingBlocks.IACELicenceAndPermit>().FirstOrDefault()?.LicenseNumberCertificateNumberPermitNumber ?? ZString.Empty;
			}
			return result;
		}

		public override ZString SecondaryLine1LicenseNumber
		{
			get { return GetLicenseNumber(SecondaryTariffLine, secondaryTariffLine1); }
		}

		public override ZString SecondaryLine2LicenseNumber
		{
			get { return GetLicenseNumber(SecondaryTariffLine2, secondaryTariffLine2); }
		}

		public override ZString SecondaryLine3LicenseNumber
		{
			get { return GetLicenseNumber(SecondaryTariffLine3, secondaryTariffLine3); }
		}

		public override ZString SecondaryLine4LicenseNumber
		{
			get { return GetLicenseNumber(SecondaryTariffLine4, secondaryTariffLine4); }
		}

		public override ZString SecondaryLine5LicenseNumber
		{
			get { return GetLicenseNumber(SecondaryTariffLine5, secondaryTariffLine5); }
		}

		public override ZString SecondaryLine6LicenseNumber
		{
			get { return GetLicenseNumber(SecondaryTariffLine6, secondaryTariffLine6); }
		}

		public override ZString SecondaryLine7LicenseNumber
		{
			get { return GetLicenseNumber(SecondaryTariffLine7, secondaryTariffLine7); }
		}

		protected override ZString CottonCertificateNumberOrganicExemptionCertificateNumber
		{
			get
			{
				var permitAndLicense = GetAllLicenceAndPermits(line.RandomLine);

				var licences = from LicenceAndPermit lic in permitAndLicense
							   where LicencePermitTypeList.DeclareInCottonOrganicExemptionFieldInACS(lic.CY_Code)
							   select lic.CY_Data;

				return licences.Any() ? "C " + new ZStringBuilder(licences).ToStringWithDelimiterBetweenAppends(", ") : "";
			}
		}

		List<LicenceAndPermit> GetAllLicenceAndPermits(JobComInvoiceLine invoiceLine)
		{
			var permitAndLicense = invoiceLine.LicenceAndPermits.Cast<LicenceAndPermit>().ToList();
			if (invoiceLine.IsVParentLine)
			{
				foreach (var child in invoiceLine.ChildVLines)
				{
					permitAndLicense.AddRange(child.LicenceAndPermits.Cast<LicenceAndPermit>());
				}
			}
			return permitAndLicense;
		}

		protected override ZString CanadianExportCertificateSugar
		{
			get { return GetCertificateNumber(LicencePermitTypeList.Codes._16); }
		}

		protected override ZString CBTPACertificationNumber
		{
			get { return GetCertificateNumber(LicencePermitTypeList.Codes._18); }
		}

		ZString GetCertificateNumber(ZString licenceCode)
		{
			var allLicencePermits = GetAllLicenceAndPermits(line.RandomLine);

			var licence = allLicencePermits.FirstOrDefault(lic => lic.CY_Code == licenceCode);

			return licence != null ? "C " + licence.CY_Data : "";
		}
	}
}
