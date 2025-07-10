using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class TaxDateDefaultingOption : ChargeGroupSetting
	{
		#region

		public static class Code
		{
			public const string Today = "TDY";
			public const string InvoiceDate = "INV";
			public const string ArrivalDate = "ARV";
			public const string DepartureDate = "DEP";
			public const string EstimatedArrivalDate = "EAD";
			public const string EstimatedDepartureDate = "EDD";
			public const string CustomClearanceDate = "CUS";
			public const string VesselArrivalDate = "VAD";
			public const string VesselDepartureDate = "VDD";
			public const string PickupDate = "PIC";
			public const string DeliveryDate = "DEL";
		}

		public static class Description
		{
			public static MultilingualString Today { get { return ResString.GetMultilingualString("39B23EC7-E088-490E-9604-7314E7660D70", "Today's Date"); } }
			public static MultilingualString InvoiceDate { get { return ResString.GetMultilingualString("B0C4DF33-0BFC-4E81-B1B6-4FF605BC0FAC", "Invoice Date"); } }
			public static MultilingualString ArrivalDate { get { return ResString.GetMultilingualString("192283B2-2C85-4E46-AC21-5D3D04A2C192", "Actual/Estimated Arrival Date"); } }
			public static MultilingualString DepartureDate { get { return ResString.GetMultilingualString("D30299F0-2E4B-4747-B2F8-D349CB222423", "Actual/Estimated Departure Date"); } }
			public static MultilingualString EstimatedArrivalDate { get { return ResString.GetMultilingualString("DED2824F-E44D-4FD4-806F-949103680083", "Estimated Arrival Date"); } }
			public static MultilingualString EstimatedDepartureDate { get { return ResString.GetMultilingualString("6AA9D2EE-F3A1-4532-98BE-698F035F8CAC", "Estimated Departure Date"); } }
			public static MultilingualString CustomClearanceDate { get { return ResString.GetMultilingualString("C60DD6E7-7680-43D1-B945-17E543A15C53", "Custom Clearance Date"); } }
			public static MultilingualString VesselArrivalDate { get { return ResString.GetMultilingualString("6453F7A1-1F3B-448C-A83E-4B5DB7748A02", "Vessel Arrival Date"); } }
			public static MultilingualString VesselDepartureDate { get { return ResString.GetMultilingualString("CB514948-9B35-4B0C-A4CD-85899E194BE2", "Vessel Departure Date"); } }
			public static MultilingualString ActualEstimatePickupDate { get { return ResString.GetMultilingualString("845B53CA-8139-485D-8D8B-22FFED5BE441", "Actual/Estimated Pickup Date"); } }
			public static MultilingualString EstimatePickupDate { get { return ResString.GetMultilingualString("4EC342D4-9EE9-4AAD-8766-F316016ABC73", "Estimated Pickup Date"); } }
			public static MultilingualString ActualEstimateDeliveryDate { get { return ResString.GetMultilingualString("B3813141-BA50-43D1-AEE1-F58D27122EB3", "Actual/Estimated Delivery Date"); } }
			public static MultilingualString EstimateDeliveryDate { get { return ResString.GetMultilingualString("21DD0DB1-3D43-4286-A1F0-5B97A8A52C24", "Estimated Delivery Date"); } }
		}

		#endregion

		#region Schema

		public new abstract class Schema : ChargeGroupSetting.Schema
		{
			public const string TaxDateOption = "TaxDateOption";
			public const string Ledger = "Ledger";
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TaxDateDefaultingOption();
		}

		public override IJobConfigurationSelector[] ParentCollectionForValidation
		{
			get
			{
				var parentCollection = GetParentCollection(this, typeof(TaxDateDefaultingOptionCollection));
				return parentCollection != null ? parentCollection.Cast<TaxDateDefaultingOption>().ToArray() : System.Array.Empty<TaxDateDefaultingOption>();
			}
		}

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateTaxDateOption();
		}

		public new TaxDateDefaultingOptionValidation Validation
		{
			get { return (TaxDateDefaultingOptionValidation)base.Validation; }
		}

		protected override JobConfigurationSelectorValidation GetNewValidation()
		{
			return new TaxDateDefaultingOptionValidation(this);
		}

		#endregion

		#region Properties

		#region Ledger

		[MaxLength(3)]
		[List("LedgerList")]
		public ZString Ledger
		{
			get { return LedgerInfo.ReadOnly ? ZString.Empty : fLedger; }
			set
			{
				CheckMaximumLength(LedgerInfo, value);
				SetNonPersistentPropertyValue(LedgerInfo, ref fLedger, value);
				if (!IsValidationSuspended)
				{
					ValidateLedger();
				}
			}
		}

		public ZPropertyInfo LedgerInfo
		{
			get { return GetZPropertyInfo(Schema.Ledger); }
		}

		public CodeDescriptionPairList LedgerList
		{
			get { return TaxDateDefaultingOptionLookups.LedgerList; }
		}

		void ValidateLedger()
		{
			LedgerInfo.ClearAllNotifications();
			Validation.ValidateLedger();
		}

		ZString fLedger;

		#endregion

		#region TaxDateOption

		[MaxLength(3)]
		[List("TaxDateOptionList")]
		public ZString TaxDateOption
		{
			get { return TaxDateOptionInfo.ReadOnly ? ZString.Empty : fTaxDateOption; }
			set
			{
				CheckMaximumLength(TaxDateOptionInfo, value);
				SetNonPersistentPropertyValue(TaxDateOptionInfo, ref fTaxDateOption, value);
				if (!IsValidationSuspended)
				{
					ValidateTaxDateOption();
				}
			}
		}

		public ZPropertyInfo TaxDateOptionInfo
		{
			get { return GetZPropertyInfo(Schema.TaxDateOption); }
		}

		public CodeDescriptionPairList TaxDateOptionList
		{
			get { return TaxDateDefaultingOptionLookups.TaxDateOptionList; }
		}

		void ValidateTaxDateOption()
		{
			TaxDateOptionInfo.ClearAllNotifications();
			Validation.ValidateTaxDateOption();
		}

		ZString fTaxDateOption;

		#endregion

		#endregion

		#region TaxDateDefaultingOptionLookups

		public TaxDateDefaultingOptionLookups TaxDateDefaultingOptionLookups
		{
			get { return (TaxDateDefaultingOptionLookups)ChargeGroupSettingLookups; }
		}

		protected override JobConfigurationSelectorLookups GetNewLookups()
		{
			return new TaxDateDefaultingOptionLookups(this);
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.TaxDateOption, TaxDateOption);
			writer.WriteElementString(Schema.Ledger, Ledger);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);
			TaxDateOption = reader.ReadElementString(Schema.TaxDateOption);
			Ledger = reader.ReadElementString(Schema.Ledger);
		}

		#endregion
	}
}
