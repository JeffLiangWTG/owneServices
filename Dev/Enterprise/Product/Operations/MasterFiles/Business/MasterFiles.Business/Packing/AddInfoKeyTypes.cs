namespace Enterprise.MasterFiles.Business
{
	using Enterprise.ZArchitecture.Core;

	public class AddInfoKeyTypes : CodeDescriptionPairList
	{
		public static class Types
		{
			public const string PreviousPackageID = "PreviousPackageID";
			public const string ForwardingShipment = "ForwardingShipment";
			public const string IsManifestedPackage = "IsManifestedPackage";
		}
	}
}
