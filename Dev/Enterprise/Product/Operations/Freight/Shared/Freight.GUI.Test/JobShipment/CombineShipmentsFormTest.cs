using System;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(CombineShipmentsForm))]
	sealed class CombineShipmentsFormTest : ZFormBasherTest
	{
		public void TestCombineButton_NoSelectedShipments()
		{
			CombineShipmentsHelper helper = new CombineShipmentsHelper(Factory.New<CommonShipment>(), Factory.New<CommonConsol>());
			using (CombineShipmentsFormForTesting form = new CombineShipmentsFormForTesting(helper))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.CombineButton.PerformClick();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				AssertEquals("Please select shipments to combine.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCombineButton_MessageFromHelper_CannotCombine()
		{
			CreateTestBOs();

			var jobLoader = new JobHeader.Loader(shipment2);
			var job = jobLoader.TryCreateWithoutMutexForTestOnly();

			var hotChequeSB3 = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Accounting.Integration.IAccHotCheque)));
			hotChequeSB3[AccHotChequeSchema.Constants.AQ_JH] = job.PK;

			Factory.Save();

			var helper = new CombineShipmentsHelper(shipment1, consol);
			AssertEquals("Precondition", true, helper.RelatedShipments.Contains(shipment2));

			using (CombineShipmentsFormForTesting form = new CombineShipmentsFormForTesting(helper))
			{
				form.Show();
				form.ShipmentsGrid.Select(0);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.CombineButton.PerformClick();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				AssertEquals("Can't combine", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals(@"S00001001 cannot be deactivated.
Hot Cheque(s) have been saved against this Invoicing Job Header (S00001001) in the company EDI.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCombineButton_MessageFromHelper_CanCombine()
		{
			CreateTestBOs();

			CombineShipmentsHelper helper = new CombineShipmentsHelper(shipment1, consol);
			AssertEquals("Precondition", true, helper.RelatedShipments.Contains(shipment2));

			using (CombineShipmentsFormForTesting form = new CombineShipmentsFormForTesting(helper))
			{
				form.Show();
				form.ShipmentsGrid.Select(0);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.CombineButton.PerformClick();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				AssertEquals("No dialogs shown", "None ", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		void CreateTestBOs()
		{
			OrgHeader organization = Factory.NewWithValidTestData<OrgHeader>();
			consol = Factory.NewWithValidTestData<CommonConsol>();

			Func<CommonShipment> createShipment = () =>
			{
				CommonShipment shipment = Factory.New<CommonShipment>();
				shipment.ConsigneePK = organization.PK;
				shipment.ConsignorPK = organization.PK;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";
				shipment.Consols.Add(consol);

				return shipment;
			};

			shipment1 = createShipment();
			shipment2 = createShipment();

			Factory.Save();
		}

		CommonShipment shipment1;
		CommonShipment shipment2;
		CommonConsol consol;

		#region Implementation

		class CombineShipmentsFormForTesting : CombineShipmentsForm
		{
			public CombineShipmentsFormForTesting(CombineShipmentsHelper helper)
				: base(helper)
			{
			}

			public new ZGrid ShipmentsGrid
			{
				get { return base.ShipmentsGrid; }
			}

			public new ZButton CombineButton
			{
				get { return base.CombineButton; }
			}
		}

		protected override Form GetFormToBashCore()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			JobSailing sailing = new SailingsForTestClasses(Factory).SydLaxSailing;
			CombineShipmentsHelper helper = new CombineShipmentsHelper(shipment, sailing);
			return new CombineShipmentsForm(helper);
		}

		#endregion
	}
}
