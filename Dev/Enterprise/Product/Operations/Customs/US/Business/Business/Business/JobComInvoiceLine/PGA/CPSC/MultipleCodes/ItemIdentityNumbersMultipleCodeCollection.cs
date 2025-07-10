using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class ItemIdentityNumbersMultipleCodeCollection : CommaSeparatedNumberCollection
	{
		public ItemIdentityNumbersMultipleCodeCollection(ZString commaSeparatedCodes, BusinessObjectFactory factory)
			: base(commaSeparatedCodes, factory)
		{
		}

		public override CommaSeparatedNumber CreatePersistentBusinessObjectCore()
		{
			return new ItemIdentityNumbersMultipleCode();
		}
	}
}
