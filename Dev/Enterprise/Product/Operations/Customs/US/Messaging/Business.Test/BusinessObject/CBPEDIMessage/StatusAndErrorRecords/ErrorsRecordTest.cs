using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	[TestedType(typeof(ErrorsRecord))]
	sealed class ErrorsRecordTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new ErrorsRecord();
	}
}
