using System.Linq;
using System.Windows.Forms;
using CargoWise.Definitions.Ecommerce;
using Enterprise.eTail.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.GUI.Testing
{
	[TestedType(typeof(HVLVOriginLoadListForm))]
	class HVLVOriginLoadListFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var form = new HVLVOriginLoadListForm(Factory.New<HVLVOriginLoadList>());
			form.WindowState = FormWindowState.Maximized;
			form.ControllerID = ControllerIDs.HVLVOriginLoadList;
			return form;
		}

		public void TestOriginDepotAndDestinationDepotHaveBoundDataSource()
		{
			var originDepot = Factory.NewWithValidTestData<OrgAddress>();
			var destinationDepot = Factory.NewWithValidTestData<OrgAddress>();

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_OA_OriginDepot = originDepot.PK;
			loadList.HVL_OA_DestinationDepot = destinationDepot.PK;

			Factory.Save();

			using (var form = new HVLVOriginLoadListForm(loadList))
			{
				form.Show();

				var originDepotAddressControl = form.Controls.Find("originDepotAddressControl", true).Single() as ZAddressControl;
				AssertEquals("Lookups.HVL_OA_OriginDepot_List", originDepotAddressControl.BindToOrgList);
				var destinationDepotAddressControl = form.Controls.Find("destinationDepotAddressControl", true).Single() as ZAddressControl;
				AssertEquals("Lookups.HVL_OA_DestinationDepot_List", destinationDepotAddressControl.BindToOrgList);
			}
		}

		public void TestOriginPort_WhenOriginIsEmpty_ThenShouldUseOriginDepotUNLOCO()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Address1 = "17 Park Avenue North";
			address.OA_RL_NKRelatedPortCode = "USNYC";

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_OA_OriginDepot = address.PK;

			Factory.Save();

			using (var form = new HVLVOriginLoadListForm(loadList))
			{
				form.Show();

				var originPort = form.Controls.Find("originPort", true).Single() as ZCodeFindBox;

				AssertEquals("Expected the value of Origin Port to be the Origin Depot UNLOCO (USNYC).", "USNYC", originPort.Text);
			}
		}

		public void TestDestinationPort_WhenDestinationIsEmpty_ThenShouldUseDestinationDepotUNLOCO()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Address1 = "286 Fox Street";
			address.OA_RL_NKRelatedPortCode = "ZAJNB";

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_OA_DestinationDepot = address.PK;

			Factory.Save();

			using (var form = new HVLVOriginLoadListForm(loadList))
			{
				form.Show();

				var destinationPort = form.Controls.Find("destinationPort", true).Single() as ZCodeFindBox;

				AssertEquals("Expected the value of Destination Port to be the Destination Depot UNLOCO (ZAJNB).", "ZAJNB", destinationPort.Text);
			}
		}

		public void TestOriginPort_WhenOriginExists_ThenShouldUseOrigin()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Address1 = "5 Traffic Street";
			address.OA_RL_NKRelatedPortCode = "GBDXY";

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_OA_OriginDepot = address.PK;
			loadList.HVL_RL_NKOrigin = "AUMEL";

			Factory.Save();

			using (var form = new HVLVOriginLoadListForm(loadList))
			{
				form.Show();

				var originPort = form.Controls.Find("originPort", true).Single() as ZCodeFindBox;

				AssertEquals("Expected the value of Origin Port to be 'AUMEL'.", "AUMEL", originPort.Text);
			}
		}

		public void TestDestinationPort_WhenDestinationExists_ThenShouldUseDestination()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Address1 = "31 Duke Street";
			address.OA_RL_NKRelatedPortCode = "GBDRL";

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_OA_DestinationDepot = address.PK;
			loadList.HVL_RL_NKDestination = "AUSUH";

			Factory.Save();

			using (var form = new HVLVOriginLoadListForm(loadList))
			{
				form.Show();

				var destinationPort = form.Controls.Find("destinationPort", true).Single() as ZCodeFindBox;

				AssertEquals("Expected the value of Destination Port to be 'AUSUH'.", "AUSUH", destinationPort.Text);
			}
		}

		public void TestOriginPort_OriginCanBeOverwritten()
		{
			var originDepotAddress = Factory.NewWithValidTestData<OrgAddress>();
			originDepotAddress.OA_Address1 = "52 Ramsay Terrace";
			originDepotAddress.OA_RL_NKRelatedPortCode = "AUADL";

			var destinationDepotAddress = Factory.NewWithValidTestData<OrgAddress>();
			destinationDepotAddress.OA_Address1 = "17 Bay View";
			destinationDepotAddress.OA_RL_NKRelatedPortCode = "CATTA";

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_OA_OriginDepot = originDepotAddress.PK;
			loadList.HVL_OA_DestinationDepot = destinationDepotAddress.PK;

			Factory.Save();

			using (var form = new HVLVOriginLoadListForm(loadList))
			{
				form.Show();

				var originPort = form.Controls.Find("originPort", true).Single() as ZCodeFindBox;

				AssertEquals("Precondition: Expected the value of Origin Port to be the Origin Depot UNLOCO (AUADL).", "AUADL", originPort.Text);

				originPort.CodeBox.Text = "CAOTT";
				_ = form.FireSaveButton();

				AssertEquals("Origin Port should have been updated to 'CAOTT'.", "CAOTT", originPort.Text);
			}
		}

		public void TestDestinationPort_DestinationCanBeOverwritten()
		{
			var originDepotAddress = Factory.NewWithValidTestData<OrgAddress>();
			originDepotAddress.OA_Address1 = "Industrial Area Street 59";
			originDepotAddress.OA_RL_NKRelatedPortCode = "QADOH";

			var destinationDepotAddress = Factory.NewWithValidTestData<OrgAddress>();
			destinationDepotAddress.OA_Address1 = "24 Green Street";
			destinationDepotAddress.OA_RL_NKRelatedPortCode = "GBCMG";

			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_OA_DestinationDepot = destinationDepotAddress.PK;
			loadList.HVL_OA_OriginDepot = originDepotAddress.PK;
			loadList.HVL_RL_NKDestination = "NCNOU";

			Factory.Save();

			using (var form = new HVLVOriginLoadListForm(loadList))
			{
				form.Show();

				var destinationPort = form.Controls.Find("destinationPort", true).Single() as ZCodeFindBox;

				AssertEquals("Expected the value of Destination Port to be the Destination Depot UNLOCO (NCNOU).", "NCNOU", destinationPort.Text);

				destinationPort.CodeBox.Text = "LKCMB";
				_ = form.FireSaveButton();

				AssertEquals("Destination Port should have been updated to 'LKCMB'", "LKCMB", destinationPort.Text);
			}
		}

		public void TestPaymentTermDisplay_FreightCollect()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_INCO = IncoTerms.FreeOnBoard;
			using (var form = new HVLVOriginLoadListForm(loadList))
			{
				form.Show();

				var label = form.Controls.Find("paymentTermDisplay", true)[0] as ZLabel;
				AssertEquals("Freight Collect", label.Text);
			}
		}

		public void TestPaymentTermDisplay_FreightPrepaid()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			loadList.HVL_INCO = IncoTerms.DeliveredAtPlace;
			using (var form = new HVLVOriginLoadListForm(loadList))
			{
				form.Show();

				var label = form.Controls.Find("paymentTermDisplay", true)[0] as ZLabel;
				AssertEquals("Freight Prepaid", label.Text);
			}
		}

		public void TestTransportBookingPluginAdded()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			using (var form = new HVLVOriginLoadListForm(loadList))
			{
				AssertNotNull(ControllerIDs.DtbBooking.Name, form.PlugIns.GetPlugIn(ControllerIDs.DtbBooking));
			}
		}

		public void TestMasterBillReadOnlyStatus()
		{
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			using (var form = new HVLVOriginLoadListForm(loadList))
			{
				form.Show();

				var neutralMasterCheckBox = form.Controls.Find("IsNeutralMAWB", true)[0] as ZCheckBox;
				var masterBillTextBox = form.Controls.Find("masterBillNumberTextBox", true)[0] as ZTextBox;

				neutralMasterCheckBox.Checked = true;
				AssertEquals(true, masterBillTextBox.ReadOnly);

				neutralMasterCheckBox.Checked = false;
				AssertEquals(false, masterBillTextBox.ReadOnly);
			}
		}

		public void TestIsNeutralMasterReadOnlyStatus()
		{
			var jobMawb = Factory.New<JobMawb>();
			jobMawb.JM_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Open;
			loadList.HVL_TransportMode = ContainerModes.AIR;

			using (var form = new HVLVOriginLoadListForm(loadList))
			{
				form.Show();

				var neutralMasterCheckBox = form.Controls.Find("IsNeutralMAWB", true)[0] as ZCheckBox;
				AssertEquals(true, neutralMasterCheckBox.ReadOnly);

				loadList.HVL_RL_NKOrigin = "AUSYD";
				AssertEquals(false, neutralMasterCheckBox.ReadOnly);
			}
		}

		public void TestIsNeutralMasterChangeToReadOnlyAndFalse()
		{
			var jobMawb = Factory.New<JobMawb>();
			jobMawb.JM_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();
			var loadList = Factory.NewWithValidTestData<HVLVOriginLoadList>();
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			loadList.HVL_RL_NKOrigin = "AUSYD";
			loadList.HVL_Status = HVLVOriginLoadListStatus.Codes.Open;
			loadList.HVL_TransportMode = ContainerModes.AIR;

			using (var form = new HVLVOriginLoadListForm(loadList))
			{
				form.Show();

				var neutralMasterCheckBox = form.Controls.Find("IsNeutralMAWB", true)[0] as ZCheckBox;
				AssertEquals(false, neutralMasterCheckBox.ReadOnly);

				neutralMasterCheckBox.Checked = true;
				AssertEquals(true, loadList.HVL_IsNeutralMaster);

				loadList.HVL_RL_NKOrigin = "CNSHA";
				AssertEquals(false, loadList.HVL_IsNeutralMaster);
				AssertEquals(true, neutralMasterCheckBox.ReadOnly);
			}
		}

		public void TestAuditPluginIsAdded()
		{
			var loadList = Factory.New<HVLVOriginLoadList>();
			using (var form = new HVLVOriginLoadListForm(loadList))
			{
				Assert("HVLVOriginLoadList form should have audit plugin", form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
			}
		}
	}
}
