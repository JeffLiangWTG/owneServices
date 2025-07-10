using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class CusInBondContainerSynchroniserTest : Customs.Business.Testing.CusInBondContainerSynchroniserTest
	{
		public void TestSynchroniseBC_RC()
		{
			synchroniser = new CusInBondContainerSynchroniser((CusInBondContainer)billContainer, container, shipment);
			synchroniser.Synchronise(true);

			var fortyFootGpContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, TestContainerTypeNK);

			container.JC_RC = fortyFootGpContainer.PK;
			AssertEquals(fortyFootGpContainer.PK, billContainer.BC_RC);

			container.JC_RC = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, billContainer.BC_RC);

			container.JC_RC = fortyFootGpContainer.PK;
			AssertEquals(fortyFootGpContainer.PK, billContainer.BC_RC);
		}

		public void TestSynchroniseBC_IsEmpty()
		{
			synchroniser = new CusInBondContainerSynchroniser((CusInBondContainer)billContainer, container, shipment);
			synchroniser.Synchronise(true);

			container.JC_IsEmptyContainer = ZBool.True;
			AssertEquals(ZBool.True, billContainer.BC_IsEmpty);

			container.JC_IsEmptyContainer = ZBool.False;
			AssertEquals(ZBool.False, billContainer.BC_IsEmpty);
		}

		public void TestSynchroniseBC_TypeOfService()
		{
			synchroniser = new CusInBondContainerSynchroniser((CusInBondContainer)billContainer, container, shipment);
			synchroniser.Synchronise(true);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.Bulk;
			container.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CFS;
			AssertEquals(ServiceTypeList.Codes.ContainerStation, billContainer.BC_TypeOfService);

			container.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CFS_CY;
			AssertEquals(ServiceTypeList.Codes.BreakBulk, billContainer.BC_TypeOfService);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AssertEquals(ZString.Empty, billContainer.BC_TypeOfService);

			container.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CY_CY;
			AssertEquals(ServiceTypeList.Codes.ContainerYard, billContainer.BC_TypeOfService);

			container.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CY_CFS;
			AssertEquals(ZString.Empty, billContainer.BC_TypeOfService);

			shipment.JS_PackingMode = Core.Constants.ContainerModes.BreakBulk;
			AssertEquals(ServiceTypeList.Codes.BreakBulk, billContainer.BC_TypeOfService);
		}
	}
}
