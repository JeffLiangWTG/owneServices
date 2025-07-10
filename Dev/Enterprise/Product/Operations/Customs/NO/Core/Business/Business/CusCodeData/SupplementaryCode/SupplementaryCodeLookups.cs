using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business;

class SupplementaryCodeLookups(SupplementaryCode parent) : BaseSupplementaryCodeLookups(parent)
{
	protected new SupplementaryCode Parent => (SupplementaryCode)base.Parent;

	public override CodeDescriptionPairList CY_CodeList
	{
		get
		{
			var codeList = base.CY_CodeList;
			if (Parent.Parent is JobComInvoiceLine invLine)
			{
				return SupplementaryCodeHelper.GetCodeListBasedOnPackageType(codeList, invLine);
			}

			return codeList;
		}
	}
}
