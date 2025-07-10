using System.Reflection;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class DocumentCommonConsolFormLayoutTest : TestCaseWithFactory
	{
		public void TestLayoutControlsFromDataContextLoadListDocument()
		{
			const int Top = 16;
			const int ZRadioHeight = 26;
			const int ZCheckBoxHeight = 24;

			CommonConsol commonConsol = Factory.New<CommonConsol>();
			DocumentCommonConsol documentCommonConsol = new DocumentCommonConsol(commonConsol, Core.Constants.DataContext.LoadListDocument);
			using (DocumentCommonConsolForm form = new DocumentCommonConsolForm(documentCommonConsol))
			{
				ZCheckBox includeConsignor = (ZCheckBox)typeof(DocumentCommonConsolForm).GetField("IncludeConsignorCheckBox", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form);
				Assertion.AssertEquals("Consignor top should be ", Top, includeConsignor.Top);

				ZCheckBox includeConsignee = (ZCheckBox)typeof(DocumentCommonConsolForm).GetField("IncludeConsigneeCheckBox", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form);
				Assertion.AssertEquals("Consignee top should be ", Top + ZCheckBoxHeight, includeConsignee.Top);

				ZCheckBox includeCustomsBroker = (ZCheckBox)typeof(DocumentCommonConsolForm).GetField("IncludeCustomsBrokerCheckBox", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form);
				Assertion.AssertEquals("None top should be ", Top + (2 * ZCheckBoxHeight), includeCustomsBroker.Top);

				ZRadioButton includeAllShipments = (ZRadioButton)typeof(DocumentCommonConsolForm).GetField("AllShipmentsRadioButton", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form);
				Assertion.AssertEquals("AllShipments top should be ", Top + (3 * ZCheckBoxHeight), includeAllShipments.Top);

				ZRadioButton includePacked = (ZRadioButton)typeof(DocumentCommonConsolForm).GetField("PackedRadioButton", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form);
				Assertion.AssertEquals("IncludePacked top should be ", Top + (3 * ZCheckBoxHeight) + ZRadioHeight, includePacked.Top);

				ZRadioButton includeUnPacked = (ZRadioButton)typeof(DocumentCommonConsolForm).GetField("UnpackedRadioButton", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form);
				Assertion.AssertEquals("IncludeUnPacked top should be ", Top + (3 * ZCheckBoxHeight) + (2 * ZRadioHeight), includeUnPacked.Top);

				ZGroupBox optionsGroupBox = (ZGroupBox)typeof(DocumentCommonConsolForm).GetField("OptionsGroupBox", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form);
				Assertion.AssertEquals("Options GroupBox height should be ", (2 * Top) + (3 * ZCheckBoxHeight) + (3 * ZRadioHeight), optionsGroupBox.Height);

				Assertion.AssertEquals("Options GroupBox text should be ", "Include", optionsGroupBox.Text);
				Assertion.AssertEquals("Caption should be ", "Document Options", form.Text);
			}
		}
	}
}
