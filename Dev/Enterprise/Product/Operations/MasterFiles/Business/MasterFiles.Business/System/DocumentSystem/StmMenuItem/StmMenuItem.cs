using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	#region IPrintTaskRunner Interface

	public interface IPrintTaskRunner
	{
		void RunPrintTask(IStmMenuItem menuItem, IDocumentSupportable documentSupportable);
		void RunPrintTaskIncludingChildMenus(IStmMenuItem menuItem, BusinessObject businessObject);
	}

	#endregion

	[CodeProperty("DocumentIdMultilingual"), DescriptionProperty("DocumentIdMultilingual")]
	public class StmMenuItem : AutoStmMenuItem, IStmMenuItem
	{
		public StmMenuItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			AllowMultipleCopies = true;
			NumberOfCopies = 1;
		}

		public static StmMenuItem New(BusinessObjectFactory factory)
		{
			return factory.New<StmMenuItem>();
		}

		public DraftOptionsList SU_DraftOption_List
		{
			get { return new DraftOptionsList(); }
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new FetchStrategies.StmMenuItemFetchStrategy(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SU_AddressCategory = "OFF";
		}

		#region Properties

		#region SU_ContactType

		[List("ContactTypes")]
		public override ZString SU_ContactType
		{
			get { return base.SU_ContactType; }
			set { base.SU_ContactType = value; }
		}

		#endregion

		#region SU_PrimaryDocPackItemId

		public override ZGuid SU_PrimaryDocPackItemId
		{
			get { return base.SU_PrimaryDocPackItemId; }
			set
			{
				base.SU_PrimaryDocPackItemId = value;
				if (Factory.Load<StmMenuTemplatePivot>(value) != null)
				{
					SU_PrimaryDocPackItemTableCode = StmMenuTemplatePivotSchema.Constants.Prefix;
				}
				else if (Factory.Load<StmMenuMenuPivot>(value) != null)
				{
					SU_PrimaryDocPackItemTableCode = StmMenuMenuPivotSchema.Constants.Prefix;
				}
				else
				{
					SU_PrimaryDocPackItemTableCode = ZString.Empty;
				}
			}
		}

		#endregion

		#region MenuItemUniqueCode

		[MaxLength(130)]
		public ZString MenuItemUniqueCode
		{
			get
			{
				return ZString.Format(
					"{0} : {1} : {2} : {3}",
					SU_MenuName,
					SU_BusinessContext,
					SU_MenuPath,
					SU_ContactType);
			}
		}

		public ZPropertyInfo MenuItemUniqueCodeInfo
		{
			get { return GetZPropertyInfo(nameof(MenuItemUniqueCode)); }
		}

		#endregion

		#region AllowMultipleCopies

		public ZBool AllowMultipleCopies { get; set; }

		#endregion

		#region NumberOfCopies

		public ZShort NumberOfCopies { get; set; }

		#endregion

		#region Culture

		public CultureInfo RenderCulture
		{
			get
			{
				if (renderCulture == null)
				{
					renderCulture = SU_IsLocalDocument ? Culture.CurrentCompanyCountryCulture : Culture.Default;
				}
				return renderCulture;
			}
			set { renderCulture = value; }
		}
		CultureInfo renderCulture;

		public bool IsLocalDocument
		{
			get { return SU_IsLocalDocument || !RenderCulture.Equals(Culture.Default); }
			set { RenderCulture = value ? Culture.CurrentCompanyCountryCulture : Culture.Default; }
		}

		public override ZBool SU_IsLocalDocument
		{
			get { return base.SU_IsLocalDocument; }
			set
			{
				if (base.SU_IsLocalDocument != value)
				{
					base.SU_IsLocalDocument = value;
					renderCulture = null;
				}
			}
		}

		#endregion

		#region SU_DeliveryRestrictionType

		[ResourceStringData("StmMenuItem|SU_DeliveryRestrictionType", Caption = "Delivery Restriction", ShortCaption = "Restriction", MediumCaption = "Delivery Restriction Type", FullDescription = "Method of Delivery Restriction. Available options “CNH” (Movement Restricted/Credit on Hold), “UDF” (User Defined Conditions) and “NON” (None)")]
		public override ZString SU_DeliveryRestrictionType
		{
			get
			{
				return base.SU_DeliveryRestrictionType;
			}
			set
			{
				base.SU_DeliveryRestrictionType = value;
				if (value != nameof(DeliveryRestrictionType.UDF))
				{
					SU_DeliveryRestrictionMacro = string.Empty;
				}
			}
		}

		#endregion

		#region SU_DeliveryRestrictionMacro

		[ResourceStringData("StmMenuItem|SU_DeliveryRestrictionMacro", Caption = "Delivery Restriction Macro", ShortCaption = "Restriction Macro", FullDescription = "The Macro of Delivery Restriction")]
		public override ZString SU_DeliveryRestrictionMacro
		{
			get
			{
				return base.SU_DeliveryRestrictionMacro;
			}
			set
			{
				base.SU_DeliveryRestrictionMacro = value;
				if (value == string.Empty)
				{
					SU_DeliveryRestrictionDescription = string.Empty;
				}
			}
		}

#if DEBUG
		internal
#endif
 protected virtual bool SU_DeliveryRestrictionMacro_ReadOnly
		{
			get { return SU_DeliveryRestrictionType != nameof(DeliveryRestrictionType.UDF); }
		}

		#endregion

		#region SU_DeliveryRestrictionDescription

		[ResourceStringData("StmMenuItem|SU_DeliveryRestrictionDescription", Caption = "Delivery Restriction Description", FullDescription = "Description of Delivery Restriction Conditions")]
		public override ZString SU_DeliveryRestrictionDescription
		{
			get
			{
				return base.SU_DeliveryRestrictionDescription;
			}
			set
			{
				base.SU_DeliveryRestrictionDescription = value;
			}
		}

#if DEBUG
		internal
#endif
 protected virtual bool SU_DeliveryRestrictionDescription_ReadOnly
		{
			get { return string.IsNullOrEmpty(SU_DeliveryRestrictionMacro); }
		}

		#endregion

		#region SU_FilterList
		[BusinessObjectTestExclude]
		public override ZString SU_FilterList
		{
			get { return base.SU_FilterList; }
			set { base.SU_FilterList = value; }
		}
		#endregion

		#region MenuTypeDescription
		public ZString MenuTypeDescription
		{
			get
			{
				var type = this.SU_MenuType;
				var des = "";
				if (Core.Constants.StmMenuItemTypes.Documents.Equals(type, StringComparison.OrdinalIgnoreCase) || Core.Constants.StmMenuItemTypes.WebReports.Equals(type, StringComparison.OrdinalIgnoreCase))
				{
					des = Res.GetString("4950DBA8-3D7B-41F4-BFCC-DCE1FA59D8D0", "Normal Document");
				}
				if (Core.Constants.StmMenuItemTypes.Forms.Equals(type, StringComparison.OrdinalIgnoreCase))
				{
					des = Res.GetString("1971B01E-A5A6-43E9-9980-37721CD9BD27", "Visualizer Form");
				}
				return des;
			}
		}
		#endregion

		public ZString DeliveryRestrictionConditionFieldType
		{
			get
			{
				return nameof(FieldType.TextMacro);
			}
		}

		[StmMenuItemTranslatableDataField(Schema.SU_MenuName, MaxLength = Schema.SU_MenuNameMaxLength, Asmid = ResString.AssemblyId)]
		public override ZString SU_MenuName
		{
			get { return base.SU_MenuName; }
			set { base.SU_MenuName = value; }
		}

		[ActionField(CollectionType = typeof(StmEventCodeDescriptionPairList), FieldType = ActionFieldType.Code)]
		public override ZString SU_SE_NKDocumentEvent
		{
			get { return base.SU_SE_NKDocumentEvent; }
			set { base.SU_SE_NKDocumentEvent = value; }
		}

		public MultilingualString SU_MenuNameMultilingual
		{
			get { return GetMultilingual(SU_MenuNameInfo); }
		}

		[StmMenuItemMenuPathTranslatableDataField(Asmid = ResString.AssemblyId)]
		[BusinessObjectTestExclude()]
		public override ZString SU_MenuPath
		{
			get { return base.SU_MenuPath; }
			set { base.SU_MenuPath = value; }
		}

		public MultilingualString SU_MenuPathMultilingual
		{
			get
			{
				return MultilingualString.Join("/", SU_MenuPathMultilingualParts);
			}
		}

		public MultilingualString[] SU_MenuPathMultilingualParts
		{
			get
			{
				return SU_MenuPath.Trim('/', ' ').Split('/').Select(caption => SU_MenuPathInfo.CustomizableDataResourceStrings.GetMultilingualString(this, caption.Trim())).ToArray();
			}
		}

		[StmMenuItemTranslatableDataField(Schema.SU_Hint, Asmid = ResString.AssemblyId)]
		[BusinessObjectTestExclude()]
		public override ZString SU_Hint
		{
			get { return base.SU_Hint; }
			set { base.SU_Hint = value; }
		}

		public MultilingualString SU_HintMultilingual
		{
			get { return GetMultilingual(SU_HintInfo); }
		}

		public bool SU_HintMultilingual_ReadOnly
		{
			get
			{
				if (!InsideSU_HintMultilingual_ReadOnly)
				{
					InsideSU_HintMultilingual_ReadOnly = true;
					bool result = SU_HintInfo.ReadOnly;
					InsideSU_HintMultilingual_ReadOnly = false;
					return result;
				}
				return false;
			}
		}
		bool InsideSU_HintMultilingual_ReadOnly;

		[ResourceStringData("StmMenuItem|SU_DefaultAttachmentType", Caption = "Default Attachment Type", ShortCaption = "Def Attach Type", MediumCaption = "Def Attach Type")]
		[List("AttachmentTypes")]
		public override ZString SU_DefaultAttachmentType
		{
			get { return base.SU_DefaultAttachmentType; }
			set { base.SU_DefaultAttachmentType = value; }
		}

		#endregion

		#region ICanDelete Members

		public override bool CanDelete
		{
			get { return !SU_MenuNameInfo.ReadOnly; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("0e6a80ab-0f7e-400c-b780-27fdeb2ce1df", "Selected item is System Defined, and cannot be deleted by users."); }
		}

		#endregion

		public override void Delete()
		{
			DeleteReportSecurityCheckpoints();
			DeleteJobDocumentDelivery();

			base.Delete();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "hard coded query string")]
		void DeleteReportSecurityCheckpoints()
		{
			var filter = new ZQuery(GlbSecuritySchema.GU_SecurityRight, "Report");
			filter.AddToFilter(GlbSecuritySchema.GU_ItemGUID, PK);
			var checkpoints = new GlbSecurityCollection(Factory);
			checkpoints.Load(filter);
			checkpoints.RemoveAndDeleteAll();
		}

		void DeleteJobDocumentDelivery()
		{
			Factory.Load<JobDocumentDelivery>(new ZQuery(JobDocumentDeliverySchema.JDC_SU_MenuItem, PK)).ForEach(b => b.Delete());
		}

		#region Testing Only
#if DEBUG

		public static StmMenuItem GetForTesting(DocumentDirection direction)
		{
			StmMenuItem result = new BusinessObjectFactory().New<StmMenuItem>();
			result.SU_DocumentDirection = direction.ToString();
			return result;
		}

#endif
		#endregion

		[MaxLength(130)]
		public ZString DocumentId
		{
			get
			{
				return ZString.Format(
					"{0} : {1} : {2} : {3}",
					SU_BusinessContext,
					SU_MenuName,
					RealPathMultilingual.GetUnresolvedString(),
					SU_ContactType);
			}
		}

		public ZString DocumentIdMultilingual
		{
			get
			{
				return ZString.Format(
					"{0} : {1} : {2} : {3}",
					SU_BusinessContext,
					SU_MenuNameMultilingual,
					RealPathMultilingual,
					SU_ContactType);
			}
		}

		public ZPropertyInfo DocumentIdInfo
		{
			get { return GetZPropertyInfo(nameof(DocumentId)); }
		}

		public MultilingualString RealPathMultilingual
		{
			get
			{
				var realPath = MultilingualString.Join("/", RealPathMultilingualParts);
				if (string.IsNullOrEmpty(realPath.GetUnresolvedString()))
				{
					realPath = (NoResString)".";
				}
				return realPath;
			}
		}

		public MultilingualString[] RealPathMultilingualParts
		{
			get
			{
				return SU_MenuPathMultilingualParts.Where(part => part.GetUnresolvedString() != Enterprise.Core.Constants.DocumentEngine.MenuPaths.LegacyDocuments).ToArray();
			}
		}

		public bool IsADocumentMenuItem => !SU_BusinessContext.StartsWith((NoResString)"Rep");

		public ICodeDescriptionPairList AttachmentTypes
		{
			get { return GetAttachmentTypesCore(); }
		}

		protected virtual ICodeDescriptionPairList GetAttachmentTypesCore()
		{
			var result = new CodeDescriptionPairList(OLookUpEditType.CustomType);

			result.AddPair(AttachmentTypeList.Codes.Xls, AttachmentTypeList.Descriptions.Xls);
			result.AddPair(AttachmentTypeList.Codes.Xlsx, AttachmentTypeList.Descriptions.Xlsx);
			result.AddPair(AttachmentTypeList.Codes.Pdf, AttachmentTypeList.Descriptions.Pdf);
			result.AddPair(AttachmentTypeList.Codes.Pdfa, AttachmentTypeList.Descriptions.Pdfa);
			result.AddPair(AttachmentTypeList.Codes.Pdfc, AttachmentTypeList.Descriptions.Pdfc);
			result.AddPair(AttachmentTypeList.Codes.Tif, AttachmentTypeList.Descriptions.Tif);
			result.AddPair(AttachmentTypeList.Codes.Html, AttachmentTypeList.Descriptions.Html);
			result.AddPair(AttachmentTypeList.Codes.Htmf, AttachmentTypeList.Descriptions.Htmf);

			return result;
		}

		internal static StmMenuItem FindDocumentMenu(BusinessObjectFactory newFactory, StmTemplate clientTemplate, BusinessContext businessContext, ZString? menuPath = null)
		{
			ZDBOnlyQuery menuQuery = new ZDBOnlyQuery(typeof(StmMenuItem));
			menuQuery.AddToFilter(StmMenuItemSchema.SU_IsSystemDefined, clientTemplate.SO_IsSystemDefined);
			menuQuery.AddToFilter(StmMenuItemSchema.SU_IsClientSpecific, clientTemplate.SO_IsClientSpecific);
			menuQuery.AddToFilter(StmMenuItemSchema.SU_MenuName, clientTemplate.SO_Name);
			menuQuery.AddToFilter(StmMenuItemSchema.SU_BusinessContext, businessContext);
			if (menuPath.HasValue)
			{
				menuQuery.AddToFilter(StmMenuItemSchema.SU_MenuPath, menuPath.Value);
			}
			return (StmMenuItem)newFactory.LoadTop1(typeof(StmMenuItem), menuQuery);
		}

		internal static StmMenuItem CreateDocumentMenu(BusinessObjectFactory newFactory, StmTemplate clientTemplate, BusinessContext copyBusinessContext, ZString copyFromMenuName, ZString newBusinessContext)
		{
			ZDBOnlyQuery copyFromMenuItemQuery = new ZDBOnlyQuery(typeof(StmMenuItem));
			copyFromMenuItemQuery.AddToFilter(StmMenuItemSchema.SU_IsSystemDefined, true);
			copyFromMenuItemQuery.AddToFilter(StmMenuItemSchema.SU_IsClientSpecific, false);
			copyFromMenuItemQuery.AddToFilter(StmMenuItemSchema.SU_MenuName, copyFromMenuName);
			copyFromMenuItemQuery.AddToFilter(StmMenuItemSchema.SU_BusinessContext, copyBusinessContext);
			StmMenuItem copyFromMenuItem = (StmMenuItem)newFactory.LoadTop1(typeof(StmMenuItem), copyFromMenuItemQuery);

			StmMenuItem newStmMenuItem = newFactory.New<StmMenuItem>();
			newStmMenuItem.CopyPersistentValuesFrom(copyFromMenuItem);
			newStmMenuItem.SU_IsSystemDefined = clientTemplate.SO_IsSystemDefined;
			newStmMenuItem.SU_IsClientSpecific = clientTemplate.SO_IsClientSpecific;
			newStmMenuItem.SU_MenuName = clientTemplate.SO_Name;
			if (!newBusinessContext.IsEmpty)
			{
				newStmMenuItem.SU_BusinessContext = newBusinessContext;
			}
			return newStmMenuItem;
		}

		ICollection IStmMenuItem.Documents => DocumentsCore;

		protected virtual ICollection DocumentsCore => documentsCore ?? (documentsCore = new List<IStmMenuTemplatePivot>());
		ICollection documentsCore;

		ICollection IStmMenuItem.ChildMenus => ChildMenusCore;

		protected virtual ICollection ChildMenusCore => childMenusCore ?? (childMenusCore = new List<StmMenuMenuPivot>());
		ICollection childMenusCore;
	}
}
