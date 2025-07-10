using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ModuleID(ModuleId.SupplierPart)]
	public class OrgSupplierPartCollection : BusinessObjectCollection<OrgSupplierPart>
	{
		#region ctor

		public OrgSupplierPartCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public OrgSupplierPartCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public OrgSupplierPartCollection(BusinessObjectFactory factory, OrgHeader supplier, OrgHeader owner, bool isExport, PartFilterOptions filterOptions = PartFilterOptions.None)
			: this(factory, GetSupplierOwnerFilterDontResolveDuplicates(supplier, owner, filterOptions))
		{
			fOwner = filterOptions.HasFlag(PartFilterOptions.OwnerMandatory) ? Argument.NotNull(owner, "owner") : owner;
			fSupplier = supplier;
			RemoveDuplicates = true;
			isIncludeInActive = filterOptions.HasFlag(PartFilterOptions.IncludeInActive);
			this.isExport = isExport;

			DefaultModuleFilterFields(fSupplier, fOwner);
		}
		readonly bool isIncludeInActive;

		protected OrgSupplierPartCollection(BusinessObjectFactory factory, OrgHeader supplier, OrgHeader owner, ZQuery filter)
			: this(factory, filter)
		{
			fOwner = owner;
			fSupplier = supplier;
			RemoveDuplicates = true;

			DefaultModuleFilterFields(fSupplier, fOwner);
		}

		public OrgSupplierPartCollection(BusinessObjectFactory factory, OrgHeader supplier, OrgHeader owner, bool forceSearchOnEnteringModule,
			ZString productDescription, ZString invoiceUnits, bool isExport)
			: this(factory, NewGetSupplierOwnerFilterDontResolveDuplicates(supplier, owner))
		{
			fOwner = owner;
			fSupplier = supplier;
			fProductDescription = productDescription;
			fInvoiceUnits = invoiceUnits;

			RemoveDuplicates = true;
			DefaultModuleFilterFields(fSupplier, fOwner);
			ForceSearchOnEnteringModule = forceSearchOnEnteringModule;
			this.isExport = isExport;
		}

		public OrgSupplierPartCollection(BusinessObjectFactory factory, OrgHeader supplier, OrgHeader owner, ZString productDescription, ZString stockKeepingUnits, bool isExport)
			: this(factory, supplier, owner, false, productDescription, stockKeepingUnits, isExport)
		{
		}

		#endregion

		public bool ForceSearchOnEnteringModule { get; protected set; }

		#region SetDefaultsForNewChild

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var part = (OrgSupplierPart)child;
			SetRelationships(part);

			if (!fProductDescription.IsEmpty)
			{
				part.OP_Desc = fProductDescription.Left(part.OP_DescInfo.MaxLength);
			}
			if (!fInvoiceUnits.IsEmpty)
			{
				part.OP_StockKeepingUnit = fInvoiceUnits;
			}
		}

		void SetRelationships(OrgSupplierPart part)
		{
			var partRelation = part.RelatedOrganisations.AddNew();
			using (partRelation.SuspendSettingHasChanges())
			{
				var newRelationship = GetNewRelationship();
				OrgHeader orgForRelationship;
				if (newRelationship.HasValue && (orgForRelationship = newRelationship.Value.Org) != null)
				{
					var relationship = newRelationship.Value.Relationship;
					partRelation.OU_OH = orgForRelationship.PK;
					partRelation.OU_Relationship = ShouldCreateRelationshipAsBoth(orgForRelationship, relationship) ? (ZString)OrgPartRelation.RelationshipTypes.Both : (ZString)relationship;
				}
				else
				{
					partRelation.OU_OH = ZGuid.Empty;
					partRelation.OU_Relationship = ZString.Empty;
				}
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected virtual (OrgHeader Org, string Relationship)? GetNewRelationship()
		{
			var supplierForRelationship = EffectiveSupplier(fSupplier);
			var ownerForRelationship = EffectiveOwner(fOwner);
			var link = GetSupplierImporterLink(ownerForRelationship, supplierForRelationship);
			var productRelation = link?.OL_ProductRelation ?? ZString.Empty;
			bool isImportRelation;

			if (!productRelation.IsEmpty && new OrgRelationTypeList().ContainsCode(productRelation))
			{
				isImportRelation = productRelation == OrgRelationTypeList.Codes.Importer;
			}
			else
			{
				isImportRelation = (ownerForRelationship != null) || supplierForRelationship == null;
			}

			return isImportRelation
				? (ownerForRelationship, OrgPartRelation.RelationshipTypes.Owner)
				: (supplierForRelationship, OrgPartRelation.RelationshipTypes.Supplier);
		}

		protected virtual bool ShouldCreateRelationshipAsBoth(OrgHeader orgForRelationship, string relationship)
		{
			return orgForRelationship.CountryData.OV_MakePartsBothImportAndExport;
		}

		OrgSupplierBuyerLink GetSupplierImporterLink(OrgHeader importer, OrgHeader supplier)
		{
			OrgSupplierBuyerLink result = null;
			if (importer != null && supplier != null)
			{
				RefCountry buyerCountry = RefCountry.OrganisationCountry(importer);
				ZString buyerCountryCode = buyerCountry != null ? buyerCountry.RN_Code : ZString.Empty;
				result = OrgSupplierBuyerLink.GetExistingOrgSupplierBuyerLink(supplier, importer, buyerCountryCode);
			}
			return result;
		}

		protected virtual OrgHeader EffectiveSupplier(OrgHeader supplier)
		{
			return supplier;
		}

		protected virtual OrgHeader EffectiveOwner(OrgHeader owner)
		{
			return owner;
		}
		#endregion

		public virtual void DefaultModuleFilterFields(OrgHeader supplier, OrgHeader owner)
		{
		}

		public static ZDBOnlyQuery GetSupplierOwnerFilterDontResolveDuplicates(OrgHeader supplier, OrgHeader owner, PartFilterOptions filterOptions, JoinCondition relationShipBetweenOwnerAndSupplier = null)
		{
			if (relationShipBetweenOwnerAndSupplier == null)
			{
				relationShipBetweenOwnerAndSupplier = JoinCondition.Or;
			}
			bool isOwnerMandatory = filterOptions.HasFlag(PartFilterOptions.OwnerMandatory);
			if (isOwnerMandatory)
			{
				Argument.NotNull(owner, "owner");
			}

			var queryFilter = new ZDBOnlyQuery(typeof(OrgSupplierPart));
			if (owner != null)
			{
				var subQueryOwnerParent = new ZDBOnlySubQuery(typeof(OrgRelatedParty), OrgRelatedPartySchema.PR_OH_Parent);
				subQueryOwnerParent.AddToFilter(OrgRelatedPartySchema.PR_OH_RelatedParty, owner.PK);
				subQueryOwnerParent.AddToFilter(OrgRelatedPartySchema.PR_PartyType, RelatedPartyTypeList.Codes.ProductRelationship);

				var subQueryOwner = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
				subQueryOwner.AddToFilter(OrgPartRelationSchema.OU_OH, owner.PK);
				subQueryOwner.AddSubQuery(OrgPartRelationSchema.OU_OH, subQueryOwnerParent, JoinCondition.Or);
				subQueryOwner.AddToFilter(JoinCondition.And, OrgPartRelationSchema.OU_Relationship,
					new[] { OrgPartRelation.RelationshipTypes.Owner, OrgPartRelation.RelationshipTypes.Both, OrgPartRelation.RelationshipTypes.ClassificationOrganization });
				
				queryFilter.AddSubQuery(subQueryOwner, relationShipBetweenOwnerAndSupplier);
			}

			if (supplier != null)
			{
				var subQuerySupplierParent = new ZDBOnlySubQuery(typeof(OrgRelatedParty), OrgRelatedPartySchema.PR_OH_Parent);
				subQuerySupplierParent.AddToFilter(OrgRelatedPartySchema.PR_OH_RelatedParty, supplier.PK);
				subQuerySupplierParent.AddToFilter(OrgRelatedPartySchema.PR_PartyType, RelatedPartyTypeList.Codes.ProductRelationship);

				var subQuerySupplier = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP);
				subQuerySupplier.AddToFilter(OrgPartRelationSchema.OU_OH, supplier.PK);
				subQuerySupplier.AddSubQuery(OrgPartRelationSchema.OU_OH, subQuerySupplierParent, JoinCondition.Or);
				subQuerySupplier.AddToFilter(JoinCondition.And, OrgPartRelationSchema.OU_Relationship, 
					new[] { OrgPartRelation.RelationshipTypes.Supplier, OrgPartRelation.RelationshipTypes.Both, OrgPartRelation.RelationshipTypes.ClassificationOrganization });

				if (isOwnerMandatory)
				{
					var noSupplierSubQuery = GetHasNoSupplierQuery(owner);
					var supplierQuery = new ZDBOnlyQuery(typeof(OrgSupplierPart));
					supplierQuery.AddSubQuery(subQuerySupplier, JoinCondition.Or);
					supplierQuery.AddSubQuery(noSupplierSubQuery, JoinCondition.Or);

					queryFilter.AddToFilter(supplierQuery);
				}
				else
				{
					queryFilter.AddSubQuery(subQuerySupplier, relationShipBetweenOwnerAndSupplier);
				}
			}

			if (supplier == null && owner == null)
			{
				queryFilter.IsNoResultQuery = true;
			}

			if (!filterOptions.HasFlag(PartFilterOptions.IncludeInActive))
			{
				queryFilter.AddToFilter(JoinCondition.And, OrgSupplierPartSchema.OP_IsActive, true);
			}
			if (filterOptions.HasFlag(PartFilterOptions.ExcludeNotForResale))
			{
				queryFilter.AddToFilter(JoinCondition.And, OrgSupplierPartSchema.OP_CanResell, true);
			}

			return queryFilter;
		}

		static ZDBOnlySubQuery GetHasNoSupplierQuery(OrgHeader owner)
		{
			var subQueryExcludeOwnerParent = new ZDBOnlySubQuery(typeof(OrgRelatedParty), OrgRelatedPartySchema.PR_OH_Parent, notIn: true);
			subQueryExcludeOwnerParent.AddToFilter(OrgRelatedPartySchema.PR_OH_RelatedParty, owner.PK);
			subQueryExcludeOwnerParent.AddToFilter(OrgRelatedPartySchema.PR_PartyType, RelatedPartyTypeList.Codes.ProductRelationship);

			// treat 'both' as supplier
			var bothAsSupplierSubQuery = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP, notIn: true);
			bothAsSupplierSubQuery.AddToFilter(OrgPartRelationSchema.OU_Relationship, OrgPartRelation.RelationshipTypes.Both);
			bothAsSupplierSubQuery.AddToFilter(OrgPartRelationSchema.OU_OH, SQLComparisonOperator.NotEqual, owner.PK);
			bothAsSupplierSubQuery.AddSubQuery(OrgPartRelationSchema.OU_OH, subQueryExcludeOwnerParent, JoinCondition.And);

			var noSupplierSubQuery = new ZDBOnlySubQuery(typeof(OrgPartRelation), OrgPartRelationSchema.OU_OP, notIn: true);
			noSupplierSubQuery.AddToFilter(OrgPartRelationSchema.OU_Relationship, OrgPartRelation.RelationshipTypes.Supplier);
			noSupplierSubQuery.AddAsUnionQuery(bothAsSupplierSubQuery, addAsUnionAll: true);

			return noSupplierSubQuery;
		}

		protected static ZDBOnlyQuery NewGetSupplierOwnerFilterDontResolveDuplicates(OrgHeader supplier, OrgHeader owner, bool loadOnlyActiveProducts = true)
		{
			if ((bool)ObjectFactory.Get<Enterprise.Integration.Customs.Shared.ICustomsDataRegistry>().EnableExactMatchForProduct.Value && supplier != null && owner != null)
			{
				return GetSupplierOwnerMatchesQuery(supplier.PK, owner.PK, loadOnlyActiveProducts);
			}
			else
			{
				return GetSupplierOwnerFilterDontResolveDuplicates(supplier, owner, loadOnlyActiveProducts ? PartFilterOptions.None : PartFilterOptions.IncludeInActive);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is text used in an SQL query")]
		public static ZDBOnlyQuery GetSupplierOwnerMatchesQuery(ZGuid supplier, ZGuid owner, bool loadOnlyActiveProducts = true)
		{
			var queryFilter = new ZDBOnlyQuery(typeof(OrgSupplierPart));

			string GetSelectParentSubQuery(string orgPKParam) => @$"
select PR_OH_Parent
from dbo.OrgRelatedParty 
where PR_OH_RelatedParty = {orgPKParam} and PR_PartyType = 'PPT'
";

			var supplierSelectClausePK = @$"ou_oh in (select @supplier UNION ALL {GetSelectParentSubQuery("@supplier")})";
			var importerSelectClausePK = @$"ou_oh in (select @importer UNION ALL {GetSelectParentSubQuery("@importer")})";
			var supplierSelectClauseRelation = @"ou_relationship in (@supplierRelationship, @bothRelationship, @clsRelationship)";
			var importerSelectClauseRelation = @"ou_relationship in (@ownerRelationship, @bothRelationship, @clsRelationship)";
			
			var supplierExcludeClause = @$"ou_relationship = @supplierRelationship or (ou_relationship = @bothRelationship and ou_oh not in (select @importer UNION ALL {GetSelectParentSubQuery("@importer")}))";
			var importerExcludeClause = @$"ou_relationship = @ownerRelationship or (ou_relationship = @bothRelationship and ou_oh not in (select @supplier UNION ALL {GetSelectParentSubQuery("@supplier")}))";

			var filter = @$"
op_pk in (select ou_op from dbo.orgpartrelation where {supplierSelectClausePK} or {importerSelectClausePK}) 
and op_pk in
(
	select op_pk from dbo.orgsupplierpart where op_pk not in
  (
		select ou_op from dbo.orgpartrelation where {supplierExcludeClause}
)
	union select ou_op from dbo.orgpartrelation where {supplierSelectClausePK} and {supplierSelectClauseRelation}
)
and op_pk in
(
	select op_pk from dbo.orgsupplierpart where op_pk not in
	(
		select ou_op from dbo.orgpartrelation where {importerExcludeClause}
	)
	union select ou_op from dbo.orgpartrelation where {importerSelectClausePK} and {importerSelectClauseRelation}
)
and (op_IsActive = 1 or op_IsActive = @inActive)
";

			var parameters = new ZSqlParameterCollection();
			parameters.Add("@supplier", supplier, OrgPartRelationSchema.OU_OH);
			parameters.Add("@importer", owner, OrgPartRelationSchema.OU_OH);
			parameters.Add("@active", "Y", OrgSupplierPartSchema.OP_IsActive);
			parameters.Add("@inActive", loadOnlyActiveProducts ? ZBool.True : ZBool.False, OrgSupplierPartSchema.OP_IsActive);
			parameters.Add("@supplierRelationship", OrgPartRelation.RelationshipTypes.Supplier, OrgPartRelationSchema.OU_Relationship);
			parameters.Add("@ownerRelationship", OrgPartRelation.RelationshipTypes.Owner, OrgPartRelationSchema.OU_Relationship);
			parameters.Add("@bothRelationship", OrgPartRelation.RelationshipTypes.Both, OrgPartRelationSchema.OU_Relationship);
			parameters.Add("@clsRelationship", OrgPartRelation.RelationshipTypes.ClassificationOrganization, OrgPartRelationSchema.OU_Relationship);
			queryFilter.AddFilterAndZSQLParameterCollection(filter, parameters);
			return queryFilter;
		}

		public readonly bool RemoveDuplicates;

		public override void Load()
		{
			base.Load();
			if (RemoveDuplicates)
			{
				RemoveSupplierPartsThatHaveCorrespondingBuyerParts();
			}
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			OrgSupplierPart part = (OrgSupplierPart)selectedBusinessObject;

			if (!part.OP_IsActive && IsParameterInAdditionalFilter(OrgSupplierPartSchema.OP_IsActive))
			{
				errors.Add(ProductIsNotActiveErrorMessage);
			}

			if (!part.OP_CanResell && IsParameterInAdditionalFilter(OrgSupplierPartSchema.OP_CanResell))
			{
				errors.Add(ProductIsNotForResaleErrorMessage);
			}
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery result = base.CreateAdditionalFilter();

			ZQuery additionalFilter = new ZQuery(ZArchitecture.Schema.OrgSupplierPartSchema.OP_PartNum, SQLComparisonOperator.NotEqual, "");

			if (result == null)
			{
				result = additionalFilter;
			}
			else
			{
				result.AddToFilter(additionalFilter);
			}

			return result;
		}

		public static string InvalidProductErrorMessage
		{
			get { return Res.GetString("e241bcc1-99bd-4b52-890c-662674b6fa26", "Please select a valid product."); }
		}
		public static string ProductIsNotActiveErrorMessage
		{
			get { return Res.GetString("580f59b7-41b1-42b5-98ae-76afffefcc21", "This product cannot be selected because it is inactive"); }
		}
		public static string ProductIsNotForResaleErrorMessage
		{
			get { return Res.GetString("abb82e55-5f41-4c48-aa60-09c386b6d41c", "This product cannot be selected because it is not for resale"); }
		}

		#region Implementation

		protected readonly OrgHeader fOwner, fSupplier;
		protected readonly ZString fProductDescription, fInvoiceUnits;
		protected readonly bool isExport;

		bool IsParameterInAdditionalFilter(SchemaColumn parameterSchemaColumn)
		{
			foreach (ZSqlParameter parameter in AdditionalFilter.Params)
			{
				if (parameter.SchemaColumn == parameterSchemaColumn)
				{
					return true;
				}
			}
			return false;
		}

		protected void RemoveSupplierPartsThatHaveCorrespondingBuyerParts()
		{
			for (int i = Count - 1; i >= 0; i--)
			{
				var part = this[i];

				var foundSupplierRelation = part.RelatedOrganisations.FindByOrganisationAndRelationship(fSupplier, OrgPartRelation.RelationshipTypes.Supplier);
				if (foundSupplierRelation != null && ContainsPartForOwner(part))
				{
					Remove(part);
				}
			}
		}

		protected bool ContainsPartForOwner(OrgSupplierPart partToCompare)
		{
			foreach (OrgSupplierPart part in this)
			{
				if (part.PK != partToCompare.PK && part.OP_PartNum == partToCompare.OP_PartNum)
				{
					var foundOwnerRelation = part.RelatedOrganisations.FindByOrganisationAndRelationship(fOwner, OrgPartRelation.RelationshipTypes.Owner);
					if (foundOwnerRelation != null)
					{
						return true;
					}
				}
			}

			return false;
		}

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return fOwner == null && fSupplier == null ? base.FindBoxListProvider : new OrgSupplierPartFindBoxListProvider(this, fOwner, fSupplier, isIncludeInActive, isExport); }
		}

		protected class OrgSupplierPartFindBoxListProvider : FindBoxListProvider
		{
			public OrgSupplierPartFindBoxListProvider(BusinessObjectCollection list, OrgHeader owner, OrgHeader supplier, bool includeInActive, bool isExport)
				: base(list)
			{
				fOwner = owner;
				fSupplier = supplier;
				IncludeInActive = includeInActive;
				this.isExport = isExport;
			}

			protected readonly OrgHeader fOwner, fSupplier;
			protected readonly bool isExport;
			readonly bool IncludeInActive;

			#region Part From Code

			protected override IEnumerable<BusinessObject> BizObjsFromCodeWithRelationshipFilter(string code)
			{
				var result = new OrgSupplierPart.Loader(List.Factory).Load(code, fOwner, fSupplier, false, IncludeInActive, isExport);
				return (result != null) ? new BusinessObject[] { result } : Enumerable.Empty<BusinessObject>();
			}

			protected override IEnumerable<BusinessObject> BizObjsFromCodeWithCompleteFilter(string code)
			{
				return BizObjsFromCodeWithRelationshipFilter(code);
			}

			#endregion
		}

		#endregion
	}
}
