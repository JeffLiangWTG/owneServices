using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	[TestedType(typeof(RateTransportProviderForm))]
	public class RateTransportProviderFormTest : ZFormBasherTest
	{
		public void TestNoNullReferenceExceptionWhenCurrentFindboxIsNull()
		{
			var provider = Factory.New<RateTransportProvider>();
			using (var form = new RateTransportProviderForm(provider))
			{
				form.Show();
				Application.DoEvents();
				var zoneItemGrid = form.Controls.Find("ZoneItemGrid", true)[0] as ZGrid;
				zoneItemGrid.CurrentCell = new DataGridCell(0, 0);
				AssertNull(form.CurrentZoneItemEdittingFindBox);

				var methodInfo = typeof(RateTransportProviderForm).GetMethod("Collection_SelectionNeeded", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

				AssertNoExceptionThrown(() =>
				{
					methodInfo.Invoke(form, new[] { null, new SelectionNeededEventArgs(ZGuid.Empty) });
				});
			}
		}

		public void TestRelatedPartyFindBoxIsDisplayedProperlyAfterResizing()
		{
			var provider = Factory.New<RateTransportProvider>();
			using (var form = new RateTransportProviderForm(provider))
			{
				form.Show();
				Application.DoEvents();
				var relatedPartyFindBox = form.Controls.Find("TP_OH_RelatedPartyFindBox", true)[0] as MasterFiles.GUI.ZOrganisationFindBox;
				AssertNotNull(relatedPartyFindBox);

				void AssertFindBoxIsDisplayedProperly(string message)
				{
					Assert($"FindBox is visible - {message}", relatedPartyFindBox.Visible);
					Assert($"FindBox height > 0 - {message}", relatedPartyFindBox.Height > 0);
					Assert($"FindBox width > 0 - {message}", relatedPartyFindBox.Width > 0);
				}

				AssertFindBoxIsDisplayedProperly("Original Size");
				form.WindowState = FormWindowState.Maximized;
				AssertFindBoxIsDisplayedProperly("Maximized Size");
				form.WindowState = FormWindowState.Normal;
				AssertFindBoxIsDisplayedProperly("After restored to Original Size");
			}
		}

		public void TestCurrentZoneItemEditingFindBox()
		{
			var provider = Factory.New<RateTransportProvider>();
			using (var form = new RateTransportProviderForm(provider))
			{
				provider.TP_RN_NKCountry = "US";
				provider.Zones.AddNew().TZ_ZoneName = "TESTZONE";
				form.Show();
				Application.DoEvents();
				var zoneItemGrid = form.Controls.Find("ZoneItemGrid", true)[0] as ZGrid;
				AssertNotNull(zoneItemGrid);
				zoneItemGrid.Focus();
				AssertGreaterThan(zoneItemGrid.TableStyles.Count, 0);
				AssertNotNull(zoneItemGrid.TableStyles[0].GridColumnStyles);
				AssertGreaterThan(zoneItemGrid.TableStyles[0].GridColumnStyles.Count, 0);

				var columnStyle = zoneItemGrid.TableStyles[0].GridColumnStyles[RateTransportZoneItemSchema.TQ_R9_CityTown.Name] as ZMultiControlColumnStyle;
				AssertNotNull(columnStyle);
				zoneItemGrid.CurrentCell = new DataGridCell(0, zoneItemGrid.TableStyles[0].GridColumnStyles.IndexOf(columnStyle));
				Application.DoEvents();
				AssertNotNull(columnStyle.EditControl);
				AssertGreaterThan(columnStyle.EditControl.Controls.Count, 0);
				var findBox = columnStyle.EditControl.Controls[0];
				Assert(findBox.GetType().Name, findBox is ZGridGuidFindBox);

				columnStyle = zoneItemGrid.TableStyles[0].GridColumnStyles[RateTransportZoneItemSchema.TQ_FromPostCode.Name] as ZMultiControlColumnStyle;
				AssertNotNull(columnStyle);
				zoneItemGrid.CurrentCell = new DataGridCell(0, zoneItemGrid.TableStyles[0].GridColumnStyles.IndexOf(columnStyle));
				Application.DoEvents();
				AssertNotNull(columnStyle.EditControl);
				AssertGreaterThan(columnStyle.EditControl.Controls.Count, 0);
				findBox = columnStyle.EditControl.Controls[0];
				Assert(findBox.GetType().Name, findBox is ZGridFindBox);
			}
		}

		public void TestProgressFormStyle()
		{
			var provider = Factory.New<RateTransportProvider>();
			using (var form = new RateTransportProviderForm(provider))
			{
				var progressForm = form.GetType().GetProperty("ProgressForm", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(form) as ProgressForm;
				AssertNotNull(progressForm);
				Assert(!progressForm.ShowCancelButton);
				Assert(!progressForm.ShowInTaskbar);
				Assert(!progressForm.ShowProgressBar);
			}
		}

		public void TestCollectionsSubscribedEvents()
		{
			var provider = Factory.New<RateTransportProvider>();
			using (var form = new RateTransportProviderForm(provider))
			{
				provider.TP_RN_NKCountry = "US";
				provider.Zones.AddNew().TZ_ZoneName = "TESTZONE";
				form.Show();
				Application.DoEvents();
				var zoneItemGrid = form.Controls.Find("ZoneItemGrid", true)[0] as ZGrid;
				zoneItemGrid.PerformMouseDownForTest(0, 1);
				var item = zoneItemGrid.ListManager.GetCurrent() as RateTransportZoneItem;
				Assert(item.Lookups.CityTowns.AreEventsSubscribed);
				Assert(item.Lookups.PostCodes.AreEventsSubscribed);
			}
		}

		public void TestImportData_MustEnterCountryCode()
		{
			var provider = Factory.New<RateTransportProvider>();
			var zone = Factory.New<RateTransportZone>();
			zone.TZ_TP = provider.PK;

			using (var form = new RateTransportProviderForm(provider))
			{
				form.Show();
				Application.DoEvents();
				var zoneItemGrid = form.Controls.Find("ZoneItemGrid", true)[0] as RateTransportZoneItemsGrid;
				zoneItemGrid.CurrentCell = new DataGridCell(0, 0);
				var menu = zoneItemGrid.ContextMenu.MenuItems.FindByText("&Import Data...");
				AssertNotNull(menu);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.PerformClick();
				AssertEquals("Please enter a valid Zone Country/Region before importing data.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				provider.TP_RN_NKCountry = "1";
				menu.PerformClick();
				AssertEquals("Please enter a valid Zone Country/Region before importing data.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				provider.TP_RN_NKCountry = Core.Constants.CountryCodes.Australia;
				menu.PerformClick();
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should be showing the right form.", typeof(ZArchitecture.GUI.DataMapping.DataImportWizardForm), ZFormModaliser.ActiveForm.GetType());
			}
		}

		public void TestPostCode_WhenDoesNotExistInDatabase_GetFromWeb()
		{
			var provider = Factory.New<RateTransportProvider>();
			provider.TP_RN_NKCountry = Core.Constants.CountryCodes.Indonesia;
			var zone = provider.Zones.AddNew();
			var zoneItem = zone.Items.AddNew();

			var refCityTown = Factory.New<RefCityTown>();
			refCityTown.R9_InternationalName = "Banjarmasin";
			refCityTown.R9_RN_NKCountry = provider.TP_RN_NKCountry;

			zoneItem.TQ_R9_CityTown = refCityTown.PK;

			Factory.Save();

			var postCode = new ZString("70112");
			var postCodeQuery = new ZQuery(new ZQuery(RefPostCodeSchema.RK_RN_NKCountry, provider.TP_RN_NKCountry)
				.AddToFilter(RefPostCodeSchema.RK_CityTownPostCode, postCode));
			var refPostCode = Factory.LoadTop1<RefPostCode>(postCodeQuery);

			CombineAssertions("Pre-condition", () =>
			{
				AssertNull("RefPostCode does not exist", refPostCode);
				Assert("RefCityPCodePivot does not exist", !Factory.Exists(typeof(RefCityPCodePivot), new ZQuery(RefCityPCodePivotSchema.R0_R9, refCityTown.PK)));
			});

			using (SystemDataRegistry.Instance.DisableNonVerifiablePostcodeWarning.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new RateTransportProviderForm(provider))
			{
				form.Show();
				Application.DoEvents();
				var zoneItemGrid = form.Controls.Find("ZoneItemGrid", true)[0] as ZGrid;
				AssertNotNull(zoneItemGrid);
				zoneItemGrid.Focus();
				AssertGreaterThan(zoneItemGrid.TableStyles.Count, 0);
				AssertNotNull(zoneItemGrid.TableStyles[0].GridColumnStyles);
				AssertGreaterThan(zoneItemGrid.TableStyles[0].GridColumnStyles.Count, 0);

				var columnStyle = zoneItemGrid.TableStyles[0].GridColumnStyles[RateTransportZoneItemSchema.TQ_FromPostCode.Name] as PostCodeColumnStyle;
				AssertNotNull(columnStyle);
				zoneItemGrid.CurrentCell = new DataGridCell(0, zoneItemGrid.TableStyles[0].GridColumnStyles.IndexOf(columnStyle));
				Application.DoEvents();
				columnStyle.SetColumnValueAtRowExposed(zoneItemGrid.ListManager, 0, postCode);
			}

			var newFactory = new BusinessObjectFactory();
			refPostCode = newFactory.LoadTop1<RefPostCode>(postCodeQuery);

			AssertNotNull("RefPostCode should exist", refPostCode);
			AssertEquals("Country", Core.Constants.CountryCodes.Indonesia, refPostCode.RK_RN_NKCountry);
			AssertEquals("PostCode", postCode, refPostCode.RK_CityTownPostCode);

			var pivotQuery = new ZQuery(new ZQuery(RefCityPCodePivotSchema.R0_R9, refCityTown.PK)
				.AddToFilter(RefCityPCodePivotSchema.R0_RK, refPostCode.PK));

			var refPivot = newFactory.LoadTop1<RefCityPCodePivot>(pivotQuery);
			AssertNotNull("RefCityPCodePivot should exist", refPivot);
		}

		public void TestDeliveryDueTimeColumnsExistInRateTransportProviderForm()
		{
			var provider = Factory.New<RateTransportProvider>();
			var zone = Factory.New<RateTransportZone>();
			zone.TZ_TP = provider.PK;

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			using (var form = new RateTransportProviderForm(provider))
			{
				form.Show();
				Application.DoEvents();
				var zoneItemGrid = form.Controls.Find("ZoneItemGrid", true)[0] as ZGrid;
				AssertNotNull(zoneItemGrid);
				zoneItemGrid.Focus();

				var columnStyle1 = zoneItemGrid.TableStyles[0].GridColumnStyles[RateTransportZoneItemSchema.TQ_DeliveryDueTime.Name] as ZDateEditColumnStyle;
				AssertNotNull(columnStyle1);
				AssertEquals("TQ_DeliveryDueTime", columnStyle1.MappingName);

				var columnStyle2 = zoneItemGrid.TableStyles[0].GridColumnStyles[RateTransportZoneItemSchema.TQ_PickupDeliveryZone.Name] as ZTextBoxColumnStyle;
				AssertNotNull(columnStyle2);
				AssertEquals("TQ_PickupDeliveryZone", columnStyle2.MappingName);
			}
		}

		public void TestDeliveryDueTimeAttributesExistInRateTransportProviderForm()
		{
			var provider = Factory.New<RateTransportProvider>();
			var zone = Factory.New<RateTransportZone>();
			zone.TZ_TP = provider.PK;

			using (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new CalculateDeliveryDueDateOptions { IsActive = true, TransportModes = ActiveTransportModesForCalculateDeliveryDateOption() }))
			using (var form = new RateTransportProviderForm(provider))
			{
				form.Show();
				Application.DoEvents();

				var defaultDeliveryDueTime = form.Controls.Find("DefaultDeliveryDueTime", true)[0] as ZTimeEdit;
				AssertNotNull(defaultDeliveryDueTime);
				Assert(defaultDeliveryDueTime.Visible);
				AssertEquals("Default Delivery Due Time", defaultDeliveryDueTime.CaptionResourceString.Caption);

				var defaultHoldForPickupTime = form.Controls.Find("DefaultHoldForPickupTime", true)[0] as ZTimeEdit;
				AssertNotNull(defaultHoldForPickupTime);
				Assert(defaultHoldForPickupTime.Visible);
				AssertEquals("Default Hold For Pickup Time", defaultHoldForPickupTime.CaptionResourceString.Caption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var provider = Factory.New<RateTransportProvider>();
			return new RateTransportProviderForm(provider);
		}

		CalculateDeliveryDueDateTransportModeCollection ActiveTransportModesForCalculateDeliveryDateOption()
		{
			var activeTransportModes = new CalculateDeliveryDueDateTransportModeCollection();
			activeTransportModes.Add(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Rail, Core.Constants.TransportModeDescriptions.Rail, true);
			return activeTransportModes;
		}
	}
}
