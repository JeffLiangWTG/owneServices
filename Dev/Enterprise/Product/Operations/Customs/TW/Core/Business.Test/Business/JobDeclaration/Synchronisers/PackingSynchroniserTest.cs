using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class PackingSynchroniserTest : TestCaseWithFactory
	{
		public void TestSynchronise()
		{
			using (TWCustomsDataRegistry.Instance.EnableCustomsDeclarationPackingList.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_HouseBill = "HBL1";
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_RL_NKLoadPort = "KRBUS";
				consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				consol.Shipments.Add(shipment);
				consol.JK_MasterBillNum = "M1";
				var forwardingContainer1 = consol.Containers.AddNew();
				forwardingContainer1.JC_ContainerNum = "C1";
				var packLine1 = shipment.OuterPackLines.AddNew();
				packLine1.JL_PackageCount = 1;
				packLine1.JL_JC = forwardingContainer1.PK;
				packLine1.JL_ActualWeight = 10M;
				packLine1.JL_ActualWeightUQ = "KG";
				packLine1.JL_ActualVolume = 2M;
				packLine1.JL_ActualVolumeUQ = "M";
				packLine1.JL_Length = 1.1M;
				packLine1.JL_Width = 1.2M;
				packLine1.JL_Height = 1.3M;
				packLine1.JL_UnitOfDimension = "M";
				packLine1.JL_MarksAndNumbers = "AAAAAA";
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_JS = shipment.PK;
				declaration.ShipmentSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
				shipment.PackLineSynchroniser.MarkSyncDirty();
				Factory.Save();
				declaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
				var package = declaration.Packages[0];
				AssertEquals(10M, package.CW_GrossWeight);
				AssertEquals("KG", package.CW_GrossWeightUQ);
				AssertEquals(2M, package.CW_Volume);
				AssertEquals("M", package.CW_VolumeUQ);
				AssertEquals(1.1M, package.CW_Length);
				AssertEquals(1.2M, package.CW_Width);
				AssertEquals(1.3M, package.CW_Height);
				AssertEquals("M", package.CW_DimensionUQ);
				AssertEquals("", package.CW_MarksAndNos);
				AssertEquals(0M, package.CW_NetWeight);
			}
		}
	}
}
