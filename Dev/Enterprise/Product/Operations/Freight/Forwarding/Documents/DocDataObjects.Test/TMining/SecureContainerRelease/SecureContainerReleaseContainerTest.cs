using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	[TestedType(typeof(SecureContainerReleaseContainer))]
	sealed class SecureContainerReleaseContainerTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SecureContainerReleaseContainer(null, SecureContainerRelease.FormModeTransfer);
		}
	}
}
