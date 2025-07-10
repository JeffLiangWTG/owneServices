using System;
using System.Collections.Specialized;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class DocketIDFountainUniqueIndexFailureHandler<T> : NumberFountainUniqueIndexFailureHandlingTest
		where T : WhsDocket
	{
		#region NumberFountainUniqueIndexFailureHandlingTest

		protected override Type BizOTypeToTest
		{
			get { return typeof(T); }
		}

		protected override SchemaColumn ColumnThatUsesNumberFountain
		{
			get { return WhsDocketSchema.WD_DocketID; }
		}

		protected override INumberFountainProxy NumberFountainToTest
		{
			get { return Env.NumberFountains.WarehouseDocketID; }
		}

		protected override void SetExtraPropertyValuesAfterCreatingBizO(BusinessObject testBizO)
		{
			base.SetExtraPropertyValuesAfterCreatingBizO(testBizO);

			var docket = (T)testBizO;
			docket.WD_OH_Client = Org.PK;
			docket.WD_WW_Whs = Warehouse.PK;
		}

		protected override NameValueCollection AdditionalInsertValues
		{
			get
			{
				var valueCollection = base.AdditionalInsertValues;
				valueCollection.Add(WhsDocketSchema.Constants.WD_OH_Client, string.Format(Culture.Invariant, "'{0}'", Org.PK));
				valueCollection.Add(WhsDocketSchema.Constants.WD_WW_Whs, string.Format(Culture.Invariant, "'{0}'", Warehouse.PK));
				valueCollection.Add(WhsDocketSchema.Constants.WD_BookingDate, string.Format(Culture.Invariant, "'{0}'", DateTime.Now));
				valueCollection.Add(WhsDocketSchema.Constants.WD_DocketType, string.Format(Culture.Invariant, "'{0}'", GetDocketType()));
				valueCollection.Add(WhsDocketSchema.Constants.WD_DocketSubType, string.Format(Culture.Invariant, "'{0}'", GetDocketSubType()));
				valueCollection.Add(WhsDocketSchema.Constants.WD_ExternalReference, "'W123'");
				return valueCollection;
			}
		}

		#endregion

		#region GetDocketType

		protected abstract string GetDocketType();
		protected abstract string GetDocketSubType();

		#endregion

		#region Data

		OrgHeader Org => org ?? (org = Factory.NewWithValidTestData<OrgHeader>());
		OrgHeader org;

		WhsWarehouse Warehouse
		{
			get
			{
				if (warehouse == null)
				{
					var helper = new WhsTestHelperFunctionsEnv(Factory);
					warehouse = helper.CreateWarehouse("Wh1", Org.MainAddress, GlbBranch.CurrentBranch);
				}
				return warehouse;
			}
		}

		WhsWarehouse warehouse;

		#endregion
	}
}
