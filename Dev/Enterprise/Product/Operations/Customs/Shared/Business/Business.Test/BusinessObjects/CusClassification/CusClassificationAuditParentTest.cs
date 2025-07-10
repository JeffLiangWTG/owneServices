using System.Linq;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseCusClassification))]
	sealed class CusClassificationAuditParentTest : AuditParentTest<BaseCusClassification>
	{
		protected override BaseCusClassification NewTestAuditParent() => Factory.New<BaseCusClassification>();

		public void TestRelatedAuditChildren()
		{
			var classification = Factory.New<BaseCusClassification>();
			Assert(!classification.RelatedAuditChildren.Any());
		}
	}
}
