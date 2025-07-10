using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business.AWB
{
	[DependentBusinessObject(typeof(ExportAWBHeader), "AWBSpecialHandlingItems")]
	public class ExportAWBSpecialHandling : Forwarding.AWB.Business.ExportAWBSpecialHandling
	{
		public ExportAWBSpecialHandling(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			originalEP_EH = EP_EH;
		}

		public override bool IsSavedByFactory
		{
			get
			{
				ExportAWBHeader.SaveMode saveMode = OriginalMaster != null ? OriginalMaster.FactorySaveMode : ExportAWBHeader.SaveMode.Normal;
				switch (saveMode)
				{
					case ExportAWBHeader.SaveMode.Normal:
						return base.IsSavedByFactory;
					case ExportAWBHeader.SaveMode.Forced:
						return true;
					default:
						return false;
				}
			}
		}

		ExportAWBHeader OriginalMaster
		{
			get { return Factory.Load<ExportAWBHeader>(IsDeleted || EP_EH.IsEmpty ? originalEP_EH : EP_EH); }
		}

		public override ZGuid EP_EH
		{
			get { return base.EP_EH; }
			set
			{
				base.EP_EH = value;
				if (!value.IsEmpty)
				{
					originalEP_EH = value;
				}
			}
		}
		ZGuid originalEP_EH;

		public override ZString EP_SpecialHandling
		{
			get { return base.EP_SpecialHandling; }
			set
			{
				if (base.EP_SpecialHandling != value)
				{
					if (!IsSettingHasChangesSuspended)
					{
						EP_SpecialHandlingHasChanges = true;
					}

					if (ParentConsolidation != null && !(ParentConsolidation as ISupportDataImporting).IsImportingData && RequiresVerificationOfFreight(value))
					{
						if (ParentConsolidation.UserHasVerifiedFreightIsSecure())
						{
							ParentConsolidation.FreightHasBeenVerifiedAsSecure = true;
						}
						else
						{
							ParentConsolidation.FreightHasBeenVerifiedAsSecure = false;
							return;
						}
					}

					base.EP_SpecialHandling = value;

					var master = (ExportAWBHeader)Master;

					if (master != null)
					{
						master.EH_SecurityStatusInfo.RefreshBinding();
						master.EH_SecurityStatusForNonBorrowedMAWBsInfo.RefreshBinding();

						if (master.ReadOnly)
						{
							master.PopulateExtraShipperInfoLine2();
						}
					}
				}
			}
		}

		internal bool EP_SpecialHandlingHasChanges { get; private set; }

		#region Implementation

		ForwardingConsol ParentConsolidation => ((ExportAWBHeader)Master).Parent as ForwardingConsol;

		protected override Forwarding.AWB.Business.ExportAWBSpecialHandlingValidation GetNewValidation()
		{
			return new ExportAWBSpecialHandlingValidation(this);
		}

		bool RequiresVerificationOfFreight(ZString newCode)
		{
			if (newCode == AWBSpecialHandlingCodeDescriptionPairList.Codes.CargoSecureForPassengerAndAllCargoAircraft)
			{
				var consol = ParentConsolidation;
				if (consol != null && consol.JK_OverrideWaybillDefaults && SupplyChainSecurityConfiguration.SCSSupportedCountryList.Any(countryCode =>
					consol.JK_RL_NKLoadPort.StartsWith(countryCode, StringComparison.OrdinalIgnoreCase)
					&& string.Equals(GlbCompany.CurrentCompany?.GC_RN_NKCountryCode ?? ZString.Empty, countryCode, StringComparison.OrdinalIgnoreCase)))
				{
					return true;
				}
			}

			return false;
		}

		public new ExportAWBSpecialHandlingLookups Lookups => (ExportAWBSpecialHandlingLookups)base.Lookups;

		protected override Forwarding.AWB.Business.ExportAWBSpecialHandlingLookups GetNewLookups()
		{
			return new ExportAWBSpecialHandlingLookups(this);
		}

		#endregion
	}
}
