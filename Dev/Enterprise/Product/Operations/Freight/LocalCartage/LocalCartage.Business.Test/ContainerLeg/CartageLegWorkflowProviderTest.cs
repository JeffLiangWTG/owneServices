using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(CommonCartageLeg))]
	class CartageLegWorkflowProviderTest : WorkflowProviderTest<CommonCartageLeg, CartageLegProcessTaskCollection>
	{
		public void TestGetTemplateFilterCriteria()
		{
			cartageLeg.Factory.Save();
			AssertGetTemplateFilterCriteria(delegate(ZGuid value)
			{
				cartageLeg.Cartage.LocalClientAddressPK = Factory.Load<OrgHeader>(value).Addresses[0].PK;
			}, ProcessTaskTemplate.P0_OH_ClientInfo, Client.PK, Client2.PK, ZGuid.Empty);
		}

		public void TestWorkflowItems()
		{
			var task = cartageLeg.WorkflowItems.AddNew();
			cartageLeg.Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var typeDecided = newFactory.Load<ProcessTask>(task.PK);
			var leg = newFactory.Load<CommonCartageLeg>(cartageLeg.PK);
			AssertNotNull(leg.WorkflowItems);
		}

		protected override ZString ExpectedWorkflowType
		{
			get
			{
				return WorkflowDescriptors.CartageLegWorkflowDescriptorCode;
			}
		}

		protected override CommonCartageLeg GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var cartage = Factory.New<CommonCartage>();
			new JobHeader.Loader(cartage).TryLoadOrCreate();
			var leg = cartage.LooseBookedMoves.AddNew().CartageLegs.AddNew();
			return leg;
		}

		CommonCartageLeg cartageLeg
		{
			get
			{
				return BusinessObject;
			}
		}
	}
}
