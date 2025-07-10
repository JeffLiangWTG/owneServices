using System.Data;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Customs.TW.Business
{
	public class N5204EDIMessage : TWMessageDocumentSupporter
	{
		public N5204EDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = MessageTypeList.Codes.ERM;
		}

		protected override DocumentSupporter GetDocumentSupporterCore()
		{
			return new N5204MessageDocumentSupporter(this);
		}
	}
}
