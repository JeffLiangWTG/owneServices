using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	/// <summary>
	/// Used in FlattenEntryLineAndBillForImmediateDeliveryCollection.
	/// Entry Summary Immediate Delivery Document shows information from 2 collections (Bills and Entry Lines) on the same row.
	/// To get everything together, need to push both bits of information into one type of NonPersistentBO 
	/// and then output from a collection containing one NPBO per line.
	/// That would mean NPBO would have 7 fields on it (first 4 fields Bill information and then Entry Line information).
	/// </summary>
	public class FlattenEntryLineAndBillForImmediateDelivery : NonPersistentBusinessObject, IObsoleteValidation
	{
		public FlattenEntryLineAndBillForImmediateDelivery(ZString tariff, ZString countryOfOrigin, ZString manufacturerID)
		{
			this.tariff = tariff;
			this.countryOfOrigin = countryOfOrigin;
			this.manufacturerID = manufacturerID;
		}

		#region Bill information

		public ZString ManifestQuantityAndUQ
		{
			get { return !ManifestQuantity.IsEmpty ? (ManifestQuantity.ToString("0.#####", CultureInfo.InvariantCulture) + " " + manifestQuantityUQ) : string.Empty; }
		}

		public ZString ItBlAwbCode
		{
			get { return itBlAwbCode; }
			set { itBlAwbCode = value; }
		}
		ZString itBlAwbCode;

		public ZString ItBlAwbNumber
		{
			get { return itBlAwbNumber; }
			set { itBlAwbNumber = value; }
		}
		ZString itBlAwbNumber;

		public ZString PortOfLading
		{
			get { return portOfLading; }
			set { portOfLading = value; }
		}
		ZString portOfLading;

		public ZDecimal ManifestQuantity
		{
			get { return manifestQuantity; }
			set { manifestQuantity = value; }
		}
		ZDecimal manifestQuantity;

		public ZString ManifestQuantityUQ
		{
			get { return manifestQuantityUQ; }
			set { manifestQuantityUQ = value; }
		}
		ZString manifestQuantityUQ;

		#endregion

		#region Entry Line information

		public ZString Tariff
		{
			get { return tariff; }
		}

		public ZString CountryOfOrigin
		{
			get { return countryOfOrigin; }
		}

		public ZString ManufacturerID
		{
			get { return manufacturerID; }
		}

		#endregion

		readonly ZString tariff;
		readonly ZString countryOfOrigin;
		readonly ZString manufacturerID;
	}
}
