using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Cartonisation.Business;
using Enterprise.Warehouse.Cartonisation.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Cartonisation.Diagnostic
{
	public class CartonisationDiagnostics : NonPersistentBusinessObject
	{
		public CartonisationDiagnostics()
			: this(new CartonisationAlgorithm())
		{
		}

		public CartonisationDiagnostics(ICartonisation algorithm)
		{
			Algorithm = algorithm;
		}

		readonly ICartonisation Algorithm;

		#region Cartons

		public CartonsCollection Cartons
		{
			get { return cartons ?? (cartons = new CartonsCollection()); }
		}

		CartonsCollection cartons;

		#endregion

		#region ItemsToPack

		public ItemsToPackCollection ItemsToPack
		{
			get { return itemsToPack ?? (itemsToPack = new ItemsToPackCollection()); }
		}

		ItemsToPackCollection itemsToPack;

		#endregion

		#region Cartonise

		public void Cartonise()
		{
			try
			{
				var startTime = ZDateTime.Now;
				var result = Algorithm.CartoniseItems(ItemsToPack.Cast<ICartonisableItem>(), Cartons.Cast<ICartonDefinition>());
				var duration = ZDateTime.Now - startTime;
				DiagnosticCartonisationResult = new DiagnosticCartonisationResult(Cartons, ItemsToPack, result, duration);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError((NoResString)"Error: " + ex.Message);
			}
		}

		#endregion

		#region SaveCartonsToXml

		public void SaveCartonsToXml(string fileName)
		{
			var serialiser = new XmlSerializer(typeof(List<SerializableCartonDefinition>));
			using (var stream = ZSaveFileDialog.OpenFile(fileName))
			{
				serialiser.Serialize(stream, Cartons.Cast<DummyCartonDefinition>().Select(c => new SerializableCartonDefinition(c)).ToList());
			}
		}

		#endregion

		#region LoadCartonsFromXml

		public void LoadCartonsFromXml(string fileName)
		{
			var serialiser = new XmlSerializer(typeof(List<SerializableCartonDefinition>));
			using (var stream = ZOpenFileDialog.OpenFile(fileName))
			{
				var result = (List<SerializableCartonDefinition>)serialiser.Deserialize(stream);
				Cartons.RemoveAll();
				foreach (var item in result)
				{
					Cartons.Add(new DummyCartonDefinition(item));
				}
			}
		}

		#endregion

		#region SaveItemsToPackToXml

		public void SaveItemsToPackToXml(string fileName)
		{
			var serialiser = new XmlSerializer(typeof(List<SerialisableItemToPack>));
			using (var stream = ZSaveFileDialog.OpenFile(fileName))
			{
				serialiser.Serialize(stream, ItemsToPack.Cast<DummyCartonisableItem>().Select(o => new SerialisableItemToPack(o)).ToList());
			}
		}

		#endregion

		#region LoadItemsToPackFromXml

		public void LoadItemsToPackFromXml(string fileName)
		{
			var serialiser = new XmlSerializer(typeof(List<SerialisableItemToPack>));
			using (var stream = ZOpenFileDialog.OpenFile(fileName))
			{
				var result = (List<SerialisableItemToPack>)serialiser.Deserialize(stream);
				ItemsToPack.RemoveAll();
				foreach (var item in result)
				{
					ItemsToPack.Add(new DummyCartonisableItem(item));
				}
			}
		}

		#endregion

		public DiagnosticCartonisationResult DiagnosticCartonisationResult { get; private set; }
	}
}

