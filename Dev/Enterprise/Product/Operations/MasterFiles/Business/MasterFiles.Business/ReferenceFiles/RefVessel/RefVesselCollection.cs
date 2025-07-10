using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.RefVessel)]
	public class RefVesselCollection : ActiveBusinessObjectCollection<RefVessel>, ICodePropertyNameProvider
	{
		#region FilterConstants

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related description")]
		public static class FilterConstants
		{
			public const string CarrierCode = "Carrier Code";
			public const string LloydsNumber = "Lloyds Number";
			public const string RadioCallSign = "Radio Call Sign";
			public const string VesselName = "Vessel Name";

			public const string ActiveStatus = "Active Status";

			public const string CountryOfRegistration = "Country of Registration";

			public const string Carrier = "Carrier";
			public const string Consortium = "Consortium";

			public const string VesselType = "Vessel Type";
		}

		#endregion

		public RefVesselCollection(BusinessObjectFactory factory, ZQuery sQLFilter) : base(factory, sQLFilter)
		{
		}

		public RefVesselCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public RefVesselCollection(BusinessObjectFactory factory, bool useLloyds = false) : base(factory)
		{
			UseLloyds = useLloyds;
		}

		public RefVesselCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}

		public readonly bool UseLloyds;

		#region FindBoxListProvider

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new RefVesselFindBoxListProvider(this); }
		}

		#endregion

		#region ICodePropertyNameProvider
		string ICodePropertyNameProvider.GetCodePropertyName(System.Type type) => (FindBoxListProvider as ICodePropertyNameProvider)?.GetCodePropertyName(type);
		#endregion
	}
}
