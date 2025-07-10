using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.Module.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public class AdHocServiceJobFilterControlTest : WhsEnvFilterControlDBHitsTestCase<WhsAdHocServiceJobCollection, AdHocServiceJobFilterBusinessObject>
	{
		#region TestNewZFilterStrip

		public void TestNewZFilterStrip()
		{
			var collection = new WhsAdHocServiceJobCollection(Factory);
			var filterBizO = new AdHocServiceJobFilterBusinessObject();
			using (var filterControl = GetNewFilterControl(collection, filterBizO))
			{
				var strip = filterBizO.FilterStrips.AddNew();
				using (var filterStrip = filterControl.AddFilterStrip(strip))
				{
					AssertType<WhsWorkflowFilterStrip>(filterStrip);
				}
			}
		}

		#endregion

		#region SetupData

		protected override void SetupData()
		{
			var today = ZDate.Today;

			for (int i = 0; i < 10; i++)
			{
				var client = Helper.CreateClient("C" + i);
				var branch = Factory.NewWithValidTestData<GlbBranch>();
				var warehouse = Helper.CreateWarehouse("Wh" + i);
				warehouse.WW_GB_RelatedCompanyBranch = branch.PK;
				var job = Helper.CreateWhsAdHocServiceJob(warehouse, client, today);
				job.WSJ_CustomerReference = "referenceNo" + i; // This line implicitly creates a JobHeader for job
			}
		}

		#endregion

		protected override bool ShouldCheckForUnusedFetchHints(string columnName) => false;

		#region GetExpectedHitsDictionary

		protected override Dictionary<string, Dictionary<string, int>> GetExpectedHitsDictionary()
		{
			var baseHits = GetBaseHits();
			var hitsDictionary = new Dictionary<string, Dictionary<string, int>>();

			var customerReferenceNoHits = new Dictionary<string, int>(baseHits);
			customerReferenceNoHits.Add(JobHeaderSchema.Constants.TableName, 0);
			hitsDictionary.Add(WhsAdHocServiceJob.Schema.WSJ_CustomerReference, customerReferenceNoHits);

			var clientHits = new Dictionary<string, int>(baseHits);
			clientHits.Add(JobHeaderSchema.Constants.TableName, 0);
			clientHits.Add(OrgAddressSchema.Constants.TableName, 0);
			clientHits.Add(OrgHeaderSchema.Constants.TableName, 1);

			hitsDictionary.Add(WhsAdHocServiceJob.Schema.Client + "+" + OrgHeaderSchema.Constants.OH_Code, clientHits);
			hitsDictionary.Add(WhsAdHocServiceJob.Schema.Client + "+" + OrgHeaderSchema.Constants.OH_FullName, clientHits);

			var warehouseHits = new Dictionary<string, int>(baseHits);
			warehouseHits.Add(WhsWarehouseSchema.Constants.TableName, 1);
			hitsDictionary.Add(WhsAdHocServiceJob.Schema.Warehouse + "+WW_WarehouseNameMultilingual", warehouseHits);

			return hitsDictionary;
		}

		#endregion

		#region GetBaseHits

		protected override Dictionary<string, int> GetBaseHits()
		{
			var baseHits = new Dictionary<string, int>();
			baseHits.Add(WhsAdHocServiceJobSchema.Constants.TableName, 1);
			return baseHits;
		}

		#endregion

		#region GetNewCollection

		protected override WhsAdHocServiceJobCollection GetNewCollection(BusinessObjectFactory factory)
		{
			return new WhsAdHocServiceJobCollection(factory);
		}

		#endregion

		#region GetNewFilterBusinessObject

		protected override AdHocServiceJobFilterBusinessObject GetNewFilterBusinessObject() => new AdHocServiceJobFilterBusinessObject();

		#endregion

		#region GetNewFilterControl

		protected override ZFilterStripControl GetNewFilterControl(WhsAdHocServiceJobCollection collection, AdHocServiceJobFilterBusinessObject filterBizO)
		{
			return new AdHocServiceJobFilterControl(collection, filterBizO);
		}

		#endregion

		#region Implementation

		protected override WhsTestHelperFunctionsEnv GetNewTestHelperFunctions()
		{
			return new WhsTestHelperFunctions(Factory);
		}

		new WhsTestHelperFunctions Helper => (WhsTestHelperFunctions)base.Helper;

		#endregion
	}
}
