using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.CFS.GUI.Testing
{
	public class RegistrationDetailsTest : BaseFreightTest
	{
		public void TestShowTabBasedOnPurpose()
		{
			using (CFSContainerFormTestClass form = new CFSContainerFormTestClass(CFSContainer))
			{
				form.Show();
				AssertEquals(form.RegistrationDetails.CFSTabPage, form.RegistrationDetails.PurposeSpecificDetailsTabControl.SelectedTab);
				AssertEquals(false, form.RegistrationDetails.PurposeSpecificDetailsTabControl.TabPages.Contains(form.RegistrationDetails.StorageTabPage));
			}

			CFSContainer.JC_Purpose = ContainerPurposeTypeCodeDescriptionPairList.Codes.Storage;
			using (CFSContainerFormTestClass form = new CFSContainerFormTestClass(CFSContainer))
			{
				form.Show();
				AssertEquals(form.RegistrationDetails.StorageTabPage, form.RegistrationDetails.PurposeSpecificDetailsTabControl.SelectedTab);
				AssertEquals(false, form.RegistrationDetails.PurposeSpecificDetailsTabControl.TabPages.Contains(form.RegistrationDetails.CFSTabPage));
			}
		}

		public void TestTransportModeControlState()
		{
			using (CFSContainerFormTestClass form = new CFSContainerFormTestClass(CFSContainer))
			{
				form.Show();

				CFSContainer.JC_TransportMode = Constants.TransportModes.Air;
				AssertEquals("Master Bill No", form.RegistrationDetails.JC_Calc_MasterBillNumTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Master Bill No", form.RegistrationDetails.StorageMasterBillNumberTextBox.GetExtension<ILabelCaptionRenderer>().Caption);

				CFSContainer.JC_TransportMode = Constants.TransportModes.Rail;
				AssertEquals("Ocean Bill No", form.RegistrationDetails.JC_Calc_MasterBillNumTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Ocean Bill No", form.RegistrationDetails.StorageMasterBillNumberTextBox.GetExtension<ILabelCaptionRenderer>().Caption);

				CFSContainer.JC_TransportMode = Constants.TransportModes.Road;
				AssertEquals("Ocean Bill No", form.RegistrationDetails.JC_Calc_MasterBillNumTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Ocean Bill No", form.RegistrationDetails.StorageMasterBillNumberTextBox.GetExtension<ILabelCaptionRenderer>().Caption);

				CFSContainer.JC_TransportMode = Constants.TransportModes.Sea;
				AssertEquals("Ocean Bill No", form.RegistrationDetails.JC_Calc_MasterBillNumTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Ocean Bill No", form.RegistrationDetails.StorageMasterBillNumberTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestCFSControlState()
		{
			using (CFSContainerFormTestClass form = new CFSContainerFormTestClass(CFSContainer))
			{
				form.Show();
				AssertEquals("Pack/Unpack Details", form.RegistrationDetails.PackUnpackDatesGroupBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Pack/Unpack Date", form.RegistrationDetails.JC_PackUnpackDate_ReadonlyDateEdit.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals(true, form.RegistrationDetails.AvailableStorageDatesPanel.Visible);
			}

			CFSLoadListConsol loadList = (CFSLoadListConsol)GetImportConsol(typeof(CFSLoadListConsol));
			Transport transport = loadList.Transports[0];
			CFSContainer newCFSContainer = loadList.Containers.AddNew();

			using (CFSContainerFormTestClass form = new CFSContainerFormTestClass(newCFSContainer))
			{
				transport.JW_RL_NKLoadPort = HomePort; //pack
				transport.JW_RL_NKDiscPort = OverseasPort; //pack
				form.Show();
				AssertEquals(true, form.RegistrationDetails.ArrivalEmptyPanel.Visible);
				AssertEquals(false, form.RegistrationDetails.ArrivalCTOSlotPanel.Visible);
				AssertEquals(false, form.RegistrationDetails.AvailableStorageDatesPanel.Visible);
				form.RegistrationDetails.ArrivalDispatchTabControl.SelectedTab = form.RegistrationDetails.ContainerDispatchTabPage;
				AssertEquals(true, form.RegistrationDetails.DispatchCTOSlotPanel.Visible);
				AssertEquals(false, form.RegistrationDetails.DispatchEmptyPanel.Visible);

				AssertEquals("Pack Details", form.RegistrationDetails.PackUnpackDatesGroupBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Pack Date", form.RegistrationDetails.JC_PackUnpackDate_ReadonlyDateEdit.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals(false, form.RegistrationDetails.AvailableStorageDatesPanel.Visible);
			}

			using (CFSContainerFormTestClass form = new CFSContainerFormTestClass(newCFSContainer))
			{
				transport.JW_RL_NKLoadPort = OverseasPort;//unpack
				transport.JW_RL_NKDiscPort = HomePort;
				form.Show();
				AssertEquals(false, form.RegistrationDetails.ArrivalEmptyPanel.Visible);
				AssertEquals(true, form.RegistrationDetails.ArrivalCTOSlotPanel.Visible);
				AssertEquals(true, form.RegistrationDetails.AvailableStorageDatesPanel.Visible);
				AssertEquals(true, form.RegistrationDetails.AvailableStorageDatesPanel.Visible);
				form.RegistrationDetails.ArrivalDispatchTabControl.SelectedTab = form.RegistrationDetails.ContainerDispatchTabPage;
				AssertEquals(false, form.RegistrationDetails.DispatchCTOSlotPanel.Visible);
				AssertEquals(true, form.RegistrationDetails.DispatchEmptyPanel.Visible);

				AssertEquals("Unpack Details", form.RegistrationDetails.PackUnpackDatesGroupBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Unpack Date", form.RegistrationDetails.JC_PackUnpackDate_ReadonlyDateEdit.GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestAUPanels()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			using (CFSContainerFormTestClass form = new CFSContainerFormTestClass(CFSContainer))
			{
				form.Show();
				AssertEquals(true, form.RegistrationDetails.AUContainerArrivalPanel.Visible);
				form.RegistrationDetails.ArrivalDispatchTabControl.SelectedTab = form.RegistrationDetails.ContainerDispatchTabPage;
				AssertEquals(true, form.RegistrationDetails.AUContainerDispatchPanel.Visible);
			}

			GlbCompany.CurrentCompany.SetCountry("ER");
			using (CFSContainerFormTestClass form = new CFSContainerFormTestClass(CFSContainer))
			{
				form.Show();
				AssertEquals(false, form.RegistrationDetails.AUContainerArrivalPanel.Visible);
				form.RegistrationDetails.ArrivalDispatchTabControl.SelectedTab = form.RegistrationDetails.ContainerDispatchTabPage;
				AssertEquals(false, form.RegistrationDetails.AUContainerDispatchPanel.Visible);
			}
		}

		public void TestIsSealOkVisibility()
		{
			CFSLoadListConsol loadList = (CFSLoadListConsol)GetImportConsol(typeof(CFSLoadListConsol));
			Transport transport = loadList.Transports[0];
			CFSContainer newContainer = loadList.Containers.AddNew();

			using (CFSContainerFormTestClass form = new CFSContainerFormTestClass(newContainer))
			{
				form.Show();
				AssertEquals(true, form.RegistrationDetails.JC_IsSealOkCheckBox.Visible);

				loadList.JK_RL_NKLoadPort = HomePort;
				loadList.JK_RL_NKDischargePort = AlternateHomePort;
				AssertEquals(true, form.RegistrationDetails.JC_IsSealOkCheckBox.Visible);

				loadList.JK_RL_NKLoadPort = HomePort;
				loadList.JK_RL_NKDischargePort = OverseasPort;
				AssertEquals(false, form.RegistrationDetails.JC_IsSealOkCheckBox.Visible);

				loadList.JK_RL_NKLoadPort = OverseasPort;
				loadList.JK_RL_NKDischargePort = HomePort;
				AssertEquals(true, form.RegistrationDetails.JC_IsSealOkCheckBox.Visible);
			}
		}

		public void TestMasterBillLabelChangesWhenTransportModeChanges()
		{
			CFSContainer cFSContainer = Factory.New<CFSContainer>();
			using (CFSContainerFormTestClass form = new CFSContainerFormTestClass(cFSContainer))
			{
				form.Show();
				cFSContainer.JC_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("Master Bill No", form.RegistrationDetails.JC_Calc_MasterBillNumTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				cFSContainer.JC_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("Ocean Bill No", form.RegistrationDetails.JC_Calc_MasterBillNumTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				cFSContainer.JC_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("Master Bill No", form.RegistrationDetails.JC_Calc_MasterBillNumTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestEmptyYardPanelChangesWhenConsolChanges()
		{
			CFSContainer container = Factory.New<CFSContainer>();
			CFSLoadListConsol importList = (CFSLoadListConsol)GetImportConsol(typeof(CFSLoadListConsol));
			CFSLoadListConsol exportList = (CFSLoadListConsol)GetExportConsol(typeof(CFSLoadListConsol));
			importList.Transports[0].JW_RL_NKLoadPort = OverseasPort;
			importList.Transports[0].JW_RL_NKDiscPort = HomePort;
			exportList.Transports[0].JW_RL_NKLoadPort = HomePort;
			exportList.Transports[0].JW_RL_NKDiscPort = OverseasPort;

			using (CFSContainerFormTestClass form = new CFSContainerFormTestClass(container))
			{
				form.Show();

				container.JC_JK = exportList.PK;
				AssertEquals(true, form.RegistrationDetails.ArrivalEmptyPanel.Visible);
				AssertEquals(false, form.RegistrationDetails.DispatchEmptyPanel.Visible);

				container.JC_JK = importList.PK;
				AssertEquals(false, form.RegistrationDetails.ArrivalEmptyPanel.Visible);
				form.RegistrationDetails.ArrivalDispatchTabControl.SelectedTab = form.RegistrationDetails.ContainerDispatchTabPage;
				AssertEquals(true, form.RegistrationDetails.DispatchEmptyPanel.Visible);
			}
		}

		#region Implementation

		CFSContainer CFSContainer
		{
			get
			{
				if (fCFSContainer == null)
				{
					fCFSContainer = Factory.New<CFSContainer>();
				}
				return fCFSContainer;
			}
		}
		CFSContainer fCFSContainer;

		#region CFS Container Form Test Class

		public class CFSContainerFormTestClass : CFSContainerForm
		{
			public CFSContainerFormTestClass(CFSContainer container)
				: base(container)
			{
			}

			public new RegistrationDetails RegistrationDetails
			{
				get { return base.RegistrationDetails; }
			}

			public new ZTemplateTabControl MainTabControl
			{
				get { return base.MainTabControl; }
			}
		}

		#endregion

		#endregion
	}
}
