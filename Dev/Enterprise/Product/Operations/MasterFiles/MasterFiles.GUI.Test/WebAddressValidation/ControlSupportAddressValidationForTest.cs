using System;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Test
{
	public partial class ControlSupportAddressValidationForTest : ContainerControl, ISupportWebAddressValidationControl
	{
		#region Constructor

		public ControlSupportAddressValidationForTest()
			: this(null)
		{
		}

		public ControlSupportAddressValidationForTest(ISupportWebAddressValidation address)
		{
			AddressForValidation = address;

			InitializeComponent();
		}

		#endregion Constructor

		#region Buttons

		public ZButton ValidateButton { get; } = new ZButton();

		public ZButton ClearAddressFieldsButton { get; } = new ZButton();

		#endregion Buttons

		#region Controls

		public ZTextBox AddressCodeControl { get; } = new ZTextBox();

		public ZTextBox AdditionalAddressInformationControl { get; } = new ZTextBox();

		public ZTextBox Address1Control { get; } = new ZTextBox();

		public ZTextBox Address2Control { get; } = new ZTextBox();

		public ZTextBox CityControl { get; } = new ZTextBox();

		public ZTextBox PostcodeControl { get; } = new ZTextBox();

		public ZDropEdit StateControl { get; } = new ZDropEdit();

		public ZCodeFindBox CountryControl { get; } = new ZCodeFindBox();

		#endregion Controls

		#region Other Properties

		public ISupportWebAddressValidation AddressForValidation { get; set; }

		public Func<Keys, bool> AddressValidationProcessCmdKey { get; set; }

		public Control SuggestionWindowParentControl => this;

		#endregion Other Properties

		#region Deprecated Properties

		public bool ValidationJustForced { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public bool ProcessManuallyVerifyShortCutKey(ref Message msg, Keys keyData) => throw new NotImplementedException();

		#endregion Deprecated Properties
	}
}
