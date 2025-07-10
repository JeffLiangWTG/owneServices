using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Tracking.Business.Quotations
{
	public class TrackingQuote : Quote, IContainerListProvider
	{
		public TrackingQuote(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			this.ShowApprovalDialog += (sender, e) => e.Cancel = true;
		}

		[SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		public static class Constants
		{
			public const string TransportMode = "TransportMode";
			public const string Origin = "Origin";
			public const string Destination = "Destination";
			public const string Volume = "Volume";
			public const string VolumeUnits = "VolumeUnits";
			public const string Weight = "Weight";
			public const string WeightUnits = "WeightUnits";
			public const string CompanyName = "CompanyName";
			public const string QuoteAmount = "QuoteAmount";

			public const string TransportModes = "TransportModes";
			public const string Origins = "Origins";
			public const string Destinations = "Destinations";
		}

		public override bool IsTrackingQuote
		{
			get { return true; }
		}

		#region Transport Mode

		[MaxLength(AutoRateOneOffShipment.Schema.TT_TransportModeMaxLength)]
		public ZString TransportMode
		{
			get { return CurrentOneOffQuote != null ? CurrentOneOffQuote.Mode : ZString.Empty; }
		}

		public ZPropertyInfo TransportModeInfo
		{
			get { return GetZPropertyInfo(Constants.TransportMode); }
		}

		#endregion

		#region Origin

		[MaxLength(AutoRateOneOffShipment.Schema.TT_RL_NKReceivalLocationMaxLength)]
		public ZString Origin
		{
			get { return CurrentOneOffQuote != null ? CurrentOneOffQuote.TT_RL_NKReceivalLocation : ZString.Empty; }
		}

		public ZPropertyInfo OriginInfo
		{
			get { return GetZPropertyInfo(Constants.Origin); }
		}

		#endregion

		#region Destination

		[MaxLength(AutoRateOneOffShipment.Schema.TT_RL_NKDeliveryLocationMaxLength)]
		public ZString Destination
		{
			get { return CurrentOneOffQuote != null ? CurrentOneOffQuote.TT_RL_NKDeliveryLocation : ZString.Empty; }
		}

		public ZPropertyInfo DestinationInfo
		{
			get { return GetZPropertyInfo(Constants.Destination); }
		}

		#endregion

		#region Volume

		public ZDecimal Volume
		{
			get { return CurrentOneOffQuote != null ? CurrentOneOffQuote.TT_ActualVolume : ZDecimal.Zero; }
		}

		public ZPropertyInfo VolumeInfo
		{
			get { return GetZPropertyInfo(Constants.Volume); }
		}

		[MaxLength(AutoRateOneOffShipment.Schema.TT_UnitOfVolumeMaxLength)]
		public ZString VolumeUnits
		{
			get { return CurrentOneOffQuote != null ? CurrentOneOffQuote.TT_UnitOfVolume : ZString.Empty; }
		}

		public ZPropertyInfo VolumeUnitsInfo
		{
			get { return GetZPropertyInfo(Constants.VolumeUnits); }
		}

		#endregion

		#region Weight

		public ZDecimal Weight
		{
			get { return CurrentOneOffQuote != null ? CurrentOneOffQuote.TT_ActualWeight : ZDecimal.Zero; }
		}

		[MaxLength(AutoRateOneOffShipment.Schema.TT_UnitOfWeightMaxLength)]
		public ZString WeightUnits
		{
			get { return CurrentOneOffQuote != null ? CurrentOneOffQuote.TT_UnitOfWeight : ZString.Empty; }
		}

		public ZPropertyInfo WeightInfo
		{
			get { return GetZPropertyInfo(Constants.Weight); }
		}

		public ZPropertyInfo WeightUnitsInfo
		{
			get { return GetZPropertyInfo(Constants.WeightUnits); }
		}

		#endregion

		#region Company Name

		[MaxLength(AutoGlbCompany.Schema.GC_NameMaxLength)]
		public ZString CompanyName
		{
			get { return Company != null ? Company.GC_Name : ZString.Empty; }
		}

		public ZPropertyInfo CompanyNameInfo
		{
			get { return GetZPropertyInfo(Constants.CompanyName); }
		}

		#endregion

		#region Lookups

		public RefUNLOCOCollection Origins
		{
			get { return new RefUNLOCOCollection(Factory); }
		}

		public RefUNLOCOCollection Destinations
		{
			get { return new RefUNLOCOCollection(Factory); }
		}

		public CodeDescriptionPairList TransportModes
		{
			get { return CurrentOneOffQuote != null ? CurrentOneOffQuote.Lookups.WebTrackerModes : new CodeDescriptionPairList(); }
		}

		#endregion

		#region IContainerListProvider

		public RefContainerCollection Container_List
		{
			get { return new ContainerHelper(Factory).List(TransportMode); }
		}

		#endregion

		protected override bool EnableLightValidationIfAvailable
		{
			get { return false; }
		}
	}
}
