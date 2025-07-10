using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class BillValidator : Customs.Business.BillValidator
	{
		protected override string GetWarningMessage(ZString billValue, BaseJobDeclaration declaration)
		{
			var result = string.Empty;
			if (!string.IsNullOrEmpty(billValue) || !declaration.IsAir)
			{
				result = base.GetWarningMessage(billValue, declaration);
			}
			return result;
		}
	}
}
