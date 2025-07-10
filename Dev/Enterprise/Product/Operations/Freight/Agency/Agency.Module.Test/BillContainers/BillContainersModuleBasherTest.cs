using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(BillContainersModule))]
	internal class BillContainersModuleBasherTest : ZModuleBasherTest
	{
		public void TestSupportsWorkflow()
		{
			Assert(Module.SupportsWorkflow);
		}

		public void TestDenyNewAndDelete()
		{
			AssertEquals(false, Module.AllowNew);
			AssertEquals(false, Module.AllowDelete);
		}

		#region Implementation
		BillContainersModule Module
		{
			get
			{
				return module ?? (module = (BillContainersModule)ZModuleFactory.Instance.Create(GetModuleID()));
			}
		}

		BillContainersModule module;
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.AgencyBillContainers;
		}

		protected override void TearDown()
		{
			if (module != null)
			{
				module.Dispose();
			}

			base.TearDown();
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			BillOfLading bill = collection.Factory.New<BillOfLading>();
			BillOfLadingContainer container = bill.RealContainers.AddNew();
			container.JC_ContainerNum = "TEST4100013";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			BillOfLadingContainerCollection containerCollection = (BillOfLadingContainerCollection)collection;
			containerCollection.AdditionalFilter = new ZQuery(JobContainerSchema.PK, container.PK);
			Assert(containerCollection.Contains(container));
		}

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var shipment = factory.NewWithValidTestData<BillOfLading>();
			return shipment.RealContainers.AddNew();
		}
		#endregion
	}
}
