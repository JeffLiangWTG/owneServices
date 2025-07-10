using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.DataTransfer.Testing
{
	[TestedType(typeof(FreightErrorType))]
	sealed class TNTErrorTypeTest : NotificationSubscriberTypeTest<FreightErrorType>
	{
		protected override FreightErrorType NewNotificationType(string message)
		{
			return new FreightErrorType(message);
		}

		protected override FreightErrorType NewNotificationType(string name, string message)
		{
			return new FreightErrorType(message);
		}
	}
}
