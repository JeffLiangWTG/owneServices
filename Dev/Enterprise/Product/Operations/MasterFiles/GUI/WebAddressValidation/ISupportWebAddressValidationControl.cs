using System;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.WebAddressValidation
{
	public interface ISupportWebAddressValidationControl
	{
		ZTextBox AddressCodeControl { get; }
		ZTextBox Address1Control { get; }
		ZTextBox Address2Control { get; }
		ZTextBox CityControl { get; }
		ZTextBox PostcodeControl { get; }
		ZDropEdit StateControl { get; }
		ZCodeFindBox CountryControl { get; }
		ZTextBox AdditionalAddressInformationControl { get; }
		ZButton ValidateButton { get; }
		bool ValidationJustForced { get; set; }
		Control SuggestionWindowParentControl { get; }
		bool ProcessManuallyVerifyShortCutKey(ref Message msg, Keys keyData);
		ISupportWebAddressValidation AddressForValidation { get; }
		ZButton ClearAddressFieldsButton { get; }
		Func<Keys, bool> AddressValidationProcessCmdKey { get; set; }
	}
}
