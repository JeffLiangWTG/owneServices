using System;
using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing.Testing
{
	internal abstract class ContainerMovementCountryProcessorFactoryTest : TestCaseWithFactory
	{
		abstract public ICMMProcessingAdapter NewAdapter(string country);
		public void TestNewProcessor()
		{
			CombineAssertions(delegate
			{
				AssertTypeForCountry("", typeof(CMMNullCountryProcessor));
				AssertTypeForCountry("AU", typeof(CMMAUCountryProcessor));
				AssertTypeForCountry("NL", typeof(CMMNullCountryProcessor));
			});
		}

		public void TestNewProcessor_Overridden()
		{
			var processorFactoryMock = new Mock<ContainerMovementCountryProcessorFactory.NewProcessorOverride>();
			var processorMock = new Mock<ICMMCountryProcessor>();

			var processorFactory = processorFactoryMock.Object;
			var processor = processorMock.Object;

			var generator = new CMMEmailGenerator();
			var adapter = NewAdapter("AU");

			processorFactoryMock
				.Setup(m => m.Invoke(generator, adapter))
				.Returns(processor);

			using (ContainerMovementCountryProcessorFactory.OverrideNewProcessor(processorFactory))
			{
				AssertSame(processor, ContainerMovementCountryProcessorFactory.NewProcessor(generator, adapter));
			}
		}

		#region Implementation
		void AssertTypeForCountry(string country, Type expectedProcessorType)
		{
			var processsor = ContainerMovementCountryProcessorFactory.NewProcessor(new CMMEmailGenerator(), NewAdapter(country));
			AssertType(country, expectedProcessorType, processsor);
		}
		#endregion
	}
}
