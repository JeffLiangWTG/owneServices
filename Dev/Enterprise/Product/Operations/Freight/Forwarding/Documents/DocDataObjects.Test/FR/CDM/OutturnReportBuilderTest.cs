using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Forwarding.Documents.Testing.AssertionHelper;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;
using Transport = Enterprise.Freight.Business.Transport;

namespace Enterprise.Freight.Forwarding.Documents.Testing.FR
{
	public class OutturnReportBuilderTest : TestCaseWithFactory
	{
		#region Test Build

		public void TestBuild()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new OutturnReportBuilder(consol, parameters);
			var data = builder.Build();

			CombineAssertions(() =>
			{
				AssertEquals("ConsolNumber", "C00001000", data.ConsolNumber);
				AssertEquals("BillOfLading", consol.JK_MasterBillNum, data.BillOfLading);
				AssertEquals("PortOfOrigin", "AUSYD", data.PortOfOrigin.Code);
				AssertEquals("PortOfDestination", "FRPRA", data.PortOfDestination.Code);
				AssertEquals("OperationalPort", "FRPAR", data.OperationalPort.Code);
				AssertEquals("PCS", FrenchPortsConstants.PCS.MGI, data.PCS);
				AssertEquals("VesselName", "Miranda", data.VesselName);
				AssertEquals("VesselName", "333", data.VoyageFlightNo);
				AssertEquals("ATPReference", "QWERTY123", data.ATPReference);
				AssertEquals("ContainerMode", "FCL", data.ContainerMode.Code);
				AssertEquals("ShipmentType", "AGT", data.ShipmentType.Code);
				AssertEquals("Containers.Count", 1, data.Containers.Count);
				AssertEquals("GoodsDetails.Count", 1, data.GoodsDetails.Count);
			});

			AssertAddressData((OrgAddress)consol.SendingForwarderWithContact.OrgAddress, data.SendingForwarder);
			AssertAddressData((OrgAddress)consol.ReceivingForwarderWithContact.OrgAddress, data.ReceivingForwarder);
			AssertAddressData(consol.UnpackDepotAddress, data.CFS);
			AssertAddressData(consol.UnpackDepotAddress, data.SendingParty);
			AssertAddressData(GlbBranch.CurrentBranch.OrgProxy.MainAddress, data.CurrentUser);

			var container = data.Containers.FirstOrDefault();
			CombineAssertions(() =>
			{
				AssertEquals("Number", "AAAA0000007", container.Number);
				AssertEquals("ContainerCount", 1, container.ContainerCount);
				AssertEquals("DeliveryMode", "CY/CY", container.DeliveryMode);
				AssertEquals("ImportDepotCustomsReference", "ImportCusRef", container.ImportDepotCustomsReference);
				AssertEquals("PackingLines.Count", 1, container.PackingLines.Count);
				AssertEquals("PackingSummaries.Count", 1, container.PackingSummaries.Count);
			});

			var packingLine = container.PackingLines.FirstOrDefault();
			CombineAssertions(() =>
			{
				AssertEquals("ContainerNumber", "AAAA0000007", packingLine.ContainerNumber);
				AssertEquals("ContainerNumber", 2, packingLine.Quantity);
				AssertEquals("PackageType.Code", "PLT", packingLine.PackageType.Code);
			});

			var containerPackingSummary = container.PackingSummaries.FirstOrDefault();

			CombineAssertions(() =>
			{
				AssertEquals("ShipmentNumber", "SH0001000", containerPackingSummary.ShipmentNumber);
				AssertEquals("TotalPackages", 2, containerPackingSummary.TotalPackages);
				AssertEquals("TotalPackagesUnit", "PLT", containerPackingSummary.TotalPackagesUnit);
				AssertEquals("TotalWeight", 200m, containerPackingSummary.TotalWeight.Value);
				AssertEquals("TotalWeight.Unit", "KG", containerPackingSummary.TotalWeight.Unit.Code);
				AssertEquals("TotalVolume", 300m, containerPackingSummary.TotalVolume.Value);
				AssertEquals("TotalVolume.Unit", "M3", containerPackingSummary.TotalVolume.Unit.Code);
			});

			var goodsDetail = data.GoodsDetails.FirstOrDefault();
			CombineAssertions(() =>
			{
				AssertEquals("ShipmentNumber", "SH0001000", goodsDetail.ShipmentNumber);
				AssertEquals("HouseBillNumber", "H0000056", goodsDetail.HouseBillNumber);
				AssertEquals("PortOfOrigin", "AUSYD", goodsDetail.PortOfOrigin.Code);
				AssertEquals("MarksAndNumbers", "Marks", goodsDetail.MarksAndNumbers);
				AssertEquals("PackingLines.Count", 1, goodsDetail.PackingLines.Count);
			});

			data.ATPReference = string.Empty;
			data.PCS = string.Empty;
			data.OperationalPort.Code = string.Empty;
			data.ValidateAllIncludingChildren();

			AssertHasMessageError("OperationalPort Error", data.ATPReferenceInfo, "ATP Reference is required, Arrival Reference missing from Sailing Schedule of Consol > Routing > Last SEA leg.");
			AssertHasMessageError("PCS Error", data.PCSInfo, "PCS Code is required. (Maintain > Locations > UNLOCO of Operational Port > Local Codes - Usage PCS)");
			AssertHasMessageError("OperationalPort Error", ((Unloco)data.OperationalPort).CodeInfo, "Operational Port is required, UNLOCO missing from Consol > Arrival > CFS Address.");
		}

		public void TestGoodsDetails_ShoudIncludedSubShipments()
		{
			AssertGoodsDetails_ShoudExcluded_IncludedSubShipment(Constants.ShipmentTypes.CoLoadMaster, 2);
			AssertGoodsDetails_ShoudExcluded_IncludedSubShipment(Constants.ShipmentTypes.BlindCoLoadMaster, 2);
			AssertGoodsDetails_ShoudExcluded_IncludedSubShipment(Constants.ShipmentTypes.AssemblyMaster, 2);
			AssertGoodsDetails_ShoudExcluded_IncludedSubShipment(Constants.ShipmentTypes.BuyersConsolLead, 3, true);
		}

		public void TestPopulateContainerPackingSummariesTotalOutturnedDetails()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new OutturnReportBuilder(consol, parameters);
			var data = builder.Build();

			var container = data.Containers.FirstOrDefault();
			CombineAssertions(() =>
			{
				AssertEquals("PackingLines.Count", 1, container.PackingLines.Count);
				AssertEquals("PackingSummaries.Count", 1, container.PackingSummaries.Count);

				var packingSummary = container.PackingSummaries.FirstOrDefault();

				AssertEquals("PackingSummary.TotalOutturnedPackages", 0, packingSummary.TotalOutturnedPackages);
				AssertEquals("PackingSummary.TotalOutturnedWeight", 0M, packingSummary.TotalOutturnedWeight.Value);
				AssertEquals("PackingSummary.TotalOutturnedVolume", 0M, packingSummary.TotalOutturnedVolume.Value);
			});

			var shipment = consol.Shipments.OfType<ForwardingShipment>().FirstOrDefault();

			var packLine = shipment.OuterPackLines.OfType<PackLine>().FirstOrDefault();

			var pkgPackage = packLine.PkgPackageCollection.AddNew();
			pkgPackage.KP_PackageQty = 1;
			pkgPackage.KP_Weight = 100M;
			pkgPackage.KP_WeightUQ = Core.Constants.Weight.Kilograms;
			pkgPackage.KP_Volume = 1000M;
			pkgPackage.KP_VolumeUQ = Core.Constants.Volume.CubicMetres;

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			shipment.JS_OA_ImportReleaseDepot_ZAddress.OrgPK = orgAddress.PK;
			packLine.JL_OA_LastKnownTransitWarehouseAddress_ZAddress.OrgPK = orgAddress.PK;

			data = builder.Build();

			container = data.Containers.FirstOrDefault();
			CombineAssertions(() =>
			{
				AssertEquals("The transit Warehouse of packingLine is equal to the delivery CFS of shipment  - Org", packLine.JL_OA_LastKnownTransitWarehouseAddress_ZAddress.OrgPK, shipment.JS_OA_ImportReleaseDepot_ZAddress.OrgPK);
				AssertEquals("The transit Warehouse of packingLine is equal to the Delivery CFS of shipment  - Address", packLine.JL_OA_LastKnownTransitWarehouseAddress_ZAddress.AddressFK, shipment.JS_OA_ImportReleaseDepot_ZAddress.AddressFK);

				AssertEquals("PackingLines.Count", 1, container.PackingLines.Count);
				AssertEquals("PackingSummaries.Count", 1, container.PackingSummaries.Count);

				var packingSummary = container.PackingSummaries.FirstOrDefault();

				AssertEquals("PackingSummary.TotalOutturnedPackages", 1, packingSummary.TotalOutturnedPackages);
				AssertEquals("PackingSummary.TotalOutturnedWeight", 100M, packingSummary.TotalOutturnedWeight.Value);
				AssertEquals("PackingSummary.TotalOutturnedVolume", 1000M, packingSummary.TotalOutturnedVolume.Value);
			});

			shipment.JS_OA_ImportReleaseDepot_ZAddress.OrgPK = ZGuid.Empty;

			data = builder.Build();

			container = data.Containers.FirstOrDefault();
			CombineAssertions(() =>
			{
				AssertNotEquals("The transit Warehouse of packingLine is not equal to the delivery CFS of shipment  - Org", packLine.JL_OA_LastKnownTransitWarehouseAddress_ZAddress.OrgPK, shipment.JS_OA_ImportReleaseDepot_ZAddress.OrgPK);

				AssertEquals("PackingLines.Count", 1, container.PackingLines.Count);
				AssertEquals("PackingSummaries.Count", 1, container.PackingSummaries.Count);

				var packingSummary = container.PackingSummaries.FirstOrDefault();

				AssertEquals("PackingSummary.TotalOutturnedPackages", 0, packingSummary.TotalOutturnedPackages);
				AssertEquals("PackingSummary.TotalOutturnedWeight", 0M, packingSummary.TotalOutturnedWeight.Value);
				AssertEquals("PackingSummary.TotalOutturnedVolume", 0M, packingSummary.TotalOutturnedVolume.Value);
			});

			packLine.JL_Outturn = 2;
			packLine.JL_OutturnedWeight = 200M;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine.JL_OutturnedVolume = 2000M;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;

			data = builder.Build();

			container = data.Containers.FirstOrDefault();
			CombineAssertions(() =>
			{
				AssertEquals("PackingLines.Count", 1, container.PackingLines.Count);
				AssertEquals("PackingSummaries.Count", 1, container.PackingSummaries.Count);

				var packingSummary = container.PackingSummaries.FirstOrDefault();

				AssertEquals("PackingSummary.TotalOutturnedPackages", 2, packingSummary.TotalOutturnedPackages);
				AssertEquals("PackingSummary.TotalOutturnedWeight", 200M, packingSummary.TotalOutturnedWeight.Value);
				AssertEquals("PackingSummary.TotalOutturnedVolume", 2000M, packingSummary.TotalOutturnedVolume.Value);
			});
		}

		void AssertGoodsDetails_ShoudExcluded_IncludedSubShipment(string shipmentType, int goodsCount, bool includedMaster = false)
		{
			var consol = CreateConsol();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000008";

			var shipment = consol.Shipments.OfType<ForwardingShipment>().FirstOrDefault();
			shipment.JS_UniqueConsignRef = "SH0001";
			shipment.JS_ShipmentType = shipmentType;

			var subshipment1 = shipment.CoLoadShipments.AddNew();
			subshipment1.JS_UniqueConsignRef = "s0001";
			var packingLine2 = subshipment1.OuterPackLines.AddNew();
			packingLine2.JL_PackageCount = 20;
			var subshipment2 = shipment.CoLoadShipments.AddNew();
			subshipment2.JS_UniqueConsignRef = "s0002";
			var packingLine3 = subshipment2.OuterPackLines.AddNew();
			packingLine3.JL_PackageCount = 30;

			var parameters = new DummyDocDataObjectParameters()
			{
				Data = consol
				  .Containers
				  .OfType<ForwardingContainer>()
				  .Where(c => c.JC_ContainerNum == "AAAA0000008")
				  .ToArray()
			};
			var data = new OutturnReportBuilder(consol, parameters).Build();

			AssertEquals(goodsCount, data.GoodsDetails.Count);
			Assert(data.GoodsDetails.Any(g => g.ShipmentNumber == subshipment1.JS_UniqueConsignRef));
			Assert(data.GoodsDetails.Any(g => g.ShipmentNumber == subshipment2.JS_UniqueConsignRef));
			AssertEquals(includedMaster, data.GoodsDetails.Any(g => g.ShipmentNumber == shipment.JS_UniqueConsignRef));
		}

		public void TestHeaderPopulateAPPlusCodes()
		{
			var consol = CreateConsol();

			AssertPopulateAPPlusCodes(consol, (OrgAddress)consol.SendingForwarderWithContact.OrgAddress, "SendingForwarderSON", "SendingForwarderCI5");
			AssertPopulateAPPlusCodes(consol, (OrgAddress)consol.ReceivingForwarderWithContact.OrgAddress, "ReceivingForwarderSON", "ReceivingForwarderCI5");

			AssertPopulateAPPlusCodes(consol, consol.UnpackDepotAddress, "SendingPartySON", "SendingPartyCI5", "SendingPartySOW", "SendingPartySOA");
			AssertPopulateAPPlusCodes(consol, consol.UnpackDepotAddress, "CFSSON", "CFSCI5", "CFSSOW", "CFSSOA");
			AssertPopulateAPPlusCodes(consol, GlbBranch.CurrentBranch.OrgProxy.MainAddress, "CurrentUserSON", "CurrentUserCI5", "CurrentUserSOW", "CurrentUserSOA");
		}

		void AssertPopulateAPPlusCodes(ForwardingConsol consol, OrgAddress address, string sonPropertyName, string ci5PropertyName, string sowPropertyName = "", string soaPropertyName = "")
		{
			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new OutturnReportBuilder(consol, parameters);

			var sonCode1 = address.Header.CustomsCodes.AddNew();
			sonCode1.OK_CodeType = OrgCusCode.FranceCodeTypes.SON;
			sonCode1.OK_RN_NKCodeCountry = Constants.CountryCodes.France;
			sonCode1.OK_CustomsRegNo = "S001";

			var ci5Code1 = address.Header.CustomsCodes.AddNew();
			ci5Code1.OK_CodeType = OrgCusCode.FranceCodeTypes.CI5;
			ci5Code1.OK_RN_NKCodeCountry = Constants.CountryCodes.France;
			ci5Code1.OK_CustomsRegNo = "C001";

			var sowCode1 = address.Header.CustomsCodes.AddNew();
			sowCode1.OK_CodeType = OrgCusCode.FranceCodeTypes.SOW;
			sowCode1.OK_RN_NKCodeCountry = Constants.CountryCodes.France;
			sowCode1.OK_CustomsRegNo = "w001";

			var soaCode1 = address.Header.CustomsCodes.AddNew();
			soaCode1.OK_CodeType = OrgCusCode.FranceCodeTypes.SOA;
			soaCode1.OK_RN_NKCodeCountry = Constants.CountryCodes.France;
			soaCode1.OK_CustomsRegNo = "a001";

			var data = builder.Build();

			AssertEquals($"{sonPropertyName} should be from org", "S001", ((RegistrationNumber)data[sonPropertyName]).Value);
			AssertEquals($"{ci5PropertyName} should be from org", "C001", ((RegistrationNumber)data[ci5PropertyName]).Value);
			if (!string.IsNullOrEmpty(sowPropertyName))
			{
				AssertEquals($"{sowPropertyName} should be from org", "w001", ((RegistrationNumber)data[sowPropertyName]).Value);
			}
			if (!string.IsNullOrEmpty(soaPropertyName))
			{
				AssertEquals($"{soaPropertyName} should be from org", "a001", ((RegistrationNumber)data[soaPropertyName]).Value);
			}

			var sonCode2 = address.CustomsCodes.AddNew();
			sonCode2.OK_CodeType = OrgCusCode.FranceCodeTypes.SON;
			sonCode2.OK_RN_NKCodeCountry = Constants.CountryCodes.France;
			sonCode2.OK_CustomsRegNo = "S002";

			var ci5Code2 = address.CustomsCodes.AddNew();
			ci5Code2.OK_CodeType = OrgCusCode.FranceCodeTypes.CI5;
			ci5Code2.OK_RN_NKCodeCountry = Constants.CountryCodes.France;
			ci5Code2.OK_CustomsRegNo = "C002";

			var sowCode2 = address.CustomsCodes.AddNew();
			sowCode2.OK_CodeType = OrgCusCode.FranceCodeTypes.SOW;
			sowCode2.OK_RN_NKCodeCountry = Constants.CountryCodes.France;
			sowCode2.OK_CustomsRegNo = "w002";

			var soaCode2 = address.CustomsCodes.AddNew();
			soaCode2.OK_CodeType = OrgCusCode.FranceCodeTypes.SOA;
			soaCode2.OK_RN_NKCodeCountry = Constants.CountryCodes.France;
			soaCode2.OK_CustomsRegNo = "a002";

			data = builder.Build();

			AssertEquals($"{sonPropertyName} should be from address", "S002", ((RegistrationNumber)data[sonPropertyName]).Value);
			AssertEquals($"{ci5PropertyName} should be from address", "C002", ((RegistrationNumber)data[ci5PropertyName]).Value);
			if (!string.IsNullOrEmpty(sowPropertyName))
			{
				AssertEquals($"{sowPropertyName} should be from address", "w002", ((RegistrationNumber)data[sowPropertyName]).Value);
			}
			if (!string.IsNullOrEmpty(soaPropertyName))
			{
				AssertEquals($"{soaPropertyName} should be from address", "a002", ((RegistrationNumber)data[soaPropertyName]).Value);
			}

			address.Header.CustomsCodes.RemoveAndDeleteAll();
			address.CustomsCodes.DeleteAll();

			data = builder.Build();

			AssertEquals(string.Empty, ((RegistrationNumber)data[sonPropertyName]).Value);
			AssertEquals(string.Empty, ((RegistrationNumber)data[ci5PropertyName]).Value);
			if (!string.IsNullOrEmpty(sowPropertyName))
			{
				AssertEquals(string.Empty, ((RegistrationNumber)data[sowPropertyName]).Value);
			}
			if (!string.IsNullOrEmpty(soaPropertyName))
			{
				AssertEquals(string.Empty, ((RegistrationNumber)data[soaPropertyName]).Value);
			}
		}

		#endregion

		#region Test Validations

		public void TestContainerValidations()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new OutturnReportBuilder(consol, parameters);
			var data = builder.Build();

			var container = data.Containers.FirstOrDefault();
			var containerPackingSummary = container.PackingSummaries.FirstOrDefault();

			containerPackingSummary.TotalPackages = 0;
			containerPackingSummary.TotalWeight.Value = 0;
			containerPackingSummary.TotalVolume.Value = 0;
			containerPackingSummary.TotalOutturnedPackages = 0;
			containerPackingSummary.ICVReference = string.Empty;

			AssertHasMessageError(containerPackingSummary.TotalPackagesInfo, "Number of Packs is required.");
			AssertHasMessageError(containerPackingSummary.TotalWeight.ValueInfo, "Weight is required.");
			AssertHasMessageError(containerPackingSummary.TotalOutturnedPackagesInfo, "Number of Outturn packs is required.");
			AssertHasMessageError(containerPackingSummary.ICVReferenceInfo, "ICV Reference is required. Import Ref Number missing from Shipment > Packing > Pack Line.");

			containerPackingSummary.TotalPackages = 15;
			containerPackingSummary.TotalWeight.Value = 12.23m;
			containerPackingSummary.TotalVolume.Value = 56.44m;
			containerPackingSummary.TotalOutturnedPackages = 123;
			containerPackingSummary.ICVReference = "ABCDE";

			AssertNoMessageError(containerPackingSummary.TotalPackagesInfo, "Number of Packs is required.");
			AssertNoMessageError(containerPackingSummary.TotalWeight.ValueInfo, "Weight is required.");
			AssertNoMessageError(containerPackingSummary.TotalOutturnedPackagesInfo, "Number of Outturn packs is required.");
			AssertNoMessageError(containerPackingSummary.ICVReferenceInfo, "ICV Reference is required. Import Ref Number missing from Shipment > Packing > Pack Line.");
		}

		public void TestAddressValidations()
		{
			var consol = CreateConsol();
			consol.UnpackDepotAddress.Delete();

			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new OutturnReportBuilder(consol, parameters);
			var data = builder.Build();

			AssertHasMessageError(data.CFS.CompanyNameInfo, "CFS name and address is required.");

			data.CFS.CompanyName = "Coper";
			data.CFS.Country.Code = "CN";
			data.CFS.AddressLine1 = "Nier Address1";
			AssertNoMessageError(data.CFS.CompanyNameInfo, "CFS name and address is required.");
		}

		public void TestMissingProviderIDValidations()
		{
			var consol = Factory.New<ForwardingConsol>();
			var builder = new OutturnReportBuilder(consol, new DummyDocDataObjectParameters());
			var data = builder.Build();

			AssertRequireAPPlusIDValidation(data.FormattedCFSProviderIDInfo, data.PCS, OrgCusCode.FranceCodeTypes.SOW, "CFS", configPath: "Consol > Carrier", isOrganization: true);
			AssertRequireAPPlusIDValidation(data.FormattedSendingPartyProviderIDInfo, data.PCS, OrgCusCode.FranceCodeTypes.SOW, "Sending Party", "Sending Party", configPath: "Consol > Carrier", isOrganization: true);

			consol = CreateConsol(FrenchPortsConstants.PCS.MGI);
			builder = new OutturnReportBuilder(consol, new DummyDocDataObjectParameters());
			data = builder.Build();

			AssertRequireAPPlusIDValidation(data.FormattedCFSProviderIDInfo, data.PCS, OrgCusCode.FranceCodeTypes.SOW, "CFS", configPath: "Consol > Carrier", isOrganization: true);
			AssertRequireAPPlusIDValidation(data.FormattedSendingPartyProviderIDInfo, data.PCS, OrgCusCode.FranceCodeTypes.SOW, "Sending Party", "Sending Party", configPath: "Consol > Carrier", isOrganization: true);

			consol = CreateConsol(FrenchPortsConstants.PCS.Soget);
			builder = new OutturnReportBuilder(consol, new DummyDocDataObjectParameters());
			data = builder.Build();

			AssertRequireAPPlusIDValidation(data.FormattedCFSProviderIDInfo, data.PCS, OrgCusCode.FranceCodeTypes.SOW, "CFS", configPath: "Consol > Carrier", isOrganization: true);
			AssertRequireAPPlusIDValidation(data.FormattedSendingPartyProviderIDInfo, data.PCS, OrgCusCode.FranceCodeTypes.SOW, "Sending Party", configPath: "Consol > Carrier", isOrganization: true);
		}

		public void TestGoodsDetailsValidations()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new OutturnReportBuilder(consol, parameters);
			var data = builder.Build();

			var goodsDetail = data.GoodsDetails.FirstOrDefault();

			goodsDetail.MarksAndNumbers = string.Empty;
			goodsDetail.HouseBillNumber = string.Empty;

			AssertHasMessageError(goodsDetail.MarksAndNumbersInfo, "Marks & Numbers are required.");
			AssertHasMessageError(goodsDetail.HouseBillNumberInfo, "House Bill Number is required.");

			goodsDetail.MarksAndNumbers = "Marks";
			goodsDetail.HouseBillNumber = "HB001";
			AssertNoMessageError(goodsDetail.MarksAndNumbersInfo, "Marks & Numbers are required.");
			AssertNoMessageError(goodsDetail.HouseBillNumberInfo, "House Bill Number is required.");
		}

		public void TestPortLocationAndAreaValidation()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new OutturnReportBuilder(consol, parameters);
			var data = builder.Build();

			var container = data.Containers.FirstOrDefault();

			AssertEquals("PortOfDestination", "FRPRA", data.PortOfDestination.Code);
			AssertEquals("PortLocation", ZString.Empty, container.UnpackLocation);
			AssertEquals("PortArea", ZString.Empty, container.UnpackArea);
			AssertEquals("LPD Reference", ZString.Empty, container.LPDReference);

			AssertHasMessageErrorContaining("Port Area is required", container.UnpackAreaInfo, "Unpacking Area is missing from Organization Consol > Arrival > CFS > Config > Registration Numbers/Codes [Type=PSN] (The code before '\\').");
			AssertHasMessageErrorContaining("Port Location is required", container.UnpackLocationInfo, "Unpacking Location is missing from Organization Consol > Arrival > CFS > Config > Registration Numbers/Codes [Type=PSN] (The code after '\\').");

			AssertHasMessageErrorContaining("LPD Reference is required", container.LPDReferenceInfo, "The Outturn Report (CDM) message cannot be sent without the LPD Reference.  Please send Provisional Unpacking List (LPD) message first and wait for Acknowledgment and LPD Notification before sending Outturn Report (CDM) message.");

			data.PortOfDestination.Code = "FRBOD";

			data.ValidateAllIncludingChildren();

			AssertHasMessageError(container.UnpackLocationInfo, "Unpacking Location is missing from Organization Consol > Arrival > CFS > Config > Registration Numbers/Codes [Type=PSN] (The code after '\\').");
			AssertHasMessageError(container.UnpackAreaInfo, "Unpacking Area is missing from Organization Consol > Arrival > CFS > Config > Registration Numbers/Codes [Type=PSN] (The code before '\\').");

			container.UnpackLocation = "AELUD";
			container.UnpackArea = "CNSHI";
			container.LPDReference = "ABCDE";

			AssertNoMessageError(container.UnpackLocationInfo, "Unpacking Location is missing from Organization Consol > Arrival > CFS > Config > Registration Numbers/Codes [Type=PSN] (The code after '\\').");
			AssertNoMessageError(container.UnpackAreaInfo, "Unpacking Area is missing from Organization Consol > Arrival > CFS > Config > Registration Numbers/Codes [Type=PSN] (The code before '\\').");
			AssertNoMessageError("LPD Reference is required", container.LPDReferenceInfo, "The Outturn Report (CDM) message cannot be sent without the LPD Reference.  Please send Provisional Unpacking List (LPD) message first and wait for Acknowledgment and LPD Notification before sending Outturn Report (CDM) message.");
		}

		public void TestValidateBillOfLading()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var builder = new OutturnReportBuilder(consol, parameters);
			var data = builder.Build();

			data.BillOfLading = "B0001";
			AssertNoMessageError("BOL is required", data.BillOfLadingInfo, "Bill of Lading (BOL) Number is required.");

			data.BillOfLading = string.Empty;
			AssertHasMessageError("BOL is required", data.BillOfLadingInfo, "Bill of Lading (BOL) Number is required.");
		}

		#endregion

		#region TestContainerMode_ShippersConsol

		public void TestContainerMode_ShippersConsol()
		{
			var consol = CreateConsol();
			consol.JK_ConsolMode = Constants.ContainerModes.ShippersConsol;

			var parameters = new DummyDocDataObjectParameters
			{
				Data = consol
					.Containers
					.OfType<ForwardingContainer>()
					.ToArray()
			};

			var data = new OutturnReportBuilder(consol, parameters).Build();
			AssertEquals(Constants.ContainerModes.FCL, data.ContainerMode.Code);
			AssertEquals(Constants.ContainerModeDescriptions.FCL, data.ContainerMode.Description);
		}

		#endregion

		#region Implement

		ForwardingConsol CreateConsol(string pcs = FrenchPortsConstants.PCS.MGI)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "FRPRA";
			consol.JK_BookingReference = "B0001100";
			consol.JK_MasterBillNum = "BOL_Reference";

			consol.JK_RequiresTemperatureControl = true;
			consol.JK_RequiredTemperatureUnit = "C";
			consol.JK_RequiresTemperatureControl = true;
			consol.JK_RequiredTemperatureMinimum = 15;
			consol.JK_RequiredTemperatureMaximum = 25;

			var carrierBookingRequest = consol.Notes.AddNew();
			carrierBookingRequest.ST_Description = PredefinedNoteTypes.Instance.CarrierBookingRequest.Description;
			carrierBookingRequest.ST_NoteText = "carrier booking request";

			CreateTransports(consol);
			CreatePackingLinesAndContainers(consol);
			CreateConsolAddresses(consol, pcs);

			return consol;
		}

		void CreateTransports(ForwardingConsol consol)
		{
			var transport = consol.Transports.OfType<Transport>().Single();
			transport.JW_LegOrder = 1;
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_TransportType = Constants.TransportPlanningType.MainVessel;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.JW_Vessel = "Dragon";
			transport.JW_VoyageFlight = "111";
			transport.JW_ETD = new ZDateTime(2018, 12, 1);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_TransportMode = Constants.TransportModes.Sea;
			transport2.JW_TransportType = Constants.TransportPlanningType.Other;
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "NZAKL";
			transport2.JW_Vessel = "Steven";
			transport2.JW_VoyageFlight = "222";

			var transport3 = consol.Transports.AddNew();
			transport3.JW_LegOrder = 3;
			transport3.JW_TransportMode = Constants.TransportModes.Sea;
			transport3.JW_TransportType = Constants.TransportPlanningType.Other;
			transport3.JW_RL_NKLoadPort = "NZAKL";
			transport3.JW_RL_NKDiscPort = "FRPRA";
			transport3.JW_Vessel = "Miranda";
			transport3.JW_VoyageFlight = "333";
			transport3.JW_ETA = new ZDateTime(2019, 1, 1);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "Test Vessel Name 2";
			vessel.RV_LloydsNumber = "Lllloyd";
			vessel.RV_RadioCallSign = "Radio123";
			transport2.JW_Vessel = vessel.RV_Code;
			transport2.JW_VoyageFlight = "CC456";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = "Random Vesel";
			voyage.JV_VoyageFlight = "012";
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "SGSIN";
			destination.JB_E_ARV = new ZDateTime(2019, 1, 1);
			voyage.GenerateSailings();

			var sailing = voyage.Sailings[0];
			sailing.Origin.JA_DepartReference = "AOEUI1234";
			sailing.Destination.JB_ArrivalReference = "QWERTY123";
			transport3.JW_JX = sailing.PK;
		}

		void CreateConsolAddresses(ForwardingConsol consol, string pcs = FrenchPortsConstants.PCS.MGI)
		{
			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "MAERSK";
			sendingForwarder.OH_RL_NKClosestPort = "DKAAL";
			sendingForwarder.MainAddress.Address1 = "Unit 13";
			sendingForwarder.MainAddress.Address2 = "4 Lost Lane";
			sendingForwarder.MainAddress.City = "Aalborg";
			sendingForwarder.MainAddress.Postcode = "2000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "DK";

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "I'm Handling Stuff";
			receivingForwarder.OH_RL_NKClosestPort = "AUSYD";
			receivingForwarder.MainAddress.Address1 = "Unit 2";
			receivingForwarder.MainAddress.Address2 = "60 What Lane";
			receivingForwarder.MainAddress.City = "Sydney";
			receivingForwarder.MainAddress.Postcode = "2023";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var unloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "FRPAR"));
			unloco.RefLocoMaps.DeleteAll();
			var mapFRSoget = unloco.RefLocoMaps.AddNew();
			mapFRSoget.RY_RN = Constants.CountryGuids.France;
			mapFRSoget.RY_SystemUsage = "PCS";
			mapFRSoget.RY_LocalPortCode = pcs;

			var unpackDepotAddress = Factory.New<OrgHeader>();
			unpackDepotAddress.OH_FullName = "I'm Receiving Stuff";
			unpackDepotAddress.MainAddress.Address1 = "Unit 400";
			unpackDepotAddress.MainAddress.Address2 = "50 What Lane";
			unpackDepotAddress.MainAddress.City = "Sydney";
			unpackDepotAddress.MainAddress.Postcode = "5023";
			unpackDepotAddress.MainAddress.OA_RN_NKCountryCode = "FR";
			unpackDepotAddress.MainAddress.OA_RL_NKRelatedPortCode = "FRPAR";

			consol.JK_OA_UnpackDepotAddress = unpackDepotAddress.MainAddress.PK;

			var packDepotAddress = Factory.New<OrgHeader>();
			packDepotAddress.OH_FullName = "I'm pack Stuff";
			packDepotAddress.OH_RL_NKClosestPort = "FRPAR";
			packDepotAddress.MainAddress.Address1 = "Unit 400";
			packDepotAddress.MainAddress.Address2 = "66 What Lane";
			packDepotAddress.MainAddress.City = "Sydney";
			packDepotAddress.MainAddress.Postcode = "1024";
			packDepotAddress.MainAddress.OA_RN_NKCountryCode = "FR";
			packDepotAddress.MainAddress.OA_RL_NKRelatedPortCode = "FRPAR";

			consol.JK_OA_PackDepotAddress = packDepotAddress.MainAddress.PK;
		}

		void CreatePackingLinesAndContainers(ForwardingConsol consol)
		{
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "AAAA0000007";
			container.JC_DeliveryMode = "CY/CY";
			container.JC_IsEmptyContainer = true;
			container.JC_ContainerCount = 1;
			container.JC_GrossWeightUQ = "KG";
			container.JC_TareWeight = 1000;
			container.JC_DunnageWeight = 1000;
			container.JC_OverhangBack = 1.1m;
			container.JC_OverhangRight = 2.1m;
			container.JC_ImportDepotCustomsReference = "ImportCusRef";

			container.JC_RC = (Factory.LoadFromNaturalKey<RefContainer>(ZArchitecture.Schema.RefContainerSchema.RC_Code, "20GP")).PK;
			var containerHandlingNote = container.Notes.AddNew();
			containerHandlingNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			containerHandlingNote.ST_NoteText = "container handling note";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "SH0001000";
			shipment.JS_HouseBill = "H0000056";
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "FRPRA";
			shipment.JS_F3_NKPackType = "PLT";
			shipment.JS_MarksAndNumbers = "Marks";
			shipment.JS_GoodsDescription = "Goods description";
			var shipmentHandlingNote = shipment.Notes.AddNew();
			shipmentHandlingNote.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			shipmentHandlingNote.ST_NoteText = "shipment handling note";
			var icvNumber = shipment.Numbers.AddNew();
			icvNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			icvNumber.CE_EntryType = FranceAdditionalReferenceNumberTypes.Codes.ImportConventional;
			icvNumber.CE_EntryNum = "ICV Ref";

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packingLine = shipment.OuterPackLines.AddNew();
			packingLine.JL_PackageCount = 2;
			packingLine.JL_F3_NKPackType = "PLT";
			packingLine.JL_ActualWeight = 200m;
			packingLine.JL_ActualWeightUQ = "KG";
			packingLine.JL_ActualVolume = 300m;
			packingLine.JL_ActualVolumeUQ = "M3";
			packingLine.JL_HarmonisedCode = "ABCDE";
			packingLine.JL_ExportRefNumber = "REF001";
			packingLine.JL_DetailedDescription = "pack1";
			packingLine.JL_ContainerPackingOrder = 1;

			packingLine.JL_RequiresTemperatureControl = true;
			packingLine.JL_RequiredTemperatureUnit = "C";
			packingLine.JL_RequiresTemperatureControl = true;
			packingLine.JL_RequiredTemperatureMinimum = 10;
			packingLine.JL_RequiredTemperatureMaximum = 30;

			var subs = UNDGSubstanceLoader.LoadSubstances(Factory, "6666", "E", "IMO").FirstOrDefault();
			if (subs == null)
			{
				subs = Factory.New<UNDGSubstance>();
				subs.DG_UNNO = "6666";
				subs.DG_Variant = "E";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			}

			container.PackLines.Add(packingLine);
		}

		#endregion
	}
}
