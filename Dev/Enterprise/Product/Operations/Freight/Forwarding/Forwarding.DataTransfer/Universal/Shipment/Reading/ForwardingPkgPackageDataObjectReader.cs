using System.Collections.Generic;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Packing.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using ForwardingPackageCollection = Enterprise.Freight.Forwarding.Business.PackageJob.ForwardingPackageCollection;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ForwardingPkgPackageDataObjectReader : PkgPackageDataObjectReader<ForwardingPackage>
	{
		public ForwardingPkgPackageDataObjectReader(
			PackingLine packingLineData, IXmlImportLogger logger, UniversalObjectFactory factory, ForwardingPackageJob packageJob, ForwardingPackageCollection packageCollection,
			ImportOption importOption = ImportOption.Default, ForwardingPackage targetPackage = null, bool isProcessingOuterPackLine = false, bool shouldCreatePackageExtension = false,
			IEnumerable<PackingLine> packingLineCollection = null, bool shouldPopulateChildPackages = true)
			: base(packingLineData, logger, factory, packageJob, packageCollection, importOption, targetPackage, isProcessingOuterPackLine, shouldCreatePackageExtension, packingLineCollection, shouldPopulateChildPackages)
		{
		}
	}
}
