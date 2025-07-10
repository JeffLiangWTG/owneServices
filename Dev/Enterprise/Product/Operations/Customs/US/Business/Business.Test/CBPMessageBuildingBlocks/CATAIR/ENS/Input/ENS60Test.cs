using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.ACS;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.US;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Input.Testing
{
	sealed class ENS60Test : BIRDLineUpdateTest
	{
		public void TestNoDuplicateOrganizationsCreated()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			declaration.Invoices.AddNew();
			var invLine = declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "VNWINSPOBIN", GlbCompany.CurrentCompany.Country);

			var ens60 = new ENS60();
			ens60.ManufacturerSupplierCode = "VNWINSPOBIN";
			ens60.InternalRevenueServiceIRSTax = 8m;

			((IBIRDLineRecord)ens60).Update(invLine, new NotificationCollection());
			AssertEquals("Manufacturer has been set", orgHeader.MainAddress.PK, invLine.JI_OA_ManufacturerAddress);

			var codes = new OrgCusCode.Loader(Factory).Load(Core.Constants.CountryCodes.UnitedStates, OrgCusCode.USACodeTypes.ManufacturerID, "VNWINSPOBIN");
			AssertEquals("Shoul be one customs code", 1, codes.Length);

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var address2 = orgHeader2.Addresses.AddNew();
			address2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "VNWINSPOBIN", GlbCompany.CurrentCompany.Country);

			codes = new OrgCusCode.Loader(Factory).Load(Core.Constants.CountryCodes.UnitedStates, OrgCusCode.USACodeTypes.ManufacturerID, "VNWINSPOBIN");
			AssertEquals("Precondition: shoul be two customs codes and new cus codes should not be created after update", 2, codes.Length);

			((IBIRDLineRecord)ens60).Update(invLine, new NotificationCollection());
			codes = new OrgCusCode.Loader(Factory).Load(Core.Constants.CountryCodes.UnitedStates, OrgCusCode.USACodeTypes.ManufacturerID, "VNWINSPOBIN");
			AssertEquals("Shoul be two customs codes", 2, codes.Length);

			AssertEquals("Manufacturer has been set even if two orgs with the same cus code", orgHeader.MainAddress.PK, invLine.JI_OA_ManufacturerAddress);
		}

		public void TestUpdateManufacturerAddressByCode()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress1 = orgHeader.Addresses.AddNew();
			orgAddress1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "PHEVEAPP80SAB", GlbCompany.CurrentCompany.Country);

			var notifications = new NotificationBuffer();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var ensEntry = declaration.ActiveEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var entryLine = ensEntry.AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var eNS60 = new ENS60() { ManufacturerSupplierCode = "PHEVEAPP80SAB" };
			((IBIRDLineRecord)eNS60).Update(invoiceLine, notifications);
			AssertEquals("Matched address found by MID", orgAddress1.PK, invoiceLine.JI_OA_ManufacturerAddress);

			notifications.Clear();
			invoiceLine.JI_OA_ManufacturerAddress = ZGuid.Empty;
			eNS60 = new ENS60() { ManufacturerSupplierCode = "  PHEVEAPP80SAB" };
			((IBIRDLineRecord)eNS60).Update(invoiceLine, notifications);
			AssertEquals("No matched address found or created because MID is invalid", ZGuid.Empty, invoiceLine.JI_OA_ManufacturerAddress);
			AssertContains(ZString.Format(OrganisationCreator.NoManufacturerCreatedAsMIDInvalid, "  PHEVEAPP80SAB"), notifications.AsString.Trim());

			notifications.Clear();
			invoiceLine.JI_OA_ManufacturerAddress = ZGuid.Empty;
			eNS60 = new ENS60() { ManufacturerSupplierCode = "PHEVEAPP80SCD" };
			((IBIRDLineRecord)eNS60).Update(invoiceLine, notifications);
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, AutocreatefromMID.FullName));
			AssertEquals("New organization and address is created", org.MainAddress.PK, invoiceLine.JI_OA_ManufacturerAddress);
		}

		protected override IBIRDLineRecord[] GetPopulatedLineRecords()
		{
			ENS60 ens60WithADD = new ENS60();
			ens60WithADD.AntidumpingCaseNumber = "A588201013";
			ens60WithADD.BondedADDIndicator = "1";
			ens60WithADD.AntidumpingDuty = 0.01m;
			ens60WithADD.ManufacturerSupplierCode = "AUABCEXP6390ALE";
			ens60WithADD.ADDDepositRate = 0.08m;

			ENS60 ens60WithCVD = new ENS60();
			ens60WithCVD.CountervailingCaseNumber = "C427819001";
			ens60WithCVD.BondedCVDIndicator = "1";
			ens60WithCVD.CountervailingDuty = 5060.00m;
			ens60WithCVD.CVDDepositRate = 0.05m;
			ens60WithCVD.InternalRevenueServiceIRSTax = 30m;

			return new IBIRDLineRecord[] { ens60WithADD, ens60WithCVD };
		}

		protected override void PrepareData(JobDeclaration declaration, JobComInvoiceHeader invoice, JobComInvoiceLine invoiceLine, IBIRDLineRecord lineRecord)
		{
			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "AUABCEXP6390ALE", GlbCompany.CurrentCompany.Country);

			Factory.Save();

			base.PrepareData(declaration, invoice, invoiceLine, lineRecord);

			ENS60 ens60 = (ENS60)lineRecord;

			if (!ens60.AntidumpingCaseNumber.IsEmpty)
			{
				invoiceLine.JI_Tariff = "8708.99.4960";
				invoiceLine.US_UC_NKCountryOfOrigin = "JP";
				invoiceLine.US_ADDDepositValue = 1m;

				USCACCase addCase = Factory.New<USCACCase>();
				addCase.U5_CaseNumber = "A588201013";
				addCase.U5_ISOCountryCode = "JP";
				addCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
				addCase.U5_CaseStatusDate = ZDateTime.Today;
				var rate = addCase.CaseRates.AddNew();
				rate.U6_EffectiveDate = ZDateTime.Today;
				rate.U6_AdValoremRate = 0.08m;
			}

			if (!ens60.CountervailingCaseNumber.IsEmpty)
			{
				invoiceLine.JI_Tariff = "2844.40.0010";
				invoiceLine.US_UC_NKCountryOfOrigin = "FR";
				invoiceLine.JI_LinePrice = 100000.00m;

				USCACCase addCase = Factory.New<USCACCase>();
				addCase.U5_CaseNumber = "C427819001";
				addCase.U5_ISOCountryCode = "FR";
				addCase.U5_CaseStatus = ACCaseStatusList.Codes.IO;
				addCase.U5_CaseStatusDate = ZDateTime.Today;
				var rate = addCase.CaseRates.AddNew();
				rate.U6_EffectiveDate = ZDateTime.Today;
				rate.U6_AdValoremRate = 0.05m;
			}

			invoiceLine.JI_Tariff = USCTariff.DistilledSpiritsFeeApplicable;

			invoiceLine.US_ADDDepositValue = 1;
		}

		protected override Type GetTypeOfMessageBlock() => typeof(ENS60);

		protected override string[] GetFieldNameToExcludeForTesting()
		{
			return new string[]
			{
				"CVDDepositRate",//This is retrieved from the reference file
				"ADDDepositRate",//This is retrieved from the reference file
			};
		}

		protected override MessageBuilders.EntryHeaderMessageBuilder<ABIInputBlockControlGenerator> GetMessageBuilder(JobDeclaration declaration)
		{
			return new MessageBuilders.EntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, false);
		}
	}
}
