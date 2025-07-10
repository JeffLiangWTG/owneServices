using System;
using System.Xml.Serialization;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Business.MessageProcessors
{
	[Serializable]
	[XmlSerializerAssembly("Enterprise.Customs.TW.Business.XmlSerializers")]
	public class TWMessageInfo
	{
		public ZString MailBox { get; set; }
		public ZString MessageType { get; set; }
		public ZString EntryNumber { get; set; }
		public ZString EntryNumberType { get; set; }
		public ZString StaffCode { get; set; }
		public ZString CompanyID { get; set; }
		public ZString PasswordType { get; set; }
	}
}
