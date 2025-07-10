using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal
{
	public sealed class RefCusCodeListAttributeNameLanguage : AutoRefCusCodeListAttributeNameLanguage
	{
		public RefCusCodeListAttributeNameLanguage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
