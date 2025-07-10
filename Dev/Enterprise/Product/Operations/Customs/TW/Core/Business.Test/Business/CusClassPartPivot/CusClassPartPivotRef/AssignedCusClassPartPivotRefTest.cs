using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.TW;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(AssignedCusClassPartPivotRef))]
	sealed class AssignedCusClassPartPivotRefTest : EnterpriseBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestConflictResolution()
		{
			Factory.RefreshEnabled = false;
			var anotherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			((IBusinessObjectFactoryInternals)anotherFactory).DisableQueryCacheReset = true;
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			part.OP_PartNum = "111";
			var pivot = part.PivotsForBinding.AddNew();
			Factory.Save();
			var pivotRef = CreatePivotRef(pivot, "REF123");
			var partInAnotherFactory = anotherFactory.Load<OrgSupplierPart>(part.PK);
			var pivotInAnotherFactory = partInAnotherFactory.PivotsForBinding[0];
			var pivotRefInAnotherFactory = CreatePivotRef(pivotInAnotherFactory, "REF123");
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

				NUnit.Framework.Assert.That(UnitTestUserNotification.Instance.LastMessage.WasNone, NUnit.Framework.Is.EqualTo(false), "User should have been notified");
				NUnit.Framework.Assert.That(UnitTestUserNotification.Instance.LastMessage.Text, NUnit.Framework.Is.EqualTo($@"Assigned Number (REF123) has already been entered on Product (111) by another user ({part.OP_SystemLastEditUser} @ {part.OP_SystemLastEditTimeUtc.ToSmallDateTimeFloor()}). Your changes have been merged, please review your changes and save again."), "Message to notify user");
				NUnit.Framework.Assert.That(pivotRefInAnotherFactory.IsDeleted, NUnit.Framework.Is.EqualTo(true), "Pivot Ref 2 should be deleted");
				anotherFactory.Save();
				NUnit.Framework.Assert.That(pivotInAnotherFactory.AssignedCusClassPartPivotRefCollection.Single().PK, NUnit.Framework.Is.EqualTo(pivotRef.PK), "Should be using the existing Pivot Ref now");
			}

			);
		}

		[ExpectNoExceptions]
		public void TestHumanReadableName()
		{
			NUnit.Framework.Assert.That(PivotRefs.HumanReadableName, NUnit.Framework.Is.EqualTo("Assigned Number").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestSetDefaultValues()
		{
			var cusClassPartPivotRef = Factory.New<AssignedCusClassPartPivotRef>();
			NUnit.Framework.Assert.That(cusClassPartPivotRef.CIR_ReferenceType, NUnit.Framework.Is.EqualTo(JobComInvLineRefsType.Codes.AssignedNumber).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestValidation()
		{
			NUnit.Framework.Assert.That(PivotRefs.Validation.GetType(), NUnit.Framework.Is.EqualTo(typeof(AssignedCusClassPartPivotRefValidation)));
		}

		[ExpectNoExceptions]
		public void TestPivot()
		{
			NUnit.Framework.Assert.That(PivotRefs.CusClassPartPivot, NUnit.Framework.Is.SameAs(Pivot));
		}

		protected override BusinessObject GetNewBusinessObject() => PivotRefs;
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return CreatePivotRef(CreatePivot(factory), "123");
		}

		AssignedCusClassPartPivotRef PivotRefs => pivotRefs ?? (pivotRefs = CreatePivotRef(Pivot, "123"));
		AssignedCusClassPartPivotRef pivotRefs;
		CusClassPartPivot Pivot => pivot ?? (pivot = CreatePivot(Factory));
		CusClassPartPivot pivot;
		static CusClassPartPivot CreatePivot(BusinessObjectFactory factory)
		{
			var part = factory.New<OrgSupplierPart>();
			part.OP_PartNum = "NEWPROD1";
			return part.PivotsForBinding.AddNew();
		}

		static AssignedCusClassPartPivotRef CreatePivotRef(CusClassPartPivot partPivot, string number)
		{
			var result = partPivot.AssignedCusClassPartPivotRefCollection.AddNew();
			result.CIR_ReferenceNumber = number;
			return result;
		}
	}
}
