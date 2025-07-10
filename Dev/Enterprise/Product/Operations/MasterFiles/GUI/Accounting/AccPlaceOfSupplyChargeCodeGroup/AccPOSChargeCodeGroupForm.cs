using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class AccPOSChargeCodeGroupForm : ZForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public AccPOSChargeCodeGroupForm()
		{
		}

		public AccPOSChargeCodeGroupForm(AccPOSChargeCodeGroup group) : base(group)
		{
		}

		protected new AccPOSChargeCodeGroup BusinessEntity
		{
			get { return (AccPOSChargeCodeGroup)base.BusinessEntity; }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();

			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			}
		}
	}
}
