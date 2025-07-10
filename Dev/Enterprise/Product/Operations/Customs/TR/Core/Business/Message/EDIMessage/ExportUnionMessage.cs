using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Business
{
	public class ExportUnionMessage : TRBaseMessage
	{
		public ExportUnionMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ZString GetApplicationCodeCore() => EDIMessage.ApplicationCodes.TRCustoms;
	}
}
