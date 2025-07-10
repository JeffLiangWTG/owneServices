//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgSupBuyLinkTrnModeValidation
//
//    This class should be used for overriding validation in AutoOrgSupBuyLinkTrnModeValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSupBuyLinkTrnModeValidation : AutoOrgSupBuyLinkTrnModeValidation
	{
		public OrgSupBuyLinkTrnModeValidation(AutoOrgSupBuyLinkTrnMode parent)
			: base(parent)
		{
		}

		public void ValidatePF_USPortOfLading()
		{
			ValidateCalculatedProperty(Parent.PF_USPortOfLadingInfo);
		}

		public void ValidatePF_USPortOfUnLading()
		{
			ValidateCalculatedProperty(Parent.PF_USPortOfUnLadingInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidatePF_USPortOfLading();
			ValidatePF_USPortOfUnLading();
		}

		protected new OrgSupBuyLinkTrnMode Parent
		{
			get { return (OrgSupBuyLinkTrnMode)base.Parent; }
		}

		#region Container Mode / Incoterm / Transpport Mode

		protected override void CheckPF_ContainerMode()
		{
			base.CheckPF_ContainerMode();
			ListValidation.ErrorIfInvalidCode(Parent.PF_ContainerModeInfo);

			if (!Parent.PF_ContainerModeInfo.HasErrors())
			{
				CheckModesAreUnique();
			}
		}

		protected override void CheckPF_IncoTerm()
		{
			base.CheckPF_IncoTerm();
			ListValidation.ErrorIfInvalidCode(Parent.PF_IncoTermInfo);
			IncotermValidation.Instance.WarningIfExpired(Parent.PF_IncoTermInfo);
		}

		protected override void CheckPF_IncoTermMode()
		{
			base.CheckPF_IncoTermMode();
			ListValidation.ErrorIfInvalidCode(Parent.PF_IncoTermModeInfo);
		}

		protected override void CheckPF_TransportMode()
		{
			base.CheckPF_TransportMode();
			MandatoryValidation.CheckEntered(Parent.PF_TransportModeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.PF_TransportModeInfo);

			if (!Parent.PF_TransportModeInfo.HasErrors())
			{
				CheckModesAreUnique();
			}
		}

		#endregion

		protected override void CheckPF_OH_ControllingCustomer()
		{
			base.CheckPF_OH_ControllingCustomer();

			var relatedOrg = Parent.Factory.Load<OrgHeader>(Parent.PF_OH_ControllingCustomer);

			if (OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.Value
				&& relatedOrg != null
				&& !relatedOrg.OH_IsControllingCustomer)
			{
				Parent.PF_OH_ControllingCustomerInfo.AddError(
					Res.GetString("a2ee4928-8430-4b88-95bf-dba77fa2993f", "Only an organization flagged as Controlling Customer can be used as a Controlling Customer."));
			}
		}

		static string DuplicateRelationship
		{
			get { return Res.GetString("4ae9d5f1-e1d9-4541-b120-8f049751acde", "There is more than one entered with this Transport/Container Mode."); }
		}

		protected void CheckModesAreUnique()
		{
			Parent.ClearRowNotifications();

			ZQuery similarModesFilter = new ZQuery();
			similarModesFilter.AddToFilter(OrgSupBuyLinkTrnModeSchema.PF_OL, Parent.PF_OL);
			similarModesFilter.AddToFilter(OrgSupBuyLinkTrnModeSchema.PF_TransportMode, Parent.PF_TransportMode);
			similarModesFilter.AddToFilter(OrgSupBuyLinkTrnModeSchema.PF_ContainerMode, Parent.PF_ContainerMode);
			similarModesFilter.AddToFilter(OrgSupBuyLinkTrnModeSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

			BusinessObject[] similarMode = Parent.Factory.Load<OrgSupBuyLinkTrnMode>(similarModesFilter);
			if (similarMode.Length > 0)
			{
				Parent.AddRowError(DuplicateRelationship);
			}
		}

		#region ValidatePF_RS_NKDefaultServiceLevel

		protected override void CheckPF_RS_NKDefaultServiceLevel()
		{
			base.CheckPF_RS_NKDefaultServiceLevel();
			ListValidation.ErrorIfInvalidCode(Parent.PF_RS_NKDefaultServiceLevelInfo, Parent.Lookups.DefaultServiceLevels);
		}

		#endregion

		protected override void CheckPF_RL_NKPlaceOfDeliveryPort()
		{
			base.CheckPF_RL_NKPlaceOfDeliveryPort();
			ListValidation.ErrorIfInvalidCode(Parent.PF_RL_NKPlaceOfDeliveryPortInfo);
		}

		protected override void CheckPF_RL_NKPlaceOfReceivalPort()
		{
			base.CheckPF_RL_NKPlaceOfReceivalPort();
			ListValidation.ErrorIfInvalidCode(Parent.PF_RL_NKPlaceOfReceivalPortInfo);
		}

		protected virtual void CheckPF_USPortOfLading()
		{
			ListValidation.WarnIfInvalidCode(Parent.PF_USPortOfLadingInfo, Parent.Lookups.USPortsOfLading, ResString.GetMultilingualString("1e610cd1-52b9-42ab-bb5e-851f991082d3", "The US Port Of Lading code is invalid."));
		}

		protected virtual void CheckPF_USPortOfUnLading()
		{
			ListValidation.WarnIfInvalidCode(Parent.PF_USPortOfUnLadingInfo, Parent.Lookups.USPortsOfUnLading, ResString.GetMultilingualString("dec391ef-799d-47c4-b16b-466c7180263c", "The US Port Of Un-Lading code is invalid."));
		}

		protected override void CheckPF_OC_OverrideNotifyParty()
		{
			base.CheckPF_OC_OverrideNotifyParty();

			if (Parent.PF_OA_OverrideNotifyPartyAddress.IsValid && Parent.Lookups.OverrideNotifyParties.Count > 0)
			{
				MandatoryValidation.CheckEntered(Parent.PF_OC_OverrideNotifyPartyInfo);
			}
		}
	}
}
