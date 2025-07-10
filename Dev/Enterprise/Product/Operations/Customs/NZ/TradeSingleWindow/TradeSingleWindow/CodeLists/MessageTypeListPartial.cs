using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.TradeSingleWindow
{
	partial class MessageTypeList
	{
		public static bool IsTSWCode(BusinessObjectFactory factory, string code)
		{
			return factory.GetCachedValue<MessageTypeList>().ContainsCode(code);
		}
	}
}
