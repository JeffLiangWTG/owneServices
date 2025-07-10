using CargoWise.EntityFramework;
using Enterprise.Customs.Module;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.NZ.Module.Declaration.FormalEntry
{
	public partial class NZJobDeclarationFilterStripControl : JobDeclarationFilterStripControl
	{
		public NZJobDeclarationFilterStripControl()
			: base()
		{
		}

		public NZJobDeclarationFilterStripControl(
			JobDeclarationModule module,
			IBusinessObjectCollection gridCollection,
			FilterStripBusinessObject filterBusinessObject)
			: base(module, gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			RemoveInvalidColumn();
		}

		void RemoveInvalidColumn()
		{
			FilteredGrid.SetAvailability(false, [JobDeclaration.Schema.JE_DateOfFirstArrival, JobDeclaration.Schema.JE_RL_NKPortOfFirstArrival]);
		}

		protected override bool IsEntrySubmitDateShort
		{
			get { return true; }
		}
	}
}


