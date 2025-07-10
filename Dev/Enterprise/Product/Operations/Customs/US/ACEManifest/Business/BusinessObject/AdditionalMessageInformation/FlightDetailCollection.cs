using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class FlightDetailCollection : NonPersistentBusinessObjectCollection<FlightDetail>
	{
		public FlightDetailCollection(AsycudaManifestHeader header)
			: base(header.Factory)
		{
			this.header = header;
		}
		readonly AsycudaManifestHeader header;

		public void InitialiseFlightArrival()
		{
			using (SuspendSettingHasChanges())
			{
				RemoveAndDeleteAll();
				PopulateFlightArrivalProperties();
			}
		}

		void PopulateFlightArrivalProperties()
		{
			var arrivalHeaders = header.ArrivalHeaders;
			var arrivalHeadersCount = arrivalHeaders.Count;
			if (arrivalHeadersCount > 0)
			{
				foreach (AsycudaArrivalHeader arrivalHeader in arrivalHeaders)
				{
					AddNewFlightDetail(arrivalHeader.ATH_VoyageFlightNo, arrivalHeader.ATH_ETAAtDischargePort.Date, arrivalHeader.ATH_Reference, arrivalHeadersCount == 1, true);
				}
			}
			else
			{
				AddNewFlightDetail(header.AMA_Voyage, header.AMA_E_ARV.Date, ZString.Empty, true, false);
			}
		}

		protected FlightDetail AddNewFlightDetail(ZString flightNo, ZDate arrivalDate, ZString flightRef, ZBool selected, ZBool isArrival)
		{
			var flightDetail = AddNew();
			using (flightDetail.SuspendSettingHasChanges())
			{
				flightDetail.FlightNo = flightNo;
				flightDetail.FlightArrivalDate = arrivalDate;
				flightDetail.FlightReference = flightRef;
				flightDetail.Selected = selected;
				flightDetail.IsArrival = isArrival;
			}
			return flightDetail;
		}

		public IEnumerable<FlightDetail> GetSelectedFlights() => this.Cast<FlightDetail>().Where(x => x.Selected);

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new FlightDetail(Factory, this);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
