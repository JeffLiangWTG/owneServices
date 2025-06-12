using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Moq;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.WcfService.Tests
{
	[TestFixture]
	public class UnhandledExceptionBehaviorAttributeTest
	{
		[Test]
		public void TestApplyDispatchBehavior()
		{
			var channelListener1 = new Mock<IChannelListener>();
			var channelDispatcher1 = new ChannelDispatcher(channelListener1.Object);
			var channelListener2 = new Mock<IChannelListener>();
			var channelDispatcher2 = new ChannelDispatcher(channelListener2.Object);
			var attribute = new Mock<UnhandledExceptionBehaviorAttribute>(typeof (GlobalErrorHandler));
			attribute
				.Setup(_ => _.GetChannelDispatchers(It.IsAny<ServiceHostBase>()))
				.Returns(new[] {channelDispatcher1, channelDispatcher2});
			var serviceDescription = new Mock<ServiceDescription>();
			var serviceHostBase = new Mock<ServiceHostBase>();
			attribute.Object.ApplyDispatchBehavior(serviceDescription.Object, serviceHostBase.Object);
			
			Assert.That(channelDispatcher1.ErrorHandlers.OfType<GlobalErrorHandler>().Count(), Is.EqualTo(1));
			Assert.That(channelDispatcher2.ErrorHandlers.OfType<GlobalErrorHandler>().Count(), Is.EqualTo(1));
		}

		[Test]
		public void TestAttributeIsApplied()
		{
			var attribute = typeof (BillingService).CustomAttributes.Single(ca => ca.AttributeType == typeof (UnhandledExceptionBehaviorAttribute));
			Assert.That(attribute.ConstructorArguments[0].Value, Is.EqualTo(typeof (GlobalErrorHandler)));
		}
	}
}