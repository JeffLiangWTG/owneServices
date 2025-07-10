using System.Data;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ZButtonTransportCoHotlinkTest : TestCaseWithDummy
	{
		public void TestBinding()
		{
			TestBusinessObject businessObject = Factory.New<TestBusinessObject>();
			businessObject.TransportCo = Factory.New<OrgHeader>();
			businessObject.TransportReferenceNo = "Reference #";

			using (ZForm form = new ZForm())
			using (ZButtonTransportCoHotlink button = new ZButtonTransportCoHotlink())
			{
				button.BindToTransportCo = "TransportCo";
				button.BindToTransportRef = "TransportReferenceNo";

				ZBindingSource bindingSource = new ZBindingSource();
				bindingSource.SetBindingMember(button, ".");
				bindingSource.SetDataBinding(businessObject, "");

				form.Controls.Add(button);
				form.Show();
				Application.DoEvents();

				AssertEquals("TransportCo", button.TransportCo, businessObject.TransportCo);
				AssertEquals("TransportReferenceNo", button.TransportReferenceNo, businessObject.TransportReferenceNo);
			}
		}

		#region Test Classes

		class TestBusinessObject : DummyBusinessObject
		{
			public TestBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public OrgHeader TransportCo { get; set; }
			public string TransportReferenceNo { get; set; }
		}

		#endregion
	}
}
