using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(Enterprise.Customs.ZA.Business.AsycudaManifestHeaderAssemblyData),
	Enterprise.Core.Constants.DocManagerCodes.OutturnGateInOut)]

namespace Enterprise.Customs.ZA.Business
{
	public class AsycudaManifestHeaderAssemblyData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(AsycudaManifestHeader);

		public override string ReferenceType => Enterprise.Core.Constants.ReferenceTypes.SupplyChainLogistics;

		protected override Type CollectionType => typeof(AsycudaManifestHeaderCollection);

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new AsycudaManifestHeaderCollection(factory);
		}
	}
}
