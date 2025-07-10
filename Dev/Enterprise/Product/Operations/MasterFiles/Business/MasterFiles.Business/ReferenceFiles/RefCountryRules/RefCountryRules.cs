using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class RefCountryRules : AutoRefCountryRules
	{
		public RefCountryRules(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString CurrentCountryCode { get; set; }

		#region Properties

		[List("Lookups.RefCountry_List")]
		public override ZString R7_RN_NKOrigin
		{
			get
			{
				return base.R7_RN_NKOrigin;
			}
			set
			{
				base.R7_RN_NKOrigin = value;
				Validation.ValidateR7_RN_NKDestination();
			}
		}

		[List("Lookups.RefCountry_List")]
		public override ZString R7_RN_NKDestination
		{
			get
			{
				return base.R7_RN_NKDestination;
			}
			set
			{
				base.R7_RN_NKDestination = value;
				Validation.ValidateR7_RN_NKOrigin();
			}
		}

		[List("Lookups.RefServiceLevel_List")]
		public override ZGuid R7_RS
		{
			get
			{
				return base.R7_RS;
			}
			set
			{
				base.R7_RS = value;
			}
		}

		[List("Lookups.TransportMode_List")]
		public override ZString R7_TransportMode
		{
			get
			{
				return base.R7_TransportMode;
			}
			set
			{
				base.R7_TransportMode = value;
			}
		}

		[List("Lookups.UltimateConsigneeRule_List")]
		public override ZString R7_UltimateConsigneeRule
		{
			get => base.R7_UltimateConsigneeRule;
			set
			{
				if (base.R7_UltimateConsigneeRule != value)
				{
					base.R7_UltimateConsigneeRule = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateR7_RN_NKDestination();
					}
				}
			}
		}

		public override ZBool R7_IsValidationRule
		{
			get
			{
				return base.R7_IsValidationRule;
			}
			set
			{
				base.R7_IsValidationRule = value;
				if (!base.R7_IsValidationRule)
				{
					R7_IsError = false;
				}
				else
				{
					R7_IsClientVisible = false;
				}
			}
		}

		public bool R7_IsClientVisible_ReadOnly
		{
			get
			{
				return R7_IsValidationRule;
			}
		}

		public bool R7_IsValidationRule_ReadOnly
		{
			get
			{
				return !Env.Security.CountriesEditCountryRuleValidation.IsAllowed;
			}
		}

		public bool R7_IsError_ReadOnly
		{
			get
			{
				return !R7_IsValidationRule || !Env.Security.CountriesEditCountryRuleValidation.IsAllowed;
			}
		}

		public ZBool R7_IsEBLNotSupported
		{
			get { return !R7_IsEBLSupported; }
			set
			{
				R7_IsEBLSupported = !value;
				R7_IsEBLNotSupportedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo R7_IsEBLNotSupportedInfo
		{
			get { return GetZPropertyInfo(nameof(R7_IsEBLNotSupported)); }
		}

		#endregion
	}
}
