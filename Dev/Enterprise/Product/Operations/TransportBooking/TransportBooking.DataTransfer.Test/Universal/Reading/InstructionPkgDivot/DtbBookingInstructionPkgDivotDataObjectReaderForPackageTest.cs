using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.TransportBookings.DataTransfer.Universal.Testing
{
	class DtbBookingInstructionPkgDivotDataObjectReaderForPackageTest : DtbBookingInstructionPkgDivotDataObjectReaderTest<InstructionPackingLineLink>
	{
		protected override void SetPackageLink(InstructionPackingLineLink divotDataObject, ZInt link)
		{
			divotDataObject.PackingLineLink = link;
		}

		protected override DtbBookingInstructionPkgDivotDataObjectReader<InstructionPackingLineLink> GetNewReader(InstructionPackingLineLink divotDataObject,
			Dictionary<ZInt, PkgPackage> packageLinks, IEnumerable<Confirmation> confirmationDataObjectsToExclude)
		{
			return new DtbBookingInstructionPkgDivotDataObjectReaderForPackage(divotDataObject, Logger, Factory, packageLinks, confirmationDataObjectsToExclude);
		}
	}
}
