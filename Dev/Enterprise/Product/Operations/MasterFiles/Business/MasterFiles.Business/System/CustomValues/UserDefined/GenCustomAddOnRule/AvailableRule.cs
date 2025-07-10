using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	public class AvailableRule : AutoAvailableRule
	{
		public AvailableRule(BusinessObjectFactory factory) : base(factory) { }

		[ResourceStringData("AvailableRule.IsEnabled", Caption = "Is Enabled")]
		public ZBool IsEnabled
		{
			get => rule?.IsEnabled ?? false;
			set
			{
				ZBool val = rule.IsEnabled;
				if (SetNonPersistentPropertyValue(IsEnabledInfo, ref val, value))
				{
					rule.IsEnabled = val;
					Validation.ValidateIsEnabled();
				}
			}
		}

		public ZPropertyInfo IsEnabledInfo => GetZPropertyInfo(nameof(IsEnabled));

		public ICustomAddOnRule Rule
		{
			get => rule;
			internal set
			{
				rule = value;
				using (SuspendSettingHasChanges())
				{
					IsEnabledInfo.RefreshBinding();
				}
			}
		}

		ICustomAddOnRule rule;

		protected override ZString GetName()
		{
			if (Rule != null)
			{
				return Rule.Name;
			}
			else
			{
				return ZString.Empty;
			}
		}
	}
}
