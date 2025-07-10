using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	[TestedType(typeof(ContainerDetentionForm))]
	internal class ContinerDetentionFormBasherTest : ZFormBasherTest
	{
		public void TestFind()
		{
			OrgHeader localDepot = Factory.NewWithValidTestData<OrgHeader>();
			localDepot.OH_Code = "Depot";
			localDepot.OH_RL_NKClosestPort = "AUBNE";
			RefContainerStock stock = Factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "TEST4100013";
			stock.R6_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUMEL";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "AUBNE";
			voyage.GenerateSailings();
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader principal = Factory.NewWithValidTestData<OrgHeader>();
			BillOfLading bill = Factory.New<BillOfLading>();
			bill.JS_JX = voyage.Sailings[0].PK;
			bill.JS_RL_NKOrigin = "AUMEL";
			bill.JS_RL_NKDestination = "AUBNE";
			bill.JS_OH_DeliveryAgent = principal.PK;
			BillOfLadingContainer container = bill.RealContainers.AddNew();
			container.JC_ContainerNum = stock.R6_ContainerNum;
			ContainerMovement movement = stock.Movements.AddNew();
			movement.E9_JV = voyage.PK;
			movement.E9_MovementType = ContainerMovementTypes.Codes.YardGateIn;
			movement.E9_OA_Depot = localDepot.MainAddress.PK;
			movement.E9_DetentionDays = 5;
			JobHeader header = new JobHeader.Loader(bill).TryLoadOrCreate();
			header.JH_GB = Env.CurrentBranch.PK;
			header.JH_GE = Env.CurrentDepartment.PK;
			header.JH_OA_LocalChargesAddr = client.MainAddress.PK;
			Factory.Save();
			using (ContainerDetentionForm form = new ContainerDetentionForm(Detention))
			{
				form.Show();
				Application.DoEvents();
				ContainerDetentionControl control = (ContainerDetentionControl)form.Controls.Find("mainControl", true)[0];
				AssertContainsExactElementsInAnyOrder((m) => string.Format("{0} {1}", m.Stock.R6_ContainerNum, m.E9_MovementType), System.Array.Empty<ContainerMovement>(), Detention.Movements);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				control.PerformFind();
				AssertEquals("Information The selected detention type is not valid.", UnitTestUserNotification.Instance.LastMessage.ToString());
				Detention.NC_DetentionType = DetentionInvoiceType.Codes.Import;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				control.PerformFind();
				AssertEquals("Information You have not entered a principal yet.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertContainsExactElementsInAnyOrder((m) => string.Format("{0} {1}", m.Stock.R6_ContainerNum, m.E9_MovementType), System.Array.Empty<ContainerMovement>(), Detention.Movements);
				AssertEquals(0, Detention.Movements.Count);
				Detention.NC_OH_Principal = principal.PK;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				control.PerformFind();
				AssertEquals("Information You have not entered a client yet.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertContainsExactElementsInAnyOrder((m) => string.Format("{0} {1}", m.Stock.R6_ContainerNum, m.E9_MovementType), System.Array.Empty<ContainerMovement>(), Detention.Movements);
				Detention.NC_OH_Client = client.PK;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				control.PerformFind();
				AssertEquals("Information Found 1 matching container.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertContainsExactElementsInAnyOrder((m) => string.Format("{0} {1}", m.Stock.R6_ContainerNum, m.E9_MovementType), new ContainerMovement[] { movement }, Detention.Movements);
				Detention.NC_DetentionType = DetentionInvoiceType.Codes.Export;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				control.PerformFind();
				AssertEquals("Information No matching containers found.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertContainsExactElementsInAnyOrder((m) => string.Format("{0} {1}", m.Stock.R6_ContainerNum, m.E9_MovementType), System.Array.Empty<ContainerMovement>(), Detention.Movements);
				Detention.NC_DetentionType = DetentionInvoiceType.Codes.Import;
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				control.PerformFind();
				AssertEquals("Information You cannot attach containers to a detention job once saved.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertContainsExactElementsInAnyOrder((m) => string.Format("{0} {1}", m.Stock.R6_ContainerNum, m.E9_MovementType), System.Array.Empty<ContainerMovement>(), Detention.Movements);
			}
		}

		public void TestCaption()
		{
			ContainerDetention detention = Factory.New<ContainerDetention>();
			using (ContainerDetentionForm form = new ContainerDetentionForm(detention))
			{
				form.Show();
				AssertEquals("New Container Detention", form.Text);
			}
		}

		#region Implementation
		protected override Form GetFormToBashCore()
		{
			return new ContainerDetentionForm(Detention);
		}

		ContainerDetention Detention
		{
			get
			{
				return detention ?? (detention = Factory.New<ContainerDetention>());
			}
		}

		ContainerDetention detention;
		#endregion
	}
}
