using System.Data;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.SG.V3.Business
{
	public class V3Message : AutoEDIMessage
	{
		public V3Message(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool ReadOnly
		{
			get { return true; }
		}

		public ZString FormattedMessage
		{
			get
			{
				return EM_MessageText.Replace(Encoding.ASCII.GetString(new byte[1] { 28 }), "'")
					.Replace(Encoding.ASCII.GetString(new byte[1] { 29 }), "+")
					.Replace(Encoding.ASCII.GetString(new byte[1] { 31 }), ":")
					.Replace("'", "'\r\n");
			}
		}

		public ZPropertyInfo FormattedMessageInfo
		{
			get { return GetZPropertyInfo(nameof(FormattedMessage)); }
		}
	}
}
