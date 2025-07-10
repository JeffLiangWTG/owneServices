using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.NZ.Business.Data.Universal.Testing
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.Customs.DataTransfer.Universal.Testing;
	using Enterprise.Customs.NZ.Registry;
	using Enterprise.Customs.Universal.Testing;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.DataTransfer.Universal;
	using Enterprise.Registry.Business.eServices;
	using Enterprise.UniversalDataBuss.DataObjects;
	using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
	using Enterprise.ZArchitecture.Schema;
	using UniversalShipment = Shipment;

	partial class JobDeclarationDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestImportDeliveryNotificationParty()
		{
			#region Setup Organization

			var declaration = Factory.New<JobDeclaration>();
			var notifyOrg = Factory.New<OrgHeader>();
			notifyOrg.OH_Code = "DELIVERTO";
			declaration.JE_OH_NotifyParty = notifyOrg.PK;

			#endregion

			var declarationDataObject = SetupDeclaration(JobMessageTypeList.Codes.Import, ZString.Empty);
			var writingManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration));
			declarationDataObject.AddOrgAddress(writingManager, declaration.NotifyParty, Constants.AddressType.DeliveryNotificationParty);
			declarationDataObject.SetDateCollection(() => new List<Date>()
			{
				new Date() { Type = DateType.EntryAuthorisation, IsEstimate = false, Value = new ZDateTime(2017, 08, 1) }
			});

			Factory.SaveForTesting();

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var newDeclarationBO = new BusinessObjectFactory().Load<JobDeclaration>(declarationBO.PK);
			AssertNotNull("DeliveryAuthority value should have been imported from universal shipment", newDeclarationBO.NotifyParty);
			AssertEquals("DELIVERTO", newDeclarationBO.NotifyParty.OH_Code);
		}

		public void TestAddFetchHintsRelatedToCommerialInvoiceLineTariff()
		{
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (NZCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var newFactory = new BusinessObjectFactory();
				var helper = new UniversalReferenceTestDataHelper(newFactory);
				var tariffType = helper.CreateNewOrGetExistingTariffType("NZ", "HSN");
				newFactory.Save();
				var tariff = helper.CreateTariff("NZ", tariffType.PK, "123456789", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), "Tariff Desc");
				newFactory.Save();

				var declarationDataObject = SetupDeclaration(JobMessageTypeList.Codes.Import, ZString.Empty);
				declarationDataObject.CommercialInfo = new CommercialInfo
				{
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>
					{
						new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>  new DataObjectList<CommercialInvoiceLine>
							{
								new CommercialInvoiceLine
								{
									HarmonisedCode = "123456789"
								}
							}))
					}
				};
				Factory.SaveForTesting();

				var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
				var declaration = reader.ReadIntoBusinessObject();
				declaration.Factory.ExecuteAllFetchHints();
				AssertEquals("Fetch hint should be added", 2, declaration.Factory.GetTableHitCount(TariffViewSchema.Constants.TableName));

				declaration.Factory.Load<Customs.Universal.TariffView>(tariff.PK);
				AssertEquals("Should not have extra db hit", 2, declaration.Factory.GetTableHitCount(TariffViewSchema.Constants.TableName));
			}
		}

		UniversalShipment SetupDeclaration(ZString messageType, ZString messageSubType)
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

			return new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				MessageType = new CodeDescriptionPair() { Code = messageType },
				MessageSubType = new CodeDescriptionPair() { Code = messageSubType }
			};
		}
	}
}
