using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Integration.Rating;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class RelatedActivityLinkLookupsTest : BusinessObjectLookupsTestCase
	{
		#region RelatableActivityTypes

		public void TestRelatableActivityTypes()
		{
			var lookups = GetNewLookups();
			AssertNotNull(lookups.RelatableActivityTypes);
			Assert(lookups.RelatableActivityTypeDefinitions.ContainsKey(RelatableActivityTypeList.Codes.CrmOpportunityManager));
			Assert(!lookups.RelatableActivityTypes.ToList<ICodeDescription>().Select(x => x.Code).Contains(RelatableActivityTypeList.Codes.CrmOpportunityManager));
		}

		#endregion

		#region RelatedActivityCollection

		public void TestRelatedActivityCollection()
		{
			var lookups = GetNewLookups();

			lookups.Link.ToActivityTypeForBinding = "";
			AssertNull(lookups.ToActivityCollection);

			lookups.Link.ToActivityTypeForBinding = RelatableActivityTypeList.Codes.Communication;
			AssertNotNull(lookups.ToActivityCollection);
		}

		#endregion

		#region RelatableActivityTypeDefinitions

		public void TestRelatableActivityTypeDefinitions_AllCollectionTypesHaveCorrectModuleId()
		{
			var lookups = GetNewLookups();

			CombineAssertions("All relatable activity collections should have correct ModuleId", () =>
			{
				foreach (var typeInfo in lookups.RelatableActivityTypeDefinitions)
				{
					var collection = typeInfo.Value.GetNewCollection();
					AssertNotNull(string.Format("RelatedActivityCollection for Type:{0}", typeInfo.Key), collection);
					if (collection != null)
					{
						AssertEquals(string.Format("ModuleId for Type:[{0}] Collection:[{1}]", typeInfo.Key, collection.GetType()), typeInfo.Value.ModuleId, ZMetaData.GetModuleId(collection));
					}
				}
			});
		}

		public void TestRelatableActivityTypeDefinitions_CanCreateAndLoadAllTypes()
		{
			var lookups = GetNewLookups();

			CombineAssertions("All relatable activity types should be able to be created and loaded", () =>
			{
				foreach (var typeInfo in lookups.RelatableActivityTypeDefinitions)
				{
					var activity = CreateNewRelatableActivity(typeInfo.Value);
					if (activity != null)
					{
						var pivot = Factory.New<ViewRelatedActivityPivot>();
						pivot.ParentActivity = activity;
						AssertNotNull(string.Format("Loaded ParentActivity for type:[{0}]", typeInfo.Key), pivot.ParentActivity);

						pivot.ChildActivity = activity;
						AssertNotNull(string.Format("Loaded ChildActivity for type:[{0}]", typeInfo.Key), pivot.ChildActivity);
					}
				}
			});
		}

		public void TestRelatableActivityTypeDefinitions_AllElementTypesHaveCorrectTablePrefix()
		{
			var lookups = GetNewLookups();

			CombineAssertions("All relatable activity types should have correct TablePrefix", () =>
			{
				foreach (var typeInfo in lookups.RelatableActivityTypeDefinitions)
				{
					var bizObj = Factory.New(typeInfo.Value.ElementType);
					AssertEquals(string.Format("Table prefix for type:[{0}]", typeInfo.Key), typeInfo.Value.TablePrefix, bizObj.TablePrefix);
				}
			});
		}

		public void TestRelatableActivityTypeDefinitions_AllElementTypesImplementIRelatableActivity()
		{
			var lookups = GetNewLookups();

			CombineAssertions("All relatable activity element types should implement IRelatableActivity", () =>
			{
				foreach (var typeInfo in lookups.RelatableActivityTypeDefinitions)
				{
					var elementType = typeInfo.Value.ElementType;
					Assert(string.Format("Element type:[{0}] should implement {1}", elementType.FullName, typeof(IRelatableActivity).FullName), typeof(IRelatableActivity).IsAssignableFrom(elementType));
				}
			});
		}

		#endregion

		#region TablePrefixesForSuperAndSubActivities

		public void TestTablePrefixesForSuperAndSubActivities()
		{
			var lookups = GetNewLookups();

			CombineAssertions("All activities implementing ISuperRelatableActivity should be included in GetTablePrefixesForSuperAndSubActivities", () =>
			{
				var actualSuperActivityTablePrefixes = RelatedActivityLinkLookups.GetTablePrefixesForSuperAndSubActivities(Factory).Keys;

				foreach (var typeInfo in lookups.RelatableActivityTypeDefinitions)
				{
					var elementType = typeInfo.Value.ElementType;
					var tablePrefix = typeInfo.Value.TablePrefix;
					if (typeof(ISuperRelatableActivity).IsAssignableFrom(elementType))
					{
						AssertCollectionContains(string.Format("Element type:[{0}] TabePrefix:[{1}]", elementType, tablePrefix), tablePrefix, actualSuperActivityTablePrefixes);
					}
					else
					{
						AssertCollectionNotContains(string.Format("Element type:[{0}] TabePrefix:[{1}]", elementType, tablePrefix), tablePrefix, actualSuperActivityTablePrefixes);
					}
				}
			});
		}

		#endregion

		#region Implementation

		IRelatableActivity CreateNewRelatableActivity(RelatableActivityTypeDefinition definition)
		{
			var elementType = definition.ElementType;

			BusinessObject bizObj = null;
			AssertNoExceptionThrown(string.Format("Could not create new element type:[{0}] for activity type:[{1}]", definition.Description, elementType.FullName), () =>
			{
				bizObj = Factory.NewWithValidTestData(elementType);
				AssertNotNull(string.Format("Created bizObj for type:[{0}]", definition.Description), bizObj);
			});

			if (definition.ModuleId == ModuleIDs.DtbBooking)
			{
				var consolidation = (BusinessObject)Factory.New<IDtbBookingConsolidation>();
				consolidation[DtbBookingConsolidationSchema.KB_JobType] = "BKG";
				bizObj[DtbBookingSchema.KM_KB_Booking] = consolidation.PK;
			}

			if (definition.ModuleId == ModuleIDs.OneOffQuotes)
			{
				var quote = Factory.New(ObjectFactory.Get<IRating>().QuoteType);
				quote[RatingHeaderSchema.TH_RateType] = "QTE";
				quote[RatingHeaderSchema.TH_OneTimeQuote] = ZBool.True;
				bizObj[ViewQuotedBookingSchema.VB_TH] = quote.PK;
			}

			return (IRelatableActivity)bizObj;
		}

		protected virtual RelatedActivityLinkLookups GetNewLookups()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			var inquiry = Factory.New<SalesEnquiry>();
			var pivot = ViewRelatedActivityPivot.Create(Factory, opportunity, inquiry);
			return RelatedActivityLinkLookups.New(RelatedActivityLink.Get(pivot, opportunity));
		}

		#endregion
	}
}
