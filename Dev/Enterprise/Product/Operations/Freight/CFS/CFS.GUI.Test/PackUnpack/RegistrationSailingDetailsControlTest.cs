using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CFS.Business;

namespace Enterprise.Freight.CFS.GUI.Testing
{
	sealed class RegistrationSailingDetailsControlTest : BaseFreightTest
	{
		public void TestTransportModeControlState()
		{
			using (RegistrationDetailsTest.CFSContainerFormTestClass form = new RegistrationDetailsTest.CFSContainerFormTestClass(CFSContainer))
			{
				form.Show();

				CFSContainer.JC_TransportMode = Constants.TransportModes.Air;
				AssertEquals(false, form.RegistrationDetails.registrationSailingDetailsControl1.VesselCodeFindBox.Visible);
				AssertEquals("Flight", form.RegistrationDetails.registrationSailingDetailsControl1.JX_VoyageTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Flight Details", form.RegistrationDetails.registrationSailingDetailsControl1.SailingDetailsGroupBox.GetExtension<ILabelCaptionRenderer>().Caption);

				CFSContainer.JC_TransportMode = Constants.TransportModes.Rail;
				AssertEquals(true, form.RegistrationDetails.registrationSailingDetailsControl1.VesselCodeFindBox.Visible);
				AssertEquals("Journey", form.RegistrationDetails.registrationSailingDetailsControl1.VesselCodeFindBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Jrny. No.", form.RegistrationDetails.registrationSailingDetailsControl1.JX_VoyageTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Journey Details", form.RegistrationDetails.registrationSailingDetailsControl1.SailingDetailsGroupBox.GetExtension<ILabelCaptionRenderer>().Caption);

				CFSContainer.JC_TransportMode = Constants.TransportModes.Road;
				AssertEquals(false, form.RegistrationDetails.registrationSailingDetailsControl1.VesselCodeFindBox.Visible);
				AssertEquals("Truck", form.RegistrationDetails.registrationSailingDetailsControl1.JX_VoyageTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Journey Details", form.RegistrationDetails.registrationSailingDetailsControl1.SailingDetailsGroupBox.GetExtension<ILabelCaptionRenderer>().Caption);

				CFSContainer.JC_TransportMode = Constants.TransportModes.Sea;
				AssertEquals(true, form.RegistrationDetails.registrationSailingDetailsControl1.VesselCodeFindBox.Visible);
				AssertEquals("Vessel", form.RegistrationDetails.registrationSailingDetailsControl1.VesselCodeFindBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Voyage", form.RegistrationDetails.registrationSailingDetailsControl1.JX_VoyageTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("Sailing Details", form.RegistrationDetails.registrationSailingDetailsControl1.SailingDetailsGroupBox.GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestSelectSailingButtonState()
		{
			using (RegistrationDetailsTest.CFSContainerFormTestClass form = new RegistrationDetailsTest.CFSContainerFormTestClass(CFSContainer))
			{
				form.Show();
				AssertEquals(true, form.RegistrationDetails.registrationSailingDetailsControl1.SelectSailingButton.Enabled);

				CFSContainer.JC_JK = Factory.New<CFSLoadListConsol>().PK;
				AssertEquals(false, form.RegistrationDetails.registrationSailingDetailsControl1.SelectSailingButton.Enabled);
				AssertEquals("Remove Schedule", form.RegistrationDetails.registrationSailingDetailsControl1.SelectSailingButton.GetExtension<ILabelCaptionRenderer>().Caption);

				CFSContainer.JC_JK = ZGuid.Empty;
				CFSContainer.JC_TransportMode = Constants.TransportModes.Air;
				AssertEquals("Select Flight Schedule", form.RegistrationDetails.registrationSailingDetailsControl1.SelectSailingButton.GetExtension<ILabelCaptionRenderer>().Caption);

				CFSContainer.JC_TransportMode = Constants.TransportModes.Road;
				AssertEquals("Select Journey Schedule", form.RegistrationDetails.registrationSailingDetailsControl1.SelectSailingButton.GetExtension<ILabelCaptionRenderer>().Caption);

				CFSContainer.JC_TransportMode = Constants.TransportModes.Rail;
				AssertEquals("Select Journey Schedule", form.RegistrationDetails.registrationSailingDetailsControl1.SelectSailingButton.GetExtension<ILabelCaptionRenderer>().Caption);

				CFSContainer.JC_TransportMode = Constants.TransportModes.Sea;
				AssertEquals("Select Sailing Schedule", form.RegistrationDetails.registrationSailingDetailsControl1.SelectSailingButton.GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestSelectSailingButtonClick()
		{
			using (RegistrationDetailsTest.CFSContainerFormTestClass form = new RegistrationDetailsTest.CFSContainerFormTestClass(CFSContainer))
			{
				CFSContainer.JC_JX = ZGuid.NewZGuid();
				form.Show();
				form.RegistrationDetails.registrationSailingDetailsControl1.SelectSailingButton.PerformClick();
				AssertEquals(ZGuid.Empty, CFSContainer.JC_JX);
			}
		}

		#region Implementation

		CFSContainer CFSContainer
		{
			get
			{
				if (fCFSContainer == null)
				{
					fCFSContainer = Factory.New<CFSContainer>();
				}
				return fCFSContainer;
			}
		}
		CFSContainer fCFSContainer;

		#endregion
	}
}
