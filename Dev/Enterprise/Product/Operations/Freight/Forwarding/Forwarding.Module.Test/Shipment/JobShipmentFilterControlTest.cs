using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	public class JobShipmentFilterControlTest : BaseFreightTest
	{
		[RequiresSTA]
		public void TestHoldReasonColumn()
		{
			var shipments = new ShipmentCollection(Factory);
			var filterBO = new JobShipmentFilterBusinessObject();

			using (var filterControl = new JobShipmentFilterControl(shipments, filterBO))
			{
				var holdReason = nameof(ForwardingShipment.Job) + "+" + nameof(ForwardingShipment.Job.JH_HoldReason);
				var columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
						.Cast<ZGridColumnInfo>()
						.Any(col => col.ColumnName == holdReason && !col.IsVisible);

				Assert("Hold Reason column should exist and should NOT be visible.", columnExistsAndNotVisible);
			}
		}

		[RequiresSTA]
		public void TestProfitLossReasonColumn()
		{
			using (var filterControl = new JobShipmentFilterControl(new ShipmentCollection(Factory), new JobShipmentFilterBusinessObject()))
			{
				var columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
					.Cast<ZGridColumnInfo>()
					.Any(c => c.ColumnName == "Job+JH_ProfitLossReasonCode" && !c.IsVisible);

				Assert("Profit/Loss Reason column should exist and should NOT be visible", columnExistsAndNotVisible);
			}
		}

		[RequiresSTA]
		public void TestMarginColumn()
		{
			using (var filterControl = new JobShipmentFilterControl(new ShipmentCollection(Factory), new JobShipmentFilterBusinessObject()))
			{
				var columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
					.Cast<ZGridColumnInfo>()
					.Any(c => c.ColumnName == "Job+JH_TotalProfitRevenueMargin" && !c.IsVisible);

				Assert("Margin% column should exist and should NOT be visible", columnExistsAndNotVisible);
			}
		}

		[RequiresSTA]
		public void TestFMCTariffIdColumn()
		{
			var shipments = new ShipmentCollection(Factory);
			var filterBO = new JobShipmentFilterBusinessObject();

			using (var filterControl = new JobShipmentFilterControl(shipments, filterBO))
			{
				var columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
						.Cast<ZGridColumnInfo>()
						.Any((c) => c.ColumnName == "JS_FMCTariffID" && !c.IsVisible);

				Assert("FMC Tariff ID column should exist and should NOT be visible", columnExistsAndNotVisible);
			}
		}

		[RequiresSTA]
		public void TestRateCommodityColumn()
		{
			var shipments = new ShipmentCollection(Factory);
			var filterBO = new JobShipmentFilterBusinessObject();

			using (var filterControl = new JobShipmentFilterControl(shipments, filterBO))
			{
				var columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
						.Cast<ZGridColumnInfo>()
						.Any((c) => c.ColumnName == "JS_RH_NKRateCommodity" && !c.IsVisible);

				Assert("Rate Commodity column should exist and should NOT be visible", columnExistsAndNotVisible);
			}
		}

		[RequiresSTA]
		public void TestCompanyTariffLevelOverrideColumn()
		{
			var shipments = new ShipmentCollection(Factory);
			var filterBO = new JobShipmentFilterBusinessObject();

			using (var filterControl = new JobShipmentFilterControl(shipments, filterBO))
			{
				var columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
						.Cast<ZGridColumnInfo>()
						.Any((c) => c.ColumnName == "JS_CompanyTariffLevelOverride" && !c.IsVisible);

				Assert("Company Tariff Level Override column should exist and should NOT be visible", columnExistsAndNotVisible);
			}
		}

		[RequiresSTA]
		public void TestRemoveImportColumnExceptForAU()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				ShipmentCollection shipments = new ShipmentCollection(Factory);
				JobShipmentFilterBusinessObject filterBO = new JobShipmentFilterBusinessObject();
				using (JobShipmentFilterControl filterControl = new JobShipmentFilterControl(shipments, filterBO))
				{
					bool foundImport = false;
					ZForm form = new ZForm();
					form.Controls.Add(filterControl);
					form.Show();
					foreach (ZGridColumnInfo column in filterControl.FilteredGrid.ColumnStyles)
					{
						if (column.ColumnName == "JS_Calc_ImportManifestStatus")
						{
							foundImport = true;
						}
					}
					form.Dispose();
					Assert("Column Import should exist", foundImport);
				}
			}
		}

		[RequiresSTA]
		public void TestShipmentFilter_IsEuropeanUnionOrCTCountry()
		{
			var shipments = new ShipmentCollection(Factory);
			var filterBO = new JobShipmentFilterBusinessObject();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Denmark))
			using (var filterControl = new JobShipmentFilterControl(shipments, filterBO))
			{
				var zDropEditColumn = filterControl.FilteredGrid.ColumnStyles.ToArray()
					.OfType<ZDropEditColumnStyleInfo>().FirstOrDefault(column => column.ColumnName == "JS_CommunityTransitStatus");

				AssertNotNull("If country is part of EU, CT Status column should be available", zDropEditColumn);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Switzerland))
			using (var filterControl = new JobShipmentFilterControl(shipments, filterBO))
			{
				var zDropEditColumn = filterControl.FilteredGrid.ColumnStyles.ToArray()
					.OfType<ZDropEditColumnStyleInfo>().FirstOrDefault(column => column.ColumnName == "JS_CommunityTransitStatus");

				AssertNotNull("If country is part of Communitry Transit not EU Countries, CT Status column should be available", zDropEditColumn);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			using (var filterControl = new JobShipmentFilterControl(shipments, filterBO))
			{
				var zDropEditColumn = filterControl.FilteredGrid.ColumnStyles.ToArray()
					.OfType<ZDropEditColumnStyleInfo>().FirstOrDefault(column => column.ColumnName == "JS_CommunityTransitStatus");

				AssertNull("If country is NOT part of EU nor Community Transit, CT Status column should not be available", zDropEditColumn);
			}
		}

		[RequiresSTA]
		public void TestColumnsWithUnits_ShouldNotOverriteDefaultDecimals()
		{
			ShipmentCollection shipments = new ShipmentCollection(Factory);
			JobShipmentFilterBusinessObject filterBO = new JobShipmentFilterBusinessObject();

			using (JobShipmentFilterControl filterControl = new JobShipmentFilterControl(shipments, filterBO))
			{
				CombineAssertions(() =>
				{
					foreach (ZGridColumnInfo column in filterControl.FilteredGrid.ColumnStyles)
					{
						if
						(
						column.ColumnName == "JS_ActualWeight"
						|| column.ColumnName == "JS_ActualVolume"
						|| column.ColumnName == "JS_ActualChargeable"
						)
						{
							var calcColumn = (ZArchitecture.ZCalcEditColumnStyleInfo)column;
							bool decimalsOverridden = (bool)calcColumn.GetType().GetProperty("DecimalsOverridden", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(calcColumn, null);

							AssertEquals("Decimals should not be overridden", false, decimalsOverridden);
						}
					}
				});
			}
		}

		[RequiresSTA]
		public void TestWorkflowFilterStripIsInherited()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				ShipmentCollection shipments = new ShipmentCollection(Factory);
				JobShipmentFilterBusinessObject filterBO = new JobShipmentFilterBusinessObject();
				using (ZForm form = new ZForm())
				{
					JobShipmentFilterControl filterControl = new JobShipmentFilterControl(shipments, filterBO);
					form.Controls.Add(filterControl);
					form.Show();
					Application.DoEvents();

					filterControl.AddNewFilterStrip();
					AssertEquals("Must return JobShipmentModuleStrip so that workflow filter strips may be selected", typeof(JobShipmentModuleStrip), filterControl.LastFilterStripType);
					AssertEquals("JobShipmentModuleStrip must inherit ZFilterStripControl<WorkflowFilterStripWithRoutingSupport> so that workflow filter strips may be selected", true, typeof(JobShipmentModuleStrip).IsSubclassOf(typeof(WorkflowFilterStripWithRoutingSupport)));
				}
			}
		}

		[RequiresSTA]
		public void TestConsignorTerminology()
		{
			FreightDataRegistry.Instance.ConsignorShipperTerminology.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, FreightDataRegistry.Instance.ConsignorShipperTerminology.DefaultValue);
			AssertDataGridText(FreightDataRegistry.Instance.ConsignorShipperTerminology.DefaultValue);

			FreightDataRegistry.Instance.ConsignorShipperTerminology.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TestTestTest");
			AssertDataGridText("TestTestTest");
		}

		[RequiresSTA]
		public void TestSGColumns()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				ShipmentCollection shipments = new ShipmentCollection(Factory);
				JobShipmentFilterBusinessObject filterBO = new JobShipmentFilterBusinessObject();
				using (JobShipmentFilterControl filterControl = new JobShipmentFilterControl(shipments, filterBO))
				{
					bool foundBroker = false;
					ZForm form = new ZForm();
					form.Controls.Add(filterControl);
					form.Show();
					foreach (ZGridColumnInfo column in filterControl.FilteredGrid.ColumnStyles)
					{
						if (column.ColumnName == "CustomsEntryType")
						{
							Assert("US Column CustomsEntryType should not exist", false);
						}
						if (column.ColumnName == "ITEntryType")
						{
							Assert("US Column ITEntryType should not exist", false);
						}
						if (column.ColumnName == "CustomsEntryNumberType")
						{
							Assert("Column CustomsEntryNumberType should not exist", true);
						}
						if (column.ColumnName == "CMRCustomsStatus")
						{
							Assert("Column CMRCustomsStatus should not exist", true);
						}
						if (column.ColumnName == "CMRMessageStatus")
						{
							Assert("Column CMRMessageStatus should not exist", true);
						}
						if (column.ColumnName == "CustomsBroker")
						{
							foundBroker = true;
						}
						form.Dispose();
					}
					Assert("Column Customs Broker should exist", foundBroker);
				}
			}
		}

		[RequiresSTA]
		public void TestUSColumns()
		{
			foreach (var country in new[] { Core.Constants.CountryCodes.PuertoRico, Core.Constants.CountryCodes.UnitedStates })
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				{
					ShipmentCollection shipments = new ShipmentCollection(Factory);
					JobShipmentFilterBusinessObject filterBO = new JobShipmentFilterBusinessObject();
					using (JobShipmentFilterControl filterControl = new JobShipmentFilterControl(shipments, filterBO))
					{
						bool foundEntryType = false;
						bool foundITType = false;
						bool foundEntryStatusDesc = false;
						bool foundEntryNumberStatus = false;
						bool foundImport = false;
						ZForm form = new ZForm();
						form.Controls.Add(filterControl);
						form.Show();
						foreach (ZGridColumnInfo column in filterControl.FilteredGrid.ColumnStyles)
						{
							if (column.ColumnName == "CustomsEntryType")
							{
								foundEntryType = true;
							}
							if (column.ColumnName == "ITEntryType")
							{
								foundITType = true;
							}
							if (column.ColumnName == "EntryStatusDescription")
							{
								foundEntryStatusDesc = true;
							}
							if (column.ColumnName == "EntryNumberStatus")
							{
								foundEntryNumberStatus = true;
							}
							if (column.ColumnName == "EntryNumberStatus")
							{
								foundImport = true;
							}
							if (column.ColumnName == "CMRCustomsStatus")
							{
								AssertEquals("US Caption should be CRL Status Desc", "CRL Status Desc.", column.Caption);
							}
							if (column.ColumnName == "CMRMessageStatus")
							{
								AssertEquals("US Caption should be ENS Status Desc", "ENS Status Desc.", column.Caption);
							}
							form.Dispose();
						}
						Assert("Column Customs Type column should be visible", foundEntryType);
						Assert("Column Customs IT Type should exist", foundITType);
						Assert("Column Import Type should NOT exist", !foundImport);
						AssertEquals("Column Customs Status Desc should NOT be visible for US", false, foundEntryStatusDesc);
						AssertEquals("Column Customs Entry Status should NOT be visible for US", false, foundEntryNumberStatus);
					}
				}
			}
		}

		[RequiresSTA]
		public void TestChargesApply()
		{
			var shipments = new ShipmentCollection(Factory);
			var filterBO = new JobShipmentFilterBusinessObject();

			using (var filterControl = new JobShipmentFilterControl(shipments, filterBO))
			{
				bool chargesApplyFoundAndInvisible = filterControl.FilteredGrid.ColumnStyles
					.Cast<ZGridColumnInfo>()
					.Any((c) => c.ColumnName == "JS_HBLAWBChargesDisplay" && !c.IsVisible);

				Assert("Charges Apply column should exist and should NOT be visible", chargesApplyFoundAndInvisible);
			}
		}

		[RequiresSTA]
		public void TestPickupAndDeliveryAddressColumns()
		{
			var shipments = new ShipmentCollection(Factory);
			var filterBO = new JobShipmentFilterBusinessObject();
			var columns = new string[]
			{
				"ConsignorPickupAddress+E2_CompanyName",
				"ConsignorPickupAddress+AddressAsASingleLine",
				"ConsignorPickupAddress+E2_Address1",
				"ConsignorPickupAddress+E2_Address2",
				"ConsignorPickupAddress+E2_City",
				"ConsignorPickupAddress+E2_State",
				"ConsignorPickupAddress+E2_RN_NKCountryCode",
				"ConsignorPickupAddress+E2_Postcode",
				"ConsigneeDeliveryAddress+E2_CompanyName",
				"ConsigneeDeliveryAddress+AddressAsASingleLine",
				"ConsigneeDeliveryAddress+E2_Address1",
				"ConsigneeDeliveryAddress+E2_Address2",
				"ConsigneeDeliveryAddress+E2_City",
				"ConsigneeDeliveryAddress+E2_State",
				"ConsigneeDeliveryAddress+E2_RN_NKCountryCode",
				"ConsigneeDeliveryAddress+E2_Postcode"
			};

			using (var filterControl = new JobShipmentFilterControl(shipments, filterBO))
			{
				foreach (string columnName in columns)
				{
					bool columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
						.Cast<ZGridColumnInfo>()
						.Any((c) => c.ColumnName == columnName && !c.IsVisible);

					Assert(string.Format("{0} column should exist and should NOT be visible", columnName), columnExistsAndNotVisible);
				}
			}
		}

		[RequiresSTA]
		public void TestPickupDeliveryDropModeColumns()
		{
			var shipments = new ShipmentCollection(Factory);
			var filterBO = new JobShipmentFilterBusinessObject();

			using (var filterControl = new JobShipmentFilterControl(shipments, filterBO))
			{
				bool pickupDropModeColumnFoundAndInvisible = filterControl.FilteredGrid.ColumnStyles
					.Cast<ZGridColumnInfo>()
					.Any((c) => c.ColumnName == "DocsAndCartage+JP_FCLPickupEquipmentNeeded" && !c.IsVisible);

				Assert("Pickup Drop Mode column should exist and should NOT be visible", pickupDropModeColumnFoundAndInvisible);

				bool deliveryDropModeColumnFoundAndInvisible = filterControl.FilteredGrid.ColumnStyles
					.Cast<ZGridColumnInfo>()
					.Any((c) => c.ColumnName == "DocsAndCartage+JP_FCLDeliveryEquipmentNeeded" && !c.IsVisible);

				Assert("Delivery Drop Mode column should exist and should NOT be visible", deliveryDropModeColumnFoundAndInvisible);
			}
		}

		[RequiresSTA]
		public void TestAdditionalReferenceColumn()
		{
			var shipments = new ShipmentCollection(Factory);
			var filterBO = new JobShipmentFilterBusinessObject();

			using (var filterControl = new JobShipmentFilterControl(shipments, filterBO))
			{
				bool columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
						.Cast<ZGridColumnInfo>()
						.Any((c) => c.ColumnName == "NumbersAsString" && !c.IsVisible);

				Assert("Additional Reference column should exist and should NOT be visible", columnExistsAndNotVisible);
			}
		}

		[RequiresSTA]
		public void TestConsignorAddressDetails()
		{
			var shipments = new ShipmentCollection(Factory);
			var filterBO = new JobShipmentFilterBusinessObject();

			using (var filterControl = new JobShipmentFilterControl(shipments, filterBO))
			{
				var consignorAddressDetailsColumns = filterControl.FilteredGrid.ColumnStyles
					.Cast<ZGridColumnInfo>()
					.Where(c => c.GroupName.Caption == "Consignor Address Details")
					.Select(s => s.ColumnName);

				AssertEquals(6, consignorAddressDetailsColumns.Count());
				AssertCollectionContains("ConsignorDocumentaryAddress+E2_AddressOverride", "ConsignorDocumentaryAddress+E2_AddressOverride", consignorAddressDetailsColumns);
				AssertCollectionContains("ConsignorDocumentaryAddress+E2_Address1", "ConsignorDocumentaryAddress+E2_Address1", consignorAddressDetailsColumns);
				AssertCollectionContains("ConsignorDocumentaryAddress+E2_Address2", "ConsignorDocumentaryAddress+E2_Address2", consignorAddressDetailsColumns);
				AssertCollectionContains("ConsignorDocumentaryAddress+E2_City", "ConsignorDocumentaryAddress+E2_City", consignorAddressDetailsColumns);
				AssertCollectionContains("ConsignorDocumentaryAddress+E2_State", "ConsignorDocumentaryAddress+E2_State", consignorAddressDetailsColumns);
				AssertCollectionContains("ConsignorDocumentaryAddress+E2_RN_NKCountryCode", "ConsignorDocumentaryAddress+E2_RN_NKCountryCode", consignorAddressDetailsColumns);
			}
		}

		[RequiresSTA]
		public void TestCommodityCodes()
		{
			var shipments = new ShipmentCollection(Factory);
			var filterBO = new JobShipmentFilterBusinessObject();

			using (var filterControl = new JobShipmentFilterControl(shipments, filterBO))
			{
				bool columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
						.Cast<ZGridColumnInfo>()
						.Any((c) => c.ColumnName == "CommodityCodes" && !c.IsVisible);

				Assert("CommunityTransitStatus column should exist and should NOT be visible", columnExistsAndNotVisible);
			}
		}

		[RequiresSTA]
		public void TestCTStatusColumn()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				var shipments = new ShipmentCollection(Factory);
				var filterBO = new JobShipmentFilterBusinessObject();

				using (var filterControl = new JobShipmentFilterControl(shipments, filterBO))
				{
					bool columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
							.Cast<ZGridColumnInfo>()
							.Any((c) => c.ColumnName == "JS_CommunityTransitStatus" && !c.IsVisible);

					Assert("CommunityTransitStatus column should exist and should NOT be visible", columnExistsAndNotVisible);
				}
			}
		}

		[RequiresSTA]
		public void TestExitStatusColumn()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				var shipments = new ShipmentCollection(Factory);
				var filterBO = new JobShipmentFilterBusinessObject();

				using (var filterControl = new JobShipmentFilterControl(shipments, filterBO))
				{
					bool columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
						.Cast<ZGridColumnInfo>()
						.Any((c) => c.ColumnName == "ExitStatus" && !c.IsVisible);

					Assert("Exit Status column should exist and should NOT be visible", columnExistsAndNotVisible);
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("TR"))
			{
				var shipments = new ShipmentCollection(Factory);
				var filterBO = new JobShipmentFilterBusinessObject();

				using (var filterControl = new JobShipmentFilterControl(shipments, filterBO))
				{
					bool columnExists = filterControl.FilteredGrid.ColumnStyles
						.Cast<ZGridColumnInfo>()
						.Any((c) => c.ColumnName == "ExitStatus");

					Assert("Exit Status column should not exist", !columnExists);
				}
			}
		}

		[RequiresSTA]
		public void TestExitStatusDescriptionColumn()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				var shipments = new ShipmentCollection(Factory);
				var filterBO = new JobShipmentFilterBusinessObject();

				using (var filterControl = new JobShipmentFilterControl(shipments, filterBO))
				{
					bool columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
						.Cast<ZGridColumnInfo>()
						.Any((c) => c.ColumnName == "ExitStatusDescription" && !c.IsVisible);

					Assert("Exit Status Description column should exist and should NOT be visible", columnExistsAndNotVisible);
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("TR"))
			{
				var shipments = new ShipmentCollection(Factory);
				var filterBO = new JobShipmentFilterBusinessObject();

				using (var filterControl = new JobShipmentFilterControl(shipments, filterBO))
				{
					bool columnExists = filterControl.FilteredGrid.ColumnStyles
						.Cast<ZGridColumnInfo>()
						.Any((c) => c.ColumnName == "ExitStatusDescription");

					Assert("Exit Status Description column should not exist", !columnExists);
				}
			}
		}

		[RequiresSTA]
		public void TestCO2eColumnExistInShipmentGridWhenGreenhouseGasEmissionCalculationEnabled()
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			{
				var shipments = new ShipmentCollection(Factory);
				var filterBO = new JobShipmentFilterBusinessObject();

				using (var filterControl = new JobShipmentFilterControl(shipments, filterBO))
				{
					var columnInfo = filterControl.FilteredGrid.ColumnStyles
							.Cast<ZGridColumnInfo>()
							.FirstOrDefault((c) => c.ColumnName == "TotalCO2eForSorting");

					AssertNotNull("TotalCO2e column should exist", columnInfo);
					AssertEquals("TotalCO2e column caption", "CO2e (kg)", columnInfo.CaptionResourceString.Caption);
					AssertEquals("TotalCO2e column default visibility is false", false, columnInfo.IsVisible);
					AssertEquals("TotalCO2e column is Upper case", System.Windows.Forms.CharacterCasing.Upper, columnInfo.CharacterCasing);
				}
			}

			using (CO2eTestHelper.MockCO2eFeatureControl(false))
			{
				var shipments = new ShipmentCollection(Factory);
				var filterBO = new JobShipmentFilterBusinessObject();

				using (var filterControl = new JobShipmentFilterControl(shipments, filterBO))
				{
					bool columnExists = filterControl.FilteredGrid.ColumnStyles
							.Cast<ZGridColumnInfo>()
							.Any((c) => c.ColumnName == "TotalCO2eForSorting");

					Assert("CO2e (kg) column should not exist", !columnExists);
				}
			}
		}

		[RequiresSTA]
		public void TestCO2eStatusColumnExistInShipmentGridWhenGreenhouseGasEmissionCalculationEnabled()
		{
			using (CO2eTestHelper.MockCO2eFeatureControl(true))
			{
				var shipments = new ShipmentCollection(Factory);
				var filterBO = new JobShipmentFilterBusinessObject();

				using (var filterControl = new JobShipmentFilterControl(shipments, filterBO))
				{
					var columnInfo = filterControl.FilteredGrid.ColumnStyles
							.Cast<ZGridColumnInfo>()
							.FirstOrDefault((c) => c.ColumnName == "CO2eStatus");

					AssertNotNull("CO2 Status column should exist", columnInfo);
					AssertEquals("CO2 Status column caption", "CO2e Status", columnInfo.CaptionResourceString.Caption);
					AssertEquals("CO2 Status column default visibility is false", false, columnInfo.IsVisible);
					AssertEquals("CO2 Status column is Upper case", System.Windows.Forms.CharacterCasing.Upper, columnInfo.CharacterCasing);
				}
			}

			using (CO2eTestHelper.MockCO2eFeatureControl(false))
			{
				var shipments = new ShipmentCollection(Factory);
				var filterBO = new JobShipmentFilterBusinessObject();

				using (var filterControl = new JobShipmentFilterControl(shipments, filterBO))
				{
					bool columnExists = filterControl.FilteredGrid.ColumnStyles
							.Cast<ZGridColumnInfo>()
							.Any((c) => c.ColumnName == "CO2eStatus");

					Assert("CO2 Status column should not exist", !columnExists);
				}
			}
		}

		[RequiresSTA]
		public void TestLastKnownTransitWarehouseStatusColumn()
		{
			var shipments = new ShipmentCollection(Factory);
			var filterBO = new JobShipmentFilterBusinessObject();

			using (var filterControl = new JobShipmentFilterControl(shipments, filterBO))
			{
				bool columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
						.Cast<ZGridColumnInfo>()
						.Any((c) => c.ColumnName == "JS_Calc_LastKnownTransitWarehouseStatus" && !c.IsVisible);

				Assert("Last Known Transit Warehouse Status column should exist and should NOT be visible", columnExistsAndNotVisible);
			}
		}

		[RequiresSTA]
		public void TestViewMeasurementForm()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			var shipments = new ShipmentCollection(Factory);

			Func<CommonShipment> createShipmentWithPackLine = () =>
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.ConsigneePK = organization.PK;
				shipment.ConsignorPK = organization.PK;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";
				var packline = shipment.OuterPackLines.AddNew();
				packline.JL_PackageCount = 1;
				packline.JL_F3_NKPackType = "CTN";
				packline.JL_ActualWeight = 500.1;
				packline.JL_ActualWeightUQ = "KG";
				packline.JL_ActualVolume = 6;
				packline.JL_ActualVolumeUQ = "M3";
				packline.JL_Length = 1;
				packline.JL_Width = 2;
				packline.JL_Height = 3;
				packline.JL_UnitOfDimension = "M";
				shipments.Add(shipment);
				return shipment;
			};

			var shipment1 = createShipmentWithPackLine();
			shipment1.JS_UniqueConsignRef = "S01";
			var shipment2 = createShipmentWithPackLine();
			shipment2.JS_UniqueConsignRef = "S02";

			var filterBO = new JobShipmentFilterBusinessObject();

			using (ZForm form = new ZForm())
			using (ZFormModaliser.SuspendDispose())
			{
				var filterControl = new JobShipmentFilterControl(shipments, filterBO);
				form.Controls.Add(filterControl);
				form.Show();
				var viewMeasurementMenuItem = filterControl.FilteredGrid.ContextMenu.MenuItems.FindByText("View Measurements");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				viewMeasurementMenuItem.PerformClick();
				AssertEquals("Please select a shipment.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				filterControl.FilteredGrid.Select(0);
				filterControl.FilteredGrid.Select(1);
				viewMeasurementMenuItem.PerformClick();

				var lastForm = ZFormModaliser.LastFormShownDialogForTest;
				AssertEquals(typeof(ShipmentViewMeasurementsForm), lastForm?.GetType());

				var shipmentPopup = lastForm.GetField("TextBox") as RichTextBox;
				var expectedMessage = @"S01
     1 CTN                       1.00 x 2.00 x 3.00 M     500.10 KG       6.000 M3

S02
     1 CTN                       1.00 x 2.00 x 3.00 M     500.10 KG       6.000 M3

Totals: 2 CTN                                             1000.20 KG      12.000 M3";
				AssertMultilineASCIIEquals(expectedMessage, shipmentPopup?.Text ?? string.Empty);
			}
		}

		[RequiresSTA]
		public void TestViewShipmentTrackingMenuItemExists()
		{
			TestCase(true);
			TestCase(false);

			void TestCase(bool isActive)
			{
				var filterBO = new JobShipmentFilterBusinessObject();
				var shipments = new ShipmentCollection(Factory);
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipments.Add(shipment);

				using (FreightDataRegistry.Instance.GlobalTrackingShipmentVisibility.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new GlobalTrackingShipmentVisibilityOptions { IsActive = isActive }))
				using (var form = new ZForm())
				using (ZFormModaliser.SuspendDispose())
				{
					var filterControl = new JobShipmentFilterControl(shipments, filterBO);
					form.Controls.Add(filterControl);
					form.Show();
					var viewShipmentTrackingMenuItem = filterControl.FilteredGrid.ContextMenu.MenuItems.FindByText("Shipment Visibility");

					if (isActive)
					{
						AssertNotNull(viewShipmentTrackingMenuItem);
					}
					else
					{
						AssertNull(viewShipmentTrackingMenuItem);
					}
				}
			}
		}

		[RequiresSTA]
		public void TestViewShipmentTracking()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/portals");

			var filterBO = new JobShipmentFilterBusinessObject();
			var shipments = new ShipmentCollection(Factory);
			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "S01";
			shipments.Add(shipment1);
			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_UniqueConsignRef = "S02";
			shipments.Add(shipment2);

			using (FreightDataRegistry.Instance.GlobalTrackingShipmentVisibility.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new GlobalTrackingShipmentVisibilityOptions { IsActive = true }))
			using (ZForm form = new ZForm())
			using (ZFormModaliser.SuspendDispose())
			{
				var filterControl = new JobShipmentFilterControl(shipments, filterBO);
				form.Controls.Add(filterControl);
				form.Show();
				var viewShipmentTrackingMenuItem = filterControl.FilteredGrid.ContextMenu.MenuItems.FindByText("Shipment Visibility");
				AssertNotNull("Precondition: menu item exists", viewShipmentTrackingMenuItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				viewShipmentTrackingMenuItem.PerformClick();
				AssertEquals("Please select a shipment.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				filterControl.FilteredGrid.Select(0);
				viewShipmentTrackingMenuItem.PerformClick();
				var launchedUrl = WebUrlLauncher.LastUrlLaunched;
				var expectedUrlPattern = @"https://address/portals/NST/Desktop\?noHeader=true#/tracker\?trackingNumber=S01&sso_otp=\w+";
				Assert(Regex.IsMatch(launchedUrl, expectedUrlPattern));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				filterControl.FilteredGrid.Select(1);
				viewShipmentTrackingMenuItem.PerformClick();
				launchedUrl = WebUrlLauncher.LastUrlLaunched;
				expectedUrlPattern = @"https://address/portals/NST/Desktop\?noHeader=true#/tracker\?trackingNumber=S01&trackingNumber=S02&sso_otp=\w+";
				Assert(Regex.IsMatch(launchedUrl, expectedUrlPattern));
			}
		}

		[RequiresSTA]
		public void TestRevisedDeliveryDueDateColumnAvailability()
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				AssertColumnAvailability("Allow override of Delivery Due Date set to true, Revised Delivery Due Date column should exist", true, "JS_RevisedDeliveryDueDate");
			}
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = false, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				AssertColumnAvailability("Allow override of Delivery Due Date set to false, Revised Delivery Due Date column should not exist", false, "JS_RevisedDeliveryDueDate");
			}
		}

		[RequiresSTA]
		public void TestDeliveryDueDateColumnAvailability()
		{
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				AssertColumnAvailability("Allow override of Delivery Due Date set to true, Delivery Due Date column should exist", true, "JS_DeliveryDueDate");
			}
			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = false, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			{
				AssertColumnAvailability("Allow override of Delivery Due Date set to false, Delivery Due Date column should not exist", false, "JS_DeliveryDueDate");
			}
		}

		[RequiresSTA]
		public void TestReceiptAndDispatchDateColumn()
		{
			AssertColumnAvailability("Receipt Requested (Delivery) column should exist", true, "JS_ImportReleaseDepotReceiptRequested");
			AssertColumnAvailability("Dispatch Requested (Delivery) column should exist", true, "JS_ImportReleaseDepotDispatchRequested");
			AssertColumnAvailability("Receipt Requested (Pickup) column should exist", true, "JS_ExportReceivingDepotReceiptRequested");
			AssertColumnAvailability("Dispatch Requested (Pickup) column should exist", true, "JS_ExportReceivingDepotDispatchRequested");
			AssertColumnAvailability("Interim Receipt Date column should exist", true, "JS_A_RCV");
		}

		[RequiresSTA]
		public void TestScreeningStatusColumn()
		{
			AssertScreeningStatusColumn(enableComplianceRisk: false);
			AssertScreeningStatusColumn(enableComplianceRisk: true);

			void AssertScreeningStatusColumn(bool enableComplianceRisk)
			{
				var shipmentHasImplementedIComplianceRiskStatusProvider = typeof(IComplianceItemRiskStatusProvider).IsAssignableFrom(typeof(ForwardingShipment));

				using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
					ComplianceWiseRegistryHelper.SetValue(enableComplianceRisk)))
				using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableComplianceRisk))
				using (var filterControl = new JobShipmentFilterControl(new ShipmentCollection(Factory), new JobShipmentFilterBusinessObject()))
				{
					var screeningStatusColumnStyle = filterControl.FilteredGrid.GetColumnStyle("JS_ScreeningStatus");

					AssertNotNull(screeningStatusColumnStyle);
				}
			}
		}

		[RequiresSTA]
		public void TestFirstLegPortATAColumn()
		{
			var shipments = new ShipmentCollection(Factory);
			var filterBO = new JobShipmentFilterBusinessObject();

			using (var filterControl = new JobShipmentFilterControl(shipments, filterBO))
			{
				bool columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
						.Cast<ZGridColumnInfo>()
						.Any((c) => c.ColumnName == "DepartureConsol+Transports+DepartureTransport+JW_ATA" && !c.IsVisible);

				Assert("First Leg Port ATA column should exist and should NOT be visible", columnExistsAndNotVisible);
			}
		}

		[RequiresSTA]
		public void TestFirstLegPortETAColumn()
		{
			var shipments = new ShipmentCollection(Factory);
			var filterBO = new JobShipmentFilterBusinessObject();

			using (var filterControl = new JobShipmentFilterControl(shipments, filterBO))
			{
				bool columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
						.Cast<ZGridColumnInfo>()
						.Any((c) => c.ColumnName == "DepartureConsol+Transports+DepartureTransport+JW_ETA" && !c.IsVisible);

				Assert("First Leg Port ETA column should exist and should NOT be visible", columnExistsAndNotVisible);
			}
		}

		void AssertColumnAvailability(string message, bool expectedVisibility, string columnName)
		{
			var shipments = new ShipmentCollection(Factory);
			var filterBO = new JobShipmentFilterBusinessObject();

			using (var filterControl = new JobShipmentFilterControl(shipments, filterBO))
			{
				var columnVisibility = filterControl.FilteredGrid.ColumnStyles
					.Cast<ZGridColumnInfo>()
					.Any(c => c.ColumnName == columnName);

				AssertEquals(message, expectedVisibility, columnVisibility);
			}
		}

		[RequiresSTA]
		public void TestElectronicBillOfLadingColumns()
		{
			AssertElectronicBillOfLadingColumns(false, false);

			AssertElectronicBillOfLadingColumns(true, true);

			void AssertElectronicBillOfLadingColumns(bool enableEBLIntegration, bool columnsExist)
			{
				var shipments = new ShipmentCollection(Factory);
				var filterBO = new JobShipmentFilterBusinessObject();

				var boleroEBLConfiguration = new BoleroEBLConfiguration()
				{
					EnableEBLIntegration = enableEBLIntegration,
					GalileoEndPointUrl = "http://test.test",
					GalileoAudience = Guid.NewGuid().ToString(),
					GalileoTestEndPointUrl = "http://test.test",
					GalileoTestAudience = Guid.NewGuid().ToString()
				};

				using (FreightDataRegistry.Instance.EnableBoleroEHBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, boleroEBLConfiguration))
				using (var filterControl = new JobShipmentFilterControl(shipments, filterBO))
				{
					bool eHBLIdentifierExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
							.Cast<ZGridColumnInfo>()
							.Any((c) => c.ColumnName == "JS_ElectronicBillOfLadingReference" && !c.IsVisible);
					bool eHBLStatusTimeExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
							.Cast<ZGridColumnInfo>()
							.Any((c) => c.ColumnName == "JS_Calc_ElectronicBillOfLadingDate" && !c.IsVisible);
					bool eHBLStatusExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
							.Cast<ZGridColumnInfo>()
							.Any((c) => c.ColumnName == "JS_ElectronicBillOfLadingStatusDescription" && !c.IsVisible);
					bool eHBLTypeExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
							.Cast<ZGridColumnInfo>()
							.Any((c) => c.ColumnName == "JS_ElectronicBillOfLadingTypeDescription" && !c.IsVisible);
					bool eHBLTermsExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
							.Cast<ZGridColumnInfo>()
							.Any((c) => c.ColumnName == "JS_ElectronicBillOfLadingTermsDescription" && !c.IsVisible);

					AssertEquals(columnsExist, eHBLIdentifierExistsAndNotVisible);
					AssertEquals(columnsExist, eHBLStatusTimeExistsAndNotVisible);
					AssertEquals(columnsExist, eHBLStatusExistsAndNotVisible);
					AssertEquals(columnsExist, eHBLTypeExistsAndNotVisible);
					AssertEquals(columnsExist, eHBLTermsExistsAndNotVisible);
				}
			}
		}

		#region Implementation

		CalculateDeliveryDueDateTransportModeCollection ActiveTransportModesForCalculateDeliveryDateOption()
		{
			var activeTransportModes = new CalculateDeliveryDueDateTransportModeCollection();
			activeTransportModes.Add(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Rail, Core.Constants.TransportModeDescriptions.Rail, true);
			return activeTransportModes;
		}

		void AssertDataGridText(string expectedValue)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				ShipmentCollection shipments = new ShipmentCollection(Factory);
				JobShipmentFilterBusinessObject filterBO = new JobShipmentFilterBusinessObject();

				using (JobShipmentFilterControl filterControl = new JobShipmentFilterControl(shipments, filterBO))
				{
					filterControl.Show();
					foreach (ZGridColumnInfo columnInfo in filterControl.FilteredGrid.ColumnStyles)
					{
						if (columnInfo is ZMultiControlColumnStyleInfo)
						{
							if (columnInfo.ColumnName == "ConsignorNameOrPK")
							{
								AssertEquals("Caption should be " + expectedValue, expectedValue, columnInfo.Caption);
							}
							else if (columnInfo.ColumnName == "ConsignorDocumentaryAddress+E2_CompanyName")
							{
								AssertEquals("Caption should be " + expectedValue + " Full Name", expectedValue + " Full Name", columnInfo.Caption);
							}
							else if (columnInfo.ColumnName == "ConsignorDocumentaryAddress+AddressAsASingleLine")
							{
								AssertEquals("Caption should be " + expectedValue + " Address", expectedValue + " Address", columnInfo.Caption);
							}
						}
					}
				}
			}
		}

		#endregion
	}
}
