using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgImpAddInfo : AutoUSOrgImpAddInfo, Integration.Customs.US.IOrgImpAddInfo
	{
		public OrgImpAddInfo(ZPropertyInfoString parentPropertyInfo)
			: base(parentPropertyInfo.BizObj.Factory)
		{
			this.organization = ((OrgCountryData)parentPropertyInfo.BizObj).OrgHeader;
			this.ParentPropertyInfo = parentPropertyInfo;
			using (GetValidationSuspender())
			using (SuspendSettingHasChanges())
			{
				Deserialise();
			}
		}
		internal readonly OrgHeader organization;

		[List(nameof(Lookups) + "." + nameof(USOrgImpAddInfoLookups.ZO_YesNoList))]
		public override ZString ZO_Purchased
		{
			get { return base.ZO_Purchased; }
			set { base.ZO_Purchased = value; }
		}

		[List(nameof(Lookups) + "." + nameof(USOrgImpAddInfoLookups.ZO_ImportSourceList))]
		public override ZString ZO_ImportSource
		{
			get { return base.ZO_ImportSource; }
			set { base.ZO_ImportSource = value; }
		}

		[List(nameof(Lookups) + "." + nameof(USOrgImpAddInfoLookups.ReconPorts))]
		public override ZString ZO_ReconFilingPort
		{
			get { return base.ZO_ReconFilingPort; }
			set { base.ZO_ReconFilingPort = value; }
		}

		[List(nameof(Lookups) + "." + nameof(USOrgImpAddInfoLookups.ReconPaymentTypes))]
		public override ZString ZO_ReconPaymentType
		{
			get { return base.ZO_ReconPaymentType; }
			set { base.ZO_ReconPaymentType = value; }
		}

		internal bool IsPaidByImporter
		{
			get
			{
				return PaymentTypeList.IsPaidByImporter(ZO_PaymentType)
					|| !PaymentTypeList.IsPaidByBroker(ZO_PaymentType) && ZO_BrokerToPay == YesNoDefaultList.Codes.No;
			}
		}

		#region ZO_NPID

		public override ZString ZO_NPID
		{
			get
			{
				var result = base.ZO_NPID;
				if (ZO_OH_NP.IsValid)
				{
					result = GetNotifyPartyRef();
				}
				return result;
			}
			set { base.ZO_NPID = value; }
		}

		ZString GetNotifyPartyRef()
		{
			var result = ZString.Empty;
			if (ZO_OH_NP.IsValid)
			{
				var notifyParty = Factory.Load<OrgHeader>(ZO_OH_NP);
				if (notifyParty != null)
				{
					result = notifyParty.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.UnitedStates,
						OrgCusCode.USACodeTypes.EmployerIdentificationNumber, OrgCusCode.USACodeTypes.CBPAssignedNumber,
						OrgCusCode.USACodeTypes.SocialSecurityNumber);
				}
			}
			return result;
		}

		#endregion

		#region ZO_OH_NP

		public override ZGuid ZO_OH_NP
		{
			get { return base.ZO_OH_NP; }
			set
			{
				base.ZO_OH_NP = value;
				ZO_NPID = ZString.Empty;
			}
		}

		#endregion

		protected override USOrgImpAddInfoLookups GetNewLookups()
		{
			return new USOrgImpAddInfoLookups(this);
		}

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool shouldBeReadOnly = false;
			if (property.Name == USOrgImpAddInfoSchema.ZO_Purchased.Name)
			{
				shouldBeReadOnly = !Env.Security.OrgConsigneeModifyCountrySpecificDetails.IsAllowed;
			}
			else
			{
				shouldBeReadOnly = !Env.Security.OrgConfigModifyCountryDefaults.IsAllowed;
			}

			return shouldBeReadOnly || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion
	}
}
