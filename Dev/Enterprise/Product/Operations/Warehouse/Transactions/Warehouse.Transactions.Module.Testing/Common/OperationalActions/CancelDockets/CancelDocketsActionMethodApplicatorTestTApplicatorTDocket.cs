using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	public abstract class CancelDocketsActionMethodApplicatorTest<TApplicator, TDocket> : OperationalActionMethodApplicatorTest
		where TDocket : WhsDocket
		where TApplicator : CancelDocketsActionMethodApplicator<TDocket>
	{
		#region CancelDockets

		public void TestCancelDockets()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var docket_withCharge = GetNewDocket();
			docket_withCharge.WD_DocketID = "W5";
			docket_withCharge.WD_DocketStatus = DocketStatus.Codes.Entered;
			helper.CreateAccountingDataWithNoCharge(docket_withCharge);
			docket_withCharge.JobHeader.JH_JobNum = "JobNum01";
			Factory.Save();

			var docket_Entered = GetNewDocket();
			var docket_Finalised = GetNewDocket();
			var docket_Cancelled1 = GetNewDocket();
			var docket_Cancelled2 = GetNewDocket();

			docket_Entered.WD_DocketID = "W1";
			docket_Finalised.WD_DocketID = "W2";
			docket_Cancelled1.WD_DocketID = "W3";
			docket_Cancelled2.WD_DocketID = "W4";

			docket_Entered.WD_DocketStatus = DocketStatus.Codes.Entered;
			docket_Finalised.WD_DocketStatus = DocketStatus.Codes.Finalised;
			docket_Cancelled1.WD_DocketStatus = DocketStatus.Codes.Cancelled;

			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_IsVirtualWarehouse = true;
			docket_Cancelled2.WD_DocketStatus = DocketStatus.Codes.Entered;
			docket_Cancelled2.AddEvents(Events.Cancelled);
			docket_Cancelled2.WD_WW_Whs = warehouse.PK;

			ApplyApplicator(new WhsDocket[] { docket_Entered, docket_Finalised, docket_Cancelled1, docket_Cancelled2, docket_withCharge }, GetExpectedMessage());
		}

		protected abstract string GetExpectedMessage();

		#endregion

		#region AddFetchHints

		public void TestAddFetchHintsWhenCancelDockets()
		{
			for (int i = 0; i < 20; i++)
			{
				var docket = (TDocket)GetNewDocket();
				docket.WD_DocketID = $"W{i}";
				docket.WD_DocketStatus = DocketStatus.Codes.Entered;
				docket.WD_OH_Client = Helper.CreateClient($"Org{i}").PK;
				docket.WD_WW_Whs = Helper.CreateWarehouse($"Whs{i}", "B", 2, 1).PK;

				GetNewDocketLine(docket, Helper.CreateProduct($"P{i}", docket.Client), 10m);
			}
			Factory.Save();

			var newfactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var dockets = newfactory.Load<TDocket>(new ZQuery());
			var expectedDbHits = new Dictionary<string, int>()
			{
				{ GlbBranchSchema.Constants.TableName, 1 },
				{ JobHeaderSchema.Constants.TableName, 21 },
				{ OrgAddressSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 1 },
				{ WhsDocketLineSchema.Constants.TableName, 1 },
				{ WhsPickLineSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 }
			};
			AddCustomFetchHint(expectedDbHits);
			AddDocketSpecificFetchHint(expectedDbHits);

			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newfactory))
			{
				SimulateRun(dockets, true);
			}
		}

		protected abstract void AddCustomFetchHint(Dictionary<string, int> expectedDbHits);

		protected virtual void AddDocketSpecificFetchHint(Dictionary<string, int> expectedDbHits)
		{
		}

		#endregion

		#region Implementation

		protected WhsDocket GetNewDocket()
		{
			return Factory.NewWithValidTestData<TDocket>();
		}

		protected abstract WhsDocketLine GetNewDocketLine(TDocket docket, OrgSupplierPart part, ZDecimal quantity);

		protected WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;

		#endregion

	}
}
