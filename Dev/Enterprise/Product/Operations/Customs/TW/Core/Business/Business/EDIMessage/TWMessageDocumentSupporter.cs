using System.Data;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Customs.TW.Business
{
	public class TWMessageDocumentSupporter : TWMessage, IDocumentSupportable
	{
		public TWMessageDocumentSupporter(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		DocumentSupporter IDocumentSupportable.DocumentSupporter => GetDocumentSupporterCore();

		protected virtual DocumentSupporter GetDocumentSupporterCore()
		{
			return new TWEDIMessageDocumentSupporter(this);
		}
	}
}
