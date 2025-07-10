using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business;

sealed class ImportJobComInvoiceLineLookups(JobComInvoiceLine parent) : JobComInvoiceLineLookups(parent)
{
	public override ICodeDescriptionPairList PrimaryPreferenceList
	{
		get
		{
			var countryOfOrigin = Parent.JI_CountryOfOrigin;
			var tariff = Parent.JI_Tariff;

			return Factory.GetCachedValue($"NO.JobComInvoiceLineLookups.PrimaryPreferenceList.{countryOfOrigin}.{tariff}", () =>
			{
				if (countryOfOrigin.IsEmpty || tariff.IsEmpty)
				{
					return new PrimaryPreferenceCodeList();
				}

				var list = new CodeDescriptionPairList();
				list.AddRange(base.PrimaryPreferenceList);
				list.RemoveCode(PrimaryPreferenceCodeList.Codes.N);
				list.AddPairIfNotExist(PrimaryPreferenceCodeList.Codes.J, PrimaryPreferenceCodeList.Descriptions.J);
				list.AddPair(PrimaryPreferenceCodeList.Codes.N, PrimaryPreferenceCodeList.Descriptions.N);

				return list;
			});
		}
	}
}
