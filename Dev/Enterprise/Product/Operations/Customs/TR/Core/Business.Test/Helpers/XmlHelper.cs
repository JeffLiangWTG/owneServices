
namespace Enterprise.Customs.TR.Business.Testing.Helpers
{
	public static class XmlHelper
	{
		public static string IgnoreXmlnsAttrOrder(string target)
		{
#if NETFRAMEWORK
			return target;
#else
			return target.Replace("xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"");
#endif
		}
	}
}
