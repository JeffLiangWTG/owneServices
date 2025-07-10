using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.US.DataRegistry.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.US.Business.XmlSerializers")]
	public class BoxNumber : RegistryBusinessObjectTemplate
	{
		#region Schema
		public static class Schema
		{
			public const string TransportMode = "TransportMode";
			public const int TransportMode_MaxLength = 3;
			public const string BoxNo = "BoxNo";
			public const int BoxNo_MaxLength = 10;
		}
		#endregion

		#region TransportMode

		[List(nameof(BoxNoTransportList))]
		[MaxLength(Schema.TransportMode_MaxLength)]
		public ZString TransportMode
		{
			get { return transportMode; }
			set
			{
				SetNonPersistentPropertyValue(TransportModeInfo, ref transportMode, value);
				if (!IsValidationSuspended)
				{
					ValidateTransportMode();
				}
			}
		}
		ZString transportMode;

		public ZPropertyInfo TransportModeInfo
		{
			get { return GetZPropertyInfo(Schema.TransportMode); }
		}

		void ValidateTransportMode()
		{
			TransportModeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(TransportModeInfo, BoxNoTransportList);
			if (ParentCollections.Count > 0)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(TransportModeInfo, DuplicateTransportMode);
			}
		}
		internal const string DuplicateTransportMode = "You can only have 1 of each Transport Mode, (ALL, AIR or SEA), for Box Numbers.";

		#endregion

		#region BoxNo

		[MaxLength(Schema.BoxNo_MaxLength)]
		public ZString BoxNo
		{
			get { return boxNo; }
			set
			{
				SetNonPersistentPropertyValue(BoxNoInfo, ref boxNo, value);
				if (!IsValidationSuspended)
				{
					ValidateBoxNo();
				}
			}
		}
		ZString boxNo;

		public ZPropertyInfo BoxNoInfo
		{
			get { return GetZPropertyInfo(Schema.BoxNo); }
		}

		void ValidateBoxNo()
		{
			BoxNoInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(BoxNoInfo);
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			BoxNumber result = new BoxNumber();
			result.TransportMode = TransportMode;
			result.BoxNo = BoxNo;
			return result;
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.TransportMode, TransportMode.ToString());
			writer.WriteElementString(Schema.BoxNo, BoxNo.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			TransportMode = reader.ReadElementString(Schema.TransportMode);
			BoxNo = reader.ReadElementString(Schema.BoxNo);
		}

		protected override void RunPreSaveValidationCore()
		{
			ClearRowNotifications();
			base.RunPreSaveValidationCore();
			ValidateTransportMode();
			ValidateBoxNo();
		}

		#region Lookups

		public CodeDescriptionPairList BoxNoTransportList
		{
			get { return new BoxNoTransportModeList(); }
		}

		#endregion
	}
}
