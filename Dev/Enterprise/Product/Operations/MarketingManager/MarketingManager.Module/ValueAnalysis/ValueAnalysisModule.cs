using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MarketingManager.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.Module
{
	public abstract class ValueAnalysisModule : ZFilterGridModule
	{
		public ZString Context { get; set; }

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => null;

		protected override IFilterControl GetNewFilterControl() => new ValueAnalysisFilterControl(GridCollection, FilterBusinessObject, Context, ValueAnalysisModuleHelper.GetProductCode(ID));

		protected override IBusinessObjectCollection GetNewGridCollection() => new ViewValueAnalysisCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new ValueAnalysisFilterBusinessObject();

		public override bool AllowNew => false;

		public override bool AllowView => false;

		public override bool AllowEdit => false;

		public override bool AllowDelete => false;

		#region CheckPoints

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.SalesValueAnalysis;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.None;

		#endregion
	}
}

#region Test
#if DEBUG

namespace Enterprise.MarketingManager.Module.Testing
{
	using Enterprise.Core.Modules;

	public interface IValueAnalysisModuleForTest : IZModule, INamedModule, ISecuredModule
	{
		IFilterControl GetNewFilterControl();
		IBusinessObjectCollection GetNewGridCollection();
		FilterBusinessObject GetNewFilterBusinessObject();
		bool AllowEdit { get; }
		bool AllowView { get; }
		bool AllowDelete { get; }
		ZString Context { get; set; }
	}
}

#endif
#endregion
