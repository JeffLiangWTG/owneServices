using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Confirmations.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Confirmations.GUI.Testing
{
	[TestedType(typeof(QuickPODForm))]
	sealed class QuickPODFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			QuickPODs quickPODs = new QuickPODs(Factory);
			return new QuickPODForm(quickPODs);
		}

		public void TestQuickPODShipmentSelectionFormIsDisposed()
		{
			using (QuickPODForm form = (QuickPODForm)GetFormToBashCore())
			{
				ShipmentCollection shipments = new ShipmentCollection(Factory);
				shipments.AddNew();
				shipments.AddNew();
				shipments.AddNew();
				QuickPODMultipleShipmentsEventArgs e = new QuickPODMultipleShipmentsEventArgs("S101", shipments);

				((QuickPODs)form.BusinessEntity).RaiseQuickPODMultipleShipments(e);

				AssertEquals(typeof(QuickPODShipmentSelectionForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals(true, ZFormModaliser.LastFormShownDialogForTest.IsDisposed);
			}
		}

		public void TestClosingFormReleasesJobHeaderMutexAfterSave()
		{
			AssertClosingFormReleasesJobHeaderMutexNotSaved(true);
		}

		public void TestClosingFormReleasesJobHeaderMutexNotSaved()
		{
			AssertClosingFormReleasesJobHeaderMutexNotSaved(false);
		}

		void AssertClosingFormReleasesJobHeaderMutexNotSaved(ZBool saveFactory)
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.JS_UniqueConsignRef = "s1";

			if (saveFactory)
			{
				Factory.Save();
			}

			QuickPODs pODs = new QuickPODs(Factory);

			QuickPOD pod = new QuickPOD(pODs);
			pod.ShipmentID = "s1";
			pODs.QuickPODsCollection.Add(pod);
			AssertEquals("creating pod should lock the mutex", true, JobHeader.GetMutex_ForTestOnly(shipment.PK).IsLocked);
			using (QuickPODForm form = new QuickPODForm(pODs))
			{
				form.Close();
			}
			AssertEquals("closing the form should release jobheader mutex", false, JobHeader.GetMutex_ForTestOnly(shipment.PK).IsLocked);
		}
	}
}
