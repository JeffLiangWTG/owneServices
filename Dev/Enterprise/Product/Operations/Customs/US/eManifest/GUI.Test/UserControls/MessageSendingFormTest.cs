using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Customs.US.eManifest.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.GUI.Testing
{
	[TestedType(typeof(MessageSendingForm))]
	sealed class MessageSendingFormTest : Customs.GUI.Testing.MessageSendingObjectFormTest
	{
		public void TestGridColumns()
		{
			var trip = Factory.New<Trip>();
			trip.BH_GB = Factory.NewWithValidTestData<GlbBranch>().PK;
			trip.BH_OA_Importer = Factory.NewWithValidTestData<OrgAddress>().PK;
			Factory.Save();
			using (var form = new MessageSendingForm(new eManifestMessageSendingObjectParent(trip)))
			{
				form.Show();
				var grid = form.FindSingle<ZGrid>("MessageSendingObjectsGrid");
				Assert(grid.Columns.Contains(AutoeManifestMessageSendingObject.Schema.MessageDescription));
				Assert(grid.Columns.Contains(BaseMessageSendingObject.SchemaShouldSend));
				foreach (ZGridColumnInfo column in grid.ColumnStyles)
				{
					Assert(!column.IsSortable);
				}
			}
		}

		protected override Form GetFormToBashCore()
		{
			var trip = Factory.New<Trip>();
			trip.BH_GB = Factory.NewWithValidTestData<GlbBranch>().PK;
			trip.BH_OA_Importer = Factory.NewWithValidTestData<OrgAddress>().PK;
			return new MessageSendingForm(new eManifestMessageSendingObjectParent(trip));
		}

		protected override bool AllowFormSizeFixed => true;
	}
}
