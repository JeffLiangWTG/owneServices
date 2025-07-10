using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyPrincipalCollection))]
	internal class AgencyPrincipalCollectionTest : AgencyAllocationItemCollectionTest<AgencyPrincipalCollection, AgencyPrincipal, OrgHeader>
	{
		public override void TestRemoveFromRelationship()
		{
			BusinessObject bizO1 = GetNewElementToAddToTheCollection();
			BusinessObject bizO2 = GetNewElementToAddToTheCollection();
			Collection.AddRange(bizO1, bizO2);
			Collection.Remove(bizO1);
			AssertEquals("Collection count", 1, Collection.Count);
			Assert("Contains element 2", Collection.Contains(bizO2));
			Assert("Doesn't contain element 1", !Collection.Contains(bizO1));
		}

		#region Implementation
		AgencyCountry schedule;
		AgencyCountry Schedule
		{
			get
			{
				if (schedule == null)
				{
					JobVoyage voyage = Factory.New<JobVoyage>();
					schedule = new AgencyCountry(voyage, new RefCountry.Loader(Factory).LoadForCountry("AU"));
				}

				return schedule;
			}
		}

		protected override BusinessObjectCollection GetCollectToWrap()
		{
			return Schedule.Principals;
		}

		protected override OrgHeader GetNewInnerElement()
		{
			return Factory.New<OrgHeader>();
		}

		protected override AgencyPrincipalCollection WrapCollection(BusinessObjectCollection collectionToWrap)
		{
			return new AgencyPrincipalCollection(Schedule);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AgencyPrincipal(Factory);
		}
		#endregion
	}
}
