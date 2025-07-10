using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Business
{
	public class VAT404Document : NonPersistentBusinessObject, IDocumentSupportable
	{
		#region ctor

		internal VAT404Document(ZGuid importer, IEnumerable<CusEntryPayInfo> cusEntryPayInfos, VAT404DocumentInstruction parent) : base(parent.Factory)
		{
			this.ImporterPK = importer;
			this.ProofOfPayments.AddRange(cusEntryPayInfos.OrderBy(x => ZString.Format("{0:-20}{1:yyyyMMdd}{2:-10}{3:-35}{4:yyyyMMdd}", x.C9_PaymentReference, x.C9_ReceiptDate, x.FANumber, x.LRNumber, x.C9_PaymentDate)));
			this.parent = parent;
		}

		readonly VAT404DocumentInstruction parent;

		#endregion

		#region Schema

		public static class Schema
		{
			public const string ImporterPK = "ImporterPK";
		}

		#endregion

		#region Override

		public override string ToString()
		{
			return Importer?.OH_Code ?? ZString.Empty;
		}

		#endregion

		#region Properties

		#region Collections

		[ChildEditable]
		public DocDeliveryContactCollection DeliveryContacts
		{
			get
			{
				if (deliveryContacts == null)
				{
					var docAutoDelivery = new DocAutoDelivery();
					deliveryContacts = docAutoDelivery.GetDeliveryContacts(VAT404ProofOfPaymentMenu, DocumentSupporter);
					RegisterEditableChildObject(deliveryContacts);
				}
				return deliveryContacts;
			}
		}
		DocDeliveryContactCollection deliveryContacts;

		public CusEntryPayInfoBulkUpdateCollection ProofOfPayments
		{
			get
			{
				if (proofOfPayments == null)
				{
					this.proofOfPayments = new CusEntryPayInfoBulkUpdateCollection(Factory);
				}
				return proofOfPayments;
			}
		}
		CusEntryPayInfoBulkUpdateCollection proofOfPayments;

		#endregion

		#region Importer

		[RelatedBusinessObject("Importer")]
		[ResourceStringData("ZA|VAT404Dpcument|ImporterPK", Caption = "Importer")]
		[List(nameof(Importers))]
		public ZGuid ImporterPK { get; private set; }

		public ZPropertyInfo ImporterPKInfo
		{
			get { return GetZPropertyInfo(Schema.ImporterPK); }
		}

		public OrgHeader Importer
		{
			get { return Factory.Load<OrgHeader>(ImporterPK); }
		}

		public OrgHeaderCollection Importers => new OrgHeaderCollection(Factory);

		#endregion

		#region PaymentPeriod

		[ResourceStringData("ZA|VAT404Dpcument|VATPaymentStartDate", Caption = "From")]
		public ZDateTime PaymentStartDate => parent.StartDate;

		[ResourceStringData("ZA|VAT404Dpcument|VATPaymentEndDate", Caption = "To")]
		public ZDateTime PaymentEndDate => parent.EndDate;

		#endregion

		internal DocumentCommand VAT404ProofOfPaymentMenu
		{
			get
			{
				if (vat404ProofOfPaymentMenu == null)
				{
					var filter = new DocumentZQuery(BusinessContext.ZAProofOfPayment, VAT404ProofOfPaymentMenuName);
					vat404ProofOfPaymentMenu = Factory.LoadTop1<DocumentCommand>(filter);
				}
				return vat404ProofOfPaymentMenu;
			}
		}
		DocumentCommand vat404ProofOfPaymentMenu;

		const string VAT404ProofOfPaymentMenuName = "VAT 404 Proof Of Payment";

		internal ZGuid PrinterPK => parent.PrinterPK;

		#endregion

		#region IDocumentSupportable

		public DocumentSupporter DocumentSupporter => documentSupporter ?? (documentSupporter = new VAT404DocumentSupporter(this));
		DocumentSupporter documentSupporter;

		#endregion

		#region Implementation

		public void DeliverDocument(INotifications notifications)
		{
			new ProofOfPaymentAutoDeliveryJob(this, VAT404ProofOfPaymentMenu.PK).Deliver(notifications);
		}

		public void PreviewDocument()
		{
			using (var task = new PrintTask())
			{
				var docPack = new DocumentPack(VAT404ProofOfPaymentMenu, this, null, null);
				task.Add(docPack);
				var deliveryInstruction = new DeliveryInstructions() { AllowModifyAndPreviewInExcel = true };
				task.Preview(deliveryInstruction);
			}
		}

		#endregion
	}
}
