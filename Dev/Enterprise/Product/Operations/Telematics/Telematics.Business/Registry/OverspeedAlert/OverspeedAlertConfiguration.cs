using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Telematics.Business.Registry
{
	[XmlSerializerAssembly("Enterprise.Telematics.Business.XmlSerializers")]
	public class OverspeedAlertConfiguration : RegistryBusinessObjectTemplate
	{
		public OverspeedAlertConfiguration(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public OverspeedAlertConfiguration()
			: base()
		{
		}

		[List("OverspeedAlertTypes")]
		public ZString OverspeedAlertType
		{
			get { return overspeedAlertType; }
			set
			{
				SetNonPersistentPropertyValue(OverspeedAlertTypeInfo, ref overspeedAlertType, value);
				if (IsOverspeedAlertTypeShownForX)
				{
					DurationInMinutes = 60;
				}
			}
		}

		ZString overspeedAlertType = OverspeedAlertOptions.Default.Code;

		public CodeDescriptionPairList OverspeedAlertTypes
		{
			get
			{
				return OverspeedAlertOptions.CodeList;
			}
		}

		public ZPropertyInfo OverspeedAlertTypeInfo
		{
			get { return GetZPropertyInfo(nameof(OverspeedAlertType)); }
		}

		public ZInt DurationInMinutes
		{
			get
			{
				return durationInMinutes;
			}
			set
			{
				SetNonPersistentPropertyValue(DurationInMinutesInfo, ref durationInMinutes, value);
				ValidateDurationInMinutes();
			}
		}

		public ZPropertyInfo DurationInMinutesInfo
		{
			get { return GetZPropertyInfo(nameof(DurationInMinutes)); }
		}

		ZInt durationInMinutes = 60;

		ZString DurationInMinutesErrorMessage => ResString.GetMultilingualString("ED66014-C004-4660-9B90-249B3D09E4C1", "Enter a numeric value greater than or equal to zero for Duration.");

		public void ValidateDurationInMinutes()
		{
			DurationInMinutesInfo.ClearAllNotifications();
			if (DurationInMinutes < 0)
			{
				DurationInMinutesInfo.AddError(DurationInMinutesErrorMessage);
			}
		}

		public ZBool IsOverspeedAlertTypeShownForX
		{
			get
			{
				return overspeedAlertType == OverspeedAlertOptions.Default.Code;
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateDurationInMinutes();
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(nameof(OverspeedAlertType), overspeedAlertType);
			writer.WriteElementString(nameof(DurationInMinutes), durationInMinutes.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			OverspeedAlertType = new ZString(reader.ReadElementString(nameof(OverspeedAlertType)));
			DurationInMinutes = new ZInt(reader.ReadElementString(nameof(DurationInMinutes)));
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OverspeedAlertConfiguration(fallbackLevel, factory);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			OverspeedAlertConfiguration castedClone = (OverspeedAlertConfiguration)clone;
			castedClone.OverspeedAlertType = OverspeedAlertType;
			castedClone.DurationInMinutes = DurationInMinutes;
		}
	}

	public class OverspeedAlertOptions : CodeDescriptionPairList
	{
		public OverspeedAlertOptions()
			: base()
		{
			Add(Default);
			Add(Never);
			Add(Always);
		}

		public static CodeDescriptionPair Default { get { return new CodeDescriptionPair("DEF", ResString.GetMultilingualString("2E8E5461-D479-4455-AEE8-AC76460D6B29", "shown for X minutes")); } }
		public static CodeDescriptionPair Never { get { return new CodeDescriptionPair("NVR", ResString.GetMultilingualString("185840E0-B56C-4E49-8E20-68F3E8546621", "never shown")); } }
		public static CodeDescriptionPair Always { get { return new CodeDescriptionPair("ALY", ResString.GetMultilingualString("68854B79-BCFC-416E-9689-B1E4050E8D25", "always shown")); } }

		public static CodeDescriptionPairList CodeList
		{
			get
			{
				return new OverspeedAlertOptions();
			}
		}
	}
}
