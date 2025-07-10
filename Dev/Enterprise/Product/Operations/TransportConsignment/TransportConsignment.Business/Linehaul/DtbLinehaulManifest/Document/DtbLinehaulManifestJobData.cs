using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.TransportConsignment.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(DtbLinehaulManifestJobData),
	Constants.DocManagerCodes.DomesticTransportLinehaulManifest)]

namespace Enterprise.TransportConsignment.Business
{
	public class DtbLinehaulManifestJobData : AssemblyData
	{
		public override Type BusinessObjectType
		{
			get { return typeof(DtbLinehaulManifest); }
		}

		protected override Type CollectionType
		{
			get { return typeof(DtbLinehaulManifestCollection); }
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new DtbLinehaulManifestCollection(factory);
		}

		public override string ReferenceType
		{
			get { return Constants.ReferenceTypes.SupplyChainLogistics; }
		}

		public override MultilingualString HumanReadableName
		{
			get { return ResString.GetMultilingualString("b18e0012-4de3-4fe9-8f36-59b71633d5d3", "Linehaul Manifest"); }
		}

		public override bool IsAllowedForUnallocatedeDocs
		{
			get { return true; }
		}
	}
}
