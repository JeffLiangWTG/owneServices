using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CreateDeclarationHelper))]
	sealed class CreateDeclarationHelperTest : TestCaseWithFactory
	{
		public void TestCreateDeclaration()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HB12312";
			Factory.Save();

			var helperMock = new Mock<CreateDeclarationHelper>();
			helperMock.CallBase = true;
			var helper = helperMock.Object;
			var notifyTypes = new List<CreateDeclarationHelper.NotifyType>();
			helper.Notify = (s, n) =>
			{
				notifyTypes.Add(n);
			};
			helper.GetAnswer = (s, q, t) => false;

			var dec = Factory.New<BaseJobDeclaration>();
			Env.Security.CustomsDeclarationEnquiryNew.IsAllowed = false;
			using (var mutex1 = new ZGlobalMutex(MutexIDs.JobBeingCreatedForShipment, "HELLO"))
			using (var mutex2 = new ZGlobalMutex(MutexIDs.JobBeingCreatedForShipment, "HELLO"))
			{
				mutex1.Lock();
				AssertNull(helper.CreateDeclaration(shipment, mutex2, () => dec));
				AssertArrayEqualsByElements(new[] { CreateDeclarationHelper.NotifyType.MutexLocked }, notifyTypes.ToArray());
				AssertEquals(ZGuid.Empty, dec.JE_JS);
				AssertEquals(false, mutex2.HasLock);
				helperMock.Protected().Verify("StartSynchroniser", Times.Never(), ItExpr.IsNull<BaseJobDeclaration>());
				mutex1.Unlock();
				notifyTypes.Clear();
				AssertNull(helper.CreateDeclaration(shipment, mutex2, () => dec));
				AssertArrayEqualsByElements(new[] { CreateDeclarationHelper.NotifyType.SecurityError, CreateDeclarationHelper.NotifyType.NotToCreateJob }, notifyTypes.ToArray());
				AssertEquals(ZGuid.Empty, dec.JE_JS);
				AssertEquals(false, mutex2.HasLock);
				helperMock.Protected().Verify("StartSynchroniser", Times.Never(), ItExpr.IsNull<BaseJobDeclaration>());
				Env.Security.CustomsDeclarationEnquiryNew.IsAllowed = true;
				notifyTypes.Clear();
				AssertNull(helper.CreateDeclaration(shipment, mutex2, () => dec));
				AssertArrayEqualsByElements(new[] { CreateDeclarationHelper.NotifyType.NotToCreateJob }, notifyTypes.ToArray());
				AssertEquals(ZGuid.Empty, dec.JE_JS);
				AssertEquals(false, mutex2.HasLock);
				helperMock.Protected().Verify("StartSynchroniser", Times.Never(), ItExpr.IsNull<BaseJobDeclaration>());
				helper.GetAnswer = null;
				notifyTypes.Clear();
				AssertSame(dec, helper.CreateDeclaration(shipment, mutex2, () => dec));
				AssertEquals(true, mutex2.HasLock);
				AssertEquals(0, notifyTypes.Count);
				helperMock.Protected().Verify("StartSynchroniser", Times.AtLeastOnce(), dec);
				mutex2.Unlock();
				helperMock.Invocations.Clear();
				var dec2 = helper.CreateDeclaration(shipment, mutex2, null);
				AssertNotEquals(dec, dec2);
				AssertEquals(true, mutex2.HasLock);
				AssertArrayEqualsByElements(new[] { CreateDeclarationHelper.NotifyType.DeclarationCreateSucceed }, notifyTypes.ToArray());
				helperMock.Protected().Verify("StartSynchroniser", Times.AtLeastOnce(), dec2);
				mutex2.Unlock();
			}
		}

		public void TestGetDescriptionForNotifyType()
		{
			var createDeclarationHelper = new CreateDeclarationHelper();
			AssertEquals("Another Declaration Create is in progress.", createDeclarationHelper.GetDescriptionForNotifyType(CreateDeclarationHelper.NotifyType.MutexLocked));
			AssertEquals("A Declaration was not created.", createDeclarationHelper.GetDescriptionForNotifyType(CreateDeclarationHelper.NotifyType.NotToCreateJob));
			AssertEquals("'Customs Declaration Enquiry New' permission is required.", createDeclarationHelper.GetDescriptionForNotifyType(CreateDeclarationHelper.NotifyType.SecurityError));
			AssertEquals("A Declaration was created.", createDeclarationHelper.GetDescriptionForNotifyType(CreateDeclarationHelper.NotifyType.DeclarationCreateSucceed));
			AssertEquals("A Declaration was imported.", createDeclarationHelper.GetDescriptionForNotifyType(CreateDeclarationHelper.NotifyType.DeclarationImportSucceed));
		}
	}
}
