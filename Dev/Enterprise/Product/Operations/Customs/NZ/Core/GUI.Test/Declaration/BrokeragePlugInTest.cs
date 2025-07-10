using System;
using CargoWise.BrandManager;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.Testing;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.NZ.GUI.Declaration.Testing
{
	abstract class BrokeragePlugInTest : Customs.GUI.PlugIn.Testing.BaseBrokeragePlugInAbstractTest
	{
		public void TestWithMinimumDataVersionRequired()
		{
			using (ZForm form = new ZForm(shipment1))
			{
				form.Controls.Add(new ZTabControl());
				form.PlugIns.Add(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment);
				form.Show();
				using (BrokeragePlugIn plugIn = (BrokeragePlugIn)form.PlugIns.GetPlugIn(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment))
				{
					AssertEquals("plugIn.PlugInNotDisplayedMessage", ZString.Empty, plugIn.PlugInNotDisplayedMessage);
				}
			}

			NZCTariffVersionLoaderTest.SetDataVersion(NZCTariffVersionLoader.MinimumDataVersionRequired);
			using (ZForm form = new ZForm(shipment1))
			{
				form.Controls.Add(new ZTabControl());
				form.PlugIns.Add(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment);
				form.Show();
				using (BrokeragePlugIn plugIn = (BrokeragePlugIn)form.PlugIns.GetPlugIn(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment))
				{
					AssertEquals("plugIn.PlugInNotDisplayedMessage", ZString.Empty, plugIn.PlugInNotDisplayedMessage);
				}
			}

			NZCTariffVersionLoaderTest.SetDataVersion(NZCTariffVersionLoader.MinimumDataVersionRequired - 1);
			using (ZForm form = new ZForm(shipment1))
			{
				form.Controls.Add(new ZTabControl());
				form.PlugIns.Add(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment);
				form.Show();
				using (BrokeragePlugIn plugIn = (BrokeragePlugIn)form.PlugIns.GetPlugIn(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment))
				{
					AssertEquals("plugIn.PlugInNotDisplayedMessage", string.Format(@"NZ Tariff minimum data version [{0}] requirement not met. The current Tariff data version is [{1}].
Please update your Tariff data by running ediTariff -> {2} -> Export NZ data to {2}.", NZCTariffVersionLoader.MinimumDataVersionRequired, NZCTariffVersionLoader.MinimumDataVersionRequired - 1, BrandingFactory.Instance.ProductName), plugIn.PlugInNotDisplayedMessage);
				}
			}

			NZCTariffVersionLoaderTest.SetDataVersion(NZCTariffVersionLoader.MinimumDataVersionRequired + 1);
			using (ZForm form = new ZForm(shipment1))
			{
				form.Controls.Add(new ZTabControl());
				form.PlugIns.Add(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment);
				form.Show();
				using (BrokeragePlugIn plugIn = (BrokeragePlugIn)form.PlugIns.GetPlugIn(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment))
				{
					AssertEquals("plugIn.PlugInNotDisplayedMessage", ZString.Empty, plugIn.PlugInNotDisplayedMessage);
				}
			}
		}

		public void TestGetDeclarationFromShipment()
		{
			declaration.JE_JS = shipment1.PK;
			using (BrokeragePlugIn testPlugIn = GetNewBrokeragePlugIn(shipment1))
			{
				AssertEquals("TestPlugIn.JobDeclaration", declaration, testPlugIn.JobDeclaration);
			}

			using (BrokeragePlugIn testPlugIn = GetNewBrokeragePlugIn(shipment2))
			{
				AssertEquals("TestPlugIn.JobDeclaration", null, testPlugIn.JobDeclaration);
			}
		}

		public void TestStmDataExists()
		{
			NZCTariffVersionLoaderTest.DropStmDataTable();

			JobDeclaration declaration = Factory.New<JobDeclaration>();

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (ZForm form = new ZForm(shipment1))
				{
					form.Controls.Add(new ZTabControl());
					form.PlugIns.Add(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment);
					form.Show();
					using (BrokeragePlugIn plugIn = (BrokeragePlugIn)form.PlugIns.GetPlugIn(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment))
					{
						AssertEquals("PlugInNotDisplayedMessage empty when UseRefDatabaseData is true, meaning no error preventing the plugin from displaying.", ZString.Empty, plugIn.PlugInNotDisplayedMessage);
					}
				}
			}

			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (ZForm form = new ZForm(shipment1))
				{
					form.Controls.Add(new ZTabControl());
					form.PlugIns.Add(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment);
					form.Show();
					using (BrokeragePlugIn plugIn = (BrokeragePlugIn)form.PlugIns.GetPlugIn(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment))
					{
						var expectedErrorMessage = NZCTariffVersionLoaderTest.GetMinimumDataVersionMessage(NZCTariffVersionLoader.MinimumDataVersionRequired, 0);
						AssertEquals("PlugInNotDisplayedMessage returns error message when UseRefDatabaseData is false, therefore the plugin cannot be displayed.", expectedErrorMessage, plugIn.PlugInNotDisplayedMessage);
					}
				}
			}
		}

		#region Implementation
		ForwardingConsol consol;
		ForwardingShipment shipment1;
		ForwardingShipment shipment2;
		JobDeclaration declaration;
		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = "SEA";
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "NZAKL";
			transport.JW_RL_NKDiscPort = "AUSYD";
			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "HKJ1234567";
			container.JC_ContainerMode = "FCL";
			shipment1 = consol.Shipments.AddNew();
			shipment2 = consol.Shipments.AddNew();
			shipment2.JS_JS_ColoadMasterShipment = shipment1.PK;
			declaration = GetNewJobDeclaration();
		}

		protected abstract JobDeclaration GetNewJobDeclaration();
		protected BrokeragePlugIn GetNewBrokeragePlugIn(ForwardingShipment shipment)
		{
			return new BrokeragePlugIn(shipment);
		}
		#endregion
	}
}
