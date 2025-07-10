using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.DataTransfer.Universal;
using Enterprise.TransportCommon.DataTransfer.Universal.Testing;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	class DtbConsignmentConfirmationDataObjectReaderTest : DtbTransportConfirmationDataObjectReaderTest<DtbConsignmentConfirmation>
	{
		#region TestRequiredByToFallbackToEstimatedDateIfEmpty
		public void TestRequiredByToFallbackToEstimatedDateIfEmpty()
		{
			var year = ZDateTime.Now.Year;
			var confirmationDataObject = new Confirmation();
			confirmationDataObject.EstimatedDate = new ZDateTime(year, 1, 2);
			var divot = GetNewDivot();
			var reader = GetNewReader(confirmationDataObject, Logger, Factory, divot.Row());
			var confirmation = reader.ReadIntoBusinessObject();
			CombineAssertions(() =>
			{
				AssertEquals("confirmation.KK_Estimated", new ZDateTime(year, 1, 2), confirmation.KK_Estimated);
				AssertEquals("confirmation.KK_RequiredFrom", new ZDateTime(year, 1, 2), confirmation.KK_RequiredFrom);
			});
		}

		#endregion
		#region Implementation
		protected override DtbTransportInstructionPkgDivot GetNewDivot()
		{
			return Factory.New<DtbConsignmentInstructionPkgDivot>();
		}

		protected override DtbTransportInstruction GetNewInstruction()
		{
			return Factory.New<DtbConsignmentInstruction>();
		}

		protected override DtbTransportConfirmationDataObjectReader<DtbConsignmentConfirmation> GetNewReader(Confirmation confirmationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, IColumnIndexer parent)
		{
			return new DtbConsignmentConfirmationDataObjectReader(confirmationDataObject, logger, factory, parent);
		}
		#endregion
	}
}
