using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class DocumentTrackingBulkUpdateBusinessObject : JobRequiredDocument
	{
		public DocumentTrackingBulkUpdateBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public event EventHandler SaveSucceeded;

		#region Business Object Overrides

		public override void Delete()
		{
			base.Delete();
			ErrorReporter.ReportOnce("CannotDelete" + nameof(DocumentTrackingBulkUpdateBusinessObject), "Cannot delete a " + nameof(DocumentTrackingBulkUpdateBusinessObject));
		}

		public override bool IsSavedByFactory
		{
			get { return false; }
		}

		protected override void OnFactorySaving()
		{
			foreach (RequiredDocToBulkUpdate doc in this.SelectedDocuments)
			{
				doc.UpdateFrom(this);
			}
		}

		protected override void OnFactorySaved(bool didSaveSucceed)
		{
			base.OnFactorySaved(didSaveSucceed);
			if (didSaveSucceed && SaveSucceeded != null)
			{
				SaveSucceeded(this, EventArgs.Empty);
			}
		}

		#endregion

		#region Related Business Objects

		[ChildEditable(true)]
		public RequiredDocToBulkUpdateCollection SelectedDocuments
		{
			get
			{
				if (fSelectedDocuments == null)
				{
					fSelectedDocuments = new RequiredDocToBulkUpdateCollection(Factory, this);
					RegisterEditableChildObject(fSelectedDocuments);
				}
				return fSelectedDocuments;
			}
		}
		RequiredDocToBulkUpdateCollection fSelectedDocuments;

		#endregion

		#region Property Overrides

		protected override bool EQ_SntToCustomsBroker_ReadOnly
		{
			get { return !Env.Security.ForwardingDocumentTrackingEditDateSentToBrokerBulkUpdate.IsAllowed; }
		}

		protected override bool EQ_RcvFromCustomsBroker_ReadOnly
		{
			get { return !Env.Security.ForwardingDocumentTrackingEditDateReceivedFromBrokerBulkUpdate.IsAllowed; }
		}

		protected override bool EQ_ReturnToShipper_ReadOnly
		{
			get { return !Env.Security.ForwardingDocumentTrackingEditDateReturnToShipperBulkUpdate.IsAllowed; }
		}

		#endregion

		#region Overridden Validation

		protected override JobRequiredDocumentValidation GetNewValidation()
		{
			return new DocumentTrackingBulkUpdateValidation(this);
		}

		public class DocumentTrackingBulkUpdateValidation : JobRequiredDocumentValidation
		{
			public DocumentTrackingBulkUpdateValidation(AutoJobRequiredDocument parent)
				: base(parent)
			{
			}

			protected override void CheckEQ_DocType()
			{
			}

			protected override void CheckEQ_DocNumber()
			{
			}

			protected override void CheckEQ_DocCategory()
			{
			}

			protected override void CheckEQ_DocUsage()
			{
			}
		}

		#endregion
	}
}
