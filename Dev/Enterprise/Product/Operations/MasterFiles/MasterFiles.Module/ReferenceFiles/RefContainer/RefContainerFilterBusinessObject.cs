using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class RefContainerFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Constants
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Code strings")]
			public static class Codes
			{
				public const string ContainerCode = "Container Code";
				public const string Description = "Description";
				public const string ISOCode = "ISO Code";
				public const string ContainerType = "Container Type";
				public const string TransportMode = "Transport Mode";
			}
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = new ModuleFilterCollection();
			AddTextFilters(result);
			return result;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter(Constants.Codes.ContainerCode, RefContainerSchema.RC_Code).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefContainerFilter|ContainerCode", "Container Code");
			filters.AddFiltersForTranslatableText(Constants.Codes.Description, RefContainerSchema.RC_Description, typeof(RefContainer), ResString.GetMultilingualString("MasterFiles|RefContainerFilter|Description", "Description"));
			filters.AddTextFilter(Constants.Codes.ISOCode, RefContainerSchema.RC_ISOType).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefContainerFilter|ISOCode", "ISO Code");
			filters.AddTextFilter(Constants.Codes.ContainerType, RefContainerSchema.RC_ContainerType, ContainerTypes).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefContainerFilter|ContainerType", "Container Type");
			filters.AddTextFilter(Constants.Codes.TransportMode, RefContainerSchema.RC_ShippingMode, ShippingModes).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|RefContainerFilter|TransportMode", "Transport Mode");
		}

		#endregion

		#region Lookups

		public CodeDescriptionPairList ShippingModes => new RefContainerLookups(null).ShippingModesList;

		public CodeDescriptionPairList ContainerTypes => new RefContainerLookups(null).ContainerTypes;

		#endregion
	}
}
