using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefPostCodeForm : ZTemplateForm
	{
		public RefPostCodeForm()
			: base()
		{
		}

		public RefPostCodeForm(RefPostCode refPostCode) : base(refPostCode)
		{
			this.postcode = refPostCode;
		}

		readonly RefPostCode postcode;

		void zModuleButtonGrid1_Detaching(object sender, ModuleButtonGridOperationCancelEventArgs args)
		{
			var cityTown = (RefCityTown)args.BizOBeingOperated;
			var pivot = (RefCityPCodePivot)((ManyToManyRelationship)postcode.CityTowns.Relationship).GetPivotObject(cityTown);
			if (pivot.R0_IsSystem)
			{
				args.Cancel = true;
				Globals.Message.ShowError(Res.GetString("353ee9d8-bec0-41e7-a1db-d02ef437ad5a", "This postcode/city relationship is system defined and therefore the selected city cannot be detached from this postcode."));
			}
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}
	}
}
