using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.Guarantees;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(DummyHostFormForBashing))]
	sealed class GuaranteeAccessLineControlTest : ZFormBasherTest
	{
		public void TestMainAccessPersonNameTextBox()
		{
			using (var control = new GuaranteeAccessLineControl())
			{
				var mainAccessPersonNameTextBox = control.MainAccessPersonNameTextBox;
				CombineAssertions(() =>
				{
					AssertEquals("BindTo", nameof(BaseCusGuaranteeHeader.MainAccessPersonName), mainAccessPersonNameTextBox.BindTo);
					AssertEquals("Label", "For Person (Name)", mainAccessPersonNameTextBox.CaptionResourceString.Caption);
					AssertEquals("CharacterCasing", System.Windows.Forms.CharacterCasing.Normal, mainAccessPersonNameTextBox.CharacterCasing);
				});
			}
		}

		public void TestAdditionalAccessCodesGrid()
		{
			using (var control = new GuaranteeAccessLineControl())
			{
				CombineAssertions(() =>
				{
					AssertSequencesEqual("Columns",
						new[] { nameof(CusGuaranteeRule.CPR_ValueFrom), nameof(CusGuaranteeRule.CPR_Description) },
						control.AdditionalAccessCodesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));

					var valueFromColumnStyle = control.AdditionalAccessCodesGrid.GetColumnStyle(nameof(CusGuaranteeRule.CPR_ValueFrom));
					AssertEquals("CPR_ValueFrom Caption", "Access Code", valueFromColumnStyle.CaptionResourceString.Caption);

					var valueToColumnStyle = control.AdditionalAccessCodesGrid.GetColumnStyle(nameof(CusGuaranteeRule.CPR_Description));
					AssertEquals("CPR_Description Caption", "For Person (Name)", valueToColumnStyle.CaptionResourceString.Caption);
					AssertEquals("CharacterCasing", CharacterCasing.Normal, valueToColumnStyle.CharacterCasing);
				});
			}
		}

		public void TestHideAccessCodes()
		{
			using (var control = new GuaranteeAccessLineControl())
			{
				CombineAssertions(() =>
				{
					AssertEquals("MainAccessCodeTextBox", '*', control.FindSingle<ZTextBox>("MainAccessCodeTextBox").PasswordChar);
					AssertEquals("Grid - CPR_ValueFrom", '*', ((ZTextBoxColumnStyleInfo)control.FindSingle<ZGrid>("AdditionalAccessCodesGrid").ColumnStyles[0]).PasswordChar);
				});
			}
		}

		protected override Form GetFormToBashCore()
		{
			var form = new DummyHostFormForBashing(Factory.New<BaseCusGuaranteeHeader>());
			form.CaptionRenderingEnabled = true;
			form.Text = "ZTemplateForm@#$_Basher_Test";
			return form;
		}
	}
}
