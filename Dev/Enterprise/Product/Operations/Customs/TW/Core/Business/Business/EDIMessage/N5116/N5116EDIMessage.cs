using System.Data;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Customs.TW.Business
{
	public class N5116EDIMessage : TWMessageDocumentSupporter
	{
		public N5116EDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = MessageTypeList.Codes.IRM;
		}

		protected override DocumentSupporter GetDocumentSupporterCore()
		{
			return new N5116MessageDocumentSupporter(this);
		}
	}
}
