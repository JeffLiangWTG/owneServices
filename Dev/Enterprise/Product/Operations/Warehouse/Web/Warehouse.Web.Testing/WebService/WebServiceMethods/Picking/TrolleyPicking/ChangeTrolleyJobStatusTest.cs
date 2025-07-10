using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.TrolleyPicking.Testing;
using PickTrolleyStatus = Enterprise.Warehouse.Transactions.TrolleyPicking.PickTrolleyStatus;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class ChangeTrolleyJobStatusTest : WhsSecureServiceTestCase
	{
		#region TestChangeTrolleyJobStatus

		#region TestChangeTrolleyJobStatus_CanChange

		public void TestChangeTrolleyJobStatus_CanChange_BLD_PIC()
		{
			TestChangeTrolleyJobStatusCore(TrolleyJobStatus.Building, TrolleyJobStatus.Picking, true);
		}

		public void TestChangeTrolleyJobStatus_CanChange_PIC_FIN()
		{
			TestChangeTrolleyJobStatusCore(TrolleyJobStatus.Picking, TrolleyJobStatus.Finalised, true);
		}

		#endregion

		#region TestChangeTrolleyJobStatus_CannotChange

		public void TestChangeTrolleyJobStatus_CannotChange_BLD_FIN()
		{
			TestChangeTrolleyJobStatusCore(TrolleyJobStatus.Building, TrolleyJobStatus.Finalised, false);
		}

		public void TestChangeTrolleyJobStatus_CannotChange_PIC_BLD()
		{
			TestChangeTrolleyJobStatusCore(TrolleyJobStatus.Picking, TrolleyJobStatus.Building, false);
		}

		public void TestChangeTrolleyJobStatus_CannotChange_FIN_BLD()
		{
			TestChangeTrolleyJobStatusCore(TrolleyJobStatus.Finalised, TrolleyJobStatus.Building, false);
		}

		public void TestChangeTrolleyJobStatus_CannotChange_FIN_PIC()
		{
			TestChangeTrolleyJobStatusCore(TrolleyJobStatus.Finalised, TrolleyJobStatus.Picking, false);
		}

		#endregion

		void TestChangeTrolleyJobStatusCore(TrolleyJobStatus initialStatus, TrolleyJobStatus newStatus, bool expectedCanChange)
		{
			var initialStatusCode = GetPickTrolleyStatusCode(initialStatus);
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, initialStatusCode);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var response = webService.ChangeTrolleyJobStatus(trolleyJob.PK.ToGuid(), newStatus);
			AssertSuccessfulResponse(response, webService);

			if (expectedCanChange)
			{
				var newStatusCode = GetPickTrolleyStatusCode(newStatus);
				CombineAssertions(() =>
				{
					AssertEquals(ErrorTypes.None, response.Error);
					AssertEquals(true, string.IsNullOrEmpty(response.ErrorMessage));
					AssertEquals(newStatusCode, trolleyJob.WTJ_Status);
					AssertEquals(false, trolleyJob.HasChanges);
				});
			}
			else
			{
				CombineAssertions(() =>
				{
					AssertEquals(ErrorTypes.BusinessValidationError, response.Error);
					AssertEquals($"Cannot change status to '{newStatus}'. Current status is '{initialStatusCode}'.", response.ErrorMessage);
				});
			}
		}

		public void TestChangeTrolleyJobStatus_JobNotFound()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var webService = GetNewWebService(data.Whs1);
			var responseMissingTrolley = webService.ChangeTrolleyJobStatus(Guid.NewGuid(), TrolleyJobStatus.Building);
			AssertEquals(ErrorTypes.BusinessValidationError, responseMissingTrolley.Error);
			AssertEquals("Trolley job was not found. Perhaps it was deleted.", responseMissingTrolley.ErrorMessage);
		}

		public void TestChangeTrolleyJobStatus_InvalidStatus()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var webService = GetNewWebService(data.Whs1);

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, PickTrolleyStatus.Codes.Building);
			Helper.Factory.Save();

			var responseInvalidStatus = webService.ChangeTrolleyJobStatus(trolleyJob.PK.ToGuid(), (TrolleyJobStatus)5);
			AssertEquals(ErrorTypes.BusinessValidationError, responseInvalidStatus.Error);
			AssertEquals("Cannot change status to '5'. Current status is 'BLD'.", responseInvalidStatus.ErrorMessage);
		}

		#region TestChangeTrolleyJobStatus_FactoryConcurrencySaveError

		public void TestChangeTrolleyJobStatus_FactoryConcurrencySaveError()
		{
			var initialStatusCode = GetPickTrolleyStatusCode(TrolleyJobStatus.Building);
			var newStatus = TrolleyJobStatus.Picking;
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var trolley = Helper.CreateTrolley("T1");
			var trolleyJob = Helper.CreateWhsPickTrolleyJob(trolley, initialStatusCode);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			var innerException = new Exception();
			var concurrencyException = new ZDataConcurrencyException(innerException, ((IBusinessObjectInternals)trolleyJob).Row, TestConnection);
			webService.Factory.Saving += f => throw new ZSaveConcurrencyException(concurrencyException, Helper.Factory);

			var response = webService.ChangeTrolleyJobStatus(trolleyJob.PK.ToGuid(), newStatus);
			AssertEquals("ZSaveConcurrencyException should be logged as an Error.", "Another user has changed trolley job status. Please restart the operation and try again.", response.ErrorMessage);
		}

		#endregion

		#endregion

		string GetPickTrolleyStatusCode(TrolleyJobStatus status)
		{
			switch (status)
			{
				case TrolleyJobStatus.Building:
					return PickTrolleyStatus.Codes.Building;
				case TrolleyJobStatus.Picking:
					return PickTrolleyStatus.Codes.Picking;
				case TrolleyJobStatus.Finalised:
					return PickTrolleyStatus.Codes.Finalised;
				default:
					throw new ArgumentException("Status code not valid");
			}
		}
	}
}
