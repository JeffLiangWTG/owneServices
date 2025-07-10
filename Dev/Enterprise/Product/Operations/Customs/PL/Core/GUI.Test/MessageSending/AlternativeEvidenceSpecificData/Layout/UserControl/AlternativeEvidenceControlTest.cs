using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.PL.Business;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class AlternativeEvidenceControlTest : TestCaseWithFactory
{
	public void TestDataSourceType()
	{
		using var control = new AlternativeEvidenceControl();
		AssertEquals(typeof(BaseMessageSendingObjectParent), control.DataSourceType);
	}

	public void TestControls()
	{
		var expectedControlsAmount = 4;
		using var control = new AlternativeEvidenceControl();
		AssertEquals($"CC583SpecificDataControl should countain only {expectedControlsAmount} controls", expectedControlsAmount, control.Controls.Count);
	}

	public void TestColumnNameOfAlternativeEvidenceGrid()
	{
		using var control = new AlternativeEvidenceControl();

		var alternativeEvidenceGrid = control.AlternativeEvidenceGrid;
		AssertSequencesEqual(new[] { nameof(AlternativeEvidence.EvidenceType), nameof(AlternativeEvidence.DocType), nameof(AlternativeEvidence.Reference) }, alternativeEvidenceGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(c => c.ColumnName));
	}
}
