
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(CertificationCodeMappingRegistryItem))]
	sealed class CertificationCodeMappingRegistryItemTest : StronglyTypedRegistryItemTestCase<CertificationCodeMappingCollection, CertificationCodeMappingCollection>
	{
		protected override StronglyTypedRegistryItem<CertificationCodeMappingCollection, CertificationCodeMappingCollection> GetNewRegistryItem()
		{
			return new CertificationCodeMappingRegistryItem("hello", (NoResString)"world");
		}
	}
}
