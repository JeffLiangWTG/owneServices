using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Universal
{
	public sealed class RefCusTradeGroupLanguage : AutoRefCusTradeGroupLanguage
	{
		public RefCusTradeGroupLanguage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
