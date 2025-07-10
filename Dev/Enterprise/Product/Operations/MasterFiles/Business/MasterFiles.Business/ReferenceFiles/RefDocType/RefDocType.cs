using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.Shared.Dash.Common.Services;

namespace Enterprise.MasterFiles.Business
{
	[CodeProperty(AutoRefDocType.Schema.RT_DocType), DescriptionProperty(AutoRefDocType.Schema.RT_Desc)]
	public class RefDocType : AutoRefDocType, ITemplateCopyable, IDocManagerSupport, IDocumentSupportable, ICanBeSavedByDocumentFactory, IRefDocType
	{
		#region Schema

		new class Schema : AutoRefDocType.Schema
		{
			public const string RT_OverrideVersions = "RT_OverrideVersions";
		}

		#endregion

		public RefDocType(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool AppliesToAllCategories
		{
			get { return (RT_ReferenceType == "ALL"); }
		}

		public RefDocType[] GetDuplicatedDocTypes()
		{
			ZQuery query = new ZQuery(RefDocTypeSchema.RT_DocType, RT_DocType);
			query.AddToFilter(RefDocTypeSchema.RT_ReferenceType, SQLComparisonOperator.NotEqual, RT_ReferenceType);
			return Factory.Load<RefDocType>(query);
		}

		public void FixDuplicatedDocTypes(RefDocType[] duplicatedDocTypes)
		{
			foreach (RefDocType docType in duplicatedDocTypes)
			{
				StmMenuTemplatePivot[] pivots = Factory.Load<StmMenuTemplatePivot>(new ZQuery(StmMenuTemplatePivotSchema.SI_RT_DocType, docType.PK));
				StmMenuEDocs[] edocs = Factory.Load<StmMenuEDocs>(new ZQuery(StmMenuEDocsSchema.SX_RT_DocType, docType.PK));
				foreach (StmMenuTemplatePivot pivot in pivots)
				{
					pivot.SI_RT_DocType = PK;
				}
				foreach (StmMenuEDocs edoc in edocs)
				{
					if (!ViolatesStmMenuEDocsUniqueIndex(edoc))
					{
						edoc.SX_RT_DocType = PK;
					}
					else
					{
						edoc.Delete();
					}
				}
				docType.Delete();
			}
		}

		bool ViolatesStmMenuEDocsUniqueIndex(StmMenuEDocs menuEDoc)
		{
			ZQuery query = new ZQuery(StmMenuEDocsSchema.SX_RT_DocType, PK);
			query.AddToFilter(JoinCondition.And, StmMenuEDocsSchema.SX_SU, menuEDoc.SX_SU);
			return Factory.Load<StmMenuEDocs>(query).Length > 0;
		}

		internal LogMacroExecutor LogMacroExecutor => new LogMacroExecutor(RT_LogMacro);

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "I do not think the design would be improved by introducing a new type")]
		public Tuple<string, IEnumerable<ErrorMessage>> EvaluateLogMacro(object data)
		{
			return LogMacroExecutor.Execute(data);
		}

		#region Business Object Overrides

		public override void OnLoaded()
		{
			base.OnLoaded();
			OldIsPublished = RT_IsPublished;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			RT_IsSystem = false;
			RT_IsPublished = false;
			RT_IsPublishUpdatable = true;
		}

		#endregion

		#region Lookups

		public CodeDescriptionPairList RT_ReferenceType_List
		{
			get
			{
				if (fRT_ReferenceType_List == null)
				{
					fRT_ReferenceType_List = new CodeDescriptionPairList(OLookUpEditType.ReferenceTypes);
				}

				return fRT_ReferenceType_List;
			}
		}
		CodeDescriptionPairList fRT_ReferenceType_List;

		public CodeDescriptionPairList RT_ParseType_List
		{
			get
			{
				if (fRT_ParseType_List == null || fRT_ParseType_List.Count == 0)
				{
					fRT_ParseType_List = new CodeDescriptionPairList();
					var codeDescriptionPairs = ObjectFactory.Get<IDashParametersService>().SupportedParseTypes.Select(x => new CodeDescriptionPair(x.TypeCode, x.Description)).ToArray();
					fRT_ParseType_List.AddRange(codeDescriptionPairs);
				}

				return fRT_ParseType_List;
			}
		}
		CodeDescriptionPairList fRT_ParseType_List;

		#endregion

		#region Properties

		#region RT_SE_NKDocumentReceivedEvent
		[List("Lookups.DocumentReceivedEvents")]
		[ActionField(CollectionType = typeof(StmEventCodeDescriptionPairList), FieldType = ActionFieldType.Code)]
		public override ZString RT_SE_NKDocumentReceivedEvent
		{
			get
			{
				return base.RT_SE_NKDocumentReceivedEvent;
			}
			set
			{
				base.RT_SE_NKDocumentReceivedEvent = value;
			}
		}
		#endregion

		public bool RT_DocType_ReadOnly
		{
			get { return RT_IsSystem && !IsCheckedOutByMe; }
		}

		public bool RT_IsSystem_ReadOnly
		{
			get { return !IsCheckedOutByMe; }
		}

		public bool RT_IsActive_ReadOnly
		{
			get { return !IsCheckedOutByMe && RT_DocType == RefDocTypeLookups.Codes.ElectronicHouseBill; }
		}

		public bool RT_IsPublished_ReadOnly
		{
			get { return !IsCheckedOutByMe && RT_DocType == RefDocTypeLookups.Codes.ElectronicHouseBill; }
		}

		public bool RT_IsPublishUpdatable_ReadOnly
		{
			get { return !IsCheckedOutByMe && RT_DocType == RefDocTypeLookups.Codes.ElectronicHouseBill; }
		}

		public bool RT_AllowMultiplePeriodicDocs_ReadOnly
		{
			get { return !IsCheckedOutByMe && RT_DocType == RefDocTypeLookups.Codes.ElectronicHouseBill; }
		}

		public bool RT_IsCompanySpecific_ReadOnly
		{
			get { return !IsCheckedOutByMe && RT_DocType == RefDocTypeLookups.Codes.ElectronicHouseBill; }
		}

		public bool RT_IsBranchSpecific_ReadOnly
		{
			get { return !IsCheckedOutByMe && RT_DocType == RefDocTypeLookups.Codes.ElectronicHouseBill; }
		}

		public bool RT_IsDepartmentSpecific_ReadOnly
		{
			get { return !IsCheckedOutByMe && RT_DocType == RefDocTypeLookups.Codes.ElectronicHouseBill; }
		}

		[TranslatableDataField(Schema.TableName, Schema.RT_Desc, DataXmlFilePaths.Documents, Type = typeof(RefDocType), SecurityCheckpoint = "DocumentTypesModify", Asmid = ResString.AssemblyId)]
		public override ZString RT_Desc
		{
			get { return base.RT_Desc; }
			set { base.RT_Desc = value; }
		}

		public bool RT_Desc_ReadOnly
		{
			get { return RT_IsSystem && !IsCheckedOutByMe; }
		}

		public MultilingualString RT_DescMultilingual
		{
			get { return GetMultilingual(RT_DescInfo); }
		}

		[List("RT_ReferenceType_List")]
		public override ZString RT_ReferenceType
		{
			get { return base.RT_ReferenceType; }
			set { base.RT_ReferenceType = value; }
		}

		[List("RT_ParseType_List")]
		public override ZString RT_ParseType
		{
			get { return base.RT_ParseType; }
			set { base.RT_ParseType = value; }
		}

		public bool RT_ReferenceType_ReadOnly
		{
			get { return RT_IsSystem && !IsCheckedOutByMe; }
		}

		public ZBool RT_OverrideVersions
		{
			get { return !base.RT_SaveVersions; }
			set { base.RT_SaveVersions = !value; }
		}

		public ZPropertyInfo RT_OverrideVersionsInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.RT_OverrideVersions, x => RT_SaveVersionsInfo); }
		}

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region IsArchived

		/// <summary>
		/// This property is used for eDoc archiving, to determine whether or not this Document Type
		/// should be archived.
		/// </summary>
		public ZBool IsArchived
		{
			get { return fIsArchived; }
			set { SetNonPersistentPropertyValue(IsArchivedInfo, ref fIsArchived, value); }
		}

		ZBool fIsArchived;

		public ZPropertyInfo IsArchivedInfo
		{
			get { return GetZPropertyInfo(nameof(IsArchived)); }
		}

		#endregion

		public bool IsCheckedOutByMe
		{
			get { return isCheckedOutByMe ?? (isCheckedOutByMe = GetIsCheckedOutByMe()).Value; }
		}

		bool GetIsCheckedOutByMe()
		{
			bool result = false;
#if DEBUG
			var type = Type.GetType("Enterprise.DocumentEngine.Build.DocumentsSetupController, Enterprise.DocumentEngine");
			var obj = Activator.CreateInstance(type);
			result = (bool)type.GetProperty("IsCheckedOutByMe").GetValue(obj, null);
			//result = new Enterprise.Builder.DataUpgradeSetup.DocumentsSetupController().IsCheckedOutByMe;
#endif
			return result;
		}

		bool? isCheckedOutByMe;

#if DEBUG

		internal void SetIsCheckedOutByMeForTesting(bool value)
		{
			isCheckedOutByMe = value;
		}

#endif

		#endregion

		#region ForceToReadReferenceTypes

		const int ForceToReadReferenceTypesTimeoutInMinutes = 30;
		[ThreadStatic]
		static List<ZString> fForceToReadReferenceTypes;
		[ThreadStatic]
		static ZDateTime fForceToReadReferenceTypesLastRead;

		static readonly object forceToReadReferenceTypesMutex = new object();

		public static ZString[] ForceToReadReferenceTypes
		{
			get
			{
				lock (forceToReadReferenceTypesMutex)
				{
					if (fForceToReadReferenceTypes == null || (ZDateTime.Now - fForceToReadReferenceTypesLastRead) > new TimeSpan(0, ForceToReadReferenceTypesTimeoutInMinutes, 0))
					{
						fForceToReadReferenceTypes = ReadReferenceTypesWithForceToReadDocTypes();
						fForceToReadReferenceTypesLastRead = ZDateTime.Now;
					}
					return fForceToReadReferenceTypes.ToArray();
				}
			}
		}

		static List<ZString> ReadReferenceTypesWithForceToReadDocTypes()
		{
			List<ZString> result = new List<ZString>();
			using (var reader = Db.Connection.Command("SELECT DISTINCT RT_ReferenceType FROM dbo.RefDocType WHERE RT_ForceUserToRead = 1").ExecuteReader())
			{
				while (reader.Read())
				{
					result.Add(reader[0].ToString());
				}
			}
			return result;
		}

		public override void Delete()
		{
			base.Delete();
			Factory.Saved += (BusinessObjectFactory factory, bool savedSuccessfully) =>
			{
				fForceToReadReferenceTypes = null;
			};
		}

		#endregion

		#region ITemplateCopyable Members

		public IBusiness TemplateCopy()
		{
			RefDocType docType = (RefDocType)base.Clone();
			docType.RT_DocType = ZString.Empty;
			docType.RT_Desc = ZString.Empty;
			docType.RT_IsSystem = false;
			return docType;
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.DocumentType);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return new RefDocTypeDocumentSupporter(this); }
		}

		#endregion

		#region UpdatingEDocs

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded)
			{
				if (RT_IsPublished != OldIsPublished)
				{
					if (ConfirmUpdate != null)
					{
						var publishState = OldIsPublished ? Res.GetString("fc022328-03df-45d8-9f8d-ec7f66902798", "unpublished") : Res.GetString("8b39685c-eae1-4005-9310-d9d91c77f914", "published");
						var message = Res.GetString("18333a21-e448-4798-a987-3c87065e7231", "Do you want to change the status of all eDocs with this Document Type to {0}?", publishState);

						if (ConfirmUpdate(this, new ConfirmEventArgs(message)))
						{
							IDocumentFactoryProvider documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
							BusinessObjectFactory parentFactory = null;
							if (!(Factory is IDocumentFactory))
							{
								parentFactory = Factory;
							}
							IDocumentFactory documentFactory = documentFactoryProvider.GetFactory(parentFactory);
							documentFactory.UpdatePublishedFlagForAllEDocs(RT_ReferenceType, RT_DocType, RT_IsPublished);
						}
					}
					OldIsPublished = RT_IsPublished;
				}

				fForceToReadReferenceTypes = null;
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (RT_DocTypeInfo.HasChanges || RT_DescInfo.HasChanges)
			{
				if (ConfirmUpdate != null)
				{
					var message = Res.GetString("D409354F-B077-450B-8686-4FA7282543FD", "Changing the Document Type and the Description will also change the Document Type and Description for documents that have been previously allocated.");
					if (ConfirmUpdate(this, new ConfirmEventArgs(message)))
					{
						IDocumentFactoryProvider documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
						BusinessObjectFactory parentFactory = null;
						if (!(Factory is IDocumentFactory))
						{
							parentFactory = Factory;
						}

						IDocumentFactory documentFactory = documentFactoryProvider.GetFactory(parentFactory);
						documentFactory.UpdateDocTypeForAllEDocs(RT_ReferenceTypeInfo, RT_DocTypeInfo, RT_DescInfo);

						if (RT_DescInfo.HasChanges && ((ZString)RT_DescInfo.Value).Length > JobRequiredDocumentSchema.EQ_DocDescription.MaxLength)
						{
							var truncateMessage = Res.GetString("5DB1E252-4FE3-4E79-90ED-555133F2E7D1", "The Doc Description exceeds the maximum allowed characters and has been truncated. Truncated value: '{0}'.", ((ZString)RT_DescInfo.Value).Substring(0, JobRequiredDocumentSchema.EQ_DocDescription.MaxLength));
							if (!ConfirmUpdate(this, new ConfirmEventArgs(truncateMessage)))
							{
								return;
							}
						}
						documentFactory.UpdateDocTypeForJobRequiredDocument(RT_ReferenceTypeInfo, RT_DocTypeInfo, RT_DescInfo);
					}
				}
			}
		}

		public event ConfirmEventHandler ConfirmUpdate;

		ZBool OldIsPublished;

		#endregion
	}

	#region Helper Classes

	public delegate bool ConfirmEventHandler(RefDocType sender, ConfirmEventArgs e);

	public class ConfirmEventArgs : EventArgs
	{
		public ConfirmEventArgs(ZString message)
		{
			this.Message = message;
		}

		public readonly ZString Message;
	}

	#endregion
}
