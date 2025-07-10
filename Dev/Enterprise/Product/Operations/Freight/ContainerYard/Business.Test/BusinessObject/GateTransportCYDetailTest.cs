using CargoWise.EntityFramework;
using Enterprise.Freight.ContainerYard.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.ContainerYard.Testing
{
	[TestedType(typeof(GateTransportCYDetail))]
	sealed class GateTransportCYDetailTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsSavedByFactory()
		{
			var gateTransportCYDetail = GetNewBusinessObject();
			AssertEquals(false, gateTransportCYDetail.IsSavedByFactory);
		}

		public void TestDocManagerInfo()
		{
			var gateTransportCYDetail = Factory.New<GateTransportCYDetail>();
			var docManagerInfo = gateTransportCYDetail.DocManagerInfo;
			AssertEquals(gateTransportCYDetail, docManagerInfo.BusinessEntity);
			AssertEquals(Core.Constants.DocManagerCodes.GateTransportCYDetail, docManagerInfo.DocManagerCode);
		}

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New(GetExpectedBusinessObjectType());
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("GateTransportCYDetail is not supposed to be saved by factory by default", true);
		}

		#endregion
	}
}
