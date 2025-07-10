using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	[TestedType(typeof(StatusErrorsDataViewCollection))]
	sealed class StatusErrorsDataViewCollectionTest : NonPersistentBusinessObjectCollectionTestCase<StatusErrorsDataViewCollection>
	{
		protected override StatusErrorsDataViewCollection GetCollectionToTest() => new StatusErrorsDataViewCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new ErrorsRecord();
	}
}
