using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(MessageLogCollection))]
	public class MessageLogCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MessageLogCollection>
	{
		protected override MessageLogCollection GetCollectionToTest()
		{
			return new MessageLogCollection(new BusinessObjectFactory());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new MessageLog(ZString.Empty, ZString.Empty);
		}
	}
}
