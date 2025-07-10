using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(FlightScheduleData),
	Enterprise.Core.Constants.DocManagerCodes.FlightSchedule)]

namespace Enterprise.Freight.Business
{
	using System;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;
	using static Enterprise.Core.Constants;
	using ResString = Enterprise.Freight.ResString;

	// Sailing schedules aren't allocated - the tab is actually on the voyage but the module returns a sailing
	// This wont be supported until module is changed to return the voyage (only when needed... no requests yet)
	class FlightScheduleData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(JobVoyage); } }
		protected override Type CollectionType
		{
			get { return null; }
		}
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("4e772e21-a402-4f3f-8ab8-839c11b38f55", "Flight Schedule"); } }

		public override IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport() => new JobVoyageEDocsViaUniversalXmlSupport(TransportModes.Air);
	}
}
