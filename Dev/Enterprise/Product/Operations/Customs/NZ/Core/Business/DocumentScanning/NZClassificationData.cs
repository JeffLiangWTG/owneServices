using Enterprise.Customs.NZ.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(typeof(NZClassificationData),
	Enterprise.Core.Constants.DocManagerCodes.Classification,
	Country = "NZ")]

namespace Enterprise.Customs.NZ.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using MasterFiles;
	using ZArchitecture.Modules;
	
	class NZClassificationData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(Customs.Business.BaseCusClassification);
		protected override Type CollectionType => typeof(Customs.Business.BaseClassificationCollection<CusClassification>);

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory) => new Customs.Business.BaseClassificationCollection<CusClassification>(factory);
		public override ModuleIdentifier ModuleID => ModuleIDs.SingleTariffClassification;
		public override string ReferenceType => Core.Constants.DocManagerCodes.Classification;
		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("ef8b6254-0f0c-448d-aaae-cce677ee4ec4", "Classification");
		public override bool IsAllowedForUnallocatedeDocs => true;
	}
}
