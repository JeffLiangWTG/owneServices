using System.Data;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Customs.TW.Business
{
	public class N5111EDIMessage : TWMessageDocumentSupporter
	{
		public N5111EDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override DocumentSupporter GetDocumentSupporterCore()
		{
			return new N5111MessageDocumentSupporter(this);
		}
	}
}
