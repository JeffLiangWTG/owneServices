using System;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CFS.Business
{
	public abstract class CFSShipmentRNSStatusProvider : Enterprise.Integration.Customs.CA.ICFSShipmentRNSStatusProvider
	{
		protected CFSShipmentRNSStatusProvider(CFSShipment shipment)
		{
			this.shipment = shipment;
		}

		#region Implementation

		protected readonly CFSShipment shipment;

#if DEBUG
		[ThreadStatic]
		internal static CFSShipmentRNSStatusProvider DummyForTest;
#endif

		#endregion

		#region Type Deciding / Factory

		public static CFSShipmentRNSStatusProvider New(CFSShipment shipment)
		{
#if DEBUG
			if (DummyForTest != null)
			{
				CFSShipmentRNSStatusProvider result = DummyForTest;
				DummyForTest = null;    // So we don't get other tests hitting it. 
				return result;
			}
#endif

			return (CFSShipmentRNSStatusProvider)Activator.CreateInstance(GetTypeForNew(), new object[] { shipment });
		}

		public static Type GetTypeForNew()
		{
			Type result = typeof(BlankCFSShipmentRNSStatusProvider);

			switch (GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				case Constants.CountryCodes.Canada:
					result = ObjectFactory.GetType<Enterprise.Integration.Customs.CA.ICFSShipmentRNSStatusProvider>();
					break;
			}

			return result;
		}

		#endregion

		#region Properties

		#region TransactionNumber

		public ZString TransactionNumber
		{
			get { return TransactionNumberCore(); }
		}

		protected abstract ZString TransactionNumberCore();

		#endregion

		#region ReleaseStatus

		public ZString ReleaseStatus
		{
			get { return ReleaseStatusCore(); }
		}

		protected abstract ZString ReleaseStatusCore();

		#endregion

		#region ReleaseStatusCode

		public ZString ReleaseStatusCode
		{
			get { return ReleaseStatusCodeCore(); }
		}

		protected abstract ZString ReleaseStatusCodeCore();

		#endregion

		#region ReleaseDate

		public ZDateTime ReleaseDate
		{
			get { return ReleaseDateCore(); }
		}

		protected abstract ZDateTime ReleaseDateCore();

		#endregion

		#region ArrivalCertificationStatus

		public ZString ArrivalCertificationStatus
		{
			get { return ArrivalCertificationStatusCore(); }
		}

		protected abstract ZString ArrivalCertificationStatusCore();

		#endregion

		#region ArrivalCertificationStatusCode

		public ZString ArrivalCertificationStatusCode
		{
			get { return ArrivalCertificationStatusCodeCore(); }
		}

		protected abstract ZString ArrivalCertificationStatusCodeCore();

		#endregion

		#region ArrivalCertificationDate

		public ZDateTime ArrivalCertificationDate
		{
			get { return ArrivalCertificationDateCore(); }
		}

		protected abstract ZDateTime ArrivalCertificationDateCore();

		#endregion

		#endregion

		public class BlankCFSShipmentRNSStatusProvider : CFSShipmentRNSStatusProvider
		{
			public BlankCFSShipmentRNSStatusProvider(CFSShipment shipment)
				: base(shipment)
			{
			}

			protected override ZString TransactionNumberCore()
			{
				return ZString.Empty;
			}

			protected override ZString ReleaseStatusCore()
			{
				return ZString.Empty;
			}

			protected override ZString ReleaseStatusCodeCore()
			{
				return ZString.Empty;
			}

			protected override ZDateTime ReleaseDateCore()
			{
				return ZDateTime.Empty;
			}

			protected override ZString ArrivalCertificationStatusCore()
			{
				return ZString.Empty;
			}

			protected override ZString ArrivalCertificationStatusCodeCore()
			{
				return ZString.Empty;
			}

			protected override ZDateTime ArrivalCertificationDateCore()
			{
				return ZDateTime.Empty;
			}
		}
	}
}
