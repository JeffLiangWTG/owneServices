using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(GroupRelatedDeclarationGenPivotCollection))]
	sealed class GroupRelatedDeclarationGenPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var group = Factory.New<BaseJobComInvoiceGroupHeader>();
			return new GroupRelatedDeclarationGenPivotCollection(group);
		}

		#endregion
	}
}
