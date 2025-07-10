using System.Reflection;
using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Testing
{
	sealed class INPD00Test : TestCase
	{
		public void TestValueIsInteger()
		{
			var attribute = typeof(INPD00).GetField(nameof(INPD00.Value)).GetCustomAttribute<MessageBlockIntAttribute>();
			AssertNotNull(attribute);
			AssertEquals(8, attribute.Length);
			AssertEquals(14, attribute.Offset);
			AssertEquals(MessageBlockAttribute.FieldStatus.Mandatory, attribute.Status);

			var inpd00 = new INPD00()
			{
				Value = 12
			};
			var text = inpd00.Serialise(false);
			AssertEquals("inpd00.Value", "D00           000000120000000000                                                ", text);
		}

		public void TestWeightIsInteger()
		{
			var attribute = typeof(INPD00).GetField(nameof(INPD00.Weight)).GetCustomAttribute<MessageBlockIntAttribute>();
			AssertNotNull(attribute);
			AssertEquals(10, attribute.Length);
			AssertEquals(22, attribute.Offset);
			AssertEquals(MessageBlockAttribute.FieldStatus.Mandatory, attribute.Status);

			var inpd00 = new INPD00()
			{
				Weight = 114514
			};
			var text = inpd00.Serialise(false);
			AssertEquals("inpd00.Weight", "D00           000000000000114514                                                ", text);
		}
	}
}
