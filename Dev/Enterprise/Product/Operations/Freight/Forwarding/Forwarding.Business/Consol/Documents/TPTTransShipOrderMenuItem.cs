using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business
{
	public sealed class TPTTransShipOrderMenuItem : IStmMenuItem
	{
		public TPTTransShipOrderMenuItem(BusinessObjectFactory factory, ZString loadPort, string menuNameMultilingual)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
			this.PortCode = Argument.NotNullOrEmpty(loadPort, nameof(loadPort));

			SU_MenuName = string.Format(CultureInfo.InvariantCulture, (NoResString)"Transhipment Order ({0})", loadPort); // Programmatic constant
			SU_MenuNameMultilingual = (NoResString)menuNameMultilingual;
			SU_MenuType = Core.Constants.StmMenuItemTypes.Forms;
			SU_IsSystemDefined = true;
		}

		readonly BusinessObjectFactory factory;

		public ZString PortCode { get; }

		public ZGuid PK { get; } = ZGuid.NewZGuid();
		public ZBlob SU_ActionDataUpdateBlob { get; set; }
		public ZBlob SU_ActionMenusAndMethodsBlob { get; set; }
		public ZString SU_AddressCategory { get; set; }
		public ZBool SU_AllowRawView { get; set; }
		public ZString SU_BusinessContext { get; set; }
		public ZString SU_ContactType { get; set; }
		public ZString SU_DocumentDirection { get; set; }
		public ZString SU_DraftOption { get; set; }
		public ZString SU_EmailSubjectLine { get; set; }
		public ZString SU_FilterList { get; set; }
		public ZDecimal SU_FlexCelLineSpacing { get; set; }
		public ZString SU_GS_NKStaffCode { get; set; }
		public ZString SU_Hint { get; set; }
		public ZString SU_IncludeDocInArchive { get; set; }
		public ZBool SU_IsClientSpecific { get; set; }
		public ZString SU_DeliveryRestrictionType { get; set; }
		public ZString SU_DeliveryRestrictionMacro { get; set; }
		public ZString SU_DeliveryRestrictionDescription { get; set; }
		public ZBool SU_IsDocPack { get; set; }
		public ZBool SU_IsLocalDocument { get; set; }
		public ZBool SU_IsModifiable { get; set; }
		public ZBool SU_IsPublished { get; set; }
		public ZBool SU_IsSystemDefined { get; set; }
		public ZString SU_LicenceLevel { get; set; }
		public ZString SU_MenuDataContext { get; set; }
		public ZShort SU_MenuIndex { get; set; }
		public ZString SU_MenuName { get; set; }
		public ZString SU_Purpose { get; set; }
		public MultilingualString SU_MenuNameMultilingual { get; set; }
		public ZString SU_MenuPath { get; set; }
		public ZString SU_MenuShortcut { get; set; }
		public ZString SU_MenuType { get; set; }
		public ZBool SU_MustRunOnline { get; set; }
		public ZBool SU_PreventAutoDelivery { get; set; }
		public ZString SU_SE_NKDocumentEvent { get; set; }
		public ZBool SU_ShowDocToSendTab { get; set; }
		public ZBool SU_SupportsVisualisation { get; set; }
		public ZBool SU_IsZippedDocPack { get; set; }
		public ZString SU_SignBy { get; set; }
		public ZString SU_EmailSenderOverride { get; set; }
		public ZGuid SU_PrimaryDocPackItemId { get; set; }
		public ZBool AllowMultipleCopies { get; set; }
		public bool IsLocalDocument { get; set; }
		public ZShort NumberOfCopies { get; set; }
		public CultureInfo RenderCulture { get; set; }
		public ZString DocumentId { get; }
		public ZString SU_DefaultAttachmentType { get; set; }

		public ICodeDescriptionPairList AttachmentTypes { get; }
		public ICollection Documents => documents ?? (documents = CreateDocumentsCollection());
		ICollection documents;

		ICollection IStmMenuItem.ChildMenus => ChildMenusCore;
		ICollection ChildMenusCore => childMenusCore ?? (childMenusCore = new List<StmMenuMenuPivot>());
		ICollection childMenusCore;

		ICollection CreateDocumentsCollection()
		{
			return new[]
			{
				new TPTTransShipOrderMenuTemplatePivot(factory, this, string.Format(CultureInfo.InvariantCulture, (NoResString)"Service Instruction - Transhipment Order({0})", PortCode))  // Programmatic constant
			};
		}
	}
}
