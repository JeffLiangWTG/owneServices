using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ZA.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.ZA.Business.XmlSerializers")]
	public class AutomaticDeferredSelection : RegistryBusinessObjectTemplate
	{
		public AutomaticDeferredSelection()
		{
		}

		public AutomaticDeferredSelection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public static class Schema
		{
			public const string AllowAutomaticDeferredSelection = "AllowAutomaticDeferredSelection";
			public const string DaysBeforeETA = "DaysBeforeETA";
		}

		#region AllowAutomaticDeferredSelection

		[CargoWiseOne.ResourceStrings.ResourceStringData("Enterprise.Customs.ZA.DataRegistry.Business.AutomaticDeferredSelection|AllowAutomaticDeferredSelection", Caption = "Allow Automatic Deferred Selection")]
		public ZBool AllowAutomaticDeferredSelection
		{
			get { return allowAutomaticDeferredSelection; }
			set
			{
				SetNonPersistentPropertyValue(AllowAutomaticDeferredSelectionInfo, ref allowAutomaticDeferredSelection, value);
			}
		}
		ZBool allowAutomaticDeferredSelection;

		public ZPropertyInfo AllowAutomaticDeferredSelectionInfo
		{
			get { return GetZPropertyInfo(Schema.AllowAutomaticDeferredSelection); }
		}

		#endregion

		#region DaysBeforeETA

		[CargoWiseOne.ResourceStrings.ResourceStringData("Enterprise.Customs.ZA.DataRegistry.Business.AutomaticDeferredSelection|DaysBeforeETA", Caption = "No. of Days Before ETA to Submit")]
		public ZInt DaysBeforeETA
		{
			get { return daysBeforeETA; }
			set
			{
				SetNonPersistentPropertyValue(DaysBeforeETAInfo, ref daysBeforeETA, value);
				if (!IsValidationSuspended)
				{
					ValidateDaysBeforeETA();
				}
			}
		}
		ZInt daysBeforeETA;

		public ZPropertyInfo DaysBeforeETAInfo
		{
			get { return GetZPropertyInfo(Schema.DaysBeforeETA); }
		}

		void ValidateDaysBeforeETA()
		{
			DaysBeforeETAInfo.ClearAllNotifications();
			if (allowAutomaticDeferredSelection)
			{
				MandatoryValidation.CheckNotNegative(DaysBeforeETAInfo);
			}
		}

		#endregion

		#region Overrides

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateDaysBeforeETA();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AutomaticDeferredSelection(fallbackLevel, factory)
			{
				AllowAutomaticDeferredSelection = AllowAutomaticDeferredSelection,
				DaysBeforeETA = DaysBeforeETA
			};
		}

		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.AllowAutomaticDeferredSelection, AllowAutomaticDeferredSelection.ToString());
			writer.WriteElementString(Schema.DaysBeforeETA, DaysBeforeETA.ToString());
		}

		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			AllowAutomaticDeferredSelection = reader.ReadElementStringAsZBool(Schema.AllowAutomaticDeferredSelection);
			DaysBeforeETA = reader.ReadElementStringAsZInt(Schema.DaysBeforeETA);
		}

		#endregion
	}
}
