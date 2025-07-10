using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(CMDPermitNumberCollection))]
	public class CMDPermitNumberCollectionTest : CusCodeDataCollectionTest<CMDPermitNumber>
	{
		public void TestAllowNew()
		{
			var cmdDataCollection = new CMDPermitNumberCollection(Shipment);
			AssertEquals("Max count is 50", 50, cmdDataCollection.MaxCount);
		}

		protected override Customs.Business.CusCodeDataCollection<CMDPermitNumber> GetCusCodeDataCollection()
		{
			return new CMDPermitNumberCollection(Shipment);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<CMDPermitNumber>();
			result.CY_ParentID = Shipment.PK;
			result.CY_ParentTableCode = Shipment.TablePrefix;
			return result;
		}

		ForwardingShipment Shipment
		{
			get
			{
				return shipment ?? (shipment = Factory.New<ForwardingShipment>());
			}
		}

		ForwardingShipment shipment;
	}
}
