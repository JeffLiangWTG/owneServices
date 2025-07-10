using System.Collections;
using System.Globalization;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Integration
{
	public interface IStmMenuItem
	{
		ZGuid PK { get; }

		ZBlob SU_ActionDataUpdateBlob { get; set; }
		ZBlob SU_ActionMenusAndMethodsBlob { get; set; }
		ZString SU_AddressCategory { get; set; }
		ZBool SU_AllowRawView { get; set; }
		ZString SU_BusinessContext { get; set; }
		ZString SU_ContactType { get; set; }
		ZString SU_DocumentDirection { get; set; }
		ZString SU_DraftOption { get; set; }
		ZString SU_EmailSubjectLine { get; set; }
		ZString SU_FilterList { get; set; }
		ZDecimal SU_FlexCelLineSpacing { get; set; }
		ZString SU_GS_NKStaffCode { get; set; }
		ZString SU_Hint { get; set; }
		ZString SU_IncludeDocInArchive { get; set; }
		ZBool SU_IsClientSpecific { get; set; }
		ZString SU_DeliveryRestrictionType { get; set; }
		ZString SU_DeliveryRestrictionMacro { get; set; }
		ZString SU_DeliveryRestrictionDescription { get; set; }
		ZBool SU_IsDocPack { get; set; }
		ZBool SU_IsLocalDocument { get; set; }
		ZBool SU_IsModifiable { get; set; }
		ZBool SU_IsPublished { get; set; }
		ZBool SU_IsSystemDefined { get; set; }
		ZString SU_LicenceLevel { get; set; }
		ZString SU_MenuDataContext { get; set; }
		ZShort SU_MenuIndex { get; set; }
		ZString SU_MenuName { get; set; }
		ZString SU_Purpose { get; set; }
		MultilingualString SU_MenuNameMultilingual { get; }
		ZString SU_MenuPath { get; set; }
		ZString SU_MenuShortcut { get; set; }
		ZString SU_MenuType { get; set; }
		ZBool SU_MustRunOnline { get; set; }
		ZBool SU_PreventAutoDelivery { get; set; }
		ZString SU_SE_NKDocumentEvent { get; set; }
		ZBool SU_ShowDocToSendTab { get; set; }
		ZBool SU_SupportsVisualisation { get; set; }
		ZBool SU_IsZippedDocPack { get; set; }
		ZGuid SU_PrimaryDocPackItemId { get; set; }
		ZString SU_SignBy { get; set; }
		ZString SU_EmailSenderOverride { get; set; }
		ZBool AllowMultipleCopies { get; set; }
		bool IsLocalDocument { get; set; }
		ZShort NumberOfCopies { get; set; }
		CultureInfo RenderCulture { get; set; }
		ZString DocumentId { get; }
		ZString SU_DefaultAttachmentType { get; set; }

		ICodeDescriptionPairList AttachmentTypes { get; }

		ICollection Documents { get; }
		ICollection ChildMenus { get; }
	}
}
