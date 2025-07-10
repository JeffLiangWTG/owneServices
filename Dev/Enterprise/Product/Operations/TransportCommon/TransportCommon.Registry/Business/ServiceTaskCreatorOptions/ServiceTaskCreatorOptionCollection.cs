using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.TransportCommon.Registry
{
	[XmlSerializerAssembly("Enterprise.TransportCommon.Registry.XmlSerializers")]
	public class ServiceTaskCreatorOptionCollection : RegistryBusinessObjectCollectionTemplate
	{
		#region Overrides

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ServiceTaskCreatorOptionCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ServiceTaskCreatorOption();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion

		#region Index

		public new ServiceTaskCreatorOption this[int i]
		{
			get { return (ServiceTaskCreatorOption)Elements[i]; }
		}

		public new ServiceTaskCreatorOption AddNew()
		{
			return (ServiceTaskCreatorOption)base.AddNew();
		}

		#endregion

		#region GetTargetModule

		public ZString GetTargetModule(ZString containerMode)
		{
			var option = this.Cast<ServiceTaskCreatorOption>().FirstOrDefault(o => o.ContainerMode == containerMode);
			return option != null ? option.TargetModule.ToString() : AutoCreatorTargetModules.Codes.PortTransport;
		}

		#endregion

		#region Defaults

		public static ServiceTaskCreatorOptionCollection GetDefault()
		{
			var result = new ServiceTaskCreatorOptionCollection();

			SetupDefaultFields(result.AddNew(), AutoCreatorContainerModes.Codes.Loose, AutoCreatorTargetModules.Codes.PortTransport, false);
			SetupDefaultFields(result.AddNew(), AutoCreatorContainerModes.Codes.Container, AutoCreatorTargetModules.Codes.PortTransport, false);
			SetupDefaultFields(result.AddNew(), AutoCreatorContainerModes.Codes.FTL, AutoCreatorTargetModules.Codes.PortTransport, false);
			SetupDefaultFields(result.AddNew(), AutoCreatorContainerModes.Codes.MixedCargo, AutoCreatorTargetModules.Codes.PortTransport, false);

			return result;
		}

		static void SetupDefaultFields(ServiceTaskCreatorOption serviceTaskOption, ZString containerMode, ZString targetModule, ZBool isSytemDefined)
		{
			using (serviceTaskOption.GetValidationSuspender())
			{
				serviceTaskOption.ContainerMode = containerMode;
				serviceTaskOption.TargetModule = targetModule;
				serviceTaskOption.IsSystemDefined = isSytemDefined;
			}
		}

		#endregion
	}
}
