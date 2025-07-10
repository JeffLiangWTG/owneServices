using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgAllocatedContactCollection : DependentBusinessObjectCollection<OrgContact, OrgHeader>
	{
		public OrgAllocatedContactCollection(OrgHeader organisation)
			: base(organisation)
		{
			this.organisation = organisation;
		}
		readonly OrgHeader organisation;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		protected override ZQuery CreateAdditionalFilter()
		{
			/// if contact has a OrgContactAllocationCollection
			/// then include in this collection
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgContact));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgContactAllocation), Enterprise.ZArchitecture.Schema.OrgContactAttributeSchema.PC_OC);

			var allocationList = DataRegistry.Instance.OrgListOfContactAllocations;
			foreach (ICodeDescription attributeValue in allocationList)
			{
				subQuery.AddToFilter(JoinCondition.Or, OrgContactAttributeSchema.PC_Type, SQLComparisonOperator.Equal, attributeValue.Code);
			}

			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		public OrgContact GetAllocatedContact(string code)
		{
			foreach (var contact in organisation.Contacts.Where(c => c.OC_IsActive))
			{
				foreach (OrgContactAllocation contactAllocation in contact.Allocations)
				{
					if (contactAllocation.PC_Type == code)
					{
						return contact;
					}
				}
			}

			return null;
		}
	}
}
