using System;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using ServiceManager.Integration.ServiceTasks.CW;
using static NUnit.Framework.Assertion;

namespace Enterprise.Customs.ServiceTasks.Testing
{
	public abstract class CommonServiceTestHelper
	{
		public static void AssertSingleHostedServiceAttribute<TService>(
			string expectedCode,
			string expectedDescription,
			string expectedCategory,
			Type expectedServiceType,
			string expectedMinimumPeriod,
			string expectedRequiresCompanyInCountry,
			bool expectedCanRunInAnyBranch)
			where TService : ServiceProviderImpl
		{
			var serviceProviderType = typeof(TService);
			var assembly = serviceProviderType.Assembly;
			var attributes = Array.ConvertAll(assembly.GetCustomAttributes(typeof(HostedServiceAttribute), false), attribute => (HostedServiceAttribute)attribute);
			var hostedServiceAttribute = Array.FindAll(attributes, attribute => attribute.TypeName.Equals(serviceProviderType.FullName, StringComparison.Ordinal)).Single();
			NUnit.Framework.AssertionWithHtml.CombineAssertions(() =>
			{
				AssertEquals("Code", expectedCode, hostedServiceAttribute.Code);
				AssertEquals("Description", expectedDescription, hostedServiceAttribute.Description);
				AssertEquals("Category", expectedCategory, hostedServiceAttribute.Category);
				AssertEquals("Type", expectedServiceType, hostedServiceAttribute.Type);
				AssertEquals("MinimumPeriod", expectedMinimumPeriod, hostedServiceAttribute.MinimumPeriod);
				AssertEquals("RequiresCompanyInCountry", expectedRequiresCompanyInCountry, hostedServiceAttribute.RequiresCompanyInCountry);
				AssertEquals("CanRunInAnyBranch", expectedCanRunInAnyBranch, hostedServiceAttribute.CanRunInAnyBranch);
			});
		}

		public static void AssertInterchangePrepared(BusinessObjectFactory factory, OutgoingMessageProcessor processor, EDIMessage message)
		{
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			factory.Save();

			processor.ProcessMessage(CancellationToken.None);
			factory.Save();
			message.Reload();

			NUnit.Framework.AssertionWithHtml.CombineAssertions(() =>
			{
				AssertNotNull("Interchange created", message.Interchange);
				AssertEquals(ZArchitecture.Schema.EDIInterchangeSchema.EI_From.Name, EDIMessageStatusList.Codes.Sent, message.EM_Status);
			});
		}
	}
}
