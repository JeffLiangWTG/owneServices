using System;
using Enterprise.Customs.EU.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.GUI.Testing
{
	[TestedType(typeof(CustomsBrokerageUserControl))]
	class CustomsBrokerageUserControlTest : CustomsBrokerageUserControlAbstractTest<CustomsBrokerageUserControl>
	{
		public void TestContainerUserControl()
		{
			declaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			control.MainTabControl.SelectedTab = control.ContainerTabPage;
			AssertType<CustomsCusContainersUserControl>(control.ContainerUserControl);
		}

		public void TestEntryInstructionsTabVisibleForCountry()
		{
			AssertEquals(false, control.EntryInstructionsTabVisibleForCountry);
		}

		protected override Type JobDeclarationUserControlType => typeof(JobDeclarationUserControl);

		protected override Type ImportSupplierHeaderUserControlType => typeof(ImportSupplierHeaderUserControl);

		protected override Type ExportSupplierHeaderUserControlType => typeof(ExportSupplierHeaderUserControl);

		protected override Type ImportInvoiceLineUserControlType => typeof(ImportInvoiceLineUserControl);

		protected override Type ExportInvoiceLineUserControlType => typeof(ExportInvoiceLineUserControl);

		protected override Type MessageUserControlType => typeof(ImportMessageUserControl);

		protected override Type EntryInstructionDetailsUserControlType => typeof(EU.GUI.EntryInstructionDetailsUserControl);

		protected override Type ContainersUserControlType => typeof(CustomsCusContainersUserControl);

		protected override Type DV1UserControlType => typeof(DV1UserControl);
	}
}
