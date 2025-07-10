using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class ContainerSelectorTest : TestCaseWithFactory
	{
		#region TestSelectContainers

		public void TestSelectContainers()
		{
			var containers = new[]
			{
				Factory.New<CommonContainer>(),
				Factory.New<CommonContainer>()
			};

			void OnSelectionFormShow(object formOrDialog)
			{
				if (formOrDialog is DocumentContainersForm form
					&& form.DataSource is DocumentContainers documentContainers)
				{
					foreach (ContainerToSelectFromForPrinting documentContainer in documentContainers.ContainersToSelectFrom)
					{
						documentContainer.JC_Calc_PrintDocumentForContainer = true;
					}
				}
				else
				{
					Fail("invalid form was shown");
				}
			}

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(OnSelectionFormShow);

			var selector = new ContainerSelector();
			var selectedContainers = selector.SelectContainers(containers);

			AssertContainsExactElementsInAnyOrder("selected containers",
				containers, selectedContainers.Right);
		}

		#endregion

		#region TestIsAccessibleViaObjectFactory

		public void TestIsAccessibleViaObjectFactory()
		{
			var selector = ObjectFactory.Get<IContainerSelector>();

			Assert("ContainerSelector can ba accessed via ObjectFactory", selector is ContainerSelector);
		}

		#endregion

		public void TestPrintSingle_CanOnlySelectOneAtATime()
		{
			CanOnlySelectOneAtATime(ContainerSelectorMode.PrintSingle);
		}

		public void TestCMRConsignmentNote_CanOnlySelectOneAtATime()
		{
			CanOnlySelectOneAtATime(ContainerSelectorMode.CMRConsignmentNote);
		}

		public void CanOnlySelectOneAtATime(ContainerSelectorMode mode)
		{
			var containers = new[]
			{
				Factory.New<CommonContainer>(),
				Factory.New<CommonContainer>()
			};

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(OnSelectionFormShow);

			var selector = new ContainerSelector();
			var selectedContainers = selector.SelectContainers(containers, mode);

			void OnSelectionFormShow(object formOrDialog)
			{
				if (formOrDialog is DocumentContainersForm form
					&& form.DataSource is DocumentContainers documentContainers)
				{
					var container1 = documentContainers.ContainersToSelectFrom[0];
					var container2 = documentContainers.ContainersToSelectFrom[1];

					container1.JC_Calc_PrintDocumentForContainer = true;

					Assert("First Container should be selected", container1.JC_Calc_PrintDocumentForContainer);
					Assert("Second Container should not be selected", !container2.JC_Calc_PrintDocumentForContainer);

					container2.JC_Calc_PrintDocumentForContainer = true;

					Assert("First Container should've been unselected", !container1.JC_Calc_PrintDocumentForContainer);
					Assert("Second Container should be selected", container2.JC_Calc_PrintDocumentForContainer);
				}
				else
				{
					Fail("invalid form was shown");
				}
			}
		}

		public void TestPrint_CanSelectMultiple()
		{
			var containers = new[]
			{
				Factory.New<CommonContainer>(),
				Factory.New<CommonContainer>()
			};

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(OnSelectionFormShow);

			var selector = new ContainerSelector();
			var selectedContainers = selector.SelectContainers(containers);

			void OnSelectionFormShow(object formOrDialog)
			{
				if (formOrDialog is DocumentContainersForm form
					&& form.DataSource is DocumentContainers documentContainers)
				{
					var container1 = documentContainers.ContainersToSelectFrom[0];
					var container2 = documentContainers.ContainersToSelectFrom[1];

					container1.JC_Calc_PrintDocumentForContainer = true;

					Assert("First Container should be selected", container1.JC_Calc_PrintDocumentForContainer);
					Assert("Second Container should not be selected", !container2.JC_Calc_PrintDocumentForContainer);

					container2.JC_Calc_PrintDocumentForContainer = true;

					Assert("First Container should be selected", container1.JC_Calc_PrintDocumentForContainer);
					Assert("Second Container should be selected", container2.JC_Calc_PrintDocumentForContainer);
				}
				else
				{
					Fail("invalid form was shown");
				}
			}
		}

		public void TestPrintDocumentForContainer_DefaultSelected()
		{
			var containers = new[]
			{
				Factory.New<CommonContainer>(),
				Factory.New<CommonContainer>()
			};

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(OnSelectionFormShow);

			var selector = new ContainerSelector();
			selector.SelectContainers(containers, ContainerSelectorMode.AMQ);
			selector.SelectContainers(containers, ContainerSelectorMode.CDM);
			selector.SelectContainers(containers, ContainerSelectorMode.LDE);
			selector.SelectContainers(containers, ContainerSelectorMode.LPD);
			selector.SelectContainers(containers, ContainerSelectorMode.TRC);
		}

		void OnSelectionFormShow(object formOrDialog)
		{
			if (formOrDialog is DocumentContainersForm form
				&& form.DataSource is DocumentContainers documentContainers)
			{
				var container1 = documentContainers.ContainersToSelectFrom[0];
				var container2 = documentContainers.ContainersToSelectFrom[1];

				Assert("First Container should be selected", container1.JC_Calc_PrintDocumentForContainer);
				Assert("Second Container should be selected", container2.JC_Calc_PrintDocumentForContainer);
			}
			else
			{
				Fail("invalid form was shown");
			}
		}

		public void TestSelectContainers_ExportPreAdviceNotificationWithSingleContainer()
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

			var containers = new[]
			{
				commonContainer
			};

			void OnSelectionFormShow(object formOrDialog)
			{
				if (formOrDialog is DocumentContainersForm form && form.DataSource is DocumentContainers documentContainers)
				{
					foreach (ContainerToSelectFromForPrinting documentContainer in documentContainers.ContainersToSelectFrom)
					{
						documentContainer.JC_Calc_PrintDocumentForContainer = true;
					}
				}
				else
				{
					Fail("invalid form was shown");
				}
			}

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(OnSelectionFormShow);

			var selector = new ContainerSelector();
			var selectedContainers = selector.SelectContainers(containers, ContainerSelectorMode.ExportPreAdviceNotification);

			AssertContainsExactElementsInAnyOrder("selected containers", containers, selectedContainers.Right);
		}
	}
}
