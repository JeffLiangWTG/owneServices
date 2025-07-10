using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.NL.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI.Testing;

sealed class ExportBottomSectionUserControlTest : TestCaseWithFactory
{
	public void TestControls()
	{
		using (var control = new ExportBottomSectionUserControl())
		{
			CombineAssertions(() =>
			{
				AssertNotNull("Alternative Proof Details group box", control.FindSingleOrDefault<ZGroupBox>("AlternativeProofDetailsGroupBox"));
				AssertNotNull("Annotation", control.FindSingleOrDefault<ZTextBox>("AnnotationTextBox"));
				AssertNotNull("Exit Type", control.FindSingleOrDefault<ZDropEdit>("ExitTypeDropEdit"));
				AssertNotNull("Exit Date", control.FindSingleOrDefault<ZDateEdit>("ExitDateEdit"));
				AssertNotNull("Exit Office", control.FindSingleOrDefault<ZCodeFindBox>("ExitCustomsOfficeCodeFindBox"));
				AssertNotNull("Export Office", control.FindSingleOrDefault<ZTextBox>("ExportCustomsOfficeTextBox"));
				AssertNotNull("Alternative Evidence group box", control.FindSingleOrDefault<ZGroupBox>("AlternativeEvidenceGroupBox"));
				AssertNotNull("Alternative Evidence grid", control.FindSingleOrDefault<ZGrid>("AlternativeEvidenceGrid"));
			});
		}
	}

	public void TestAlternativeEvidence_NumberOfRows()
	{
		using (var control = new ExportBottomSectionUserControl())
		{
			var alternativeEvidenceGrid = control.FindSingleOrDefault<ZGrid>("AlternativeEvidenceGrid");
			AssertEquals("Number of rows in Alternative Evidence Grid", 9, alternativeEvidenceGrid.MaximumRows);
		}
	}

	public void TestAlternativeEvidence_Columns()
	{
		using (var control = new ExportBottomSectionUserControl())
		{
			var alternativeEvidenceGrid = control.FindSingleOrDefault<ZGrid>("AlternativeEvidenceGrid");
			var columnNames = alternativeEvidenceGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName).ToArray();

			CombineAssertions(() =>
			{
				AssertSequencesEqual("Columns", new[] { nameof(AlternativeEvidence.DocType), nameof(AlternativeEvidence.DocTypeDescription), nameof(AlternativeEvidence.Reference) }, columnNames);

				var docTypeColumnStyle = alternativeEvidenceGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == nameof(AlternativeEvidence.DocType));
				AssertEquals("DocType-ColumnStyle", typeof(ZDropEditColumnStyle), docTypeColumnStyle.ColumnStyleType);
				AssertEquals("DocType-Width", 80, docTypeColumnStyle.Width);

				var docTypeDescriptionColumnStyle = alternativeEvidenceGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == nameof(AlternativeEvidence.DocTypeDescription));
				AssertEquals("DocTypeDescription-ColumnStyle", typeof(ZTextBoxColumnStyle), docTypeDescriptionColumnStyle.ColumnStyleType);
				AssertEquals("DocTypeDescription-Width", 150, docTypeDescriptionColumnStyle.Width);

				var referenceColumnStyle = alternativeEvidenceGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == nameof(AlternativeEvidence.Reference));
				AssertEquals("Reference-ColumnStyle", typeof(ZTextBoxColumnStyle), referenceColumnStyle.ColumnStyleType);
				AssertEquals("Reference-Width", 150, referenceColumnStyle.Width);
			});
		}
	}
}
