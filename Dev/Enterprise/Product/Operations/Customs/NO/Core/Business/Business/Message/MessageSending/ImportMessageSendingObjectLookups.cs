using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NO.Business
{
	public sealed class ImportMessageSendingObjectLookups : MessageSendingObjectLookups
	{
		public ImportMessageSendingObjectLookups(BusinessObject parent) : base(parent)
		{
		}

		protected override ZString CustomsAuthorizationHeaderType => CusAuthorizationHeaderTypeList.Codes.ImportCustomsDeclaration;
	}
}
