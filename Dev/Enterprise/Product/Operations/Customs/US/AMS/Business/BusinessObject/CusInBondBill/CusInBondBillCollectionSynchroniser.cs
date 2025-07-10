using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInBondBillCollectionSynchroniser : Customs.Business.ManifestBillCollectionSynchroniser<CusInBondHeader, CusInBondBill>
	{
		public CusInBondBillCollectionSynchroniser(CusInBondHeader header)
			: base(header)
		{
		}

		#region Synchronise

		protected override void HookEvents()
		{
			base.HookEvents();
			HookInfoValueChanged(Destination.BH_GBInfo);
		}

		protected override void UnHookEvents()
		{
			Destination.BH_GBInfo.ValueChanged -= Info_ValueChanged;
			base.UnHookEvents();
		}

		protected override bool AreAdditionalKeysMatching(ForwardingShipment shipment, CusInBondBill bill)
		{
			var billNumber = shipment.JS_HouseBill.KeepValidBillNumberCharacters();
			var validSCACs = shipment.GetValidSCACIssuerCodes(shipment.TransportMode);
			return billNumber.GetSCAC(validSCACs).EqualsIgnoringCase(bill.B0_IssuerCode.ToString());
		}

		protected override ZString GetBillNumber(ForwardingShipment shipment)
		{
			var trimmedBillNumber = shipment.JS_HouseBill.KeepValidBillNumberCharacters();
			var validSCACs = shipment.GetValidSCACIssuerCodes(shipment.TransportMode);
			return trimmedBillNumber.ShouldTrimSCACFromBills(validSCACs) ? trimmedBillNumber.GetBillNumberTrimSCAC() : trimmedBillNumber;
		}

		protected override Customs.Business.ConsolDataCalculator ConsolDataCalculator
		{
			get { return consolDataCalculator ?? (consolDataCalculator = new ConsolDataCalculator(Source, Destination)); }
		}
		ConsolDataCalculator consolDataCalculator;

		#endregion

		protected override void DisposeCore()
		{
			base.DisposeCore();
			if (consolDataCalculator != null)
			{
				consolDataCalculator.Dispose();
				consolDataCalculator = null;
			}
		}

		protected override Customs.Business.ManifestBillSynchroniser<CusInBondBill> GetNewManifestBillSynchroniser(CusInBondBill destination, ForwardingShipment source)
		{
			return new CusInBondBillSynchroniser(destination, source);
		}
	}
}
