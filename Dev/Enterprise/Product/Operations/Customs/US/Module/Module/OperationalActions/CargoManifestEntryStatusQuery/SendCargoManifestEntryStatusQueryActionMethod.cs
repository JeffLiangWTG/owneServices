using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.US.Module.OperationalActions
{
	public class SendCargoManifestEntryStatusQueryActionMethod : USOperationalActionMethod
	{
		public SendCargoManifestEntryStatusQueryActionMethod()
			: base(new ZGuid("476E2DB4-F7BE-4F36-8332-7D6352B71816"))
		{
		}

		public override string Name => "Send Cargo/Manifest/Entry Status Query operational action";

		public override string Description => "Send Cargo/Manifest/Entry Status Query(US)";

		public override OperationalActionMethodApplicator NewApplicator(CargoWise.EntityFramework.BusinessObjectFactory factory, OperationalActionMethodSettings settings) => new SendCargoManifestEntryStatusQueryActionMethodApplicator(factory);

		public override bool HasControl => true;

		public override IComponent NewGuiControl() => new SendCargoManifestEntryStatusQueryActionControl();

		public override bool HasSettings => false;
	}
}
