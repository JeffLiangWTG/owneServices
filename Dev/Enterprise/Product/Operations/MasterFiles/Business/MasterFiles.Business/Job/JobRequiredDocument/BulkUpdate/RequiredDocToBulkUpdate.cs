using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class RequiredDocToBulkUpdate : NonPersistentBusinessObject, IObsoleteValidation
	{
		public RequiredDocToBulkUpdate(BusinessObjectFactory factory, DocumentTrackingBulkUpdateBusinessObject parent)
			: base(factory)
		{
			this.Parent = parent;
		}

		public readonly DocumentTrackingBulkUpdateBusinessObject Parent;

		public void UpdateFrom(DocumentTrackingBulkUpdateBusinessObject documentToUpdateFrom)
		{
			((IBusinessObjectInternals)this).IsCopying = true;
			try
			{
				foreach (string propertyName in PropertiesToUpdate)
				{
					IZType value = (IZType)documentToUpdateFrom[propertyName];
					if (value.IsValid && !value.IsEmpty)
					{
						Document[propertyName] = value;
					}
				}
			}
			finally
			{
				((IBusinessObjectInternals)this).IsCopying = false;
			}
		}

		public void SetDocument(JobRequiredDocument document)
		{
			this.DocumentPK = document.PK;
			this.DocumentNumber = document.EQ_DocNumber;
			this.DocumentParentID = document.EQ_Calc_ParentUniqueConsignRef;
			this.DocumentType = document.EQ_DocType;
			this.DateReceived = document.EQ_DateReceived;
			this.DateSentToBroker = document.EQ_SntToCustomsBroker;
			this.DateReceivedFromBroker = document.EQ_RcvFromCustomsBroker;
			this.DateReturnedToShipper = document.EQ_ReturnToShipper;
			this.DocumentOwner = document.DocumentOwner != null ? document.DocumentOwner.OH_Code : ZString.Empty;
		}

		#region New Bound Properties

		ZGuid documentPK;

		[ReadOnly(true)]
		public ZGuid DocumentPK
		{
			get { return documentPK; }
			set { SetNonPersistentPropertyValue(DocumentPKInfo, ref documentPK, value); }
		}

		public ZPropertyInfo DocumentPKInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(DocumentPK));
			}
		}

		ZString documentNumber;

		[ReadOnly(true)]
		[MaxLength(JobRequiredDocument.Schema.EQ_DocNumberMaxLength)]
		public ZString DocumentNumber
		{
			get { return documentNumber; }
			set
			{
				CheckMaximumLength(DocumentNumberInfo, value);
				SetNonPersistentPropertyValue(DocumentNumberInfo, ref documentNumber, value);
			}
		}

		public ZPropertyInfo DocumentNumberInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(DocumentNumber));
			}
		}

		ZString documentParentID;

		[ReadOnly(true)]
		[MaxLength(255)]
		public ZString DocumentParentID
		{
			get { return documentParentID; }
			set
			{
				CheckMaximumLength(DocumentParentIDInfo, value);
				SetNonPersistentPropertyValue(DocumentParentIDInfo, ref documentParentID, value);
			}
		}

		public ZPropertyInfo DocumentParentIDInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(DocumentParentID));
			}
		}

		ZString documentType;

		[ReadOnly(true)]
		[MaxLength(JobRequiredDocument.Schema.EQ_DocTypeMaxLength)]
		public ZString DocumentType
		{
			get { return documentType; }
			set
			{
				CheckMaximumLength(DocumentTypeInfo, value);
				SetNonPersistentPropertyValue(DocumentTypeInfo, ref documentType, value);
			}
		}

		public ZPropertyInfo DocumentTypeInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(DocumentType));
			}
		}

		ZDateTimeOffset dateReceived;

		[ReadOnly(true)]
		public ZDateTimeOffset DateReceived
		{
			get { return dateReceived; }
			set { SetNonPersistentPropertyValue(DateReceivedInfo, ref dateReceived, value); }
		}

		public ZPropertyInfo DateReceivedInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(DateReceived));
			}
		}

		ZDateTime dateSentToBroker;

		[ReadOnly(true)]
		public ZDateTime DateSentToBroker
		{
			get { return dateSentToBroker; }
			set { SetNonPersistentPropertyValue(DateSentToBrokerInfo, ref dateSentToBroker, value); }
		}

		public ZPropertyInfo DateSentToBrokerInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(DateSentToBroker));
			}
		}

		ZDateTime dateReceivedFromBroker;

		[ReadOnly(true)]
		public ZDateTime DateReceivedFromBroker
		{
			get { return dateReceivedFromBroker; }
			set { SetNonPersistentPropertyValue(DateReceivedFromBrokerInfo, ref dateReceivedFromBroker, value); }
		}

		public ZPropertyInfo DateReceivedFromBrokerInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(DateReceivedFromBroker));
			}
		}

		ZDateTime dateReturnedToShipper;

		[ReadOnly(true)]
		public ZDateTime DateReturnedToShipper
		{
			get { return dateReturnedToShipper; }
			set { SetNonPersistentPropertyValue(DateReturnedToShipperInfo, ref dateReturnedToShipper, value); }
		}

		public ZPropertyInfo DateReturnedToShipperInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(DateReturnedToShipper));
			}
		}

		ZString documentOwner;

		[ReadOnly(true)]
		[MaxLength(OrgHeader.Schema.OH_CodeMaxLength)]
		public ZString DocumentOwner
		{
			get { return documentOwner; }
			set
			{
				CheckMaximumLength(DocumentOwnerInfo, value);
				SetNonPersistentPropertyValue(DocumentOwnerInfo, ref documentOwner, value);
			}
		}

		public ZPropertyInfo DocumentOwnerInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(DocumentOwner));
			}
		}

		#endregion

		#region Related Business Objects

		public JobRequiredDocument Document
		{
			get
			{
				JobRequiredDocument result = null;
				ZQuery filter = new ZQuery();
				if (DocumentPK.IsEmpty)
				{
					filter = ZQuery.NoResultQuery;
				}
				else
				{
					filter.AddToFilter(JobRequiredDocumentSchema.PK, DocumentPK);
				}

				JobRequiredDocument[] documents = (JobRequiredDocument[])Factory.Load(typeof(JobRequiredDocument), filter);

				if (documents.Length == 1)
				{
					result = documents[0];
				}

				if (result != null)
				{
					result.SetReadOnlyIncludingChildren(true);
				}

				return result;
			}
		}

		#endregion

		#region Implementation

		public static readonly string[] PropertiesToUpdate = new string[]
		{
			JobRequiredDocumentSchema.Constants.EQ_DateReceived,
			JobRequiredDocumentSchema.Constants.EQ_SntToCustomsBroker,
			JobRequiredDocumentSchema.Constants.EQ_RcvFromCustomsBroker,
			JobRequiredDocumentSchema.Constants.EQ_ReturnToShipper
		};

		#endregion
	}
}
