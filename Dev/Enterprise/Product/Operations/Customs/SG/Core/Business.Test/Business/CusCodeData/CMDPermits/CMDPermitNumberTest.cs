using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.SG;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(CMDPermitNumber))]
	public class CMDPermitNumberTest : Customs.Business.Testing.CusCodeDataTest<CMDPermitNumber>
	{
		public void TestICustomsManifestLineSequenceIsCorrectlySetup()
		{
			AssertEquals(typeof(CMDPermitNumber), ObjectFactory.GetType<Integration.Customs.SG.ICMDPermitNumber>());
		}

		public void TestSetDefaultValues()
		{
			var cMDPermitNumber = Factory.New<CMDPermitNumber>();
			AssertEquals(CusCodeDataTypeList.Codes.CMD, cMDPermitNumber.CY_Type);
		}

		#region Overrides
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<CMDPermitNumber>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var shipment = factory.New<ForwardingShipment>();
			var parent = shipment;
			var result = factory.New<CMDPermitNumber>();
			result.Parent = parent;
			return result;
		}

		#endregion
	}
}
