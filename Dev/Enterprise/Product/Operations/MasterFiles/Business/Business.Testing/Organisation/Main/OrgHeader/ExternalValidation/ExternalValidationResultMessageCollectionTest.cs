using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ExternalValidationResultMessageCollection))]
	sealed class ExternalValidationResultMessageCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ExternalValidationResultMessageCollection>
	{
		protected override ExternalValidationResultMessageCollection GetCollectionToTest()
		{
			return new ExternalValidationResultMessageCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ExternalValidationResultMessage("", "");
		}
	}
}
