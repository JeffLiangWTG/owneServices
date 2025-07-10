using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	[XmlRoot("Truck")]
	sealed class BigCar : Car
	{
		public new static class Schema
		{
			public const string MaxWeight = "MaxWeight";
			public const string MaxWeightString = "MaxWeightString";
		}

		public BigCar(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ZInt MaxWeight
		{
			get { return maxWeight; }
			set { maxWeight = value; }
		}
		ZInt maxWeight;

		public ZString MaxWeightString
		{
			get { return maxWeight.ToString(); }
		}
	}
}
