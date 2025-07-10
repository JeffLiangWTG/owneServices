using System.Windows.Forms;
using CargoWise.Data;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class DivideAllLegacyEstimatedValuesByTwelveController
	{
		public void Show()
		{
			if (OrganisationsDataRegistry.Instance.DivideAllOpportunityValuesByTwelveHasRun.Value)
			{
				Globals.Message.ShowError(
					Res.GetString("D455A1E5-F0A4-4C1B-8C4B-7959D55DCF88", "This has already been run previously. This is a one-off function."),
					CannotDivideAllLegacyEstimatedValuesByTwelveString);

				return;
			}

			var dialogResult = Globals.Message.Show(Res.GetString("849AB39A-D894-4D73-A5A3-581692CF0C15", @"Are you sure you want to divide the legacy estimated values for all opportunities by 12?

Note: This does not recalculate the total estimated values."),
					Res.GetString("A28CEEB5-5D03-4C4C-9A16-F505195EB095", "Divide all Legacy Estimated Values by 12"),
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Exclamation,
					DialogResult.No);

			if (dialogResult == DialogResult.Yes)
			{
				DivideAllLegacyEstimatedValuesByTwelve();
			}
		}

		protected virtual void DivideAllLegacyEstimatedValuesByTwelve()
		{
			try
			{
				using (var progressForm = new ProgressForm())
				{
					progressForm.ShowCancelButton = false;
					progressForm.Show();
					progressForm.Status = Res.GetString("83BFECE2-3913-4F2C-9517-6D273BBDACEE", "Dividing all Legacy Estimated Values by 12..");
					UpdateOpportunityValuesAndSetRegistryFlagInTransaction();
				}

				Globals.Message.Show(Res.GetString("0001A2F7-74F6-46C4-902E-14B67595A846", "Divide all Legacy Estimated Values by 12 Completed."));
			}
			catch (SqlException ex)
			{
				Globals.Message.ShowError(
					ex.State == 1 ?
						Res.GetString("4E1C2888-C387-4D22-A7C7-7B908A819C94", "Another user has run this function at the same time.") :
						ex.Message,
					CannotDivideAllLegacyEstimatedValuesByTwelveString);
			}
		}

		static string CannotDivideAllLegacyEstimatedValuesByTwelveString
		{
			get { return Res.GetString("309D5CA9-88C8-4E30-B836-DF27A3AF95FE", "Cannot divide all Legacy Estimated Values by 12"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "One-off bulk operation on the whole table")]
		void UpdateOpportunityValuesAndSetRegistryFlagInTransaction()
		{
			if (Db.Connection.IsInTransactionOtherThanTransactionedTestCase)
			{
				Globals.Message.ShowError(Res.GetString("CEF1AE00-EE14-40D9-AC54-13C4ED4F9B82", "This function cannot be called in an existing transaction context."));
			}
			else
			{
				using (var transactionManager = Db.Connection.BeginTransactionWithManager())
				using (var cmd = Db.Connection.Command(DivideAllOpportunityValuesByTwelveSql))
				{
					cmd.AddParameterBasedOnDbColumn("@SystemLastEditUser", GlbStaff.CurrentUser.GS_Code.ToString(), OrgOpportunityValueSchema.PV_SystemLastEditUser);
					cmd.ExecuteNonQuery();
					transactionManager.CommitTransaction();
				}
			}
		}

		const string DivideAllOpportunityValuesByTwelveSql = @"
			-- Final check within transaction to handle concurrency
			DECLARE @HasAlreadyRun BIT =
				CASE
					WHEN EXISTS (
						SELECT 1 FROM dbo.StmData WITH (UPDLOCK, SERIALIZABLE)
						WHERE SD_Name = 'DivideAllOpportunityValuesByTwelveHasRun'
						AND SD_Owner IS NULL
						AND SD_DepartmentGuid IS NULL
						AND SD_BinaryValue = CONVERT(VARBINARY(MAX), N'True')
					) THEN 1
					ELSE 0
				END;

			IF (@HasAlreadyRun = 1)
			BEGIN
				RAISERROR ('Has already been run previously. This cannot be run multiple times.', 16, 1);
			END
			ELSE
			BEGIN
				UPDATE dbo.OrgOpportunityValue
					SET
						PV_Value /= 12,
						PV_Discount = CASE WHEN PV_DiscountBasis = 'PCT' THEN PV_Value * PV_DiscountPercent / 1200 ELSE PV_Discount END,
						PV_SystemLastEditTimeUtc = GETUTCDATE(),
						PV_SystemLastEditUser = @SystemLastEditUser;

				INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_BinaryValue)
					VALUES (NEWID(), 'DivideAllOpportunityValuesByTwelveHasRun', NULL, NULL, 'BOL', CONVERT(VARBINARY(MAX), N'True'));
			END";
	}
}
