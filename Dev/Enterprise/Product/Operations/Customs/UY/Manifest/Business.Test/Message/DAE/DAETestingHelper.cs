using System.IO;
using System.Xml;
using CargoWise.Types;

namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	public static class DAETestingHelper
	{
		public static ZString GetExpectedMessageXML(ZString path)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(path);

			return doc.OuterXml;
		}

		public static ZString GetExpectedMessageTXT(ZString path)
		{
			StreamReader sr = new StreamReader(path);
			return sr.ReadToEnd();
		}
	}
}
