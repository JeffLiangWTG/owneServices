using System;
using CargoWise.Data.Testing;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Commands;

namespace Enterprise.Winzor.Architecture.Test;

class WithSnapshotProtectionAttribute : Attribute, IWrapTestMethod
{
	public TestCommand Wrap(TestCommand command)
	{
		return new EnterpriseTestSetupAttributeTestCommand(new UseSnapshotProtectionAttribute(), command);
	}
}
