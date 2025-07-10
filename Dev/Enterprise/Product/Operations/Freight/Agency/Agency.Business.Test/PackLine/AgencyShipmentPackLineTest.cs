using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyShipmentPackLineTest : BaseFreightTest
	{
		public void TestSetDefaultValues()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentPackLine packLine = shipment.OuterPackLines.AddNew();
			AssertEquals("JL_PackageCount should be equal 1 by default", 1, packLine.JL_PackageCount);
		}

		public void TestJL_JC()
		{
			AgencyShipment shipment = Factory.New<AgencyShipment>();
			AgencyShipmentPackLine packLine = shipment.OuterPackLines.AddNew();
			AgencyShipmentContainer container1 = shipment.RealContainers.AddNew();
			AgencyShipmentContainer container2 = shipment.RealContainers.AddNew();
			packLine.JL_JCInfo.ValueChanged += new EventHandler(ValueChanged);
			AssertEquals("JL_JC not set yet", ZGuid.Empty, packLine.JL_JC);
			fValueChanged = 0;
			packLine.JL_JC = container1.PK;
			AssertEquals("Value changed once", 1, fValueChanged);
			AssertEquals("JL_JC set to Container1", container1.PK, packLine.JL_JC);
			AssertEquals(1, packLine.Containers.Count);
			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			AgencyShipmentPackLine packLine2 = factory2.Load<AgencyShipmentPackLine>(packLine.PK);
			packLine2.JL_JCInfo.ValueChanged += new EventHandler(ValueChanged);
			AssertEquals("JL_JC loads correctly", packLine2.JL_JC, container1.PK);
			fValueChanged = 0;
			packLine2.JL_JC = container2.PK;
			AssertEquals("Value changed once", 1, fValueChanged);
			AssertEquals("JL_JC changed to another container", container2.PK, packLine2.JL_JC);
			fValueChanged = 0;
			factory2.Save();
			AssertEquals("Value changed via refresh bus", 1, fValueChanged);
			AssertEquals("JL_JC works with the data refresh bus", container2.PK, packLine.JL_JC);
			fValueChanged = 0;
			container2.Delete();
			AssertEquals("Value changed via delete", 1, fValueChanged);
			AssertEquals("JL_JC should have been reset.", ZGuid.Empty, packLine.JL_JC);
			fValueChanged = 0;
			Factory.Save();
			AssertEquals("Value changed via refresh bus", 1, fValueChanged);
			AssertEquals("JL_JC should be cleared by the data refresh bus", ZGuid.Empty, packLine2.JL_JC);
			packLine.JL_JC = container1.PK;
			fValueChanged = 0;
			Factory.Save();
			AssertEquals("Value changed via refresh bus", 1, fValueChanged);
			AssertEquals("JL_JC should be set to container 1", container1.PK, packLine2.JL_JC);
		}

		public void TestTypeOfUNDGDataItemCollection()
		{
			var shipment = Factory.New<AgencyShipment>();
			var packLine = shipment.OuterPackLines.AddNew();
			AssertEquals(typeof(AgencyUNDGDataItemCollection), packLine.UNDGs.GetType());
		}

		#region Implementation

		void ValueChanged(object sender, EventArgs e)
		{
			fValueChanged++;
		}

		int fValueChanged;
		protected override void SetUp()
		{
			base.SetUp();
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode != HomePort.SubstringSafe(0, 2))
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = HomePort.SubstringSafe(0, 2);
			}
		}
		#endregion
	}
}
