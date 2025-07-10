using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class GlbCompanyCampaignSenderTestCaseNoFactory : TestCase
	{
		[UseSnapshotProtection]
		public void TestDripSend_CorThenCor_UseLastEmailWithInactiveOldCoordinator()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };

			var data = GlbCompanyCampaignSenderTest.CreateDripData(factory, 2);
			data.Touch1.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
			data.Touch2.G0_EmailSenderOption = EmailSenderOptionCodeDescriptionList.Codes.COR;
			data.Touch2.G0_UseLastEmailSenderAddress = true;
			factory.Save();

			var separateInstanceTask = Task.Run(() =>
			{
				using (Db.DisposableActionForDbConnection())
				using (var taskConnection = Db.NewExtraConnectionToMainDb())
				{
					ChangeStaffActivity(taskConnection, data.Touch1.CampaignCoordinator.PK);
				}
			});

			if (!separateInstanceTask.Wait(5000))
			{
				throw new TimeoutException();
			}

			data.Touch1.TransitionAndSchedule();
			data.Touch2.CampaignsItemsSent.Reload(true);
			AssertEquals("All 2 should be sent", 2, data.Touch2.CampaignsItemsSent.Count);

			foreach (var item in data.Touch2.CampaignsItemsSent.Cast<GlbCompanyCampaignItem>())
			{
				AssertEquals("Sender is the new coordinator", data.Touch2.CampaignCoordinator.GS_Code, item.G8_GS_NKSender);
				AssertEquals("Sender is the new coordinator", data.Touch2.CampaignCoordinator.GS_EmailAddress, item.G8_SenderEmailAddress);
				AssertEquals("Sender is the new coordinator", data.Touch2.CampaignCoordinator.GS_FullName, item.G8_EmailSenderName);
			}
		}

		static void ChangeStaffActivity(DbConnection connection, ZGuid filterPk)
		{
			using (var cmd = connection.Command($"UPDATE {GlbStaffSchema.Constants.SqlSchemaName}.{GlbStaffSchema.Constants.TableName} SET GS_SystemLastEditTimeUtc = GetUtcDate(), GS_SystemLastEditUser = 'E', " +
																					$"{GlbStaffSchema.GS_IsActive.Name}=@isActive " +
																					$"WHERE {GlbStaffSchema.PK.Name}=@pk"))
			{
				cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, filterPk.ToGuid());
				cmd.AddParameter("@isActive", SqlDbType.Bit, 0);
				cmd.ExecuteNonQuery();
			}
		}
	}
}
