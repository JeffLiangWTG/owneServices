using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	public class ImportRelatedActivityPromptUserDeciderFactoryTest : TestCase
	{
		public void TestGetIfAvailable()
		{
			using (var form = new ZForm())
			{
				var factory = new ImportRelatedActivityPromptUserDeciderFactory(form, "My Reason");
				var decider = factory.GetIfAvailable<IDummyImportRelatedActivityDecider>();
				AssertType(typeof(DummyImportRelatedActivityPromptUserDecider), decider);
			}
		}

		[ImportRelatedActivityPromptUserDecider("Enterprise.MasterFiles.GUI.Testing.ImportRelatedActivityPromptUserDeciderFactoryTest+DummyImportRelatedActivityPromptUserDecider, Enterprise.MasterFiles.GUI.Test")]
		interface IDummyImportRelatedActivityDecider : IImportRelatedActivityDecider
		{
		}

		public class DummyImportRelatedActivityPromptUserDecider : ImportRelatedActivityPromptUserDecider, IDummyImportRelatedActivityDecider
		{
			public DummyImportRelatedActivityPromptUserDecider(KForm parentForm)
				: base(parentForm)
			{
			}
		}
	}
}
