using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(MDMSupportCertificateRegistryItem))]
	public class MDMSupportCertificateRegistryItemTest : StronglyTypedRegistryItemTestCase<SystemToSystemTrustInfo>
	{
		protected override StronglyTypedRegistryItem<SystemToSystemTrustInfo, SystemToSystemTrustInfo> GetNewRegistryItem()
		{
			return new MDMSupportCertificateRegistryItem("Test", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", MDMProductCodes.AVS);
		}
	}
}
