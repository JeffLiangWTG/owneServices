using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefCityTownForm : ZTemplateForm
	{
		public RefCityTownForm()
			: base()
		{
		}

		public RefCityTownForm(RefCityTown refCityTown) : base(refCityTown)
		{
			this.citytown = refCityTown;
		}

		readonly RefCityTown citytown;

		void zModuleButtonGrid1_Detaching(object sender, ModuleButtonGridOperationCancelEventArgs args)
		{
			var postcode = (RefPostCode)args.BizOBeingOperated;
			var pivot = (RefCityPCodePivot)((ManyToManyRelationship)citytown.PostCodes.Relationship).GetPivotObject(postcode);
			if (pivot.R0_IsSystem)
			{
				args.Cancel = true;
				Globals.Message.ShowError(Res.GetString("0317221C-A421-470B-8674-0023AC713B2A", "This city/postcode relationship is system defined and therefore the selected postcode cannot be detached from this city."));
			}
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
