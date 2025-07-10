using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	sealed class EmailToContactUserControl_Test : TestCaseWithFactory
	{
		public void TestRelativeBinding()
		{
			DummyBizOForEmailToContactUserControl bizO = Factory.New<DummyBizOForEmailToContactUserControl>();
			using (DummyFormWithEmailToContactUserControl form = new DummyFormWithEmailToContactUserControl(bizO))
			{
				form.Show();
				AssertEquals("Field bind to should start with prefix string", BizOsWithPrefixName, ((ICompositeControlBindingSourceProvider)form).BindingSource.GetBindingMember(form.EmailControl));
			}
		}

		#region Implementation

		class DummyFormWithEmailToContactUserControl : ZChildForm
		{
			public DummyFormWithEmailToContactUserControl(DummyBizOForEmailToContactUserControl bizO) : base(bizO)
			{
				BindingSource.SetBindingMember(EmailControl, BizOsWithPrefixName);
			}

			public EmailToContactUserControl EmailControl;

			protected override void InitializeComponent()
			{
				EmailControl = new EmailToContactUserControl();
				Controls.Add(EmailControl);
			}
		}

		const string BizOsWithPrefixName = "BizOsWithPrefix"; //Name of collection

		class DummyBizOForEmailToContactUserControl : DummyBusinessObject
		{
			public DummyBizOForEmailToContactUserControl(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public EmailToContactBusinessObjectCollection BizOsWithPrefix
			{
				get
				{
					if (fBizOsWithPrefix == null)
					{
						fBizOsWithPrefix = new EmailToContactBusinessObjectCollection(this);
					}
					return fBizOsWithPrefix;
				}
			}

			EmailToContactBusinessObjectCollection fBizOsWithPrefix;
		}

		#endregion
	}
}
