
namespace Enterprise.Customs.TR.Messaging.Testing
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

		public static string GetNamespacePrefix()
		{
#if NETFRAMEWORK
			return "q1:";
#else
			return string.Empty;
#endif
		}

		public static string GetNamespaceSuffix()
		{
#if NETFRAMEWORK
			return ":q1";
#else
			return string.Empty;
#endif
		}
	}
}
