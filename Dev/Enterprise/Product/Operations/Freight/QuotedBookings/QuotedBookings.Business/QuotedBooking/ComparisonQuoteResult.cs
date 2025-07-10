using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.QuotedBookings.Business
{
	public class ComparisonQuoteResult : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		protected abstract class Schema
		{
			public const string Selected = "Selected";
		}

		#endregion

		#region Constructors

		public ComparisonQuoteResult(BusinessObjectFactory factory)
			: this(factory, ZString.Empty, ZString.Empty, ZGuid.Empty, ZBool.True)
		{
		}

		public ComparisonQuoteResult(BusinessObjectFactory factory, ZString mode, ZString serviceLevel, ZGuid carrierPK, ZBool showLocalCurrency)
			: base(factory)
		{
			this.Mode = mode;
			this.ServiceLevel = serviceLevel;
			this.CarrierPK = carrierPK;
			this.IsProcessed = false;
			this.ShowLocalCurrency = showLocalCurrency;
		}

		#endregion

		#region Properties

		public OrgHeader Carrier { get { return Factory.Load<OrgHeader>(CarrierPK); } }

		public ZString Mode { get; set; }

		public ZString ServiceLevel { get; set; }

		public ZGuid CarrierPK { get; set; }

		public ZBool Selected
		{
			get
			{
				return fSelected;
			}
			set
			{
				SetNonPersistentPropertyValue(SelectedInfo, ref fSelected, value);
			}
		}

		public ZPropertyInfo SelectedInfo
		{
			get { return GetZPropertyInfo(Schema.Selected); }
		}

		public ZBool IsProcessed { get; set; }

		public ComparisonQuoteChargeCollection Charges
		{
			get
			{
				if (fCharges == null)
				{
					fCharges = new ComparisonQuoteChargeCollection();
				}
				return fCharges;
			}
		}

		public ZBool ShowLocalCurrency { get; set; }

		#endregion

		public void PopulateQuote(QuotedBooking spotQuote)
		{
			spotQuote.Mode = Mode;
			spotQuote.ServiceLevel = ServiceLevel;
			spotQuote.OH_Carrier = CarrierPK;
		}

		#region Implementation

		ZBool fSelected;
		ComparisonQuoteChargeCollection fCharges;

		#endregion
	}
}
