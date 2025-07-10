using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefDocTypeFindboxCollection))]
	sealed class RefDocTypeFindboxCollectionTest : ActiveBusinessObjectCollectionTestCase<RefDocTypeFindboxCollection>
	{
		public void TestAddNewElementDefaultsToDefaultReferenceType()
		{
			RefDocTypeFindboxCollection collection = new RefDocTypeFindboxCollection(Factory, "XXX");
			RefDocType docType = collection.AddNew();
			AssertEquals("docType.RT_ReferenceType", "XXX", docType.RT_ReferenceType);
		}

		public void TestLikeAUserWould()
		{
			RefDocTypeCollection collectionForDelete = new RefDocTypeCollection(Factory);
			collectionForDelete.DeleteAll();

			RefDocType sclDocType = Factory.New<RefDocType>();
			sclDocType.RT_ReferenceType = "SCL";
			RefDocType allDocType = Factory.New<RefDocType>();
			allDocType.RT_ReferenceType = "ALL";
			RefDocType fftDocType = Factory.New<RefDocType>();
			fftDocType.RT_ReferenceType = "FFT";

			RefDocTypeFindboxCollection collectionForSCL = new RefDocTypeFindboxCollection(Factory, "SCL");
			ActiveBusinessObjectCollectionFindboxModuleSimulator moduleSimulatorForSCL = new ActiveBusinessObjectCollectionFindboxModuleSimulator(collectionForSCL);
			moduleSimulatorForSCL.PerformSearch();

			AssertEquals("tester.GridCollection.Count", 3, moduleSimulatorForSCL.GridCollection.Count);
			AssertCollectionContains(sclDocType, moduleSimulatorForSCL.GridCollection);
			AssertCollectionContains(allDocType, moduleSimulatorForSCL.GridCollection);
			AssertCollectionContains(fftDocType, moduleSimulatorForSCL.GridCollection);

			AssertEquals("sclDocType.RowNotifications", "", sclDocType.RowNotifications.ToMessageListString());
			AssertEquals("allDocType.RowNotifications", "", allDocType.RowNotifications.ToMessageListString());
			AssertEquals("fftDocType.RowNotifications", "A Document Type selected from here must have a Reference Type of [SCL] or [ALL] selected.", fftDocType.RowNotifications.ToMessageListString());
			((NotificationCollection)fftDocType.RowNotifications).Clear();

			RefDocTypeFindboxCollection collectionForOTH = new RefDocTypeFindboxCollection(Factory, "OTH");
			ActiveBusinessObjectCollectionFindboxModuleSimulator moduleSimulatorForOTH = new ActiveBusinessObjectCollectionFindboxModuleSimulator(collectionForOTH);
			moduleSimulatorForOTH.PerformSearch();

			AssertEquals("tester.GridCollection.Count", 3, moduleSimulatorForOTH.GridCollection.Count);
			AssertCollectionContains(sclDocType, moduleSimulatorForOTH.GridCollection);
			AssertCollectionContains(allDocType, moduleSimulatorForOTH.GridCollection);
			AssertCollectionContains(fftDocType, moduleSimulatorForOTH.GridCollection);

			AssertEquals("sclDocType.RowNotifications", "A Document Type selected from here must have a Reference Type of [OTH] or [ALL] selected.", sclDocType.RowNotifications.ToMessageListString());
			AssertEquals("allDocType.RowNotifications", "", allDocType.RowNotifications.ToMessageListString());
			AssertEquals("fftDocType.RowNotifications", "A Document Type selected from here must have a Reference Type of [OTH] or [ALL] selected.", fftDocType.RowNotifications.ToMessageListString());
			((NotificationCollection)sclDocType.RowNotifications).Clear();
			((NotificationCollection)fftDocType.RowNotifications).Clear();

			RefDocTypeFindboxCollection collectionForEmpty = new RefDocTypeFindboxCollection(Factory, ZString.Empty);
			ActiveBusinessObjectCollectionFindboxModuleSimulator moduleSimulatorForEmpty = new ActiveBusinessObjectCollectionFindboxModuleSimulator(collectionForEmpty);
			moduleSimulatorForEmpty.PerformSearch();

			AssertEquals("tester.GridCollection.Count", 3, moduleSimulatorForEmpty.GridCollection.Count);
			AssertCollectionContains(sclDocType, moduleSimulatorForEmpty.GridCollection);
			AssertCollectionContains(allDocType, moduleSimulatorForEmpty.GridCollection);
			AssertCollectionContains(fftDocType, moduleSimulatorForEmpty.GridCollection);

			AssertEquals("sclDocType.RowNotifications", "A Document Type selected from here must have a Reference Type of [ALL] selected.", sclDocType.RowNotifications.ToMessageListString());
			AssertEquals("allDocType.RowNotifications", "", allDocType.RowNotifications.ToMessageListString());
			AssertEquals("fftDocType.RowNotifications", "A Document Type selected from here must have a Reference Type of [ALL] selected.", fftDocType.RowNotifications.ToMessageListString());
		}

		public void TestDocTypeOfAllIsIncludedInCollection()
		{
			RefDocType sclDocType = Factory.New<RefDocType>();
			sclDocType.RT_ReferenceType = "XXX";

			RefDocType allDocType = Factory.New<RefDocType>();
			allDocType.RT_ReferenceType = "ALL";

			RefDocTypeFindboxCollection collection = new RefDocTypeFindboxCollection(Factory, "XXX");
			AssertCollectionContains(sclDocType, collection);
			AssertCollectionContains(allDocType, collection);

			collection = new RefDocTypeFindboxCollection(Factory, "YYY");
			AssertCollectionNotContains(sclDocType, collection);
			AssertCollectionContains(allDocType, collection);
		}

		public void TestCollectionIsFilteredOnLoad()
		{
			RefDocTypeFindboxCollection collection = new RefDocTypeFindboxCollection(Factory, "SCL");

			foreach (RefDocType docType in collection)
			{
				Assert("A DocType of different RefType was found in the collection - should have been filtered out - [" + docType.RT_ReferenceType + "]"
					, docType.RT_ReferenceType == "SCL" || docType.RT_ReferenceType == "ALL");
			}

			collection = new RefDocTypeFindboxCollection(Factory, ZString.Empty);

			foreach (RefDocType docType in collection)
			{
				AssertEquals("A DocType of different RefType was found in the collection - should have been filtered out", "ALL", docType.RT_ReferenceType);
			}
		}

		public void TestRefDocTypes_FreightRelatedTypesDisabledWhenProductivityWiseModeIsEnabled()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = false;

			var prePWCollectionOfAlls = new RefDocTypeFindboxCollection(Factory, "ALL").Select(type => type.RT_ReferenceType);
			var prePWCollectionOfTypes = new RefDocTypeFindboxCollection(Factory, "SCL").Select(type => type.RT_ReferenceType);

			AssertCollectionContains("We should have only doctypes with All referencetypes, and yet...",
				Enterprise.Core.Constants.ReferenceTypes.All,
				prePWCollectionOfAlls);

			AssertCollectionContains("We should have doctypes with Freight-related referencetypes, and yet...",
				Enterprise.Core.Constants.ReferenceTypes.All,
				prePWCollectionOfTypes);

			AssertCollectionContains("We should have doctypes with Freight-related referencetypes, and yet...",
				Enterprise.Core.Constants.ReferenceTypes.SupplyChainLogistics,
				prePWCollectionOfTypes);

			DataRegistry.Instance.ProductivityWiseModeEnabled = true;

			var postPWCollectionOfAlls = new RefDocTypeFindboxCollection(Factory, "ALL").Select(type => type.RT_ReferenceType);
			var postPWCollectionOfTypes = new RefDocTypeFindboxCollection(Factory, "SCL").Select(type => type.RT_ReferenceType);

			AssertCollectionNotContains("We should have no doctypes with Freight-related referencetypes, and yet...",
				new ZString[] { Enterprise.Core.Constants.ReferenceTypes.ClientSupplierRelationship,
								Enterprise.Core.Constants.ReferenceTypes.SupplyChainLogistics },
				postPWCollectionOfAlls);

			AssertCollectionNotContains("We should have no doctypes with Freight-related referencetypes, and yet...",
				new ZString[] { Enterprise.Core.Constants.ReferenceTypes.ClientSupplierRelationship,
								Enterprise.Core.Constants.ReferenceTypes.SupplyChainLogistics },
				postPWCollectionOfTypes);
		}

		#region Implementation

		protected override RefDocTypeFindboxCollection GetCollectionToTest()
		{
			return new RefDocTypeFindboxCollection(Factory, "SHP");
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			RefDocType result = Factory.New<RefDocType>();
			result.RT_ReferenceType = "SHP";
			return result;
		}

		class ActiveBusinessObjectCollectionFindboxModuleSimulator
		{
			internal ActiveBusinessObjectCollectionFindboxModuleSimulator(IActiveBusinessObjectCollection collection)
			{
				GridCollection = collection.Clone();
				rowValidationFilter = collection.AdditionalFilter;
				rowValidationFilter.ModificationsEnabled = false;
				GridCollection.IncrementReadOnlyIncludingChildren();
				GridCollection.AdditionalFilter = new ZQuery();
			}
			readonly ZQuery rowValidationFilter;

			internal readonly IActiveBusinessObjectCollection GridCollection;

			internal void PerformSearch()
			{
				foreach (BusinessObject bizObj in GridCollection)
				{
					if (!bizObj.MatchesFilter(rowValidationFilter))
					{
						using (bizObj.ResumeValidationTemporarily())
						{
							bizObj.AddRowError(GridCollection.GetAllNotificationsWhenAdditionalFilterNotMet(bizObj));
						}
					}
				}
			}
		}

		#endregion
	}
}
