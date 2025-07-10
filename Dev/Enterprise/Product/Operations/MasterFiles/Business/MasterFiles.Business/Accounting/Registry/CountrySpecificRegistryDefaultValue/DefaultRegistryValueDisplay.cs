using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.MasterFiles.Business.CountrySpecificRegistryDefaultValue
{
	public class DefaultRegistryValueDisplay : NonPersistentBusinessObject
	{
		public DefaultRegistryValueDisplay(ZString caption, ZString defaultValue) : base(new ReadOnlyBusinessObjectFactory())
		{
			Caption = caption;
			DefaultValue = defaultValue;
		}

		[ResourceStringData("CEA66CA7-85B9-4867-8FCF-D2E0DB08CF35", Caption = "Caption")]
		public ZString Caption { get; }

		[ResourceStringData("08F1E5B6-138A-4A7C-BB3E-E4A62E13478D", Caption = "Default Value")]
		public ZString DefaultValue { get; }
	}
}
