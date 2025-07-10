using Enterprise.Customs.SG.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(SGV4ClassificationData),
	Enterprise.Core.Constants.DocManagerCodes.Classification,
	Country = "SG")]

namespace Enterprise.Customs.SG.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.Customs.SG.V4.Business;
	using ZArchitecture.Modules;
	using ZArchitecture.Modules.DocumentScanning;

	class SGV4ClassificationData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(Customs.Business.BaseCusClassification);
		protected override Type CollectionType => typeof(ClassificationCollection);

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory) => new ClassificationCollection(factory);
		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.SG.SG4Classification;
		public override string ReferenceType => Core.Constants.DocManagerCodes.Classification;
		public override ZArchitecture.Core.MultilingualString HumanReadableName => ResString.GetMultilingualString("7ccb2da6-4ba9-4c6f-b19f-e381e6a9c474", "Classification");
		public override bool IsAllowedForUnallocatedeDocs => true;
	}
}
