using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	[TestedType(typeof(CusContainer))]
	sealed class CusContainerTest : Customs.Business.Testing.BaseCusContainerTest<CusContainer, JobDeclaration>
	{
		protected override ICustomLabelsProvider GetNewCustomLabelsProvider(BusinessObject bo) => new Customs.Business.BaseCusContainer.CustomLabelsProvider(((CusContainer)bo).Declaration);
	}
}
