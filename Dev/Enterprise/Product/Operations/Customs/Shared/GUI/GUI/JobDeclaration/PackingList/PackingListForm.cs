using System;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GUI
{
	public partial class PackingListForm : ZTemplateForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public PackingListForm()
		{
		}

		public PackingListForm(CusPackingList cusPackingList) : base(cusPackingList)
		{
			PackageJob = cusPackingList.PackageJob;
			InitializeComponent();
			InitializeTabsLazyCreate();
			AddPlugins();
		}

		public CusPackageJob PackageJob { get; }

		public override string FormCaption
		{
			get
			{
				string caption = Res.GetString("25A95D86-B725-4116-A452-7C1012D8CE2C", "Customs Packing List");

				if (!PackageJob.KJ_JobID.IsEmpty)
				{
					caption += " - " + PackageJob.KJ_JobID;
				}
				return caption;
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DisableNewAction();
		}

		protected virtual void AddPlugins()
		{
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
		}

		protected virtual Type GetPackingListDetailsUserControl() => typeof(PackingListDetailsUserControl);

		void InitializeTabsLazyCreate()
		{
			MainTabPage.RunWhenBindingOrFirstShown((s, args) => PackingListDetailsDynamicUserControl.UserControlType = GetPackingListDetailsUserControl());
		}
	}
}
