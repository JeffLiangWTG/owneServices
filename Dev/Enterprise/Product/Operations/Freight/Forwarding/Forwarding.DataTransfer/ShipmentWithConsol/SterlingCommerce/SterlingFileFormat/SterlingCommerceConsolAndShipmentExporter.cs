using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class SterlingCommerceConsolAndShipmentExporter : NonPersistentBusinessObject
	{
		public SterlingCommerceConsolAndShipmentExporter(BusinessObjectFactory factory, Xsd.Consol consol, Xsd.Shipment shipment, Xsd.InterchangeInfo interchange)
			: base(factory)
		{
			this.Shipment = shipment;
			this.Consol = consol;
			this.Interchange = interchange;
		}

		public readonly Xsd.Consol Consol;
		public readonly Xsd.Shipment Shipment;
		public readonly Xsd.InterchangeInfo Interchange;

		#region properties

		#region Header Info

		public SterlingHeader HeaderInfo
		{
			get
			{
				if (fHeaderInfo == null)
				{
					fHeaderInfo = new SterlingHeader(this);
				}
				return fHeaderInfo;
			}
		}
		SterlingHeader fHeaderInfo;

		#endregion

		#region Shipment Info

		public SterlingShipment ShipmentInfo
		{
			get
			{
				if (fShipmentInfo == null)
				{
					fShipmentInfo = new SterlingShipment(this);
				}
				return fShipmentInfo;
			}
		}
		SterlingShipment fShipmentInfo;

		#endregion

		#region Name Info

		public SterlingNameCollection NameInfo
		{
			get { return nameInfo ?? (nameInfo = new SterlingNameCollection(this)); }
		}

		SterlingNameCollection nameInfo;

		#endregion

		#region Note Info

		public SterlingNoteCollection NoteInfo
		{
			get
			{
				if (fNoteInfo == null)
				{
					fNoteInfo = new SterlingNoteCollection(this);
				}
				return fNoteInfo;
			}
		}
		SterlingNoteCollection fNoteInfo;

		#endregion

		#region Order Reference Info

		public SterlingOrderReferenceCollection OrderReferenceInfo
		{
			get
			{
				if (fOrderReferenceInfo == null)
				{
					fOrderReferenceInfo = new SterlingOrderReferenceCollection(this);
				}
				return fOrderReferenceInfo;
			}
		}

		SterlingOrderReferenceCollection fOrderReferenceInfo;

		#endregion

		#region Event Info

		public SterlingEventCollection EventInfo
		{
			get
			{
				if (fEventInfo == null)
				{
					fEventInfo = new SterlingEventCollection(this);
				}
				return fEventInfo;
			}
		}
		SterlingEventCollection fEventInfo;

		#endregion

		#region Custom Info

		public SterlingCustom CustomInfo
		{
			get
			{
				if (fCustomInfo == null)
				{
					fCustomInfo = new SterlingCustom(this);
				}
				return fCustomInfo;
			}
		}
		SterlingCustom fCustomInfo;

		#endregion

		#region Routing Info

		public SterlingRoutingCollection RoutingInfo
		{
			get
			{
				if (fRoutingInfo == null)
				{
					fRoutingInfo = new SterlingRoutingCollection(this);
				}
				return fRoutingInfo;
			}
		}
		SterlingRoutingCollection fRoutingInfo;

		#endregion

		#region Invoice Info

		public SterlingInvoiceCollection InvoiceInfo
		{
			get
			{
				if (fInvoiceInfo == null)
				{
					fInvoiceInfo = new SterlingInvoiceCollection(this);
				}
				return fInvoiceInfo;
			}
		}
		SterlingInvoiceCollection fInvoiceInfo;

		#endregion

		#region POD

		public SterlingPODCollection PODInfo
		{
			get
			{
				if (fPODInfo == null)
				{
					fPODInfo = new SterlingPODCollection(this);
				}
				return fPODInfo;
			}
		}
		SterlingPODCollection fPODInfo;

		#endregion

		#region Packs

		public SterlingPackageCollection PackageInfo
		{
			get
			{
				if (fPackageInfo == null)
				{
					fPackageInfo = new SterlingPackageCollection(this);
				}
				return fPackageInfo;
			}
		}
		SterlingPackageCollection fPackageInfo;

		#endregion

		#endregion

		#region GenerateFile

		#region Export

		public virtual void Export(Stream toFile)
		{
			Write(toFile, HeaderInfo.ToUTF8());
			Write(toFile, ShipmentInfo.ToUTF8());
			foreach (SterlingName name in this.NameInfo)
			{
				Write(toFile, name.ToUTF8());
			}
			foreach (SterlingNote note in this.NoteInfo)
			{
				Write(toFile, note.ToUTF8());
			}
			foreach (SterlingOrderReference reference in OrderReferenceInfo)
			{
				Write(toFile, reference.ToUTF8());
			}
			foreach (SterlingEvent @event in EventInfo)
			{
				Write(toFile, @event.ToUTF8());
			}
			Write(toFile, CustomInfo.ToUTF8());
			foreach (SterlingRouting routing in RoutingInfo)
			{
				Write(toFile, routing.ToUTF8());
			}
			foreach (SterlingInvoice invoice in InvoiceInfo)
			{
				Write(toFile, invoice.ToUTF8());
				foreach (SterlingInvoiceItem iItem in invoice.InvoiceItemInfo)
				{
					Write(toFile, iItem.ToUTF8());
				}
			}
			foreach (SterlingPOD pOD in PODInfo)
			{
				Write(toFile, pOD.ToUTF8());
			}
			foreach (SterlingPackage pack in PackageInfo)
			{
				Write(toFile, pack.ToUTF8());
			}
			toFile.Position = 0;
		}

		void Write(Stream toFile, byte[] info)
		{
			toFile.Write(info, 0, info.Length);
		}

		#endregion

		#region Dialog

		protected void ExportFile(string fileName)
		{
			string tempFileName = fileName + ".dt.tmp";
			NotificationBuffer notify = new NotificationBuffer();
			try
			{
				using (Stream toFile = File.Create(tempFileName))
				{
					Export(toFile);
				}
				if (!notify.HasErrors)
				{
					MoveOrOverwriteFile(tempFileName, fileName);
				}
				else
				{
					DeleteFileIfExists(fileName);
				}
			}
			catch (IOException ex)
			{
				notify.Notify(new ErrorNotification(ErrorType.Error, ex.Message));
			}
			finally
			{
				DeleteFileIfExists(tempFileName);
			}

			if (!string.IsNullOrEmpty(notify.AsString))
			{
				Globals.Message.ShowInformation(notify.AsString);
			}
		}

		#region DialogImplementation

		void MoveOrOverwriteFile(string source, string target)
		{
			DeleteFileIfExists(target);
			File.Move(source, target);
		}

		void DeleteFileIfExists(string fileName)
		{
			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}
		}

		public ZString DefaultFileName
		{
			get { return defaultFileName; }
			set { defaultFileName = value; }
		}
		ZString defaultFileName = "";

		public ZString InitialDirectory
		{
			get { return initialDirectory; }
			set { initialDirectory = value; }
		}
		ZString initialDirectory = "";

		#endregion

		#endregion

		#endregion
	}
}
