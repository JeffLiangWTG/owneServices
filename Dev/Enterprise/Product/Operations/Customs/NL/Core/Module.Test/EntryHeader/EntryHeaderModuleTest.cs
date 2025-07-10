using NUnit.Framework;

namespace Enterprise.Customs.NL.Module.Testing;

[TestedType(typeof(EntryHeaderModule))]
sealed class EntryHeaderModuleTest : Customs.Module.Testing.EntryHeaderModuleTest
{
	public void TestGetNewController()
	{
		AssertType<EntryHeaderController>("Controller must be of type EntryHeaderController", module.GetNewController());
	}

	public void TestHasActions()
	{
		AssertEquals("HasActions must be true", true, module.HasActions);
	}

	public void TestAllowNew()
	{
		AssertEquals("AllowNew must be false", false, module.AllowNew);
	}

	public void TestAllowDelete()
	{
		AssertEquals("AllowDelete must be false", false, module.AllowDelete);
	}

	public void TestFilterBusinessObject()
	{
		AssertType<EntryHeaderFilterBusinessObject>("FilterBusinessObject Type", module.FilterBusinessObject);
	}

	protected override void SetUp()
	{
		base.SetUp();
		module = new EntryHeaderModule();
	}

	protected override void TearDown()
	{
		module.Dispose();
		base.TearDown();
	}

	EntryHeaderModule module;
}
