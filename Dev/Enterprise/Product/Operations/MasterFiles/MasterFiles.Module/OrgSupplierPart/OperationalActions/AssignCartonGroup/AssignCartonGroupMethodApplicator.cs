using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.MasterFiles.Module
{
	public class AssignCartonGroupMethodApplicator : OperationalActionMethodApplicator
	{
		public AssignCartonGroupMethodApplicator(string name, BusinessObjectFactory factory)
			: base(name, factory)
		{
		}

		#region CartonGroupPK

		[List("Lookups.CartonGroups")]
		[ResourceStringData("AssignCartonGroupMethodApplicator|CartonGroupPK", Caption = "Carton Group")]
		public ZGuid CartonGroupPK
		{
			get { return cartonGroupPK; }
			set
			{
				SetNonPersistentPropertyValue(CartonGroupPKInfo, ref cartonGroupPK, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateCartonGroupPK();
				}
			}
		}

		public ZPropertyInfo CartonGroupPKInfo
		{
			get { return GetZPropertyInfo(nameof(CartonGroupPK)); }
		}

		ZGuid cartonGroupPK;

		#endregion

		#region OrganisationPK

		[List("Lookups.Organisations")]
		[ResourceStringData("AssignCartonGroupMethodApplicator|OrganisationPK", Caption = "Organization")]
		public ZGuid OrganisationPK
		{
			get { return organisationPK; }
			set
			{
				SetNonPersistentPropertyValue(OrganisationPKInfo, ref organisationPK, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateOrganisationPK();
				}
			}
		}

		public ZPropertyInfo OrganisationPKInfo
		{
			get { return GetZPropertyInfo(nameof(OrganisationPK)); }
		}

		ZGuid organisationPK;

		#endregion

		#region ApplyCore

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] products)
		{
			Validation.ValidateAll();

			if (!HasErrors)
			{
				log.SetSectionProgressMax(products.Length);

				var isUnassigning = CartonGroupPK.IsEmpty;

				foreach (OrgSupplierPart product in products)
				{
					log.BumpSectionProgress();

					var relationship = isUnassigning
						? product.RelatedOrganisations.FindByOrganisationPKAndRelationship(OrganisationPK, OrgPartRelation.RelationshipTypes.Owner)
						: product.RelatedOrganisations.AddOrganisationIfNotExist(OrganisationPK, OrgPartRelation.RelationshipTypes.Owner);

					if (relationship != null)
					{
						relationship.OU_WCG_CartonGroup = CartonGroupPK;

						var message = isUnassigning
							? Res.GetString("3bc99b53-9932-4562-89f6-50970e5d75da", "Unassigned Carton Group from {0}.", product.OP_PartNum)
							: Res.GetString("73c1625f-9bc8-45e2-891f-b4e62bc7a1cd", "Assigned Carton Group to {0}.", product.OP_PartNum);

						log.Notify(OperationalActionLogErrorLevel.Informational, message);
					}
					else if (isUnassigning) // relationship should only ever be null if unassigning from a relation that doesn't exist, but this check here is for safety
					{
						log.Notify(OperationalActionLogErrorLevel.Informational, Res.GetString("9ff047a7-8774-4b85-a058-3ac64faa7bdb", "No Relationship to remove Carton Group from for {0}.", product.OP_PartNum));
					}
				}
			}
			else
			{
				log.Notify(OperationalActionLogErrorLevel.Error, Res.GetString("d8510729-41a4-4980-b542-fa5be654c94b", "Enter an Organization."));
			}
		}

		#endregion

		#region Validation

		public AssignCartonGroupValidation Validation
		{
			get { return new AssignCartonGroupValidation(this); }
		}

		#endregion

		#region Lookups

		public AssignCartonGroupLookups Lookups
		{
			get { return lookups ?? (lookups = new AssignCartonGroupLookups(this)); }
		}

		AssignCartonGroupLookups lookups;

		#endregion
	}
}
