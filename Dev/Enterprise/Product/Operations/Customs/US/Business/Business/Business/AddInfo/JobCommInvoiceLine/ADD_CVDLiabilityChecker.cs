using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class ADD_CVDLiabilityChecker
	{
		public enum ADD_CVD { ADD, CVD }

		public static ZString GetCaseNumberPrefix(ADD_CVD add_CVD) => add_CVD == ADD_CVD.ADD ? "A" : "C";

		public void ValidateLiability(ZPropertyInfo caseNumberInfo, IADDCVDLiability invoiceLine, ADD_CVD add_CVD, params USCTariff[] tariffs)
		{
			if (!invoiceLine.IsSetXLine)
			{
				foreach (USCTariff tariff in tariffs)
				{
					if (tariff != null)
					{
						if (add_CVD == ADD_CVD.ADD && !invoiceLine.US_ADD_NA
							|| add_CVD == ADD_CVD.CVD && !invoiceLine.US_CVD_NA)
						{
							var caseNumberPrefix = GetCaseNumberPrefix(add_CVD);

							var validateCountryOfOrigin = invoiceLine.IsCountryOfOriginCanada ? (ZString)Core.Constants.CountryCodes.Canada : invoiceLine.US_UC_NKCountryOfOrigin;

							var caseExists = new USCACCase.Loader(caseNumberInfo.BizObj.Factory).Exists(tariff.UE_Tariff, validateCountryOfOrigin, caseNumberPrefix);

							if (caseExists)
							{
								if (invoiceLine.IsEntrySummaryValidationMode)
								{
									caseNumberInfo.AddMessageError(string.Format(CultureInfo.CurrentCulture, MayBeSubjectToADD_CVD, add_CVD.ToString()));
								}
								else if (invoiceLine.IsLVS)
								{
									caseNumberInfo.AddMessageError(string.Format(CultureInfo.CurrentCulture, MayBeSubjectToADD_CVD_LowValueEntries, add_CVD.ToString()));
								}
								else
								{
									caseNumberInfo.AddWarning(string.Format(CultureInfo.CurrentCulture, MayBeSubjectToADD_CVD, add_CVD.ToString()));
								}
								break;
							}
						}
					}
				}
			}
		}

		public const string MayBeSubjectToADD_CVD = @"This invoice line may be subject to {0}. Please press F4 and use the search facility to review applicable {0} cases.  If this line is not subject to {0}, please indicate this in the '{0} N/A' field.";
		public const string MayBeSubjectToADD_CVD_LowValueEntries = @"This commodity item may be subject to {0}. If this item is not subject to {0}, please indicate this in the '{0} N/A' field.";
	}
}
