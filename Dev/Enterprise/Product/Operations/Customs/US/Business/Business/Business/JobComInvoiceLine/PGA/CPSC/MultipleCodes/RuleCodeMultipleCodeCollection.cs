using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class RuleCodeMultipleCodeCollection : CommaSeparatedNumberCollection
	{
		public RuleCodeMultipleCodeCollection(ZString commaSeparatedCodes, BusinessObjectFactory factory)
			: base(commaSeparatedCodes, factory)
		{
		}

		public override CommaSeparatedNumber CreatePersistentBusinessObjectCore()
		{
			return new RuleCodeMultipleCode();
		}
	}
}
