using System.Data;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Customs.TW.Business
{
	public class N5110EDIMessage : TWMessageDocumentSupporter
	{
		public N5110EDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = MessageTypeList.Codes.TPC;
		}

		protected override DocumentSupporter GetDocumentSupporterCore()
		{
			return new N5110MessageDocumentSupporter(this);
		}
	}
}
