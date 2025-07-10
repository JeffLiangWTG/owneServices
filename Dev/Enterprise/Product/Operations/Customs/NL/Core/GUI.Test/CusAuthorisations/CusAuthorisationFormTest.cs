using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.NL.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing
{
	[TestedType(typeof(CusAuthorisationForm))]
	sealed class CusAuthorisationFormTest : ZFormBasherTest
	{
		public void TestAuthorisationRuleGrid()
		{
			(string ColumnName, string ColumnCaption, int ColumnWidth, Type columnStyleType)[] orderedColumnDetails = {
				(CusAuthorisationRule.Schema.CPR_RuleCode, "Rule Code", ControlDpiScalingHelper.ScaleToCurrentDpiX(72), typeof(ZDropEditColumnStyle)),
				("GoodsLocationDescription", "Goods Location", ControlDpiScalingHelper.ScaleToCurrentDpiX(100), typeof(GoodsLocationColumnStyle)),
				(CusAuthorisationRule.Schema.CPR_Description, "Description", ControlDpiScalingHelper.ScaleToCurrentDpiX(280), typeof(ZMultiControlColumnStyle))
			};

			using (var form = new CusAuthorisationForm(cusAuthorisationHeader))
			{
				form.Show();
				var groupBox = form.FindSingle<ZGroupBox>("AuthorisationRuleGroupBox");
				var grid = form.FindSingle<ZGrid>("AuthorisationRuleGrid");
				CombineAssertions(() =>
				{
					AssertEquals("Grid caption", "Authorization Rules", groupBox.CaptionResourceString.Caption);
					foreach (var (columnName, columnCaption, columnWidth, columnStyleType) in orderedColumnDetails)
					{
						var columnStyle = grid.GetColumnStyle(columnName);
						AssertEquals($"{columnName} caption", columnCaption, grid.GetColumnCaption(columnName));
						AssertEquals($"{columnName} width", columnWidth, columnStyle.Width);
						AssertEquals($"{columnName} type", columnStyleType, columnStyle.ColumnStyleType);
					}
				});
			}
		}

		protected override bool ShouldTestFormIsFullyTranslatable => false; // Errors on the values of AuthorisationTypeZDropEdit, they are filled by NLCusAuthorisationHeaderTypeList which contains constant values

		protected override Form GetFormToBashCore()
		{
			cusAuthorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			Factory.Save();
			return new CusAuthorisationForm(cusAuthorisationHeader);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusAuthorisationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
		}
		CusAuthorisationHeader cusAuthorisationHeader;
	}
}
