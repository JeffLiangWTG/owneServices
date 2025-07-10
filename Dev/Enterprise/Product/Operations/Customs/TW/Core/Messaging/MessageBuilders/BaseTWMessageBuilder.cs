using System;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Messaging.MessageBuilders
{
	public abstract class BaseTWMessageBuilder<DataSource, SerializableClassType> : ITWMessageBuilder
	{
		public abstract SerializableClassType PopulateDeclaration(DataSource input, string functionCode = null);

		public ZString SerializeToMessageString(object input, string functionCode = null)
		{
			var messageObject = PopulateDeclaration((DataSource)input, functionCode);
			return messageObject != null ? XmlHelper.Serializer(typeof(SerializableClassType), messageObject, true) : string.Empty;
		}

		protected void PopulateValueIfNodeValueIsNotEmpty(IZType nodeValue, Action populateValueAction)
		{
			if (!(nodeValue?.IsEmpty ?? true))
			{
				populateValueAction();
			}
		}

		protected void PopulateValueIfNodeValueIsValid(IZType nodeValue, Action populateValueAction)
		{
			if (nodeValue.IsValid)
			{
				populateValueAction();
			}
		}

		protected void PopulateValueIfNodeValueHasValue(ZDecimal? nodeValue, Action populateValueAction)
		{
			if (nodeValue.HasValue)
			{
				populateValueAction();
			}
		}
	}
}
