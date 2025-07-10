using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business.Testing
{
	sealed class PackLineTest : BaseFreightTest
	{
		public void TestTypeDecider()
		{
			AssertEquals(typeof(PackLineTypeDecider), PackLine.TypeDecider.GetType());
		}

		#region ConfirmConcurrencyErrorPreventsSaving

		public void TestConfirmConcurrencyErrorPreventsSaving()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_PackingMode = Constants.ContainerModes.Loose;
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 10;

			var date1 = new ZDateTime(2021, 1, 2);
			var date2 = new ZDateTime(2021, 1, 1);

			var confirm = shipment.DeliveryConfirms.AddNew();
			confirm.EU_PlannedPickupDeliveryTime = date1;

			Factory.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipment2 = factory2.Load<CommonShipment>(shipment.PK);

			var factory3 = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipment3 = factory3.Load<CommonShipment>(shipment.PK);

			shipment2.DocsAndCartage.Validation.ValidateJP_DeliveryCartageCompleted();
			AssertEquals(1, shipment2.DeliveryConfirms.Count);
			AssertCheckPackLineDivot(shipment2.DeliveryConfirms[0], false);

			shipment3.DeliveryConfirms.DeleteAll();
			factory3.Save();

			ErrorReporter.Clear();
			AssertNoExceptionThrown(() => { shipment2.DocsAndCartage.JP_EstimatedDelivery = date2; });
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);

			AssertExceptionThrown<ZSaveConcurrencyException>(factory2.Save);
			ErrorReporter.Clear();
		}

		void AssertCheckPackLineDivot(CommonPickupDeliveryConfirm confirm, bool expectDivotsExisted)
		{
			AssertNotNull(confirm);

			var divotsFieldInfo = confirm.GetType().GetField("divots", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			AssertNotNull(divotsFieldInfo);
			var divotsValue = divotsFieldInfo.GetValue(confirm);
			AssertEquals("Divots may not be loaded and is null although confirm exists.", expectDivotsExisted, divotsValue != null);
		}

		#endregion

		#region TestUseOutturn

		public void TestUseOutturn()
		{
			// 1. Import : Containerised (has import container) : Container is unpacked : Outturn > 0
			CommonConsol consol = GetImportConsol(typeof(CommonConsol));
			CommonShipment shipment = consol.Shipments.AddNew();
			PackLine line1 = shipment.OuterPackLines.AddNew();
			line1.JL_PackageCount = 5;
			CommonContainer container = consol.Containers.AddNew();
			Assert(!line1.UseOutturn);

			line1.JL_Outturn = 4;
			Assert(line1.UseOutturn);

			// 2. Neither Import/Export : No CommonShipment : Outturn > 0
			PackLine line2 = Factory.New<PackLine>();
			line2.JL_PackageCount = 5;
			Assert(!line2.UseOutturn);

			line2.JL_Outturn = 4;
			Assert(line2.UseOutturn);

			// 3. CommonShipment is Export : Outturn > 0
			CommonShipment shipment2 = Factory.New<CommonShipment>();
			shipment2.JS_RL_NKOrigin = HomePort;
			shipment2.JS_RL_NKDestination = OverseasPort;
			PackLine line3 = shipment2.OuterPackLines.AddNew();
			line3.JL_PackageCount = 5;
			Assert(!line3.UseOutturn);

			line3.JL_Outturn = 4;
			Assert(line3.UseOutturn);

			// 4. No Manifested Package Count, but there is outturn
			PackLine line4 = Factory.New<PackLine>();
			line4.JL_PackageCount = 0;
			line4.JL_Outturn = 4;
			Assert(line4.UseOutturn);

			// 5. Cross trade shipments (foreign -> foreign) AND Outturn > 0
			CommonShipment shipment3 = Factory.New<CommonShipment>();
			shipment3.JS_RL_NKOrigin = OverseasPort;
			shipment3.JS_RL_NKDestination = OverseasPort2;
			PackLine line5 = shipment3.OuterPackLines.AddNew();
			line5.JL_PackageCount = 5;
			Assert(!line5.UseOutturn);

			line5.JL_Outturn = 4;
			Assert(line5.UseOutturn);
		}

		#endregion

		#region TestContactsOfUNDGs_ShouldContainAllData

		public void TestContactsOfUNDGs_ShouldContainAllData()
		{
			TestCaseHelper.ClearTable(OrgContactSchema.Constants.TableName);

			var consignor = Factory.New<OrgHeader>();
			var consignorContact = consignor.Contacts.AddNew();

			var shipment = Factory.New<CommonShipment>();
			shipment.ConsignorPK = consignor.PK;

			var contactNotRelatedShipment = Factory.New<OrgContact>();

			var packline = shipment.OuterPackLines.AddNew();

			var contacts = packline.UNDGs.Contacts;
			contacts.Load();

			AssertCollectionContains(consignorContact, contacts);
			AssertCollectionContains(contactNotRelatedShipment, contacts);
		}

		#endregion

		#region Clone

		public void TestCloneIncludesUNDGs()
		{
			PackLine line = Factory.New<PackLine>();
			line.JL_FreightMode = FreightConstants.OuterPackType;
			line.UNDGs.AddNew();
			line.UNDGs.AddNew();

			PackLine clonedLine = (PackLine)line.Clone();
			AssertEquals(2, clonedLine.UNDGs.Count);
		}

		public void TestCloneCanExcludeUNDGs()
		{
			PackLine line = Factory.New<PackLine>();
			line.UNDGs.AddNew();
			line.UNDGs.AddNew();

			BusinessObjectCloneArgs cloneArgs = new BusinessObjectCloneArgs(new[] { PackLine.Schema.DangerousGoodsCollection });

			PackLine clonedLine = (PackLine)line.Clone(cloneArgs);
			AssertEquals(0, clonedLine.UNDGs.Count);
		}

		public void TestCloneCopiesHarmonisedCodes()
		{
			var line = Factory.New<PackLine>();
			line.JL_FreightMode = FreightConstants.OuterPackType;
			line.HarmonisedCodes.AddNew().JLH_RN_NKCountry = "BE";
			line.HarmonisedCodes.AddNew().JLH_RN_NKCountry = "ZA";

			var clonedLine = (PackLine)line.Clone();
			AssertEquals(2, clonedLine.HarmonisedCodes.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "BE", "ZA" }, clonedLine.HarmonisedCodes.Select(x => x.JLH_RN_NKCountry));
		}

		public void TestHarmonisedCode_HSCountryANDHSCode_ShouldNotBeSetedToNewValue_WhenNewValueIsMany()
		{
			PackLine source = Factory.New<PackLine>();
			var hsCodeSource = source.HarmonisedCodes.AddNew();
			hsCodeSource.JLH_RN_NKCountry = "US";
			hsCodeSource.JLH_Code = "2234567";
			var secondHsCodeSource = source.HarmonisedCodes.AddNew();
			secondHsCodeSource.JLH_RN_NKCountry = "AU";
			secondHsCodeSource.JLH_Code = "123456";

			PackLine dest = Factory.New<PackLine>();
			var hsCodeDest = dest.HarmonisedCodes.AddNew();
			var destHSCountryInitialValue = "CA";
			var destHSCodeInitialValue = "999999";
			hsCodeDest.JLH_RN_NKCountry = destHSCountryInitialValue;
			hsCodeDest.JLH_Code = destHSCodeInitialValue;

			AssertNoExceptionThrown(() =>
			{
				dest.HarmonisedCodes.HSCountryManager.Value = source.HarmonisedCodes.HSCountryManager.Value;
			});
			AssertEquals("Multi item manager should not set value to Many for HS Country", destHSCountryInitialValue, dest.HarmonisedCodes.HSCountryManager.Value);
			dest.HarmonisedCodes.HSCodeManager.Value = source.HarmonisedCodes.HSCodeManager.Value;
			AssertEquals("Multi item manager should not set value to Many for HS Code", destHSCodeInitialValue, dest.HarmonisedCodes.HSCodeManager.Value);
		}

		public void TestClone_BusinessObjectCloneArgsShouldNotBeModified()
		{
			var line1 = Factory.New<PackLine>();
			line1.JL_ActualWeight = 300m;
			line1.JL_ActualVolume = 3m;

			var line2 = Factory.New<PackLine>();
			line2.JL_ActualWeight = 500m;
			line2.JL_ActualVolume = 5m;

			var args = new BusinessObjectCloneArgs();

			var clonedLine1 = (PackLine)line1.Clone(args);
			AssertEquals(300m, clonedLine1.JL_ActualWeight);
			AssertEquals(3m, clonedLine1.JL_ActualVolume);

			var clonedLine2 = (PackLine)line2.Clone(args);
			AssertEquals(500m, clonedLine2.JL_ActualWeight);
			AssertEquals(5m, clonedLine2.JL_ActualVolume);

			AssertEquals(false, args.IsExcludedFromCloning(PackLine.Schema.JL_ActualWeight));
			AssertEquals(false, args.IsExcludedFromCloning(PackLine.Schema.JL_ActualVolume));
			AssertEquals(false, args.IsExcludedFromCloning(PackLine.Schema.JL_JS));
		}

		public void TestCloneBusinessObjectCloneArgs()
		{
			var args = new BusinessObjectCloneArgs(Factory, new[] { DummyBizoSchema.Z0_Number.Name }, typeof(DummyBusinessObject), true, (bizo1, bizo2, column) => column.ColumnName != DummyBizoSchema.Z0_VarBinaryMax.Name);
			var clonedArgs = new BusinessObjectCloneArgs(args.AlternativeFactoryToInstantiateCloneIn, args.GetExcludedColumns(), args.TypeToCloneAs, args.PerformRowCopyWithoutTriggeringValidationAndSetter, args.CopyDecider);

			AssertEquals(args.AlternativeFactoryToInstantiateCloneIn, clonedArgs.AlternativeFactoryToInstantiateCloneIn);
			AssertContainsExactElementsInAnyOrder(args.GetExcludedColumns(), clonedArgs.GetExcludedColumns());
			AssertEquals(args.TypeToCloneAs, clonedArgs.TypeToCloneAs);
			AssertEquals(args.PerformRowCopyWithoutTriggeringValidationAndSetter, clonedArgs.PerformRowCopyWithoutTriggeringValidationAndSetter);
			AssertEquals(args.CopyDecider, clonedArgs.CopyDecider);
		}

		public void TestCloneExcludesProperties()
		{
			var shipment = Factory.New<CommonShipment>();
			var innerpackline = Factory.New<PackLine>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();

			innerpackline.JL_JS = shipment.PK;
			innerpackline.JL_OriginTransitWarehouseStatus = "DIS";
			innerpackline.JL_LastKnownTransitWarehouseStatus = "RCV";
			innerpackline.JL_LastKnownTransitWarehouseStatusDateTime = ZDateTime.Now.AddDays(-2);
			innerpackline.JL_OA_LastKnownTransitWarehouseAddress = orgAddress.PK;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_OriginTransitWarehouseStatus = "DIS";
			packLine.JL_JL_OuterPackLine = innerpackline.PK;
			packLine.JL_LastKnownTransitWarehouseStatus = "RCV";
			packLine.JL_LastKnownTransitWarehouseStatusDateTime = ZDateTime.Now;
			packLine.JL_OA_LastKnownTransitWarehouseAddress = orgAddress.PK;

			Factory.Save();

			var clonedLine = packLine.Clone() as PackLine;
			CombineAssertions("lines were excluded", () =>
			{
				AssertEquals("JL_OriginTransitWarehouseStatus", "UNK", clonedLine.JL_OriginTransitWarehouseStatus);
				AssertEquals("JL_LastKnownTransitWarehouseStatus", string.Empty, clonedLine.JL_LastKnownTransitWarehouseStatus);
				AssertEquals("JL_LastKnownTransitWarehouseStatusDateTime", ZDateTime.Empty, clonedLine.JL_LastKnownTransitWarehouseStatusDateTime);
				AssertEquals("JL_OA_LastKnownTransitWarehouseAddress", ZGuid.Empty, clonedLine.JL_OA_LastKnownTransitWarehouseAddress);
				AssertEquals("JL_JL_OuterPackLine", ZGuid.Empty, clonedLine.JL_JL_OuterPackLine);
			});
		}

		public void TestDefaultLastKnownTransitWarehouseAddress()
		{
			using (ZArchitecture.Environment.Globals.SetIsUserInteractiveForTest(true))
			{
				var shipment = Factory.New<CommonShipment>();
				var consol = shipment.Consols.AddNew();
				var packLine = shipment.OuterPackLines.AddNew();
				var orgHeader = Factory.New<OrgHeader>();
				packLine.JL_OriginTransitWarehouseStatus = FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched;
				packLine.JL_LastKnownTransitWarehouseStatusDateTime = ZDateTime.Now;
				packLine.JL_OA_LastKnownTransitWarehouseAddress_ZAddress.OrgPK = orgHeader.PK;

				consol.JK_OA_PackDepotAddress = Factory.New<OrgHeader>().MainAddress.PK;
				consol.JK_OA_UnpackDepotAddress = Factory.New<OrgHeader>().MainAddress.PK;
				shipment.JS_OA_ExportReceivingDepot = Factory.New<OrgHeader>().MainAddress.PK;
				shipment.JS_OA_ImportReleaseDepot = Factory.New<OrgHeader>().MainAddress.PK;

				foreach (var status in new ZString[] { ZString.Empty, FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received, FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched })
				{
					packLine.JL_LastKnownTransitWarehouseStatus = status;

					foreach (var addressType in OrgAddressType.AddressTypeList)
					{
						packLine.JL_OA_LastKnownTransitWarehouseAddress_ZAddress.OrgPK = ZGuid.Empty;
						orgHeader.MainAddress.CapabilitiesCollection[0].PZ_AddressType = addressType.Code;
						packLine.JL_OA_LastKnownTransitWarehouseAddress_ZAddress.OrgPK = orgHeader.PK;

						AssertEquals(orgHeader.MainAddress.PK, packLine.JL_OA_LastKnownTransitWarehouseAddress);
					}
				}

				orgHeader.Addresses.AddNew();
				foreach (var status in new ZString[] { ZString.Empty, FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received, FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched })
				{
					packLine.JL_LastKnownTransitWarehouseStatus = status;
					foreach (var addressType in OrgAddressType.AddressTypeList)
					{
						packLine.JL_OA_LastKnownTransitWarehouseAddress_ZAddress.OrgPK = ZGuid.Empty;
						orgHeader.MainAddress.CapabilitiesCollection[0].PZ_AddressType = addressType.Code;
						packLine.JL_OA_LastKnownTransitWarehouseAddress_ZAddress.OrgPK = orgHeader.PK;

						AssertEquals(ZGuid.Empty, packLine.JL_OA_LastKnownTransitWarehouseAddress);
					}
				}

				packLine.JL_OA_LastKnownTransitWarehouseAddress_ZAddress.OrgPK = ZGuid.Empty;
				foreach (var status in new ZString[] { ZString.Empty, FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received, FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched })
				{
					packLine.JL_LastKnownTransitWarehouseStatus = status;
					foreach (var tempOrg in new OrgHeader[] { orgHeader, consol.PackDepotAddress.Header, consol.UnpackDepotAddress.Header, shipment.ImportReleaseDepot.Header, shipment.ExportReceivingDepot.Header })
					{
						packLine.JL_OA_LastKnownTransitWarehouseAddress_ZAddress.OrgPK = tempOrg.PK;

						if (tempOrg != orgHeader)
						{
							AssertEquals(tempOrg.MainAddress.PK, packLine.JL_OA_LastKnownTransitWarehouseAddress);
						}
						else
						{
							AssertEquals(ZGuid.Empty, packLine.JL_OA_LastKnownTransitWarehouseAddress);
						}
					}
				}

				consol.PackDepotAddress.Header.Addresses.AddNew();
				consol.UnpackDepotAddress.Header.Addresses.AddNew();
				shipment.ExportReceivingDepot.Header.Addresses.AddNew();
				shipment.ImportReleaseDepot.Header.Addresses.AddNew();
				packLine.JL_OA_LastKnownTransitWarehouseAddress_ZAddress.OrgPK = ZGuid.Empty;
				foreach (var status in new ZString[] { ZString.Empty, FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received, FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched })
				{
					packLine.JL_LastKnownTransitWarehouseStatus = status;
					foreach (var tempOrg in new OrgHeader[] { orgHeader, consol.PackDepotAddress.Header, consol.UnpackDepotAddress.Header, shipment.ImportReleaseDepot.Header, shipment.ExportReceivingDepot.Header })
					{
						packLine.JL_OA_LastKnownTransitWarehouseAddress_ZAddress.OrgPK = tempOrg.PK;

						if ((status.IsEmpty && tempOrg != orgHeader) ||
							(status == FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received && (tempOrg == shipment.ExportReceivingDepot.Header || tempOrg == consol.PackDepotAddress.Header)) ||
							(status == FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched && (tempOrg == shipment.ImportReleaseDepot.Header || tempOrg == consol.UnpackDepotAddress.Header)))
						{
							AssertEquals(tempOrg.MainAddress.PK, packLine.JL_OA_LastKnownTransitWarehouseAddress);
						}
						else
						{
							AssertEquals(ZGuid.Empty, packLine.JL_OA_LastKnownTransitWarehouseAddress);
						}
					}
				}

				packLine.JL_LastKnownTransitWarehouseStatus = ZString.Empty;
				var newAddress = shipment.ExportReceivingDepot.Header.Addresses.AddNew();
				consol.JK_OA_PackDepotAddress = newAddress.PK;
				packLine.JL_OA_LastKnownTransitWarehouseAddress_ZAddress.OrgPK = newAddress.OA_OH;
				AssertEquals(ZGuid.Empty, packLine.JL_OA_LastKnownTransitWarehouseAddress);
			}

			using (ZArchitecture.Environment.Globals.SetIsUserInteractiveForTest(false))
			{
				var shipment = Factory.New<CommonShipment>();
				var consol = shipment.Consols.AddNew();
				var packLine = shipment.OuterPackLines.AddNew();
				var orgHeader = Factory.New<OrgHeader>();
				packLine.JL_OriginTransitWarehouseStatus = FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched;
				packLine.JL_LastKnownTransitWarehouseStatusDateTime = ZDateTime.Now;
				packLine.JL_OA_LastKnownTransitWarehouseAddress_ZAddress.OrgPK = orgHeader.PK;

				consol.JK_OA_PackDepotAddress = Factory.New<OrgHeader>().MainAddress.PK;
				consol.JK_OA_UnpackDepotAddress = Factory.New<OrgHeader>().MainAddress.PK;
				shipment.JS_OA_ExportReceivingDepot = Factory.New<OrgHeader>().MainAddress.PK;
				shipment.JS_OA_ImportReleaseDepot = Factory.New<OrgHeader>().MainAddress.PK;

				foreach (var status in new ZString[] { ZString.Empty, FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received, FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched })
				{
					packLine.JL_LastKnownTransitWarehouseStatus = status;

					foreach (var addressType in OrgAddressType.AddressTypeList)
					{
						packLine.JL_OA_LastKnownTransitWarehouseAddress_ZAddress.OrgPK = ZGuid.Empty;
						orgHeader.MainAddress.CapabilitiesCollection[0].PZ_AddressType = addressType.Code;
						packLine.JL_OA_LastKnownTransitWarehouseAddress_ZAddress.OrgPK = orgHeader.PK;

						AssertEquals(ZGuid.Empty, packLine.JL_OA_LastKnownTransitWarehouseAddress);
					}
				}

				orgHeader.Addresses.AddNew();
				foreach (var status in new ZString[] { ZString.Empty, FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received, FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched })
				{
					packLine.JL_LastKnownTransitWarehouseStatus = status;
					foreach (var addressType in OrgAddressType.AddressTypeList)
					{
						packLine.JL_OA_LastKnownTransitWarehouseAddress_ZAddress.OrgPK = ZGuid.Empty;
						orgHeader.MainAddress.CapabilitiesCollection[0].PZ_AddressType = addressType.Code;
						packLine.JL_OA_LastKnownTransitWarehouseAddress_ZAddress.OrgPK = orgHeader.PK;

						AssertEquals(ZGuid.Empty, packLine.JL_OA_LastKnownTransitWarehouseAddress);
					}
				}

				packLine.JL_OA_LastKnownTransitWarehouseAddress_ZAddress.OrgPK = ZGuid.Empty;
				foreach (var status in new ZString[] { ZString.Empty, FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received, FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched })
				{
					packLine.JL_LastKnownTransitWarehouseStatus = status;
					foreach (var tempOrg in new OrgHeader[] { orgHeader, consol.PackDepotAddress.Header, consol.UnpackDepotAddress.Header, shipment.ImportReleaseDepot.Header, shipment.ExportReceivingDepot.Header })
					{
						packLine.JL_OA_LastKnownTransitWarehouseAddress_ZAddress.OrgPK = tempOrg.PK;

						AssertEquals(ZGuid.Empty, packLine.JL_OA_LastKnownTransitWarehouseAddress);
					}
				}

				consol.PackDepotAddress.Header.Addresses.AddNew();
				consol.UnpackDepotAddress.Header.Addresses.AddNew();
				shipment.ExportReceivingDepot.Header.Addresses.AddNew();
				shipment.ImportReleaseDepot.Header.Addresses.AddNew();
				packLine.JL_OA_LastKnownTransitWarehouseAddress_ZAddress.OrgPK = ZGuid.Empty;
				foreach (var status in new ZString[] { ZString.Empty, FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Received, FreightConstants.PacklineLastKnownTransitWarehouseStatus.Codes.Dispatched })
				{
					packLine.JL_LastKnownTransitWarehouseStatus = status;
					foreach (var tempOrg in new OrgHeader[] { orgHeader, consol.PackDepotAddress.Header, consol.UnpackDepotAddress.Header, shipment.ImportReleaseDepot.Header, shipment.ExportReceivingDepot.Header })
					{
						packLine.JL_OA_LastKnownTransitWarehouseAddress_ZAddress.OrgPK = tempOrg.PK;

						AssertEquals(ZGuid.Empty, packLine.JL_OA_LastKnownTransitWarehouseAddress);
					}
				}

				packLine.JL_LastKnownTransitWarehouseStatus = ZString.Empty;
				var newAddress = shipment.ExportReceivingDepot.Header.Addresses.AddNew();
				consol.JK_OA_PackDepotAddress = newAddress.PK;
				packLine.JL_OA_LastKnownTransitWarehouseAddress_ZAddress.OrgPK = newAddress.OA_OH;
				AssertEquals(ZGuid.Empty, packLine.JL_OA_LastKnownTransitWarehouseAddress);
			}
		}

		#endregion

		#region TestHasHazardous

		public void TestHasHazardous()
		{
			PackLine.JL_RH_NKCommodityCode = "GEN";
			Assert(!PackLine.HasHazardous);

			PackLine.JL_RH_NKCommodityCode = "HAZD";
			Assert(PackLine.HasHazardous);

			PackLine.JL_RH_NKCommodityCode = "";
			PackLine.UNDGs.AddNew();
			Assert(PackLine.HasHazardous);
		}

		#endregion

		#region TestPacklineFieldsShouldBePrefixedWithPackLineType

		public void TestPacklineTypeInfoShouldBePrefixedWithPackLineType()
		{
			PackLine packLine = Factory.New<PackLine>();

			packLine.JL_FreightMode = FreightConstants.InnerPackType;
			AssertInfoPrefix("Inner packline", "Inner Package: ", packLine.JL_F3_NKPackTypeInfo);

			packLine.JL_FreightMode = FreightConstants.OuterPackType;
			AssertInfoPrefix("Outer packline", "Outer Package: ", packLine.JL_F3_NKPackTypeInfo);

			packLine.JL_FreightMode = FreightConstants.DeliveryPackType;
			AssertInfoPrefix("Delivery packline", "Delivery Package: ", packLine.JL_F3_NKPackTypeInfo);
		}

		void AssertInfoPrefix(string message, string prefix, ZPropertyInfo info)
		{
			System.Text.StringBuilder builder = new System.Text.StringBuilder();

			if (!info.HumanReadableName.StartsWith(prefix))
			{
				AssertEquals(message, prefix + info.Description, info.HumanReadableName);
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region TestIsReceivedShouldNotThrowException

		public void TestIsReceivedShouldNotThrowException()
		{
			PackLine packline = Factory.New<CommonShipment>().OuterPackLines.AddNew();
			AssertEquals(false, packline.JL_Calc_IsReceived);
		}

		#endregion

		#region TestIPackLineInfo properties

		public void TestContainerNumber()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "AAAA1111111";
			Shipment.Consols.Add(consol);

			PackLine.SetContainer(consol, container1);
			JobContainerPackPivot pivot1 = (JobContainerPackPivot)PackLine.Containers.GetRelationshipBusinessObject(container1);

			Factory.Save();

			PackLine.CurrentConsol = consol;
			AssertEquals("Container Number:", "AAAA1111111", ((IPackLineInfo)PackLine).ContainerNumber);
		}

		public void TestGoodsDescription()
		{
			PackLine.JL_Description = "books";
			AssertEquals("GoodsDescription:", "books", ((IPackLineInfo)PackLine).GoodsDescription);
		}

		public void TestDimensions()
		{
			PackLine.JL_Height = 10.2m;
			PackLine.JL_Length = 1.1m;
			PackLine.JL_Width = 5.3m;
			PackLine.JL_UnitOfDimension = "KG";
			AssertEquals("Height:", 10.2m, ((IPackLineInfo)PackLine).Height);
			AssertEquals("Length:", 1.1m, ((IPackLineInfo)PackLine).Length);
			AssertEquals("Width:", 5.3m, ((IPackLineInfo)PackLine).Width);
			AssertEquals("UnitOfDimension", "KG", ((IPackLineInfo)PackLine).UnitOfDimension);
		}

		public void TestMarksAndNumbers()
		{
			PackLine.JL_MarksAndNumbers = "Line Marks";
			Shipment.JS_MarksAndNumbers = "Shipment Marks";
			AssertEquals("Marks and Numbers", "Line Marks", ((IPackLineInfo)PackLine).MarksAndNumbers);
			PackLine.JL_MarksAndNumbers = ZString.Empty;
			AssertEquals("Marks and Numbers from shipment", "Shipment Marks", ((IPackLineInfo)PackLine).MarksAndNumbers);
		}

		public void TestNumberOfPackages()
		{
			PackLine.JL_PackageCount = 10;
			AssertEquals("Number of Packages", 10, ((IPackLineInfo)PackLine).NumberOfPackages);
		}

		public void TestPackType()
		{
			PackLine.JL_F3_NKPackType = Constants.PkgUnit.Bag;
			AssertEquals("Pack Type", Constants.PkgUnit.Bag, ((IPackLineInfo)PackLine).PackType);
		}

		public void TestWeight()
		{
			PackLine.JL_ActualWeight = 5.0m;
			PackLine.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			AssertEquals("Weight", 5.0m, ((IPackLineInfo)PackLine).Weight.Amount);
			AssertEquals("Weight UQ", Constants.Weight.Kilograms, ((IPackLineInfo)PackLine).Weight.Unit);
		}

		public void TestVolume()
		{
			PackLine.JL_ActualVolume = 0.55m;
			PackLine.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
			AssertEquals("Volume", 0.55m, ((IPackLineInfo)PackLine).Volume.Amount);
			AssertEquals("Volume UQ", Constants.Volume.CubicMetres, ((IPackLineInfo)PackLine).Volume.Unit);
		}

		public void TestParentCollection()
		{
			var masterShipment = CommonShipment.New(Factory);
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			Shipment.JS_JS_ColoadMasterShipment = masterShipment.PK;
			AssertNotNull("Master Shipment packline collection contains sub-shipment's packline.", masterShipment.OuterPackLines.FindByPK(PackLine.PK));
			AssertEquals("ParentCollection is sub-shipment packline collection.", Shipment.PK, PackLine.ParentCollection.Master.PK);
		}

		public void TestCurrentConsolIsNullWhenConsolIsAttachedToMasterShipmentInDifferentFactory()
		{
			var masterShipment = CommonShipment.New(Factory);
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			masterShipment.JS_IsForwardRegistered = false;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var subShipment = CommonShipment.New(newFactory);
			subShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			subShipment.JS_ActualWeight = 5m;
			subShipment.JS_ActualVolume = 5m;
			subShipment.JS_OuterPacks = 5;
			subShipment.JS_JS_ColoadMasterShipment = masterShipment.PK;

			var consol = masterShipment.Consols.AddNew();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CONT1";
			Factory.Save();

			AssertEquals(1, subShipment.OuterPackLines.Count);
			var masterShipmentInNewFac = subShipment.CoLoadMasterShipment;
			AssertEquals("Precondition: Refresh Bus added new consol to master shipment in new Factory", 1, masterShipmentInNewFac.Consols.Count);
			var consolInNewFac = masterShipmentInNewFac.Consols[0];
			AssertNull("Precondition: Sub-shipment is not attached to the consol.", consolInNewFac.Shipments.FindByPK(subShipment.PK));
			var packLine = subShipment.OuterPackLines[0];
			AssertNull("PackLine.CurrentConsol should be null.", packLine.CurrentConsol);
		}

		#endregion

		#region TestAddNewNotSupported

		[ExpectException(typeof(NotSupportedException))]
		public void TestAddNewNotSupported()
		{
			Factory.New<CommonContainer>().PackLines.AddNew();
		}

		#endregion

		#region TestSetJL_OutturnedVolume

		public void TestSetJL_OutturnedVolume()
		{
			PackLine line = Factory.New<PackLine>();
			line.JL_UnitOfDimension = Constants.Length.Metres;
			AssertEquals("Outturned volume only calculates if Length, width and height and Outturn are non zero", 0m, line.JL_OutturnedVolume);

			line.JL_Outturn = 2;
			AssertEquals("Outturned volume only calculates if Length, width and height and Outturn are non zero", 0m, line.JL_OutturnedVolume);

			line.JL_OutturnedHeight = 1.5m;
			AssertEquals("Outturned volume only calculates if Length, width and height and Outturn are non zero", 0m, line.JL_OutturnedVolume);

			line.JL_OutturnedWidth = 3m;
			AssertEquals("Outturned volume only calculates if Length, width and height and Outturn are non zero", 0m, line.JL_OutturnedVolume);

			line.JL_OutturnedLength = 2.33m;
			AssertEquals("Outturned volume only calculates if Length, width and height and Outturn are non zero", 20.97m, line.JL_OutturnedVolume);

			line.JL_Outturn = 4;
			AssertEquals("Outturn Volume recalcs on JL_Outturn change.", 41.94m, line.JL_OutturnedVolume);

			line.JL_OutturnedHeight = 20m;
			AssertEquals("Outturn Volume recalcs on JL_Outturn change.", 559.2m, line.JL_OutturnedVolume);

			line.JL_OutturnedLength = 30m;
			AssertEquals("Outturn Volume recalcs on JL_Outturn change.", 7200m, line.JL_OutturnedVolume);

			line.JL_OutturnedWidth = 30m;
			AssertEquals("Outturn Volume recalcs on JL_Outturn change.", 72000m, line.JL_OutturnedVolume);
		}

		#endregion

		#region TestDefaultOutturnFromManifested

		public void TestDefaultOutturnFromManifested()
		{
			PackLine line = Factory.New<PackLine>();
			line.JL_UnitOfDimension = Constants.Length.Metres;
			line.JL_PackageCount = 3;
			line.JL_Height = 3m;
			line.JL_Length = 1.5m;
			line.JL_Width = 2.43m;
			line.JL_ActualWeight = 35;
			line.JL_Outturn = 0;
			AssertEquals("Outturned values only default if outtrun is greater than zero", 0m, line.JL_OutturnedHeight);
			AssertEquals("Outturned values only default if outtrun is greater than zero", 0m, line.JL_OutturnedWidth);
			AssertEquals("Outturned values only default if outtrun is greater than zero", 0m, line.JL_OutturnedLength);
			AssertEquals("Outturned values only default if outtrun is greater than zero", 0m, line.JL_OutturnedWeight);
			AssertEquals("Outturned values only default if outtrun is greater than zero", 0m, line.JL_OutturnedVolume);

			line.JL_OutturnedLength = 1m;
			line.JL_OutturnedWidth = 2m;
			line.JL_OutturnedHeight = 3m;
			line.JL_OutturnedWeight = 4;
			line.JL_Outturn = 5;

			AssertEquals("Outturned Length only defaults if it is zero", 1m, line.JL_OutturnedLength);
			AssertEquals("Outturned Width only defaults if it is zero", 2m, line.JL_OutturnedWidth);
			AssertEquals("Outturned Height only defaults if it is zero", 3m, line.JL_OutturnedHeight);
			AssertEquals("Outturned Weight only defaults if it is zero", 4m, line.JL_OutturnedWeight);
			AssertEquals("Outturned Volume Calculates as it should", 30m, line.JL_OutturnedVolume);

			line.JL_Outturn = 0;
			AssertEquals("Outturned Height defaults to zero if outtrun is zero", 0m, line.JL_OutturnedHeight);
			AssertEquals("Outturned Length defaults to zero if outtrun is zero", 0m, line.JL_OutturnedLength);
			AssertEquals("Outturned width defaults to zero if outtrun is zero", 0m, line.JL_OutturnedWidth);
			AssertEquals("Outturned weight defaults to zero if outtrun is zero", 0m, line.JL_OutturnedWeight);
			AssertEquals("Outturned Volume defaults to zero if outtrun is zero", 0m, line.JL_OutturnedVolume);

			line.JL_Outturn = 2;
			AssertEquals("Outturned Height defaults to manifest if outtrun is greater than zero", 3m, line.JL_OutturnedHeight);
			AssertEquals("Outturned Length defaults to manifest if outtrun is greater than zero", 1.5m, line.JL_OutturnedLength);
			AssertEquals("Outturned Width defaults to manifest if outtrun is greater than zero", 2.43m, line.JL_OutturnedWidth);
			AssertEquals("Outturned Weight defaults to manifest if outtrun is greater than zero", 35m, line.JL_OutturnedWeight);
			AssertEquals("Outturned Volume Calculates as per CalculatedOutturnedVolutme", 21.87m, line.JL_OutturnedVolume);

			line.JL_Outturn = 0;
			line.JL_Length = 0m;
			line.JL_Width = 0m;
			line.JL_Height = 0m;
			line.JL_ActualVolume = 35;
			line.JL_Outturn = 2;
			AssertEquals("Outturned Volume doesn't calculate but defaults to ActualVolume", line.JL_ActualVolume, line.JL_OutturnedVolume);
		}

		#endregion

		#region TestContainerWeightUpdatedWhenPacklineWeightChanges

		public void TestContainerWeightUpdatedWhenPacklineWeightChanges()
		{
			CommonContainer container1 = Factory.New<CommonContainer>();

			PackLine line1 = Factory.New<CommonShipment>().OuterPackLines.AddNew();
			line1.JL_ActualWeight = 1000m;
			container1.PackLines.Add(line1);
			AssertEquals("Container 1: Weight added before: Weight should be 1000", 1000m, container1.JC_GrossWeight);

			PackLine line2 = Factory.New<CommonShipment>().OuterPackLines.AddNew();
			container1.PackLines.Add(line2);
			line2.JL_ActualWeight = 600m;
			AssertEquals("Container 1: Weight added after: Weight should be 1600", 1600m, container1.JC_GrossWeight);

			line2.JL_ActualWeight = 0.6;
			AssertEquals("Container 1: Weight changed: Weight should be 1000.6", 1000.6m, container1.JC_GrossWeight);

			line2.JL_ActualWeightUQ = Constants.Weight.Tonnes;
			AssertEquals("Container 1: Weight unit changed: Weight should be 1600", 1600m, container1.JC_GrossWeight);
		}

		#endregion

		#region JL_LoadingMeters

		public void TestJL_LoadingMetersReadOnly()
		{
			FreightDataRegistry.Instance.EnableRoadLoadingMeters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Road;
			AssertEquals("Precondition", true, shipment.IsRoadLoadingMetersEnabled);

			PackLine line = Factory.New<PackLine>();
			AssertEquals("Readonly when no shipment", true, line.JL_LoadingMetersInfo.ReadOnly);

			line = shipment.OuterPackLines.AddNew();
			AssertEquals("Writeable when loading meters enabled on shipment", false, line.JL_LoadingMetersInfo.ReadOnly);

			shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals("Precondition", false, shipment.IsRoadLoadingMetersEnabled);
			AssertEquals("Readonly when loading meters disabled on shipment", true, line.JL_LoadingMetersInfo.ReadOnly);
		}

		#endregion

		#region TestFirstImportContainer

		public void TestFirstImportContainer()
		{
			AssertEquals("initially nothing there", null, PackLine.JL_Calc_FirstImportContainer);
			CommonContainer import = PackLine.Containers.AddNew();
			import.JC_JX = CreateNewSailing(true).PK;
			CommonContainer export = PackLine.Containers.AddNew();
			export.JC_JX = CreateNewSailing(false).PK;
			AssertEquals("found the correct container", import, PackLine.JL_Calc_FirstImportContainer);
		}

		#endregion

		#region TestFirstExportContainer

		public void TestFirstExportContainer()
		{
			AssertEquals("initially nothing there", null, PackLine.JL_Calc_FirstExportContainer);
			CommonContainer import = PackLine.Containers.AddNew();
			import.JC_JX = CreateNewSailing(true).PK;
			CommonContainer export = PackLine.Containers.AddNew();
			export.JC_JX = CreateNewSailing(false).PK;
			AssertEquals("found the correct container", export, PackLine.JL_Calc_FirstExportContainer);
		}

		#endregion

		#region Test Calculated Properties

		public void TestJL_Calc_ShortLanded()
		{
			var shipment = Factory.New<CommonShipment>();
			var line = shipment.OuterPackLines.AddNew();
			line.JL_PackageCount = 50;
			line.JL_Outturn = 20;
			AssertEquals("ShortLanded should be 30", 30, line.JL_Calc_Shortlanded);

			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			AssertEquals("ShortLanded should be 0 because CommonShipment is a coload master", 0, line.JL_Calc_Shortlanded);
			Assert(line.IsDeleted);
		}

		public void TestJL_Calc_LargestDimension()
		{
			var shipment = Factory.New<CommonShipment>();
			var line = shipment.OuterPackLines.AddNew();
			line.JL_Width = 50;
			line.JL_Length = 20;
			line.JL_Height = 60;
			AssertEquals(60m, line.JL_Calc_LargestDimension);
		}

		public void TestJL_Calc_Girth()
		{
			var shipment = Factory.New<CommonShipment>();
			var line = shipment.OuterPackLines.AddNew();
			line.JL_Width = 115;
			line.JL_Length = 77;
			line.JL_Height = 66;
			AssertEquals(286m, line.JL_Calc_Girth);
		}

		#endregion

		#region TestJL_Calc_JV_Vessel

		public void TestJL_Calc_JV_Vessel()
		{
			SailingsForTestClasses sailingsHelper = new SailingsForTestClasses(Factory);
			CommonShipment shipment = Factory.New<CommonShipment>();
			JobSailing sailing = sailingsHelper.SydLaxSailing;
			shipment.JS_JX = sailing.PK;

			PackLine line = shipment.OuterPackLines.AddNew();

			AssertEquals("", sailing.JX_JV_NKVessel, line.JL_Calc_JV_Vessel);

			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_Name = "DAVID";
			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_Name = "BOB";

			sailing.Voyage.JV_RV_NKVessel = vessel1.RV_FK;
			CommonContainer container = sailing.Containers.AddNew();
			line.SetContainer(container.PK);
			AssertEquals("", "DAVID", line.JL_Calc_JV_Vessel);

			sailing.Voyage.JV_RV_NKVessel = vessel2.RV_FK;
			CommonConsol consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport = consol.Transports[0];
			transport.JW_JX = sailing.PK;

			CommonContainer container2 = consol.Containers.AddNew();
			line.SetContainer(consol, container2);
			AssertEquals("", "BOB", line.JL_Calc_JV_Vessel);
		}

		#endregion

		#region TestJL_Calc_JV_VoyageNo

		public void TestJL_Calc_JV_VoyageNo()
		{
			SailingsForTestClasses sailingsHelper = new SailingsForTestClasses(Factory);
			CommonShipment shipment = Factory.New<CommonShipment>();
			JobSailing sailing = sailingsHelper.SydLaxSailing;
			shipment.JS_JX = sailing.PK;

			PackLine line = shipment.OuterPackLines.AddNew();

			AssertEquals("", sailing.JX_JV_VoyageFlight, line.JL_Calc_JV_VoyageNo);

			sailing.Voyage.JV_VoyageFlight = "DAVID";
			CommonContainer container = sailing.Containers.AddNew();
			line.SetContainer(container.PK);
			AssertEquals("", "DAVID", line.JL_Calc_JV_VoyageNo);

			sailing.Voyage.JV_VoyageFlight = "BOB";
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport = consol.Transports[0];
			transport.JW_JX = sailing.PK;

			CommonContainer container2 = consol.Containers.AddNew();
			line.SetContainer(consol, container2);
			AssertEquals("", "BOB", line.JL_Calc_JV_VoyageNo);
		}

		#endregion

		#region TestPackLineVolume

		public void TestPackLineVolume()
		{
			PackLine.JL_UnitOfDimension = Constants.Length.Centimetres;
			PackLine.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;

			PackLine.JL_ActualVolume = 1.5m;
			PackLine.JL_PackageCount = 0;
			PackLine.JL_Length = 50m;
			PackLine.JL_Width = 50m;
			PackLine.JL_Height = 50m;
			AssertEquals("Only calculate Volume if Lgth, Wdth, Hgt, Pkgs all > 0.", 1.5m, PackLine.JL_ActualVolume);

			PackLine.JL_PackageCount = 10;
			AssertEquals("Calculate Volume from Lgth, Wdth, Hgt, Pkgs.", 1.25m, PackLine.JL_ActualVolume);

			PackLine.JL_Length = 0.5m;
			PackLine.JL_Width = 0.5m;
			PackLine.JL_Height = 0.5m;
			PackLine.JL_UnitOfDimension = Constants.Length.Metres;
			AssertEquals("Dimension unit changed to CM.", 1.25m, PackLine.JL_ActualVolume);

			PackLine.JL_ActualVolumeUQ = Constants.Volume.CubicFeet;
			AssertEquals("Volume unit changed to CF.", 44.143m, ZArchitecture.Core.Utilities.Round(PackLine.JL_ActualVolume, 3));
		}

		#endregion

		#region TestCalcContainerNumber

		public void TestCalcContainerNumber()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "AAAA1111111";
			CommonContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "BBBB2222222";
			Shipment.Consols.Add(consol);

			PackLine.SetContainer(consol, container1);
			JobContainerPackPivot pivot1 = (JobContainerPackPivot)PackLine.Containers.GetRelationshipBusinessObject(container1);

			Factory.Save();

			PackLine.CurrentConsol = consol;
			AssertEquals("Get Calc_ContainerNumber.", "AAAA1111111", PackLine.JL_Calc_ContainerNumber);

			PackLine.JL_JC = container2.PK;
			AssertEquals("Get Calc_ContainerNumber.", "BBBB2222222", PackLine.JL_Calc_ContainerNumber);
			AssertEquals("Pivot1.IsDeleted", true, pivot1.IsDeleted);

			JobContainerPackPivot[] pivots = (JobContainerPackPivot[])Factory.Load(typeof(JobContainerPackPivot), new ZQuery(JobContainerPackPivotSchema.J6_JL, PackLine.PK));
			AssertEquals("Retrieved Pivot count", 1, pivots.Length);
			AssertEquals("J6_JC", container2.PK, pivots[0].J6_JC);
		}

		public void TestJL_JC()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer container1 = consol.Containers.AddNew();
			CommonContainer container2 = consol.Containers.AddNew();
			Shipment.Consols.Add(consol);

			PackLine.JL_FreightMode = FreightConstants.OuterPackType;
			PackLine.JL_JS = Shipment.PK;

			PackLine.SetContainer(consol, container1);
			JobContainerPackPivot pivot1 = (JobContainerPackPivot)PackLine.Containers.GetRelationshipBusinessObject(container1);

			Factory.Save();

			PackLine.CurrentConsol = consol;
			AssertEquals("Get Calc_ContainerNumber.", container1.PK, PackLine.JL_JC);

			PackLine.JL_JC = container2.PK;
			AssertEquals("Get Calc_ContainerNumber.", container2.PK, PackLine.JL_JC);
			AssertEquals("Pivot1.IsDeleted", true, pivot1.IsDeleted);

			JobContainerPackPivot[] pivots = (JobContainerPackPivot[])Factory.Load(typeof(JobContainerPackPivot), new ZQuery(JobContainerPackPivotSchema.J6_JL, PackLine.PK));
			AssertEquals("Retrieved Pivot count", 1, pivots.Length);
			AssertEquals("J6_JC", container2.PK, pivots[0].J6_JC);
		}

		#endregion

		#region TestJL_Calc_JS_MarksAndNumbers

		public void TestJL_Calc_JS_MarksAndNumbers()
		{
			Shipment.JS_MarksAndNumbers = "bob";
			AssertEquals(Shipment.JS_MarksAndNumbers, PackLine.JL_Calc_JS_MarksAndNumbers);
		}

		#endregion

		#region TestJL_Calc_JS_Destination

		public void TestJL_Calc_JS_Destination()
		{
			Shipment.JS_RL_NKDestination = "DEST";
			AssertEquals(Shipment.JS_RL_NKDestination, PackLine.JL_Calc_JS_Destination);
		}

		#endregion

		#region TestContainer

		public void TestContainer()
		{
			CommonConsol consol = Factory.New<CommonConsol>();

			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "AAAA1111111";

			CommonContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "BBBB2222222";

			Shipment.Consols.Add(consol);

			PackLine.JL_FreightMode = FreightConstants.OuterPackType;
			PackLine.SetContainer(consol, container1);
			Factory.Save();

			var retrievedPackLine = Factory.Load<PackLine>(PackLine.PK);
			AssertEquals("Retrieved Packline's container number", container1.JC_ContainerNum, retrievedPackLine.GetContainer(consol).JC_ContainerNum);

			PackLine.SetContainer(consol, container2);
			PackLine.SetContainer(consol, container1);
			AssertEquals("Containers", 1, PackLine.Containers.Count);
		}

		#endregion

		#region TestGetContainer + TestGetContainerWithMultipleContainersReomvesBadContainer

		public void TestGetContainer()
		{
			ErrorReporter.Clear();
			CommonConsol consol = Factory.New<CommonConsol>();

			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "AAAA1111111";

			CommonContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "BBBB2222222";

			Shipment.Consols.Add(consol);

			PackLine.Containers.RemoveAll();
			container1.PackLines.Add(PackLine);

			CommonContainer retrievedContainer = PackLine.GetContainer(consol);
			AssertEquals("No error should have been reported", "", ErrorReporter.LastMessageReported);
		}

		public void TestGetContainerWithMultipleContainersRemovesExtraContainers()
		{
			CommonConsol consol = Factory.New<CommonConsol>();

			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "AAAA1111111";

			CommonContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "BBBB2222222";

			Shipment.Consols.Add(consol);

			PackLine.JL_FreightMode = FreightConstants.OuterPackType;
			container1.PackLines.Add(PackLine);
			container2.PackLines.Add(PackLine);

			try
			{
				CommonContainer retrievedContainer = PackLine.GetContainer(consol);
				AssertEquals("Should have the packline", 1, retrievedContainer.PackLines.Count);

				CommonContainer otherContainer = (retrievedContainer == container1 ? container2 : container1);
				AssertEquals("Should have removed packline from 2nd container", 0, otherContainer.PackLines.Count);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		#endregion

		#region TestCalculatedVolume

		public void TestCalculatedVolume()
		{
			PackLine.JL_PackageCount = 1;
			PackLine.JL_Length = 1m;
			PackLine.JL_Width = 1m;
			PackLine.JL_Height = 1.5m;
			PackLine.JL_UnitOfDimension = Constants.Length.Metres;
			PackLine.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
			AssertEquals("Actual Volume", 1.5m, PackLine.CalculatedVolume);

			PackLine.JL_UnitOfDimension = "";
			PackLine.JL_Height = 1.7m;
			AssertEquals("Blank Length Unit.", 1.5m, PackLine.CalculatedVolume);

			PackLine.JL_UnitOfDimension = "XX";
			AssertEquals("Invalid Length Unit.", 1.5m, PackLine.CalculatedVolume);

			PackLine.JL_ActualVolumeUQ = "";
			PackLine.JL_UnitOfDimension = Constants.Length.Metres;
			AssertEquals("Blank Volume Unit.", 1.5m, PackLine.CalculatedVolume);

			PackLine.JL_ActualVolumeUQ = "XX";
			AssertEquals("Invalid Volume Unit.", 1.5m, PackLine.CalculatedVolume);

			PackLine.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
			PackLine.JL_ActualVolume = 2.5m;
			AssertEquals("Length and Volume Units valid.", 1.7m, PackLine.CalculatedVolume);
		}

		#endregion

		#region TestCalculatedOutturnedVolume

		public void TestCalculatedOutturnedVolume()
		{
			PackLine.JL_PackageCount = 3;
			PackLine.JL_OutturnedLength = 1m;
			PackLine.JL_OutturnedWidth = 1m;
			PackLine.JL_OutturnedHeight = 1.5m;
			PackLine.JL_Outturn = 2;
			PackLine.JL_UnitOfDimension = Constants.Length.Metres;
			PackLine.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
			AssertEquals("Outturned Volume", 3m, PackLine.CalculatedOutturnedVolume);

			PackLine.JL_UnitOfDimension = "";
			PackLine.JL_OutturnedHeight = 1.7m;
			AssertEquals("Blank Length Unit.", 3m, PackLine.CalculatedOutturnedVolume);

			PackLine.JL_UnitOfDimension = "XX";
			AssertEquals("Invalid Length Unit.", 3m, PackLine.CalculatedOutturnedVolume);

			PackLine.JL_ActualVolumeUQ = "";
			PackLine.JL_UnitOfDimension = Constants.Length.Metres;
			AssertEquals("Blank Volume Unit.", 3m, PackLine.CalculatedOutturnedVolume);

			PackLine.JL_ActualVolumeUQ = "XX";
			AssertEquals("Invalid Volume Unit.", 3m, PackLine.CalculatedOutturnedVolume);

			PackLine.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
			AssertEquals("Length and Volume Units valid.", 3.4m, PackLine.CalculatedOutturnedVolume);
		}

		#endregion

		#region TestJL_Calc_SealNumber

		public void TestJL_Calc_SealNumber()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_SealNum = "1234";
			Shipment.Consols.Add(consol);

			PackLine.JL_JS = Shipment.PK;
			PackLine.CurrentConsol = consol;

			JobContainerPackPivot pivot1 = Factory.New<JobContainerPackPivot>();
			pivot1.J6_JL = PackLine.PK;
			pivot1.J6_JC = container1.PK;

			Factory.Save();

			AssertEquals("JL_Calc_SealNumber", "1234", PackLine.JL_Calc_SealNumber);
		}

		#endregion

		#region JL_OutturnUD

		public void TestJL_OutturnUD()
		{
			PackLine line = Factory.New<PackLine>();
			line.JL_UnitOfDimension = Constants.Length.Inches;
			AssertEquals("OutturnUD should return JL_UnitOfDimension.", line.JL_UnitOfDimension, line.JL_OutturnUD);
			Assert("OutturnUD should be read only.", line.JL_OutturnUDInfo.ReadOnly);
		}

		#endregion

		#region TestContainers

		public void TestContainers()
		{
			CommonContainer container1 = Factory.New<CommonContainer>();
			CommonContainer container2 = Factory.New<CommonContainer>();
			CommonContainer container3 = Factory.New<CommonContainer>();

			JobContainerPackPivot pivot1 = Factory.New<JobContainerPackPivot>();
			pivot1.J6_JL = PackLine.PK;
			pivot1.J6_JC = container1.PK;

			JobContainerPackPivot pivot2 = Factory.New<JobContainerPackPivot>();
			pivot2.J6_JL = PackLine.PK;
			pivot2.J6_JC = container2.PK;

			Factory.Save();

			AssertEquals("Count", 2, PackLine.Containers.Count);
		}

		#endregion

		#region TestPkgPackageCollection

		public void TestPkgPackageCollection()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			var pkgPackageCollection = packline.PkgPackageCollection;
			var pkgPackage = pkgPackageCollection.AddNew();
			pkgPackage.FillWithValidTestData();

			AssertNotNull("PkgPackage created successfully!", pkgPackage);

			Factory.Save();

			var query = new ZQuery(new ZQuery(JobPackLinePackageSchema.JPP_JL_PackLine, packline.PK));
			query.AddToFilter(new ZQuery(JobPackLinePackageSchema.JPP_KP_Packge, pkgPackage.PK), JoinCondition.And);

			var pivots = Factory.Load<JobPackLinePackage>(query);
			AssertEquals("Pivot Created.", 1, pivots.Length);

			pkgPackageCollection.RemoveFromRelationship(pkgPackage);
			Factory.Save();

			pivots = Factory.Load<JobPackLinePackage>(query);
			AssertEquals(0, pivots.Length);
		}

		public void TestDeletePacklineDeletesPkgpackage()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			var pkgPackageCollection = packline.PkgPackageCollection;
			var pkgPackage = pkgPackageCollection.AddNew();
			pkgPackage.FillWithValidTestData();

			AssertNotNull("PkgPackage created successfully!", pkgPackage);

			Factory.Save();

			packline.Delete();
			Factory.Save();
			Assert(pkgPackage.IsDeleted);
		}

		public void TestDeletePacklineDeletesPkgpackageForLink()
		{
			using (FreightConfigurationRegistry.Instance.EnableTWPackageLinking.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = Factory.NewWithValidTestData<CommonShipment>();
				Factory.Save();

				var packline1 = shipment.OuterPackLines.AddNew();
				packline1.JL_F3_NKPackType = "PLT";
				packline1.JL_PackageCount = 1;
				var pkgPackage1 = packline1.PkgPackageCollection.AddNew();
				pkgPackage1.KP_F3_NKPackType = "PLT";
				pkgPackage1.KP_PackageID = "PKG1";

				var packline2 = shipment.OuterPackLines.AddNew();
				packline2.JL_F3_NKPackType = "PLT";
				packline2.JL_PackageCount = 1;
				var pkgPackage2 = packline2.PkgPackageCollection.AddNew();
				pkgPackage2.KP_F3_NKPackType = "PLT";
				pkgPackage2.KP_PackageID = "PKG2";

				var packageJob = Factory.New<PkgPackageJob>();
				packageJob.KJ_JobID = "RC0001";
				packageJob.KJ_ParentID = shipment.PK;
				packageJob.KJ_ParentTableCode = shipment.TablePrefix;
				pkgPackage1.KP_KJ_ParentPackageJob = packageJob.PK;
				pkgPackage2.KP_KJ_ParentPackageJob = packageJob.PK;

				AssertNotNull("PkgPackage created successfully!", pkgPackage1);
				AssertNotNull("PkgPackage created successfully!", pkgPackage2);

				Factory.Save();

				packline1.Delete();
				Factory.Save();
				Assert(packline1.IsDeleted);
				Assert(pkgPackage1.IsDeleted);
				Assert(!packageJob.IsDeleted);

				packline2.Delete();
				Factory.Save();
				Assert(packline2.IsDeleted);
				Assert(pkgPackage2.IsDeleted);
				Assert(packageJob.IsDeleted);

				var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
				var warehouse = (Warehouse.Integration.IWhsWarehouse)helper.CreateWarehouse("WAREHOUSE NAME", "TWH", "A");
				var receiveConsignment = helper.CreateReceiveConsignment("RC0003", warehouse.PK);
				var rcnPackageJob = Factory.New<PkgPackageJob>();
				rcnPackageJob.KJ_JobID = "RC0003";
				rcnPackageJob.KJ_ParentID = receiveConsignment.PK;
				rcnPackageJob.KJ_ParentTableCode = receiveConsignment.TablePrefix;

				var packline3 = shipment.OuterPackLines.AddNew();
				packline3.JL_F3_NKPackType = "PLT";
				packline3.JL_PackageCount = 1;
				var pkgPackage3 = packline3.PkgPackageCollection.AddNew();
				pkgPackage3.KP_F3_NKPackType = "PLT";
				pkgPackage3.KP_PackageID = "PKG3";
				pkgPackage3.KP_KJ_ParentPackageJob = rcnPackageJob.PK;
				Factory.Save();

				packline3.Delete();
				Factory.Save();
				Assert(packline3.IsDeleted);
				Assert(!pkgPackage3.IsDeleted);
				Assert(!rcnPackageJob.IsDeleted);

				var packageJob4 = Factory.New<PkgPackageJob>();
				packageJob4.KJ_JobID = "RC0004";
				packageJob4.KJ_ParentID = shipment.PK;
				packageJob4.KJ_ParentTableCode = shipment.TablePrefix;
				var packline4 = shipment.OuterPackLines.AddNew();
				packline4.JL_F3_NKPackType = "PLT";
				packline4.JL_PackageCount = 1;
				var pkgPackage41 = packline4.PkgPackageCollection.AddNew();
				pkgPackage41.KP_F3_NKPackType = "PLT";
				pkgPackage41.KP_PackageID = "PKG41";
				var pkgPackage42 = packline4.PkgPackageCollection.AddNew();
				pkgPackage42.KP_F3_NKPackType = "PLT";
				pkgPackage42.KP_PackageID = "PKG42";
				pkgPackage41.KP_KJ_ParentPackageJob = packageJob4.PK;
				pkgPackage42.KP_KJ_ParentPackageJob = rcnPackageJob.PK;
				Factory.Save();

				packline4.Delete();
				Factory.Save();
				Assert(packline4.IsDeleted);
				Assert(packageJob4.IsDeleted);
				Assert(pkgPackage41.IsDeleted);
				Assert(!pkgPackage42.IsDeleted);
				Assert(!rcnPackageJob.IsDeleted);
			}
		}

		#endregion

		#region TestPkgPackageCollection_Totals

		public void TestPkgPackageCollection_DefaultsToEmptyCollection()
		{
			var packLine = Factory.New<PackLine>();

			AssertEquals("PkgPackageCollection defaults to an empty collection", 0, packLine.PkgPackageCollection.Count);
		}

		public void TestPkgPackageCollection_TotalQty()
		{
			var packLine = Factory.New<PackLine>();
			var package1 = packLine.PkgPackageCollection.AddNew();
			package1.KP_PackageQty = 5;
			var package2 = packLine.PkgPackageCollection.AddNew();
			package2.KP_PackageQty = 7;

			AssertEquals("PkgPackageCollection_TotalQty returns sum of packages quantity", 12, packLine.PkgPackageCollection_TotalQty);
		}

		public void TestPkgPackageCollection_TotalWeight()
		{
			var packLine = Factory.New<PackLine>();
			packLine.JL_ActualWeightUQ = "KG";
			var package1 = packLine.PkgPackageCollection.AddNew();
			package1.KP_Weight = 5;
			package1.KP_WeightUQ = "KG";
			var package2 = packLine.PkgPackageCollection.AddNew();
			package2.KP_Weight = 7;
			package2.KP_WeightUQ = "KG";

			AssertEquals("PkgPackageCollection_TotalWeight returns sum of packages weight", 12m, packLine.PkgPackageCollection_TotalWeight);
		}

		public void TestPkgPackageCollection_TotalWeight_ConvertsToPackLineUQ()
		{
			var packLine = Factory.New<PackLine>();
			packLine.JL_ActualWeightUQ = "KG";

			var package1 = packLine.PkgPackageCollection.AddNew();
			package1.KP_Weight = 2.205;
			package1.KP_WeightUQ = "LB";
			var package2 = packLine.PkgPackageCollection.AddNew();
			package2.KP_Weight = 1;
			package2.KP_WeightUQ = "KG";

			AssertEquals("PkgPackageCollection_TotalWeight returns sum of packages weight in packLine weight unit", 2m, packLine.PkgPackageCollection_TotalWeight);
		}

		public void TestPkgPackageCollection_TotalWeight_ConvertsSafely()
		{
			AssertNoExceptionThrown("PkgPackageCollection_TotalWeight sums packages weight safely", () =>
			{
				var packLine = Factory.New<PackLine>();
				packLine.JL_ActualWeightUQ = "ZZ";
				var package1 = packLine.PkgPackageCollection.AddNew();
				package1.KP_Weight = 2.205;
				package1.KP_WeightUQ = "LB";
				var package2 = packLine.PkgPackageCollection.AddNew();
				package2.KP_Weight = 1;
				package2.KP_WeightUQ = "AA";

				_ = packLine.PkgPackageCollection_TotalWeight;
			});
		}

		public void TestPkgPackageCollection_TotalVolume()
		{
			var packLine = Factory.New<PackLine>();
			packLine.JL_ActualVolumeUQ = "L";
			var package1 = packLine.PkgPackageCollection.AddNew();
			package1.KP_Volume = 5;
			package1.KP_VolumeUQ = "L";
			var package2 = packLine.PkgPackageCollection.AddNew();
			package2.KP_Volume = 7;
			package2.KP_VolumeUQ = "L";

			AssertEquals("PkgPackageCollection_TotalVolume returns sum of packages volume", 12m, packLine.PkgPackageCollection_TotalVolume);
		}

		public void TestPkgPackageCollection_TotalVolume_ConvertsToPackLineUQ()
		{
			var packLine = Factory.New<PackLine>();
			packLine.JL_ActualWeightUQ = "M3";

			var package1 = packLine.PkgPackageCollection.AddNew();
			package1.KP_Volume = 35.315;
			package1.KP_VolumeUQ = "CF";
			var package2 = packLine.PkgPackageCollection.AddNew();
			package2.KP_Volume = 1;
			package2.KP_VolumeUQ = "M3";

			AssertEquals("PkgPackageCollection_TotalVolume returns sum of packages volume in packLine volume unit", 2m, packLine.PkgPackageCollection_TotalVolume);
		}

		public void TestPkgPackageCollection_TotalVolume_ConvertsSafely()
		{
			AssertNoExceptionThrown("PkgPackageCollection_TotalVolume sums packages volume safely", () =>
			{
				var packLine = Factory.New<PackLine>();
				packLine.JL_ActualVolumeUQ = "ZZ";
				var package1 = packLine.PkgPackageCollection.AddNew();
				package1.KP_Volume = 2.205;
				package1.KP_VolumeUQ = "M3";
				var package2 = packLine.PkgPackageCollection.AddNew();
				package2.KP_Volume = 1;
				package2.KP_VolumeUQ = "AA";

				_ = packLine.PkgPackageCollection_TotalVolume;
			});
		}

		#endregion

		public void TestSetUNDGsFromPackageCollection_HasOverpack()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var packLine = shipment.OuterPackLines.AddNew();

			var package1 = packLine.PkgPackageCollection.AddNew();
			package1.KP_F3_NKPackType = "PLT";
			package1.KP_PackageID = "OVP-1";
			var innerPack1 = CreateInnerPackage(package1);
			innerPack1.KP_F3_NKPackType = "PLT";
			innerPack1.KP_PackageID = "PLT-1";
			AddUNDGToPkgPackage(innerPack1, "1111", "1");
			AddUNDGToPkgPackage(innerPack1, "2222", "2");
			var innerPack2 = CreateInnerPackage(package1);
			innerPack2.KP_F3_NKPackType = "PLT";
			innerPack2.KP_PackageID = "PLT-2";

			var package2 = packLine.PkgPackageCollection.AddNew();
			package2.KP_F3_NKPackType = "PLT";
			package2.KP_PackageID = "PLT-3";
			AddUNDGToPkgPackage(package2, "3333", "3");

			packLine.SetUNDGsFromPackageCollection();
			AssertEquals("3 UNDGs are created from Package Collection", 3, packLine.UNDGs.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "1111", "2222", "3333" }, packLine.UNDGs.Select(item => item.UNDGSubstance.DG_UNNO));
			CombineAssertions("HasOverpack", () =>
			{
				var overpackItems = packLine.UNDGs.Where(item => item.DI_HasOverpack).ToArray();
				AssertEquals(2, overpackItems.Length);
				AssertContainsExactElementsInAnyOrder(new[] { "1111", "2222" }, overpackItems.Select(item => item.UNDGSubstance.DG_UNNO));
				Assert(overpackItems.All(item => item.DI_OverpackID == "OVP-1"));
			});
		}

		public void TestRoundingRoundDownDecimalToItsPrecisionAndScaleOnDataImport()
		{
			var collection = new DefaultNumberOfDecimalsCollection(Enterprise.Registry.Business.Module.Freight);
			var registryEntryWeight = new DefaultNumberOfDecimals();
			registryEntryWeight.UnitOfMeasure = Constants.Weight.Kilograms;
			registryEntryWeight.TransportMode = Constants.TransportModes.Air;
			registryEntryWeight.NumberOfDecimals = 1;
			registryEntryWeight.RoundingMode = RoundingModes.Up;
			collection.Add(registryEntryWeight);

			var registryEntryVolume = new DefaultNumberOfDecimals();
			registryEntryVolume.UnitOfMeasure = Constants.Volume.CubicMetres;
			registryEntryVolume.TransportMode = Constants.TransportModes.Air;
			registryEntryVolume.NumberOfDecimals = 1;
			registryEntryVolume.RoundingMode = RoundingModes.Up;
			collection.Add(registryEntryVolume);

			var registryEntryDimension = new DefaultNumberOfDecimals();
			registryEntryDimension.UnitOfMeasure = Constants.Dimension.Metres;
			registryEntryDimension.TransportMode = Constants.TransportModes.Air;
			registryEntryDimension.NumberOfDecimals = 1;
			registryEntryDimension.RoundingMode = RoundingModes.Up;
			collection.Add(registryEntryDimension);

			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
			packLine.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			using (packLine.GetValidationSuspender())
			{
				packLine.JL_ActualVolume = 999999.999m;
				packLine.JL_ActualWeight = 999999.999m;
				packLine.JL_Length = 999999.999m;
				packLine.JL_Height = 999999.999m;
				packLine.JL_Width = 999999.999m;
				packLine.JL_OutturnedLength = 999999.999m;
				packLine.JL_OutturnedHeight = 999999.999m;
				packLine.JL_OutturnedWidth = 999999.999m;
				packLine.JL_OutturnedVolume = 999999.999m;
				packLine.JL_OutturnedWeight = 999999.999m;
			}

			AssertEquals((ZDecimal)999999.9, packLine.JL_ActualVolume);
			AssertEquals((ZDecimal)999999.9, packLine.JL_ActualWeight);
			AssertEquals((ZDecimal)999999.9, packLine.JL_Length);
			AssertEquals((ZDecimal)999999.9, packLine.JL_Height);
			AssertEquals((ZDecimal)999999.9, packLine.JL_Width);
			AssertEquals((ZDecimal)999999.9, packLine.JL_OutturnedLength);
			AssertEquals((ZDecimal)999999.9, packLine.JL_OutturnedHeight);
			AssertEquals((ZDecimal)999999.9, packLine.JL_OutturnedWidth);
			AssertEquals((ZDecimal)999999.9, packLine.JL_OutturnedWeight);
		}

		public void TestSetUNDGsFromPackageCollection_HasPISection()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var packLine = shipment.OuterPackLines.AddNew();

			var package = packLine.PkgPackageCollection.AddNew();
			package.KP_F3_NKPackType = "PLT";
			package.KP_PackageID = "OVP-1";

			var innerPack = CreateInnerPackage(package);
			innerPack.KP_F3_NKPackType = "PLT";
			innerPack.KP_PackageID = "PLT-1";

			var packagingInstruction = "PT";
			var undgDataItem = AddUNDGToPkgPackage(innerPack, "1111", "1");
			undgDataItem.DI_PackingInstructionSection = packagingInstruction;

			packLine.SetUNDGsFromPackageCollection();
			AssertEquals("3 UNDGs are created from Package Collection", 1, packLine.UNDGs.Count);
			AssertEquals("PI Section has value", packagingInstruction, packLine.UNDGs[0].DI_PackingInstructionSection);
		}

		PkgPackage CreateInnerPackage(PkgPackage handlingUnitPackage)
		{
			var innerPackage1 = Factory.New<PkgPackage>();
			var divot = handlingUnitPackage.PackageHandlingUnitHandlingUnitDivots.AddNew();
			divot.KPD_KP_Package = innerPackage1.PK;
			return innerPackage1;
		}

		UNDGDataItem AddUNDGToPkgPackage(PkgPackage pkg, string unno, string imoClass)
		{
			var substanceQuery = new ZQuery();
			substanceQuery.AddToFilter(UNDGSubstanceSchema.DG_Standard, UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);
			substanceQuery.AddToFilter(UNDGSubstanceSchema.DG_UNNO, unno);
			substanceQuery.AddToFilter(UNDGSubstanceSchema.DG_Class, imoClass);
			var substance = Factory.LoadTop1<UNDGSubstance>(substanceQuery);
			if (substance == null)
			{
				substance = Factory.New<UNDGSubstance>();
				substance.DG_UNNO = unno;
				substance.DG_Class = imoClass;
				substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			}

			var undg = pkg.UNDGs.AddNew();
			undg.DI_DG = substance.PK;
			undg.LinkDefault(substance);

			return undg;
		}

		public void TestSetRadioactiveDetailsFromPackageCollection_NewRadioactiveUNDG()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var packLine = shipment.OuterPackLines.AddNew();

			AssertEquals("There are no UNDGs on the packline", 0, packLine.UNDGs.Count);

			var package = packLine.PkgPackageCollection.AddNew();
			package.KP_F3_NKPackType = "PLT";
			package.KP_PackageID = "PLT-1";

			var substance = Factory.New<UNDGSubstance>();
			substance.DG_UNNO = "1111";
			substance.DG_Class = "7";
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			AddUNDGToPkgPackage(package, substance);

			packLine.SetUNDGsFromPackageCollection();

			AssertEquals("1 UNDG is created from Package Collection", 1, packLine.UNDGs.Count);
			AssertUNDGRadioactiveDetails(packLine.UNDGs.FirstOrDefault());
		}

		public void TestSetRadioactiveDetailsFromPackageCollection_ExistingRadioactiveUNDG()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();

			var packLine = shipment.OuterPackLines.AddNew();

			var substance = Factory.New<UNDGSubstance>();
			substance.DG_UNNO = "1111";
			substance.DG_Class = "7";
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			var existingUndg = packLine.UNDGs.AddNew();
			existingUndg.DI_DG = substance.PK;

			AssertEquals("There is an existing UNDG on the packline", 1, packLine.UNDGs.Count);

			var package = packLine.PkgPackageCollection.AddNew();
			package.KP_F3_NKPackType = "PLT";
			package.KP_PackageID = "PLT-1";
			var firstUndgDataItem = AddUNDGToPkgPackage(package, substance);

			packLine.SetUNDGsFromPackageCollection();

			AssertEquals("No new UNDGs have been created", 1, packLine.UNDGs.Count);
			AssertUNDGRadioactiveDetails(packLine.UNDGs.FirstOrDefault());

			firstUndgDataItem.DI_PackingInstructionSection = "PM";

			var secondUndgDataItem = AddUNDGToPkgPackage(package, substance);
			secondUndgDataItem.DI_PackingInstructionSection = "DB";

			packLine.SetUNDGsFromPackageCollection();
			AssertEquals("New UNDGs has been created", 2, packLine.UNDGs.Count);
			AssertEquals("PM", packLine.UNDGs[0].DI_PackingInstructionSection);
			AssertEquals("DB", packLine.UNDGs[1].DI_PackingInstructionSection);

			secondUndgDataItem.DI_PackingInstructionSection = "PM";
			packLine.SetUNDGsFromPackageCollection();
			AssertEquals("Only one UNDG has been created, because they are the same", 1, packLine.UNDGs.Count);
			AssertEquals("PM", packLine.UNDGs[0].DI_PackingInstructionSection);
		}

		public void TestSetRadioactiveDetailsFromPackageCollection_MultipleRadioactiveUNDG()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();

			var packLine = shipment.OuterPackLines.AddNew();

			var substance = Factory.New<UNDGSubstance>();
			substance.DG_UNNO = "1111";
			substance.DG_Class = "7";
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			var existingUndg = packLine.UNDGs.AddNew();
			existingUndg.DI_DG = substance.PK;
			existingUndg.DI_MaterialFormDescription = "New UNDG, what do you think?";
			existingUndg.DI_RadionuclideElement = "Au";

			AssertEquals("There is an existing UNDG on the packline", 1, packLine.UNDGs.Count);

			var package = packLine.PkgPackageCollection.AddNew();
			package.KP_F3_NKPackType = "PLT";
			package.KP_PackageID = "PLT-1";

			var undg1 = AddUNDGToPkgPackage(package, substance);
			undg1.DI_MaterialFormDescription = "Impressive, very nice.";
			undg1.DI_RadionuclideElement = "Ag";

			var undg2 = AddUNDGToPkgPackage(package, substance);
			undg2.DI_MaterialFormDescription = "Let's see Paul Allen's UNDG.";
			undg2.DI_RadionuclideElement = "Zr";

			var undg3 = AddUNDGToPkgPackage(package, substance);
			undg3.DI_MaterialFormDescription = "Oh my God, it even has a watermark.";
			undg3.DI_RadionuclideElement = "U";

			packLine.SetUNDGsFromPackageCollection();

			AssertEquals("The UNDGs from package collection override the existing UNDG", 3, packLine.UNDGs.Count);

			var newDescriptions = packLine.UNDGs.Select(undg => undg.DI_MaterialFormDescription).ToList();
			AssertContainsExactElementsInAnyOrder(new[] { "Impressive, very nice.", "Let's see Paul Allen's UNDG.", "Oh my God, it even has a watermark." }, newDescriptions);

			var newElements = packLine.UNDGs.Select(undg => undg.DI_RadionuclideElement).ToList();
			AssertContainsExactElementsInAnyOrder(new[] { "Ag", "Zr", "U" }, newElements);
		}

		UNDGDataItem AddUNDGToPkgPackage(PkgPackage pkg, UNDGSubstance substance)
		{
			var undg = pkg.UNDGs.AddNew();
			undg.DI_DG = substance.PK;
			undg.DI_RadionuclideElement = "U";
			undg.DI_RadionuclideElementSuffix = "230";
			undg.DI_RadioactiveMaximumActivity = 6.9m;
			undg.DI_RadioactiveMaximumActivityUnit = "TBQ";
			undg.DI_RadioactiveLabelCategory = "WH1";
			undg.DI_RadioactiveTransportIndex = 4.2m;
			undg.DI_MaterialFormDescription = "New radioactive isotope, what do you think?";
			undg.DI_IsFissileExcepted = true;
			undg.DI_IsExclusiveUse = true;
			undg.DI_IsHighwayRouteControlledQuantity = true;
			undg.DI_PackingInstructionSection = "IA";

			undg.LinkDefault(substance);
			return undg;
		}

		void AssertUNDGRadioactiveDetails(UNDGDataItem undg)
		{
			CombineAssertions("Radioactive details have been updated", () =>
			{
				AssertEquals("U", undg.DI_RadionuclideElement);
				AssertEquals("230", undg.DI_RadionuclideElementSuffix);
				AssertEquals(6.9m, undg.DI_RadioactiveMaximumActivity);
				AssertEquals("TBQ", undg.DI_RadioactiveMaximumActivityUnit);
				AssertEquals("WH1", undg.DI_RadioactiveLabelCategory);
				AssertEquals(4.2m, undg.DI_RadioactiveTransportIndex);
				AssertEquals("New radioactive isotope, what do you think?", undg.DI_MaterialFormDescription);
				AssertEquals(true, undg.DI_IsFissileExcepted);
				AssertEquals(true, undg.DI_IsExclusiveUse);
				AssertEquals(true, undg.DI_IsHighwayRouteControlledQuantity);
				AssertEquals("IA", undg.DI_PackingInstructionSection);
			});
		}

		#region TestAllContainersOnShipment_List

		public void TestAllContainersOnShipment_List()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer container1 = consol.Containers.AddNew();
			CommonContainer container2 = consol.Containers.AddNew();

			var shipment = Factory.New<CommonShipment>();
			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			AssertEquals("Count", 0, packLine1.AllContainersOnShipment_List.Count);

			shipment.Consols.Add(consol);
			packLine1.CurrentConsol = consol;
			AssertEquals("Count", 2, packLine1.AllContainersOnShipment_List.Count);
		}

		#endregion

		#region TestGetSetContainerUsingConsol

		public void TestGetSetContainerUsingConsol()
		{
			CommonShipment shipment = CommonShipment.New(Factory);
			CommonConsol consol = shipment.Consols.AddNew();

			CommonContainer container1 = consol.Containers.AddNew();
			CommonContainer container2 = consol.Containers.AddNew();

			PackLine packLine1 = null;
			if (shipment.OuterPackLines.Count == 1)
			{
				packLine1 = shipment.OuterPackLines[0];
			}
			else
			{
				packLine1 = shipment.OuterPackLines.AddNew();
			}

			AssertNotNull("GetContainer", packLine1.GetContainer(consol));
			AssertEquals("Containers.Count", 1, packLine1.Containers.Count);

			packLine1.SetContainer(consol, container1);
			AssertEquals("GetContainer", container1, packLine1.GetContainer(consol));
			AssertEquals("Containers.Count", 1, packLine1.Containers.Count);

			packLine1.SetContainer(consol, container2);
			AssertEquals("GetContainer", container2, packLine1.GetContainer(consol));
			AssertEquals("Containers.Count", 1, packLine1.Containers.Count);

			packLine1.SetContainer(consol, null);
			AssertNull("GetContainer", packLine1.GetContainer(consol));
			AssertEquals("Containers.Count", 0, packLine1.Containers.Count);
		}

		#endregion

		#region TestGetSetContainerUsingSailing

		public void TestGetSetContainerUsingSailing()
		{
			var vessel = RefVessel.LookupVesselByName("ARAFURA", Factory).First();

			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_VoyageFlight = "234";
			voyage.JV_RV_NKVessel = vessel.RV_FK;

			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Origins.Add(origin);

			VoyageDestination destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = "USLAX";
			voyage.Destinations.Add(destination);

			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];

			CommonContainer tempContainer = sailing.Containers.AddNew();
			var container1 = Factory.Load<CommonContainer>(tempContainer.PK);

			tempContainer = sailing.Containers.AddNew();
			var container2 = Factory.Load<CommonContainer>(tempContainer.PK);

			AssertEquals("JC_JX", sailing.PK, container1.JC_JX);
			AssertEquals("JC_JX", sailing.PK, container2.JC_JX);

			CommonShipment shipment = CommonShipment.New(Factory);
			PackLine packLine1 = shipment.OuterPackLines.AddNew();

			AssertNull("GetContainer", packLine1.GetContainer(sailing));
			AssertEquals("Containers.Count", 0, packLine1.Containers.Count);

			packLine1.SetContainer(sailing, container1);
			AssertEquals("GetContainer", container1, packLine1.GetContainer(sailing));
			AssertEquals("Containers.Count", 1, packLine1.Containers.Count);

			packLine1.SetContainer(sailing, container2);
			AssertEquals("GetContainer", container2, packLine1.GetContainer(sailing));
			AssertEquals("Containers.Count", 1, packLine1.Containers.Count);

			packLine1.SetContainer(sailing, null);
			AssertNull("GetContainer", packLine1.GetContainer(sailing));
			AssertEquals("Containers.Count", 0, packLine1.Containers.Count);
		}

		#endregion

		#region TestGetSetContainerUsingConsolAndSailing

		public void TestGetSetContainerUsingConsolAndSailing()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USLAX";
			transport.JW_Vessel = "ARAFURA";
			transport.JW_VoyageFlight = "234";

			CommonContainer container1 = consol.Containers.AddNew();
			CommonContainer container2 = consol.Containers.AddNew();

			CommonShipment shipment = consol.Shipments.AddNew();
			PackLine packLine1 = null;
			if (shipment.OuterPackLines.Count == 1)
			{
				packLine1 = shipment.OuterPackLines[0];
			}
			else
			{
				packLine1 = shipment.OuterPackLines.AddNew();
			}
			shipment.JS_OuterPacks = 3;

			Assert("JC_JX should be empty.", container1.JC_JX.IsEmpty);
			AssertEquals("Container's sailing should be the consol's primary sailing.", consol.Schedule.PK, container1.Sailing.PK);
			Assert("JC_JX should be empty.", container2.JC_JX.IsEmpty);

			AssertNotNull("GetContainer", packLine1.GetContainer(consol, consol.Schedule));
			AssertEquals("Containers.Count", 1, packLine1.Containers.Count);

			packLine1.SetContainer(consol, consol.Schedule, container1);
			AssertEquals("GetContainer", container1, packLine1.GetContainer(consol, consol.Schedule));
			AssertEquals("Containers.Count", 1, packLine1.Containers.Count);

			packLine1.SetContainer(consol, consol.Schedule, container2);
			AssertEquals("GetContainer", container2, packLine1.GetContainer(consol, consol.Schedule));
			AssertEquals("Containers.Count", 1, packLine1.Containers.Count);

			packLine1.SetContainer(consol, consol.Schedule, null);
			AssertNull("GetContainer", packLine1.GetContainer(consol, consol.Schedule));
			AssertEquals("Containers.Count", 0, packLine1.Containers.Count);
		}

		#endregion

		#region TestSetContainer

		public void TestSetContainer()
		{
			PackLine packLine1 = Factory.New<CommonShipment>().OuterPackLines.AddNew();
			CommonContainer container1 = Factory.New<CommonContainer>();
			CommonContainer container2 = Factory.New<CommonContainer>();

			packLine1.SetContainer(container1.PK);

			AssertEquals("Count of containers", 1, packLine1.Containers.Count);
			AssertEquals("Container", container1, packLine1.Containers[0]);

			packLine1.SetContainer(ZGuid.Empty);
			AssertEquals("Count of containers", 1, packLine1.Containers.Count);
			AssertEquals("Container", container1, packLine1.Containers[0]);

			packLine1.SetContainer(container2.PK);
			AssertEquals("Count of containers", 2, packLine1.Containers.Count);
			AssertEquals("Contains Container 1", true, packLine1.Containers.Contains(container1));
			AssertEquals("Contains Container 0", true, packLine1.Containers.Contains(container2));
		}

		public void TestSetContainer_ErrorReportForContainerNotOnConsol()
		{
			ErrorReporter.Clear();

			var consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = "C00001234";

			var container = Factory.New<CommonContainer>();
			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			container.JC_RC = refContainer.PK;
			container.JC_ContainerNum = "TEST1234567";

			var shipment = consol.Shipments.AddNew();
			var packLine1 = shipment.OuterPackLines.AddNew();
			var packLine2 = shipment.OuterPackLines.AddNew();

			packLine1.SetContainer(consol, container);

			var expectedError = FormattableString.Invariant($"Packline ({packLine1.PK}) should not be packed into a Container TEST1234567 which is not on the Consol C00001234.");
			AssertEquals("Error reported as container is not on the consol", expectedError, ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();

			consol.Containers.Add(container);
			packLine2.SetContainer(consol, container);

			AssertNullOrEmpty("No error as container is on the consol", ErrorReporter.LastMessageReported);
		}

		public void TestSetContainer_ExceedGrossWeightLimit()
		{
			var consol = Factory.New<CommonConsol>();
			var container = consol.Containers.AddNew();
			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			container.JC_RC = refContainer.PK;
			container.JC_ContainerNum = "TEST1234567";

			var shipment = Factory.New<CommonShipment>();
			var packLine1 = shipment.OuterPackLines.AddNew();
			var packLine2 = shipment.OuterPackLines.AddNew();
			var packLine3 = shipment.OuterPackLines.AddNew();
			var packLine4 = shipment.OuterPackLines.AddNew();
			var packLine5 = shipment.OuterPackLines.AddNew();
			packLine1.JL_ActualWeight = 220000m;
			packLine2.JL_ActualWeight = 220000m;
			packLine3.JL_ActualWeight = 220000m;
			packLine4.JL_ActualWeight = 220000m;
			packLine5.JL_ActualWeight = 220000m;

			var jobConShipLink = Factory.New<IJobConShipLink>();
			jobConShipLink.JN_JK = consol.PK;
			jobConShipLink.JN_JS = shipment.PK;

			packLine1.SetContainer(consol, container);
			AssertSetContainerPackingLineResult(packLine1, false);

			packLine2.SetContainer(consol, container);
			AssertSetContainerPackingLineResult(packLine2, false);

			packLine3.SetContainer(consol, container);
			AssertSetContainerPackingLineResult(packLine3, false);

			packLine4.SetContainer(consol, container);
			AssertSetContainerPackingLineResult(packLine4, false);

			packLine5.SetContainer(consol, container);
			AssertSetContainerPackingLineResult(packLine5, true);
		}

		void AssertSetContainerPackingLineResult(PackLine packingLine, bool hasError)
		{
			packingLine.RunPreSaveValidation();
			if (hasError)
			{
				AssertHasErrors(packingLine.JL_ActualWeightInfo);
				AssertHasError(packingLine.JL_ActualWeightInfo, "The container 'TEST1234567' is over-packed, try packing one or more lines into another container, or reduce the lines' weight.");
			}
			else
			{
				AssertNoErrors(packingLine.JL_ActualWeightInfo);
			}
		}

		#endregion

		#region TestContainerCollectionCountChanged

		public void TestContainerCollectionCountChanged()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			PackLine packLine1 = Factory.New<PackLine>();
			packLine1.JL_FreightMode = FreightConstants.OuterPackType;
			packLine1.Containers.CountChanged += new CollectionCountChangedEventHandler(Containers_CountChanged);
			ContainersCountChangedHitCountAdded = 0;
			ContainersCountChangedHitCountRemoved = 0;

			AssertEquals("Containter Count", 0, packLine1.Containers.Count);

			AssertEquals("Added event hit count", 0, ContainersCountChangedHitCountAdded);
			AssertEquals("Removed event hit count", 0, ContainersCountChangedHitCountRemoved);

			CommonContainer container1 = consol.Containers.AddNew();
			packLine1.SetContainer(consol, container1);

			AssertEquals("Added event hit count", 1, ContainersCountChangedHitCountAdded);
			AssertEquals("Removed event hit count", 0, ContainersCountChangedHitCountRemoved);

			AssertEquals("Containter Count", 1, packLine1.Containers.Count);

			CommonContainer container2 = consol.Containers.AddNew();
			packLine1.SetContainer(consol, container2);
			AssertEquals("Containter Count", 1, packLine1.Containers.Count);

			AssertEquals("Added event hit count", 2, ContainersCountChangedHitCountAdded);
			AssertEquals("Removed event hit count", 1, ContainersCountChangedHitCountRemoved);
		}

		#endregion

		#region TestDeletePackLineDeletesPackLocations

		public void TestDeletePackLineDeletesPackLocations()
		{
			PackLine pack = Factory.New<PackLine>();

			AssertEquals("precondition", 0, pack.PackLocations.Count);

			PackLocation location1 = pack.PackLocations.AddNew();
			PackLocation location2 = pack.PackLocations.AddNew();

			AssertEquals("added", 2, pack.PackLocations.Count);
			AssertEquals("added", true, pack.PackLocations.Contains(location1));
			AssertEquals("added", true, pack.PackLocations.Contains(location2));
			AssertEquals("added", false, location1.IsDeleted);
			AssertEquals("added", false, location1.IsDeleted);

			pack.Delete();

			AssertEquals("deleted", 0, pack.PackLocations.Count);
			AssertEquals("deleted", true, location1.IsDeleted);
			AssertEquals("deleted", true, location1.IsDeleted);
		}

		#endregion

		#region TestDeletePackLineDeletesCusEntryNumbers

		public void TestDeletePackLineDeletesCusEntryNumber()
		{
			var pack = Factory.NewWithValidTestData<PackLine>();
			pack.JL_InspectionTypeCode = "XYZ";

			Factory.Save();

			var query = new ZQuery(CusEntryNumSchema.CE_ParentID, pack.PK);
			var cusEntryNums = Factory.Load<CusEntryNumber>(query);
			AssertEquals(1, cusEntryNums.Length);

			pack.Delete();
			cusEntryNums = Factory.Load<CusEntryNumber>(query);
			AssertEquals(0, cusEntryNums.Length);

			pack = Factory.NewWithValidTestData<PackLine>();
			pack.JL_AdditionalInspectionTypeCode = "PHS";

			Factory.Save();

			query = new ZQuery(CusEntryNumSchema.CE_ParentID, pack.PK);
			cusEntryNums = Factory.Load<CusEntryNumber>(query);
			AssertEquals(1, cusEntryNums.Length);

			pack.Delete();
			cusEntryNums = Factory.Load<CusEntryNumber>(query);
			AssertEquals(0, cusEntryNums.Length);
		}

		#endregion

		#region TestDeletePackLineDeletesContainerPivots

		public void TestDeletePackLineDeletesContainerPivots()
		{
			CommonShipment shipment = CommonShipment.New(Factory);

			CommonConsol consol1 = shipment.Consols.AddNew();
			CommonContainer container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "CONT-ONE";

			CommonConsol consol2 = shipment.Consols.AddNew();
			CommonContainer container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = "CONT-TWO";

			shipment.JS_OuterPacks = 1;
			shipment.JS_ActualWeight = 100;
			AssertEquals("Should automatically create 1 OuterPackLine.", 1, shipment.OuterPackLines.Count);
			shipment.OuterPackLines[0].CurrentConsol = consol1;
			shipment.OuterPackLines[0].JL_JC = container1.PK;
			shipment.OuterPackLines[0].CurrentConsol = consol2;
			shipment.OuterPackLines[0].JL_JC = container2.PK;
			Factory.Save();
			AssertEquals("Should be 2 PackContainerPivots", 2, shipment.OuterPackLines[0].Containers.Count);

			CommonContainerManyToManyCollection containerPivots = shipment.OuterPackLines[0].Containers;
			shipment.OuterPackLines[0].Delete();
			AssertEquals("PackContainerPivots should be deleted when PackLine is deleted.", 0, containerPivots.Count);
		}

		#endregion

		#region TestCalculatedVolumeRounding

		public void TestCalculatedVolumeRounding()
		{
			PackLine pack = Factory.New<PackLine>();
			pack.JL_Height = new ZDecimal(1.0);
			pack.JL_PackageCount = 5;
			pack.JL_Height = new ZDecimal(1);
			pack.JL_Width = new ZDecimal(1.515);
			pack.JL_Length = new ZDecimal(0.3);
			pack.JL_UnitOfDimension = Constants.Length.Metres;
			pack.JL_ActualVolume = pack.CalculatedVolume;

			AssertEquals("Pack volume should be 2.73", new ZDecimal(2.273), pack.JL_ActualVolume);
		}

		#endregion

		#region TestIsLooseImport

		public void TestIsLooseImport()
		{
			AssertEquals("PackLine cannot be loose import if it has no shipment", false, (Factory.New<PackLine>()).IsLooseImport);

			CommonShipment shipment = Factory.New<CommonShipment>();
			PackLine packLine = shipment.OuterPackLines.AddNew();
			shipment.JS_RL_NKOrigin = OverseasPort;
			shipment.JS_RL_NKDestination = HomePort;
			AssertEquals("PackLine is loose import if it's CommonShipment is an import and it has no first import container", true, packLine.IsLooseImport);

			shipment.JS_RL_NKOrigin = HomePort;
			shipment.JS_RL_NKDestination = OverseasPort;
			AssertEquals("PackLine cannot be loose import if it's CommonShipment is an export", false, packLine.IsLooseImport);

			CommonConsol consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = OverseasPort;
			consol.JK_RL_NKDischargePort = HomePort;

			Transport transport = consol.Transports[0];
			transport.JW_ETD = ZDateTime.Now;
			transport.JW_ETA = ZDateTime.Now.AddDays(2);
			transport.JW_Vessel = TestVessel1.RV_FK;
			transport.JW_VoyageFlight = "TEST";

			CommonContainer container = consol.Containers.AddNew();
			container.AddPackLine(packLine);
			shipment.JS_RL_NKOrigin = OverseasPort;
			shipment.JS_RL_NKDestination = HomePort;
			AssertEquals("PackLine cannot be loose import if it has a first import container", false, packLine.IsLooseImport);
		}

		#endregion

		#region TestIsOnImportShipment

		public void TestIsOnImportShipment()
		{
			AssertEquals("PackLine cannot be on import if it has no shipment", false, (Factory.New<PackLine>()).IsOnImportShipment);

			CommonShipment shipment = CommonShipment.New(Factory);
			PackLine packLine = shipment.OuterPackLines.AddNew();
			shipment.JS_RL_NKOrigin = OverseasPort;
			shipment.JS_RL_NKDestination = HomePort;
			AssertEquals("PackLine is on import if it's CommonShipment is an import", true, packLine.IsOnImportShipment);

			shipment.JS_RL_NKOrigin = HomePort;
			shipment.JS_RL_NKDestination = OverseasPort;
			AssertEquals("PackLine cannot be on import if it's CommonShipment is not and import", false, packLine.IsOnImportShipment);

			shipment.JS_RL_NKOrigin = OverseasPort;
			shipment.JS_RL_NKDestination = OverseasPort;
			AssertEquals("PackLine cannot be on import if it's CommonShipment is not an import", false, packLine.IsOnImportShipment);
		}

		#endregion

		#region TestIsOnExportShipment

		public void TestIsOnExportShipment()
		{
			AssertEquals("PackLine cannot be on export if it has no shipment", false, (Factory.New<PackLine>()).IsOnExportShipment);

			CommonShipment shipment = CommonShipment.New(Factory);
			shipment.JS_RL_NKOrigin = HomePort;
			shipment.JS_RL_NKDestination = OverseasPort;
			PackLine packLine = shipment.OuterPackLines.AddNew();
			AssertEquals("PackLine is on export if it's CommonShipment is an export", true, packLine.IsOnExportShipment);

			shipment.JS_RL_NKOrigin = OverseasPort;
			shipment.JS_RL_NKDestination = HomePort;
			AssertEquals("PackLine cannot be on export if it's CommonShipment is not an export", false, packLine.IsOnExportShipment);

			shipment.JS_RL_NKOrigin = OverseasPort;
			shipment.JS_RL_NKDestination = OverseasPort;
			AssertEquals("PackLine cannot be on export if it's CommonShipment is not an export", false, packLine.IsOnExportShipment);
		}

		#endregion

		#region TestIsOnSubShipment

		public void TestIsOnColoadMasterShipment()
		{
			AssertIsOnColoadMasterShipment(Constants.ShipmentTypes.CoLoadMaster);
			AssertIsOnColoadMasterShipment(Constants.ShipmentTypes.BlindCoLoadMaster);
		}

		void AssertIsOnColoadMasterShipment(ZString shipmentType)
		{
			var masterShipment = Factory.New<CommonShipment>();
			var subShipment = Factory.New<CommonShipment>();

			var masterPackLine = masterShipment.OuterPackLines.AddNew();
			var subPackLine = subShipment.OuterPackLines.AddNew();

			masterShipment.JS_ShipmentType = shipmentType;
			subShipment.JS_JS_ColoadMasterShipment = masterShipment.PK;

			Assert("masterPackLine is removed from master CommonShipment so should return false", !masterPackLine.IsOnColoadMasterShipment);
			AssertNull(masterPackLine.Shipment);
			Assert("subPackLine is on a sub CommonShipment so should return false", !subPackLine.IsOnColoadMasterShipment);

			var masterShipmentWithNoSubs = Factory.New<CommonShipment>();
			var masterPackLinesWithNoSubs = masterShipmentWithNoSubs.OuterPackLines.AddNew();
			masterShipmentWithNoSubs.JS_ShipmentType = shipmentType;
			Assert("masterPackLinesWithNoSubs is on a master CommonShipment but has no subs, so should return false", !masterPackLinesWithNoSubs.IsOnColoadMasterShipment);
		}

		#endregion

		#region TestJL_Calc_PackagesToDeliver

		public void TestJL_Calc_PackagesToDeliver()
		{
			CommonConsol consol = GetImportConsol(typeof(CommonConsol));
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport = consol.Transports[0];
			transport.JW_ETA = ZDateTime.Today;
			transport.JW_Vessel = TestVessel1.RV_FK;
			transport.JW_VoyageFlight = "855";

			CommonContainer container = consol.Containers.AddNew();
			CommonShipment shipment = Factory.New<CommonShipment>();

			PackLine containerPackLine = shipment.OuterPackLines.AddNew();
			//ContainerPackLine.SetContainer(Consol, Container);
			//Container.JC_LCLUnpack = ZDateTime.Today;
			containerPackLine.JL_Outturn = 7;
			AssertEquals("Packages to Deliver should = outurn", containerPackLine.JL_Outturn, containerPackLine.JL_Calc_PackagesToDeliver);
			containerPackLine.JL_Outturn = 0;
			container.JC_LCLUnpack = ZDateTime.Empty;
			containerPackLine.JL_PackageCount = 5;
			AssertEquals("Packages to Deliver should = package count", containerPackLine.JL_PackageCount, containerPackLine.JL_Calc_PackagesToDeliver);

			PackLine shipmentPackLine = shipment.OuterPackLines.AddNew();
			//ShipmentPackLine.SetContainer(Consol, Container);
			//Container.JC_LCLUnpack = ZDateTime.Today;
			shipmentPackLine.JL_Outturn = 4;
			AssertEquals("CommonShipment PackLine Packages to deliver should = out turn", shipmentPackLine.JL_Outturn, shipmentPackLine.JL_Calc_PackagesToDeliver);
			shipmentPackLine.JL_Outturn = 0;
			shipmentPackLine.JL_PackageCount = 3;
			AssertEquals("Packages to Deliver should = package count", shipmentPackLine.JL_PackageCount, shipmentPackLine.JL_Calc_PackagesToDeliver);
		}

		#endregion

		#region TestJL_Calc_PackLineToPkgPackage Discrepancies

		public void TestJL_Calc_PacklineToPkgPackage_QtyDiscrepancy()
		{
			var packLine = Factory.New<PackLine>();
			packLine.JL_PackageCount = 3;

			var pkgPackage1 = Factory.New<PkgPackage>();
			var pkgPackage2 = Factory.New<PkgPackage>();
			var pkgPackage3 = Factory.New<PkgPackage>();
			pkgPackage1.KP_PackageQty = 1;
			pkgPackage2.KP_PackageQty = 2;
			pkgPackage3.KP_PackageQty = 3;

			CombineAssertions("JL_Calc_PacklineToPkgPackage_QtyDiscrepancy Calculation", () =>
			{
				AssertEquals("Discrepancy calculated when PkgPackageCollection is empty", 3, packLine.JL_Calc_PacklineToPkgPackage_QtyDiscrepancy);

				packLine.PkgPackageCollection.Add(pkgPackage1);
				AssertEquals("Positive discrepancy calculated when packLine has more quantity than packages", 2, packLine.JL_Calc_PacklineToPkgPackage_QtyDiscrepancy);

				packLine.PkgPackageCollection.Add(pkgPackage2);
				AssertEquals("No discrepancy calculated when packLine quantity equals packages quantity", 0, packLine.JL_Calc_PacklineToPkgPackage_QtyDiscrepancy);

				packLine.PkgPackageCollection.Add(pkgPackage3);
				AssertEquals("Negative discrepancy calculated when packLine has less quantity than packages", -3, packLine.JL_Calc_PacklineToPkgPackage_QtyDiscrepancy);
			});
		}

		public void TestJL_Calc_PacklineToPkgPackage_WeightDiscrepancy()
		{
			var packLine = Factory.New<PackLine>();
			packLine.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			packLine.JL_ActualWeight = 3.333;

			var pkgPackage1 = Factory.New<PkgPackage>();
			var pkgPackage2 = Factory.New<PkgPackage>();
			var pkgPackage3 = Factory.New<PkgPackage>();
			pkgPackage1.KP_WeightUQ = Constants.Weight.Kilograms;
			pkgPackage2.KP_WeightUQ = Constants.Weight.Kilograms;
			pkgPackage3.KP_WeightUQ = Constants.Weight.Kilograms;
			pkgPackage1.KP_Weight = 1.111;
			pkgPackage2.KP_Weight = 2.222;
			pkgPackage3.KP_Weight = 3.333;

			CombineAssertions("JL_Calc_PacklineToPkgPackage_WeightDiscrepancy Calculation", () =>
			{
				AssertEquals("Discrepancy calculated when PkgPackageCollection is empty", 3.333m, packLine.JL_Calc_PacklineToPkgPackage_WeightDiscrepancy);

				packLine.PkgPackageCollection.Add(pkgPackage1);
				AssertEquals("Positive discrepancy calculated when packLine has more weight than packages", 2.222m, packLine.JL_Calc_PacklineToPkgPackage_WeightDiscrepancy);

				packLine.PkgPackageCollection.Add(pkgPackage2);
				AssertEquals("No discrepancy calculated when packLine weight equals packages weight", 0m, packLine.JL_Calc_PacklineToPkgPackage_WeightDiscrepancy);

				packLine.PkgPackageCollection.Add(pkgPackage3);
				AssertEquals("Negative discrepancy calculated when packLine has less weight than packages", -3.333m, packLine.JL_Calc_PacklineToPkgPackage_WeightDiscrepancy);
			});
		}

		public void TestJL_Calc_PacklineToPkgPackage_VolumeDiscrepancy()
		{
			var packLine = Factory.New<PackLine>();
			packLine.JL_ActualVolumeUQ = Constants.Volume.Litre;
			packLine.JL_ActualVolume = 3.333;

			var pkgPackage1 = Factory.New<PkgPackage>();
			var pkgPackage2 = Factory.New<PkgPackage>();
			var pkgPackage3 = Factory.New<PkgPackage>();
			pkgPackage1.KP_VolumeUQ = Constants.Volume.Litre;
			pkgPackage2.KP_VolumeUQ = Constants.Volume.Litre;
			pkgPackage3.KP_VolumeUQ = Constants.Volume.Litre;
			pkgPackage1.KP_Volume = 1.111;
			pkgPackage2.KP_Volume = 2.222;
			pkgPackage3.KP_Volume = 3.333;

			CombineAssertions("JL_Calc_PacklineToPkgPackage_VolumeDiscrepancy Calculation", () =>
			{
				AssertEquals("Discrepancy calculated when PkgPackageCollection is empty", 3.333m, packLine.JL_Calc_PacklineToPkgPackage_VolumeDiscrepancy);

				packLine.PkgPackageCollection.Add(pkgPackage1);
				AssertEquals("Positive discrepancy calculated when packLine has more volume than packages", 2.222m, packLine.JL_Calc_PacklineToPkgPackage_VolumeDiscrepancy);

				packLine.PkgPackageCollection.Add(pkgPackage2);
				AssertEquals("No discrepancy calculated when packLine volume equals packages Volume", 0m, packLine.JL_Calc_PacklineToPkgPackage_VolumeDiscrepancy);

				packLine.PkgPackageCollection.Add(pkgPackage3);
				AssertEquals("Negative discrepancy calculated when packLine has less volume than packages", -3.333m, packLine.JL_Calc_PacklineToPkgPackage_VolumeDiscrepancy);
			});
		}

		#endregion

		#region TestJL_Calc_WeightToDeliver

		public void TestJL_Calc_WeightToDeliver()
		{
			CommonConsol consol = GetImportConsol(typeof(CommonConsol));
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport = consol.Transports[0];
			transport.JW_ETA = ZDateTime.Today;
			transport.JW_Vessel = TestVessel1.RV_FK;
			transport.JW_VoyageFlight = "855";

			CommonContainer container = consol.Containers.AddNew();
			PackLine line = consol.Shipments.AddNew().OuterPackLines.AddNew();
			line.SetContainer(consol, container);
			container.JC_LCLUnpack = ZDateTime.Today;
			line.JL_PackageCount = 10;
			line.JL_ActualWeight = 1000m;
			AssertEquals("Weight to Deliver should be 1000", 1000m, line.JL_Calc_WeightToDeliver);
			line.JL_Outturn = 8;
			AssertEquals("Weight to Deliver should be 1000", 1000m, line.JL_Calc_WeightToDeliver);
			line.JL_OutturnedWeight = 800m;
			AssertEquals("Weight to Deliver should be 800", 800m, line.JL_Calc_WeightToDeliver);
		}

		#endregion

		#region TestJL_Calc_VolumeToDeliver

		public void TestJL_Calc_VolumeToDeliver()
		{
			CommonConsol consol = GetImportConsol(typeof(CommonConsol));
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Transport transport = consol.Transports[0];
			transport.JW_ETA = ZDateTime.Today;
			transport.JW_Vessel = TestVessel1.RV_FK;
			transport.JW_VoyageFlight = "855";

			CommonContainer container = consol.Containers.AddNew();
			PackLine line = consol.Shipments.AddNew().OuterPackLines.AddNew();
			line.SetContainer(consol, container);
			container.JC_LCLUnpack = ZDateTime.Today;
			line.JL_PackageCount = 10;
			line.JL_ActualVolume = 10m;
			AssertEquals("Volume to Deliver should be 10", 10m, line.JL_Calc_VolumeToDeliver);
			line.JL_Outturn = 8;
			AssertEquals("Volume to Deliver should be 10", 10m, line.JL_Calc_VolumeToDeliver);
			line.JL_OutturnedVolume = 8m;
			AssertEquals("Volume to Deliver should be 8", 8m, line.JL_Calc_VolumeToDeliver);
		}

		#endregion

		#region TestUpdateTotalsFromPkgPackageCollection

		RefContainer ContainerRef => fContainerRef ?? (fContainerRef = GetRefContainer("20PP", 16m));

		RefContainer fContainerRef;

		RefContainer ContainerRef2 => fContainerRef2 ?? (fContainerRef2 = GetRefContainer("21PP", 15m));

		RefContainer fContainerRef2;

		RefContainer GetRefContainer(ZString code, ZDecimal cubicCapacity)
		{
			var containerRef = Factory.New<RefContainer>();
			containerRef.RC_Code = code;
			containerRef.RC_CubicCapacity = cubicCapacity;
			containerRef.RC_GrossWeight = 10000m;
			containerRef.RC_Height = 3m;
			containerRef.RC_Length = 6m;
			containerRef.RC_Width = 2m;
			containerRef.RC_TareWeight = 1000m;
			containerRef.RC_TEU = 1;
			return containerRef;
		}

		public void TestUpdateDetailFromPackagePackageCollection_Discrepencies()
		{
			var packType = Factory.New<RefPackType>();
			packType.F3_Code = "TST";
			packType.F3_Description = "Test";
			packType.F3_Length = 100;
			packType.F3_Height = 200;
			packType.F3_Width = 300;
			packType.F3_UnitOfDimension = "KM";
			packType.F3_Weight = 400;
			packType.F3_UnitOfWeight = "T";

			var packageTemplates = new[]
			{
			new PackageTemplate
			{
				PackageID = "PKG1",
				PackType = "TST",
				GoodsDescription = "Test",
				Qty = 1,
				Weight = 100,
				WeightUQ = Core.Constants.Weight.Kilograms,
				Volume = 1,
				VolumeUQ = Core.Constants.Volume.CubicMetres,
				Length = 1,
				Width = 1,
				Height = 1,
				UnitOfDimension = "M",
				IsDamaged = true
			},
			new PackageTemplate
			{
				PackageID = "PKG2",
				PackType = "TST",
				GoodsDescription = "Test",
				Qty = 1,
				Weight = 100,
				WeightUQ = Core.Constants.Weight.Kilograms,
				Volume = 1,
				VolumeUQ = Core.Constants.Volume.CubicMetres,
				Length = 1,
				Width = 1,
				Height = 1,
				UnitOfDimension = "M"
			},
			new PackageTemplate
			{
				PackageID = "PKG3",
				PackType = "TST",
				GoodsDescription = "Test",
				Qty = 1,
				Weight = 100,
				WeightUQ = Core.Constants.Weight.Kilograms,
				Volume = 1,
				VolumeUQ = Core.Constants.Volume.CubicMetres,
				Length = 1,
				Width = 1,
				Height = 1,
				UnitOfDimension = "M"
			},
			new PackageTemplate
			{
				PackageID = "PKG4",
				PackType = Core.Constants.PkgUnit.Box,
				GoodsDescription = "stationery",
				Qty = 1,
				IsDamaged = true,
				Weight = 50,
				WeightUQ = Core.Constants.Weight.Kilograms,
				Volume = 2,
				VolumeUQ = Core.Constants.Volume.CubicMetres,
				Length = 1,
				Width = 2,
				Height = 3,
				UnitOfDimension = "M"
			},
			new PackageTemplate
			{
				PackageID = "PKG5",
				PackType = Core.Constants.PkgUnit.Box,
				GoodsDescription = "stationery",
				Qty = 1,
				Weight = 50,
				WeightUQ = Core.Constants.Weight.Kilograms,
				Volume = 2,
				VolumeUQ = Core.Constants.Volume.CubicMetres,
				Length = 1,
				Width = 2,
				Height = 3,
				UnitOfDimension = "M"
			}
			};

			var shipment = CreateShipmentPacklineWithPackages(
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies,
				packageTemplates);

			AssertEquals("prerequisite: shipment has 1 packline", 1, shipment.OuterPackLines.Count);

			var packingLine = shipment.OuterPackLines.Cast<PackLine>().Single();

			AssertEquals("prerequisite: 5 packages were created", 5, packingLine.PkgPackageCollection.Count);

			var consol1 = shipment.Consols.AddNew();
			var container1 = consol1.Containers.AddNew();
			var container2 = consol1.Containers.AddNew();

			container1.JC_RC = ContainerRef.PK;
			container2.JC_RC = ContainerRef2.PK;
			Assert("pre: CanAllocateShipmentsToContainers", consol1.CheckCanAllocateShipmentsToContainers());

			packingLine.JL_RefNumber = "REF001";
			packingLine.JL_HarmonisedCode = "";
			packingLine.JL_RN_NKOrigin = "AU";
			packingLine.JL_Damaged = 2;
			packingLine.JL_DetailedDescription = "detailed";
			packingLine.JL_EndItemNo = 5;
			packingLine.JL_ExportRefNumber = "export";
			packingLine.JL_ImportRefNumber = "import";
			packingLine.JL_ItemNo = 88;
			packingLine.JL_Pillaged = 38;
			packingLine.JL_LinePrice = 77;
			packingLine.JL_CustomAttrib1 = "aaa";
			packingLine.JL_CustomAttrib2 = "bbb";
			packingLine.JL_CustomDate1 = new ZDate(2011, 5, 5);
			packingLine.JL_CustomDate2 = new ZDate(2009, 5, 5);
			packingLine.JL_LastKnownTransitWarehouseStatus = "RCV";
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			packingLine.JL_OA_LastKnownTransitWarehouseAddress = orgAddress.PK;
			packingLine.JL_LastKnownTransitWarehouseStatusDateTime = new ZDate(2021, 5, 5);
			packingLine.SetContainer(consol1, container2);

			var undg1 = Factory.NewWithValidTestData<UNDGDataItem>();
			undg1.DI_HasOverpack = true;
			undg1.DI_OverpackID = "overpackID";
			foreach (var package in packingLine.PkgPackageCollection)
			{
				package.UNDGs.Add(undg1);
			}

			Factory.Save();

			AssertEquals("pre1: container is correct", packingLine.JL_JC, container2.PK);

			packingLine.UpdateTotalsFromPkgPackageCollection();

			AssertEquals("pre2: container is still correct", packingLine.JL_JC, container2.PK);

			AssertEquals("shipment has 2 packlines", 2, shipment.OuterPackLines.Count);

			var descriptions = string.Join(System.Environment.NewLine, shipment.OuterPackLines.Cast<PackLine>().Select(GetPackLineDetailWithPackagesDescription));

			AssertMultilineASCIIEquals("packinglines",
@"packline Goods Des: Test Qty: 3 Weight: 300 KG Volume: 3 M3 Length: 1 M Width: 1 M Height: 1 M
   package Goods Des: Test Qty: 1 Weight 100 KG Volume: 1 M3 Length: 1 M Width: 1 M Height: 1 M
   package Goods Des: Test Qty: 1 Weight 100 KG Volume: 1 M3 Length: 1 M Width: 1 M Height: 1 M
   package Goods Des: Test Qty: 1 Weight 100 KG Volume: 1 M3 Length: 1 M Width: 1 M Height: 1 M
packline Goods Des: stationery Qty: 2 Weight: 100 KG Volume: 12 M3 Length: 1 M Width: 2 M Height: 3 M
   package Goods Des: stationery Qty: 1 Weight 50 KG Volume: 6 M3 Length: 1 M Width: 2 M Height: 3 M
   package Goods Des: stationery Qty: 1 Weight 50 KG Volume: 6 M3 Length: 1 M Width: 2 M Height: 3 M", descriptions);

			var packLine2 = shipment.OuterPackLines[1];
			CombineAssertions("new packline has copied attributes", () =>
			{
				AssertEquals("REF001", packLine2.JL_RefNumber);
				AssertEquals("", packLine2.JL_HarmonisedCode);
				AssertEquals("AU", packLine2.JL_RN_NKOrigin);
				AssertEquals(1, packLine2.JL_Damaged);
				AssertEquals("detailed", packLine2.JL_DetailedDescription);
				AssertEquals(5, (int)packLine2.JL_EndItemNo);
				AssertEquals("export", packLine2.JL_ExportRefNumber);
				AssertEquals("import", packLine2.JL_ImportRefNumber);
				AssertEquals(88, (int)packLine2.JL_ItemNo);
				AssertEquals(38, packLine2.JL_Pillaged);
				AssertEquals(77, (int)packLine2.JL_LinePrice);
				AssertEquals("aaa", packLine2.JL_CustomAttrib1);
				AssertEquals("bbb", packLine2.JL_CustomAttrib2);
				AssertEquals(new ZDate(2011, 5, 5), packLine2.JL_CustomDate1);
				AssertEquals(new ZDate(2009, 5, 5), packLine2.JL_CustomDate2);
				AssertEquals("RCV", packLine2.JL_LastKnownTransitWarehouseStatus);
				AssertEquals(orgAddress.PK, packLine2.JL_OA_LastKnownTransitWarehouseAddress);
				AssertEquals(new ZDate(2021, 5, 5), packLine2.JL_LastKnownTransitWarehouseStatusDateTime);
				AssertEquals("container is correct", container2.PK, packLine2.JL_JC);

				AssertEquals("has 1 undg", 1, packLine2.UNDGs.Count);
				var copiedUndg1 = packLine2.UNDGs[0];
				AssertEquals("overpackID", copiedUndg1.DI_OverpackID);
				AssertEquals(true, copiedUndg1.DI_HasOverpack);
			});
			CombineAssertions("split packline excluded clone attributes", () =>
			{
				AssertEquals("packline id is empty", "", packLine2.JL_PackLineId);
				AssertEquals("status is Confirmed", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed, packLine2.JL_OriginTransitWarehouseStatus);
			});

			AssertNoExceptionThrown(Factory.Save);
		}

		string GetPackLineDetailWithPackagesDescription(PackLine packLine)
		{
			var packlineDesc = $"packline Goods Des: {packLine.JL_Description} Qty: {packLine.JL_PackageCount} Weight: {packLine.JL_ActualWeight} {packLine.JL_ActualWeightUQ} Volume: {packLine.JL_ActualVolume} {packLine.JL_ActualVolumeUQ} Length: {packLine.JL_Length} {packLine.JL_UnitOfDimension} Width: {packLine.JL_Width} {packLine.JL_UnitOfDimension} Height: {packLine.JL_Height} {packLine.JL_UnitOfDimension}";
			string GerPackageDetailDescription(PkgPackage package) => $"   package Goods Des: {package.KP_GoodsDescription} Qty: {package.KP_PackageQty} Weight {package.KP_Weight} {package.KP_WeightUQ} Volume: {package.KP_Volume} {package.KP_VolumeUQ} Length: {package.KP_Length} {package.KP_DimensionUQ} Width: {package.KP_Width} {package.KP_DimensionUQ} Height: {package.KP_Height} {package.KP_DimensionUQ}";
			var packagesDesc = packLine.PkgPackageCollection.Select(GerPackageDetailDescription);

			return string.Concat(packlineDesc, System.Environment.NewLine, string.Join(System.Environment.NewLine, packagesDesc));
		}

		public void TestUpdateTotalsFromPkgPackageCollection_Discrepencies()
		{
			var packageTemplates = new[]
			{
				new PackageTemplate
				{
					PackageID = "PKG1",
					PackType = Core.Constants.PkgUnit.Carton,
					GoodsDescription = "books",
					Qty = 1,
					Weight = 100,
					WeightUQ = Core.Constants.Weight.Kilograms,
					Volume = 1,
					VolumeUQ = Core.Constants.Volume.CubicMetres
				},
				new PackageTemplate
				{
					PackageID = "PKG2",
					PackType = Core.Constants.PkgUnit.Carton,
					GoodsDescription = "books",
					Qty = 1,
					Weight = 100,
					WeightUQ = Core.Constants.Weight.Kilograms,
					Volume = 1,
					VolumeUQ = Core.Constants.Volume.CubicMetres
				},
				new PackageTemplate
				{
					PackageID = "PKG3",
					PackType = Core.Constants.PkgUnit.Carton,
					GoodsDescription = "books",
					Qty = 1,
					Weight = 100,
					WeightUQ = Core.Constants.Weight.Kilograms,
					Volume = 1,
					VolumeUQ = Core.Constants.Volume.CubicMetres
				},
				new PackageTemplate
				{
					PackageID = "PKG4",
					PackType = Core.Constants.PkgUnit.Box,
					GoodsDescription = "stationery",
					Qty = 1,
					Weight = 50,
					WeightUQ = Core.Constants.Weight.Kilograms,
					Volume = 2,
					VolumeUQ = Core.Constants.Volume.CubicMetres,
				},
				new PackageTemplate
				{
					PackageID = "PKG5",
					PackType = Core.Constants.PkgUnit.Box,
					GoodsDescription = "stationery",
					Qty = 1,
					Weight = 50,
					WeightUQ = Core.Constants.Weight.Kilograms,
					Volume = 2,
					VolumeUQ = Core.Constants.Volume.CubicMetres
				}
			};

			var shipment = CreateShipmentPacklineWithPackages(
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies,
				packageTemplates);

			AssertEquals("prerequisite: shipment has 1 packline", 1, shipment.OuterPackLines.Count);

			var packingLine = shipment.OuterPackLines.Cast<PackLine>().Single();

			AssertEquals("prerequisite: 5 packages were created", 5, packingLine.PkgPackageCollection.Count);

			packingLine.UpdateTotalsFromPkgPackageCollection();

			AssertEquals("shipment has 2 packlines", 2, shipment.OuterPackLines.Count);

			var descriptions = string.Join(System.Environment.NewLine, shipment.OuterPackLines.Cast<PackLine>().Select(GetPackLineWithPackagesDescription));

			AssertMultilineASCIIEquals("packinglines",
@"packline Goods Des: books Qty: 3 Weight: 300 KG Volume: 3 M3
   package Goods Des: books Qty: 1 Weight 100 KG Volume: 1 M3
   package Goods Des: books Qty: 1 Weight 100 KG Volume: 1 M3
   package Goods Des: books Qty: 1 Weight 100 KG Volume: 1 M3
packline Goods Des: stationery Qty: 2 Weight: 100 KG Volume: 4 M3
   package Goods Des: stationery Qty: 1 Weight 50 KG Volume: 2 M3
   package Goods Des: stationery Qty: 1 Weight 50 KG Volume: 2 M3", descriptions);

			AssertNoExceptionThrown("no exception was throwns during save", Factory.Save);
		}

		public void TestUpdateTotalsFromPkgPackageCollection_Discrepencies_UpdatePacklineAndConfirm()
		{
			var (shipment, packingLine) = GetShipmentWithOnePackage();
			packingLine.UpdateTotalsFromPkgPackageCollection(PackLineConfirmDiscrepancyAction.UpdatePacklineAndConfirm);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);
			query.AddToFilter(StmALogSchema.SL_Reference, $"Packline ID: {packingLine.JL_PackLineId}, packline updated");

			CombineAssertions("packline was confirmed by accepting discrepancies", () =>
			{
				AssertEquals("shipment has 1 packline", 1, shipment.OuterPackLines.Count);
				AssertEquals("packline weight is updated", 100.0m, packingLine.JL_ActualWeight);
				Assert("contains log", shipment.Logs.HasLogWith(query));
			});

			AssertNoExceptionThrown("no exception was throwns during save", Factory.Save);
		}

		public void TestUpdateTotalsFromPkgPackageCollection_Discrepencies_AcceptDiscrepancyAndConfirm()
		{
			var (shipment, packingLine) = GetShipmentWithOnePackage();
			packingLine.UpdateTotalsFromPkgPackageCollection(PackLineConfirmDiscrepancyAction.AcceptDiscrepancyAndConfirm);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);
			query.AddToFilter(StmALogSchema.SL_Reference, $"Packline ID: {packingLine.JL_PackLineId}, discrepancy accepted");

			CombineAssertions("packline was confirmed by accepting discrepancies", () =>
			{
				AssertEquals("shipment has 1 packline", 1, shipment.OuterPackLines.Count);
				AssertEquals("packline weight is unchanged", 5.0m, packingLine.JL_ActualWeight);
				Assert("contains log", shipment.Logs.HasLogWith(query));
			});

			AssertNoExceptionThrown("no exception was throwns during save", Factory.Save);
		}

		public void TestUpdateTotalsFromPkgPackageCollection_Discrepencies_NoAction()
		{
			var (shipment, packingLine) = GetShipmentWithOnePackage();
			packingLine.UpdateTotalsFromPkgPackageCollection(PackLineConfirmDiscrepancyAction.NoAction);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code);
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, $"Packline ID: {packingLine.JL_PackLineId}");

			CombineAssertions("packline was confirmed by accepting discrepancies", () =>
			{
				AssertEquals("shipment has 1 packline", 1, shipment.OuterPackLines.Count);
				AssertEquals("packline weight is unchanged", 5.0m, packingLine.JL_ActualWeight);
				Assert("no log", !shipment.Logs.HasLogWith(query));
			});

			AssertNoExceptionThrown("no exception was throwns during save", Factory.Save);
		}

		public (CommonShipment shipment, PackLine packingLine) GetShipmentWithOnePackage()
		{
			var packageTemplates = new[]
			{
				new PackageTemplate
				{
					PackType = Core.Constants.PkgUnit.Carton,
					GoodsDescription = "books",
					Qty = 1,
					Weight = 100,
					WeightUQ = Core.Constants.Weight.Kilograms,
					Volume = 1,
					VolumeUQ = Core.Constants.Volume.CubicMetres
				}
			};

			var shipment = CreateShipmentPacklineWithPackages(
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies,
				packageTemplates);

			AssertEquals("prerequisite: shipment has 1 packline", 1, shipment.OuterPackLines.Count);

			var packingLine = shipment.OuterPackLines.Cast<PackLine>().Single();

			AssertEquals("prerequisite: 1 package was created", 1, packingLine.PkgPackageCollection.Count);
			packingLine.JL_ActualWeight = 5.0;

			return (shipment, packingLine);
		}

		public void TestUpdateTotalsFromPkgPackageCollectionForUNDG_Discrepencies()
		{
			var packageTemplates = new[]
						{
				new PackageTemplate
				{
					PackageID = "PKG1",
					PackType = Constants.PkgUnit.Carton,
					GoodsDescription = "books",
					Qty = 1,
					Weight = 100,
					WeightUQ = Constants.Weight.Kilograms,
					Volume = 1,
					VolumeUQ = Constants.Volume.CubicMetres
				},
				new PackageTemplate
				{
					PackageID = "PKG2",
					PackType = Constants.PkgUnit.Carton,
					GoodsDescription = "books",
					Qty = 1,
					Weight = 100,
					WeightUQ = Constants.Weight.Kilograms,
					Volume = 1,
					VolumeUQ = Constants.Volume.CubicMetres,
				}
			};

			var shipment = CreateShipmentPacklineWithPackages(
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies,
				packageTemplates);

			var substance = Factory.New<UNDGSubstance>();
			substance.DG_UNNO = "6666";
			substance.DG_Code = "6666";
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			var pkgUNDG1 = Factory.NewWithValidTestData<UNDGDataItem>();
			pkgUNDG1.DI_DG = substance.PK;
			pkgUNDG1.LinkDefault(substance);
			pkgUNDG1.DI_TechnicalName = "WHATEVER";
			pkgUNDG1.DI_IMOClass = "CLAS";
			pkgUNDG1.DI_IsCombustible = true;
			pkgUNDG1.DI_DGFlashPoint = 15.0m;
			pkgUNDG1.DI_MPMarinePollutant = "N";
			pkgUNDG1.DI_DGVolume = 2m;
			pkgUNDG1.DI_UnitOfVolume = "M3";
			pkgUNDG1.DI_DGWeight = 200m;
			pkgUNDG1.DI_UnitOfWeight = "KG";
			pkgUNDG1.DI_IsLimitedQuantity = true;
			pkgUNDG1.DI_PackageCount = 5;
			pkgUNDG1.DI_F3_NKPackType = "BAG";
			pkgUNDG1.DI_OverpackID = "A1";
			pkgUNDG1.DI_HasOverpack = true;

			shipment.OuterPackLines[0].PkgPackageCollection[0].UNDGs.Add(pkgUNDG1);

			AssertEquals("prerequisite: shipment has 1 packline", 1, shipment.OuterPackLines.Count);

			var packingLine = shipment.OuterPackLines.Cast<PackLine>().Single();

			AssertEquals("prerequisite: 2 packages were created", 2, packingLine.PkgPackageCollection.Count);

			packingLine.UpdateTotalsFromPkgPackageCollection();

			AssertEquals("shipment has 1 packlines", 1, shipment.OuterPackLines.Count);

			var descriptions = string.Join(System.Environment.NewLine, shipment.OuterPackLines.Cast<PackLine>().Select(GetPackLineWithPackagesDescription));

			AssertMultilineASCIIEquals("packinglines",
@"packline Goods Des: books Qty: 2 Weight: 200 KG Volume: 2 M3
   package Goods Des: books Qty: 1 Weight 100 KG Volume: 1 M3
   package Goods Des: books Qty: 1 Weight 100 KG Volume: 1 M3", descriptions);

			AssertNoExceptionThrown("no exception was throwns during save", Factory.Save);
		}

		string GetPackLineWithPackagesDescription(PackLine packLine)
		{
			var packlineDesc = $"packline Goods Des: {packLine.JL_Description} Qty: {packLine.JL_PackageCount} Weight: {packLine.JL_ActualWeight} {packLine.JL_ActualWeightUQ} Volume: {packLine.JL_ActualVolume} {packLine.JL_ActualVolumeUQ}";
			var packagesDesc = packLine.PkgPackageCollection.Select(GerPackageDescription);

			return string.Concat(packlineDesc, System.Environment.NewLine, string.Join(System.Environment.NewLine, packagesDesc));
		}

		string GerPackageDescription(PkgPackage package) => $"   package Goods Des: {package.KP_GoodsDescription} Qty: {package.KP_PackageQty} Weight {package.KP_Weight} {package.KP_WeightUQ} Volume: {package.KP_Volume} {package.KP_VolumeUQ}";

		public void TestUpdateTotalsFromPkgPackageCollection_ShortShipped()
		{
			var packageTemplates = new[]
			{
				new PackageTemplate
				{
					PackType = Core.Constants.PkgUnit.Box,
					GoodsDescription = "books",
					Qty = 2,
					Weight = 125,
					WeightUQ = Core.Constants.Weight.Kilograms,
					Volume = 2,
					VolumeUQ = Core.Constants.Volume.CubicMetres
				}
			};

			var shipment = CreateShipmentPacklineWithPackages(
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.ShortShipped,
				packageTemplates);

			AssertEquals("prerequisite: shipment has 1 packline", 1, shipment.OuterPackLines.Count);

			var packingLine = shipment.OuterPackLines.Cast<PackLine>().Single();

			AssertEquals("prerequisite: 1 package was created", 1, packingLine.PkgPackageCollection.Count);

			packingLine.UpdateTotalsFromPkgPackageCollection();

			Assert("packingline was deleted", packingLine.IsDeleted);
			AssertNoExceptionThrown("no exception was throwns during save", Factory.Save);
		}

		public void TestUpdateTotalsFromPkgPackageCollection_ShortShipped_ForNoPackage()
		{
			var packageTemplates = new[]
			{
				new PackageTemplate
				{
					PackType = Core.Constants.PkgUnit.Box,
					GoodsDescription = "books",
					Qty = 2,
					Weight = 125,
					WeightUQ = Core.Constants.Weight.Kilograms,
					Volume = 2,
					VolumeUQ = Core.Constants.Volume.CubicMetres
				}
			};

			var shipment = CreateShipmentPacklineWithPackages(
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.ShortShipped,
				packageTemplates);

			var packingLine = shipment.OuterPackLines.Cast<PackLine>().Single();

			var deletingPakcageArray = packingLine.PkgPackageCollection.ToArray();
			packingLine.PkgPackageCollection.RemoveAllFromRelationship();
			deletingPakcageArray.ForEach(p => p.Delete());
			Factory.Save();

			packingLine.UpdateTotalsFromPkgPackageCollection();

			Assert("packingline was deleted", packingLine.IsDeleted);
			AssertNoExceptionThrown("no exception was throwns during save", Factory.Save);
		}

		public void TestUpdateTotalsFromPkgPackageCollection_Surplus()
		{
			var packageTemplates = new[]
			{
				new PackageTemplate
				{
					PackType = Core.Constants.PkgUnit.Box,
					GoodsDescription = "books",
					Qty = 2,
					Weight = 125,
					WeightUQ = Core.Constants.Weight.Kilograms,
					Volume = 2,
					VolumeUQ = Core.Constants.Volume.CubicMetres
				},
				new PackageTemplate
				{
					PackType = Core.Constants.PkgUnit.Box,
					GoodsDescription = "books",
					Qty = 3,
					IsDamaged = true,
					Weight = 75,
					WeightUQ = Core.Constants.Weight.Kilograms,
					Volume = 1,
					VolumeUQ = Core.Constants.Volume.CubicMetres
				}
			};

			var shipment = CreateShipmentPacklineWithPackages(
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Surplus,
				packageTemplates);

			AssertEquals("prerequisite: shipment has 1 packline", 1, shipment.OuterPackLines.Count);

			var packingLine = shipment.OuterPackLines.Cast<PackLine>().Single();
			AssertEquals("prerequisite: 2 packages were created", 2, packingLine.PkgPackageCollection.Count);

			AssertEquals("prerequisite: JL_PackageCount", 0, packingLine.JL_PackageCount);
			AssertEquals("prerequisite: JL_ActualWeight", 0m, packingLine.JL_ActualWeight);
			AssertEquals("prerequisite: JL_ActualVolume", 0m, packingLine.JL_ActualVolume);
			AssertEquals("prerequisite: JL_Damaged", 0, packingLine.JL_Damaged);

			packingLine.UpdateTotalsFromPkgPackageCollection();

			AssertEquals("JL_PackageCount", 5, packingLine.JL_PackageCount);
			AssertEquals("JL_ActualWeight", 200m, packingLine.JL_ActualWeight);
			AssertEquals("JL_ActualVolume", 3m, packingLine.JL_ActualVolume);
			AssertEquals("JL_Damaged", 3, packingLine.JL_Damaged);

			AssertNoExceptionThrown("no exception was throwns during save", Factory.Save);
		}

		CommonShipment CreateShipmentPacklineWithPackages(ZString transitWarehouseStatus, IEnumerable<PackageTemplate> packageTemplates)
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_OriginTransitWarehouseStatus = transitWarehouseStatus;
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Box;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;

			var packageParent = Factory.New<DummyBusinessObject>();

			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = packageParent.PK;
			packageJob.KJ_ParentTableCode = packageParent.TablePrefix;

			foreach (var packageTemplate in packageTemplates)
			{
				var package = packLine.PkgPackageCollection.AddNew();
				package.KP_KJ_ParentPackageJob = packageJob.PK;
				package.KP_GoodsDescription = packageTemplate.GoodsDescription;
				package.KP_F3_NKPackType = packageTemplate.PackType;
				package.KP_PackageQty = packageTemplate.Qty;
				package.KP_Weight = packageTemplate.Weight;
				package.KP_WeightUQ = packageTemplate.WeightUQ;
				package.KP_Volume = packageTemplate.Volume;
				package.KP_VolumeUQ = packageTemplate.VolumeUQ;
				package.KP_Length = packageTemplate.Length;
				package.KP_Width = packageTemplate.Width;
				package.KP_Height = packageTemplate.Height;
				package.KP_DimensionUQ = packageTemplate.UnitOfDimension;
				package.KP_IsDamaged = packageTemplate.IsDamaged;
				package.KP_PackageID = packageTemplate.PackageID;
			}

			Factory.Save();

			return shipment;
		}

		sealed class PackageTemplate
		{
			public ZString PackType { get; set; }
			public ZString GoodsDescription { get; set; }
			public ZInt Qty { get; set; }
			public ZDecimal Weight { get; set; }
			public ZString WeightUQ { get; set; }
			public ZDecimal Volume { get; set; }
			public ZString VolumeUQ { get; set; }
			public ZDecimal Length { get; set; }
			public ZDecimal Width { get; set; }
			public ZDecimal Height { get; set; }
			public ZString UnitOfDimension { get; set; }
			public ZBool IsDamaged { get; set; } = false;
			public ZString PackageID { get; set; }
		}

		#endregion

		#region TestCopyValuesFromPackage

		public void TestCopyValuesFromPackage()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_OriginTransitWarehouseStatus = "DIS";
			packLine.JL_F3_NKPackType = Core.Constants.PkgUnit.Box;
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			var undg = packLine.UNDGs.AddNew();

			var packageParent = Factory.New<DummyBusinessObject>();

			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = packageParent.PK;
			packageJob.KJ_ParentTableCode = packageParent.TablePrefix;

			var package = packLine.PkgPackageCollection.AddNew();
			package.KP_KJ_ParentPackageJob = packageJob.PK;
			package.KP_F3_NKPackType = Constants.PkgUnit.Container;
			package.KP_DimensionUQ = "CM";
			package.KP_Height = 1m;
			package.KP_Length = 2m;
			package.KP_PackageID = "PACKAGE123";
			package.KP_PackageQty = 3;
			package.KP_VolumeUQ = Constants.Volume.CubicCentimeters;
			package.KP_Weight = 5m;
			package.KP_WeightUQ = "T";
			package.KP_Width = 6m;
			package.KP_MarksAndNumbers = "MARK123";
			package.KP_TransportRef = "TRANSPORT REF";
			package.KP_GoodsDescription = "GOODS DESC";
			package.KP_HSCode = "HARMON CODE";
			package.KP_ExternalReference = "PackLineID";
			package.KP_Volume = 36m;
			package.KP_RH_NKCommodityCode = "XX";
			package.KP_RequiresTemperatureControl = true;
			package.KP_RequiredTemperatureMinimum = -10;
			package.KP_RequiredTemperatureMaximum = -5;
			package.KP_RequiredTemperatureUnit = Constants.Temperature.Fahrenheit;
			package.KP_HSCode = "HARMON CODE310";
			var pkgContact = Factory.NewWithValidTestData<OrgContact>();
			pkgContact.OC_ContactName = "PKG Contact";
			pkgContact.OC_Phone = "010-12345432";

			var substance = Factory.New<UNDGSubstance>();
			substance.DG_UNNO = "6666";
			substance.DG_Variant = "E";
			substance.DG_PSN = "DG SHIPPER NAME";
			substance.DG_PG = "GRO";
			substance.DG_SubLabel1 = "sub1";
			substance.DG_SubLabel2 = "su2";
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			var pkgUNDG1 = Factory.NewWithValidTestData<UNDGDataItem>();
			pkgUNDG1.DI_DG = substance.PK;
			pkgUNDG1.LinkDefault(substance);
			pkgUNDG1.DI_TechnicalName = "WHATEVER";
			pkgUNDG1.DI_IMOClass = "CLAS";
			pkgUNDG1.DI_IsCombustible = true;
			pkgUNDG1.DI_DGFlashPoint = 15.0m;
			pkgUNDG1.DI_MPMarinePollutant = "N";
			pkgUNDG1.DI_DGVolume = 2m;
			pkgUNDG1.DI_UnitOfVolume = "M3";
			pkgUNDG1.DI_DGWeight = 200m;
			pkgUNDG1.DI_UnitOfWeight = "KG";
			pkgUNDG1.DI_IsLimitedQuantity = true;
			pkgUNDG1.DI_PackageCount = 5;
			pkgUNDG1.DI_F3_NKPackType = "BAG";
			pkgUNDG1.DI_OverpackID = "overpack1";
			pkgUNDG1.DI_HasOverpack = true;
			pkgUNDG1.DI_OC_DGContact = pkgContact.PK;
			package.UNDGs.Add(pkgUNDG1);

			var pkgUNDG2 = Factory.NewWithValidTestData<UNDGDataItem>();
			pkgUNDG2.DI_DG = substance.PK;
			pkgUNDG2.LinkDefault(substance);
			pkgUNDG2.DI_TechnicalName = "WHATEVER";
			pkgUNDG2.DI_IMOClass = "CLAS";
			pkgUNDG2.DI_IsCombustible = true;
			pkgUNDG2.DI_DGFlashPoint = 15.0m;
			pkgUNDG2.DI_MPMarinePollutant = "N";
			pkgUNDG2.DI_DGVolume = 3m;
			pkgUNDG2.DI_UnitOfVolume = "M3";
			pkgUNDG2.DI_DGWeight = 201m;
			pkgUNDG2.DI_UnitOfWeight = "KG";
			pkgUNDG2.DI_IsLimitedQuantity = true;
			pkgUNDG2.DI_PackageCount = 2;
			pkgUNDG2.DI_F3_NKPackType = "BAG";
			pkgUNDG2.DI_OverpackID = "overpack1";
			pkgUNDG2.DI_HasOverpack = true;
			pkgUNDG2.DI_OC_DGContact = pkgContact.PK;
			package.UNDGs.Add(pkgUNDG2);

			var pkgUNDG3 = Factory.NewWithValidTestData<UNDGDataItem>();
			pkgUNDG3.DI_DG = substance.PK;
			pkgUNDG3.LinkDefault(substance);
			pkgUNDG3.DI_TechnicalName = "WHATEVER OTHER";
			pkgUNDG3.DI_IMOClass = "CLAS";
			pkgUNDG3.DI_IsCombustible = true;
			pkgUNDG3.DI_DGFlashPoint = 15.0m;
			pkgUNDG3.DI_MPMarinePollutant = "N";
			pkgUNDG3.DI_DGVolume = 7m;
			pkgUNDG3.DI_UnitOfVolume = "M3";
			pkgUNDG3.DI_DGWeight = 501m;
			pkgUNDG3.DI_UnitOfWeight = "KG";
			pkgUNDG3.DI_IsLimitedQuantity = true;
			pkgUNDG3.DI_PackageCount = 9;
			pkgUNDG3.DI_F3_NKPackType = "BAG";
			pkgUNDG3.DI_OverpackID = "overpack2";
			pkgUNDG3.DI_HasOverpack = false;
			pkgUNDG3.DI_OC_DGContact = pkgContact.PK;
			package.UNDGs.Add(pkgUNDG3);

			AssertEquals("BOX", packLine.JL_F3_NKPackType);
			AssertEquals("", packLine.JL_Description);
			AssertEquals("M3", packLine.JL_ActualVolumeUQ);
			AssertEquals("KG", packLine.JL_ActualWeightUQ);
			AssertEquals("M", packLine.JL_UnitOfDimension);
			AssertEquals(0m, packLine.JL_Length);
			AssertEquals(0m, packLine.JL_Height);
			AssertEquals(0m, packLine.JL_Width);
			AssertEquals(0m, packLine.JL_ActualWeight);
			AssertEquals(0m, packLine.JL_ActualVolume);
			AssertEquals("", packLine.JL_MarksAndNumbers);
			AssertEquals(0m, packLine.JL_RequiredTemperatureMinimum);
			AssertEquals(0m, packLine.JL_RequiredTemperatureMaximum);
			AssertEquals("C", packLine.JL_RequiredTemperatureUnit);
			AssertEquals(false, packLine.JL_RequiresTemperatureControl);
			AssertEquals("", packLine.JL_HarmonisedCode);
			AssertEquals("GEN", packLine.JL_RH_NKCommodityCode);
			AssertEquals(ZString.Empty, packLine.JL_InspectionTypeCode);

			undg = packLine.UNDGs.First();
			AssertEquals(false, undg.DI_IsCombustible);
			AssertEquals(0m, undg.DI_DGFlashPoint);
			AssertEquals("", undg.DI_F3_NKPackType);
			AssertEquals("", undg.DI_IMOClass);
			AssertEquals(false, undg.DI_IsLimitedQuantity);
			AssertEquals("", undg.DI_MPMarinePollutant);
			AssertEquals(ZGuid.Empty, undg.DI_OC_DGContact);
			AssertEquals("", undg.DI_OverpackID);
			AssertEquals(false, undg.DI_HasOverpack);
			AssertEquals("", undg.DI_TechnicalName);
			AssertEquals(0m, undg.DI_DGVolume);
			AssertEquals(0m, undg.DI_DGWeight);
			AssertEquals("", undg.DI_UnitOfVolume);
			AssertEquals("", undg.DI_UnitOfWeight);

			packLine.UpdateTotalsFromPkgPackageCollection();

			AssertEquals("CNT", packLine.JL_F3_NKPackType);
			AssertEquals("GOODS DESC", packLine.JL_Description);
			AssertEquals("CC", packLine.JL_ActualVolumeUQ);
			AssertEquals("T", packLine.JL_ActualWeightUQ);
			AssertEquals("CM", packLine.JL_UnitOfDimension);
			AssertEquals(2m, packLine.JL_Length);
			AssertEquals(1m, packLine.JL_Height);
			AssertEquals(6m, packLine.JL_Width);
			AssertEquals(5m, packLine.JL_ActualWeight);
			AssertEquals(36m, packLine.JL_ActualVolume);
			AssertEquals("MARK123", packLine.JL_MarksAndNumbers);
			AssertEquals(-10m, packLine.JL_RequiredTemperatureMinimum);
			AssertEquals(-5m, packLine.JL_RequiredTemperatureMaximum);
			AssertEquals("F", packLine.JL_RequiredTemperatureUnit);
			AssertEquals(true, packLine.JL_RequiresTemperatureControl);
			AssertEquals("HARMON CODE310", packLine.JL_HarmonisedCode);
			AssertEquals("XX", packLine.JL_RH_NKCommodityCode);

			AssertEquals("there are two undgs - UNDG 'WHATEVER' is merged to one item", 2, packLine.UNDGs.Count);

			undg = packLine.UNDGs.First(x => x.DI_TechnicalName == "WHATEVER");
			AssertEquals(true, undg.DI_IsCombustible);
			AssertEquals(15.0m, undg.DI_DGFlashPoint);
			AssertEquals("BAG", undg.DI_F3_NKPackType);
			AssertEquals("CLAS", undg.DI_IMOClass);
			AssertEquals(true, undg.DI_IsLimitedQuantity);
			AssertEquals("N", undg.DI_MPMarinePollutant);
			AssertEquals(pkgContact.PK, undg.DI_OC_DGContact);
			AssertEquals(true, undg.DI_HasOverpack);
			AssertEquals("overpack1", undg.DI_OverpackID);
			AssertEquals("WHATEVER", undg.DI_TechnicalName);
			AssertEquals(5m, undg.DI_DGVolume);
			AssertEquals(401m, undg.DI_DGWeight);
			AssertEquals("M3", undg.DI_UnitOfVolume);
			AssertEquals("KG", undg.DI_UnitOfWeight);
			AssertEquals(7, undg.DI_PackageCount);

			undg = packLine.UNDGs.First(x => x.DI_TechnicalName == "WHATEVER OTHER");
			AssertEquals(true, undg.DI_IsCombustible);
			AssertEquals(15.0m, undg.DI_DGFlashPoint);
			AssertEquals("BAG", undg.DI_F3_NKPackType);
			AssertEquals("CLAS", undg.DI_IMOClass);
			AssertEquals(true, undg.DI_IsLimitedQuantity);
			AssertEquals("N", undg.DI_MPMarinePollutant);
			AssertEquals(pkgContact.PK, undg.DI_OC_DGContact);
			AssertEquals(false, undg.DI_HasOverpack);
			AssertEquals("overpack2", undg.DI_OverpackID);
			AssertEquals("WHATEVER OTHER", undg.DI_TechnicalName);
			AssertEquals(7m, undg.DI_DGVolume);
			AssertEquals(501m, undg.DI_DGWeight);
			AssertEquals("M3", undg.DI_UnitOfVolume);
			AssertEquals("KG", undg.DI_UnitOfWeight);
			AssertEquals(9, undg.DI_PackageCount);
		}

		#endregion

		#region TestJL_Calc_OriginTransitWarehouse

		public void TestJL_Calc_OriginTransitWarehouse_ReturnsShipmentCFS()
		{
			var cfsOrg = Factory.NewWithValidTestData<OrgAddress>();

			var shipment = Factory.New<CommonShipment>();
			shipment.JS_OA_ExportReceivingDepot = cfsOrg.PK;

			var packLine = shipment.OuterPackLines.AddNew();

			AssertEquals("JL_Calc_OriginTransitWarehouse returns Shipment Pickup CFS", shipment.JS_OA_ExportReceivingDepot, packLine.JL_Calc_OriginTransitWarehouse.PK);
		}

		public void TestJL_Calc_OriginTransitWarehouse_FallsBackToConsolCFS()
		{
			var cfsOrg = Factory.NewWithValidTestData<OrgAddress>();

			var shipment = Factory.New<CommonShipment>();
			var consol = shipment.Consols.AddNew();
			consol.JK_OA_PackDepotAddress = cfsOrg.PK;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.CurrentConsol = consol;

			AssertEquals("JL_Calc_OriginTransitWarehouse falls back to CurrentConsol Departure CFS", consol.JK_OA_PackDepotAddress, packLine.JL_Calc_OriginTransitWarehouse.PK);
		}

		public void TestJL_Calc_OriginTransitWarehouse_IsNullSafe()
		{
			AssertNoExceptionThrown("JL_Calc_OriginTransitWarehouse does not throw exception when Shipment CFS and Consol / Consol CFS is null", () =>
			{
				var shipment = Factory.New<CommonShipment>();
				var packLine = shipment.OuterPackLines.AddNew();

				_ = packLine.JL_Calc_OriginTransitWarehouse;
			});
		}

		#endregion

		#region TestJL_OutturnVolumeUQProxysValue

		public void TestJL_OutturnVolumeUQProxysValue()
		{
			PackLine packLine = Factory.New<PackLine>();
			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			AssertEquals(Core.Constants.Volume.CubicMetres, packLine.JL_OutturnVolumeUQ);

			packLine.JL_ActualVolumeUQ = Core.Constants.Volume.Litre;
			AssertEquals(Core.Constants.Volume.Litre, packLine.JL_OutturnVolumeUQ);
		}

		#endregion

		#region TestJL_OutturnWeightUQProxysValue

		public void TestJL_OutturnWeightUQProxysValue()
		{
			PackLine packLine = Factory.New<PackLine>();
			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals(Core.Constants.Weight.Kilograms, packLine.JL_OutturnWeightUQ);

			packLine.JL_ActualWeightUQ = Core.Constants.Weight.Tonnes;
			AssertEquals(Core.Constants.Weight.Tonnes, packLine.JL_OutturnWeightUQ);
		}

		#endregion

		#region TestPackLocations

		public void TestPackLocations()
		{
			PackLine packLine = Factory.New<PackLine>();
			AssertNotNull(packLine.PackLocations);
		}

		#endregion

		#region TestHumanReadableName

		public void TestHumanReadableName()
		{
			PackLine = Factory.New<PackLine>();

			AssertEquals("Inner Package", PackLine.HumanReadableName);

			PackLine.JL_FreightMode = FreightConstants.OuterPackType;
			AssertEquals("Outer Package", PackLine.HumanReadableName);

			PackLine.JL_FreightMode = FreightConstants.DeliveryPackType;
			AssertEquals("Delivery Package", PackLine.HumanReadableName);
		}

		#endregion

		#region TestPackingTooMuchIntoContainerHasError

		public void TestPackingTooMuchIntoContainerHasError()
		{
			string errorMsg = "The container 'MAEU1234567' is over-packed, try packing one or more lines into another container, or reduce the lines' weight.";

			CommonContainer container = Factory.New<CommonContainer>();

			CommonShipment shipment = Factory.New<CommonShipment>();
			PackLine line1 = shipment.OuterPackLines.AddNew();
			PackLine line2 = shipment.OuterPackLines.AddNew();

			container.JC_ContainerNum = "MAEU1234567";
			container.PackLines.Add(line1);
			container.PackLines.Add(line2);

			line1.JL_ActualWeightUQ = Constants.Weight.Tonnes;
			line1.JL_ActualWeight = 999;
			AssertNoError(line1.JL_ActualWeightInfo, errorMsg);

			line1.JL_ActualWeight = 1000;
			AssertHasError(line1.JL_ActualWeightInfo, errorMsg);

			line2.JL_ActualWeight = 0;
			AssertNoError(line2.JL_ActualWeightInfo, errorMsg); // no error if line has 0 weight

			line2.JL_ActualWeight = 1;
			AssertHasError(line2.JL_ActualWeightInfo, errorMsg);
		}

		#endregion

		#region TestConfirmDivotValidationForPackLine

		public void TestConfirmDivotValidationForPackLine_PackageCount()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_OuterPacks = 100;
			CommonPickupDeliveryConfirm confirm = shipment.DeliveryConfirms.AddNew();
			CommonConfirmDivot divot = confirm.Divots[0];

			divot.Validation.ValidateJ8_PackagesDelivered();
			shipment.RunPreSaveValidation();
			Factory.Save();

			Assert("Pre-condition", !divot.J8_PackagesDeliveredInfo.HasErrors());

			shipment.JS_OuterPacks = 50;
			shipment.RunPreSaveValidation();

			AssertHasWarningContaining(divot.J8_PackagesDeliveredInfo, "You cannot deliver more packs than there are in the shipment. Please check before proceeding.");
		}

		public void TestConfirmDivotValidationForPackLine_ActualWeight()
		{
			var shipment = Factory.New<CommonShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 10;
			packline.JL_ActualWeight = 100;

			var confirm = shipment.DeliveryConfirms.AddNew();
			var divot = confirm.Divots[0];

			shipment.RunPreSaveValidation();
			Assert(!divot.J8_DeliveryWeightInfo.HasErrors());

			packline.JL_ActualWeight = 50;
			shipment.RunPreSaveValidation();
			AssertHasWarningContaining(divot.J8_DeliveryWeightInfo, "You cannot deliver more weight than there is in the shipment.");
		}

		public void TestConfirmDivotValidationForPackLine_ActualVolume()
		{
			var shipment = Factory.New<CommonShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 10;
			packline.JL_ActualVolume = 100;

			var confirm = shipment.DeliveryConfirms.AddNew();
			var divot = confirm.Divots[0];

			shipment.RunPreSaveValidation();
			Assert(!divot.J8_DeliveryVolumeInfo.HasErrors());

			packline.JL_ActualVolume = 50;
			shipment.RunPreSaveValidation();
			AssertHasWarningContaining(divot.J8_DeliveryVolumeInfo, "You cannot deliver more volume than there is in the shipment.");
		}

		#endregion

		#region TestDeletingConfirmDivots

		[ExpectNoExceptions()]
		public void TestDeletingConfirmDivots()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_OuterPacks = 100;
			PackLine packLine = shipment.OuterPackLines[0];
			CommonPickupDeliveryConfirm confirm = shipment.DeliveryConfirms.AddNew();
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			CommonShipment shipmentInNewFactory = newFactory.Load<CommonShipment>(shipment.PK);
			PackLine packLineInNewFactory = shipmentInNewFactory.OuterPackLines[0];
			packLineInNewFactory.Delete();
		}

		#endregion

		#region TestNoConcurrencyExceptionWhenUpdatingJL_PackageCountOnDeletedPacklineInAnotherFactory

		public void TestNoConcurrencyExceptionWhenUpdatingJL_PackageCountOnDeletedPacklineInAnotherFactory()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_OuterPacks = 10;
			var packLine = shipment.OuterPackLines[0];
			var confirm = shipment.DeliveryConfirms.AddNew();
			((ILightValidationInternals)confirm.Divots[0]).IsValid = true;
			Factory.Save();

			packLine.Delete();

			var newFactory = new BusinessObjectFactory();
			var shipmentInNewFactory = newFactory.Load<CommonShipment>(shipment.PK);
			var packLineInNewFactory = shipmentInNewFactory.OuterPackLines[0];
			packLineInNewFactory.JL_PackageCount = 5;

			AssertNoExceptionThrown(() =>
			{
				packLineInNewFactory.RunPreSaveValidation();
				newFactory.Save();
			});
		}

		#endregion

		#region TestIsRegisteredEditableForConfirmDivots

		public void TestIsRegisteredEditableForConfirmDivots()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_OuterPacks = 10;
			var packLine = shipment.OuterPackLines[0];

			Assert(packLine.IsRegisteredEditableChildObject(packLine.ConfirmDivots));
		}

		#endregion

		#region TestSetupDefaultWeightAndDimensionsForPackType

		public void TestSetupDefaultWeightAndDimensionsForPackType()
		{
			var packType = Factory.New<RefPackType>();
			packType.F3_Code = "ROB";
			packType.F3_Description = "ROBERT";
			packType.F3_Length = 100;
			packType.F3_Height = 200;
			packType.F3_Width = 300;
			packType.F3_UnitOfDimension = "KM";
			packType.F3_Weight = 400;
			packType.F3_UnitOfWeight = "T";

			RefUNLOCO currentPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			ZQuery otherCountryFilter = new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			RefCountry otherCountry = Factory.LoadTop1<RefCountry>(otherCountryFilter);
			RefUNLOCO otherUnloco1 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, otherCountry.RN_Code));
			otherCountryFilter.AddToFilter(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, otherCountry.RN_Code);
			RefCountry otherCountry2 = Factory.LoadTop1<RefCountry>(otherCountryFilter);
			RefUNLOCO otherUnloco2 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, otherCountry2.RN_Code));
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_RL_NKClosestPort = otherUnloco1.RL_Code;

			OrgSupplierBuyerLink link1 = consignor.BuyerLinks.AddNew(consignee);
			link1.OL_RN_NKImporterCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			OrgSupplierBuyerLink link2 = consignor.BuyerLinks.AddNew(consignee);
			link2.OL_RN_NKImporterCountry = otherCountry.RN_Code;

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_RL_NKDestination = currentPort.RL_Code;

			PackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_F3_NKPackType = "ROB";

			packLine.JL_PackageCount = 2;
			AssertEquals(packLine.JL_F3_NKPackType, packType.F3_Code);
			AssertEquals(100m, packLine.JL_Length);
			AssertEquals(200m, packLine.JL_Height);
			AssertEquals(300m, packLine.JL_Width);
			AssertEquals("KM", packLine.JL_UnitOfDimension);
			AssertEquals(800m, packLine.JL_ActualWeight);
			AssertEquals("T", packLine.JL_ActualWeightUQ);

			packType.F3_Length = 101;
			packType.F3_Height = 201;
			packType.F3_Width = 301;
			packType.F3_Weight = 0;

			packLine.JL_PackageCount = 3;
			AssertEquals(101m, packLine.JL_Length);
			AssertEquals(201m, packLine.JL_Height);
			AssertEquals(301m, packLine.JL_Width);
			AssertEquals(800m, packLine.JL_ActualWeight);

			OrgBuyerSupplierLinkPackPivot packageDetails1 = link1.PackPivots.AddNew();
			packageDetails1.Q0_F3 = packType.PK;
			packageDetails1.Q0_Length = 0;
			packageDetails1.Q0_Height = 20;
			packageDetails1.Q0_Width = 0;
			packageDetails1.Q0_UnitOfDimension = "CM";
			packageDetails1.Q0_Weight = 0;
			packageDetails1.Q0_UnitOfWeight = "KG";
			OrgBuyerSupplierLinkPackPivot packageDetails2 = link2.PackPivots.AddNew();
			packageDetails2.Q0_F3 = packType.PK;
			packageDetails2.Q0_Length = 1;
			packageDetails2.Q0_Height = 30;
			packageDetails2.Q0_Width = 2;
			packageDetails2.Q0_UnitOfDimension = "CM";
			packageDetails2.Q0_Weight = 3;
			packageDetails2.Q0_UnitOfWeight = "KG";

			packType.F3_Weight = 400;

			packLine.JL_PackageCount = 4;
			AssertEquals(packType.F3_Code, packLine.JL_F3_NKPackType);
			AssertEquals(0m, packLine.JL_Length);
			AssertEquals(20m, packLine.JL_Height);
			AssertEquals(0m, packLine.JL_Width);
			AssertEquals("CM", packLine.JL_UnitOfDimension);
			AssertEquals(1600m, packLine.JL_ActualWeight);
			AssertEquals("T", packLine.JL_ActualWeightUQ);

			packageDetails1.Q0_Weight = 40;
			packageDetails1.Q0_Height = 0;
			packageDetails1.Q0_Length = 0;
			packageDetails1.Q0_Width = 0;

			packLine.JL_PackageCount = 5;
			AssertEquals(101m, packLine.JL_Length);
			AssertEquals(201m, packLine.JL_Height);
			AssertEquals(301m, packLine.JL_Width);
			AssertEquals("KM", packLine.JL_UnitOfDimension);
			AssertEquals(200m, packLine.JL_ActualWeight);
			AssertEquals("KG", packLine.JL_ActualWeightUQ);

			shipment.JS_RL_NKDestination = otherUnloco2.RL_Code;
			packLine.JL_PackageCount = 6;
			AssertEquals(1m, packLine.JL_Length);
			AssertEquals(30m, packLine.JL_Height);
			AssertEquals(2m, packLine.JL_Width);
			AssertEquals("CM", packLine.JL_UnitOfDimension);
			AssertEquals(18m, packLine.JL_ActualWeight);
			AssertEquals("KG", packLine.JL_ActualWeightUQ);

			consignee.OH_RL_NKClosestPort = otherUnloco2.RL_Code;
			packLine.JL_PackageCount = 7;

			AssertEquals(101m, packLine.JL_Length);
			AssertEquals(201m, packLine.JL_Height);
			AssertEquals(301m, packLine.JL_Width);
			AssertEquals("KM", packLine.JL_UnitOfDimension);
			AssertEquals(2800m, packLine.JL_ActualWeight);
			AssertEquals("T", packLine.JL_ActualWeightUQ);
		}

		#endregion

		#region TestSave_PackLineWithoutShipment_ReportError

		public void TestSave_PackLineWithoutShipment_ReportError()
		{
			PackLine.ForcePacklineWithoutShipmentErrorReporting = true;

			try
			{
				PackLine.JL_JS = ZGuid.Empty;

				AssertExceptionThrown(typeof(ZSaveException), Factory.Save);
				AssertEquals(true, ErrorReporter.LastKeyReported.Contains("PacklineWithoutShipment"));
			}
			finally
			{
				PackLine.ForcePacklineWithoutShipmentErrorReporting = false;
			}

			ErrorReporter.Clear();
		}

		#endregion

		#region TestTemperatureRangeDefaultingOnRefCommodityCodeTemperatureRange

		public void TestRefCommodityCodeTemperatureRangeBothZeroDoesNotChangeTemperatureRange()
		{
			var packLine = Factory.NewWithValidTestData<PackLine>();
			var refCommodityCodeBO = Factory.NewWithValidTestData<RefCommodityCode>();
			refCommodityCodeBO.RH_ReeferMinTemperature = 0;
			refCommodityCodeBO.RH_ReeferMaxTemperature = 0;
			refCommodityCodeBO.RH_Code = "TEST";

			packLine.JL_RequiresTemperatureControl = true;
			packLine.JL_RequiredTemperatureMinimum = -5.0;
			packLine.JL_RequiredTemperatureMaximum = 5.0;
			packLine.JL_RequiredTemperatureUnit = Constants.Temperature.Centigrade;

			packLine.JL_RH_NKCommodityCode = refCommodityCodeBO.RH_Code;

			CombineAssertions("the packline should be unchanged", () =>
			{
				AssertEquals(true, packLine.JL_RequiresTemperatureControl);
				AssertEquals((ZDecimal)(-5.0), packLine.JL_RequiredTemperatureMinimum);
				AssertEquals((ZDecimal)5.0, packLine.JL_RequiredTemperatureMaximum);
				AssertEquals(Constants.Temperature.Centigrade, packLine.JL_RequiredTemperatureUnit);
			});
		}

		public void TestInvalidRefCommodityCodeDoesNotChangeTemperatureRange()
		{
			var packLine = Factory.NewWithValidTestData<PackLine>();
			packLine.JL_RequiresTemperatureControl = true;
			packLine.JL_RequiredTemperatureMinimum = -5.0;
			packLine.JL_RequiredTemperatureMaximum = 5.0;
			packLine.JL_RequiredTemperatureUnit = Constants.Temperature.Centigrade;

			packLine.JL_RH_NKCommodityCode = "INVA";

			CombineAssertions("the packline should be unchanged", () =>
			{
				AssertEquals(true, packLine.JL_RequiresTemperatureControl);
				AssertEquals((ZDecimal)(-5.0), packLine.JL_RequiredTemperatureMinimum);
				AssertEquals((ZDecimal)5.0, packLine.JL_RequiredTemperatureMaximum);
				AssertEquals(Constants.Temperature.Centigrade, packLine.JL_RequiredTemperatureUnit);
			});
		}

		#endregion

		#region JL_AdditionalInspectionTypeCode

		public void TestJL_AdditionalInspectionTypeCode_Saving()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var pack1 = shipment.OuterPackLines.AddNew();
			var pack2 = shipment.OuterPackLines.AddNew();

			pack1.FillWithValidTestData();
			pack2.FillWithValidTestData();
			pack1.JL_IsHighRisk = true;
			pack2.JL_IsHighRisk = true;
			pack1.JL_AdditionalInspectionTypeCode = "";
			pack2.JL_AdditionalInspectionTypeCode = "XYZ";

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedPack1 = newFactory.Load<PackLine>(pack1.PK);
			var reloadedPack2 = newFactory.Load<PackLine>(pack2.PK);

			AssertEquals("", reloadedPack1.JL_AdditionalInspectionTypeCode);
			AssertEquals("XYZ", reloadedPack2.JL_AdditionalInspectionTypeCode);

			Func<ZGuid, CusEntryNumCollection> getCusEntryNumCollection = pk =>
			{
				var filter = new ZQuery(CusEntryNumSchema.CE_ParentID, pk);
				var collection = new CusEntryNumCollection(newFactory, filter);
				collection.Load();
				return collection;
			};

			var reloadedPack1CusEntryNumCollection = getCusEntryNumCollection(reloadedPack1.PK);
			var reloadedPack2CusEntryNumCollection = getCusEntryNumCollection(reloadedPack2.PK);

			AssertEquals("No CusEntryNums saved for default value", 0, reloadedPack1CusEntryNumCollection.Count);
			AssertEquals("One CusEntryNum saved for non-default value", 1, reloadedPack2CusEntryNumCollection.Count);

			pack2.JL_AdditionalInspectionTypeCode = ZString.Empty;
			Factory.Save();
			reloadedPack2CusEntryNumCollection = getCusEntryNumCollection(reloadedPack2.PK);
			AssertEquals("Old CusEntryNum is deleted", 0, reloadedPack2CusEntryNumCollection.Count);
		}

		public void TestJL_AdditionalInspectionTypeCodeHasChanges()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.HongKong))
			{
				var shipment = CommonShipment.New(Factory);
				shipment.JS_TransportMode = Constants.TransportModes.Air;

				var packLine = shipment.OuterPackLines.AddNew();
				Assert(!packLine.JL_AdditionalInspectionTypeCodeHasChanges);

				packLine.JL_AdditionalInspectionTypeCode = "PHS";
				Assert(packLine.JL_AdditionalInspectionTypeCodeHasChanges);
			}
		}

		public void TestAdditionalInspectionTypes()
		{
			var shipment = Factory.New<CommonShipment>();
			var pack = shipment.OuterPackLines.AddNew();
			var codes = pack.AdditionalInspectionTypes.GetAllCodes();

			AssertCollectionContains("XRY", codes);
			AssertCollectionContains("PHS", codes);
			AssertCollectionContains("VCK", codes);
			AssertCollectionContains("EDS", codes);
			AssertCollectionContains("AOM", codes);
			AssertCollectionContains("EDD", codes);
			AssertCollectionContains("ETD", codes);
			AssertCollectionContains("CMD", codes);
			AssertCollectionContains("ZZZ", codes);
			AssertCollectionContains("UNK", codes);
		}

		#endregion

		#region JL_InspectionTypeCode

		public void TestJL_InspectionTypeCode_Saving()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var pack1 = shipment.OuterPackLines.AddNew();
			var pack2 = shipment.OuterPackLines.AddNew();

			pack1.FillWithValidTestData();
			pack2.FillWithValidTestData();

			pack1.JL_InspectionTypeCode = "";
			pack2.JL_InspectionTypeCode = "XYZ";

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedPack1 = newFactory.Load<PackLine>(pack1.PK);
			var reloadedPack2 = newFactory.Load<PackLine>(pack2.PK);

			AssertEquals("", reloadedPack1.JL_InspectionTypeCode);
			AssertEquals("XYZ", reloadedPack2.JL_InspectionTypeCode);

			Func<ZGuid, CusEntryNumCollection> getCusEntryNumCollection = pk =>
			{
				var filter = new ZQuery(CusEntryNumSchema.CE_ParentID, pk);
				var collection = new CusEntryNumCollection(newFactory, filter);
				collection.Load();
				return collection;
			};

			var reloadedPack1CusEntryNumCollection = getCusEntryNumCollection(reloadedPack1.PK);
			var reloadedPack2CusEntryNumCollection = getCusEntryNumCollection(reloadedPack2.PK);

			AssertEquals("No CusEntryNums saved for default value", 0, reloadedPack1CusEntryNumCollection.Count);
			AssertEquals("One CusEntryNum saved for non-default value", 1, reloadedPack2CusEntryNumCollection.Count);
		}

		public void TestInspectionTypes()
		{
			var shipment = Factory.New<CommonShipment>();
			var pack = shipment.OuterPackLines.AddNew();
			var codes = pack.InspectionTypes.GetAllCodes();

			AssertCollectionContains("XRY", codes);
			AssertCollectionContains("DIP", codes);
			AssertCollectionContains("UNK", codes);

			AssertCollectionNotContains("APP", codes);
			AssertCollectionNotContains("SMU", codes);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("JM"))
			{
				codes = pack.InspectionTypes.GetAllCodes();

				AssertCollectionContains("XRY", codes);
				AssertCollectionContains("DIP", codes);
				AssertCollectionContains("SMU", codes);
				AssertCollectionContains("UNK", codes);

				AssertCollectionNotContains("APP", codes);
			}
		}

		public void TestOriginTransitWarehouseStatus()
		{
			var shipment = Factory.New<CommonShipment>();
			var pack = shipment.OuterPackLines.AddNew();
			var codes = pack.JL_OriginTransitWarehouseStatus_List.GetAllCodes();

			AssertContainsExactElementsInAnyOrder(new ZString[] {
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown,
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed,
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies,
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.ShortShipped,
				FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Surplus,
			}, codes);
		}

		#endregion

		#region TestJL_Calc_DGClass

		public void TestJL_Calc_DGClass()
		{
			PackLine line = Factory.New<PackLine>();

			AssertEquals("No attached substances should return empty string", "", line.JL_Calc_DGClass);

			var undg = line.UNDGs.AddNew();
			var subs = UNDGSubstanceLoader.LoadSubstances(Factory, "1001", "", "IMO").FirstOrDefault();
			undg.LinkDefault(subs);
			undg.DI_DG = subs.PK;

			AssertEquals("1 attached substance directly returns Class Info", "2.1", line.JL_Calc_DGClass);

			var undg2 = line.UNDGs.AddNew();
			undg2.DI_DG = subs.PK;
			undg2.LinkDefault(subs);
			AssertEquals("More than one attached substance of the same type returns that Class Info", "2.1", line.JL_Calc_DGClass);

			var undg3 = line.UNDGs.AddNew();
			var subs2 = UNDGSubstanceLoader.LoadSubstances(Factory, "1002", "", "IMO").FirstOrDefault();
			undg3.LinkDefault(subs2);
			AssertEquals("If there are several types of Class attached, return Mixed", "Mixed", line.JL_Calc_DGClass);
		}

		#endregion

		#region TestJL_Calc_DGSubstance

		public void TestJL_Calc_DGSubstance_Deprecated()
		{
			PackLine line = Factory.New<PackLine>();

			AssertEquals("No attached substances should return empty string", "", line.JL_Calc_DGSubstance);

			var undg = line.UNDGs.AddNew();
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_Code = "1001";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undg.LinkDefault(subs);

			AssertEquals("1 attached substance directly returns Substance Info", "1001", line.JL_Calc_DGSubstance);

			var undg2 = line.UNDGs.AddNew();
			undg2.LinkDefault(subs);
			AssertEquals("More than one attached substance of the same type returns that Substance Info", "1001", line.JL_Calc_DGSubstance);

			var undg3 = line.UNDGs.AddNew();
			var subs2 = Factory.New<UNDGSubstance>();
			subs2.DG_Code = "1002";
			subs2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			undg3.LinkDefault(subs2);
			AssertEquals("If there are several types of Substance attached, return Mixed", "Mixed", line.JL_Calc_DGSubstance);
		}

		public void TestJL_Calc_DGSubstance()
		{
			PackLine line = Factory.New<PackLine>();

			AssertEquals("No attached substances should return empty string", "", line.JL_Calc_DGSubstance);

			var subs1 = Factory.New<UNDGSubstance>();
			subs1.DG_UNNO = "1001";
			subs1.DG_Variant = "a";

			var subs2 = Factory.New<UNDGSubstance>();
			subs2.DG_UNNO = "1002";
			subs2.DG_Variant = "a";

			var undg1 = line.UNDGs.AddNew();
			undg1.UNDGSubstancePivotCollection.UpdateDefaultPivot(subs1);
			AssertEquals("1 attached substance directly returns Substance Info", "1001a", line.JL_Calc_DGSubstance);

			var undg2 = line.UNDGs.AddNew();
			undg2.UNDGSubstancePivotCollection.UpdateDefaultPivot(subs2);
			AssertEquals("More than one attached substance of the same type returns that Substance Info", "Mixed", line.JL_Calc_DGSubstance);
		}

		public void TestJL_Calc_DGSubstanceWithNoDefault()
		{
			var subs1 = Factory.New<UNDGSubstance>();
			subs1.DG_UNNO = "1001";
			subs1.DG_Variant = "a";

			PackLine line = Factory.New<PackLine>();

			var undg1 = line.UNDGs.AddNew();
			undg1.UNDGSubstancePivotCollection.AddPivotFromSubstance(subs1);
			AssertEquals("No default substance attached returns empty string", ZString.Empty, line.JL_Calc_DGSubstance);
		}

		#endregion

		#region TestJL_Calc_DIHazardousWasteCode

		public void TestJL_Calc_DIHazardousWasteCode()
		{
			PackLine line = Factory.New<PackLine>();

			AssertEquals("No waste codes", "", line.JL_Calc_DIHazardousWasteCode);

			var undg1 = line.UNDGs.AddNew();
			AssertEquals("Empty waste codes", "", line.JL_Calc_DIHazardousWasteCode);

			undg1.DI_HazardousWasteCode = "xxx";
			AssertEquals("One waste code", "xxx", line.JL_Calc_DIHazardousWasteCode);

			var undg2 = line.UNDGs.AddNew();
			AssertEquals("One waste code and One stub", "Many", line.JL_Calc_DIHazardousWasteCode);

			undg2.DI_HazardousWasteCode = "xxx";
			AssertEquals("Two of the same waste codes", "xxx", line.JL_Calc_DIHazardousWasteCode);

			var undg3 = line.UNDGs.AddNew();
			undg3.DI_HazardousWasteCode = "yyy";
			AssertEquals("Many waste codes", "Many", line.JL_Calc_DIHazardousWasteCode);
		}

		#endregion

		#region TestJL_Calc_DISpecialPermitIssueDate

		public void TestJL_Calc_DISpecialPermitIssueDate()
		{
			PackLine line = Factory.New<PackLine>();
			AssertEquals("No permit issue dates", "", line.JL_Calc_DISpecialPermitIssueDate);

			var undg1 = line.UNDGs.AddNew();
			AssertEquals("Empty permit issue dates", "", line.JL_Calc_DISpecialPermitIssueDate);

			undg1.DI_SpecialPermitIssueDate = new ZDate(2020, 1, 1);
			AssertEquals("One permit issue date", "01-Jan-20", line.JL_Calc_DISpecialPermitIssueDate);

			var undg2 = line.UNDGs.AddNew();
			AssertEquals("One permit issue date and One stub", "Many", line.JL_Calc_DISpecialPermitIssueDate);

			undg2.DI_SpecialPermitIssueDate = new ZDate(2020, 1, 1);
			AssertEquals("Two of the same permit issue dates", "01-Jan-20", line.JL_Calc_DISpecialPermitIssueDate);

			var undg3 = line.UNDGs.AddNew();
			undg3.DI_SpecialPermitIssueDate = new ZDate(2020, 1, 2);
			AssertEquals("Many permit issue dates", "Many", line.JL_Calc_DISpecialPermitIssueDate);
		}

		#endregion

		#region TestJL_Calc_DISpecialPermitNumber

		public void TestJL_Calc_DISpecialPermitNumber()
		{
			PackLine line = Factory.New<PackLine>();

			AssertEquals("No permit numbers", "", line.JL_Calc_DISpecialPermitNumber);

			var undg1 = line.UNDGs.AddNew();
			AssertEquals("Empty permit numbers", "", line.JL_Calc_DISpecialPermitNumber);

			undg1.DI_SpecialPermitNumber = "xxx";
			AssertEquals("One permit number", "xxx", line.JL_Calc_DISpecialPermitNumber);

			var undg2 = line.UNDGs.AddNew();
			AssertEquals("One permit number and One stub", "Many", line.JL_Calc_DISpecialPermitNumber);

			undg2.DI_SpecialPermitNumber = "xxx";
			AssertEquals("Two of the same permit numbers", "xxx", line.JL_Calc_DISpecialPermitNumber);

			var undg3 = line.UNDGs.AddNew();
			undg3.DI_SpecialPermitNumber = "yyy";
			AssertEquals("Many permit numbers", "Many", line.JL_Calc_DISpecialPermitNumber);
		}

		#endregion

		#region TestJL_Calc_DIIsSalvagePackaging

		public void TestJL_Calc_DIIsSalvagePackaging()
		{
			PackLine line = Factory.New<PackLine>();

			AssertEquals("None to salvage", "", line.JL_Calc_DIIsSalvagePackaging);

			var undg1 = line.UNDGs.AddNew();
			AssertEquals("default value", "N", line.JL_Calc_DIIsSalvagePackaging);

			undg1.DI_IsSalvagePackaging = true;
			AssertEquals("One true", "Y", line.JL_Calc_DIIsSalvagePackaging);

			var undg2 = line.UNDGs.AddNew();
			undg2.DI_IsSalvagePackaging = true;
			AssertEquals("Two of the same UNDG Substance", "Y", line.JL_Calc_DIIsSalvagePackaging);

			var undg3 = line.UNDGs.AddNew();
			undg3.DI_IsSalvagePackaging = false;
			AssertEquals("both true and false", "Many", line.JL_Calc_DIIsSalvagePackaging);
		}

		#endregion

		#region TestJL_Calc_DIIsResidueLastContained

		public void TestJL_Calc_DIIsResidueLastContained()
		{
			PackLine line = Factory.New<PackLine>();

			AssertEquals("No undgs", "", line.JL_Calc_DIIsResidueLastContained);

			var undg1 = line.UNDGs.AddNew();
			AssertEquals("default value", "N", line.JL_Calc_DIIsResidueLastContained);

			undg1.DI_IsResidueLastContained = true;
			AssertEquals("One true", "Y", line.JL_Calc_DIIsResidueLastContained);

			var undg2 = line.UNDGs.AddNew();
			undg2.DI_IsResidueLastContained = true;
			AssertEquals("Two of the same UNDG Substance", "Y", line.JL_Calc_DIIsResidueLastContained);

			var undg3 = line.UNDGs.AddNew();
			undg3.DI_IsResidueLastContained = false;
			AssertEquals("both true and false", "Many", line.JL_Calc_DIIsResidueLastContained);
		}

		#endregion

		#region TestPopulateJL_PackLineId

		public void TestPopulateJL_PackLineId()
		{
			var enterpriseCode = GlbCompany.CurrentCompany.LicenceEnterpriseCode;
			var serverCode = GlbCompany.CurrentCompany.LicenceServerID;
			var prefix = $"{enterpriseCode}{serverCode}";

			var shipment = CommonShipment.New(Factory);
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			var packLine = shipment.OuterPackLines.AddNew();
			AssertEquals("Precondition: JL_PackLineId should be readonly", true, packLine.JL_PackLineIdInfo.ReadOnly);
			Factory.Save();
			AssertEquals("Populate on first saving: JS_PackLineId", $"{prefix}00000002", packLine.JL_PackLineId);

			var columnNamesToExcludeFromCopy = new List<string>
				{
					JobShipmentSchema.Constants.JS_UniqueConsignRef
				};

			var clonedShipment = (CommonShipment)shipment.Clone(new BusinessObjectCloneArgs(columnNamesToExcludeFromCopy.ToArray()));
			AssertEquals("Precondition: JL_PackLineId should be empty", ZString.Empty, clonedShipment.OuterPackLines[0].JL_PackLineId);
			Factory.Save();
			AssertEquals("Populate on second saving: JS_PackLineId", $"{prefix}00000003", clonedShipment.OuterPackLines[0].JL_PackLineId);
		}

		public void TestJL_PackLineId_NewPackLine_PackLineIdIsNotEmptyAfterSaveFailed()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_EFreightStatus = "CCC";
			var packline = shipment.OuterPackLines.AddNew();
			var pkgPackage = packline.PkgPackageCollection.AddNew();
			pkgPackage.FillWithValidTestData();

			AssertExceptionThrown("Precondition: ZSaveException should be thrown and save should fail", typeof(ZSaveException), Factory.Save);

			Assert("packline is not saved.", !packline.IsInDatabase);
			Assert("packline id is not cleared.", !packline.JL_PackLineId.IsEmpty);
			Assert("packline id on package is not cleared.", !packline.PkgPackageCollection.First().KP_ExternalReference.IsEmpty);
		}

		public void TestJL_PackLineId_NewPackLineWithDisorder()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			var shipment1 = factory1.Load<CommonShipment>(shipment.PK);
			var packline1 = shipment1.OuterPackLines.AddNew();

			var factory2 = new BusinessObjectFactory();
			var shipment2 = factory2.Load<CommonShipment>(shipment.PK);
			var packline2 = shipment2.OuterPackLines.AddNew();

			factory2.Save();
			factory1.Save();

			AssertEquals("EDIDAT00000003", packline1.JL_PackLineId);
			AssertEquals("EDIDAT00000002", packline2.JL_PackLineId);
		}

		public void TestJL_PackLineId_NewPackLine_NextPackLineIdAfterSaveFailed()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_EFreightStatus = "CCC";
			var packline1 = shipment.OuterPackLines.AddNew();
			var pkgPackage = packline1.PkgPackageCollection.AddNew();
			pkgPackage.FillWithValidTestData();

			AssertExceptionThrown("Precondition: ZSaveException should be thrown and save should fail", typeof(ZSaveException), Factory.Save);

			Assert("packline is not saved.", !packline1.IsInDatabase);
			AssertEquals("EDIDAT00000002", packline1.JL_PackLineId);

			var packline2 = shipment.OuterPackLines.AddNew();
			AssertExceptionThrown("Precondition: ZSaveException should be thrown and save should fail", typeof(ZSaveException), Factory.Save);
			Assert("packline is not saved.", !packline2.IsInDatabase);
			AssertEquals("EDIDAT00000003", packline2.JL_PackLineId);
			AssertEquals("EDIDAT00000002", packline1.JL_PackLineId);
		}

		public void TestJL_PackLineId_SavedPackLine_PackLineIdRemainsUnchangedAfterSaveFailed()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			var packline = shipment.OuterPackLines.AddNew();
			var pkgPackage = packline.PkgPackageCollection.AddNew();
			pkgPackage.FillWithValidTestData();

			Factory.Save();

			Assert("Precondition: Packline id is generated", !packline.JL_PackLineId.IsEmpty);
			Assert("packline is saved.", packline.IsInDatabase);

			var savedPackLineId = packline.JL_PackLineId;
			shipment.JS_EFreightStatus = "CCC"; //condition to cause save failed
			packline.JL_PackageCount = 2; //make sure packline has changes

			AssertExceptionThrown("Precondition: ZSaveException should be thrown and save should fail", typeof(ZSaveException), Factory.Save);

			AssertEquals("packline id remains the same.", savedPackLineId, packline.JL_PackLineId);
			AssertEquals("packline id on package is consistent with packline id.", savedPackLineId, packline.PkgPackageCollection.First().KP_ExternalReference);
		}

		public void TestJL_PackLineId_SavedPackLine_PackLineIdIsEmptyAfterSaveFailedIfConcurrency()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			var packline = shipment.OuterPackLines.AddNew();
			var pkgPackage = packline.PkgPackageCollection.AddNew();
			pkgPackage.FillWithValidTestData();
			packline.JL_PackageCount = 1;
			Factory.Save();

			Assert("Precondition: Packline id is generated", !packline.JL_PackLineId.IsEmpty);
			Assert("packline is saved.", packline.IsInDatabase);
			var savedPackLineId = packline.JL_PackLineId;

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var shipmentInNewFactory = newFactory.Load<CommonShipment>(shipment.PK);
			var packlineInNewFactory = shipmentInNewFactory.OuterPackLines[0];
			packlineInNewFactory.JL_PackageCount = 3;
			newFactory.Save();

			var newpackline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 2;
			AssertExceptionThrown("Precondition: ZSaveConcurrencyException should be thrown and save should fail", typeof(ZSaveConcurrencyException), Factory.Save);

			AssertEquals("packline id remains the same.", savedPackLineId, packline.JL_PackLineId);
			AssertEquals("packline id on package is consistent with packline id.", savedPackLineId, packline.PkgPackageCollection.First().KP_ExternalReference);
			Assert("newpackline is not saved.", !newpackline.IsInDatabase);
			Assert("newpackline id is not cleared.", !newpackline.JL_PackLineId.IsEmpty);
		}

		public void TestJL_PackLineId_SavedPackLine_PackLineIdGeneratedOnlyOnceInTheSameTransaction()
		{
			var shipment = Factory.New<CommonShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			var packline = shipment.OuterPackLines.AddNew();
			var pkgPackage = packline.PkgPackageCollection.AddNew();
			pkgPackage.FillWithValidTestData();
			packline.JL_PackageCount = 1;
			Factory.Save();

			AssertEquals("packline id is generated from number fountain only once", "EDIDAT00000002", packline.JL_PackLineId);

			var newpackline = shipment.OuterPackLines.AddNew();
			Factory.Save();

			AssertEquals("packline id is generated from number fountain only once", "EDIDAT00000003", newpackline.JL_PackLineId);
		}

		public void TestJL_OriginTransitWarehouseStatusExcludedFromCloning()
		{
			var shipment = CommonShipment.New(Factory);
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			var packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies;
			Factory.Save();
			AssertEquals(FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies, packLine.JL_OriginTransitWarehouseStatus);

			var columnNamesToExcludeFromCopy = new List<string>
			{
				JobShipmentSchema.Constants.JS_UniqueConsignRef
			};

			var clonedShipment = (CommonShipment)shipment.Clone(new BusinessObjectCloneArgs(columnNamesToExcludeFromCopy.ToArray()));
			AssertEquals("Precondition: JL_OriginTransitWarehouseStatus should be UNK", FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown, clonedShipment.OuterPackLines[0].JL_OriginTransitWarehouseStatus);
		}

		#endregion

		#region TestJL_HarmonisedCode

		public void TestJL_HarmonisedCode()
		{
			var tariff1 = LoadOrCreateNewTariff(Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO,
				"1000", Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff2 = LoadOrCreateNewTariff(Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO,
				"200010", Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff3 = LoadOrCreateNewTariff(Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO,
				"200090", Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var pack1 = shipment.OuterPackLines.AddNew();
			pack1.JL_HarmonisedCode = "1000";
			AssertEquals(tariff1, pack1.HarmonisedCodeTariff);
			pack1.JL_HarmonisedCode = "2000";
			AssertNull(pack1.HarmonisedCodeTariff);

			var pack2 = shipment.OuterPackLines.AddNew();
			pack2.JL_HarmonisedCode = "200010";
			AssertEquals(tariff2, pack2.HarmonisedCodeTariff);
			pack2.JL_HarmonisedCode = "200090";
			AssertEquals(tariff3, pack2.HarmonisedCodeTariff);

			Factory.Save();

			var newFac = new BusinessObjectFactory();
			var shipmentInNewFac = newFac.Load<CommonShipment>(shipment.PK);
			AssertNull(shipmentInNewFac.OuterPackLines[0].HarmonisedCodeTariff);
			AssertEquals(tariff3.PK, shipmentInNewFac.OuterPackLines[1].HarmonisedCodeTariff.PK);
		}

		public void TestJL_HarmonisedCode_UpdatedByDataRefresh()
		{
			var tariff1 = LoadOrCreateNewTariff(Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO,
				"1000", Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff2 = LoadOrCreateNewTariff(Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO,
				"200010", Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);
			var tariff3 = LoadOrCreateNewTariff(Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO,
				"200090", Enterprise.Customs.Universal.Constants.TariffTypes.HarmonizedSystem);

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var pack1 = shipment.OuterPackLines.AddNew();
			pack1.JL_HarmonisedCode = "1000";
			AssertEquals(tariff1, pack1.HarmonisedCodeTariff);

			Factory.Save();

			var newFac = new BusinessObjectFactory
			{
				RefreshEnabled = true
			};
			var shipmentInNewFac = newFac.Load<CommonShipment>(shipment.PK);
			var pack1InNewFac = shipmentInNewFac.OuterPackLines[0];
			AssertEquals(tariff1.PK, pack1InNewFac.HarmonisedCodeTariff.PK);
			pack1InNewFac.JL_HarmonisedCode = "2000";
			AssertNull(pack1InNewFac.HarmonisedCodeTariff);

			newFac.Save();
			AssertNull("HarmonisedCodeTariff is updated after OnUpdatedByDataRefresh", pack1.HarmonisedCodeTariff);
		}

		TariffView LoadOrCreateNewTariff(string dataGrouping, string tariffCode, string nkTariffType)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(dataGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(dataGrouping, nkTariffType);
			var tariffView = helper.LoadOrCreateNewTariff(dataGrouping, tariffType.PK, tariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			return tariffView;
		}

		#endregion

		#region CusEntryNumbers

		public void TestCusEntryNumbers()
		{
			var shipment = Factory.New<CommonShipment>();
			var packLine = shipment.OuterPackLines.AddNew();

			CusEntryNumber num1 = packLine.CusEntryNums.AddNew();
			num1.CE_EntryType = "111";
			num1.CE_EntryNum = "222";
			num1.CE_Category = "ABC";
			num1.CE_ParentID = packLine.PK;

			CusEntryNumber num2 = packLine.CusEntryNums.AddNew();
			num2.CE_EntryType = "333";
			num2.CE_EntryNum = "444";
			num2.CE_Category = "OTH";
			num2.CE_ParentID = packLine.PK;

			CusEntryNumber num3 = packLine.CusEntryNums.AddNew();
			num3.CE_EntryType = "555";
			num3.CE_EntryNum = "666";
			num3.CE_Category = "PRT";
			num3.CE_ParentID = packLine.PK;

			Factory.Save();

			packLine = new BusinessObjectFactory().Load<PackLine>(packLine.PK);
			AssertEquals(3, packLine.CusEntryNums.Count);

			packLine.CusEntryNums.Delete(num3);

			Factory.Save();

			packLine = new BusinessObjectFactory().Load<PackLine>(packLine.PK);
			AssertEquals(2, packLine.CusEntryNums.Count);
		}

		#endregion

		#region Universal Copy

		public void TestUniversalCopyIgnoreElement()
		{
			var componentType = typeof(PackLine);
			var ignoreElementAttributes = componentType.GetCustomAttributes(typeof(UniversalCopyIgnoreElementAttribute), true);
			var attribute = ignoreElementAttributes[0] as UniversalCopyIgnoreElementAttribute;
			AssertCollectionContains("JL_PackLineId", AutoJobPackLines.Schema.JL_PackLineId, attribute.ElementNames);
			AssertCollectionContains("JL_OriginTransitWarehouseStatus", AutoJobPackLines.Schema.JL_OriginTransitWarehouseStatus, attribute.ElementNames);
		}

		public void TestOuterPacklineCanBeSuccessfullyCopiedEvenWhenFreightModeIsNotChosenInUniversalCopyTemplate()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "MYKUL";

			var packline = shipment.OuterPackLines.AddNew();
			PackLine.JL_FreightMode = FreightConstants.OuterPackType;

			Factory.Save();

			var elementType = typeof(PackLine);
			var interfaceType = GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(elementType, true);
			var copyTemplateTree = new CopyTemplateTree(interfaceType, elementType, BusinessObjectCopyManager.CopyTreeConfiguration);

			var packLine = (EntityCopyTemplateNode)copyTemplateTree.InnerNode;
			AssertNotNull("Pre-condition: packline node", packLine);

			var copyManager = new BusinessObjectCopyManager();
			var copiedPackLine = copyManager.Copy(packline, copyTemplateTree).Object as PackLine;

			AssertEquals(FreightConstants.OuterPackType, copiedPackLine.JL_FreightMode);
		}

		#endregion

		[ExpectNoExceptions]
		public void TestHandleContainerAllocationAndDeallocation()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();

			var handlerMock1 = new Mock<IContainerPenaltyCalculateHandler>();
			var container1 = Factory.New<DummyContainer>();
			container1.Handler = handlerMock1.Object;

			var handlerMock2 = new Mock<IContainerPenaltyCalculateHandler>();
			var container2 = Factory.New<DummyContainer>();
			container2.Handler = handlerMock2.Object;

			consol.Containers.Add(container1);
			consol.Containers.Add(container2);

			var packLine = shipment.OuterPackLines.AddNew();
			handlerMock1.Verify(x => x.HandleContainerAllocation(packLine), Moq.Times.Never);
			handlerMock1.Verify(x => x.HandleContainerDeallocation(packLine), Moq.Times.Never);
			handlerMock2.Verify(x => x.HandleContainerAllocation(packLine), Moq.Times.Once);
			handlerMock2.Verify(x => x.HandleContainerDeallocation(packLine), Moq.Times.Never);

			packLine.JL_JC = container1.PK;
			handlerMock1.Verify(x => x.HandleContainerAllocation(packLine), Moq.Times.Exactly(2));
			handlerMock1.Verify(x => x.HandleContainerDeallocation(packLine), Moq.Times.Never);
			handlerMock2.Verify(x => x.HandleContainerAllocation(packLine), Moq.Times.Once);
			handlerMock2.Verify(x => x.HandleContainerDeallocation(packLine), Moq.Times.Once);

			packLine.JL_JC = ZGuid.Empty;
			handlerMock1.Verify(x => x.HandleContainerAllocation(packLine), Moq.Times.Exactly(2));
			handlerMock1.Verify(x => x.HandleContainerDeallocation(packLine), Moq.Times.Exactly(2));
			handlerMock2.Verify(x => x.HandleContainerAllocation(packLine), Moq.Times.Once);
			handlerMock2.Verify(x => x.HandleContainerDeallocation(packLine), Moq.Times.Once);
		}

		public void TestJL_InspectionTypeCodeHasChanges()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.HongKong))
			{
				var shipment = CommonShipment.New(Factory);
				shipment.JS_TransportMode = Constants.TransportModes.Air;

				var packLine = shipment.OuterPackLines.AddNew();
				Assert(!packLine.JL_InspectionTypeCodeHasChanges);

				packLine.JL_InspectionTypeCode = "XRY";
				Assert(packLine.JL_InspectionTypeCodeHasChanges);
			}
		}

		public void TestSetInnerPackLinesFromPackageCollection()
		{
			var outerPackLine = CreatePackLine("REF004", "TEST004", "CTN", FreightConstants.OuterPackType, 5, 3000m, "KG", 300m, "M3", "GEN");
			var innerPackLine1 = CreatePackLine("REF001", "TEST001", "BOX", FreightConstants.InnerPackType, 3, 300m, "KG", 30m, "M3", "GEN", outerPackLine);
			var innerPackLine2 = CreatePackLine("REF002", "TEST002", "BAG", FreightConstants.InnerPackType, 3, 600m, "KG", 60m, "M3", "GEN", outerPackLine);
			var shipment = Factory.New<CommonShipment>();
			shipment.OuterPackLines.Add(outerPackLine);
			shipment.InnerPackLines.Add(innerPackLine1);
			shipment.InnerPackLines.Add(innerPackLine2);

			var outerPackage = CreatePackage("S00001000-CTN-1", "CTN", 5, 3000m, "KG", 300m, "M3", "GEN");
			CreatePackage("S00001000-BOX-1", "BOX", 1, 100m, "KG", 10m, "M3", "GEN", outerPackage);
			CreatePackage("S00001000-CTN-1", "BAG", 2, 200m, "KG", 20m, "M3", "CRMC", outerPackage);
			CreatePackage("S00001000-CTN-2", "BAG", 3, 300m, "KG", 30m, "M3", "CRMC", outerPackage);
			outerPackLine.PkgPackageCollection.Add(outerPackage);

			outerPackLine.SetInnerPackLinesFromPackageCollection();
			AssertEquals("2 inner packlines", 2, shipment.InnerPackLines.Count);
			var innerPackLine = shipment.InnerPackLines.Cast<PackLine>().FirstOrDefault(l => l.JL_F3_NKPackType == "BOX");
			AssertPackLine(innerPackLine, 1, "BOX", FreightConstants.InnerPackType, 100m, "KG", 10m, "M3", "GEN");
			innerPackLine = shipment.InnerPackLines.Cast<PackLine>().FirstOrDefault(l => l.JL_F3_NKPackType == "BAG");
			AssertPackLine(innerPackLine, 5, "BAG", FreightConstants.InnerPackType, 500m, "KG", 50m, "M3", "CRMC");
		}

		PackLine CreatePackLine(
			string refNumber, string packLineId, string packType, string freightMode, int packageCount, decimal actualWeight,
			string actualWeightUQ, decimal actualVolume, string actualVolumeUQ, string commodityCode, PackLine outerPackLine = null)
		{
			var packLine = Factory.New<PackLine>();
			packLine.JL_RefNumber = refNumber;
			packLine.JL_PackLineId = packLineId;
			packLine.JL_F3_NKPackType = packType;
			packLine.JL_FreightMode = freightMode;
			packLine.JL_PackageCount = packageCount;
			packLine.JL_ActualWeight = actualWeight;
			packLine.JL_ActualWeightUQ = actualWeightUQ;
			packLine.JL_ActualVolume = actualVolume;
			packLine.JL_ActualVolumeUQ = actualVolumeUQ;
			packLine.JL_RH_NKCommodityCode = commodityCode;
			packLine.JL_JL_OuterPackLine = outerPackLine?.PK ?? ZGuid.Empty;
			return packLine;
		}

		PkgPackage CreatePackage(
			string packageID, string packType, int packageQty, decimal weight,
			string weightUQ, decimal volume, string volumeUQ, string commodityCode, PkgPackage outerPackage = null)
		{
			var package = Factory.New<PkgPackage>();
			package.KP_PackageID = packageID;
			package.KP_F3_NKPackType = packType;
			package.KP_PackageQty = packageQty;
			package.KP_Weight = weight;
			package.KP_WeightUQ = weightUQ;
			package.KP_Volume = volume;
			package.KP_VolumeUQ = volumeUQ;
			package.KP_RH_NKCommodityCode = commodityCode;

			if (outerPackage != null)
			{
				var divot = outerPackage.PackageHandlingUnitHandlingUnitDivots.AddNew();
				divot.KPD_KP_Package = package.PK;
				divot.KPD_PackedTime = ZDateTimeOffset.Now;
				divot.KPD_GS_NKPackedUser = Env.CurrentUser.Initials;
			}

			return package;
		}

		static void AssertPackLine(
			PackLine packLine, int packageCount, string packType, string freightMode,
			decimal actualWeight, string actualWeightUQ, decimal actualVolume, string actualVolumeUQ, string commodityCode)
		{
			AssertNotNull($"packageCount: {packageCount}, packType: {packType}, freightMode: {freightMode}, commodityCode: {commodityCode}", packLine);
			AssertEquals(nameof(packLine.JL_PackageCount), packageCount, packLine.JL_PackageCount);
			AssertEquals(nameof(packLine.JL_F3_NKPackType), packType, packLine.JL_F3_NKPackType);
			AssertEquals(nameof(packLine.JL_FreightMode), freightMode, packLine.JL_FreightMode);
			AssertEquals(nameof(packLine.JL_ActualWeight), actualWeight, packLine.JL_ActualWeight);
			AssertEquals(nameof(packLine.JL_ActualWeightUQ), actualWeightUQ, packLine.JL_ActualWeightUQ);
			AssertEquals(nameof(packLine.JL_ActualVolume), actualVolume, packLine.JL_ActualVolume);
			AssertEquals(nameof(packLine.JL_ActualVolumeUQ), actualVolumeUQ, packLine.JL_ActualVolumeUQ);
			AssertEquals(nameof(packLine.JL_RH_NKCommodityCode), commodityCode, packLine.JL_RH_NKCommodityCode);
		}

		class DummyContainer : CommonContainer
		{
			public DummyContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override IContainerPenaltyCalculateHandler[] ContainerPenaltyCalculateHandlers => new IContainerPenaltyCalculateHandler[] { Handler };

			public IContainerPenaltyCalculateHandler Handler { get; set; }
		}

		#region TestZSaveExceptionContainsRequiredMessages_WhenLastKnownTransitWarehouseAdrressInvalid

		public void TestZSaveExceptionContainsRequiredMessages_WhenLastKnownTransitWarehouseAdrressInvalid()
		{
			// Arrange
			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			var pack1 = shipment.OuterPackLines.AddNew();
			pack1.JL_OA_LastKnownTransitWarehouseAddress = Guid.NewGuid();

			// Act
			var message = string.Empty;
			try
			{
				Factory.Save();
				Fail("ZSaveException should have been thrown");
			}
			catch (ZSaveException ex)
			{
				message = ex.Message;
			}

			// Assert
			AssertContains("Contains additional info", "Business object additional info: LastKnownTransitWarehouseAddress is invalid:", message);
			AssertContains("Contains Inner Message", "Inner Message = The INSERT statement conflicted with the FOREIGN KEY constraint", message);
		}

		#endregion

		#region Implementation

			CommonShipment Shipment;
		PackLine PackLine;

		JobSailing CreateNewSailing(bool import)
		{
			ZString domesticPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			ZString foreignPort;

			if (GlbBranch.CurrentBranch.HomePort.RL_RN_NKCountryCode == Constants.CountryCodes.Singapore)
			{
				foreignPort = "AUBNE";
			}
			else
			{
				foreignPort = "SGSIN";
			}

			if (import)
			{
				return CreateNewSailing(foreignPort, domesticPort);
			}
			else
			{
				return CreateNewSailing(domesticPort, foreignPort);
			}
		}

		JobSailing CreateNewSailing(ZString portOfLoading, ZString portOfDischarge)
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "123";

			VoyageOrigin origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = portOfLoading;
			origin.JA_E_DEP = ZDateTime.Today.AddDays(-10);
			voyage.Origins.Add(origin);

			VoyageDestination destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = portOfDischarge;
			destination.JB_E_ARV = ZDateTime.Today;
			voyage.Destinations.Add(destination);

			voyage.GenerateSailings();
			return voyage.Sailings[0];
		}

		protected override void SetUp()
		{
			base.SetUp();

			FreightTestHelper.TryRemovePackLineIdSequence();
			Shipment = CommonShipment.New(Factory);
			PackLine = Shipment.OuterPackLines.AddNew();

			PackLine.JL_JS = Shipment.PK;
		}

		protected override void TearDown()
		{
			base.TearDown();

			FreightTestHelper.TryRemovePackLineIdSequence();
		}

		int ContainersCountChangedHitCountAdded;
		int ContainersCountChangedHitCountRemoved;

		void Containers_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				ContainersCountChangedHitCountAdded++;
			}
			else
			{
				ContainersCountChangedHitCountRemoved++;
			}
		}

		#endregion
	}
}
