using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.Declaration
{
	public class CustomsContainersWithTrackingUserControlTest : TestCaseWithFactory
	{
		public void TestPlugins()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			using (ZForm form = new ZForm(declaration))
			{
				CustomsBrokerageUserControl brokerageUserControl = new CustomsBrokerageUserControl();
				brokerageUserControl.JobDeclaration = declaration;
				form.Controls.Add(brokerageUserControl);
				form.Show();
				brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.ContainerTabPage;
				CustomsContainersWithTrackingUserControl userControl = brokerageUserControl.ContainerUserControl as CustomsContainersWithTrackingUserControl;
				AssertNotNull("Should have MAFeBACCaContainerPlugin.", userControl.containersUserControl1.DetailTabControl.PlugIns.GetPlugIn(Enterprise.ZArchitecture.Modules.ControllerIDs.Customs.NZ.MAFeBACCaContainerPlugin));
			}
		}

		public void TestGridColumnsContainsCO_ContainerSize()
		{
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			using (DeclarationForm declarationForm = new DeclarationForm(Declaration))
			{
				declarationForm.Show();
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.ContainerTabPage;
				CustomsBrokerageUserControl brokerageUserControl = declarationForm.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				CustomsContainersWithTrackingUserControl userControl = brokerageUserControl.ContainerUserControl as CustomsContainersWithTrackingUserControl;
				Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
				AssertEquals("Grid.Columns.Contains(CO_ContainerSize)", true, userControl.CusContainersBoundGrid.InnerGrid.Columns.Contains(CusContainer.Schema.CO_ContainerSize));
				Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
				AssertEquals("Grid.Columns.Contains(CO_ContainerSize)", false, userControl.CusContainersBoundGrid.InnerGrid.Columns.Contains(CusContainer.Schema.CO_ContainerSize));
				Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
				AssertEquals("Grid.Columns.Contains(CO_ContainerSize)", true, userControl.CusContainersBoundGrid.InnerGrid.Columns.Contains(CusContainer.Schema.CO_ContainerSize));
				Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				AssertEquals("Grid.Columns.Contains(CO_ContainerSize) - this is a legacy only field for write-off. should not show for TSW", false, userControl.CusContainersBoundGrid.InnerGrid.Columns.Contains(CusContainer.Schema.CO_ContainerSize));
			}
		}

		public void TestQuarantineGroupBoxNotVisibleForTSWEX1()
		{
			using (CustomsContainersWithTrackingUserControl userControl = new CustomsContainersWithTrackingUserControl())
			{
				userControl.JobDeclaration = Declaration;
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
				Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
				AssertEquals("UserControl.QuarantineGroupBox.Visible", false, (userControl.FindSingle<ZGroupBox>("QuarantineGroupBox")).Visible);
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("UserControl.QuarantineGroupBox.Visible", true, (userControl.FindSingle<ZGroupBox>("QuarantineGroupBox")).Visible);
				Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
				AssertEquals("UserControl.QuarantineGroupBox.Visible", true, (userControl.FindSingle<ZGroupBox>("QuarantineGroupBox")).Visible);
			}
		}

		public void TestControlVisibilitySwitchingBetweenTSWCREAndLegacyWriteOff()
		{
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			using (DeclarationForm declarationForm = new DeclarationForm(Declaration))
			{
				declarationForm.Show();
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.ContainerTabPage;
				CustomsBrokerageUserControl brokerageUserControl = declarationForm.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				CustomsContainersWithTrackingUserControl userControl = brokerageUserControl.ContainerUserControl as CustomsContainersWithTrackingUserControl;
				AssertEquals("CusContainersBoundGrid columns has TSW column (TSW Container Type/Size)", true, userControl.CusContainersBoundGrid.InnerGrid.Columns.Contains(CusContainer.Schema.CO_MAF_ContainerType));
				Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
				AssertEquals("CusContainersBoundGrid columns has TSW column (TSW Container Type/Size)", false, userControl.CusContainersBoundGrid.InnerGrid.Columns.Contains(CusContainer.Schema.CO_MAF_ContainerType));
			}
		}

		public void TestControlVisibilityForTSWExport()
		{
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			using (DeclarationForm declarationForm = new DeclarationForm(Declaration))
			{
				declarationForm.Show();
				declarationForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = declarationForm.CustomsBrokerageUserControl.ContainerTabPage;
				CustomsBrokerageUserControl brokerageUserControl = declarationForm.CustomsBrokerageUserControl as CustomsBrokerageUserControl;
				CustomsContainersWithTrackingUserControl userControl = brokerageUserControl.ContainerUserControl as CustomsContainersWithTrackingUserControl;
				AssertEquals("CusContainersBoundGrid columns has TSW column (TSW Container Type/Size)", true, userControl.CusContainersBoundGrid.InnerGrid.Columns.Contains(CusContainer.Schema.CO_MAF_ContainerType));
				AssertEquals("CusContainersBoundGrid columns has TSW column (CO_SealingParty)", true, userControl.CusContainersBoundGrid.InnerGrid.Columns.Contains(CusContainer.Schema.CO_SealingParty));
				AssertEquals("CusContainersBoundGrid columns has TSW column (CO_MPIApprovedSystemNumber)", true, userControl.CusContainersBoundGrid.InnerGrid.Columns.Contains(CusContainer.Schema.CO_MPIApprovedSystemNumber));
				AssertEquals("CusContainersBoundGrid columns has TSW column (PackingLocationOrgPK)", true, userControl.CusContainersBoundGrid.InnerGrid.Columns.Contains(CusContainer.Schema.PackingLocationOrgPK));
				AssertEquals("CusContainersBoundGrid columns has TSW column (CO_OA_PackingLocation)", true, userControl.CusContainersBoundGrid.InnerGrid.Columns.Contains(CusContainer.Schema.CO_OA_PackingLocation));
				Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
				AssertEquals("CusContainersBoundGrid columns has TSW column (TSW Container Type/Size)", false, userControl.CusContainersBoundGrid.InnerGrid.Columns.Contains(CusContainer.Schema.CO_MAF_ContainerType));
				AssertEquals("CusContainersBoundGrid columns has TSW column (CO_SealingParty)", false, userControl.CusContainersBoundGrid.InnerGrid.Columns.Contains(CusContainer.Schema.CO_SealingParty));
				AssertEquals("CusContainersBoundGrid columns has TSW column (CO_MPIApprovedSystemNumber)", false, userControl.CusContainersBoundGrid.InnerGrid.Columns.Contains(CusContainer.Schema.CO_MPIApprovedSystemNumber));
				AssertEquals("CusContainersBoundGrid columns has TSW column (PackingLocationOrgPK)", false, userControl.CusContainersBoundGrid.InnerGrid.Columns.Contains(CusContainer.Schema.PackingLocationOrgPK));
				AssertEquals("CusContainersBoundGrid columns has TSW column (CO_OA_PackingLocation)", false, userControl.CusContainersBoundGrid.InnerGrid.Columns.Contains(CusContainer.Schema.CO_OA_PackingLocation));
			}
		}

		public void TestControlCanBeInstantiatedAndDeclarationCanBeSet()
		{
			using (CustomsContainersWithTrackingUserControl userControl = new CustomsContainersWithTrackingUserControl())
			{
				userControl.JobDeclaration = Declaration;
				Assert(true);
			}
		}

		#region Implementation
		protected JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Enterprise.Customs.NZ.Business.Declaration.JobDeclaration.New(Factory);
				}

				return fDeclaration;
			}
		}

		JobDeclaration fDeclaration;
		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			Business.EDITariff_ReferenceFiles_NZ.Testing.NZCTariffVersionLoaderTest.SetDataVersion(NZCTariffVersionLoader.MinimumDataVersionRequired);
		}
	}
}
