using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ComplianceDocumentImageCollection : RegistryBusinessObjectCollection
	{
		public new ComplianceDocumentImage this[int i]
		{
			get { return (ComplianceDocumentImage)Elements[i]; }
		}

		public new ComplianceDocumentImage AddNew()
		{
			return (ComplianceDocumentImage)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ComplianceDocumentImage();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ComplianceDocumentImageCollection();
		}

		#region Clean Up

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			base.RemoveAndDelete(elementToDelete);
			DeletedElements.Add((ComplianceDocumentImage)elementToDelete);
		}

		internal void CleanUpDeletedElements()
		{
			if (deletedElements != null)
			{
				foreach (ComplianceDocumentImage element in deletedElements)
				{
					element.DeleteImage();
				}
			}
		}

		List<ComplianceDocumentImage> DeletedElements
		{
			get
			{
				if (deletedElements == null)
				{
					deletedElements = new List<ComplianceDocumentImage>();
				}
				return deletedElements;
			}
		}

		List<ComplianceDocumentImage> deletedElements;

		#endregion
	}
}
