using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Module.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	public class GatewayConsolProfitShareRedistributionFilterControlTest : FilterControlBashFetchHintTest<ForwardingProfitShareRedistribution>
	{
		public void TestBashFetchForView_PSR_SystemCreateUser()
		{
			BashFetchForView("PSR_SystemCreateUser", 0);
		}

		public void TestBashFetchForView_PSR_SystemCreateTimeUtc()
		{
			BashFetchForView("PSR_SystemCreateTimeUtc", 0);
		}

		public void TestBashFetchForView_PSR_SystemLastEditUser()
		{
			BashFetchForView("PSR_SystemLastEditUser", 0);
		}

		public void TestBashFetchForView_PSR_SystemLastEditTimeUtc()
		{
			BashFetchForView("PSR_SystemLastEditTimeUtc", 0);
		}

		public void TestBashFetchForView_PSR_BatchNumber()
		{
			BashFetchForView("PSR_BatchNumber", 0);
		}

		[RequiresSTA]
		public void TestAuditColumsAreVisible()
		{
			using (var control = GetNewFilterStripControl())
			{
				var auditColumns = control.Grid.ColumnStyles.Cast<ZGridColumnInfo>().Where(c => c.GroupName.Caption == FilterStripAuditDetails.AuditDetailsGroupText.Caption).ToList();
				AssertEquals("Audit Columns should exist", 4, auditColumns.Count);
				Assert("All columns should be visible", auditColumns.All(c => c.IsVisible));
			}
		}

		#region Implementation

		protected override ZGuid[] CreateKeysForTest()
		{
			var result = new List<ZGuid>();
			var factory = new BusinessObjectFactory();

			for (var i = 0; i < 10; i++)
			{
				var redistributionBatch = factory.NewWithValidTestData<ForwardingProfitShareRedistribution>();
				var consol = factory.NewWithValidTestData<ForwardingConsol>();
				var consolProfitShare = redistributionBatch.ConsolProfitShares.AddNew();
				consolProfitShare.CPS_JK = consol.PK;
				consolProfitShare.CPS_RX_NKCurrency = "AUD";

				result.Add(redistributionBatch.PK);
			}

			factory.Save();

			return result.ToArray();
		}

		protected override ZFilterStripControl GetNewFilterStripControl()
		{
			var collection = GetNewCollection();
			var filterBusinessObject = new GatewayConsolProfitShareRedistributionFilterBusinessObject();
			return new GatewayConsolProfitShareRedistributionFilterControl(collection, filterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewCollection() => new ForwardingProfitShareRedistributionCollection(Factory);

		protected override SchemaPKColumn PkColumn => ProfitShareRedistributionSchema.PK;

		#endregion
	}
}
