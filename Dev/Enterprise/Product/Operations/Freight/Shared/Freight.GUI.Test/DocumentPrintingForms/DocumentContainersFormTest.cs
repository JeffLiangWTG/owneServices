using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(DocumentContainersForm))]
	sealed class DocumentContainersFormTest : ZFormBasherTest
	{
		public void TestReadOnlyColumns()
		{
			DocumentContainers documentContainers = new DocumentContainers(new ContainerToSelectFromForPrintingCollection(Factory));

			using (DocumentContainersForm form = new DocumentContainersForm(documentContainers, ContainerSelectorMode.PrintIncludingUncontainerised))
			{
				form.Show();
				ZGrid grid = form.Controls.Find("containersGrid", true)[0] as ZGrid;
				IEnumerable<ZGridColumnInfo> columns = grid.ColumnStyles.Cast<ZGridColumnInfo>();
				ZGridColumnInfo containerNumberColumn = columns.First(c => c.ColumnName == "ContainerNumber");
				ZGridColumnInfo sealNumberColumn = columns.First(c => c.ColumnName == "Container+JC_SealNum");
				ZGridColumnInfo containerModeColumn = columns.First(c => c.ColumnName == "Container+JC_ContainerMode");
				ZGridColumnInfo printCheckboxColumn = columns.First(c => c.ColumnName == "JC_Calc_PrintDocumentForContainer");
				Assert("Container Number should be read only.", containerNumberColumn.IsReadOnly);
				Assert("Seal Number should be read only.", sealNumberColumn.IsReadOnly);
				Assert("Container Mode should be read only.", containerModeColumn.IsReadOnly);
				Assert("Print check box should not be read only.", !printCheckboxColumn.IsReadOnly);
			}
		}

		protected override Form GetFormToBashCore()
		{
			DocumentContainers documentContainers = new DocumentContainers(new ContainerToSelectFromForPrintingCollection(Factory));
			return new DocumentContainersForm(documentContainers, ContainerSelectorMode.PrintIncludingUncontainerised);
		}

		public void TestDialogResult()
		{
			using (DocumentContainersForm form = (DocumentContainersForm)GetFormToBash())
			{
				form.Show();

				ZButton printButton = form.Controls.Find("PrintButton", true)[0] as ZButton;
				printButton.PerformClick();

				AssertEquals(form.DialogResult, DialogResult.Yes);
			}

			using (DocumentContainersForm form = (DocumentContainersForm)GetFormToBash())
			{
				form.Show();

				ZButton cancelPrintButton = form.Controls.Find("CancelPrintButton", true)[0] as ZButton;
				cancelPrintButton.PerformClick();

				AssertEquals(form.DialogResult, DialogResult.No);
			}
		}

		public void TestIncludeUnContainerisedCheckBox()
		{
			DocumentContainers documentContainers = new DocumentContainers(new ContainerToSelectFromForPrintingCollection(Factory));

			using (DocumentContainersForm form = new DocumentContainersForm(documentContainers, ContainerSelectorMode.PrintIncludingUncontainerised))
			{
				form.Show();

				ZCheckBox checkBox = form.Controls.Find("IncludeUnContainerisedCheckBox", true)[0] as ZCheckBox;
				AssertEquals(true, checkBox.Visible);

				checkBox.Checked = false;
				AssertEquals(false, form.IncludeUnContainerised);

				checkBox.Checked = true;
				AssertEquals(true, form.IncludeUnContainerised);
			}

			using (DocumentContainersForm form = new DocumentContainersForm(documentContainers, ContainerSelectorMode.Print))
			{
				form.Show();

				ZCheckBox checkBox = form.Controls.Find("IncludeUnContainerisedCheckBox", true)[0] as ZCheckBox;
				AssertEquals(false, checkBox.Visible);
			}
		}

		public void TestIncludeVGMColumns()
		{
			var documentContainers = new DocumentContainers(new ContainerToSelectFromForPrintingCollection(Factory));

			using (var form = new DocumentContainersForm(documentContainers, ContainerSelectorMode.VGM))
			{
				form.Show();

				var grid = form.Controls.Find("containersGrid", true)[0] as ZGrid;
				var columnNames = grid
					.ColumnStyles
					.Cast<ZTextBoxColumnStyleInfo>()
					.Select(x => x.ColumnName)
					.ToArray();

				AssertContainsExactElementsInAnyOrder("grid columns",
					new[]
					{
						"ContainerNumber",
						"Container+JC_ContainerMode",
						"Container+JC_SealNum",
						"JC_Calc_PrintDocumentForContainer",
						"Container+GrossWeightVerifiedByNameOrPK",
						"Container+JC_GrossWeightVerificationStatus",
						"Container+JC_GrossWeightVerificationDateTime",
						"Container+JC_GrossWeightVerificationType",
					},
					columnNames);

				var selectAllButton = form.Controls.Find("SelectAllButton", true)[0];
				AssertEquals(true, selectAllButton.Visible);

				var selectNoneButton = form.Controls.Find("SelectNoneButton", true)[0];
				AssertEquals(true, selectNoneButton.Visible);
			}

			using (var form = new DocumentContainersForm(documentContainers, ContainerSelectorMode.Print))
			{
				form.Show();

				var grid = form.Controls.Find("containersGrid", true)[0] as ZGrid;
				var columnNames = grid
					.ColumnStyles
					.Cast<ZTextBoxColumnStyleInfo>()
					.Select(x => x.ColumnName)
					.ToArray();

				AssertContainsExactElementsInAnyOrder("grid columns",
					new[]
					{
						"ContainerNumber",
						"Container+JC_ContainerMode",
						"Container+JC_SealNum",
						"JC_Calc_PrintDocumentForContainer"
					},
					columnNames);

				var selectAllButton = form.Controls.Find("SelectAllButton", true)[0];
				AssertEquals(false, selectAllButton.Visible);

				var selectNoneButton = form.Controls.Find("SelectNoneButton", true)[0];
				AssertEquals(false, selectNoneButton.Visible);
			}
		}

		public void TestContainerAMQColumns()
		{
			var documentContainers = new DocumentContainers(new ContainerToSelectFromForPrintingCollection(Factory));

			using (var form = new DocumentContainersForm(documentContainers, ContainerSelectorMode.AMQ))
			{
				form.Show();

				var grid = form.Controls.Find("containersGrid", true).Single() as ZGrid;
				var columnNames = grid.ColumnStyles
					.Cast<ZTextBoxColumnStyleInfo>()
					.Select(x => x.ColumnName)
					.ToArray();

				AssertContainsExactElementsInAnyOrder("grid columns",
					new[]
					{
						"ContainerNumber",
						"Container+JC_SealNum",
						"Container+JC_ContainerMode",
						"JC_Calc_PrintDocumentForContainer"
					},
					columnNames);

				var deliverButton = form.Controls.Find("PrintButton", true).Single();
				AssertEquals("Caption of Send Message button", "Send Message", deliverButton.Text);

				var checkBox = form.Controls.Find("IncludeUnContainerisedCheckBox", true)[0] as ZCheckBox;
				AssertEquals(false, checkBox.Visible);
			}
		}

		public void TestContainerLDEColumns()
		{
			var documentContainers = new DocumentContainers(new ContainerToSelectFromForPrintingCollection(Factory));

			using (var form = new DocumentContainersForm(documentContainers, ContainerSelectorMode.LDE))
			{
				form.Show();

				var grid = form.Controls.Find("containersGrid", true).Single() as ZGrid;
				var columnNames = grid.ColumnStyles
					.Cast<ZTextBoxColumnStyleInfo>()
					.Select(x => x.ColumnName)
					.ToArray();

				AssertContainsExactElementsInAnyOrder("grid columns",
					new[]
					{
						"ContainerNumber",
						"Container+JC_SealNum",
						"Container+JC_ContainerMode",
						"JC_Calc_PrintDocumentForContainer"
					},
					columnNames);

				var deliverButton = form.Controls.Find("PrintButton", true).Single();
				AssertEquals("Caption of Send Message button", "Send Message", deliverButton.Text);

				var checkBox = form.Controls.Find("IncludeUnContainerisedCheckBox", true)[0] as ZCheckBox;
				AssertEquals(false, checkBox.Visible);
			}
		}

		public void TestContainerLPDColumns()
		{
			var documentContainers = new DocumentContainers(new ContainerToSelectFromForPrintingCollection(Factory));

			using (var form = new DocumentContainersForm(documentContainers, ContainerSelectorMode.LPD))
			{
				form.Show();

				var grid = form.Controls.Find("containersGrid", true).Single() as ZGrid;
				var columnNames = grid.ColumnStyles
					.Cast<ZTextBoxColumnStyleInfo>()
					.Select(x => x.ColumnName)
					.ToArray();

				AssertContainsExactElementsInAnyOrder("grid columns",
					new[]
					{
						"ContainerNumber",
						"Container+JC_SealNum",
						"Container+JC_ContainerMode",
						"JC_Calc_PrintDocumentForContainer"
					},
					columnNames);

				var deliverButton = form.Controls.Find("PrintButton", true).Single();
				AssertEquals("Caption of Send Message button", "Send Message", deliverButton.Text);

				var checkBox = form.Controls.Find("IncludeUnContainerisedCheckBox", true)[0] as ZCheckBox;
				AssertEquals(false, checkBox.Visible);
			}
		}

		public void TestContainerLoadPlanColumns()
		{
			var documentContainers = new DocumentContainers(new ContainerToSelectFromForPrintingCollection(Factory));

			using (var form = new DocumentContainersForm(documentContainers, ContainerSelectorMode.ContainerLoadPlan))
			{
				form.Show();

				var grid = form.Controls.Find("containersGrid", true).Single() as ZGrid;
				var columnNames = grid.ColumnStyles
					.Cast<ZTextBoxColumnStyleInfo>()
					.Select(x => x.ColumnName)
					.ToArray();

				AssertContainsExactElementsInAnyOrder("grid columns",
					new[]
					{
						"ContainerNumber",
						"Container+JC_RC",
						"Container+JC_SealNum",
						"JC_Calc_PrintDocumentForContainer"
					},
					columnNames);

				var deliverButton = form.Controls.Find("PrintButton", true).Single();
				AssertEquals("Caption of Deliver button", "Deliver", deliverButton.Text);

				var selectAllButton = form.Controls.Find("SelectAllButton", true).Single();
				AssertEquals("SelectAllButton is not visible", false, selectAllButton.Visible);

				var selectNoneButton = form.Controls.Find("SelectNoneButton", true).Single();
				AssertEquals("SelectNoneButton is not visible", false, selectNoneButton.Visible);
			}
		}

		public void TestExportPreAdviceNotificationColumns()
		{
			var documentContainers = new DocumentContainers(new ContainerToSelectFromForPrintingCollection(Factory));

			using (var form = new DocumentContainersForm(documentContainers, ContainerSelectorMode.ExportPreAdviceNotification))
			{
				form.Show();

				var grid = form.Controls.Find("containersGrid", true).Single() as ZGrid;
				var columnNames = grid.ColumnStyles
					.Cast<ZTextBoxColumnStyleInfo>()
					.Select(x => x.ColumnName)
					.ToArray();

				AssertContainsExactElementsInAnyOrder("grid columns",
					new[]
					{
						"ContainerNumber",
						"Container+RefContainer+RC_ISOType",
						"Container+JC_ContainerMode",
						"Container+JC_GrossWeightVerificationType",
						"JC_EPANStatusForBinding",
						"JC_Calc_PrintDocumentForContainer"
					},
					columnNames);

				var selectAllButton = form.Controls.Find("SelectAllButton", true).Single();
				AssertEquals("SelectAllButton is visible", true, selectAllButton.Visible);

				var selectNoneButton = form.Controls.Find("SelectNoneButton", true).Single();
				AssertEquals("SelectNoneButton is visible", true, selectNoneButton.Visible);

				var sendButton = form.Controls.Find("PrintButton", true).Single();
				AssertEquals("Caption of Send button", "Send", sendButton.Text);
				AssertEquals("SendButton is disabled", false, sendButton.Enabled);
			}
		}

		public void TestExportPreAdviceNotification_EnableSendButtonWhenSingleValidContainer()
		{
			var departureCTOAddress = Factory.New<OrgHeader>();
			departureCTOAddress.OH_RL_NKClosestPort = "NZAKL";

			var consol = Factory.New<CommonConsol>();
			consol.JK_OA_DepartureCTOAddress = departureCTOAddress.MainAddress.PK;

			var commonContainer = Factory.New<CommonContainer>();
			commonContainer.JC_JK = consol.PK;
			commonContainer.JC_GrossWeightVerificationStatus = "NRQ";
			commonContainer.JC_GrossWeightVerificationDateTime = ZDateTime.Now;

			var container1ToSelectFrom = new ContainerToSelectFromForPrinting(commonContainer);
			container1ToSelectFrom.JC_Calc_PrintDocumentForContainer = false;

			var documentContainers = new DocumentContainers(new ContainerToSelectFromForPrintingCollection(Factory));
			documentContainers.ContainersToSelectFrom.Add(container1ToSelectFrom);

			using (var form = new DocumentContainersForm(documentContainers, ContainerSelectorMode.ExportPreAdviceNotification))
			{
				form.Show();

				var grid = form.Controls.Find("containersGrid", true).Single() as ZGrid;
				var sendButton = form.Controls.Find("PrintButton", true).Single();

				AssertEquals(1, grid.List.Count);
				AssertEquals("Single container, ticked by default", true, ((ContainerToSelectFromForPrinting)grid.List[0]).JC_Calc_PrintDocumentForContainer);
				AssertEquals("SendButton is enabled", true, sendButton.Enabled);
			}
		}

		public void TestExportPreAdviceNotification_MissingOperationalPort()
		{
			var consol = Factory.New<CommonConsol>();

			var commonContainer = Factory.New<CommonContainer>();
			commonContainer.JC_JK = consol.PK;
			commonContainer.JC_GrossWeightVerificationStatus = "NRQ";
			commonContainer.JC_GrossWeightVerificationDateTime = ZDateTime.Now;

			var container1ToSelectFrom = new ContainerToSelectFromForPrinting(commonContainer);
			container1ToSelectFrom.JC_Calc_PrintDocumentForContainer = false;

			var documentContainers = new DocumentContainers(new ContainerToSelectFromForPrintingCollection(Factory));
			documentContainers.ContainersToSelectFrom.Add(container1ToSelectFrom);

			using (var form = new DocumentContainersForm(documentContainers, ContainerSelectorMode.ExportPreAdviceNotification))
			{
				form.Show();

				var grid = form.Controls.Find("containersGrid", true).Single() as ZGrid;

				AssertEquals(1, grid.List.Count);
				AssertHasError(container1ToSelectFrom.JC_EPANStatusForBindingInfo, "Operational Port is required.");
			}
		}

		public void TestExportPreAdviceNotification_InvalidSingleContainer()
		{
			var errorMessage = @"Export Pre-Advice submissions to the Port of Auckland,
the Verified Method and Verified Date are mandatory.";

			var departureCTOAddress = Factory.New<OrgHeader>();
			departureCTOAddress.OH_RL_NKClosestPort = "NZAKL";

			var consol = Factory.New<CommonConsol>();
			consol.JK_OA_DepartureCTOAddress = departureCTOAddress.MainAddress.PK;

			var commonContainer = Factory.New<CommonContainer>();
			commonContainer.JC_JK = consol.PK;
			commonContainer.JC_GrossWeightVerificationStatus = "NON";

			var container1ToSelectFrom = new ContainerToSelectFromForPrinting(commonContainer);
			container1ToSelectFrom.JC_Calc_PrintDocumentForContainer = false;

			var documentContainers = new DocumentContainers(new ContainerToSelectFromForPrintingCollection(Factory));
			documentContainers.ContainersToSelectFrom.Add(container1ToSelectFrom);

			using (var form = new DocumentContainersForm(documentContainers, ContainerSelectorMode.ExportPreAdviceNotification))
			{
				form.Show();

				var grid = form.Controls.Find("containersGrid", true).Single() as ZGrid;

				AssertEquals(1, grid.List.Count);
				AssertHasError("VGM Status is NON and Missing Verified Date", container1ToSelectFrom.JC_EPANStatusForBindingInfo, errorMessage);

				container1ToSelectFrom.JC_Calc_PrintDocumentForContainer = false;
				commonContainer.JC_GrossWeightVerificationStatus = "NRQ";
				container1ToSelectFrom.JC_Calc_PrintDocumentForContainer = true;

				AssertHasError("Missing Verified Date", container1ToSelectFrom.JC_EPANStatusForBindingInfo, errorMessage);

				container1ToSelectFrom.JC_Calc_PrintDocumentForContainer = false;
				commonContainer.JC_GrossWeightVerificationDateTime = ZDateTime.Now;
				container1ToSelectFrom.JC_Calc_PrintDocumentForContainer = true;

				AssertNoErrors(container1ToSelectFrom.JC_EPANStatusForBindingInfo);
			}
		}

		public void TestExportPreAdviceNotification_InvalidMultipleContainersInPortNZLYT()
		{
			var errorMessage = @"Export Pre-Advice submissions to the Port of Lyttelton,
the Verified Method, Verified Date, and VGM Verified By are mandatory.";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;

			var orgCusCode = orgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.TaxIDNumber;
			orgCusCode.OK_CustomsRegNo = "1234/AA/0111";
			orgCusCode.OK_OA_PremisesAddress = address.PK;

			var departureCTOAddress = Factory.New<OrgHeader>();
			departureCTOAddress.OH_RL_NKClosestPort = "NZLYT";

			var consol = Factory.New<CommonConsol>();
			consol.JK_OA_DepartureCTOAddress = departureCTOAddress.MainAddress.PK;

			var commonContainer = Factory.New<CommonContainer>();
			commonContainer.JC_JK = consol.PK;
			commonContainer.JC_GrossWeightVerificationStatus = "NON";
			commonContainer.JC_GrossWeightVerificationDateTime = ZDateTime.Now;
			commonContainer.GrossWeightVerifiedByAddress.OrganisationPK = orgHeader.PK;

			var commonContainer2 = Factory.New<CommonContainer>();
			commonContainer2.JC_JK = consol.PK;
			commonContainer2.JC_GrossWeightVerificationStatus = "NRQ";

			var container1ToSelectFrom = new ContainerToSelectFromForPrinting(commonContainer);
			container1ToSelectFrom.JC_Calc_PrintDocumentForContainer = false;

			var container2ToSelectFrom = new ContainerToSelectFromForPrinting(commonContainer2);
			container2ToSelectFrom.JC_Calc_PrintDocumentForContainer = false;

			var documentContainers = new DocumentContainers(new ContainerToSelectFromForPrintingCollection(Factory));

			documentContainers.ContainersToSelectFrom.Add(container1ToSelectFrom);
			documentContainers.ContainersToSelectFrom.Add(container2ToSelectFrom);

			using (var form = new DocumentContainersForm(documentContainers, ContainerSelectorMode.ExportPreAdviceNotification))
			{
				form.Show();

				var grid = form.Controls.Find("containersGrid", true).Single() as ZGrid;
				var sendButton = form.Controls.Find("PrintButton", true).Single();

				AssertEquals(2, grid.List.Count);
				AssertEquals("SendButton is disabled", false, sendButton.Enabled);

				var containerList = grid.List.Cast<ContainerToSelectFromForPrinting>().ToList();
				containerList.ForEach(c => c.JC_Calc_PrintDocumentForContainer = true);

				AssertHasError("VGM Status is NON", container1ToSelectFrom.JC_EPANStatusForBindingInfo, errorMessage);
				AssertHasError("Missing Verified Date and VGM Verified By", container2ToSelectFrom.JC_EPANStatusForBindingInfo, errorMessage);
				AssertEquals("SendButton is disabled", false, sendButton.Enabled);

				containerList.ForEach(c => c.JC_Calc_PrintDocumentForContainer = false);
				commonContainer.JC_GrossWeightVerificationStatus = "NRQ";
				commonContainer2.GrossWeightVerifiedByAddress.OrganisationPK = orgHeader.PK;
				containerList.ForEach(c => c.JC_Calc_PrintDocumentForContainer = true);

				AssertHasError("Missing Verified Date", container2ToSelectFrom.JC_EPANStatusForBindingInfo, errorMessage);
				AssertEquals("SendButton is disabled", false, sendButton.Enabled);

				container2ToSelectFrom.JC_Calc_PrintDocumentForContainer = false;
				commonContainer2.JC_GrossWeightVerificationDateTime = ZDateTime.Now;
				container2ToSelectFrom.JC_Calc_PrintDocumentForContainer = true;

				foreach (var container in containerList)
				{
					AssertNoErrors(container.JC_EPANStatusForBindingInfo);
				}

				AssertEquals("SendButton is enabled", true, sendButton.Enabled);
			}
		}

		public void TestIncludeCMRConsignmentNoteColumns()
		{
			var documentContainers = new DocumentContainers(new ContainerToSelectFromForPrintingCollection(Factory));

			using (var form = new DocumentContainersForm(documentContainers, ContainerSelectorMode.CMRConsignmentNote))
			{
				form.Show();

				var grid = form.Controls.Find("containersGrid", true)[0] as ZGrid;
				var columnNames = grid
					.ColumnStyles
					.Cast<ZTextBoxColumnStyleInfo>()
					.Select(x => x.ColumnName)
					.ToArray();

				AssertContainsExactElementsInAnyOrder("grid columns",
					new[]
					{
						"ContainerNumber",
						"Container+JC_ContainerMode",
						"Container+JC_SealNum",
						"JC_Calc_PrintDocumentForContainer"
					},
					columnNames);

				var deliverButton = form.Controls.Find("PrintButton", true).Single();
				AssertEquals("Caption of Deliver button", "Deliver", deliverButton.Text);

				var selectLabel = form.Controls.Find("SelectContainersLabel", true).Single();
				AssertEquals("Caption of Deliver button", "Select container", selectLabel.Text);

				var selectAllButton = form.Controls.Find("SelectAllButton", true).Single();
				AssertEquals("SelectAllButton is not visible", false, selectAllButton.Visible);

				var selectNoneButton = form.Controls.Find("SelectNoneButton", true).Single();
				AssertEquals("SelectNoneButton is not visible", false, selectNoneButton.Visible);
			}
		}
	}
}
