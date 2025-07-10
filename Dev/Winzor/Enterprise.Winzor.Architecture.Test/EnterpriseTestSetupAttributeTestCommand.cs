using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace Enterprise.Winzor.Architecture.Test;

class EnterpriseTestSetupAttributeTestCommand : DelegatingTestCommand
{
	public EnterpriseTestSetupAttributeTestCommand(TestSetupAttribute testSetupAttribute, TestCommand innerCommand)
		: base(innerCommand)
	{
		this.testSetupAttribute = testSetupAttribute;
	}

	[SuppressMessage("Usage", "VSTHRD002:Avoid problematic synchronous waits", Justification = "Implementing non-async interface")]
	public override NUnit.Framework.Internal.TestResult Execute(TestExecutionContext context)
	{
		EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() => testSetupAttribute.SetUp(null)).GetAwaiter().GetResult();
		try
		{
			return innerCommand.Execute(context);
		}
		finally
		{
			EnterpriseTestSetup.WinzorDispatcher.InvokeAsync(() => testSetupAttribute.TearDown(null)).GetAwaiter().GetResult();
		}
	}

	readonly TestSetupAttribute testSetupAttribute;
}
