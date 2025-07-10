using NUnit.Framework;

namespace Enterprise.MasterFiles.Business
{
	sealed class ImportRelatedActivityNoDecisionFactoryTest : TestCase
	{
		public void GetIfAvailable()
		{
			var factory = new ImportRelatedActivityNoDecisionFactory();
			AssertNull(factory.GetIfAvailable<IImportRelatedActivityYesNoDecider>());
		}
	}
}
