using System.ComponentModel;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.Business
{
	public class CusUSLVItemPGAWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public CusUSLVItemPGAWrapper(BusinessObjectFactory factory, CusUSLVItemPGAAgencyRequirementsProvider pgaProvider, string agencyCode, CusUSLVItemPGA pga = null)
			: base(factory)
		{
			this.pgaProvider = Argument.NotNull(pgaProvider, "pgaProvider");
			this.pga = pga;
			AgencyCode = agencyCode;
			if (pga != null)
			{
				pga.AgencyCode = agencyCode;
			}
		}

		readonly CusUSLVItemPGAAgencyRequirementsProvider pgaProvider;
		CusUSLVItemPGA pga;
		public ZString AgencyCode { get; }

		[ReadOnly(true)]
		public ZString Agency { get; set; }

		[ReadOnly(true)]
		public ZString AgencyProgram { get; set; }

		public CusUSLVItemPGA PGA => pga;

		public CodeDescriptionPairList PGADisclaimReasonList => pgaProvider.GetDisclaimReasonList(AgencyCode);

		[ReadOnly(true)]
		public ZString AgencyCodeWithDescription { get; set; }

		[ReadOnly(true)]
		public ZString Requirement => pgaProvider.GetRequirementDescription(AgencyCode);

		public ZPropertyInfo RequirementInfo => GetZPropertyInfo(nameof(Requirement));

		[List(nameof(OGAIndicatorList))]
		[MaxLength(1)]
		public ZString Indicator
		{
			get
			{
				return pga?.ULP_Indicator ?? ZString.Empty;
			}
			set
			{
				var oldValue = Indicator;
				if (oldValue != value && !IsCopying)
				{
					if (value.IsEmpty)
					{
						RemovePGAIfExist();
					}
					else
					{
						CreateNewPGAIfNotExist();
						pga.ULP_Indicator = value;

						if (value != OGAIndicatorList.Codes.Disclaimed)
						{
							DisclaimReason = ZString.Empty;
						}
					}
				}

				IndicatorInfo.RefreshBinding(oldValue);
			}
		}

		[List(nameof(PGADisclaimReasonList))]
		[MaxLength(1)]
		public ZString DisclaimReason
		{
			get
			{
				return pga?.ULP_DisclaimReason ?? ZString.Empty;
			}
			set
			{
				var oldValue = DisclaimReason;
				if (oldValue != value && !IsCopying)
				{
					if (value.IsEmpty)
					{
						RemovePGAIfExist();
					}
					else
					{
						CreateNewPGAIfNotExist();
						pga.ULP_DisclaimReason = value;
					}

					ValidateDisclaimReason();
				}

				DisclaimReasonInfo.RefreshBinding(oldValue);
			}
		}

		void CreateNewPGAIfNotExist()
		{
			if (pga == null)
			{
				pga = pgaProvider.ParentItem.CusUSLVItemPGAs.AddNew();
				pga.ULP_Agency = Agency;
				pga.ULP_AgencyProgram = AgencyProgram;
				pga.AgencyCode = AgencyCode;
				pga.ULP_Indicator = OGAIndicatorList.Codes.Disclaimed;
			}
		}

		void RemovePGAIfExist()
		{
			if (pga != null)
			{
				pgaProvider.ParentItem.CusUSLVItemPGAs.RemoveAndDelete(pga);
				pga = null;
			}
		}

		public void ValidateDisclaimReason()
		{
			DisclaimReasonInfo.ClearAllNotifications();
			pga?.ULP_DisclaimReasonInfo?.RunAdditionalValidation();
			if (!IsValidationSuspended && pga != null)
			{
				DisclaimReasonInfo.AddAllNotificationsFrom(pga.ULP_DisclaimReasonInfo);
			}

			if (!Requirement.IsEmpty && DisclaimReason.IsEmpty)
			{
				if (AgencyCode == GovernmentAgencyProgramCodeList.Codes.APHIS && (pgaProvider?.ParentItem?.PGARequirementIndicator.MayRequireAPHISNoDisclaimRequired ?? false))
				{
					DisclaimReasonInfo.AddWarning(APHISDataMayBeRequired);
				}
				else
				{
					DisclaimReasonInfo.AddMessageError(Res.GetString("130a2ecf-3ce1-4e96-aaed-3e6e5e5527ab", "Agency Program declarations are not supported in this module and will need to be completed using a stand alone declaration."));
				}
			}
		}
		internal const string APHISDataMayBeRequired = "APHIS Data May be required, no disclaim is required if APHIS does not apply";

		public ZPropertyInfo DisclaimReasonInfo => GetZPropertyInfo(nameof(DisclaimReason));

		public ZPropertyInfo IndicatorInfo => GetZPropertyInfo(nameof(Indicator));
	}
}
