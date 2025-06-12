using System;

namespace Enterprise.Freight.DistanceCalculation.Integration
{
	[Serializable]
	public class DistanceCalculationAddress
	{
		public DistanceCalculationAddress()
		{
		}

		public DistanceCalculationAddress(string Address1, string Address2, string City, string State, string PostCode, string Country)
		{
			this.Address1 = Address1;
			this.Address2 = Address2;
			this.City = City;
			this.State = State;
			this.PostCode = PostCode;
			this.Country = Country;
		}

		public string Address1 { get; set; }
		public string Address2 { get; set; }
		public string City { get; set; }
		public string State { get; set; }
		public string PostCode { get; set; }
		public string Country { get; set; }

		public override string ToString()
		{
			string addressText = "";

			if (!string.IsNullOrWhiteSpace(Address1) && addressText.IndexOf(Address1) == -1)
			{
				addressText += " " + Address1;
			}

			if (!string.IsNullOrWhiteSpace(Address2) && addressText.IndexOf(Address2) == -1)
			{
				addressText += " " + Address2;
			}

			if (!string.IsNullOrWhiteSpace(City) && addressText.IndexOf(City) == -1)
			{
				addressText += " " + City;
			}

			if (!string.IsNullOrWhiteSpace(State) && addressText.IndexOf(State) == -1)
			{
				addressText += " " + State;
			}

			if (!string.IsNullOrWhiteSpace(PostCode) && addressText.IndexOf(PostCode) == -1)
			{
				addressText += " " + PostCode;
			}

			if (!string.IsNullOrWhiteSpace(Country) && addressText.IndexOf(Country) == -1)
			{
				addressText += " " + Country;
			}

			return addressText;
		}

		public bool IsEmpty
		{
			get
			{
				return (string.IsNullOrEmpty(Address1) &&
						string.IsNullOrEmpty(Address2) &&
						string.IsNullOrEmpty(City) &&
						string.IsNullOrEmpty(State) &&
						string.IsNullOrEmpty(PostCode) &&
						string.IsNullOrEmpty(Country));
			}
		}
	}
}
