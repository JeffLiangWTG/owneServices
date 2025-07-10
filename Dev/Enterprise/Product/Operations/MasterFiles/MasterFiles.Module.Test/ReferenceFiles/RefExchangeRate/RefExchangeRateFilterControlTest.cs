using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class RefExchangeRateFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestFilterControlContainsAllColumns()
		{
			var collection = new RefExchangeRateCollection(Factory, new ZQuery(RefExchangeRateSchema.RE_GC, GlbCompany.CurrentCompany.PK));
			collection[0].RE_OH_Client = OrgHeader.DefaultOrg.PK;
			var filterBizO = new RefExchangeRateFilterBusinessObject();
			using (var control = new RefExchangeRateFilterControlForTest(collection, filterBizO))
			{
				var columnStyle = control.Grid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();

				AssertNotNull(columnStyle.FirstOrDefault(u => u.ColumnName == "RE_RX_NKExCurrency"));
				AssertNotNull(columnStyle.FirstOrDefault(u => u.ColumnName == "RE_ExRateType"));
				AssertNotNull(columnStyle.FirstOrDefault(u => u.ColumnName == "RE_StartDate"));
				AssertNotNull(columnStyle.FirstOrDefault(u => u.ColumnName == "RE_ExpiryDate"));
				AssertNotNull(columnStyle.FirstOrDefault(u => u.ColumnName == "RE_OH_Client"));
			}
		}

		[RequiresSTA]
		public void TestLocalClientColumn()
		{
			var collection = new RefExchangeRateCollection(Factory, new ZQuery(RefExchangeRateSchema.RE_GC, GlbCompany.CurrentCompany.PK));
			var filterBizO = new RefExchangeRateFilterBusinessObject();
			using (var control = new RefExchangeRateFilterControlForTest(collection, filterBizO))
			{
				var columnStyle = control.Grid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();

				AssertNotNull(columnStyle.FirstOrDefault(u => u.ColumnName == "RE_RX_NKExCurrency"));
				AssertNotNull(columnStyle.FirstOrDefault(u => u.ColumnName == "RE_ExRateType"));
				AssertNotNull(columnStyle.FirstOrDefault(u => u.ColumnName == "RE_StartDate"));
				AssertNotNull(columnStyle.FirstOrDefault(u => u.ColumnName == "RE_ExpiryDate"));
				AssertNull(columnStyle.FirstOrDefault(u => u.ColumnName == "RE_OH_Client"));
			}
		}
	}
}
