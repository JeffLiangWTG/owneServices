using CargoWise.EntityFramework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class SterlingNoteCollection : NonPersistentBusinessObjectCollection<SterlingNote>
	{
		public SterlingNoteCollection(SterlingCommerceConsolAndShipmentExporter master)
		{
			this.Master = master;
			UpdateCollection();
		}
		readonly SterlingCommerceConsolAndShipmentExporter Master;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SterlingNote();
		}

		public void UpdateCollection()
		{
			foreach (Xsd.NotesNote noteToAdd in Master.Shipment.Notes)
			{
				if (this.Count < 1000)
				{
					SterlingNote note = this.AddNew();
					note.Source = noteToAdd;
				}
			}
		}
	}
}

