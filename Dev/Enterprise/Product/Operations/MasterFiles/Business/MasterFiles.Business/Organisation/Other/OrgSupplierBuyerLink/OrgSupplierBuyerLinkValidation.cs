using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business.Customs.ZA;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSupplierBuyerLinkValidation : AutoOrgSupplierBuyerLinkValidation
	{
		public OrgSupplierBuyerLinkValidation(AutoOrgSupplierBuyerLink parent)
			: base(parent)
		{
			this.Parent = (OrgSupplierBuyerLink)parent;
		}

		#region Parent

		readonly new OrgSupplierBuyerLink Parent;

		#endregion

		#region Helper Methods

		ZBool ValidOrgLevelAirBroker
		{
			get { return Parent.Buyer != null && Parent.Buyer.DeliveryAirCustomsBroker != null; }
		}

		ZBool ValidOrgLevelSeaBroker
		{
			get { return Parent.Buyer != null && Parent.Buyer.DeliverySeaCustomsBroker != null; }
		}

		bool IsValidCountryforVDN(ZString countryCode)
		{
			if (countryCode.IsEmpty)
			{
				return false;
			}
			else
			{
				RefCountry refCountry = new RefCountry.Loader(Parent.Factory).LoadForCountry(countryCode);
				return countryCode == Core.Constants.CountryCodes.SouthAfrica || refCountry.IsPartOfEuropeanUnion;
			}
		}

		#endregion

		#region Unique Supplier Buyer Link

		static string DuplicateRelationshipImportCountry
		{
			get { return Res.GetString("e0d8819e-6a73-4293-8447-0b953cd97afe", "There is more than one relationship entered for this import country/region."); }
		}

		/// <summary>
		/// Business Rules:
		/// The Import Country must be unique for each Buyer/Supplier relationship. 
		/// </summary>
		protected void CheckLinksAreUnique()
		{
			Parent.ClearRowNotifications();

			ZQuery similarLinksFilter = new ZQuery();
			similarLinksFilter.AddToFilter(OrgSupplierBuyerLinkSchema.OL_OH_Supplier, Parent.OL_OH_Supplier);
			similarLinksFilter.AddToFilter(OrgSupplierBuyerLinkSchema.OL_OH_Buyer, Parent.OL_OH_Buyer);
			similarLinksFilter.AddToFilter(OrgSupplierBuyerLinkSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);
			similarLinksFilter.AddToFilter(OrgSupplierBuyerLinkSchema.OL_RN_NKImporterCountry, Parent.OL_RN_NKImporterCountry);

			OrgSupplierBuyerLink similarLink = Parent.Factory.LoadTop1<OrgSupplierBuyerLink>(similarLinksFilter);
			if (similarLink != null)
			{
				Parent.AddRowError(DuplicateRelationshipImportCountry);
			}
		}

		#endregion

		#region Valuation Basis

		protected override void CheckOL_ValuationBasisMarkupPercent()
		{
			base.CheckOL_ValuationBasisMarkupPercent();
			CompareValidation.CheckWithinRange(Parent.OL_ValuationBasisMarkupPercentInfo, 0, 999);
		}

		protected override void CheckOL_RelatedParty()
		{
			base.CheckOL_RelatedParty();
			ListValidation.ErrorIfInvalidCode(Parent.OL_RelatedPartyInfo);
			ValidateOL_ValuationBasis();
		}

		protected override void CheckOL_ValuationBasis()
		{
			base.CheckOL_ValuationBasis();
			ListValidation.ErrorIfInvalidCode(Parent.OL_ValuationBasisInfo);

			if (IsValidCountryforVDN(Parent.OL_RN_NKImporterCountry))
			{
				var valuationCode = Parent.OL_ValuationBasis;
				var relatedIndicator = Parent.OL_RelatedParty;
				ValuationCodeListValidation.ValidateIndicator(relatedIndicator, valuationCode, Parent.OL_ValuationBasisInfo);
			}
		}

		protected override void CheckOL_ValuationBasisDeterminationNum()
		{
			base.CheckOL_ValuationBasisDeterminationNum();

			// Refactor the check if the country is ZA,AU... so it is expandable in future - not inline like this.
			// - xb 10/3/05
			if (!IsValidCountryforVDN(Parent.OL_RN_NKImporterCountry) && !Parent.OL_ValuationBasisDeterminationNum.IsEmpty)
			{
				MandatoryValidation.CheckNotEntered(Parent.OL_ValuationBasisDeterminationNumInfo);
			}
			else if (!Parent.OL_ValuationBasisDeterminationNum.IsEmpty)
			{
				if (Parent.Supplier != null)
				{
					if (Parent.Supplier.LocalCustomsSupplierCode.IsEmpty)
					{
						Parent.OL_ValuationBasisDeterminationNumInfo.AddWarning(Res.GetString("7d87ca55-b9a4-4b01-94e2-d2d6db2a3ffd", "Value Determination Number should not be entered without a Customs supplier code"));
					}
				}

				if (Parent.Buyer != null && Parent.Buyer.LocalCustomsClientCode.IsEmpty)
				{
					Parent.OL_ValuationBasisDeterminationNumInfo.AddWarning(Res.GetString("1d87ddf7-3439-4b07-a8b1-3f6529993149", "Value Determination Number should not be entered without a Customs importer code"));
				}
			}
		}

		protected bool SupplierHasCSCRegNo()
		{
			return Parent.Supplier.CustomsCodes.Cast<OrgCusCode>().Any(x => x.OK_CodeType == OrgCusCode.CodeTypes.SupplierCode);
		}

		#endregion

		#region Importer Country

		protected override void CheckOL_RN_NKImporterCountry()
		{
			base.CheckOL_RN_NKImporterCountry();
			MandatoryValidation.CheckEntered(Parent.OL_RN_NKImporterCountryInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OL_RN_NKImporterCountryInfo);

			if (!Parent.OL_RN_NKImporterCountryInfo.HasErrors())
			{
				CheckLinksAreUnique();
			}
		}

		#endregion

		#region OL_RX_NKDefaultCurrency

		protected override void CheckOL_RX_NKDefaultCurrency()
		{
			base.CheckOL_RX_NKDefaultCurrency();
			ListValidation.ErrorIfInvalidCode(Parent.OL_RX_NKDefaultCurrencyInfo);
		}

		#endregion

		#region OL_AuthorityToLeave

		protected override void CheckOL_AuthorityToLeave()
		{
			base.CheckOL_AuthorityToLeave();
			var info = Parent.OL_AuthorityToLeaveInfo;
			ListValidation.ErrorIfInvalidCode(info);
			MandatoryValidation.CheckEntered(info);
		}

		#endregion

		#region Buyer/Supplier

		protected override void CheckOL_OH_Buyer()
		{
			base.CheckOL_OH_Buyer();

			if (!Parent.OL_OH_BuyerInfo.HasErrors())
			{
				CheckLinksAreUnique();
			}
		}

		protected override void CheckOL_OH_Supplier()
		{
			base.CheckOL_OH_Supplier();

			if (!Parent.OL_OH_SupplierInfo.HasErrors())
			{
				CheckLinksAreUnique();
			}
		}

		#endregion

		#region OL_OH_ControllingCustomer

		protected override void CheckOL_OH_ControllingCustomer()
		{
			base.CheckOL_OH_ControllingCustomer();

			var relatedOrg = Parent.Factory.Load<OrgHeader>(Parent.OL_OH_ControllingCustomer);

			if (OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.Value
				&& relatedOrg != null
				&& !relatedOrg.OH_IsControllingCustomer)
			{
				Parent.OL_OH_ControllingCustomerInfo.AddError(
					Res.GetString("a2ee4928-8430-4b88-95bf-dba77fa2993f", "Only an organization flagged as Controlling Customer can be used as a Controlling Customer."));
			}
		}

		#endregion

		#region Send Import Docs To

		/// <summary>
		/// Business Rules:
		/// 1. The OL_SendImportDocsTo can be blank.
		/// 2. The OL_SendImportDocsTo can be 'IMP'.
		/// 3. If the OL_SendImportDocsTo is 'BRK' or 'BTH' then need to check for a broker.
		///			If OL_ImportBroker is valid, then OK.
		///			If OL_ImportBroker is not valid then check Organisation level for Broker:
		///				If OL_TransportMode is 'ALL' then both Air and Sea Customs brokers need to be present
		///				If OL_TransportMode is 'AIR' then the Air Customs broker needs to be present
		///				If OL_TransportMode is anything else, then the Sea Customs broker needs to be present 
		/// </summary>

		protected override void CheckOL_SendImportDocsTo()
		{
			base.CheckOL_SendImportDocsTo();

			ZString noAirAndSeaBroker = Res.GetString("a005e7b9-5a57-46c8-b122-7baf8bbf7433", "No fallback brokers have been specified. Either select a Broker for this relationship, or ensure that both a fallback Air Broker and Sea Broker have been selected at an organization level.");
			ZString noAirBroker = Res.GetString("dce8488a-9e78-4dda-89e0-c69bbf94c48a", "No fallback broker has been specified. Either select a fallback Broker for this relationship, or ensure that a fallback Air Broker has been selected at an organization level.");
			ZString noSeaBroker = Res.GetString("87b02d7c-7b74-4c4e-a459-0d5905e432a0", "No fallback broker has been specified. Either select a fallback Broker for this relationship, or ensure that a fallback Sea Broker has been selected at an organization level.");

			base.ValidateOL_SendImportDocsTo();
			if (Parent.OL_SendImportDocsTo != "")
			{
				ListValidation.ErrorIfInvalidCode(Parent.OL_SendImportDocsToInfo);
				if (!Parent.OL_SendImportDocsToInfo.HasErrors())
				{
					if ((Parent.OL_SendImportDocsTo == OrgConstants.SendDocsTo.Broker || Parent.OL_SendImportDocsTo == OrgConstants.SendDocsTo.Both)
						&& !Parent.OL_OH_ImportBroker.IsValid)
					{
						bool errorForAir = false;
						bool errorForSea = false;
						foreach (OrgSupBuyLinkTrnMode modeLink in Parent.OrgSupBuyLinkTrnModes)
						{
							if (modeLink.PF_TransportMode == Constants.TransportModes.Air && !ValidOrgLevelAirBroker)
							{
								errorForAir = true;
							}
							else if (!ValidOrgLevelSeaBroker)
							{
								errorForSea = true;
							}
						}

						if (errorForAir && errorForSea)
						{
							Parent.OL_SendImportDocsToInfo.AddError(noAirAndSeaBroker);
						}
						else if (errorForAir)
						{
							Parent.OL_SendImportDocsToInfo.AddError(noAirBroker);
						}
						else if (errorForSea)
						{
							Parent.OL_SendImportDocsToInfo.AddError(noSeaBroker);
						}
					}
				}
			}
		}

		#endregion

		#region ImportBroker

		protected override void CheckOL_OH_ImportBroker()
		{
			base.CheckOL_OH_ImportBroker();
			ValidateOL_SendImportDocsTo();
		}

		#endregion

		#region Expected Shipment Months

		public void ValidateExpectedShipmentMonthsAddition()
		{
			ValidateCalculatedProperty(Parent.ExpectedShipmentMonthsAdditionInfo);
		}

		protected void CheckExpectedShipmentMonthsAddition()
		{
			if (!Parent.UpdateShipmentDateInfo.ReadOnly && !IsExpectedShipmentMonthsAdditionValid())
			{
				Parent.ExpectedShipmentMonthsAdditionInfo.AddError(Res.GetString("7dbae715-fa1f-4273-991f-38d63dc0d9fe", "Please enter a valid number of months"));
			}
		}

		bool IsExpectedShipmentMonthsAdditionValid()
		{
			if (Parent.ExpectedShipmentMonthsAddition.IsEmpty)
			{
				return true;
			}
			else if (ZInt.CanParse(Parent.ExpectedShipmentMonthsAddition))
			{
				ZInt expectedShipmentMonths;
				bool result = ZInt.TryParse(Parent.ExpectedShipmentMonthsAddition, out expectedShipmentMonths);
				result &= expectedShipmentMonths >= 0 && expectedShipmentMonths <= 12;
				return result;
			}
			else
			{
				return false;
			}
		}

		#endregion

		#region OL_EFreightStatus

		protected override void CheckOL_EFreightStatus()
		{
			ListValidation.ErrorIfInvalidCode(Parent.OL_EFreightStatusInfo, Parent.EFreightStatus_List);
		}

		#endregion

		protected override void CheckOL_ProductRelation()
		{
			base.CheckOL_ProductRelation();

			ListValidation.ErrorIfInvalidCode(Parent.OL_ProductRelationInfo);
		}
	}
}
