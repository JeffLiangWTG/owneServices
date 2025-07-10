using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.DialogDefault;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module.DialogDefault
{
	public class DialogDefaultOwnerModuleFilter : ModuleFilter
	{
		public DialogDefaultOwnerModuleFilter(string description)
			: base(description, new BusinessObjectFactory())
		{
			MultilingualDescription = ResString.GetMultilingualString("CD2CF72B-E73D-4076-835E-CC4B78BC8B0C", "Owner");
		}

		#region Properties

		#region LevelCode

		ZString levelCodes;

		[List("LevelCodeList")]
		public ZString LevelCode
		{
			get { return levelCodes; }
			set
			{
				if (levelCodes != value)
				{
					SetNonPersistentPropertyValue(LevelCodeInfo, ref levelCodes, value);
					OwnerPk = ZGuid.Empty;
					LevelCodeInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo LevelCodeInfo { get { return GetZPropertyInfo(nameof(LevelCode)); } }

		[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		public CodeDescriptionPairList LevelCodeList
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(DialogDefaultLevel.Codes.Global, Res.GetString("96E3484B-6C47-4F2C-BFB1-6D7392AE11B2", "Global"));
				list.AddPair(DialogDefaultLevel.Codes.Company, Res.GetString("D0E5CF8A-54C1-425B-8984-416B33107967", "Company"));
				list.AddPair(DialogDefaultLevel.Codes.User, Res.GetString("D6DDECF7-26A8-46B3-95E6-4434AA8481AB", "User"));

				return list;
			}
		}

		#endregion

		#region Owner

		ZGuid ownerPk;

		[List("OwnerPkList")]
		public ZGuid OwnerPk
		{
			get { return ownerPk; }
			set
			{
				if (value != ownerPk)
				{
					SetNonPersistentPropertyValue(OwnerPkInfo, ref ownerPk, value);
					OwnerPkInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public bool OwnerPk_ReadOnly
		{
			get
			{
				return LevelCode != DialogDefaultLevel.Codes.Company && LevelCode != DialogDefaultLevel.Codes.User;
			}
		}

		public ZPropertyInfo OwnerPkInfo { get { return GetZPropertyInfo(nameof(OwnerPk)); } }

		public IActiveBusinessObjectCollection OwnerPkList
		{
			get { return LevelCode == DialogDefaultLevel.Codes.User ? new GlbStaffCollection(Factory) : new GlbCompanyCollection(Factory); }
		}

		#endregion

		#endregion

		#region Query

		protected override ZQuery GetQuery()
		{
			var query = new ZQuery();
			if (!string.IsNullOrEmpty(LevelCode))
			{
				query.AddToFilter(StmDialogDefaultSchema.SDD_Level, LevelCode);

				if (!OwnerPk.IsEmpty)
				{
					query.AddToFilter(StmDialogDefaultSchema.SDD_Owner, ownerPk);
				}
			}

			return query;
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			return GetQuery();
		}

		protected override bool IsEmptyCore => !string.IsNullOrEmpty(LevelCode);

		public override bool IsExpensiveQuery
		{
			get { return false; }
		}

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { LevelCode, OwnerPk }; }
		}

		#endregion

		#region XML

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			writer.WriteElementString(LevelCodeInfo.Name, LevelCode);
			writer.WriteElementString(OwnerPkInfo.Name, OwnerPk.ToStringKey());
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			if (reader.Name == LevelCodeInfo.Name)
			{
				LevelCode = reader.ReadElementString(LevelCodeInfo.Name);
			}
			else if (reader.Name == OwnerPkInfo.Name)
			{
				ZGuid pk;
				OwnerPk = ZGuid.TryParse(reader.ReadElementString(OwnerPkInfo.Name), out pk) ? pk : ZGuid.Empty;
			}
		}

		#endregion

		#region Implementation

		protected override void ClearCore()
		{
			LevelCode = ZString.Empty;
			OwnerPk = ZGuid.Empty;
		}

		protected override FilterCategory DefaultCategory
		{
			get { return FilterCategories.Other; }
		}

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new DialogDefaultOwnerModuleFilter("Owner");
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			var other = (DialogDefaultOwnerModuleFilter)filterToCopyFrom;
			LevelCode = other.LevelCode;
			OwnerPk = other.OwnerPk;
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new DialogDefaultOwnerModuleFilterValidation(this);
		}

		#region Test Data Setup
#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			LevelCode = RandomString(MaxLength);
			OwnerPk = ZGuid.NewZGuid();
		}

#endif
		#endregion

		#endregion

		#region For testing

		public void CopyPersistantValuesFromFilter_Exposed(ModuleFilter filterToCopyFrom)
		{
			CopyPersistantValuesFromFilter(filterToCopyFrom);
		}

		#endregion

	}

	#region Validation

	class DialogDefaultOwnerModuleFilterValidation : ModuleFilterValidation
	{
		public DialogDefaultOwnerModuleFilterValidation(DialogDefaultOwnerModuleFilter parent)
			: base(parent)
		{
		}

		new DialogDefaultOwnerModuleFilter ParentFilter { get { return (DialogDefaultOwnerModuleFilter)base.ParentFilter; } }

		public void Validate_OwnerPk()
		{
			ErrorIfInvalidPK(ParentFilter.OwnerPkInfo, ParentFilter.OwnerPkList);
		}

		public void Validate_LevelCode()
		{
			ErrorIfInvalidCode(ParentFilter.LevelCodeInfo, new DialogDefaultLevel());
		}

		public override void ValidateAll()
		{
			Validate_LevelCode();
			Validate_OwnerPk();
		}

		public override Type AutoValidationType
		{
			get { throw new NotImplementedException(); }
		}
	}

	#endregion
}
