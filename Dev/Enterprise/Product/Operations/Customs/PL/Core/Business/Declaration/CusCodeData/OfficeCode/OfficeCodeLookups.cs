using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business.Declaration;

public class OfficeCodeLookups : EuOfficeCodeLookups
{
	public OfficeCodeLookups(OfficeCode officeCode) : base(officeCode)
	{
	}

	public CodeDescriptionPairList OfficeTypeLookupList
	{
		get
		{
			var purposes = new CodeDescriptionPairList();
			purposes.AddRange(base.CY_CodeList);
			foreach (OfficeCode customsOffice in Parent.Declaration.CustomsOfficesForBinding)
			{
				if (customsOffice.CY_Code != Parent.CY_Code)
				{
					purposes.RemoveCode(customsOffice.CY_Code);
				}
			}
			return purposes;
		}
	}

	protected new OfficeCode Parent => (OfficeCode)base.Parent;
}
