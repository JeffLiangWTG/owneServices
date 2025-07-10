using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	sealed class OrgContainerPenaltyMatcherTest : TestCaseWithFactory
	{
		public void TestFallBackToRegistry()
		{
			AssertImportContainerPenalty(Carrier, null, "", "", "", Company, CompanyDefaultFreeDays);
			AssertExportContainerFreeDays(Carrier, null, "", "", "", Company, CompanyDefaultFreeDays);

			AssertDeliveryContainerPenalty(Carrier, null, "", "", "", Company, CompanyDefaultFreeDays);
			AssertPickupContainerFreeDays(Carrier, null, "", "", "", Company, CompanyDefaultFreeDays);
		}

		public void TestCalculateDetentionFreeDays_CarrierOnly()
		{
			CarrierImportDetention.PD_FreeDays = 2;
			CarrierExportDetention.PD_FreeDays = 3;

			AssertImportContainerPenalty(Carrier, Client, "", "", "", Company, 2);
			AssertExportContainerFreeDays(Carrier, Client, "", "", "", Company, 3);

			AssertDeliveryContainerPenalty(Carrier, Client, "", "", "", Company, CompanyDefaultFreeDays);
			AssertPickupContainerFreeDays(Carrier, Client, "", "", "", Company, CompanyDefaultFreeDays);
		}

		public void TestCalculateDetentionFreeDays_ClientOnly()
		{
			ImporterDetention.PD_FreeDays = 2;
			ExporterDetention.PD_FreeDays = 3;

			AssertImportContainerPenalty(Carrier, Client, "", "", "", Company, CompanyDefaultFreeDays);
			AssertExportContainerFreeDays(Carrier, Client, "", "", "", Company, CompanyDefaultFreeDays);

			AssertDeliveryContainerPenalty(Carrier, Client, "", "", "", Company, 2);
			AssertPickupContainerFreeDays(Carrier, Client, "", "", "", Company, 3);

			ImporterDetention.PD_OH_Carrier = Carrier.PK;
			ExporterDetention.PD_OH_Carrier = Carrier.PK;

			AssertImportContainerPenalty(Carrier, Client, "", "", "", Company, 2);
			AssertExportContainerFreeDays(Carrier, Client, "", "", "", Company, 3);

			AssertDeliveryContainerPenalty(Carrier, Client, "", "", "", Company, 2);
			AssertPickupContainerFreeDays(Carrier, Client, "", "", "", Company, 3);
		}

		public void TestCalculateDetentionFreeDays_CarrierAndClient()
		{
			CarrierImportDetention.PD_FreeDays = 2;
			ImporterDetention.PD_FreeDays = 3;

			CarrierExportDetention.PD_FreeDays = 4;
			ExporterDetention.PD_FreeDays = 5;

			AssertImportContainerPenalty(Carrier, Client, "", "", "", Company, 2);
			AssertExportContainerFreeDays(Carrier, Client, "", "", "", Company, 4);

			AssertDeliveryContainerPenalty(Carrier, Client, "", "", "", Company, 3);
			AssertPickupContainerFreeDays(Carrier, Client, "", "", "", Company, 5);
		}

		public void TestCalculateDetentionFreeDays_ContainerClass()
		{
			CarrierImportDetention.PD_FreeDays = 2;
			CarrierImportDetention.PD_ContainerType = "20F";
			CarrierImportDetention2.PD_FreeDays = 3;

			CarrierExportDetention.PD_FreeDays = 4;
			CarrierExportDetention.PD_ContainerType = "20F";
			CarrierExportDetention2.PD_FreeDays = 5;

			AssertImportContainerPenalty(Carrier, null, "", "", "20F", Company, 2);
			AssertImportContainerPenalty(Carrier, null, "", "", "40F", Company, 3);

			AssertExportContainerFreeDays(Carrier, null, "", "", "20F", Company, 4);
			AssertExportContainerFreeDays(Carrier, null, "", "", "40F", Company, 5);

			AssertDeliveryContainerPenalty(Carrier, null, "", "", "20F", Company, CompanyDefaultFreeDays);
			AssertDeliveryContainerPenalty(Carrier, null, "", "", "40F", Company, CompanyDefaultFreeDays);

			AssertPickupContainerFreeDays(Carrier, null, "", "", "20F", Company, CompanyDefaultFreeDays);
			AssertPickupContainerFreeDays(Carrier, null, "", "", "40F", Company, CompanyDefaultFreeDays);
		}

		public void TestCalculateDetentionFreeDays_Origin()
		{
			CarrierImportDetention.PD_FreeDays = 2;
			CarrierImportDetention.PD_OriginPortOrCountry = "AUBNE";
			CarrierImportDetention2.PD_FreeDays = 3;
			CarrierImportDetention2.PD_OriginPortOrCountry = "AU";
			CarrierImportDetention3.PD_FreeDays = 4;
			CarrierImportDetention3.PD_OriginPortOrCountry = "NZ";
			CarrierImportDetention4.PD_FreeDays = 5;
			CarrierImportDetention4.PD_OriginPortOrCountry = "";

			CarrierExportDetention.PD_FreeDays = 6;
			CarrierExportDetention.PD_OriginPortOrCountry = "AUBNE";
			CarrierExportDetention2.PD_FreeDays = 7;
			CarrierExportDetention2.PD_OriginPortOrCountry = "AU";
			CarrierExportDetention3.PD_FreeDays = 8;
			CarrierExportDetention3.PD_OriginPortOrCountry = "NZ";
			CarrierExportDetention4.PD_FreeDays = 9;
			CarrierExportDetention4.PD_OriginPortOrCountry = "";

			AssertImportContainerPenalty(Carrier, Client, "AUBNE", "", "", Company, 2);
			AssertImportContainerPenalty(Carrier, Client, "AUSYD", "", "", Company, 3);
			AssertImportContainerPenalty(Carrier, Client, "SGSIN", "", "", Company, 5);

			AssertExportContainerFreeDays(Carrier, Client, "AUBNE", "", "", Company, 6);
			AssertExportContainerFreeDays(Carrier, Client, "AUSYD", "", "", Company, 7);
			AssertExportContainerFreeDays(Carrier, Client, "SGSIN", "", "", Company, 9);

			AssertDeliveryContainerPenalty(Carrier, Client, "AUBNE", "", "", Company, CompanyDefaultFreeDays);
			AssertDeliveryContainerPenalty(Carrier, Client, "AUSYD", "", "", Company, CompanyDefaultFreeDays);
			AssertDeliveryContainerPenalty(Carrier, Client, "SGSIN", "", "", Company, CompanyDefaultFreeDays);

			AssertPickupContainerFreeDays(Carrier, Client, "AUBNE", "", "", Company, CompanyDefaultFreeDays);
			AssertPickupContainerFreeDays(Carrier, Client, "AUSYD", "", "", Company, CompanyDefaultFreeDays);
			AssertPickupContainerFreeDays(Carrier, Client, "SGSIN", "", "", Company, CompanyDefaultFreeDays);
		}

		public void TestCalculateDetentionFreeDays_DetentionPort()
		{
			CarrierImportDetention.PD_FreeDays = 2;
			CarrierImportDetention.PD_DetentionPortOrCountry = "AUBNE";
			CarrierImportDetention2.PD_FreeDays = 3;
			CarrierImportDetention2.PD_DetentionPortOrCountry = "AU";
			CarrierImportDetention3.PD_FreeDays = 4;
			CarrierImportDetention3.PD_DetentionPortOrCountry = "NZ";
			CarrierImportDetention4.PD_FreeDays = 5;
			CarrierImportDetention4.PD_DetentionPortOrCountry = "";

			CarrierExportDetention.PD_FreeDays = 6;
			CarrierExportDetention.PD_DetentionPortOrCountry = "AUBNE";
			CarrierExportDetention2.PD_FreeDays = 7;
			CarrierExportDetention2.PD_DetentionPortOrCountry = "AU";
			CarrierExportDetention3.PD_FreeDays = 8;
			CarrierExportDetention3.PD_DetentionPortOrCountry = "NZ";
			CarrierExportDetention4.PD_FreeDays = 9;
			CarrierExportDetention4.PD_DetentionPortOrCountry = "";

			AssertImportContainerPenalty(Carrier, Client, "", "AUBNE", "", Company, 2);
			AssertImportContainerPenalty(Carrier, Client, "", "AUSYD", "", Company, 3);
			AssertImportContainerPenalty(Carrier, Client, "", "SGSIN", "", Company, 5);

			AssertExportContainerFreeDays(Carrier, Client, "", "AUBNE", "", Company, 6);
			AssertExportContainerFreeDays(Carrier, Client, "", "AUSYD", "", Company, 7);
			AssertExportContainerFreeDays(Carrier, Client, "", "SGSIN", "", Company, 9);

			AssertDeliveryContainerPenalty(Carrier, Client, "", "AUBNE", "", Company, CompanyDefaultFreeDays);
			AssertDeliveryContainerPenalty(Carrier, Client, "", "AUSYD", "", Company, CompanyDefaultFreeDays);
			AssertDeliveryContainerPenalty(Carrier, Client, "", "SGSIN", "", Company, CompanyDefaultFreeDays);

			AssertPickupContainerFreeDays(Carrier, Client, "", "AUBNE", "", Company, CompanyDefaultFreeDays);
			AssertPickupContainerFreeDays(Carrier, Client, "", "AUSYD", "", Company, CompanyDefaultFreeDays);
			AssertPickupContainerFreeDays(Carrier, Client, "", "SGSIN", "", Company, CompanyDefaultFreeDays);
		}

		[ExpectNoExceptions]
		public void TestCalculateDetentionFreeDays_CarrierAndImporterNull()
		{
			CarrierImportDetention.PD_FreeDays = 2;
			ImporterDetention.PD_FreeDays = 3;

			CarrierExportDetention.PD_FreeDays = 4;
			ExporterDetention.PD_FreeDays = 5;

			AssertImportContainerPenalty(null, null, "", "", "", Company, CompanyDefaultFreeDays);
			AssertExportContainerFreeDays(null, null, "", "", "", Company, CompanyDefaultFreeDays);

			AssertDeliveryContainerPenalty(null, null, "", "", "", Company, CompanyDefaultFreeDays);
			AssertPickupContainerFreeDays(null, null, "", "", "", Company, CompanyDefaultFreeDays);
		}

		public void TestCalculateCTOStorageFreeDays()
		{
			var collection = new OrgContainerDetentionCollection(Factory);
			var cto1 = Factory.NewWithValidTestData<OrgHeader>();
			var cto2 = Factory.NewWithValidTestData<OrgHeader>();
			var client1 = Factory.NewWithValidTestData<OrgHeader>();
			var client2 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier1 = Factory.NewWithValidTestData<OrgHeader>();
			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			var detention = collection.AddNew();

			foreach (var creditorType in new[] { ContainerPenaltyCreditorType.Codes.Carrier, ContainerPenaltyCreditorType.Codes.CTO })
			{
				foreach (var carrierPK in new[] { ZGuid.Empty, carrier1.PK, carrier2.PK })
				{
					foreach (var clientPK in new[] { ZGuid.Empty, client1.PK, client2.PK })
					{
						foreach (var ctoPK in new[] { ZGuid.Empty, cto1.PK, cto2.PK })
						{
							foreach (var portOrCountry in new[] { string.Empty, "AU", "CN", "AUSYD", "CNSHA" })
							{
								foreach (var containerType in new[] { string.Empty, "20F", "40R" })
								{
									FillCTOStorage(detention, 99, creditorType, ContainerDetentionDirection.Export, carrierPK, clientPK, ctoPK, portOrCountry, containerType);

									Factory.Save();

									var matchResult = new OrgContainerPenaltyMatcher().MatchStorage(new ContainerPenaltyMatchFilter
									{
										Carrier = carrier2,
										Client = client1,
										CTO = cto2,
										OriginPort = ZString.Empty,
										DetentionPort = "CNSHA",
										ContainerClass = "40R",
										Direction = ContainerDetentionDirection.Export,
										CreditorType = ContainerPenaltyCreditorType.Codes.CTO,
										ProcessType = Core.Constants.ContainerPenaltyProcessType.Export
									});

									if (creditorType == ContainerPenaltyCreditorType.Codes.Carrier ||
										(carrierPK == ZGuid.Empty && clientPK != ZGuid.Empty) ||
										carrierPK == carrier1.PK ||
										clientPK == client2.PK ||
										ctoPK == cto1.PK ||
										portOrCountry == "AU" ||
										portOrCountry == "AUSYD" ||
										containerType == "20F")
									{
										AssertNull(matchResult);
									}
									else
									{
										AssertNotNull(matchResult);
									}
								}
							}
						}
					}
				}
			}

			detention.Delete();

			foreach (var creditorType in new[] { ContainerPenaltyCreditorType.Codes.Carrier, ContainerPenaltyCreditorType.Codes.CTO })
			{
				foreach (var carrierPK in new[] { ZGuid.Empty, carrier1.PK, carrier2.PK })
				{
					foreach (var clientPK in new[] { ZGuid.Empty, client1.PK, client2.PK })
					{
						foreach (var ctoPK in new[] { ZGuid.Empty, cto1.PK, cto2.PK })
						{
							foreach (var portOrCountry in new[] { string.Empty, "AU", "CN", "AUSYD", "CNSHA" })
							{
								foreach (var containerType in new[] { string.Empty, "20F", "40R" })
								{
									FillCTOStorage(collection.AddNew(), 99, creditorType, ContainerDetentionDirection.Export, carrierPK, clientPK, ctoPK, portOrCountry, containerType);
								}
							}
						}
					}
				}
			}

			Factory.Save();

			var result = new OrgContainerPenaltyMatcher().MatchStorage(new ContainerPenaltyMatchFilter
			{
				Carrier = carrier2,
				Client = client1,
				CTO = cto2,
				OriginPort = ZString.Empty,
				DetentionPort = "CNSHA",
				ContainerClass = "40R",
				Direction = ContainerDetentionDirection.Export,
				CreditorType = ContainerPenaltyCreditorType.Codes.CTO,
				ProcessType = Core.Constants.ContainerPenaltyProcessType.Import
			});

			AssertNotNull(result);
			var matchedDetention = result as OrgContainerDetention;
			AssertEquals(ContainerPenaltyCreditorType.Codes.CTO, matchedDetention.PD_CreditorType);
			AssertEquals(carrier2.PK, matchedDetention.PD_OH_Carrier);
			AssertEquals(client1.PK, matchedDetention.PD_OH_Client);
			AssertEquals(cto2.PK, matchedDetention.PD_OH_CTO);
			AssertEquals("CNSHA", matchedDetention.PD_DetentionPortOrCountry);
			AssertEquals("40R", matchedDetention.PD_ContainerType);
		}

		#region Implementation

		const byte CompanyDefaultFreeDays = 11;

		void FillCTOStorage(OrgContainerDetention ctoStorage, ZByte freeDays, ZString creditorType, ZString direction, ZGuid carrierPK, ZGuid clientPK, ZGuid ctoPK, ZString portOrCountry, ZString containerType)
		{
			ctoStorage.PD_PenaltyType = ContainerDetentionPenaltyType.STO;
			ctoStorage.PD_FreeDays = freeDays;
			ctoStorage.PD_CreditorType = creditorType;
			ctoStorage.PD_Direction = direction;
			ctoStorage.PD_OH_Carrier = carrierPK;
			ctoStorage.PD_OH_Client = clientPK;
			ctoStorage.PD_OH_CTO = ctoPK;
			ctoStorage.PD_ContainerType = containerType;
			ctoStorage.PD_DetentionPortOrCountry = portOrCountry;
		}

		void AssertImportContainerPenalty(OrgHeader carrier, OrgHeader importer, ZString originPort, ZString detentionPort, ZString containerClass, GlbCompany company, ZByte? expectedFreeDays)
		{
			var filter = new ContainerPenaltyMatchFilter
			{
				Carrier = carrier,
				Client = importer,
				OriginPort = originPort,
				DetentionPort = detentionPort,
				ContainerClass = containerClass,
				Direction = ContainerDetentionDirection.Import,
				ProcessType = Core.Constants.ContainerPenaltyProcessType.Import,
				Company = company
			};
			var actualFreeDays = new OrgContainerPenaltyMatcher().MatchDetention(filter)?.FreeDays ?? new RegistryPenaltyMatcher().MatchDetention(filter)?.FreeDays;
			AssertEquals(expectedFreeDays, actualFreeDays);
		}

		void AssertExportContainerFreeDays(OrgHeader carrier, OrgHeader exporter, ZString originPort, ZString detentionPort, ZString containerClass, GlbCompany company, ZByte? expectedFreeDays)
		{
			var filter = new ContainerPenaltyMatchFilter
			{
				Carrier = carrier,
				Client = exporter,
				OriginPort = originPort,
				DetentionPort = detentionPort,
				ContainerClass = containerClass,
				Direction = ContainerDetentionDirection.Export,
				ProcessType = Core.Constants.ContainerPenaltyProcessType.Export,
				Company = company
			};

			var actualFreeDays = new OrgContainerPenaltyMatcher().MatchDetention(filter)?.FreeDays ?? new RegistryPenaltyMatcher().MatchDetention(filter)?.FreeDays;
			AssertEquals(expectedFreeDays, actualFreeDays);
		}

		void AssertDeliveryContainerPenalty(OrgHeader carrier, OrgHeader importer, ZString originPort, ZString detentionPort, ZString containerClass, GlbCompany company, ZByte? expectedFreeDays)
		{
			var filter = new ContainerPenaltyMatchFilter
			{
				Carrier = carrier,
				Client = importer,
				OriginPort = originPort,
				DetentionPort = detentionPort,
				ContainerClass = containerClass,
				Direction = ContainerDetentionDirection.Import,
				ProcessType = Core.Constants.ContainerPenaltyProcessType.Delivery,
				Company = company
			};

			var actualFreeDays = new OrgContainerPenaltyMatcher().MatchDetention(filter)?.FreeDays ?? new RegistryPenaltyMatcher().MatchDetention(filter)?.FreeDays;
			AssertEquals(expectedFreeDays, actualFreeDays);
		}

		void AssertPickupContainerFreeDays(OrgHeader carrier, OrgHeader exporter, ZString originPort, ZString detentionPort, ZString containerClass, GlbCompany company, ZByte? expectedFreeDays)
		{
			var filter = new ContainerPenaltyMatchFilter
			{
				Carrier = carrier,
				Client = exporter,
				OriginPort = originPort,
				DetentionPort = detentionPort,
				ContainerClass = containerClass,
				Direction = ContainerDetentionDirection.Export,
				ProcessType = Core.Constants.ContainerPenaltyProcessType.Pickup,
				Company = company
			};
			var actualFreeDays = new OrgContainerPenaltyMatcher().MatchDetention(filter)?.FreeDays ?? new RegistryPenaltyMatcher().MatchDetention(filter)?.FreeDays;
			AssertEquals(expectedFreeDays, actualFreeDays);
		}

		GlbCompany Company
		{
			get { return Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "SIN"); }
		}

		OrgHeader Carrier
		{
			get
			{
				if (carrier == null)
				{
					carrier = Factory.NewWithValidTestData<OrgHeader>();
				}
				return carrier;
			}
		}
		OrgHeader carrier;

		OrgHeader Client
		{
			get
			{
				if (client == null)
				{
					client = Factory.NewWithValidTestData<OrgHeader>();
				}
				return client;
			}
		}
		OrgHeader client;

		OrgContainerDetention CarrierImportDetention
		{
			get
			{
				if (carrierImportDetention == null)
				{
					carrierImportDetention = Carrier.CarrierContainerPenalties.AddNew();
					carrierImportDetention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
					carrierImportDetention.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
				}
				return carrierImportDetention;
			}
		}
		OrgContainerDetention carrierImportDetention;

		OrgContainerDetention CarrierImportDetention2
		{
			get
			{
				if (carrierImportDetention2 == null)
				{
					carrierImportDetention2 = Carrier.CarrierContainerPenalties.AddNew();
					carrierImportDetention2.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
					carrierImportDetention2.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
				}
				return carrierImportDetention2;
			}
		}
		OrgContainerDetention carrierImportDetention2;

		OrgContainerDetention CarrierImportDetention3
		{
			get
			{
				if (carrierImportDetention3 == null)
				{
					carrierImportDetention3 = Carrier.CarrierContainerPenalties.AddNew();
					carrierImportDetention3.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
					carrierImportDetention3.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
				}
				return carrierImportDetention3;
			}
		}
		OrgContainerDetention carrierImportDetention3;

		OrgContainerDetention CarrierImportDetention4
		{
			get
			{
				if (carrierImportDetention4 == null)
				{
					carrierImportDetention4 = Carrier.CarrierContainerPenalties.AddNew();
					carrierImportDetention4.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
					carrierImportDetention4.PD_Direction = Core.Constants.ContainerDetentionDirection.Import;
				}
				return carrierImportDetention4;
			}
		}
		OrgContainerDetention carrierImportDetention4;

		OrgContainerDetention CarrierExportDetention
		{
			get
			{
				if (carrierExportDetention == null)
				{
					carrierExportDetention = Carrier.CarrierContainerPenalties.AddNew();
					carrierExportDetention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
					carrierExportDetention.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
				}
				return carrierExportDetention;
			}
		}
		OrgContainerDetention carrierExportDetention;

		OrgContainerDetention CarrierExportDetention2
		{
			get
			{
				if (carrierExportDetention2 == null)
				{
					carrierExportDetention2 = Carrier.CarrierContainerPenalties.AddNew();
					carrierExportDetention2.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
					carrierExportDetention2.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
				}
				return carrierExportDetention2;
			}
		}
		OrgContainerDetention carrierExportDetention2;

		OrgContainerDetention CarrierExportDetention3
		{
			get
			{
				if (carrierExportDetention3 == null)
				{
					carrierExportDetention3 = Carrier.CarrierContainerPenalties.AddNew();
					carrierExportDetention3.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
					carrierExportDetention3.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
				}
				return carrierExportDetention3;
			}
		}
		OrgContainerDetention carrierExportDetention3;

		OrgContainerDetention CarrierExportDetention4
		{
			get
			{
				if (carrierExportDetention4 == null)
				{
					carrierExportDetention4 = Carrier.CarrierContainerPenalties.AddNew();
					carrierExportDetention4.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
					carrierExportDetention4.PD_Direction = Core.Constants.ContainerDetentionDirection.Export;
				}
				return carrierExportDetention4;
			}
		}
		OrgContainerDetention carrierExportDetention4;

		OrgContainerDetention ImporterDetention
		{
			get
			{
				if (importerDetention == null)
				{
					importerDetention = Client.ConsigneeContainerPenalties.AddNew();
					importerDetention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
				}
				return importerDetention;
			}
		}
		OrgContainerDetention importerDetention;

		OrgContainerDetention ExporterDetention
		{
			get
			{
				if (exporterDetention == null)
				{
					exporterDetention = Client.ConsignorContainerPenalties.AddNew();
					exporterDetention.PD_PenaltyType = Core.Constants.ContainerDetentionPenaltyType.DET;
				}
				return exporterDetention;
			}
		}
		OrgContainerDetention exporterDetention;

		protected override void SetUp()
		{
			base.SetUp();
			FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForImport.SetValue(Company.PK.ToGuid(), Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = CompanyDefaultFreeDays });
			FreightDataRegistry.Instance.DefaultContainerDetentionFreeDaysForExport.SetValue(Company.PK.ToGuid(), Guid.Empty, Guid.Empty, new ContainerPenaltyFreeDaysOptions { FreeDays = CompanyDefaultFreeDays });
		}

		#endregion
	}
}
