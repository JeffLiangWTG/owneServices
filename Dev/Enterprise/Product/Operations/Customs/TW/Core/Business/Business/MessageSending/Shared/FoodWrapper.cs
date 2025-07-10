using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class FoodWrapper : IFood
	{
		public FoodWrapper(ZDecimal? phValueNumeric, ZDecimal? sterilizationValueNumeric, IEnumerable<IFoodConstituent> constituents)
		{
			PHValueNumeric = phValueNumeric;
			SterilizationValueNumeric = sterilizationValueNumeric;
			Constituents = constituents;
		}

		public ZDecimal? PHValueNumeric { get; }

		public ZDecimal? SterilizationValueNumeric { get; }

		public IEnumerable<IFoodConstituent> Constituents { get; }
	}
}
