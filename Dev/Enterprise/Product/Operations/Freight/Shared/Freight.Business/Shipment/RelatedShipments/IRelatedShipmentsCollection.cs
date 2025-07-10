using System.Collections;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public interface IRelatedShipmentsCollection : IEnumerable
	{
		FilterBusinessObjectDefaults FilterBusinessObjectDefaults { get; }

		ZQuery AdditionalFilter { get; }
	}
}
