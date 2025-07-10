using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public class ComplianceDocumentRePrintRestriction : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string OrganizationCategory = "OrganizationCategory";
			public const string NumberOfReprintAllowed = "NumberOfReprintAllowed";
			public const string ComplianceDocumentMenu = "ComplianceDocumentMenu";
		}

		#endregion

		readonly MultilingualString SameOrgCategoryAndDocMenuExist = ResString.GetMultilingualString("DC7CDA1B-1BAC-4cd6-A901-0DCFD85505AE", "The same organization category and compliance document menu already exists.");
		readonly MultilingualString NumberOfReprintAllowedExceed = ResString.GetMultilingualString("D1447C4E-D41E-4420-A48A-F764823415D6", "Number of re-print allowed must be between 0 and 9.");
		readonly MultilingualString OrganizationCategoryInvalid = ResString.GetMultilingualString("CA695922-3785-4C19-9401-CDE8EBE16E1F", "Organization category is invalid.");

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ComplianceDocumentRePrintRestriction();
		}

		#region OrganizationCategory

		[List("OrganizationCategoryList")]
		public ZString OrganizationCategory
		{
			get
			{
				return organizationCategory;
			}
			set
			{
				CheckMaximumLength(OrganizationCategoryInfo, value);
				SetNonPersistentPropertyValue(OrganizationCategoryInfo, ref organizationCategory, value);
				if (!IsValidationSuspended)
				{
					ValidateOrganizationCategory();
				}
			}
		}
		ZString organizationCategory;

		public ZPropertyInfo OrganizationCategoryInfo
		{
			get { return GetZPropertyInfo(Schema.OrganizationCategory); }
		}

		public void ValidateOrganizationCategory()
		{
			OrganizationCategoryInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(OrganizationCategoryInfo, (IMultilingualString)ResString.GetMultilingualString("5B0A20C0-2CE8-42bb-BE16-317F019450C8", "Organization Category"));

			if (!OrganizationCategoryInfo.HasErrors() && !OrganizationCategoryList.ContainsCode(OrganizationCategory))
			{
				OrganizationCategoryInfo.AddError(OrganizationCategoryInvalid);
			}
		}

		public CodeDescriptionPairList OrganizationCategoryList
		{
			get
			{
				if (organizationCategoryList == null)
				{
					organizationCategoryList = new CodeDescriptionPairList(OLookUpEditType.OrgHeaderCategory);
				}
				return organizationCategoryList;
			}
		}
		CodeDescriptionPairList organizationCategoryList;

		#endregion

		#region NumberOfReprintAllowed
		public ZInt NumberOfReprintAllowed
		{
			get { return numberOfReprintAllowed; }
			set
			{
				SetNonPersistentPropertyValue(NumberOfReprintAllowedInfo, ref numberOfReprintAllowed, value);
				if (!IsValidationSuspended)
				{
					ValidateNumberOfReprintAllowed();
				}
			}
		}
		ZInt numberOfReprintAllowed;

		public ZPropertyInfo NumberOfReprintAllowedInfo
		{
			get { return GetZPropertyInfo(Schema.NumberOfReprintAllowed); }
		}

		public void ValidateNumberOfReprintAllowed()
		{
			NumberOfReprintAllowedInfo.ClearAllNotifications();
			if (NumberOfReprintAllowed < 0 || NumberOfReprintAllowed > 9)
			{
				NumberOfReprintAllowedInfo.AddError(NumberOfReprintAllowedExceed);
			}
		}

		#endregion

		#region ComplianceDocumentMenu

		[List("ComplianceDocumentMenuList")]
		public ZString ComplianceDocumentMenu
		{
			get
			{
				return complianceDocumentMenu;
			}
			set
			{
				CheckMaximumLength(ComplianceDocumentMenuInfo, value);
				SetNonPersistentPropertyValue(ComplianceDocumentMenuInfo, ref complianceDocumentMenu, value);
				if (!IsValidationSuspended)
				{
					ValidateComplianceDocumentMenu();
				}
			}
		}
		ZString complianceDocumentMenu;

		public ZPropertyInfo ComplianceDocumentMenuInfo
		{
			get { return GetZPropertyInfo(Schema.ComplianceDocumentMenu); }
		}

		public void ValidateComplianceDocumentMenu()
		{
			ComplianceDocumentMenuInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ComplianceDocumentMenuInfo, (IMultilingualString)ResString.GetMultilingualString("BF95DF43-98C6-414c-A8C7-1D8397300224", "Compliance Document Menu"));
			if (!ComplianceDocumentMenuInfo.HasErrors() && CheckSameOrgCategoryAndDocMenuExist())
			{
				ComplianceDocumentMenuInfo.AddError(SameOrgCategoryAndDocMenuExist);
			}
		}

		public StmMenuItemFilteredCollection ComplianceDocumentMenuList
		{
			get
			{
				if (complianceDocumentMenuList == null)
				{
					ZQuery menuItemFilter = new ZQuery(StmMenuItemSchema.SU_BusinessContext, new string[] { nameof(BusinessContext.ARComplianceDocument), nameof(BusinessContext.APComplianceDocument) });
					complianceDocumentMenuList = new StmMenuItemFilteredCollection(new BusinessObjectFactory(), menuItemFilter);
				}
				return complianceDocumentMenuList;
			}
		}
		StmMenuItemFilteredCollection complianceDocumentMenuList;

		#endregion

		ZBool CheckSameOrgCategoryAndDocMenuExist()
		{
			ZBool result = ZBool.False;
			if (ParentCollections.Count > 0)
			{
				var sameItmes = ParentCollection.Cast<ComplianceDocumentRePrintRestriction>().Where(x => x.OrganizationCategory == OrganizationCategory && x.ComplianceDocumentMenu == ComplianceDocumentMenu);
				if (sameItmes.Count() > 1)
				{
					result = ZBool.True;
				}
			}
			return result;
		}

		public ComplianceDocumentRePrintRestrictionCollection ParentCollection
		{
			get
			{
				if (((IBusinessObjectInternals)this).ParentCollections.Length > 0)
				{
					return (ComplianceDocumentRePrintRestrictionCollection)((IBusinessObjectInternals)this).ParentCollections[0];
				}
				else
				{
					return new ComplianceDocumentRePrintRestrictionCollection();
				}
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateOrganizationCategory();
			ValidateNumberOfReprintAllowed();
			ValidateComplianceDocumentMenu();
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.OrganizationCategory, OrganizationCategory);
			writer.WriteElementString(Schema.NumberOfReprintAllowed, NumberOfReprintAllowed.ToString());
			writer.WriteElementString(Schema.ComplianceDocumentMenu, ComplianceDocumentMenu);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			OrganizationCategory = reader.ReadElementString(Schema.OrganizationCategory);
			NumberOfReprintAllowed = reader.ReadElementStringAsZInt(Schema.NumberOfReprintAllowed);
			ComplianceDocumentMenu = reader.ReadElementString(Schema.ComplianceDocumentMenu);
		}

		#endregion
	}
}
