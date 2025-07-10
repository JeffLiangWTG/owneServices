using System;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(WarningAcknowledgementForm))]
	sealed class WarningAcknowledgementFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var warning = Factory.NewWithValidTestData<GenCustomAddOnRuleAck>();
			warning.XK_ParentID = Guid.NewGuid();
			warning.XK_RuleID = Core.Constants.CargoWiseOneGenCustomAddOnRuleIDs.PhoneNumberFormatValidation;
			warning.XK_ParentTableCode = "GS";
			warning.XK_SystemCreateTimeUtc = DateTime.UtcNow;
			warning.XK_SystemCreateUser = "E";
			Factory.Save();

			var form = new WarningAcknowledgementForm(warning)
			{
				Width = 800,
				Height = 600,
				CaptionRenderingEnabled = true
			};
			return form;
		}
	}
}
