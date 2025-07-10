using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public abstract class DeclarationDocumentData<T> : Customs.Business.MultiLineAddInfos.CusAddInfoWithAutoDelete<T>, IDocumentDeliveredLogSupporter, IParentDocManagerSupport
		where T : Customs.Business.BaseAddInfo
	{
		protected DeclarationDocumentData(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region IDocumentDeliveredLogSupporter Members

		Type IDocumentDeliveredLogSupporter.BusinessObjectTypeToLogAgainst
		{
			get { return typeof(JobDeclaration); }
		}

		ZGuid IDocumentDeliveredLogSupporter.Identifier
		{
			get { return B7_ParentID; }
		}

		#endregion

		#region IParentDocManagerSupport Members

		ZGuid IParentDocManagerSupport.ParentGuid
		{
			get { return B7_ParentID; }
		}

		ZString IParentDocManagerSupport.ParentTableName
		{
			get
			{
				CargoWise.Schema.ITableSchema tableSchema = EnterpriseSchema.GetTableSchemaFromColumnNamePrefix(B7_ParentTableCode);
				return tableSchema != null ? tableSchema.TableName : "";
			}
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.JobDeclaration)); }
		}
		DocManagerInfo docManagerInfo;

		#endregion
	}
}
