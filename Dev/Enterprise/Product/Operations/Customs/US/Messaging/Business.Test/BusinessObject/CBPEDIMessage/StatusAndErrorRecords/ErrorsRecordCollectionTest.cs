using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	[TestedType(typeof(ErrorsRecordCollection))]
	sealed class ErrorsRecordCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ErrorsRecordCollection>
	{
		protected override ErrorsRecordCollection GetCollectionToTest() => new ErrorsRecordCollection(new BusinessObjectFactory());

		protected override BusinessObject GetNewElementToAddToTheCollection() => new ErrorsRecord();
	}
}
