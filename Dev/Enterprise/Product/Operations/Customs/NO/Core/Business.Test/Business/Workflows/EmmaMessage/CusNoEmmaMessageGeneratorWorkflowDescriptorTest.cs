using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CusNOEmmaMessageGeneratorWorkflowDescriptor))]
sealed class CusNoEmmaMessageGeneratorWorkflowDescriptorTest : WorkflowDescriptorTestCase<CusNOEmmaMessageGeneratorWorkflowDescriptor>
{
	public override void TestID()
	{
		AssertEquals(WorkflowDescriptors.CusNOEmmaMessageGeneratorWorkflowDescriptorCode, WorkflowDescriptor.Code);
	}

	public override void TestDescription()
	{
		AssertEquals("EMMA Message Generation", WorkflowDescriptor.Description);
	}

	public override void TestSubTypes()
	{
		AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
	}

	public override void TestRequiresPorts() => CombineAssertions(() =>
	{
		AssertEquals("RequiresPort1", expected: false, WorkflowDescriptor.RequiresPort1);
		AssertEquals("RequiresPort2", expected: false, WorkflowDescriptor.RequiresPort2);
	});

	public override void TestRequiresClient()
	{
		AssertEquals(expected: false, WorkflowDescriptor.RequiresClient);
	}

	public override void TestRequiresBranch()
	{
		AssertEquals(expected: true, WorkflowDescriptor.RequiresBranch);
	}

	public override void TestRequiresDepartment()
	{
		AssertEquals(expected: false, WorkflowDescriptor.RequiresDepartment);
	}

	public override void TestSupportsEventTracking()
	{
		AssertEquals("Should be true because we want to send universal events from line level triggers.", true, WorkflowDescriptor.SupportsEventTracking);
	}

	public override void TestWorkflowProviderType() => AssertEquals(typeof(CusEntryHeader), WorkflowDescriptor.WorkflowProviderType);

	public override void TestSupportsWorkflowTemplates()
	{
		AssertEquals(false, WorkflowDescriptor.SupportsWorkflowTemplates);
	}

	protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
	{
		var declaration = Factory.New<JobDeclaration>();
		_ = declaration.CustomsEntryHeaders.AddNew();
		return [declaration];
	}

	protected override bool ExpectingTasksToBeCompanySpecific => true;
}
