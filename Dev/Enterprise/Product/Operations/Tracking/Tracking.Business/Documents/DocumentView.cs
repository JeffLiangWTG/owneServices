using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Cache;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business
{
	public class DocumentView : NonPersistentBusinessObject, IBusinessObjectInternals, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string PK = "PK";
			public const string StorageDocPK = "StorageDocPK";
			public const string DocType = "DocType";
			public const string Description = "Description";
			public const string RT_Desc = "RT_Desc";
			public const string DateReceived = "DateReceived";
			public const string ParentPK = "ParentPK";
			public const string ReqDoc = "ReqDoc";
			public const string StorageDoc = "StorageDoc";
			public const string IsDocumentReceived = "IsDocumentReceived";
			public const string DocumentNotes = "DocumentNotes";
			public const string Dummy = "Dummy";
			public const string IsValidated = "IsValidated";
		}

		#endregion

		#region Constructor

		public DocumentView(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocumentView(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region Properties

		public override SchemaGuidColumn PKSchemaColumn
		{
			get
			{
				SchemaGuidColumn result = base.PKSchemaColumn;
				DataRow row = ((IBusinessObjectInternals)this).Row;
				if (row != null)
				{
					foreach (DataColumn column in row.Table.Columns)
					{
						if (column.ColumnName.Length == 5 && column.ColumnName.EndsWith("_PK"))
						{
							result = new SchemaGuidColumn(result.TableSchema, column.ColumnName, result.Ordinal, ZGuid.NewZGuid(), result.IsNullable);
						}
					}
				}
				return result;
			}
		}

		#region Wrapped business objects

		public JobRequiredDocument ReqDoc
		{
			get
			{
				return fReqDoc;
			}
			set
			{
				fReqDoc = value;
				fDocType = fReqDoc.EQ_DocType;
				fDescription = fReqDoc.EQ_DocDescriptionMultilingual;
				fDateReceived = fReqDoc.EQ_DateReceived;
				DocTypeInfo.RefreshBinding();
				DescriptionInfo.RefreshBinding();
				DateReceivedInfo.RefreshBinding();
			}
		}
		JobRequiredDocument fReqDoc;

		public StorageDocsBase StorageDoc
		{
			get
			{
				return fStorageDoc;
			}
			set
			{
				fStorageDoc = value;
				fStorageDocPK = fStorageDoc.PK;
				fDocType = fStorageDoc.SC_DocType;
				fDescription = fStorageDoc.SC_DescMultilingual;
				fDateReceived = fStorageDoc.SC_Date.UtcToDateTimeOffset();
				StorageDocPKInfo.RefreshBinding();
				DocTypeInfo.RefreshBinding();
				DescriptionInfo.RefreshBinding();
				DateReceivedInfo.RefreshBinding();
			}
		}
		StorageDocsBase fStorageDoc;

		#endregion

		#region StorageDocPK

		public ZGuid StorageDocPK
		{
			get
			{
				if (StorageDoc != null)
				{
					return StorageDoc.PK;
				}

				return fStorageDocPK;
			}
			set
			{
				fStorageDocPK = value;
				StorageDocPKInfo.RefreshBinding();
			}
		}
		ZGuid fStorageDocPK;

		public ZPropertyInfo StorageDocPKInfo
		{
			get { return GetZPropertyInfo(Schema.StorageDocPK); }
		}

		#endregion

		#region DocType

		[MaxLength(AutoStorageDocs.Schema.SC_DocTypeMaxLength)]
		public virtual ZString DocType
		{
			get
			{
				if (ReqDoc != null)
				{
					return ReqDoc.EQ_DocType;
				}
				if (StorageDoc != null)
				{
					return StorageDoc.SC_DocType;
				}
				return fDocType;
			}
			set
			{
				CheckMaximumLength(DocTypeInfo, value);

				if (ReqDoc != null)
				{
					ReqDoc.EQ_DocType = value;
				}
				if (StorageDoc != null)
				{
					StorageDoc.SC_DocType = value;
				}

				fDocType = value;
				fRT_Desc = (NoResString)ZString.Empty;

				DocTypeInfo.RefreshBinding();
			}
		}
		ZString fDocType;

		public ZPropertyInfo DocTypeInfo
		{
			get { return GetZPropertyInfo(Schema.DocType); }
		}

		#endregion

		#region Description

		[BusinessObjectMaxLengthTestExclude]
		[MaxLength(AutoStorageDocs.Schema.SC_DescMaxLength)]
		public virtual MultilingualString Description
		{
			get
			{
				if (ReqDoc != null)
				{
					return ReqDoc.EQ_DocDescriptionMultilingual;
				}
				var attribute = (TranslatableDataFieldAttribute)Attribute.GetCustomAttribute(typeof(RefDocType).GetProperty(AutoRefDocType.Schema.RT_Desc), typeof(TranslatableDataFieldAttribute));
				return CustomizableDataResourceStrings.GetMultilingualString(attribute, null, fDescription);
			}
			set
			{
				if (value == null)
				{
					value = (NoResString)"";
				}
				if (fDescription != value.GetUnresolvedString())
				{
					ZString desc = value.GetUnresolvedString();
					if (ReqDoc != null)
					{
						ReqDoc.EQ_DocDescription = desc.Length > AutoJobRequiredDocument.Schema.EQ_DocDescriptionMaxLength ?
							desc.Substring(0, AutoJobRequiredDocument.Schema.EQ_DocDescriptionMaxLength) : desc;
					}
					fDescription = desc.Length > DescriptionInfo.MaxLength ? desc.Left(DescriptionInfo.MaxLength) : desc;
					DescriptionInfo.RefreshBinding();
				}
			}
		}
		ZString fDescription;

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Description); }
		}

		#endregion

		#region RT_Desc

		[MaxLength(AutoRefDocType.Schema.RT_DescMaxLength)]
		public MultilingualString RT_Desc
		{
			get
			{
				if (fRT_Desc.IsEmpty)
				{
					ZQuery filter = new ZQuery(RefDocTypeSchema.RT_DocType, DocType);
					RefDocType docType = Factory.LoadTop1<RefDocType>(filter);
					if (docType != null)
					{
						fRT_Desc = docType.RT_DescMultilingual;
					}
				}
				return fRT_Desc;
			}
		}
		MultilingualString fRT_Desc = (NoResString)"";

		public ZPropertyInfo RT_DescInfo
		{
			get { return GetZPropertyInfo(Schema.RT_Desc); }
		}

		#endregion

		#region DataReceived

		public virtual ZDateTimeOffset DateReceived
		{
			get
			{
				if (ReqDoc != null)
				{
					return ReqDoc.EQ_DateReceived;
				}
				return fDateReceived;
			}
			set
			{
				if (ReqDoc != null)
				{
					ReqDoc.EQ_DateReceived = value;
				}
				fDateReceived = value;
				DateReceivedInfo.RefreshBinding();
			}
		}
		ZDateTimeOffset fDateReceived;

		public ZPropertyInfo DateReceivedInfo
		{
			get { return GetZPropertyInfo(Schema.DateReceived); }
		}

		#endregion

		#region ParentPK

		public ZGuid ParentPK
		{
			get { return fParentPK; }
			set
			{
				fParentPK = value;
				ParentPKInfo.RefreshBinding();
			}
		}
		ZGuid fParentPK;

		public ZPropertyInfo ParentPKInfo
		{
			get { return GetZPropertyInfo(Schema.ParentPK); }
		}

		#endregion

		#region HasImage

		public bool HasImage
		{
			get { return !fStorageDocPK.IsEmpty; }
		}

		#endregion

		#region IsDocumentReceived

		public ZBool IsDocumentReceived
		{
			get
			{
				if (ReqDoc != null)
				{
					return ReqDoc.EQ_DateReceived.IsValid;
				}
				if (StorageDoc != null)
				{
					return DateReceived.IsValid;
				}
				return fIsDocumentReceived;
			}
			set
			{
				ZDateTimeOffset dateTimeValue = (value) ? ZDateTimeOffset.Now : ZDateTimeOffset.Empty;

				if (ReqDoc != null)
				{
					ReqDoc.EQ_DateReceived = dateTimeValue;
				}
				if (StorageDoc != null)
				{
					DateReceived = dateTimeValue;
				}
				fIsDocumentReceived = value;
				IsDocumentReceivedInfo.RefreshBinding();
			}
		}
		ZBool fIsDocumentReceived;

		public ZPropertyInfo IsDocumentReceivedInfo
		{
			get { return GetZPropertyInfo(Schema.IsDocumentReceived); }
		}

		#endregion

		#region DocumentNotes

		[MaxLength(AutoJobRequiredDocument.Schema.EQ_DocumentNotesMaxLength)]
		public ZString DocumentNotes
		{
			get
			{
				if (ReqDoc != null)
				{
					return ReqDoc.EQ_DocumentNotes;
				}
				return fDocumentNotes;
			}
			set
			{
				if (DocumentNotesInfo != null)
				{
					CheckMaximumLength(DocumentNotesInfo, value);

					if (ReqDoc != null)
					{
						ReqDoc.EQ_DocumentNotes = value;
					}
				}
				fDocumentNotes = value;
				DocumentNotesInfo.RefreshBinding();
			}
		}
		ZString fDocumentNotes;

		public ZPropertyInfo DocumentNotesInfo
		{
			get { return GetZPropertyInfo(Schema.DocumentNotes); }
		}

		#endregion

		#region Dummy

		public ZString Dummy
		{
			get { return ""; }
		}

		public ZPropertyInfo DummyInfo
		{
			get { return GetZPropertyInfo(Schema.Dummy); }
		}

		#endregion

		#region IsValidated

		public ZBool IsValidated
		{
			get
			{
				if (ReqDoc != null)
				{
					return ReqDoc.EQ_TemplateTransportMode == "Y";
				}

				return fIsValidated;
			}
			set
			{
				if (ReqDoc != null)
				{
					ReqDoc.EQ_TemplateTransportMode = value ? "Y" : "N";
				}
				fIsValidated = value;
				IsValidatedInfo.RefreshBinding();
			}
		}
		ZBool fIsValidated;

		public ZPropertyInfo IsValidatedInfo
		{
			get { return GetZPropertyInfo(Schema.IsValidated); }
		}

		#endregion

		#endregion
	}
}
