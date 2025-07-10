using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class InvoiceLinePartClassificationTariffDescriptionSyncroniserAbstractTest : TestCaseWithFactory
	{
		public void TestPartIsFirstEntered()
		{
			InvoiceLine.JI_PartNo = Part.OP_PartNum;
			AssertEquals("Part Number on InvoiceLine", PartNumber, InvoiceLine.JI_PartNo);
			AssertEquals("Lookup Code on InvoiceLine", Lookup.PK, InvoiceLine.JI_CC);
			AssertEquals("Tariff Code on InvoiceLine", TariffCode, InvoiceLine.JI_Tariff);
			AssertEquals("Description on InvoiceLine", PartDescription, InvoiceLineDescription);
		}

		public void TestLookupIsFirstEntered()
		{
			InvoiceLine.JI_CC = Lookup.PK;
			AssertEquals("Part Number on InvoiceLine", "", InvoiceLine.JI_PartNo);
			AssertEquals("Lookup Code on InvoiceLine", Lookup.PK, InvoiceLine.JI_CC);
			AssertEquals("Tariff Code on InvoiceLine", TariffCode, InvoiceLine.JI_Tariff);
			AssertEquals("Description on InvoiceLine", LookupDescription, InvoiceLineDescription);
		}

		public virtual void TestTariffIsFirstEntered()
		{
			InvoiceLine.JI_Tariff = TariffCode;
			AssertEquals("Part Number on InvoiceLine", "", InvoiceLine.JI_PartNo);
			AssertEquals("Lookup Code on InvoiceLine", ZGuid.Empty, InvoiceLine.JI_CC);
			AssertEquals("Tariff Code on InvoiceLine", TariffCode, InvoiceLine.JI_Tariff);
			AssertEquals("Description on InvoiceLine", TariffDescription.ToUpper(), InvoiceLineDescription);
		}

		public virtual void TestDescriptionOnTarrifWhenMerged()
		{
			var declaration = CreateBaseJobDeclarationForMerge();
			if (!IntegratedCountryHelper.CountryHasBuiltInDeclaration(declaration.CountryCode))
			{
				Assert("For integrated countries, merge is turned off.", true);
				return;
			}

			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_MessageType = MessageType;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			var line1 = invoice.JobComInvoiceLines.AddNew();
			line1.JI_Tariff = TariffCode;
			line1.JI_Description = "LINE";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(true);
			DoMerge(declaration);

			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("Description", ExpectedDescriptionFromMergeOfOneLine, entryLine.Description);

			var line2 = invoice.JobComInvoiceLines.AddNew();
			line2.JI_Tariff = TariffCode;
			line2.JI_Description = "LINE 2";

			declaration.DoMerge();
			AssertEquals("Description", ExpectedDescriptionFromMergeOfMultipleLines, entryLine.Description);

			AssertEquals("(pre-condition) invoice lines merged to one entry line", 2, entryLine.InvoiceLines.Count);
			line1.JI_ExtraInfoForClassification = "Extended Description";
			declaration.DoMerge();
			AssertEquals("ExtendedCommercialDescription", "", entryLine.ExtendedCommercialDescription);

			line2.JI_ExtraInfoForClassification = "Extended Description";
			declaration.DoMerge();
			AssertEquals("ExtendedCommercialDescription", "Extended Description", entryLine.ExtendedCommercialDescription);
		}

		public void TestTariffThenLookupIsEntered()
		{
			InvoiceLine.JI_Tariff = TariffCode;
			InvoiceLine.JI_CC = Lookup.PK;
			AssertEquals("Part Number on InvoiceLine", "", InvoiceLine.JI_PartNo);
			AssertEquals("Lookup Code on InvoiceLine", Lookup.PK, InvoiceLine.JI_CC);
			AssertEquals("Tariff Code on InvoiceLine", TariffCode, InvoiceLine.JI_Tariff);
			AssertEquals("Description on InvoiceLine", LookupDescription, InvoiceLineDescription);
			AssertEquals("Extra Info For Classification on InvoiceLine", "", InvoiceLine.JI_ExtraInfoForClassification);
		}

		public void TestTariffThenPartIsEntered()
		{
			InvoiceLine.JI_Tariff = TariffCode;
			InvoiceLine.JI_PartNo = Part.OP_PartNum;
			AssertEquals("Part Number on InvoiceLine", PartNumber, InvoiceLine.JI_PartNo);
			AssertEquals("Lookup Code on InvoiceLine", Lookup.PK, InvoiceLine.JI_CC);
			AssertEquals("Tariff Code on InvoiceLine", TariffCode, InvoiceLine.JI_Tariff);
			AssertEquals("Description on InvoiceLine", PartDescription, InvoiceLineDescription);
			AssertEquals("Extra Info For Classification on InvoiceLine", ExpectedPartExtendedCommercialDescription, InvoiceLine.JI_ExtraInfoForClassification);
		}

		public void TestLookupThenPartIsEntered()
		{
			InvoiceLine.JI_CC = Lookup.PK;
			InvoiceLine.JI_PartNo = Part.OP_PartNum;
			AssertEquals("Part Number on InvoiceLine", PartNumber, InvoiceLine.JI_PartNo);
			AssertEquals("Lookup Code on InvoiceLine", Lookup.PK, InvoiceLine.JI_CC);
			AssertEquals("Tariff Code on InvoiceLine", TariffCode, InvoiceLine.JI_Tariff);
			AssertEquals("Description on InvoiceLine", PartDescription, InvoiceLineDescription);
			AssertEquals("Extra Info For Classification on InvoiceLine", ExpectedPartExtendedCommercialDescription, InvoiceLine.JI_ExtraInfoForClassification);
		}

		public void TestDescriptionThenTariffIsEntered()
		{
			InvoiceLineDescription = UserDescription;
			InvoiceLine.JI_Tariff = TariffCode;
			AssertEquals("Part Number on InvoiceLine", "", InvoiceLine.JI_PartNo);
			AssertEquals("Lookup Code on InvoiceLine", ZGuid.Empty, InvoiceLine.JI_CC);
			AssertEquals("Tariff Code on InvoiceLine", TariffCode, InvoiceLine.JI_Tariff);
			AssertEquals("Description on InvoiceLine", UserDescription, InvoiceLineDescription);
			AssertEquals("Extra Info For Classification on InvoiceLine", "", InvoiceLine.JI_ExtraInfoForClassification);
		}

		public void TestDescriptionThenLookupIsEntered()
		{
			InvoiceLineDescription = UserDescription;
			InvoiceLine.JI_CC = Lookup.PK;
			AssertEquals("Part Number on InvoiceLine", "", InvoiceLine.JI_PartNo);
			AssertEquals("Lookup Code on InvoiceLine", Lookup.PK, InvoiceLine.JI_CC);
			AssertEquals("Tariff Code on InvoiceLine", TariffCode, InvoiceLine.JI_Tariff);
			AssertEquals("Description on InvoiceLine", UserDescription, InvoiceLineDescription);
			AssertEquals("Extra Info For Classification on InvoiceLine", "", InvoiceLine.JI_ExtraInfoForClassification);
		}

		public void TestDescriptionThenPartIsEntered()
		{
			InvoiceLineDescription = UserDescription;
			InvoiceLine.JI_PartNo = Part.OP_PartNum;
			AssertEquals("Part Number on InvoiceLine", PartNumber, InvoiceLine.JI_PartNo);
			AssertEquals("Lookup Code on InvoiceLine", Lookup.PK, InvoiceLine.JI_CC);
			AssertEquals("Tariff Code on InvoiceLine", TariffCode, InvoiceLine.JI_Tariff);
			AssertEquals("Description on InvoiceLine", UserDescription, InvoiceLineDescription);
			AssertEquals("Extra Info For Classification on InvoiceLine", "", InvoiceLine.JI_ExtraInfoForClassification);
		}

		public void TestRemovingLookupLeavesPartAndPartDescription()
		{
			InvoiceLine.JI_PartNo = Part.OP_PartNum;
			InvoiceLine.JI_CC = ZGuid.Empty;
			AssertEquals("Part Number on InvoiceLine", PartNumber, InvoiceLine.JI_PartNo);
			AssertEquals("Lookup Code on InvoiceLine", ZGuid.Empty, InvoiceLine.JI_CC);
			AssertEquals("Tariff Code on InvoiceLine", TariffCode, InvoiceLine.JI_Tariff);
			AssertEquals("Description on InvoiceLine", PartDescription, InvoiceLineDescription);
			AssertEquals("Extra Info For Classification on InvoiceLine", ExpectedPartExtendedCommercialDescription, InvoiceLine.JI_ExtraInfoForClassification);
		}

		public void TestRemovingTariffRemovesLookupButLeavesPartAndPartDescription()
		{
			InvoiceLine.JI_PartNo = Part.OP_PartNum;
			InvoiceLine.JI_Tariff = "";
			AssertEquals("Part Number on InvoiceLine", PartNumber, InvoiceLine.JI_PartNo);
			AssertEquals("Lookup Code on InvoiceLine", ZGuid.Empty, InvoiceLine.JI_CC);
			AssertEquals("Tariff Code on InvoiceLine", "", InvoiceLine.JI_Tariff);
			AssertEquals("Description on InvoiceLine", PartDescription, InvoiceLineDescription);
			AssertEquals("Extra Info For Classification on InvoiceLine", ExpectedPartExtendedCommercialDescription, InvoiceLine.JI_ExtraInfoForClassification);
		}

		public void TestRemovingPartLeavesLookupAndClassification()
		{
			InvoiceLine.JI_PartNo = Part.OP_PartNum;
			InvoiceLine.JI_PartNo = "";
			AssertEquals("Part Number on InvoiceLine", "", InvoiceLine.JI_PartNo);
			AssertEquals("Lookup Code on InvoiceLine", Lookup.PK, InvoiceLine.JI_CC);
			AssertEquals("Tariff Code on InvoiceLine", TariffCode, InvoiceLine.JI_Tariff);
			AssertEquals("Description on InvoiceLine", LookupDescription, InvoiceLineDescription);
			AssertEquals("Extra Info For Classification on InvoiceLine", "", InvoiceLine.JI_ExtraInfoForClassification);
		}

		public void TestTariffThenLookupIsEnteredAfterSave()
		{
			InvoiceLine.JI_Tariff = TariffCode;
			AssertNotNull("This is to cause the Lazy Getter to Load the Lookup before saving", Lookup);
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			BaseJobComInvoiceLine loadedInvoiceLine = ReLoadInvoiceLine(newFactory, Declaration);
			loadedInvoiceLine.JI_CC = Lookup.PK;
			AssertEquals("Part Number on InvoiceLine", "", loadedInvoiceLine.JI_PartNo);
			AssertEquals("Lookup Code on InvoiceLine", Lookup.PK, loadedInvoiceLine.JI_CC);
			AssertEquals("Tariff Code on InvoiceLine", TariffCode, loadedInvoiceLine.JI_Tariff);
			AssertEquals("Description on InvoiceLine", LookupDescription, ReLoadInvoiceLineDescription(loadedInvoiceLine));
			AssertEquals("Extra Info For Classification on InvoiceLine", "", loadedInvoiceLine.JI_ExtraInfoForClassification);
		}

		public void TestTariffThenPartIsEnteredAfterSave()
		{
			InvoiceLine.JI_Tariff = TariffCode;
			AssertNotNull("This is to cause the Lazy Getter to Load the Part before saving", Part);
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			BaseJobComInvoiceLine loadedInvoiceLine = ReLoadInvoiceLine(newFactory, Declaration);
			loadedInvoiceLine.JI_PartNo = Part.OP_PartNum;
			AssertEquals("Part Number on InvoiceLine", PartNumber, loadedInvoiceLine.JI_PartNo);
			AssertEquals("Lookup Code on InvoiceLine", Lookup.PK, loadedInvoiceLine.JI_CC);
			AssertEquals("Tariff Code on InvoiceLine", TariffCode, loadedInvoiceLine.JI_Tariff);
			AssertEquals("Description on InvoiceLine", PartDescription, ReLoadInvoiceLineDescription(loadedInvoiceLine));
			AssertEquals("Extra Info For Classification on InvoiceLine", ExpectedPartExtendedCommercialDescription, loadedInvoiceLine.JI_ExtraInfoForClassification);
		}

		public void TestLookupThenPartIsEnteredAfterSave()
		{
			InvoiceLine.JI_CC = Lookup.PK;
			AssertNotNull("This is to cause the Lazy Getter to Load the Part before saving", Part);
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			BaseJobComInvoiceLine loadedInvoiceLine = ReLoadInvoiceLine(newFactory, Declaration);
			loadedInvoiceLine.JI_PartNo = Part.OP_PartNum;
			AssertEquals("Part Number on InvoiceLine", PartNumber, loadedInvoiceLine.JI_PartNo);
			AssertEquals("Lookup Code on InvoiceLine", Lookup.PK, loadedInvoiceLine.JI_CC);
			AssertEquals("Tariff Code on InvoiceLine", TariffCode, loadedInvoiceLine.JI_Tariff);
			AssertEquals("Description on InvoiceLine", PartDescription, ReLoadInvoiceLineDescription(loadedInvoiceLine));
			AssertEquals("Extra Info For Classification on InvoiceLine", ExpectedPartExtendedCommercialDescription, loadedInvoiceLine.JI_ExtraInfoForClassification);
		}

		public void TestDescriptionThenTariffIsEnteredAfterSave()
		{
			InvoiceLineDescription = UserDescription;
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			BaseJobComInvoiceLine loadedInvoiceLine = ReLoadInvoiceLine(newFactory, Declaration);
			loadedInvoiceLine.JI_Tariff = TariffCode;
			AssertEquals("Part Number on InvoiceLine", "", loadedInvoiceLine.JI_PartNo);
			AssertEquals("Lookup Code on InvoiceLine", ZGuid.Empty, loadedInvoiceLine.JI_CC);
			AssertEquals("Tariff Code on InvoiceLine", TariffCode, loadedInvoiceLine.JI_Tariff);
			AssertEquals("Description on InvoiceLine", UserDescription, ReLoadInvoiceLineDescription(loadedInvoiceLine));
			AssertEquals("Extra Info For Classification on InvoiceLine", "", loadedInvoiceLine.JI_ExtraInfoForClassification);
		}

		public void TestDescriptionThenLookupIsEnteredAfterSave()
		{
			InvoiceLineDescription = UserDescription;
			AssertNotNull("This is to cause the Lazy Getter to Load the Lookup before saving", Lookup);
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			BaseJobComInvoiceLine loadedInvoiceLine = ReLoadInvoiceLine(newFactory, Declaration);
			loadedInvoiceLine.JI_CC = Lookup.PK;
			AssertEquals("Part Number on InvoiceLine", "", loadedInvoiceLine.JI_PartNo);
			AssertEquals("Lookup Code on InvoiceLine", Lookup.PK, loadedInvoiceLine.JI_CC);
			AssertEquals("Tariff Code on InvoiceLine", TariffCode, loadedInvoiceLine.JI_Tariff);
			AssertEquals("Description on InvoiceLine", UserDescription, ReLoadInvoiceLineDescription(loadedInvoiceLine));
			AssertEquals("Extra Info For Classification on InvoiceLine", "", loadedInvoiceLine.JI_ExtraInfoForClassification);
		}

		public void TestDescriptionChangesWhenTariffDescriptionAlreadyExisted()
		{
			InvoiceLine.JI_Tariff = TariffCode;
			AssertEquals(TariffDescription.ToUpper(), InvoiceLineDescription);
			InvoiceLine.JI_CC = Lookup.PK;
			AssertEquals(Lookup.CC_Description, InvoiceLineDescription);
		}

		public void TestDescriptionThenPartIsEnteredAfterSave()
		{
			InvoiceLineDescription = UserDescription;
			AssertNotNull("This is to cause the Lazy Getter to Load the Part before saving", Part);
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			BaseJobComInvoiceLine loadedInvoiceLine = ReLoadInvoiceLine(newFactory, Declaration);
			loadedInvoiceLine.JI_PartNo = Part.OP_PartNum;
			AssertEquals("Part Number on InvoiceLine", PartNumber, loadedInvoiceLine.JI_PartNo);
			AssertEquals("Lookup Code on InvoiceLine", Lookup.PK, loadedInvoiceLine.JI_CC);
			AssertEquals("Tariff Code on InvoiceLine", TariffCode, loadedInvoiceLine.JI_Tariff);
			AssertEquals("Description on InvoiceLine", UserDescription, ReLoadInvoiceLineDescription(loadedInvoiceLine));
			AssertEquals("Extra Info For Classification on InvoiceLine", "", loadedInvoiceLine.JI_ExtraInfoForClassification);
		}

		public void TestRemovingLookupLeavesPartAndPartDescriptionAfterSave()
		{
			InvoiceLine.JI_PartNo = Part.OP_PartNum;
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			BaseJobComInvoiceLine loadedInvoiceLine = ReLoadInvoiceLine(newFactory, Declaration);
			loadedInvoiceLine.JI_CC = ZGuid.Empty;
			AssertEquals("Part Number on InvoiceLine", PartNumber, loadedInvoiceLine.JI_PartNo);
			AssertEquals("Lookup Code on InvoiceLine", ZGuid.Empty, loadedInvoiceLine.JI_CC);
			AssertEquals("Tariff Code on InvoiceLine", TariffCode, loadedInvoiceLine.JI_Tariff);
			AssertEquals("Description on InvoiceLine", PartDescription, ReLoadInvoiceLineDescription(loadedInvoiceLine));
			AssertEquals("Extra Info For Classification on InvoiceLine", ExpectedPartExtendedCommercialDescription, loadedInvoiceLine.JI_ExtraInfoForClassification);
		}

		public void TestRemovingTariffRemovesLookupButLeavesPartAndPartDescriptionAfterSave()
		{
			InvoiceLine.JI_PartNo = Part.OP_PartNum;
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			BaseJobComInvoiceLine loadedInvoiceLine = ReLoadInvoiceLine(newFactory, Declaration);
			loadedInvoiceLine.JI_Tariff = "";
			AssertEquals("Part Number on InvoiceLine", PartNumber, loadedInvoiceLine.JI_PartNo);
			AssertEquals("Lookup Code on InvoiceLine", ZGuid.Empty, loadedInvoiceLine.JI_CC);
			AssertEquals("Tariff Code on InvoiceLine", "", loadedInvoiceLine.JI_Tariff);
			AssertEquals("Description on InvoiceLine", PartDescription, ReLoadInvoiceLineDescription(loadedInvoiceLine));
			AssertEquals("Extra Info For Classification on InvoiceLine", ExpectedPartExtendedCommercialDescription, loadedInvoiceLine.JI_ExtraInfoForClassification);
		}

		public void TestRemovingPartLeavesLookupAndClassificationAfterSave()
		{
			InvoiceLine.JI_PartNo = Part.OP_PartNum;
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			BaseJobComInvoiceLine loadedInvoiceLine = ReLoadInvoiceLine(newFactory, Declaration);
			loadedInvoiceLine.JI_PartNo = "";
			AssertEquals("Part Number on InvoiceLine", "", loadedInvoiceLine.JI_PartNo);
			AssertEquals("Lookup Code on InvoiceLine", Lookup.PK, loadedInvoiceLine.JI_CC);
			AssertEquals("Tariff Code on InvoiceLine", TariffCode, loadedInvoiceLine.JI_Tariff);
			AssertEquals("Description on InvoiceLine", LookupDescription, ReLoadInvoiceLineDescription(loadedInvoiceLine));
			AssertEquals("Extra Info For Classification on InvoiceLine", "", loadedInvoiceLine.JI_ExtraInfoForClassification);
		}

		public void TestUpdatingDescriptionInAnotherFactoryUpdatesDescriptionIfDescriptionWasGeneratedFromPartAfterLoad()
		{
			var newPartDescription = GetPartDescriptionForTestUpdatingDescriptionInAnotherFactoryUpdatesDescriptionIfDescriptionWasGeneratedFromPartAfterLoad();
			InvoiceLine.JI_PartNo = Part.OP_PartNum;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			BaseJobComInvoiceLine loadedInvoiceLine = ReLoadInvoiceLine(newFactory, Declaration);
			JobComInvoiceLinePartSynchronisationManager.SetCurrentPartSyncManagerActiveDeciderPK(newFactory, Declaration.PK);
			AssertEquals("Part Number on InvoiceLine", PartNumber, loadedInvoiceLine.JI_PartNo);
			AssertEquals("Lookup Code on InvoiceLine", Lookup.PK, loadedInvoiceLine.JI_CC);
			AssertEquals("Tariff Code on InvoiceLine", TariffCode, loadedInvoiceLine.JI_Tariff);
			AssertEquals("Description on InvoiceLine", PartDescription, ReLoadInvoiceLineDescription(loadedInvoiceLine));

			BusinessObjectFactory anotherNewFactory = new BusinessObjectFactory();
			OrgSupplierPart loadedPart = anotherNewFactory.Load<OrgSupplierPart>(Part.PK);
			loadedPart.OP_Desc = newPartDescription;
			anotherNewFactory.Save();

			AssertEquals("Part Number on InvoiceLine", PartNumber, loadedInvoiceLine.JI_PartNo);
			AssertEquals("Lookup Code on InvoiceLine", Lookup.PK, loadedInvoiceLine.JI_CC);
			AssertEquals("Tariff Code on InvoiceLine", TariffCode, loadedInvoiceLine.JI_Tariff);
			AssertEquals("Description on InvoiceLine", newPartDescription, ReLoadInvoiceLineDescription(loadedInvoiceLine));
		}

		protected virtual string GetPartDescriptionForTestUpdatingDescriptionInAnotherFactoryUpdatesDescriptionIfDescriptionWasGeneratedFromPartAfterLoad() => "NEW PART DESCRIPTION FROM OTHER FACTORY SAVING";

		public void TestAddingClassificationOnPartGetsUpdatedThroughToInvoiceLine()
		{
			InvoiceLine.JI_PartNo = Part.OP_PartNum;
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			BaseJobComInvoiceLine loadedInvoiceLine = ReLoadInvoiceLine(newFactory, Declaration);

			AssertEquals("Part Number on InvoiceLine", PartNumber, loadedInvoiceLine.JI_PartNo);
			AssertEquals("Lookup Code on InvoiceLine", Lookup.PK, loadedInvoiceLine.JI_CC);
			AssertEquals("Tariff Code on InvoiceLine", TariffCode, loadedInvoiceLine.JI_Tariff);
			AssertEquals("Description on InvoiceLine", PartDescription, ReLoadInvoiceLineDescription(loadedInvoiceLine));
			AssertEquals("Extra Info For Classification on InvoiceLine", ExpectedPartExtendedCommercialDescription, loadedInvoiceLine.JI_ExtraInfoForClassification);
		}

		public void TestChangingClassificationOnPartGetsUpdatedThroughToInvoiceLine()
		{
			Lookup.CC_TariffNum = ZString.Empty;
			InvoiceLine.JI_PartNo = Part.OP_PartNum;

			BaseCusClassification secondLookup = GetNewLookup();
			secondLookup.CC_LookupCode = "SECONDLOOKUP";
			secondLookup.CC_Description = secondLookup.CC_LookupCode;

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			BaseJobComInvoiceLine loadedInvoiceLine = ReLoadInvoiceLine(newFactory, Declaration);
			JobComInvoiceLinePartSynchronisationManager.SetCurrentPartSyncManagerActiveDeciderPK(newFactory, Declaration.PK);
			AssertEquals("Part Number on InvoiceLine", PartNumber, loadedInvoiceLine.JI_PartNo);
			AssertEquals("Lookup Code on InvoiceLine", Lookup.PK, loadedInvoiceLine.JI_CC);
			AssertEquals("Tariff Code on InvoiceLine", ZString.Empty, loadedInvoiceLine.JI_Tariff);
			AssertEquals("Description on InvoiceLine", PartDescription, ReLoadInvoiceLineDescription(loadedInvoiceLine));
			AssertEquals("Extra Info For Classification on InvoiceLine", ExpectedPartExtendedCommercialDescription, loadedInvoiceLine.JI_ExtraInfoForClassification);

			ChangeLookupOnPartInAnotherFactory(Part.PK, secondLookup.PK);
			AssertEquals("Part Number on InvoiceLine", PartNumber, loadedInvoiceLine.JI_PartNo);
			AssertEquals("Lookup Code on InvoiceLine", secondLookup.PK, loadedInvoiceLine.JI_CC);
			AssertEquals("Tariff Code on InvoiceLine", TariffCode, loadedInvoiceLine.JI_Tariff);
			AssertEquals("Description on InvoiceLine", PartDescription, ReLoadInvoiceLineDescription(loadedInvoiceLine));
			AssertEquals("Extra Info For Classification on InvoiceLine", ExpectedPartExtendedCommercialDescription, loadedInvoiceLine.JI_ExtraInfoForClassification);
		}

		public void TestSettingDescriptionWhenChangingTariff()
		{
			InvoiceLine.JI_Tariff = TariffCode;
			AssertEquals("InvoiceLine.TariffDescription", TariffDescription.ToUpper(), InvoiceLineDescription);
			InvoiceLine.JI_Tariff = TariffCode2;
			AssertEquals("InvoiceLine.TarrifDescription", TariffDescription2.ToUpper(), InvoiceLineDescription);
		}

		public void TestChangingClassificationOnPartBackAndForthGetsUpdatedThroughToInvoiceLine()
		{
			Lookup.CC_TariffNum = ZString.Empty;
			InvoiceLine.JI_PartNo = Part.OP_PartNum;

			BaseCusClassification secondLookup = GetNewLookup();
			secondLookup.CC_LookupCode = "SECONDLOOKUP";
			secondLookup.CC_Description = secondLookup.CC_LookupCode;

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			BaseJobComInvoiceLine loadedInvoiceLine = ReLoadInvoiceLine(newFactory, Declaration);
			JobComInvoiceLinePartSynchronisationManager.SetCurrentPartSyncManagerActiveDeciderPK(newFactory, Declaration.PK);
			AssertEquals("Part Number on InvoiceLine", PartNumber, loadedInvoiceLine.JI_PartNo);
			AssertEquals("Lookup Code on InvoiceLine", Lookup.PK, loadedInvoiceLine.JI_CC);
			AssertEquals("Tariff Code on InvoiceLine", ZString.Empty, loadedInvoiceLine.JI_Tariff);
			AssertEquals("Description on InvoiceLine", PartDescription, ReLoadInvoiceLineDescription(loadedInvoiceLine));
			AssertEquals("Extra Info For Classification on InvoiceLine", ExpectedPartExtendedCommercialDescription, loadedInvoiceLine.JI_ExtraInfoForClassification);

			ChangeLookupOnPartInAnotherFactory(Part.PK, secondLookup.PK);
			AssertEquals("Part Number on InvoiceLine", PartNumber, loadedInvoiceLine.JI_PartNo);
			AssertEquals("LoadedInvoiceLine.Part.Classifications.Count", 1, loadedInvoiceLine.Part.ClassificationsForBinding.Count);
			AssertEquals("Lookup Code on InvoiceLine", secondLookup.PK, loadedInvoiceLine.JI_CC);
			AssertEquals("Tariff Code on InvoiceLine", TariffCode, loadedInvoiceLine.JI_Tariff);
			AssertEquals("Description on InvoiceLine", PartDescription, ReLoadInvoiceLineDescription(loadedInvoiceLine));
			AssertEquals("Extra Info For Classification on InvoiceLine", ExpectedPartExtendedCommercialDescription, loadedInvoiceLine.JI_ExtraInfoForClassification);

			ChangeLookupOnPartInAnotherFactory(Part.PK, Lookup.PK);
			AssertEquals("Part Number on InvoiceLine", PartNumber, loadedInvoiceLine.JI_PartNo);
			AssertEquals("LoadedInvoiceLine.Part.Classifications.Count", 1, loadedInvoiceLine.Part.ClassificationsForBinding.Count);
			AssertEquals("Lookup Code on InvoiceLine", Lookup.PK, loadedInvoiceLine.JI_CC);
			AssertEquals("Tariff Code on InvoiceLine", ZString.Empty, loadedInvoiceLine.JI_Tariff);
			AssertEquals("Description on InvoiceLine", PartDescription, ReLoadInvoiceLineDescription(loadedInvoiceLine));
			AssertEquals("Extra Info For Classification on InvoiceLine", ExpectedPartExtendedCommercialDescription, loadedInvoiceLine.JI_ExtraInfoForClassification);

			ChangeLookupOnPartInAnotherFactory(Part.PK, secondLookup.PK);
			AssertEquals("Part Number on InvoiceLine", PartNumber, loadedInvoiceLine.JI_PartNo);
			AssertEquals("LoadedInvoiceLine.Part.Classifications.Count", 1, loadedInvoiceLine.Part.ClassificationsForBinding.Count);
			AssertEquals("Lookup Code on InvoiceLine", secondLookup.PK, loadedInvoiceLine.JI_CC);
			AssertEquals("Tariff Code on InvoiceLine", TariffCode, loadedInvoiceLine.JI_Tariff);
			AssertEquals("Description on InvoiceLine", PartDescription, ReLoadInvoiceLineDescription(loadedInvoiceLine));
			AssertEquals("Extra Info For Classification on InvoiceLine", ExpectedPartExtendedCommercialDescription, loadedInvoiceLine.JI_ExtraInfoForClassification);
		}

		public void TestSetJI_LineDescription()
		{
			var part = Factory.New<OrgSupplierPart>();
			Importer.OH_Code = "~~~";
			part.OP_PartNum = "APPLE";
			part.OP_Desc = "FRUIT";
			part.RelatedOrganisations.RemoveAndDeleteAll();
			part.RelatedOrganisations.AddOrganisationIfNotExist(Importer.PK, OrgPartRelation.RelationshipTypes.Owner);

			var part1 = Factory.New<OrgSupplierPart>();
			Importer.OH_Code = "!!!";
			part1.OP_PartNum = "BANANA";
			part1.OP_Desc = "FRUIT";
			part1.RelatedOrganisations.RemoveAndDeleteAll();
			part1.RelatedOrganisations.AddOrganisationIfNotExist(Importer.PK, OrgPartRelation.RelationshipTypes.Both);

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration1.JE_OH_Importer = Importer.PK;
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine1 = declaration1.Invoices.AddNew().InvoiceLines.AddNew();

			var provider = invoiceLine.GetClassificationTypeProvider();

			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_OP = part.PK;
			pivot.CI_Description = "FRUIT APPLE";
			pivot.CI_OH = Importer.PK;
			pivot.CI_ChildType = provider.HTICode;
			pivot.CI_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var pivot1 = Factory.New<BaseCusClassPartPivot>();
			pivot1.CI_OP = part1.PK;
			pivot1.CI_OH = Importer.PK;
			pivot1.CI_ChildType = provider.HTICode;
			pivot1.CI_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			invoiceLine.JI_PartNo = "APPLE";
			AssertEquals("JI_Description should be set to description of classification", "FRUIT APPLE", invoiceLine.JI_Description);

			invoiceLine1.JI_PartNo = "BANANA";
			AssertEquals("JI_Description should be set to description of product", "FRUIT", invoiceLine1.JI_Description);
		}

		public void TestNonWesternEuropeanCharsAreRemoved()
		{
			var part = Factory.New<OrgSupplierPart>();
			Importer.OH_Code = "~~~";
			part.OP_PartNum = "APPLE";
			part.OP_Desc = "Fancy Λpple";
			part.RelatedOrganisations.RemoveAndDeleteAll();
			part.RelatedOrganisations.AddOrganisationIfNotExist(Importer.PK, OrgPartRelation.RelationshipTypes.Owner);

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration1.JE_OH_Importer = Importer.PK;
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;

			var provider = invoiceLine.GetClassificationTypeProvider();

			var pivot = Factory.New<BaseCusClassPartPivot>();
			pivot.CI_OP = part.PK;
			pivot.CI_Description = "Fancy Λpple";
			pivot.CI_OH = Importer.PK;
			pivot.CI_ChildType = provider.HTICode;
			pivot.CI_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			invoiceLine.JI_PartNo = "APPLE";
			AssertEquals("JI_Description should not contain Non-W-E Chars", "FANCY PPLE", invoiceLine.JI_Description);
		}

		protected virtual BaseJobDeclaration CreateBaseJobDeclarationForMerge() => BaseJobDeclaration.New(Factory);

		protected virtual ZString MessageType
		{
			get { return JobMessageTypeList.Codes.Export; }
		}

		protected virtual ZString ExpectedDescriptionFromMergeOfOneLine
		{
			get { return "LINE"; }
		}

		protected virtual ZString ExpectedDescriptionFromMergeOfMultipleLines
		{
			get { return TariffDescription; }
		}

		protected virtual void DoMerge(BaseJobDeclaration declaration)
		{
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
		}

		protected virtual ZString InvoiceLineDescription
		{
			get => InvoiceLine.JI_Description;
			set => InvoiceLine.JI_Description = value;
		}

		protected virtual ZString ReLoadInvoiceLineDescription(BaseJobComInvoiceLine invoiceLine) => invoiceLine.JI_Description;

		protected virtual void ChangeLookupOnPartInAnotherFactory(ZGuid partPK, ZGuid newClassificationPK)
		{
			BusinessObjectFactory anotherNewFactory = new BusinessObjectFactory();
			OrgSupplierPart loadedPart = anotherNewFactory.Load<OrgSupplierPart>(partPK);
			BaseCusClassification newClassInAnotherFactory = anotherNewFactory.Load<BaseCusClassification>(newClassificationPK);

			foreach (BaseCusClassPartPivot p in loadedPart.PivotsForBinding)
			{
				if (p.CI_CC.IsValid)
				{
					p.CI_CC = newClassificationPK;
				}
			}
			anotherNewFactory.Save();
			anotherNewFactory.ForcePublishForDataRefresh(loadedPart);
		}

		protected const string UserDescription = "USER_INVOICE_LINE_DESCRIPTION";

		protected abstract ZString TariffCode { get; }
		protected abstract ZString TariffDescription { get; }
		protected abstract ZString TariffCode2 { get; }
		protected abstract ZString TariffDescription2 { get; }

		protected const string LookupCode = "LOOKUPCODE";
		protected const string LookupDescription = "LOOKUP_DESCRIPTION";
		protected const string PartNumber = "PART_CODE";
		protected const string PartDescription = "PART_DESCRIPTION";
		protected const string PartExtendedCommercialDescription = "PART_EXTENDED_DESCRIPTION";
		protected virtual string ExpectedPartExtendedCommercialDescription
		{
			get { return ""; }
		}

		#region ReLoadInvoiceLine
		BaseJobComInvoiceLine ReLoadInvoiceLine(BusinessObjectFactory factory, BaseJobDeclaration declaration)
		{
			BaseJobDeclaration loadedDeclaration = (BaseJobDeclaration)factory.Load(DeclarationTypeForTest, declaration.PK);
			return loadedDeclaration.FilteredInvoiceLines[0];
		}

		#endregion

		protected BaseJobComInvoiceLine InvoiceLine
		{
			get
			{
				if (fInvoiceLine == null)
				{
					fInvoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew();
				}
				return fInvoiceLine;
			}
		}
		BaseJobComInvoiceLine fInvoiceLine;

		protected BaseJobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = GetNewJobDeclaration();
					fDeclaration.JE_OH_Importer = Importer.PK;
				}
				return fDeclaration;
			}
		}
		BaseJobDeclaration fDeclaration;

		protected virtual BaseJobDeclaration GetNewJobDeclaration()
		{
			return (BaseJobDeclaration)Factory.New(DeclarationTypeForTest);
		}

		protected abstract Type DeclarationTypeForTest { get; }

		protected BaseJobComInvoiceHeader InvoiceHeader
		{
			get
			{
				if (fInvoiceHeader == null)
				{
					fInvoiceHeader = Declaration.Invoices.AddNew();
					fInvoiceHeader.JZ_OH_Buyer = Importer.PK;
					fInvoiceHeader.JZ_OH_Supplier = Supplier.PK;
				}
				return fInvoiceHeader;
			}
		}
		BaseJobComInvoiceHeader fInvoiceHeader;

		protected BaseCusClassification Lookup
		{
			get
			{
				if (fLookup == null)
				{
					fLookup = GetNewLookup();
				}
				return fLookup;
			}
		}
		BaseCusClassification fLookup;
		protected OrgSupplierPart Part
		{
			get
			{
				if (fPart == null)
				{
					fPart = Factory.New<OrgSupplierPart>();
					fPart.OP_PartNum = PartNumber;
					fPart.OP_Desc = PartDescription;
					fPart.Notes.AddNew(false, PredefinedNoteTypes.Instance.ExtendedCommercialDescription.Description, PartExtendedCommercialDescription);

					AddLookupToPart(fPart, Lookup);
					AddRelatedOrganisationToPart(fPart, Supplier, OrgPartRelation.RelationshipTypes.Supplier);
					AddRelatedOrganisationToPart(fPart, Importer, OrgPartRelation.RelationshipTypes.Owner);

					Factory.Save();
				}
				return fPart;
			}
		}
		OrgSupplierPart fPart;
		protected OrgHeader Importer
		{
			get
			{
				if (fImporter == null)
				{
					fImporter = OrgHeader.New(Factory);
					fImporter.FillWithValidTestData();
					fImporter.OH_IsConsignee = true;
					fImporter.OH_RL_NKClosestPort = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					fImporter.OH_FullName = "IMPORTER";
					fImporter.OH_Code = "ZZIMPZZZ";
				}
				return fImporter;
			}
		}
		OrgHeader fImporter;
		protected OrgHeader Supplier
		{
			get
			{
				if (fSupplier == null)
				{
					fSupplier = OrgHeader.New(Factory);
					fSupplier.FillWithValidTestData();
					fSupplier.OH_IsConsignor = true;
					fSupplier.OH_RL_NKClosestPort = "ZZZZZ";
					fSupplier.OH_FullName = "SUPPLIER";
					fSupplier.OH_Code = "ZZSUPZZZ";
				}
				return fSupplier;
			}
		}
		OrgHeader fSupplier;

		protected virtual BaseCusClassification GetNewLookup()
		{
			BaseCusClassification result = Factory.New<BaseCusClassification>(); // BaseCusClassification.New(Factory)
			result.CC_LookupCode = LookupCode;
			result.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
			result.CC_TariffNum = TariffCode;
			result.CC_Description = LookupDescription;
			return result;
		}

		protected virtual void AddLookupToPart(OrgSupplierPart part, BaseCusClassification lookup)
		{
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_CC = lookup.PK;
			pivot.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTB;
		}

		protected void AddRelatedOrganisationToPart(OrgSupplierPart part, OrgHeader organisation, ZString relationshipType)
		{
			OrgPartRelation partRelation = part.RelatedOrganisations.AddNew();
			partRelation.OU_Relationship = relationshipType;
			partRelation.OU_OH = organisation.PK;
		}
	}
}
