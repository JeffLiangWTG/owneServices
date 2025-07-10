using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.TransportBookings.DataTransfer.Universal
{
	class DtbBookingInstructionPkgDivotDataObjectReaderForContainer : DtbBookingInstructionPkgDivotDataObjectReader<InstructionContainerLink>
	{
		public DtbBookingInstructionPkgDivotDataObjectReaderForContainer(InstructionContainerLink confirmationParentDivotDataObject, IXmlImportLogger logger,
			UniversalObjectFactory factory, Dictionary<ZInt, PkgPackage> packageLinks, IEnumerable<Confirmation> confirmationDataObjectsToExclude)
			: base(confirmationParentDivotDataObject, logger, factory, packageLinks, confirmationDataObjectsToExclude)
		{
		}

		protected override ZInt GetPackageLinkFromDataObject()
		{
			return dataObject.ContainerLink.GetValueOrDefault();
		}
	}
}
