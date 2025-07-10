using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.TransportCommon.Registry
{
	[XmlSerializerAssembly("Enterprise.TransportCommon.Registry.XmlSerializers")]
	public class ServiceTaskCreatorOption : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string ContainerMode = "ContainerMode";
			public const string TargetModule = "TargetModule";
			public const string IsSystemDefined = "IsSystemDefined";
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var clone = new ServiceTaskCreatorOption();

			using (clone.GetValidationSuspender())
			{
				clone.ContainerMode = ContainerMode;
				clone.TargetModule = TargetModule;
				clone.IsSystemDefined = IsSystemDefined;
			}

			return clone;
		}

		#endregion

		#region Properties

		#region ContainerMode

		[ReadOnly(true)]
		public ZString ContainerMode
		{
			get { return containerMode; }
			set { SetNonPersistentPropertyValue(ContainerModeInfo, ref containerMode, value); }
		}

		public ZPropertyInfo<ZString> ContainerModeInfo
		{
			get { return (ZPropertyInfo<ZString>)GetZPropertyInfo(Schema.ContainerMode); }
		}

		ZString containerMode;

		#endregion

		#region TargetModule

		[List("TargetModules")]
		[ReadOnlyMember(nameof(IsSystemDefined))]
		public ZString TargetModule
		{
			get { return targetModule; }
			set
			{
				SetNonPersistentPropertyValue(TargetModuleInfo, ref targetModule, value);
				if (!IsValidationSuspended)
				{
					ValidateTargetModule();
				}
			}
		}

		public ZPropertyInfo<ZString> TargetModuleInfo
		{
			get { return (ZPropertyInfo<ZString>)GetZPropertyInfo(Schema.TargetModule); }
		}

		ZString targetModule;

		#endregion

		#region IsSystemDefined

		public ZBool IsSystemDefined
		{
			get { return isSystemDefined; }
			set { isSystemDefined = value; }
		}

		ZBool isSystemDefined;

		#endregion

		#endregion

		#region Validation

		#region ValidateTargetModule

		public void ValidateTargetModule()
		{
			TargetModuleInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(TargetModuleInfo);
			ListValidation.ErrorIfInvalidCode(TargetModuleInfo);
		}

		#endregion

		#endregion

		#region Delete

		public override bool CanDelete
		{
			get { return false; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("ServiceTaskCreatorOption|ReasonForNotAbleToDelete", "Cannot be deleted."); }
		}

		#endregion

		#region Lists

		#region TargetModules

		public CodeDescriptionPairList TargetModules
		{
			get { return targetModules ?? (targetModules = new AutoCreatorTargetModules().List); }
		}

		CodeDescriptionPairList targetModules;

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.ContainerMode, ContainerMode.ToString());
			writer.WriteElementString(Schema.TargetModule, TargetModule.ToString());
			writer.WriteElementString(Schema.IsSystemDefined, IsSystemDefined.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper wrapper)
		{
			using (GetValidationSuspender())
			{
				ContainerMode = wrapper.ReadElementString(Schema.ContainerMode);
				TargetModule = wrapper.ReadElementString(Schema.TargetModule);
				IsSystemDefined = wrapper.ReadElementStringAsZBool(Schema.IsSystemDefined);
			}
		}

		#endregion
	}
}
