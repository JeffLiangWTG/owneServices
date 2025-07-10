using System.Drawing;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Organisation.UserControls.Receivables.Testing
{
	[TestedType(typeof(ARAccountDetailsGridFormForBash))]
	sealed class ARAccountDetailsGridBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			Factory.Save();

			return new ARAccountDetailsGridFormForBash(companyData);
		}

		internal class ARAccountDetailsGridFormForBash : ZForm
		{
			internal ARAccountDetailsGridFormForBash(OrgCompanyData companyData)
				: base(companyData)
			{ }

			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				Size = new Size(1024, 768);

				Controls.Add(ARAccountDetailsGrid);
				BindingSource.SetBindingMember(ARAccountDetailsGrid, ".");
				CaptionRenderingEnabled = true;
			}

			readonly ARAccountDetailsGrid ARAccountDetailsGrid = new ARAccountDetailsGrid();
		}
	}
}
