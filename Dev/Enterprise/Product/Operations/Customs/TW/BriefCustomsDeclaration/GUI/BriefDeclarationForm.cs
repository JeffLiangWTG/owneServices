using System;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI
{
	public partial class BriefDeclarationForm : ManifestForm, ICustomerServiceMenuSectionCodeOverridable
	{
		public BriefDeclarationForm(AsycudaManifestHeader header)
			: base(header)
		{
			InitializeComponent();
			ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("85710BD9-DB23-4531-9C4D-873438A7EC97", "Calculate Duties && Taxes"), ZCalculateMenuItem_Click);
		}

		public override string FormCaption
		{
			get
			{
				string caption = (NoResString)"Brief Customs Declaration";
				if (!this.IsDesignMode())
				{
					var readablePrefix = BusinessEntity.HumanReadableNamePrefix ?? ZString.Empty;
					if (BusinessEntity.IsInDatabase)
					{
						caption = Res.GetString("BriefDeclarationForm|FormCaption|Customs", "{0} - {1} - Customs", BusinessEntity.AMA_JobReference, readablePrefix);
					}
					else
					{
						caption = Res.GetString("BriefDeclarationForm|FormCaption|New", "{0} Brief Customs Declaration", readablePrefix);
					}
				}
				return caption;
			}
		}

		string ICustomerServiceMenuSectionCodeOverridable.SectionCode => ModuleTreeCustomerServiceMenuSectionList.Codes.Customs;

		void ZCalculateMenuItem_Click(object sender, EventArgs e)
		{
			if (BusinessEntity is AsycudaManifestHeader header)
			{
				header.CalculateCustomsValueAndTaxesAndDuties();
			}
		}
	}
}
