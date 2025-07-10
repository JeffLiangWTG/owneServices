using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public abstract class DeclarationDataPrint : NonPersistentBusinessObject, IObsoleteValidation, IDocumentDeliveredLogSupporter, IParentDocManagerSupport, IVisualizerNoteSupporter
	{
		protected DeclarationDataPrint(JobDeclaration declaration)
			: base(declaration.Factory)
		{
			this.declaration = declaration;
		}

		public JobDeclaration Declaration
		{
			get { return declaration; }
		}
		readonly JobDeclaration declaration;

		#region IDocumentDeliveredLogSupporter Members

		Type IDocumentDeliveredLogSupporter.BusinessObjectTypeToLogAgainst
		{
			get { return Declaration.GetType(); }
		}

		ZGuid IDocumentDeliveredLogSupporter.Identifier
		{
			get { return Declaration.PK; }
		}

		#endregion

		#region IParentDocManagerSupport Members

		ZGuid IParentDocManagerSupport.ParentGuid
		{
			get { return Declaration.PK; }
		}

		ZString IParentDocManagerSupport.ParentTableName
		{
			get { return JobDeclarationSchema.Constants.TableName; }
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.JobDeclaration)); }
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IVisualizerNoteSupporter members

		ZGuid IVisualizerNoteSupporter.PK => declaration.PK;

		ZGuid IVisualizerNoteSupporter.ChildBusinessObjectPK => ZGuid.Empty;

		string IVisualizerNoteSupporter.TableCode => JobDeclarationSchema.Constants.Prefix;

		#endregion
	}
}
