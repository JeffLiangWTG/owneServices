using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	public class ITAndSplitDetailsCollection : DependentCusAddInfoCollection<ITAndSplitDetails, BusinessObject>
	{
		public ITAndSplitDetailsCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.USITNumber)
		{
		}

		public ZInt TotalNoOfPacks
		{
			get
			{
				var result = ZInt.Zero;
				foreach (ITAndSplitDetails itNo in this)
				{
					result += itNo.US_NoOfPacks;
				}
				return result;
			}
		}

		public ITAndSplitDetails[] FindByItNumber(ZString itNumber)
		{
			return new List<ITAndSplitDetails>(Find(x => x.US_ITNumber == itNumber)).ToArray();
		}

		public List<ZString> GetListOfUniqueVITNumbers()
		{
			var result = new List<ZString>();
			foreach (ITAndSplitDetails itNo in this)
			{
				if (itNo.US_ITNumber.StartsWith("V") && !result.Contains(itNo.US_ITNumber))
				{
					result.Add(itNo.US_ITNumber);
				}
			}
			return result;
		}

		internal void RemoveSplitDetails()
		{
			foreach (ITAndSplitDetails itNo in this)
			{
				itNo.US_ArrivalDate = ZDateTime.Empty;
				itNo.US_CarrierCode = ZString.Empty;
				itNo.US_FlightNumber = ZString.Empty;
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var newElement = (ITAndSplitDetails)child;
			var declaration = newElement.Declaration;
			if (this.Count == 0 && declaration != null && (declaration.IsACE || declaration.IsFTZAdmission))
			{
				var bill = newElement.Bill;
				if (bill.US_SESplitShip)
				{
					newElement.US_ArrivalDate = declaration.JE_DateOfArrival;
					newElement.US_CarrierCode = declaration.US_UI_NKCarrierSCAC;
					newElement.US_FlightNumber = declaration.JE_VoyageFlightNo.Left(newElement.US_FlightNumberInfo.MaxLength);
				}
			}
		}

		protected override bool EnableRemovingDependentWithoutDeletingErrorReport { get { return true; } }
	}
}
