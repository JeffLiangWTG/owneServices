using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	sealed class DocumentActionReasonSelectorTest : TestCaseWithFactory
	{
		#region TestSelectDocumentActionReason

		public void TestSelectDocumentActionReason()
		{
			var reasonList = new CodeDescriptionPairList();
			reasonList.AddPair("REASON1", "Description Reason 1");
			reasonList.AddPair("REASON2", "Description Reason 2");

			void OnSelectionFormShow(object formOrDialog)
			{
				if (formOrDialog is DocumentActionReasonDialog form)
				{
					form.model.ReasonCode = reasonList[0].Code;
					form.model.ReasonText = "Free Text field";
				}
				else
				{
					Fail("invalid form was shown");
				}
			}

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(OnSelectionFormShow);

			var selector = new DocumentActionReasonSelector();
			var selectedReason = selector.SelectDocumentActionReason("message", "caption", reasonList);

			AssertEquals("selected reasonCode", selectedReason.ReasonCode, reasonList[0].Code);
			AssertEquals("selected reasonCode", selectedReason.ReasonText, "Free Text field");
		}

		#endregion

		#region TestIsAccessibleViaObjectFactory

		public void TestIsAccessibleViaObjectFactory()
		{
			var selector = ObjectFactory.Get<IDocumentActionReasonSelector>();

			Assert("DocumentActionReasonSelector can ba accessed via ObjectFactory", selector is DocumentActionReasonSelector);
		}

		#endregion
	}
}
