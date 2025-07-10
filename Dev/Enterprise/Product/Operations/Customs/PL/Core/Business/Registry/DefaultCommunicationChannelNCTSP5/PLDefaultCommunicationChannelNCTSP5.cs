using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.PL.Business;

[XmlSerializerAssembly("Enterprise.Customs.PL.Business.XmlSerializers")]
public class PLDefaultCommunicationChannelNCTSP5 : AutoPLDefaultCommunicationChannelNCTSP5
{
	public PLDefaultCommunicationChannelNCTSP5()
	{
	}

	public PLDefaultCommunicationChannelNCTSP5(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		: base(fallbackLevel, factory)
	{
	}

	protected override void SetCustomDefaultValuesCore()
	{
		base.SetCustomDefaultValuesCore();

		IsSeapID = PLCustomsDataRegistry.Instance.CommunicationEmailChannelEmailAddress.Value.IsNullOrEmpty();
		IsEmailChannel = !IsSeapID;
	}

	protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
	{
		return new PLDefaultCommunicationChannelNCTSP5(fallbackLevel, factory);
	}

	protected override void ReadElements(XmlReaderWrapper reader)
	{
		IsEmailChannel = reader.ReadElementStringAsZBool(Schema.IsEmailChannel);
		IsSeapID = reader.ReadElementStringAsZBool(Schema.IsSeapID);
	}

	protected override void WriteElements(XmlWriter writer)
	{
		base.WriteElements(writer);
		writer.WriteElementString(Schema.IsEmailChannel, IsEmailChannel.ToString());
		writer.WriteElementString(Schema.IsSeapID, IsSeapID.ToString());
	}
}
