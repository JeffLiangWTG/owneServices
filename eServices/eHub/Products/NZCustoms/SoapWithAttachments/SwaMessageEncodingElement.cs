using System;
using System.Configuration;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;

namespace CargoWise.eHub.Products.NZCustoms.SoapWithAttachments
{
	public class SwaMessageEncodingElement : BindingElementExtensionElement
	{
		[ConfigurationProperty("innerMessageEncoding", DefaultValue = "textMessageEncoding")]
		public string InnerMessageEncoding
		{
			get { return (string)base["innerMessageEncoding"]; }
			set { base["innerMessageEncoding"] = value; }
		}

		public override Type BindingElementType
		{
			get { return typeof(SwaEncodingBindingElement); }
		}

		public override void ApplyConfiguration(BindingElement bindingElement)
		{
			base.ApplyConfiguration(bindingElement);
			SwaEncodingBindingElement binding = (SwaEncodingBindingElement)bindingElement;
			PropertyInformationCollection propertyInfos = ElementInformation.Properties;
			if (propertyInfos["innerMessageEncoding"].ValueOrigin != PropertyValueOrigin.Default)
			{
				switch (InnerMessageEncoding)
				{
					case "textMessageEncoding":
						binding.InnerBindingElement = new TextMessageEncodingBindingElement();
						break;
					case "binaryMessageEncoding":
						binding.InnerBindingElement = new BinaryMessageEncodingBindingElement();
						break;
					default:
						throw new ConfigurationErrorsException("Inner message encoding can be binary or text, only!");
				}
			}
		}

		protected override BindingElement CreateBindingElement()
		{
			SwaEncodingBindingElement bindingElement = new SwaEncodingBindingElement();
			ApplyConfiguration(bindingElement);
			return bindingElement;
		}
	}
}
