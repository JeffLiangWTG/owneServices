using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.TransportBookings.DataTransfer.Universal.Testing
{
	class DtbBookingInstructionPkgDivotDataObjectReaderForContainerTest : DtbBookingInstructionPkgDivotDataObjectReaderTest<InstructionContainerLink>
	{
		protected override void SetPackageLink(InstructionContainerLink divotDataObject, ZInt link)
		{
			divotDataObject.ContainerLink = link;
		}

		protected override DtbBookingInstructionPkgDivotDataObjectReader<InstructionContainerLink> GetNewReader(InstructionContainerLink divotDataObject,
			Dictionary<ZInt, PkgPackage> packageLinks, IEnumerable<Confirmation> confirmationDataObjectsToExclude)
		{
			return new DtbBookingInstructionPkgDivotDataObjectReaderForContainer(divotDataObject, Logger, Factory, packageLinks, confirmationDataObjectsToExclude);
		}
	}
}
