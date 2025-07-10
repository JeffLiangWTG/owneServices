using System.ComponentModel;
using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.Registry.AWB
{
	[DebuggerDisplay("IATACode: {IATACode}, Entitlement: {Entitlement}")]
	[XmlSerializerAssembly("Enterprise.Freight.Forwarding.Registry.XmlSerializers")]
	public class AWBDisplayOption : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string IATACode = "IATACode";
			public const string IATADescription = "IATADescription";
			public const string Visibility = "Visibility";
			public const string Entitlement = "Entitlement";
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AWBDisplayOption();
		}

		#endregion

		#region Properties

		#region IATACode

		[ReadOnly(true)]
		[MaxLength(3)]
		public ZString IATACode
		{
			get { return fIATACode; }
			set
			{
				SetNonPersistentPropertyValue(IATACodeInfo, ref fIATACode, value);
			}
		}

		ZString fIATACode;

		public ZPropertyInfo IATACodeInfo
		{
			get { return GetZPropertyInfo(Schema.IATACode); }
		}

		#endregion

		#region IATADescription

		[ReadOnly(true)]
		public MultilingualString IATADescription
		{
			get { return iATADescription ?? (NoResString)""; }
			set
			{
				if (value == null)
				{
					value = (NoResString)"";
				}
				SetNonPersistentPropertyValue(IATADescriptionInfo, ref iATADescription, value, false);
			}
		}

		MultilingualString iATADescription;

		public ZPropertyInfo IATADescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.IATADescription); }
		}

		#endregion

		#region Visibility

		[List("VisibilityModes")]
		[MaxLength(4)]
		public ZString Visibility
		{
			get { return fVisibility; }
			set
			{
				SetNonPersistentPropertyValue(VisibilityInfo, ref fVisibility, value);
				if (!IsValidationSuspended)
				{
					ValidateVisibility();
				}
			}
		}

		ZString fVisibility;

		public ZPropertyInfo VisibilityInfo
		{
			get { return GetZPropertyInfo(Schema.Visibility); }
		}

		public void ValidateVisibility()
		{
			VisibilityInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(VisibilityInfo, VisibilityModes);
		}

		#endregion

		#region Entitlement

		[List("EntitlementModes")]
		[MaxLength(7)]
		public ZString Entitlement
		{
			get { return fEntitlement; }
			set
			{
				SetNonPersistentPropertyValue(EntitlementInfo, ref fEntitlement, value);
				if (!IsValidationSuspended)
				{
					ValidateEntitlement();
				}
			}
		}

		ZString fEntitlement;

		public ZPropertyInfo EntitlementInfo
		{
			get { return GetZPropertyInfo(Schema.Entitlement); }
		}

		public void ValidateEntitlement()
		{
			EntitlementInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(EntitlementInfo, EntitlementModes);
		}

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateVisibility();
			ValidateEntitlement();
		}

		#endregion

		#region BindToLists

		public CodeDescriptionPairList VisibilityModes
		{
			get
			{
				if (fVisibilityModes == null)
				{
					fVisibilityModes = new CodeDescriptionPairList();
					fVisibilityModes.AddPair(nameof(AWBDisplayOptionVisibility.Hide), ResString.GetMultilingualString("84507ad8-8f48-449e-b008-4da62feef601", "Hide"));
					fVisibilityModes.AddPair(nameof(AWBDisplayOptionVisibility.Show), ResString.GetMultilingualString("28f130e0-97c4-4c18-9bfd-8b934eff93c0", "Show"));
				}
				return fVisibilityModes;
			}
		}

		CodeDescriptionPairList fVisibilityModes;

		public CodeDescriptionPairList EntitlementModes
		{
			get
			{
				if (fEntitlementModes == null)
				{
					fEntitlementModes = new CodeDescriptionPairList(OLookUpEditType.AWBEntitlementCodes);
					fEntitlementModes.AddPair(Core.Constants.AWB.EntitlementCode.Split, ResString.GetMultilingualString("a524c67e-c0d0-4858-b0a0-818e1200ed90", "Split"));
				}
				return fEntitlementModes;
			}
		}

		CodeDescriptionPairList fEntitlementModes;

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.IATACode, IATACode);
			writer.WriteElementString(Schema.IATADescription, IATADescription);
			writer.WriteElementString(Schema.Entitlement, Entitlement);
			writer.WriteElementString(Schema.Visibility, Visibility);
		}

		protected override void ReadElements(XmlReaderWrapper wrapper)
		{
			IATACode = wrapper.ReadElementString(Schema.IATACode);
			IATADescription = (NoResString)wrapper.ReadElementString(Schema.IATADescription);
			Entitlement = wrapper.ReadElementString(Schema.Entitlement);
			Visibility = wrapper.ReadElementString(Schema.Visibility);
		}

		#endregion

		public static readonly string MissingIATACode = "MSC";
		internal static MultilingualString MissingIATADescription
		{
			get { return ResString.GetMultilingualString("2fd5b2eb-938e-47ec-baa1-ad7c223ca7bf", "Blank IATA Code Mapping"); }
		}
	}

	public enum AWBDisplayOptionVisibility
	{
		Hide = 0,
		Show = 1,
	}

	public enum AWBDisplayOptionType
	{
		HAWB = 0,
		MAWB = 1,
	}
}
