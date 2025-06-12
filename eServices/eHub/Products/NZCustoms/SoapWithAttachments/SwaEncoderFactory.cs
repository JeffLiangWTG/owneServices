using System;
using System.ServiceModel.Channels;

namespace CargoWise.eHub.Products.NZCustoms.SoapWithAttachments
{
	public class SwaEncoderFactory : MessageEncoderFactory
	{
		readonly SwaEncoder encoder;

		public SwaEncoderFactory(MessageEncoderFactory encoderFactory)
		{
			if (encoderFactory == null)
			{
				throw new ArgumentNullException("encoderFactory",
					"You need to pass an inner encoder to the SwaEncoderFactory to support SOAP-message processing!");
			}

			encoder = new SwaEncoder(encoderFactory.Encoder);
		}

		public override MessageEncoder Encoder
		{
			get { return encoder; }
		}

		public override MessageVersion MessageVersion
		{
			get { return encoder.MessageVersion; }
		}

	}
}
