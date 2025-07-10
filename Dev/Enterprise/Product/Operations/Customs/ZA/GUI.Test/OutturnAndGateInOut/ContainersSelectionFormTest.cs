using System.Windows.Forms;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using BaseAsycudaContainer = Enterprise.Customs.ManifestBase.AsycudaContainer;
using BaseAsycudaManifestHeader = Enterprise.Customs.ManifestBase.AsycudaManifestHeader;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	[TestedType(typeof(ContainersSelectionForm))]
	sealed class ContainersSelectionFormTest : ZFormBasherTest
	{
		public void TestCancelButton()
		{
			var copyBO = new BaseAsycudaManifestHeaderCopyBO(basManifestHeader, manifestHeader);
			Factory.Save();
			using (var form = new ContainersSelectionForm(copyBO))
			{
				form.Show();
				Application.DoEvents();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				form.cancelButton.PerformClick();
				AssertEquals("Do you want to cancel the copy process?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Form should have not been closed", form.Visible);
				AssertNull(copyBO.SourceContainerToCopy);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.cancelButton.PerformClick();
				AssertEquals("Do you want to cancel the copy process?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Form should have been closed", !form.Visible);
				AssertNull(copyBO.SourceContainerToCopy);
			}
		}

		public void TestOKButton()
		{
			var copyBO = new BaseAsycudaManifestHeaderCopyBO(basManifestHeader, manifestHeader);
			Factory.Save();
			using (var form = new ContainersSelectionForm(copyBO))
			{
				form.Show();
				Application.DoEvents();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.okButton.PerformClick();
				AssertEquals("Please select one item.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(copyBO.SourceContainerToCopy);
				Assert("Form should have not been closed", form.Visible);
				AssertEquals(0, manifestHeader.Containers.Count);
				form.ContainersGrid.SelectAllElements();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				AssertEquals("Please select one item.", UnitTestUserNotification.Instance.LastMessage.Text);
				form.okButton.PerformClick();
				AssertNull(copyBO.SourceContainerToCopy);
				Assert("Form should have not been closed", form.Visible);
				AssertEquals(0, manifestHeader.Containers.Count);
				form.ContainersGrid.SelectSingleElementByPK(container2.PK);
				form.okButton.PerformClick();
				Assert("Form should have been closed", !form.Visible);
				AssertEquals(1, manifestHeader.Containers.Count);
				AssertEquals("CVB00002", manifestHeader.Containers[0].ACN_ContainerNumber);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var copyBO = new BaseAsycudaManifestHeaderCopyBOForTesting(basManifestHeader, manifestHeader);
			var result = new ContainersSelectionForm(copyBO);
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			manifestHeader = CreateAsycudaManifestHeader();
			basManifestHeader = CreateBaseManifestHeader();
			container1 = CreateContainer("CVB00001");
			container1.ACN_AMA_Manifest = basManifestHeader.PK;
			container2 = CreateContainer("CVB00002");
			container2.ACN_AMA_Manifest = basManifestHeader.PK;
		}

		AsycudaManifestHeader CreateAsycudaManifestHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var masterBill = header.MasterBill;
			var bills = header.Bills;
			return header;
		}

		BaseAsycudaManifestHeader CreateBaseManifestHeader()
		{
			var manifestHeader = Factory.NewWithValidTestData<BaseAsycudaManifestHeader>();
			manifestHeader.AMA_ApplicationCode = "NVC";
			manifestHeader.AMA_TransportMode = "SEA";
			manifestHeader.AMA_ContainerMode = "CNT";
			manifestHeader.AMA_AgentType = "DRT";
			manifestHeader.AMA_Voyage = "V0001";
			return manifestHeader;
		}

		BaseAsycudaContainer CreateContainer(string conainerNumber)
		{
			var container = Factory.New<BaseAsycudaContainer>();
			container.ACN_ContainerNumber = conainerNumber;
			container.ACN_Seal1 = "SN0003";
			container.ACN_SealingPartyType = "CAR";
			return container;
		}

		AsycudaManifestHeader manifestHeader;
		BaseAsycudaManifestHeader basManifestHeader;
		BaseAsycudaContainer container1;
		BaseAsycudaContainer container2;
		sealed class BaseAsycudaManifestHeaderCopyBOForTesting : BaseAsycudaManifestHeaderCopyBO
		{
			public BaseAsycudaManifestHeaderCopyBOForTesting(BaseAsycudaManifestHeader sourceHeader, AsycudaManifestHeader destinationHeader) : base(sourceHeader, destinationHeader)
			{
			}

			public new ManifestBase.AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader> SourceContainers => new ManifestBase.AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>(DestinationHeader);
		}
	}
}
