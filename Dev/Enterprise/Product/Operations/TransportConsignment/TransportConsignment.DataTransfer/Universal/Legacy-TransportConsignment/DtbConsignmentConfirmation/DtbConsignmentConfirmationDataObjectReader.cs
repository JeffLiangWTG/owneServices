using System;
using CargoWise.EntityFramework;
using Enterprise.TransportCommon.DataTransfer.Universal;
using Enterprise.TransportConsignment.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.DataTransfer.Universal
{
	class DtbConsignmentConfirmationDataObjectReader : DtbTransportConfirmationDataObjectReader<DtbConsignmentConfirmation>
	{
		public DtbConsignmentConfirmationDataObjectReader(Confirmation confirmationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory, IColumnIndexer parent)
			: base(confirmationDataObject, logger, factory, parent)
		{ }

		protected override void PopulateBusinessObject(DtbConsignmentConfirmation confirmation)
		{
			base.PopulateBusinessObject(confirmation);

			var row = GetColumnIndexerFromRow(confirmation);
			if (row[DtbBookingConfirmationSchema.Constants.KK_RequiredFrom] == DBNull.Value)
			{
				SetValue(row, DtbBookingConfirmationSchema.KK_RequiredFrom, dataObject.EstimatedDate);
			}
		}
	}
}
