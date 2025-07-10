//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoExportAWBAccountingInformationLookups
//
//    This class should be used for overriding collections in AutoExportAWBAccountingInformationLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	public class ExportAWBAccountingInformationLookups : Forwarding.AWB.Business.ExportAWBAccountingInformationLookups
	{
		public ExportAWBAccountingInformationLookups(ExportAWBAccountingInformation parent)
			: base(parent)
		{
		}

		protected override CodeDescriptionPairList GetNewAccountingCodesList()
		{
			var result = base.GetNewAccountingCodesList();

			for (int i = result.Count - 1; i >= 0; i--)
			{
				if (!accountCodes.Contains(result[i].Code))
				{
					result.RemoveAt(i);
				}
			}

			if (GlbCompany.CurrentCompany != null && GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Italy)
			{
				result.AddPair(IssuedByIVA, Res.GetString("fc1bac00-a79a-4f78-a0b7-bad11dec8c28", "IVA of Issued By (For Italian AWBs only)"));
				result.AddPair(ShipperCodiceFiscaleOrIVA, Res.GetString("2fb349c8-3e79-4774-a47d-81d547ea4f40", "Shipper's {0}/IVA (For Italian AWBs only)", "Codice Fiscale")); // Italian words
			}

			return result;
		}

		public const string ShipperCodiceFiscaleOrIVA = "SIV";
		public const string IssuedByIVA = "IIV";

		readonly HashSet<string> accountCodes = [
			Core.Constants.AWB.AccountingCodes.GBL,
			Core.Constants.AWB.AccountingCodes.RET,
			Core.Constants.AWB.AccountingCodes.GEN,
			Core.Constants.AWB.AccountingCodes.MCO,
			Core.Constants.AWB.AccountingCodes.SRN,
			Core.Constants.AWB.AccountingCodes.STL
			];
	}
}
