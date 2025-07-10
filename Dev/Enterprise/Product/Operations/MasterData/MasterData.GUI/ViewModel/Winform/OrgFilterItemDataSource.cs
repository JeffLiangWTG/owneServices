using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterData.GUI
{
	public class OrgFilterItemDataSource : NonPersistentBusinessObject
	{
		#region OrgFilterType

		public ZString OrgFilterType
		{
			get
			{
				return orgFilterType;
			}
			set
			{
				if (orgFilterType != value)
				{
					SetNonPersistentPropertyValue(OrgFilterTypeInfo, ref orgFilterType, value);
				}
			}
		}
		ZString orgFilterType;
		public ZPropertyInfo OrgFilterTypeInfo => GetZPropertyInfo(nameof(OrgFilterType));

		public FilterStyle FilterStyle
		{
			get
			{
				if (OrgFilterType == OrgFilterTypeList.Descriptions.Email.GetUnresolvedString()
					|| OrgFilterType == OrgFilterTypeList.Descriptions.MainUNLOCO.GetUnresolvedString()
					|| OrgFilterType == OrgFilterTypeList.Descriptions.Name.GetUnresolvedString())
				{
					return FilterStyle.Input;
				}

				if (OrgFilterType == OrgFilterTypeList.Descriptions.OrgTypes.GetUnresolvedString())
				{
					return FilterStyle.CheckBox;
				}

				return FilterStyle.Default;
			}
		}

		[List(nameof(OrgFilterTypes))]
		public ZString OrgFilterTypeDescription
		{
			get
			{
				return OrgFilterTypes.GetDescriptionFromCode(OrgFilterType);
			}
			set
			{
				var code = OrgFilterTypes.GetCodeFromDescription(value);
				if (!string.IsNullOrEmpty(code))
				{
					OrgFilterType = code;
				}

				OrgFilterTypeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo OrgFilterTypeDescriptionInfo => GetWrappedZPropertyInfo(nameof(OrgFilterTypeDescription), x => OrgFilterTypeInfo);

		public OrgFilterTypeList OrgFilterTypes => orgFilterTypes ?? (orgFilterTypes = new OrgFilterTypeList());
		OrgFilterTypeList orgFilterTypes;

		#endregion

		#region OrgFilterOption

		public ZString OrgFilterOption
		{
			get
			{
				return orgFilterOption;
			}
			set
			{
				if (orgFilterOption != value)
				{
					SetNonPersistentPropertyValue(OrgFilterOptionInfo, ref orgFilterOption, value);
				}
			}
		}
		ZString orgFilterOption = OrgFilterOptionList.Descriptions.Contains.GetUnresolvedString();

		public ZPropertyInfo OrgFilterOptionInfo => GetZPropertyInfo(nameof(OrgFilterOption));

		[List(nameof(OrgFilterOptions))]
		public ZString OrgFilterOptionDescription
		{
			get
			{
				return OrgFilterOptions.GetDescriptionFromCode(OrgFilterOption);
			}
			set
			{
				var code = OrgFilterOptions.GetCodeFromDescription(value);
				if (!string.IsNullOrEmpty(code))
				{
					OrgFilterOption = code;
				}

				OrgFilterOptionInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo OrgFilterOptionDescriptionInfo => GetWrappedZPropertyInfo(nameof(OrgFilterOptionDescription), x => OrgFilterOptionInfo);

		public OrgFilterOptionList OrgFilterOptions => orgFilterOptions ?? (orgFilterOptions = new OrgFilterOptionList());
		OrgFilterOptionList orgFilterOptions;

		#endregion

		#region OrgFilterKeyword

		public ZString OrgFilterKeyword
		{
			get => orgFilterKeyword;
			set
			{
				if (orgFilterKeyword != value)
				{
					SetNonPersistentPropertyValue(OrgFilterKeywordInfo, ref orgFilterKeyword, value);
				}
			}
		}
		ZString orgFilterKeyword;
		public ZPropertyInfo OrgFilterKeywordInfo => GetZPropertyInfo(nameof(OrgFilterKeyword));

		#endregion

		public List<CheckBoxItem> CheckBoxItems { get; set; } = new List<CheckBoxItem>()
		{
			new CheckBoxItem(TextConstant.Receivables),
			new CheckBoxItem(TextConstant.Payables),
			new CheckBoxItem(TextConstant.Consignee),
			new CheckBoxItem(TextConstant.Consignor),
			new CheckBoxItem(TextConstant.Carrier),
			new CheckBoxItem(TextConstant.Forwarder),
			new CheckBoxItem(TextConstant.TransportClient),
			new CheckBoxItem(TextConstant.Warehouse),
			new CheckBoxItem(TextConstant.Broker),
			new CheckBoxItem(TextConstant.Services),
			new CheckBoxItem(TextConstant.Competitor),
			new CheckBoxItem(TextConstant.Sales),
			new CheckBoxItem(TextConstant.ControllingAgent),
			new CheckBoxItem(TextConstant.ControllingCustomer)
		};
	}
}
