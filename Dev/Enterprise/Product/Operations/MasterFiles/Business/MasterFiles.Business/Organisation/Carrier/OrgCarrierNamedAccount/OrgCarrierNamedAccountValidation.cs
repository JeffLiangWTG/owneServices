//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoOrgCarrierNamedAccountValidation
//
//    This class should be used for overriding validation in AutoOrgCarrierNamedAccountValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCarrierNamedAccountValidation : AutoOrgCarrierNamedAccountValidation
	{
		public OrgCarrierNamedAccountValidation(AutoOrgCarrierNamedAccount parent) : base(parent)
		{
		}

		protected override void CheckONA_ForeignName()
		{
			if (!RatingFeatureHelper.Urs.IsEnabled)
			{
				return;
			}
			MandatoryValidation.CheckEntered(Parent.ONA_ForeignNameInfo);

			if (Parent.ONA_ForeignName.IsEmpty)
			{
				return;
			}

			if (!OrgCarrierNamedAccountLookups.GetCachedNamedAccountsList(Parent.Factory).Contains(Parent.ONA_ForeignName))
			{
				Parent.ONA_ForeignNameInfo.AddError(Res.GetString("14145dfc-dfb6-4edd-95d5-059d33107c91", "Foreign Name is not valid."));
				return;
			}

			var collection = Parent.Factory.Load<OrgCarrierNamedAccount>(new ZQuery(OrgCarrierNamedAccountSchema.ONA_OH_Carrier, Parent.ONA_OH_Carrier.ToSqlParameter()));
			var overlap = collection.FirstOrDefault(bo => bo.PK != Parent.PK && !bo.IsDeleted && bo.Organization != null && bo.ONA_ForeignName.ToString().Equals(Parent.ONA_ForeignName, StringComparison.OrdinalIgnoreCase));

			if (overlap != null)
			{
				Parent.ONA_ForeignNameInfo.AddError(MustBeUniqueMessage(Parent.Carrier, overlap, Parent.ONA_ForeignName));
			}
		}

		static string MustBeUniqueMessage(OrgHeader carrier, OrgCarrierNamedAccount overlap, ZString foreignName)
		{
			var errorMessage = Res.GetString("03975c36-298e-4a84-b138-c0cb7cd90d82", "Named account {0} is already mapped to an Organization {1}, under no carrier.", foreignName, overlap.Organization.OH_Code);
			if (carrier != null)
			{
				errorMessage = Res.GetString("34c50d55-30ef-4c93-b689-37b4b15d3094", "Named account {0} is already mapped to an Organization {1}, under carrier {2}.", foreignName, overlap.Organization.OH_Code, carrier.OH_Code);
			}
			return errorMessage;
		}

		protected override void CheckONA_OH_Organization()
		{
			if (!RatingFeatureHelper.Urs.IsEnabled)
			{
				return;
			}
			if (Parent.Organization != null)
			{
				if (!Parent.Organization.OH_IsConsignee && !Parent.Organization.OH_IsConsignor && !Parent.Organization.OH_IsControllingCustomer)
				{
					Parent.ONA_OH_OrganizationInfo.AddError(Res.GetString("7617101f-55cb-4f89-aaa7-12fc9216fc23", "The entered value does not correspond to a valid Organization of type Consignor, Consignee or Controlling Customer."));
				}
			}
		}
	}
}
