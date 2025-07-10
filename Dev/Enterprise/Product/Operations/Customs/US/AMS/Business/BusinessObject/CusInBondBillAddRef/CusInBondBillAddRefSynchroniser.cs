using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.AMS.Business
{
	public class CusInbondBillAddRefSynchroniser : BusinessObjectSynchroniser
	{
		public CusInbondBillAddRefSynchroniser(CusInbondBillAddRef destination, ForwardingConsol source)
			: base(destination, source)
		{
		}

		public new CusInbondBillAddRef Destination
		{
			get { return (CusInbondBillAddRef)base.Destination; }
		}

		public new ForwardingConsol Source
		{
			get { return (ForwardingConsol)base.Source; }
		}

		#region Implementation

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.BR_QualifierInfo, () => new ZString(BillReferenceList.Codes.OB), () => new[] { Destination.BR_B0Info }));
			Synchronisers.Add(new FieldSynchroniser(Destination.BR_ReferenceNumInfo, GetOceanBill, GetOceanBillInfo));
		}

		IZType GetOceanBill()
		{
			var billNumber = Source.JK_MasterBillNum;
			if (!billNumber.IsEmpty)
			{
				var carrierSCAC = ConsolDataCalculator.CarrierSCAC;
				if (!carrierSCAC.IsEmpty && billNumber.Left(4).ToUpper() != carrierSCAC)
				{
					billNumber = carrierSCAC.Trim() + billNumber;
				}
			}
			return billNumber.Left(CusInbondBillAddRef.Schema.BR_ReferenceNumMaxLength);
		}

		IEnumerable<ZPropertyInfo> GetOceanBillInfo()
		{
			yield return Source.JK_MasterBillNumInfo;
			yield return Source.MasterBillIssuingPartyDocumentaryAddress.OrganisationPKInfo;
			yield return Source.JK_OA_ShippingLineAddressInfo;
		}

		ConsolDataCalculator ConsolDataCalculator
		{
			get { return consolDataCalculator ?? (consolDataCalculator = new ConsolDataCalculator(Source, Destination.Header)); }
		}
		ConsolDataCalculator consolDataCalculator;

		protected override void DisposeCore()
		{
			base.DisposeCore();
			if (consolDataCalculator != null)
			{
				consolDataCalculator.Dispose();
				consolDataCalculator = null;
			}
		}

		#endregion
	}
}
