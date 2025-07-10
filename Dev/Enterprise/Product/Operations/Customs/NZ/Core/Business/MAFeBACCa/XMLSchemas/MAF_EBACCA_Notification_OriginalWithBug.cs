using System.Xml.Serialization;

namespace Enterprise.Customs.NZ.Business.MAFeBACCa.XMLSchemas
{
	/// <summary>
	/// This class has been added to deal with a bug that MAF had in their original implementation of the eBACCa response. They 
	/// didn't quite manage to follow their own schema, so we had to be able to process EBACCANotification records with an
	/// XML Root of EBACCANotificationType instead of EBACCANotification.
	/// </summary>
	[XmlRoot("EBACCANotificationType", Namespace = "http://www.maf.govt.nz/EBACCA/Messaging/Notification/2008/03/", IsNullable = false)]
	[XmlSerializerAssembly("Enterprise.Customs.NZ.Business.XmlSerializers")]
	public class EBACCANotificationTypeType : EBACCANotificationType
	{
	}
}
