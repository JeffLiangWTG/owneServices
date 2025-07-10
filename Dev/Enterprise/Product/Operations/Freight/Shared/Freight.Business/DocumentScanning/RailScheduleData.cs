using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(RailScheduleData),
	Enterprise.Core.Constants.DocManagerCodes.RailSchedule)]

namespace Enterprise.Freight.Business
{
	using System;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;
	using ResString = Enterprise.Freight.ResString;

	// Sailing schedules aren't allocated - the tab is actually on the voyage but the module returns a sailing
	// This wont be supported until module is changed to return the voyage (only when needed... no requests yet)
	class RailScheduleData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(JobVoyage); } }
		protected override Type CollectionType
		{
			get { return null; }
		}
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("e47eb32e-26ae-4a2d-a7b0-013a9e6c5bba", "Rail Schedule"); } }
		public override IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport() => new JobVoyageEDocsViaUniversalXmlSupport(Constants.TransportModes.Rail);
	}
}
