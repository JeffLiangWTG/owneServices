using System.Web.UI.WebControls;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(OrgSupplierPartColumnProvider))]
	sealed class OrgSupplierPartColumnProviderTest : GridColumnProviderTest
	{
		#region TestCases

		public override void TestColumnKeys()
		{
			isLookUp = true;
			SetupNewProvider();
			base.TestColumnKeys();

			isLookUp = false;
			SetupNewProvider();
			base.TestColumnKeys();
		}

		public override void TestDefaultColumns()
		{
			isLookUp = true;
			SetupNewProvider();
			base.TestDefaultColumns();

			isLookUp = false;
			SetupNewProvider();
			base.TestDefaultColumns();
		}

		public override void TestFixOldInvalidLayout()
		{
			isLookUp = true;
			SetupNewProvider();
			base.TestFixOldInvalidLayout();

			isLookUp = false;
			SetupNewProvider();
			base.TestFixOldInvalidLayout();
		}

		public override void TestFixOldLayout()
		{
			isLookUp = true;
			SetupNewProvider();
			base.TestFixOldLayout();

			isLookUp = false;
			SetupNewProvider();
			base.TestFixOldLayout();
		}

		public override void TestRequiredColumns()
		{
			isLookUp = true;
			SetupNewProvider();
			base.TestRequiredColumns();

			isLookUp = false;
			SetupNewProvider();
			base.TestRequiredColumns();
		}

		public override void TestUniqueColumns()
		{
			isLookUp = true;
			SetupNewProvider();
			base.TestUniqueColumns();

			isLookUp = false;
			SetupNewProvider();
			base.TestUniqueColumns();
		}

		#endregion

		#region Implementation

		protected override DataGridColumn[] GetColumnsForLayoutFixNoDynamicColumns()
		{
			return new DataGridColumn[]
			{
				TestProvider[WebTracker.Grids.OrgSupplierParts.Description],
				TestProvider[WebTracker.Grids.OrgSupplierParts.ProductNumber],
			};
		}

		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			if (isLookUp)
			{
				AddRequiredColumn(new ZButtonColumn("Product#", OrgSupplierPartSchema.OP_PartNum.Name) { ColumnKey = WebTracker.Grids.OrgSupplierParts.ProductNumber });
			}
			else
			{
				AddRequiredColumn(new ZHyperLinkColumn("Product#", OrgSupplierPartSchema.OP_PartNum.Name)
				{
					ColumnKey = WebTracker.Grids.OrgSupplierParts.ProductNumber,
					DataNavigateUrlFormatString = TrackingConstants.RelativePath.ProductProfileDetailsPage + "?Ref={0}", // Partial URL
					DataNavigateUrlFields = new string[1] { "PK" }
				});
			}
			AddDefaultsColumn(new ZTextEditColumn("Description", OrgSupplierPartSchema.OP_Desc.Name) { ColumnKey = WebTracker.Grids.OrgSupplierParts.Description });
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new OrgSupplierPartColumnProvider(isLookUp);
		}

		protected override void SetUp()
		{
			isLookUp = false;
			base.SetUp();
		}

		bool isLookUp;

		#endregion
	}
}
