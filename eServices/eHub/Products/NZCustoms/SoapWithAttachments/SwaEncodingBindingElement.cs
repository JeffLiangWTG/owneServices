using System;
using System.ServiceModel.Channels;
using System.Xml;

namespace CargoWise.eHub.Products.NZCustoms.SoapWithAttachments
{
	public class SwaEncodingBindingElement : MessageEncodingBindingElement
	{
		readonly XmlDictionaryReaderQuotas readerQuotas;

		public SwaEncodingBindingElement()
			: this(new TextMessageEncodingBindingElement())
		{
			readerQuotas = new XmlDictionaryReaderQuotas();
		}

		public SwaEncodingBindingElement(MessageEncodingBindingElement innerBindingElement)
		{
			innerBindingElement.MessageVersion = MessageVersion.Soap11;
			if (innerBindingElement == null)
			{
				throw new ArgumentNullException("innerBindingElement",
					"The inner binding element cannot be null, please specify a valid binding element!");
			}
			InnerBindingElement = innerBindingElement;
		}

		public MessageEncodingBindingElement InnerBindingElement
		{
			get;
			set;
		}

		public override MessageVersion MessageVersion
		{
			get
			{
				return InnerBindingElement.MessageVersion;
			}
			set
			{
				InnerBindingElement.MessageVersion = value;
			}
		}

		public XmlDictionaryReaderQuotas ReaderQuotas
		{
			get
			{
				return readerQuotas;
			}
		}

		public override BindingElement Clone()
		{
			return new SwaEncodingBindingElement(InnerBindingElement);
		}

		public override MessageEncoderFactory CreateMessageEncoderFactory()
		{
			return new SwaEncoderFactory(InnerBindingElement.CreateMessageEncoderFactory());
		}

		public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
		{
			if (context == null)
				throw new ArgumentException("Context cannot be null, please pass a BindingContext!", "context");

			context.BindingParameters.Add(this);
			return base.BuildChannelFactory<TChannel>(context);
		}

		public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
		{
			if (context == null)
				throw new ArgumentException("Context cannot be null, please pass a BindingContext!", "context");

			return context.CanBuildInnerChannelFactory<TChannel>();
		}

		public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
		{
			if (context == null)
				throw new ArgumentException("Context cannot be null, please pass a BindingContext!", "context");

			context.BindingParameters.Add(this);
			return base.BuildChannelListener<TChannel>(context);
		}

		public override bool CanBuildChannelListener<TChannel>(BindingContext context)
		{
			if (context == null)
				throw new ArgumentException("Context cannot be null, please pass a BindingContext!", "context");

			context.BindingParameters.Add(this);
			return base.CanBuildChannelListener<TChannel>(context);
		}

		public override T GetProperty<T>(BindingContext context)
		{
			if (typeof(T) == typeof(XmlDictionaryReaderQuotas))
			{
				return (T)(object)ReaderQuotas;
			}
			return base.GetProperty<T>(context);
		}
	}
}
