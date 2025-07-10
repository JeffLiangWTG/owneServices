using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(MonthlyStatementMessageHeader))]
	sealed class MonthlyStatementMessageHeaderTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var message = Factory.New<MQEDIMessage>();
			var statementHeader = Factory.New<CusStatementHeader>();
			return new MonthlyStatementMessageHeader(statementHeader, message);
		}
	}
}
