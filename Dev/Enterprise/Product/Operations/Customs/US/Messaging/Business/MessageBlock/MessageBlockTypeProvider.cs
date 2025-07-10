using System;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.Definitions;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

[assembly: Enterprise.Customs.US.Messaging.Business.ABIMessageBlockTypeProvider(typeof(Enterprise.Customs.US.Messaging.Business.MessageBlockTypeProvider))]
namespace Enterprise.Customs.US.Messaging.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.US.Messaging.Business.XmlSerializers")]
	[Serializable]
	public sealed class ABIMessageBlockTypeProviderAttribute : AssemblyMetaDataAttributeWithType
	{
		public ABIMessageBlockTypeProviderAttribute(Type messageBlockTypeProviderType)
			: base(messageBlockTypeProviderType)
		{ }

		public ABIMessageBlockTypeProviderAttribute()
		{ }
	}

	public class MessageBlockTypeProvider
	{
		public MessageBlockTypeProvider(Type messageBlockType, string applicationCode, string applicationIdentifier, string mandatoryCharacters, string version)
		{
			Argument.NotNullOrEmpty(applicationCode, "applicationCode");

			ApplicationIdentifierCodeList.EnsureIsValidFormat(applicationIdentifier);
			if (messageBlockType == null)
			{
				throw new ArgumentNullException(nameof(messageBlockType));
			}

			MessageBlockType = messageBlockType;
			ApplicationCode = applicationCode;
			ApplicationIdentifier = applicationIdentifier;
			MandatoryCharacters = Argument.NotNullOrEmpty(mandatoryCharacters, "mandatoryCharacters");
			Version = Argument.NotNull(version, "version");
		}

		public string FullKey
		{
			get { return ApplicationCode + ApplicationIdentifier + MandatoryCharacters + Version; }
		}

		public readonly Type MessageBlockType;
		public readonly string ApplicationCode;
		public readonly string ApplicationIdentifier;
		public readonly string MandatoryCharacters;
		public readonly string Version;
	}
}
