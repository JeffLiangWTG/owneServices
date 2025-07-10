using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class CusEntryLineCollection : Customs.Business.CusEntryLineCollection<CusEntryLine>
	{
		public CusEntryLineCollection(CusEntryHeader entryHeader, BusinessObjectFactory factory)
			: base(entryHeader)
		{
		}

		#region Boolean Properties

		public bool HasMultipleECCN
		{
			get
			{
				ZString eccn = ZString.Empty;
				foreach (CusEntryLine line in this)
				{
					if (eccn.IsEmpty)
					{
						eccn = line.ECCN;
					}
					else
					{
						ZString nextEccn = line.ECCN;
						if (!nextEccn.IsEmpty && eccn != nextEccn)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public bool HasSoftwoodLumberLines
		{
			get
			{
				if (hasSoftwoodLumberLinesCached == null)
				{
					hasSoftwoodLumberLinesCached = new CachedProperty<bool>(Factory, delegate
					{
						foreach (CusEntryLine line in this)
						{
							if (line.IsSoftwoodLumberSection804FarmBillRequirement)
							{
								return true;
							}
						}
						return false;
					});
				}
				return hasSoftwoodLumberLinesCached.Value;
			}
		}
		CachedProperty<bool> hasSoftwoodLumberLinesCached;

		#region DOT / FCC / FDA

		public bool HasPGALinesRequireThreeTimesCustomsValue
		{
			get
			{
				if (hasPGALinesCached == null)
				{
					hasPGALinesCached = new CachedProperty<bool>(Factory, delegate
					{
						foreach (CusEntryLine line in this)
						{
							if (OGAIndicatorList.IsToBeDeclared(line.RandomLine.US_FDAIndicator) ||
								OGAIndicatorList.IsToBeDeclared(line.RandomLine.US_VNEInd) ||
								OGAIndicatorList.IsToBeDeclared(line.RandomLine.US_TSCAInd) ||
								OGAIndicatorList.IsToBeDeclared(line.RandomLine.US_PSTIndicator) ||
								OGAIndicatorList.IsToBeDeclared(line.RandomLine.US_HFCInd) ||
								OGAIndicatorList.IsToBeDeclared(line.RandomLine.US_ODSInd) ||
								OGAIndicatorList.IsToBeDeclared(line.RandomLine.US_ATFInd) ||
								OGAIndicatorList.IsToBeDeclared(line.RandomLine.US_CPSCInd) ||
								OGAIndicatorList.IsToBeDeclared(line.RandomLine.US_AMSInd) ||
								OGAIndicatorList.IsToBeDeclared(line.RandomLine.US_NOPInd))
							{
								return true;
							}
						}
						return false;
					});
				}
				return hasPGALinesCached.Value;
			}
		}
		CachedProperty<bool> hasPGALinesCached;

		#endregion

		#region Visa Quota

		public ZDecimal VisaOrQuotaLinesMerchandiseValue
		{
			get
			{
				if (visaOrQuotaLinesMerchandise == null)
				{
					visaOrQuotaLinesMerchandise = new CachedProperty<ZDecimal>(Factory, delegate
					{
						ZDecimal result = 0m;
						foreach (CusEntryLine line in this)
						{
							if (line.ImportTariff != null &&
							   (line.ImportTariff.UE_QuotaIndicator || line.RandomLine.US_VisaNo.Length > 0))
							{
								result += line.CL_CustomsValue;
							}
						}
						return result;
					}
					);
				}

				return visaOrQuotaLinesMerchandise.Value;
			}
		}
		CachedProperty<ZDecimal> visaOrQuotaLinesMerchandise;

		public ZDecimal NonVisaOrQuotaLinesValuePlusDutiesTaxesAndFees
		{
			get
			{
				if (nonVisaOrQuotaLinesValuePlusDutiesTaxesAndFees == null)
				{
					nonVisaOrQuotaLinesValuePlusDutiesTaxesAndFees = new CachedProperty<ZDecimal>(Factory, delegate
					{
						ZDecimal result = 0m;
						foreach (CusEntryLine line in this)
						{
							if (line.ImportTariff != null &&
							   (!line.ImportTariff.UE_QuotaIndicator && line.RandomLine.US_VisaNo.IsEmpty))
							{
								ZDecimal lineFeesAndDuty = 0;
								foreach (IFee fee in line.Fees)
								{
									lineFeesAndDuty += fee.Amount;
								}

								result += lineFeesAndDuty;
								result += line.CL_CustomsValue;
							}
						}
						return result;
					}
					);
				}

				return nonVisaOrQuotaLinesValuePlusDutiesTaxesAndFees.Value;
			}
		}
		CachedProperty<ZDecimal> nonVisaOrQuotaLinesValuePlusDutiesTaxesAndFees;

		#endregion

		public bool HasMultipleLicenseDetails
		{
			get
			{
				ZString licenseDetails = ZString.Empty;
				foreach (CusEntryLine line in this)
				{
					if (licenseDetails.IsEmpty)
					{
						licenseDetails = line.LicenseNumberAndExemptionCode;
					}
					else
					{
						ZString nextLicenseDetails = line.LicenseNumberAndExemptionCode;
						if (!nextLicenseDetails.IsEmpty && licenseDetails != nextLicenseDetails)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public bool HasLaceyActData
		{
			get
			{
				bool result = false;
				foreach (CusEntryLine line in this)
				{
					if (line.RandomLine != null && line.RandomLine.HasLaceyActData)
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}

		public bool Has98130075Articles
		{
			get
			{
				if (has98130075ArticlesCached == null)
				{
					has98130075ArticlesCached = new CachedProperty<bool>(Factory, delegate
					{
						foreach (CusEntryLine line in this)
						{
							if (line.CL_AdValoremTariff.StartsWith("98130075"))
							{
								return true;
							}
						}
						return false;
					});
				}
				return has98130075ArticlesCached.Value;
			}
		}
		CachedProperty<bool> has98130075ArticlesCached;

		#endregion

		protected override System.Collections.IComparer GetComparer()
		{
			return new CusEntryLineComparer();
		}

		public CusEntryLine FindNonSecondaryLineByFormattedLineNumber(ZString lineNumber)
		{
			foreach (CusEntryLine entryLine in this)
			{
				if (entryLine.CL_LineNumberFormatted == lineNumber)
				{
					if (entryLine.ParentLine == null || entryLine.IsSetVLine)
					{
						return entryLine;
					}
				}
			}
			return null;
		}

		public CusEntryLine FindNonSecondaryLineByLineNumber(ZInt lineNumber)
		{
			foreach (CusEntryLine entryLine in this)
			{
				if (entryLine.CL_LineNumber == lineNumber)
				{
					if (entryLine.ParentLine == null || entryLine.IsSetVLine)
					{
						return entryLine;
					}
				}
			}
			return null;
		}

		public CusEntryLine FindByFormattedLineNumber(ZString lineNumber)
		{
			foreach (CusEntryLine entryLine in this)
			{
				if (entryLine.CL_LineNumberFormatted == lineNumber)
				{
					return entryLine;
				}
			}
			return null;
		}
	}
}
