using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class Radio : XmlSerializableNonPersistentBusinessObject
	{
		public static class Schema
		{
			public const string HasUSB = "HasUSB";
			public const string CurrentTime = "CurrentTime";
		}

		public Radio(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		[XmlArray("FavouriteStations")]
		[XmlArrayItem("Station")]
		public NonPersistentBusinessObjectCollection<RadioStation> SavedStations
		{
			get { return savedStations ?? (savedStations = new StationsCollection(Factory)); }
		}
		StationsCollection savedStations;

		public NonPersistentBusinessObjectCollection<RadioStation> NormalStations
		{
			get { return normalStations ?? (normalStations = new StationsCollection(Factory)); }
		}
		StationsCollection normalStations;

		public ZBool HasUSB
		{
			get { return hasUSB; }
			set { hasUSB = value; }
		}
		ZBool hasUSB;

		public ZDateTime CurrentTime
		{
			get { return currentTime; }
			set { currentTime = value; }
		}
		ZDateTime currentTime;
	}
}
