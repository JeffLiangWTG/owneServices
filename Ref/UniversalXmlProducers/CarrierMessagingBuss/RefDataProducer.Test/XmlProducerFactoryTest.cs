using System;
using CargoWise.RefDbRepo.CarrierMessagingBuss.RefDataProducer;
using CargoWise.RefDbRepo.CarrierMessagingBuss.Shared.Test;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CarrierMessagingBuss.RefAccessorialDataProducer.Test
{
	public class XmlProducerFactoryTest
	{
		[TestCase("RefAccessorial")]
		public void CreateProducer_ShouldReturnValidProducer_WhenValidInputProvided(string type)
		{
			// Act  
			var producer = XmlProducerFactory.CreateProducer(type, MockHelper.GetTokenProvider("something"));

			// Assert  
			Assert.NotNull(producer);
			Assert.IsInstanceOf<IXmlProducer>(producer);
		}

		[TestCase("Unknown")]
		[TestCase("")]
		public void CreateProducer_ShouldThrowException_WhenInvalidInputProvided(string type)
		{
			// Act & Assert  
			var exception = Assert.Throws<ArgumentException>(() => XmlProducerFactory.CreateProducer(type, null!));
			Assert.That(exception.Message, Is.EqualTo($"Invalid type: {type} (Parameter 'type')"));
		}
	}
}
