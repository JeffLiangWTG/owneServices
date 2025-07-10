using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(TruckingScheduleData),
	Enterprise.Core.Constants.DocManagerCodes.TruckingSchedule)]

namespace Enterprise.Freight.Business
{
	using System;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;
	using ResString = Enterprise.Freight.ResString;

	// Sailing schedules aren't allocated - the tab is actually on the voyage but the module returns a sailing
	// This wont be supported until module is changed to return the voyage (only when needed... no requests yet)
	class TruckingScheduleData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(JobVoyage); } }
		protected override Type CollectionType
		{
			get { return null; }
		}
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("3359861e-bc83-4af1-9b4a-d7ee18e01756", "Trucking Schedule"); } }
		public override IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport() => new JobVoyageEDocsViaUniversalXmlSupport(Constants.TransportModes.Road);
	}
}
