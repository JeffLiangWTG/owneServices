using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DataTransfer.Universal
{
	class CommonConsolEventParentFinderHelper
	{
		internal CommonConsolEventParentFinderHelper(IXmlEventValueObject xmlEvent, IUniversalFreightHelper helper)
		{
			this.xmlEvent = xmlEvent;
			this.helper = Argument.NotNull(helper, "helper");

			var mawbNumber = xmlEvent.Context.MAWBNumber.Replace("-", "");
			if (!mawbNumber.IsEmpty)
			{
				this.Masterbill = mawbNumber;
			}

			var mbolNumber = xmlEvent.Context.MBOLNumber.GetValueOrDefault();
			if (!mbolNumber.IsEmpty)
			{
				this.Masterbill = mbolNumber;
			}

			isAir = !mawbNumber.IsEmpty && mbolNumber.IsEmpty;
			isNotAir = !mbolNumber.IsEmpty && mawbNumber.IsEmpty;

			var bookingRef = xmlEvent.Context.CarriersBookingReference;
			if (!bookingRef.IsEmpty)
			{
				this.BookingReference = bookingRef;
			}
		}

		readonly IXmlEventValueObject xmlEvent;
		readonly IUniversalFreightHelper helper;

		internal ZString Masterbill { get; private set; }
		internal ZString BookingReference { get; private set; }

		readonly bool isAir;
		readonly bool isNotAir;

		internal ZDBOnlySubQuery BuildConsolSubQueryIfMasterBillPresent()
		{
			var consolQuery = new ZDBOnlySubQuery(typeof(CommonConsol), JobConsolSchema.PK);
			helper.AddConsolParameters(consolQuery);

			if (!Masterbill.IsEmpty)
			{
				consolQuery.AddToFilter(JobConsolSchema.JK_MasterBillNum, Masterbill);

				if (isAir)
				{
					consolQuery.AddToFilter(JobConsolSchema.JK_TransportMode, Constants.TransportModes.Air);
				}
				else if (isNotAir)
				{
					consolQuery.AddToFilter(JobConsolSchema.JK_TransportMode, SQLComparisonOperator.NotEqual, Constants.TransportModes.Air);
				}
			}

			return consolQuery;
		}

		internal CommonConsolReferences GetConsolReferences()
		{
			var consolReferences = new CommonConsolReferences();
			consolReferences.MBOLNumber = xmlEvent.Context.MBOLNumber.GetValueOrDefault();
			consolReferences.MAWBNumber = xmlEvent.Context.MAWBNumber.Replace("-", "");
			consolReferences.CarriersBookingReference = xmlEvent.Context.CarriersBookingReference;
			consolReferences.AgentsReference = xmlEvent.Context.AgentsReference;

			if (helper.ConsolHasAdditionalReferences)
			{
				consolReferences.PopulateAdditionalReferences(xmlEvent);
			}

			consolReferences.LoadPort = xmlEvent.Context.MBOLOriginUNLOCO;
			consolReferences.DischargePort = xmlEvent.Context.MBOLDestinationUNLOCO;
			consolReferences.CarrierC1CCode = xmlEvent.Context.CarrierC1CCode.GetValueOrDefault();
			consolReferences.MatchMainCarrierReferencesToCoLoader = (xmlEvent.DataContext?.DataProviderForCodeMapping ?? ZString.Empty).EqualsIgnoringCase($"WTG Tracking & Automation");

			return consolReferences;
		}
	}
}
