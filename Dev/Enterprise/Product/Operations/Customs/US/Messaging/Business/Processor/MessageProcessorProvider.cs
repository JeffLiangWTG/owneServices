using System;
using System.Xml.Serialization;
using CargoWise.Definitions;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture;

[assembly: Enterprise.Customs.US.Messaging.Business.CBPMessageProcessorProvider(typeof(Enterprise.Customs.US.Messaging.Business.MessageProcessorProvider))]
namespace Enterprise.Customs.US.Messaging.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.US.Messaging.Business.XmlSerializers")]
	[Serializable]
	public sealed class CBPMessageProcessorProviderAttribute : AssemblyMetaDataAttributeWithType
	{
		public CBPMessageProcessorProviderAttribute(Type messageProcessorProviderType)
			: base(messageProcessorProviderType)
		{ }

		public CBPMessageProcessorProviderAttribute()
		{ }
	}

	public interface IMessageProcessorProvider
	{
		IProcessor GetProcessor(string applicationCode, string applicationIdentifier, MessageBlock topLevelBlock);
	}

	class MessageProcessorProvider : IMessageProcessorProvider
	{
		public IProcessor GetProcessor(string applicationCode, string applicationIdentifier, MessageBlock topLevelBlock)
		{
			Type processorType = null;

			if (topLevelBlock != null)
			{
				Dictionary.TryGetValue(applicationCode, applicationIdentifier, topLevelBlock.GetType(), out processorType);
			}

			if (processorType == null)
			{
				Dictionary.TryGetValue(applicationCode, applicationIdentifier, out processorType);
			}

			return processorType != null ? (IProcessor)Activator.CreateInstance(processorType) : null;
		}

		#region Implementation

		ProcessorDictionary LoadDictionary()
		{
			ProcessorDictionary dictionary = new ProcessorDictionary();

			foreach (CBPMessageProcessorProviderAttribute attribute in AssemblyMetaDataReader.GetAttributes<CBPMessageProcessorProviderAttribute>())
			{
				foreach (Type type in attribute.Type.Assembly.GetTypes())
				{
					if (typeof(IProcessor).IsAssignableFrom(type) && !type.IsAbstract)
					{
						object[] applicationIdentifierAttributes = type.GetCustomAttributes(typeof(ApplicationIdentifierAttribute), false);
						if (applicationIdentifierAttributes.Length == 0)
						{
							throw new ApplicationException("Class does not implement ApplicationIdentifier attribute : " + type.Name);
						}

						object[] attributes = type.GetCustomAttributes(typeof(TopLevelAttribute), true);

						foreach (ApplicationIdentifierAttribute applicationIdentifierAttribute in applicationIdentifierAttributes)
						{
							if (attributes.Length > 0)
							{
								TopLevelAttribute topLevelAttribute = (TopLevelAttribute)attributes[0];
								dictionary.Add(applicationIdentifierAttribute.ApplicationCode, applicationIdentifierAttribute.ApplicationIdentifier, topLevelAttribute.MessageBlockType, type);
							}
							else
							{
								dictionary.Add(applicationIdentifierAttribute.ApplicationCode, applicationIdentifierAttribute.ApplicationIdentifier, type);
							}
						}
					}
				}
			}
			return dictionary;
		}

		ProcessorDictionary Dictionary
		{
			get { return dictionaryStatic ?? (dictionaryStatic = LoadDictionary()); }
		}

		[ThreadStatic]
		static ProcessorDictionary dictionaryStatic;
		#endregion
	}
}
