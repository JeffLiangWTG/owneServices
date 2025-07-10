using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V3.Business.Testing
{
	[TestedType(typeof(V3Brokerage))]
	public class V3BrokerageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDeclarations()
		{
			V3JobDeclaration declaration = Factory.New<V3JobDeclaration>();
			V3Brokerage brokerage = new V3Brokerage(declaration.PK, Factory);
			AssertEquals(1, brokerage.Declarations.Count);
			AssertEquals(declaration.PK, brokerage.Declarations[0].PK);
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			declaration = Factory.New<V3JobDeclaration>();
			declaration.JE_GB = Factory.NewWithValidTestData<MasterFiles.Business.GlbCompany>().Branches.AddNew().PK;
			declaration.JE_JS = shipment.PK;
			brokerage = new V3Brokerage(shipment, Factory);
			AssertEquals(2, brokerage.Declarations.Count);
			AssertNotNull(brokerage.Declarations.FindByPK(declaration.PK));
		}

		#region implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return V3Brokerage;
		}

		V3Brokerage V3Brokerage
		{
			get
			{
				return v3Brokerage ?? (v3Brokerage = new V3Brokerage(Factory.New<V3JobDeclaration>().PK, Factory));
			}
		}

		V3Brokerage v3Brokerage;
		#endregion
	}
}
