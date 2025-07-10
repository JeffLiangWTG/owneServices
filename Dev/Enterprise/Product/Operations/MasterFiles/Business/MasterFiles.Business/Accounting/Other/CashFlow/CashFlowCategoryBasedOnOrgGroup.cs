using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public abstract class CashFlowCategoryBasedOnOrgGroup : RegistryBusinessObjectTemplate
	{
		#region Schema

		abstract class Schema
		{
			public const string OrgGroupPK = "OrgGroupPK";
			public const string CashFlowCategory = "CashFlowCategory";
		}

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			ClearAllNotifications();
			base.RunPreSaveValidationCore();

			ValidateOrgGroupPK();
			ValidateCashFlowCategory();
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			if (IsInDatabase)
			{
				base.OnFactorySavingBeforeTransactionCore();
			}
		}

		#region OrgGroupPK

		[List("OrgGroupList")]
		public ZGuid OrgGroupPK
		{
			get { return orgGroupPK; }
			set
			{
				if (orgGroupPK != value)
				{
					SetNonPersistentPropertyValue(OrgGroupPKInfo, ref orgGroupPK, value);
					orgGroupDescription = GetOrgGroupDescriptionCore();
				}
				if (!IsValidationSuspended)
				{
					ValidateOrgGroupPK();
				}
			}
		}

		public ZPropertyInfo OrgGroupPKInfo
		{
			get { return GetZPropertyInfo(Schema.OrgGroupPK); }
		}

		public void ValidateOrgGroupPK()
		{
			OrgGroupPKInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(OrgGroupPKInfo);
			ListValidation.ErrorIfInvalidPK(OrgGroupPKInfo, OrgGroupList);

			if (ParentCollections.Count > 0)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(OrgGroupPKInfo, Res.GetString("8188ea70-9b55-4125-af87-5090772c99b2", "There must be only one line for each org. group."));
			}
		}

		ZGuid orgGroupPK;

		public abstract BusinessObjectCollection OrgGroupList
		{ get; }

		protected abstract ZString GetOrgGroupDescriptionCore();

		#endregion

		#region OrgGroupDescription

		ZString orgGroupDescription;
		public ZString OrgGroupDescription
		{
			get { return orgGroupDescription; }
		}

		#endregion

		#region CashFlowCategory

		[MaxLength(3)]
		[List("CashFlowCategoryList")]
		public ZString CashFlowCategory
		{
			get { return cashFlowCategory; }
			set
			{
				SetNonPersistentPropertyValue(CashFlowCategoryInfo, ref cashFlowCategory, value);
				cashFlowCategoryDescription = CashFlowCategoryList.GetDescriptionFromCode(CashFlowCategory);
				if (!IsValidationSuspended)
				{
					ValidateCashFlowCategory();
				}
			}
		}

		public ZPropertyInfo CashFlowCategoryInfo
		{
			get { return GetZPropertyInfo(Schema.CashFlowCategory); }
		}

		public void ValidateCashFlowCategory()
		{
			CashFlowCategoryInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CashFlowCategoryInfo);
			ListValidation.ErrorIfInvalidCode(CashFlowCategoryInfo, CashFlowCategoryList);
		}

		ZString cashFlowCategory;

		public CodeDescriptionPairList CashFlowCategoryList
		{
			get
			{
				if (cashFlowCategoryList == null)
				{
					cashFlowCategoryList = new CodeDescriptionPairList();
					foreach (CashFlowActivityConfiguration cashFlowActivity in AccountingMasterFilesRegistry.Instance.CashFlowActivityConfiguration.Value)
					{
						if (cashFlowActivity.Code != CashFlowCodeLists.Codes.XXX &&
							cashFlowActivity.Code != CashFlowCodeLists.Codes.NON &&
							cashFlowActivity.Code != CashFlowCodeLists.Codes.CSH &&
							cashFlowActivity.Code != CashFlowCodeLists.Codes.EXX)
						{
							cashFlowCategoryList.AddPair(cashFlowActivity.Code, cashFlowActivity.Description);
						}
					}
				}
				return cashFlowCategoryList;
			}
		}

		CodeDescriptionPairList cashFlowCategoryList;

		#endregion

		#region CashFlowCategoryDescription

		ZString cashFlowCategoryDescription;
		public ZString CashFlowCategoryDescription
		{
			get { return cashFlowCategoryDescription; }
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.OrgGroupPK, OrgGroupPK.ToString());
			writer.WriteElementString(Schema.CashFlowCategory, CashFlowCategory);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			OrgGroupPK = new ZGuid(reader.ReadElementString(Schema.OrgGroupPK));
			CashFlowCategory = new ZString(reader.ReadElementString(Schema.CashFlowCategory));
		}

		#endregion
	}
}
