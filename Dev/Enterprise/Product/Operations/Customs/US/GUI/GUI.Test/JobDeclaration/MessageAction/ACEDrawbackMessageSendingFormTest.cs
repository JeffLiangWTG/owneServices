using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI
{
	[TestedType(typeof(ACEDrawbackMessageSendingForm))]
	sealed class ACEDrawbackMessageSendingFormTest : ZFormBasherTest
	{
		public override void TestBashingForm()
		{
			Assert(true);
		}

		public override void TestBoundListsAreNotLoadedOnAccess()
		{
			Assert(true);
		}

		public new void TestFormIsFullyTranslatable()
		{
			Assert(true);
		}

		protected override Form GetFormToBashCore() => new ACEDrawbackMessageSendingForm(new ACEDrawbackAcknowledgeAndSign(Declaration, Messaging.Business.UpdateActionCode.Add));

		JobDeclaration declaration;
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					Declaration.US_EntryFilerCode = "XJ5";
				}

				return declaration;
			}
		}
	}
}
