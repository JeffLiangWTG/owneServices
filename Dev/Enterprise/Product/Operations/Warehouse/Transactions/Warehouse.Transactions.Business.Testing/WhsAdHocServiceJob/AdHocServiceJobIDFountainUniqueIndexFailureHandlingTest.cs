using System;
using System.Collections.Specialized;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class AdHocServiceJobIDFountainUniqueIndexFailureHandlingTest : NumberFountainUniqueIndexFailureHandlingTest
	{
		protected override Type BizOTypeToTest => typeof(WhsAdHocServiceJob);

		protected override SchemaColumn ColumnThatUsesNumberFountain => WhsAdHocServiceJobSchema.WSJ_JobNumber;

		protected override INumberFountainProxy NumberFountainToTest => Env.NumberFountains.WorkItemNo;

		protected override NameValueCollection AdditionalInsertValues
		{
			get
			{
				if (values == null)
				{
					values = base.AdditionalInsertValues;
					values.Add(WhsAdHocServiceJobSchema.Constants.WSJ_SystemCreateTimeUtc, ZDateTime.Now.ToString("yyyy-MM-dd"));
					values.Add(WhsAdHocServiceJobSchema.Constants.WSJ_SystemLastEditTimeUtc, ZDateTime.Now.ToString("yyyy-MM-dd"));
					values.Add(WhsAdHocServiceJobSchema.Constants.WSJ_SystemCreateUser, "'Bob'");
					values.Add(WhsAdHocServiceJobSchema.Constants.WSJ_SystemLastEditUser, "'Bob'");
					values.Add(WhsAdHocServiceJobSchema.Constants.WSJ_BillingDate, $"'{ZDateTime.Now.ToString("yyyy-MM-dd")}'");
					values.Add(WhsAdHocServiceJobSchema.Constants.WSJ_IsFinalised, "'0'");
					values.Add(WhsAdHocServiceJobSchema.Constants.WSJ_CustomerReference, "'1'");
					values.Add(WhsAdHocServiceJobSchema.Constants.WSJ_OH_Client, $"'{Org.ToString()}'");
					values.Add(WhsAdHocServiceJobSchema.Constants.WSJ_WW_Whs, $"'{Whs.PK.ToString()}'");
				}
				return values;
			}
		}
		NameValueCollection values;

		protected override void SetExtraPropertyValuesAfterCreatingBizO(BusinessObject testBizO)
		{
			var adHocServiceJob = (WhsAdHocServiceJob)testBizO;
			adHocServiceJob.WSJ_OH_Client = Org;
			adHocServiceJob.WSJ_WW_Whs = Whs.PK;
		}

		BusinessObject Whs => whs ?? (whs = Helper.CreateWarehouse("Warehouse", "A"));
		BusinessObject whs;

		ZGuid Org => (org ?? (org = Helper.CreateClient("CL1"))) ?? ZGuid.Empty;
		ZGuid? org;

		IWhsTransactionTestHelper Helper => helper ?? (helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory));
		IWhsTransactionTestHelper helper;
	}
}
