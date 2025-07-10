using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class RadioStation : XmlSerializableNonPersistentBusinessObject
	{
		public static class Schema
		{
			public const string Frequency = "Frequency";
		}

		public RadioStation(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		[DecimalPrecision(4)]
		[DecimalPlaces(1)]
		public ZDecimal Frequency
		{
			get { return frequency; }
			set { frequency = value; }
		}
		ZDecimal frequency;
	}
}
