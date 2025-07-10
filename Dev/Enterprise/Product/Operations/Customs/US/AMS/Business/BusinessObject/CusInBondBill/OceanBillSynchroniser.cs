using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class OceanBillSynchroniser : BusinessObjectSynchroniser
	{
		public OceanBillSynchroniser(CusInBondBill destination, ForwardingConsol source)
			: base(destination, source)
		{
		}

		public new CusInBondBill Destination
		{
			get { return (CusInBondBill)base.Destination; }
		}

		public new ForwardingConsol Source
		{
			get { return (ForwardingConsol)base.Source; }
		}

		#region Implementation

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			if (Source.MasterBillIssuingPartyDocumentaryAddress != null)
			{
				Synchronisers.Add(new FieldSynchroniser(
					Destination.B0_IssuerCodeInfo,
					() => GetIssuerCode(),
					() => new[]
					{
						Source.MasterBillIssuingPartyDocumentaryAddress.OrganisationPKInfo,
						Source.JK_OA_ShippingLineAddressInfo,
						Source.JK_MasterBillNumInfo
					}));
			}
			Synchronisers.Add(new FieldSynchroniser(Destination.B0_MasterBillNumberInfo, GetBillNumber, GetInfosAffectingBillNumber));
			Synchronisers.Add(new FieldSynchroniser(Destination.B0_BillStatusInfo, () => (ZString)BillOfLadingStatusIndicatorList.Codes.MasterBill, () => new[] { Destination.B0_BHInfo }));
		}

		IZType GetBillNumber()
		{
			var billNumber = Source.JK_MasterBillNum.KeepValidBillNumberCharacters();
			var validSCACs = Source.GetValidSCACIssuerCodes(Source.TransportMode);
			return billNumber.ShouldTrimSCACFromBills(validSCACs) ? billNumber.GetBillNumberTrimSCAC() : billNumber;
		}

		IZType GetIssuerCode()
		{
			var billNumber = Source.JK_MasterBillNum.KeepValidBillNumberCharacters();
			var validSCACs = Source.GetValidSCACIssuerCodes(Source.TransportMode);
			return billNumber.GetSCAC(validSCACs);
		}

		IEnumerable<ZPropertyInfo> GetInfosAffectingBillNumber()
		{
			yield return Source.JK_MasterBillNumInfo;
		}

		#endregion
	}
}
