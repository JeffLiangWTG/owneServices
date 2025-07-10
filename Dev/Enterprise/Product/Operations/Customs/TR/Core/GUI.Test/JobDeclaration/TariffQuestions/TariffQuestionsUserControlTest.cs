using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI.Testing
{
	public class TariffQuestionsUserControlTest : TestCaseWithFactory
	{
		public void TestTariffQuestionsGrid()
		{
			using (var control = new TariffQuestionsUserControl())
			{
				var grid = control.FindSingle<ZGrid>("TariffQuestionsGrid");
				AssertNotNull("Not null", grid);

				var allColumnStyles = grid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();
				AssertEquals("ColumnStyles", 5, allColumnStyles.Length);

				CombineAssertions("TariffQuestionsGrid Test", () =>
				{
					var columnStyle = grid.GetColumnStyle("ON_QuestionType");
					AssertEquals("Type", columnStyle.CaptionResourceString.Caption);

					columnStyle = grid.GetColumnStyle("ON_CPDecNum");
					AssertEquals("Code", columnStyle.CaptionResourceString.Caption);

					columnStyle = grid.GetColumnStyle("ON_AnswerCode");
					AssertEquals("Answer", columnStyle.CaptionResourceString.Caption);

					columnStyle = grid.GetColumnStyle("Description");
					AssertEquals("Description", columnStyle.CaptionResourceString.Caption);

					AssertColumnVisibleAndAtSpecificIndex<ZDropEditColumnStyleInfo>(CusEntryCPDec.Schema.ON_QuestionType, 0);
					AssertColumnVisibleAndAtSpecificIndex<ZTextBoxColumnStyleInfo>("QuestionTypeDescription", 1);
					AssertColumnVisibleAndAtSpecificIndex<ZCalcEditColumnStyleInfo>(CusEntryCPDec.Schema.ON_CPDecNum, 2);
					AssertColumnVisibleAndAtSpecificIndex<ZDropEditColumnStyleInfo>(CusEntryCPDec.Schema.ON_AnswerCode, 3);
					AssertColumnVisibleAndAtSpecificIndex<ZTextBoxColumnStyleInfo>("Description", 4);

					void AssertColumnVisibleAndAtSpecificIndex<TColumnInfo>(ZString columnName, ZInt expectedIndex)
					where TColumnInfo : ZGridColumnInfo
					{
						columnStyle = allColumnStyles.SingleOrDefault(x => x.ColumnName == columnName);
						AssertNotNull($"{columnName} not null", columnStyle);
						AssertType<TColumnInfo>($"Expected type for {columnStyle}", columnStyle);
						Assert($"{columnName} is visible", columnStyle.IsVisible);
						AssertEquals($"{columnName} is at specific index", expectedIndex, Array.IndexOf(allColumnStyles, columnStyle));
					}
				});
			}
		}
	}
}
