using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Business
{
	[XmlSerializerAssembly("Enterprise.Freight.XmlSerializers")]
	public class DefaultContainerModes : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string TransportMode = "TransportMode";
			public const string ContainerMode = "ContainerMode";
		}

		#endregion

		public DefaultContainerModes()
		{
		}

		public DefaultContainerModes(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DefaultContainerModes(factory);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateTransportMode();
			ValidateContainerMode();
		}

		#region Bound Properties

		#region Transport Mode

		[MaxLength(3)]
		public ZString TransportMode
		{
			get { return fTransportMode; }
			set
			{
				CheckMaximumLength(TransportModeInfo, value);
				SetNonPersistentPropertyValue(TransportModeInfo, ref fTransportMode, value);

				if (!IsValidationSuspended)
				{
					ValidateTransportMode();
				}
			}
		}

		public ZPropertyInfo TransportModeInfo
		{
			get { return GetZPropertyInfo(Schema.TransportMode); }
		}

		public void ValidateTransportMode()
		{
			TransportModeInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(TransportModeInfo);
			ListValidation.ErrorIfInvalidCode(TransportModeInfo, TransportModeList);

			if (!TransportMode.IsValid || TransportMode == "")
			{
				TransportModeInfo.AddError(Res.GetString("6ceba3d8-d678-404a-9fcb-b11d54c662e0", "Please enter a valid Transport Type code."));
			}
			else
			{
				CheckDuplicateItems();
			}
		}

		ZString fTransportMode;

		#region CheckDuplicateItems

		void CheckDuplicateItems()
		{
			if (ParentCollections.Count > 0 && ((DefaultContainerModesCollection)ParentCollections.First()).IsDuplicateItem(this))
			{
				TransportModeInfo.AddError(Res.GetString("19179818-c082-4e03-b035-7343e813f4e2", "This Transport Mode has already been entered with a default Container Mode for this fallback level"));
			}
		}

		#endregion

		#endregion

		#region Container Mode

		[MaxLength(3)]
		public ZString ContainerMode
		{
			get { return fContainerMode; }
			set
			{
				CheckMaximumLength(ContainerModeInfo, value);
				SetNonPersistentPropertyValue(ContainerModeInfo, ref fContainerMode, value);

				if (!IsValidationSuspended)
				{
					ValidateContainerMode();
				}
			}
		}

		public ZPropertyInfo ContainerModeInfo
		{
			get { return GetZPropertyInfo(Schema.ContainerMode); }
		}

		public void ValidateContainerMode()
		{
			ContainerModeInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(ContainerModeInfo);
			ListValidation.ErrorIfInvalidCode(ContainerModeInfo, ContainerModeList);

			if (!ContainerMode.IsValid || ContainerMode == "")
			{
				ContainerModeInfo.AddError(Res.GetString("520f3007-f1a4-4f32-aee0-378da8d6ef25", "Please enter a valid Container Packing Mode code."));
			}
		}

		ZString fContainerMode;

#if DEBUG
		internal bool DoNotPerformListValidationOnContainerMode;
#endif

		#endregion

		#endregion

		#region Lookups

		#region Transport Mode List

		public CodeDescriptionPairList TransportModeList
		{
			get
			{
				if (fTransportModeList == null)
				{
					fTransportModeList = new CodeDescriptionPairList(OLookUpEditType.TransportType);
				}

				return fTransportModeList;
			}
		}

		CodeDescriptionPairList fTransportModeList;

		#endregion

		#region Container Mode List

		public CodeDescriptionPairList ContainerModeList
		{
			get
			{
				var result = new CodeDescriptionPairList();

				if (!TransportMode.IsEmpty)
				{
					result = CurrentFactory.GetCachedValue("FreightCodePairLists.JS_PackingModeList_" + TransportMode,
							() => FreightCodePairLists.JS_PackingModeList(TransportMode));
				}

				return result;
			}
		}

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.TransportMode, TransportMode);
			writer.WriteElementString(Schema.ContainerMode, ContainerMode);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			TransportMode = reader.ReadElementString(Schema.TransportMode);
			ContainerMode = reader.ReadElementString(Schema.ContainerMode);
		}

		#endregion
	}
}
