using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class SingleAddressValidationControlForTest : SingleAddressValidationControl
	{
		public SingleAddressValidationControlForTest()
		{
			BindingCompleted = true;
		}

		public bool ClearDataBindingWhenGetWebAddressValidationResult { get; set; }

		public bool SimulateIsOnSelectedTab
		{
			get;
			set;
		}

		public bool FireProcessCmdKey(Keys keyData)
		{
			Message msg = new Message();
			return this.ProcessCmdKey(ref msg, keyData);
		}

		public override bool IsOnCurrentSelectedTab()
		{
			if (SimulateIsOnSelectedTab)
			{
				return true;
			}

			return base.IsOnCurrentSelectedTab();
		}

		protected override UpdateChoice GetUpdateChoice(string title, bool showYesButton = true)
		{
			if (UpdateChoice == UpdateChoice.Yes)
			{
				return base.GetUpdateChoice("", false);
			}

			return UpdateChoice;
		}

		public string ValidationStatusForTest { get; set; }

		public UpdateChoice UpdateChoice { get; set; } = UpdateChoice.Yes;

		public new BusinessObject CurrentAddressEntity => base.CurrentAddressEntity;

		public void PerformValidateAddressClickForTest(string expectedStatusCode)
		{
			var resultItem = new ValidationResultItem()
			{
				ResultStatusCode = expectedStatusCode,
				Address1 = "AVE 1",
				Country = "AU"
			};
			AddressSuggestionControl.SetupListView(new List<ValidationResultItem>() { resultItem }, resultItem);
		}

		protected override Task<WebAddressValidationResult> GetWebAddressValidationResult()
		{
			WebAddressValidationResult result = new WebAddressValidationResult();
			AddressForValidation.ValidationStatus = ValidationStatusForTest;

			if (ClearDataBindingWhenGetWebAddressValidationResult)
			{
				SetDataBinding(null, "");
			}

			return Task.FromResult(result);
		}

		internal override AddressUserControl ParentControl => parentControl ?? (parentControl = new AddressUserControl(new AdministrationPanelManager(new BusinessObjectFactory())));
		AddressUserControl parentControl;

		protected override void Dispose(bool disposing)
		{
			ParentControl.Dispose();
			DesignerActionExtenderProvider.Dispose();
			base.Dispose(disposing);
		}

		public bool ShouldValidateForTest => ShouldValidate;
	}
}
