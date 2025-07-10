using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class OrderFormCustomisationSettingsProvider : FormCustomisationSettingsProvider
	{
		protected override string[] GetPropertiesThatAffectWorkflow()
		{
			return new[]
			{
				JobOrderHeaderSchema.JD_OA_BuyerAddress.Name,
				JobOrderHeaderSchema.JD_OA_SupplierAddress.Name,
				JobOrderHeaderSchema.JD_TransportMode.Name
			};
		}
	}
}
