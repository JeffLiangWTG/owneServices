using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DailyStatementMessageHeader))]
	sealed class DailyStatementMessageHeaderTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var message = Factory.New<MQEDIMessage>();
			var statementHeader = Factory.New<CusStatementHeader>();
			return new DailyStatementMessageHeader(statementHeader, message);
		}
	}
}
