using System.Xml.Serialization;
using CargoWise.Types;
using Enterprise.Registry.Business.Customs;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.US.Business.XmlSerializers")]
	public class LowValueEntriesManifestGroupNotification : ManifestGroupNotification
	{
		public LowValueEntriesManifestGroupNotification()
		{
		}

		public LowValueEntriesManifestGroupNotification(ZString sendMode, ZGuid sendGroupPK, ZBool sendErrorOnly)
			: base(sendMode, sendGroupPK, sendErrorOnly)
		{
		}

		public new static ManifestGroupNotification Default
		{
			get { return new ManifestGroupNotification(Core.Constants.EmailTo.NoEmails, ZGuid.Empty, false); }
		}
	}
}
