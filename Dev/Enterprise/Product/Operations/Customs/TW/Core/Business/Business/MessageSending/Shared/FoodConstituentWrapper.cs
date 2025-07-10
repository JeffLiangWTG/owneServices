using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class FoodConstituentWrapper : IFoodConstituent
	{
		public FoodConstituentWrapper(ZString elementName, ZDecimal elementPercentNumeric)
		{
			ElementName = elementName;
			ElementPercentNumeric = elementPercentNumeric;
		}

		public ZString ElementName { get; }

		public ZDecimal ElementPercentNumeric { get; }
	}
}
