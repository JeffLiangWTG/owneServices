using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.OperationalActions.Testing
{
	[TestedType(typeof(SendCargoManifestEntryStatusQueryActionMethodApplicator))]
	sealed class SendCargoManifestEntryStatusQueryActionControlTest : Services.OperationalActions.Support.Testing.OperationalActionMethodApplicatorTest
	{
		public void TestActionChange()
		{
			Applicator.Action = CargoManifestStatusQueryActionList.Codes.MAWB;
			using (var form = new TestForm(Applicator))
			{
				form.Show();
				var userControl = form.UserControl;
				userControl.BindingSource.DataSourceType = typeof(SendCargoManifestEntryStatusQueryActionMethodApplicator);
				userControl.SetDataBinding(form.BusinessEntity, "");
				AssertEquals("RequestForReleatedBOL.ReadOnly", false, userControl.requestForReleatedBOL.ReadOnly);
				AssertEquals("UpdateEntryWithResults.ReadOnly", false, userControl.updateEntryWithResults.ReadOnly);

				Applicator.Action = CargoManifestStatusQueryActionList.Codes.InBond;
				AssertEquals("RequestForReleatedBOL.ReadOnly", true, userControl.requestForReleatedBOL.ReadOnly);
				AssertEquals("UpdateEntryWithResults.ReadOnly", true, userControl.updateEntryWithResults.ReadOnly);
			}
		}

		new SendCargoManifestEntryStatusQueryActionMethodApplicator Applicator => (SendCargoManifestEntryStatusQueryActionMethodApplicator)base.Applicator;

		sealed class TestForm : ZForm
		{
			public TestForm(SendCargoManifestEntryStatusQueryActionMethodApplicator pivot) : base(pivot)
			{
			}

			internal SendCargoManifestEntryStatusQueryActionControl UserControl;

			protected override void InitializeComponent()
			{
				UserControl = new SendCargoManifestEntryStatusQueryActionControl();
				Controls.Add(UserControl);
			}
		}
	}
}
