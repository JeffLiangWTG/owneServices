using CargoWise.Types;

namespace Enterprise.Customs.Business.AccumulativeAmendment
{
	[System.Xml.Serialization.XmlInclude(typeof(ZBlob))]
	[System.Xml.Serialization.XmlInclude(typeof(ZBool))]
	[System.Xml.Serialization.XmlInclude(typeof(ZByte))]
	[System.Xml.Serialization.XmlInclude(typeof(ZDate))]
	[System.Xml.Serialization.XmlInclude(typeof(ZDateTime))]
	[System.Xml.Serialization.XmlInclude(typeof(ZDecimal))]
	[System.Xml.Serialization.XmlInclude(typeof(ZGuid))]
	[System.Xml.Serialization.XmlInclude(typeof(ZInt))]
	[System.Xml.Serialization.XmlInclude(typeof(ZLong))]
	[System.Xml.Serialization.XmlInclude(typeof(ZShort))]
	[System.Xml.Serialization.XmlInclude(typeof(ZString))]

	[System.Xml.Serialization.XmlSerializerAssembly("Enterprise.Customs.Business.XmlSerializers")]
	public partial class Entity
	{
	}
}
