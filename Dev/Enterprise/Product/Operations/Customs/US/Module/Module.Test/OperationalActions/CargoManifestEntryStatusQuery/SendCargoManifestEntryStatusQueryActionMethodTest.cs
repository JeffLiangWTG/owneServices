using NUnit.Framework;

namespace Enterprise.Customs.US.Module.OperationalActions.Testing
{
	[TestedType(typeof(SendCargoManifestEntryStatusQueryActionMethod))]
	sealed class SendCargoManifestEntryStatusQueryActionMethodTest : Services.OperationalActions.Support.Testing.OperationalActionMethodTest<SendCargoManifestEntryStatusQueryActionMethod>
	{
		public void TestProperties()
		{
			var method = new SendCargoManifestEntryStatusQueryActionMethod();
			AssertEquals("Send Cargo/Manifest/Entry Status Query operational action", method.Name);
			AssertEquals("Send Cargo/Manifest/Entry Status Query(US)", method.Description);
			AssertEquals(true, method.HasControl);
			AssertEquals(false, method.HasSettings);
			using (var control = method.NewGuiControl())
			{
				AssertEquals(typeof(SendCargoManifestEntryStatusQueryActionControl), control.GetType());
			}
		}

		protected override SendCargoManifestEntryStatusQueryActionMethod NewMethod() => new SendCargoManifestEntryStatusQueryActionMethod();
	}
}
