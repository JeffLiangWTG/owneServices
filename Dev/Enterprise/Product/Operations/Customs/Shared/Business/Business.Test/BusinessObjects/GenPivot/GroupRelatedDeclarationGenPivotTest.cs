using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(GroupRelatedDeclarationGenPivot))]
	sealed class GroupRelatedDeclarationGenPivotTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<GroupRelatedDeclarationGenPivot>();
		}
	}
}
