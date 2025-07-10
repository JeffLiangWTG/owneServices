using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusClassPartPivotRefUniqueIndexFailureHandlerTest : TestCaseWithFactory
	{
		public void TestConflictResolution()
		{
			Factory.RefreshEnabled = false;
			var anotherFactory = new BusinessObjectFactory { RefreshEnabled = false };

			((IBusinessObjectFactoryInternals)anotherFactory).DisableQueryCacheReset = true;

			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			part.OP_PartNum = "111";
			var pivot = part.PivotsForBinding.AddNew();

			Factory.Save();

			var pivotRef = CreatePivotRef(Factory, pivot, "AGA", "REF123");
			var partInAnotherFactory = anotherFactory.Load<OrgSupplierPart>(part.PK);
			var pivotRefInAnotherFactory = CreatePivotRef(anotherFactory, partInAnotherFactory.PivotsForBinding[0], "AGA", "REF123");

			Factory.Save();

			((IBusinessObjectFactoryInternals)anotherFactory).DisableQueryCacheReset = false;

			CombineAssertions(() =>
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				try
				{
					anotherFactory.Save();
					Fail("First save should not have succeeded.");
				}
				catch (Exception ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}

				AssertEquals("User should have been notified", false, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("Message to notify user", $@"CusClassPartPivotRef (REF123) has already been entered on Product (111) by another user ({part.OP_SystemLastEditUser} @ {part.OP_SystemLastEditTimeUtc.ToSmallDateTimeFloor()}). Your changes have been merged, please review your changes and save again.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Pivot Ref 2 should be deleted", true, pivotRefInAnotherFactory.IsDeleted);

				anotherFactory.Save();

				var query = new ZQuery();
				query.AddToFilter(CusClassPartPivotRefSchema.CIR_CI, pivot.PK);
				query.AddToFilter(CusClassPartPivotRefSchema.CIR_ReferenceType, "AGA");
				query.AddToFilter(CusClassPartPivotRefSchema.CIR_ReferenceNumber, "REF123");
				AssertEquals("Should be using the existing Pivot Ref now", pivotRef.PK, anotherFactory.Load<CusClassPartPivotRef>(query).Single().PK);
			});
		}

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CusClassPartPivotRefUniqueIndexFailureHandler(null));
		}

		public void TestHandledUniqueIndexNames()
		{
			AssertEquals(CusClassPartPivotRefSchema.Constants.Indexes.FK_UC__CIR_CI_CIR_ReferenceType_CIR_ReferenceNumber, new CusClassPartPivotRefUniqueIndexFailureHandler(Factory.New<CusClassPartPivotRef>()).HandledUniqueIndexNames.Single());
		}

		static CusClassPartPivotRef CreatePivotRef(BusinessObjectFactory factory, BaseCusClassPartPivot partPivot, string referenceType, string number)
		{
			var result = factory.New<CusClassPartPivotRef>();
			result.CIR_CI = partPivot.PK;
			result.CIR_ReferenceType = referenceType;
			result.CIR_ReferenceNumber = number;
			return result;
		}
	}
}
