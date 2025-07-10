using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(SailingScheduleData),
	Enterprise.Core.Constants.DocManagerCodes.SailingSchedule)]

namespace Enterprise.Freight.Business
{
	using System;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;
	using ResString = Enterprise.Freight.ResString;

	// Sailing schedules aren't allocated - the tab is actually on the voyage but the module returns a sailing
	// This wont be supported until module is changed to return the voyage (only when needed... no requests yet)
	class SailingScheduleData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(JobVoyage); } }
		protected override Type CollectionType
		{
			get { return null; }
		}
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("1e4a703c-460d-43d1-8bdc-86329d96cd68", "Sailing Schedule"); } }
		public override IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport() => new JobVoyageEDocsViaUniversalXmlSupport(Constants.TransportModes.Sea);
	}
}
