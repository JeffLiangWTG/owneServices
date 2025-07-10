using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GUI.Testing
{
	/// <summary>
	/// Do not inherit this. Using mock and test job declaration etc
	/// </summary>
	sealed class AmendmentDetectionForDeclarationFormTest : AmendmentDetectionOnSavingAbstractTest
	{
		public void TestDeclarationDeletingIsReported()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var declaration = Factory.New<BaseJobDeclaration>();
				using (var form = new BaseJobDeclarationForm(declaration))
				{
					form.Show();
					var row = ((INeedRow)declaration).Row;
					var rows = row.Table.Rows;
					ErrorReporter.Clear();
					var newDec = Factory.New<BaseJobDeclaration>();
					var newRow = ((INeedRow)newDec).Row;
					rows.Remove(newRow);
					AssertEquals("Logging should only be done for the form's Declaration", "", ErrorReporter.LastKeyReported);
					CombineAssertions(() =>
					{
						rows.Remove(row);
						AssertEquals("LastKeyReported", "Declaration should not be deleted", ErrorReporter.LastKeyReported);
						AssertEquals("LastMessageReported", $"Declaration row is being deleted (Action=Delete, State=Added,PK={declaration.PK})", ErrorReporter.LastMessageReported);
						ErrorReporter.Clear();
					});
				}
				var dec2 = Factory.New<BaseJobDeclaration>();
				var dec2Row = ((INeedRow)dec2).Row;
				dec2Row.Table.Rows.Remove(dec2Row);
				AssertEquals("Logging should have been removed when form was disposed", "", ErrorReporter.LastKeyReported);
			}
		}

		protected override IDecFormOrPlugIn GetDeclarationFormOrPlugIn(BaseJobDeclaration declaration)
		{
			return new DeclarationFormForTest(declaration);
		}
	}
}
