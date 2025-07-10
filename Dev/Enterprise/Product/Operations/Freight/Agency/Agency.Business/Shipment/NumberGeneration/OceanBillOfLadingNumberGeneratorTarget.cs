using Enterprise.Freight.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Business
{
	internal sealed class OceanBillOfLadingNumberGeneratorTarget : BillOfLadingNumberGeneratorTarget
	{
		public override string NumberCustomisationLocation
		{
			get { return (NoResString)"Liner & Agency -> Bill Of Lading Number Customization"; } // points to non-localised registry entry
		}

		protected override BillOfLadingNumberCustomisation GetNumberCustomisationCore()
		{
			return Context.AccessRegistry(AgencyRegistry.Instance.OceanBillNumberCustomisation);
		}
	}
}
