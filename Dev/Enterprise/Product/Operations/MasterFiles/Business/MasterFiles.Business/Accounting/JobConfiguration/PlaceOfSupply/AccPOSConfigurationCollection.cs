using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	public class AccPOSConfigurationCollection : BusinessObjectCollection<AccPOSConfiguration>
	{
		public AccPOSConfigurationCollection(GlbCompany company)
			: base(company?.Factory, GetZQueryForChargeCodeLevel(company))
		{
			Argument.NotNull(company, nameof(company));

			CompanyPK = company.PK;
			Level = AccPOSConfigurationLevel.Company;
		}

		public AccPOSConfigurationCollection(AccPOSChargeCodeGroup chargeCodeGroup)
			: base(chargeCodeGroup?.Factory, GetZQueryForChargeCodeLevel(chargeCodeGroup))
		{
			Argument.NotNull(chargeCodeGroup, nameof(chargeCodeGroup));

			CompanyPK = chargeCodeGroup.GRO_GC;
			ChargeCodeGroupPK = chargeCodeGroup.PK;
			Level = AccPOSConfigurationLevel.ChargeCodeGroup;
		}

		public AccPOSConfigurationCollection(AccChargeCode chargeCode)
			: base(chargeCode?.Factory, GetZQueryForChargeCodeLevel(chargeCode))
		{
			Argument.NotNull(chargeCode, nameof(chargeCode));

			CompanyPK = chargeCode.AC_GC;
			ChargeCodeGroupPK = chargeCode.PlaceOfSupplyGroup?.PK ?? ZGuid.Empty;
			ChargeCodePK = chargeCode.PK;
			Level = AccPOSConfigurationLevel.ChargeCode;
		}

		public AccPOSConfigurationCollection(BusinessObjectFactory factory)
			: base(factory, ZQuery.NoResultQuery)
		{
			Level = AccPOSConfigurationLevel.Null;
		}

		readonly ZGuid CompanyPK;
		readonly ZGuid ChargeCodeGroupPK;
		readonly ZGuid ChargeCodePK;

		public AccPOSConfigurationLevel Level { get; }

		public override bool ReadOnly => IsEditingForbidden() || base.ReadOnly;

		/// <summary>
		/// Finds the best matching rule in this collection based on parameters.
		/// Note: this will sort the collection in-place.
		/// </summary>
		public AccPOSConfiguration LookupBestMatch(IReadOnlyList<ZGuid> parentIdsInPriorityOrder, string jobType, string chargeType,
			string incoTerm, string serviceDirection, string transportMode, string taxRegistration, string branchCode, string supplyType)
		{
			SortByPrioritySpecificToGeneric();

			var result = this.Cast<AccPOSConfiguration>()
							.FirstOrDefault(c => parentIdsInPriorityOrder.Contains(c.PSC_ParentId)
											  && (c.PSC_JobType == jobType || c.PSC_JobType == JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All)
											  && (c.PSC_ChargeType == chargeType || c.PSC_ChargeType == AccPOSChargeTypeList.Codes.CostAndRevenue)
											  && (c.PSC_IncoTerm == incoTerm || c.PSC_IncoTerm == ZString.Empty)
											  && (c.PSC_ServiceDirection == serviceDirection || c.PSC_ServiceDirection == FreightShipmentDirection.Code.All)
											  && (c.PSC_TransportMode == transportMode || c.PSC_TransportMode == JobConfigurationSelectorLookups.ModeAdditionalCodes.All)
											  && (c.PSC_TaxRegistrationType == taxRegistration || c.PSC_TaxRegistrationType == ZString.Empty)
											  && (c.PSC_NK_Branch == branchCode || c.PSC_NK_Branch == ZString.Empty)
											  && (c.PSC_SupplyType == supplyType || c.PSC_SupplyType == ZString.Empty)
							);
			return result;
		}

		/// <summary>
		/// Sort the collection most generic to most specific. Eg: Company ... Charge Code.
		/// </summary>
		public void SortByPriorityGenericToSpecific() => Sort<AccPOSConfiguration>(AccPOSConfiguration.CompareByRulePriorityGenericToSpecific);

		/// <summary>
		/// Sort the collection most specific to most generic. Eg: Charge Code ... Company.
		/// </summary>
		public void SortByPrioritySpecificToGeneric() => Sort<AccPOSConfiguration>(AccPOSConfiguration.CompareByRulePrioritySpecificToGeneric);

		#region Overrides

		protected override void OnLoaded()
		{
			base.OnLoaded();
			SortByPriorityGenericToSpecific();
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var config = (AccPOSConfiguration)child;
			config.PSC_GC = CompanyPK;

			if (Level == AccPOSConfigurationLevel.Company)
			{
				config.PSC_ParentTableCode = ZString.Empty;
			}
			else if (Level == AccPOSConfigurationLevel.ChargeCodeGroup)
			{
				config.PSC_ParentTableCode = AccPOSChargeCodeGroupViewSchema.Constants.Prefix;
				config.PSC_ParentId = ChargeCodeGroupPK;
			}
			else if (Level == AccPOSConfigurationLevel.ChargeCode)
			{
				config.PSC_ParentTableCode = AccChargeCodeSchema.Constants.Prefix;
				config.PSC_ParentId = ChargeCodePK;
			}
			else if (Level == AccPOSConfigurationLevel.Null)
			{
				throw new NotSupportedException("Creating new child objects of a Null Level configuration is not supported. This Level is a null placeholder.");
			}
			else
			{
				throw new InvalidOperationException("Unknown POS Config Level: " + Level);
			}
		}

		#endregion

		#region Implementation

		bool IsEditingForbidden()
		{
			if (Level == AccPOSConfigurationLevel.Null)
			{
				return true;
			}
			else if (Level == AccPOSConfigurationLevel.Company)
			{
				return !Env.Security.CompaniesModifyPlaceOfSupplyConfiguration.IsAllowed;
			}
			else if (Level == AccPOSConfigurationLevel.ChargeCodeGroup)
			{
				return !Env.Security.POSChargeCodeGroupsModify.IsAllowed;
			}
			else if (Level == AccPOSConfigurationLevel.ChargeCode)
			{
				return !Env.Security.ChargeCodesPlaceOfSupplyConfiguration.IsAllowed;
			}

			return false;
		}
		static ZQuery GetZQueryForChargeCodeLevel(GlbCompany company)
			=> GetZQueryForChargeCodeLevel(company, null, null);

		static ZQuery GetZQueryForChargeCodeLevel(AccPOSChargeCodeGroup chargeCodeGroup)
			=> GetZQueryForChargeCodeLevel(chargeCodeGroup?.Company, chargeCodeGroup, null);

		static ZQuery GetZQueryForChargeCodeLevel(AccChargeCode chargeCode)
			=> GetZQueryForChargeCodeLevel(chargeCode?.Company, chargeCode?.PlaceOfSupplyGroup, chargeCode);

		static ZQuery GetZQueryForChargeCodeLevel(GlbCompany company, AccPOSChargeCodeGroup chargeCodeGroup, AccChargeCode chargeCode)
		{
			var companyPK = company?.PK ?? ZGuid.Empty;
			var query = new ZQuery(AccPOSConfigurationViewSchema.PSC_GC, companyPK);
			var subQuery = new ZQuery();
			if (company != null)
			{
				subQuery.AddToFilter(GetFilterByParent(ZString.Empty, null), JoinCondition.Or);
			}
			if (chargeCodeGroup != null)
			{
				subQuery.AddToFilter(GetFilterByParent(AccPOSChargeCodeGroupViewSchema.Constants.Prefix, chargeCodeGroup.PK), JoinCondition.Or);
			}
			if (chargeCode != null)
			{
				subQuery.AddToFilter(GetFilterByParent(AccChargeCodeSchema.Constants.Prefix, chargeCode.PK), JoinCondition.Or);
			}
			query.AddToFilter(subQuery);
			return query;
		}

		static ZQuery GetFilterByParent(string parentTableCode, ZGuid? parentPk)
			=> new ZQuery(
					new ZQuery(AccPOSConfigurationViewSchema.PSC_ParentTableCode, parentTableCode),
					new ZQuery(AccPOSConfigurationViewSchema.PSC_ParentId, parentPk));

		#endregion

		#region On Removed and logging deletes

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);

			if (ChargeCodePK.IsValid)
			{
				AccChargeCode chargeCode = Factory.Load<AccChargeCode>(ChargeCodePK);
				if (chargeCode != null)
				{
					ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetAccountingLogHelper().AddLog(chargeCode, (AccPOSConfiguration)bizO, true);
				}
			}
		}

		#endregion
	}
}
