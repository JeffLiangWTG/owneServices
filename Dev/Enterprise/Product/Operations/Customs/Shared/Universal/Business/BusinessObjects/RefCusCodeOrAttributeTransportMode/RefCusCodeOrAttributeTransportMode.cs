using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal
{
	public sealed class RefCusCodeOrAttributeTransportMode : AutoRefCusCodeOrAttributeTransportMode
	{
		public RefCusCodeOrAttributeTransportMode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
