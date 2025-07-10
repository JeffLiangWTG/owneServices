using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ContractManagement.Business;
using NUnit.Framework;

namespace Enterprise.ContractManagement.GUI.Testing
{
	[TestedType(typeof(ViewMultiAllocationSelectionManager))]
	public sealed class ViewMultiAllocationSelectionManagerTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ViewMultiAllocationSelectionManager(Factory, new RatingContractAllocationLine[]
			{
				Factory.New<RatingContractAllocationLine>()
			});
		}
	}
}
