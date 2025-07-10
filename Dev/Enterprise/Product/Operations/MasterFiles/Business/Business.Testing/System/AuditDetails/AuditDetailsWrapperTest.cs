using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AuditDetailsWrapper))]
	class AuditDetailsWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new AuditDetailsWrapper(Factory, Factory.New<OrgHeader>());
		}
	}
}
