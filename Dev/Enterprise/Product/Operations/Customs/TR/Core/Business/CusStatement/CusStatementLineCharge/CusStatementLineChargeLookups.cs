using System.Collections.Generic;
using System.Linq;
using CargoWise.Integration;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business
{
	public class CusStatementLineChargeLookups : Customs.Business.CusStatementLineChargeLookups
	{
		public CusStatementLineChargeLookups(AutoCusStatementLineCharge parent) : base(parent)
		{
		}

		public new CusStatementLineCharge Parent => (CusStatementLineCharge)base.Parent;

		public CodeDescriptionPairList TaxOrFeeCodeList
		{
			get
			{
				var taxOrFeeCodeList = UniversalReferenceDataHelper.GetTaxTypeList(Factory);
				var entryType = Parent.StatementLine?.B3_EntryType.ToString();

				IEnumerable<ICodeDescription> filteredList;

				if (entryType == "MAN")
				{
					filteredList = taxOrFeeCodeList.Cast<ICodeDescription>().Where(c =>
						c.Code == "ABS" || c.Code == "GMS" || c.Code == "OBS" || c.Code == "SBS");
				}
				else
				{
					filteredList = taxOrFeeCodeList.Cast<ICodeDescription>().Where(c => c.Code == "89");
				}

				var result = new CodeDescriptionPairList();
				result.AddRange(filteredList.ToList());
				return result;
			}
		}
	}
}
