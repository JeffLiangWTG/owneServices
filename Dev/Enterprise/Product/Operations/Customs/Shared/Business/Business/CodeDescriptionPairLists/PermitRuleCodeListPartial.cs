using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public partial class PermitRuleCodeList
	{
		public virtual string GetShortDescriptionFromCode(ZString code)
		{
			return GetDescriptionFromCode(code);
		}
	}
}
