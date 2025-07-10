using System;
using System.Data;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[UseSnapshotProtection]
	class GlbCompanyCampaignContactFilterBusinessObjectTestForDifferentInstances : TestCase
	{
		public void TestGetLastUsedLayout_FromTheOtherFactory()
		{
			var factory = new BusinessObjectFactory(Db.Connection) { RefreshEnabled = false };

			ZGuid filterPk;
			ZGuid campaignPk;
			CampaignPk(factory, out campaignPk, out filterPk);

			var separateInstanceTask = Task.Run(() =>
			{
				using (var taskConnection = Db.NewExtraConnectionToMainDb())
				{
					ChangeFilter(taskConnection, filterPk);
				}
			});

			if (!separateInstanceTask.Wait(5000))
			{
				throw new TimeoutException();
			}

			var campaign = factory.Load<GlbCompanyCampaign>(campaignPk);
			var campaignContactFilterStrip = new GlbCompanyCampaignContactFilterBusinessObject(campaign);

			var filter = campaignContactFilterStrip.GetLastUsedLayout();
			AssertEquals("Name two", filter.S9_FilterName);
			AssertEquals(ZBlob.FromAscii("New blob"), filter.S9_FilterData);
		}

		static void CampaignPk(BusinessObjectFactory factory, out ZGuid campaignPk, out ZGuid filterPk)
		{
			var campaign = factory.NewWithValidTestData<GlbCompanyCampaign>();
			var campaignContactFilterStrip = new GlbCompanyCampaignContactFilterBusinessObject(campaign);
			campaignContactFilterStrip.Factory.RefreshEnabled = false;
			((IFilterStripBusinessObjectInternals)campaignContactFilterStrip).LayoutContext = "GlbCompanyCampaignContact";
			var filter = factory.New<StmModuleFilter>();
			filter.S9_FilterName = "Name One";
			filter.S9_ModuleID = ((IFilterStripBusinessObjectInternals)campaignContactFilterStrip).LayoutContext;
			filter.S9_IsPublished = true;
			filter.S9_RelatedEntityID = campaign.PK;
			filter.S9_FilterData = ZBlob.FromAscii("Initial blob");
			campaignContactFilterStrip.SaveLastUsedLayout(filter.PK);
			factory.Save();

			filter = campaignContactFilterStrip.GetLastUsedLayout();

			campaignPk = campaign.PK;
			filterPk = filter.PK;
		}

		static void ChangeFilter(DbConnection connection, ZGuid filterPk)
		{
			using (var cmd = connection.Command($"UPDATE {StmModuleFilterSchema.Constants.SqlSchemaName}.{StmModuleFilterSchema.Constants.TableName} SET " +
												$"{StmModuleFilterSchema.S9_FilterName.Name}=@name, " +
												$"{StmModuleFilterSchema.S9_FilterData.Name}=@data " +
												$"WHERE {StmModuleFilterSchema.PK.Name}=@pk"))
			{
				cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, filterPk.ToGuid());
				cmd.AddParameter("@name", SqlDbType.NVarChar, 50, "Name two");
				cmd.AddParameter("@data", SqlDbType.VarBinary, (byte[])ZBlob.FromAscii("New blob"));
				cmd.ExecuteNonQuery();
			}
		}
	}
}
