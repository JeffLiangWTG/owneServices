using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class Car : XmlSerializableNonPersistentBusinessObject
	{
		public static class Schema
		{
			public const string Number = "Number";
			public const string SeatsCount = "SeatsCount";
			public const string Model = "Model";
			public const string Kilometers = "Kilometers";
			public const string Guid = "Guid";
			public const string ProductionDate = "ProductionDate";
		}

		public Car(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		[MaxLength(10)]
		public ZString Number
		{
			get { return number; }
			set
			{
				number = value;
				HasChanges = true;
			}
		}
		ZString number;

		public ZByte SeatsCount
		{
			get { return seatsCount; }
			set { seatsCount = value; }
		}
		ZByte seatsCount;

		[MaxLength(35)]
		public ZString Model
		{
			get { return model; }
			set { model = value; }
		}
		ZString model;

		public ZInt Kilometers
		{
			get { return kilometers; }
			set { kilometers = value; }
		}
		ZInt kilometers;

		[XmlElement("UID")]
		public ZGuid Guid
		{
			get { return guid; }
			set { guid = value; }
		}
		ZGuid guid;

		public ZDate ProductionDate
		{
			get { return productionDate; }
			set { productionDate = value; }
		}
		ZDate productionDate;

		public string Description { get { return Model + " - " + ProductionDate.Year; } }

		public Engine Engine
		{
			get { return engine ?? (engine = new Engine(Factory)); }
		}
		Engine engine;

		public Radio Radio
		{
			get { return radio ?? (radio = new Radio(Factory)); }
		}
		Radio radio;

		public NonPersistentBusinessObjectCollection<Human> Owners
		{
			get { return owners ?? (owners = new Humans(Factory)); }
		}
		Humans owners;
	}
}
