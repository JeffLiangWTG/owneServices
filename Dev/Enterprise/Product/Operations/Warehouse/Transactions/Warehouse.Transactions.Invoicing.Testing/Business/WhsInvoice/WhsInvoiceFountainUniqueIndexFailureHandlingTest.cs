using System;
using System.Collections.Specialized;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Invoicing.Testing
{
	public class WhsInvoiceFountainUniqueIndexFailureHandlingTest : NumberFountainUniqueIndexFailureHandlingTest
	{
		protected override Type BizOTypeToTest => typeof(WhsInvoice);

		protected override SchemaColumn ColumnThatUsesNumberFountain => JobStorageSchema.ET_StorageJobNumber;

		protected override INumberFountainProxy NumberFountainToTest => Env.NumberFountains.WarehouseInvoiceNumber;

		protected override void SetExtraPropertyValuesAfterCreatingBizO(BusinessObject testBizO)
		{
			base.SetExtraPropertyValuesAfterCreatingBizO(testBizO);

			var org = Factory.NewWithValidTestData<OrgHeader>();

			var invoice = (WhsInvoice)testBizO;
			invoice.ET_OH_Client = org.PK;
		}

		protected override NameValueCollection AdditionalInsertValues
		{
			get
			{
				var values = base.AdditionalInsertValues;

				var org = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();

				values.Add(JobStorageSchema.Constants.ET_OH_Client, string.Format(Culture.Invariant, "'{0}'", org.PK));
				values.Add(JobStorageSchema.Constants.ET_StorageType, "'foo'");
				return values;
			}
		}

		#region data

		#endregion
	}
}