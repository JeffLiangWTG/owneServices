using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.DataTransfer.Universal.Outturn
{
	public class TWGateEventProcessor
	{
		public TWGateEventProcessor(IXmlEventValueObject eventDataObject, IXmlImportLogger logger)
		{
			this.eventDataObject = Argument.NotNull(eventDataObject, nameof(eventDataObject));
			this.logger = Argument.NotNull(logger, nameof(logger));
		}

		#region Process

		public void Process(CusOutturn outturn)
		{
			PopulateCargoReceiptDateOnOutturns(outturn, eventDataObject);
		}

		#endregion

		void PopulateCargoReceiptDateOnOutturns(CusOutturn outturn, IXmlEventValueObject eventDataObject)
		{
			var containerGateInTime = eventDataObject.Context.TimeOfArrival.GetValueOrDefault();

			if (!containerGateInTime.IsEmpty)
			{
				outturn.C5_CargoReceiptDate = containerGateInTime;
			}
		}

		protected readonly IXmlEventValueObject eventDataObject;
		protected readonly IXmlImportLogger logger;
	}
}
