using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	public sealed class TWManifestXmlTestCaseWithFactory : TestCaseWithFactory
	{
		public static ZString GetExpectedMessageXML(ZString path)
		{
			using (var inStream = typeof(TWManifestXmlTestCaseWithFactory).Assembly.GetManifestResourceStream(path))
			{
				return new StreamReader(inStream).ReadToEnd();
			}
		}
	}
}
