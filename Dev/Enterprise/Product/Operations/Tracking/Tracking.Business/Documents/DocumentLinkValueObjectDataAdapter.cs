using System;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Tracking.Business
{
	public class DocumentLinkValueObjectDataAdapter : ValueObjectDataAdapter<StorageDocsBase, Xsd.DocumentLink>
	{
		public override string RootCollectionElementName
		{
			get { return "DocumentLinks"; }
		}

		public override string RootElementName
		{
			get { return "DocumentLink"; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return XmlSchemaDefinitions.Instance.DocumentLinksSchema; }
		}

		public override XmlSchema Schema
		{
			get { return XmlSchemaDefinitions.Instance.SingleDocumentLinkSchema; }
		}

		protected override StorageDocsBase FindBusinessObject(Xsd.DocumentLink value, IValueObjectImportContext context)
		{
			return null;
		}

		protected override void NotifyBizObjCreatedOrUpdated(INotifications notifications, BusinessObject bizObj)
		{
		}

		#region ImportFromValueObjectCore

		protected override void ImportFromValueObjectCore(StorageDocsBase bizObj, Xsd.DocumentLink value, IValueObjectImportContext context)
		{
			throw new NotSupportedException("DocumentLink import is not supported");
		}

		#endregion

		#region ExportToValueObjectCore

		protected override void ExportToValueObjectCore(StorageDocsBase documentBizObj, Xsd.DocumentLink documentLinkValue, IValueObjectExportContext context)
		{
			if (!documentBizObj.SC_Date.IsEmpty)
			{
				documentLinkValue.Date = documentBizObj.SC_Date;
			}
			if (!documentBizObj.SC_DescriptionForWeb.IsEmpty)
			{
				documentLinkValue.Description = documentBizObj.SC_DescriptionForWeb;
			}
			else
			{
				documentLinkValue.Description = documentBizObj.SC_DescMultilingual;
			}

			if (documentBizObj.ParentMain != null)
			{
				OrgContact webUser = WebEnv.CurrentUser as OrgContact;
				documentLinkValue.Link = TrackingUrlCreator.Instance.CreateUrl(webUser != null ? webUser.PK : ZGuid.Empty,
					TrackingConstants.BusinessContext.eDoc,
#if DEBUG
 Globals.IsTest ? documentBizObj.ParentMain.SM_ParentFK :
#endif
 documentBizObj.PK,
					documentBizObj.ParentMain.SM_ParentFK);
			}
		}

		#endregion

		public Xsd.DocumentLinkCollection ExportToXmlValueObjectCollection(StorageDocsCollectionViewBase bizObjDocuments, IValueObjectExportContext context)
		{
			Xsd.DocumentLinkCollection documentLinks = new Xsd.DocumentLinkCollection();
			if (bizObjDocuments.Count > 0)
			{
				foreach (StorageDocsBase doc in bizObjDocuments)
				{
					if (doc.SC_IsPublished)
					{
						ExportToValueObject(doc, documentLinks.AddNew(), context);
					}
				}
			}

			return documentLinks;
		}

		public Xsd.DocumentLinkCollection ExportToXmlValueObjectCollection(DocumentViewCollection bizObjDocumentViews, IValueObjectExportContext context)
		{
			Xsd.DocumentLinkCollection documentLinks = new Xsd.DocumentLinkCollection();
			if (bizObjDocumentViews.Count > 0)
			{
				foreach (DocumentView docView in bizObjDocumentViews)
				{
					if (docView.StorageDoc != null && docView.StorageDoc.SC_IsPublished)
					{
						ExportToValueObject(docView.StorageDoc, documentLinks.AddNew(), context);
					}
				}
			}

			return documentLinks;
		}
	}
}
