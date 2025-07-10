using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NO.Business
{
	public sealed class ExportMessageSendingObjectLookups : MessageSendingObjectLookups
	{
		public ExportMessageSendingObjectLookups(BusinessObject parent) : base(parent)
		{
		}

		protected override ZString CustomsAuthorizationHeaderType => CusAuthorizationHeaderTypeList.Codes.ExportCustomsDeclaration;
	}
}
