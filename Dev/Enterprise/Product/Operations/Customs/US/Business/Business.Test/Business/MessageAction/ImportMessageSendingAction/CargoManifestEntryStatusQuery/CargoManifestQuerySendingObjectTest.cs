using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CargoManifestQuerySendingObject))]
	sealed class CargoManifestQuerySendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidateLimitOutputOption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XXX";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_CertifyCargoRelease = true;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_CPSCInd = "D";
			invoiceLine.CPSCHeaders.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var sendingHeader = new CargoManifestStatusQueryHeaderObject(declaration);
			var obj = new CargoManifestQuerySendingObject(sendingHeader, entry);
			sendingHeader.ActionCode = CargoManifestStatusQueryActionList.Codes.Entry;
			obj.LimitOutputOption = "~";
			AssertHasErrorContaining(obj.LimitOutputOptionInfo, ListValidation.InvalidCodeError);
			obj.LimitOutputOption = LimitOutputCodeList.Codes._1Last5Results;
			AssertNoErrorContaining(obj.LimitOutputOptionInfo, ListValidation.InvalidCodeError);
			obj.UpdateEntryWithResults = true;
			obj.LimitOutputOption = LimitOutputCodeList.Codes._2AllAvailableResults;
			AssertHasErrorContaining(obj.LimitOutputOptionInfo, CargoManifestQuerySendingObject.LimitOutputShouldBeBlank);
			obj.LimitOutputOption = LimitOutputCodeList.Codes._0MostRecentResults;
			AssertNoErrorContaining(obj.LimitOutputOptionInfo, CargoManifestQuerySendingObject.LimitOutputShouldBeBlank);
		}

		public void TestEntryManifestQueryRequestFlags()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill = declaration.Bills.AddNew();
			var sendingHeader = new CargoManifestStatusQueryHeaderObject(declaration);
			var obj = new CargoManifestQuerySendingObject(sendingHeader, bill);
			AssertEquals(ZString.Empty, obj.LimitOutputOption);
			sendingHeader.ActionCode = CargoManifestStatusQueryActionList.Codes.HAWB;
			obj = new CargoManifestQuerySendingObject(sendingHeader, bill);
			AssertEquals(LimitOutputCodeList.Codes._2AllAvailableResults, obj.LimitOutputOption);
			obj.LimitOutputOption = ZString.Empty;
			Assert(obj.RequestForRelatedBOLInfo.ReadOnly);
			Assert(!obj.LimitOutputOption_ReadOnly);
			obj.LimitOutputOption = LimitOutputCodeList.Codes._1Last5Results;
			sendingHeader.ActionCode = CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill;
			Assert(!obj.RequestForRelatedBOLInfo.ReadOnly);
			obj.RequestForRelatedBOL = true;
			Assert(obj.RequestForRelatedBOL);
			Assert(!obj.LimitOutputOption_ReadOnly);
			obj.LimitOutputOption = LimitOutputCodeList.Codes._1Last5Results;
			sendingHeader.ActionCode = CargoManifestStatusQueryActionList.Codes.Entry;
			Assert(obj.UpdateEntryWithResultsInfo.ReadOnly);
			Assert(obj.LimitOutputOption_ReadOnly);
			AssertEquals(LimitOutputCodeList.Codes._1Last5Results, obj.LimitOutputOption);
			USCustomsDataRegistry.Instance.RequestForBillAndEntryData.SetValue(Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			Assert(obj.UpdateEntryWithResultsInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill = declaration.Bills.AddNew();
			var sendingHeader = new CargoManifestStatusQueryHeaderObject(declaration);
			return new CargoManifestQuerySendingObject(sendingHeader, bill);
		}
	}
}
