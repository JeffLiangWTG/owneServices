using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class ChangeOffBandProcessingStatusActionMethod : OperationalActionMethod
	{
		public ChangeOffBandProcessingStatusActionMethod()
			: base(new ZGuid("C14DBC1C-C983-4E4B-9963-591D0B4D3B37"))
		{
		}

		#region GUI Control

		public override IComponent NewGuiControl() => new SelectOffBandProcessingStatusControl();

		public override bool HasControl => true;

		#endregion

		#region NameAndDesc

		public override string Name => NameAndDesc;

		public override string Description => NameAndDesc;

		static string NameAndDesc => Res.GetString("ChangeOffBandProcessingStatusActionMethod|Name&Description", "Change Off Band Processing Status");

		#endregion

		#region NewApplicator

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new ChangeOffBandProcessingStatusActionMethodApplicator(factory);
		}

		#endregion
	}
}
