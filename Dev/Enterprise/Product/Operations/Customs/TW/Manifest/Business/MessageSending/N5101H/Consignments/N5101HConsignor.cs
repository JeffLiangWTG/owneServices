using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class N5101HConsignor : IPartyDetails
	{
		public N5101HConsignor(AsycudaBill bill)
		{
			this.bill = bill;
		}

		readonly AsycudaBill bill;

		public ZString ID => SharedHelper.GetIDStartWithNO(bill.ABL_ShipperRegNo, TypeCode);

		public ZString Name => bill.ABL_ShipperName;

		public ZString ChineseName => bill.ABL_ShipperLocalName;

		public ZString TypeCode => N5101HHelpers.RegNoTypeToTypeCode(bill.ABL_ShipperRegNoType);

		public ZString CustomsControlID => ZString.Empty;

		public ZString PaymentOnAccountBusinessID => ZString.Empty;

		public ZString RoleCode => ZString.Empty;

		public ZString SubBoxID => ZString.Empty;

		public IAddress Address => new N5101HConsignorAddress(bill);

		public ILPCOAuthorizedParty LPCOAuthorizedParty => null;

		public IEnumerable<ICommunication> Communications => null;

		public ZString ContactName => ZString.Empty;

		public ZString MainManufacturer => ZString.Empty;

		public ZString UndertakeCode => ZString.Empty;

		public ZString OwnerName => null;

		public IEnumerable<IAdditionalInformation> AdditionalInformations => null;
	}
}
