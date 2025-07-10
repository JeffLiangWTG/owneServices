using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ForwardingConsolFormCustomisationSettingsProvider : FormCustomisationSettingsProvider
	{
		protected override string[] GetPropertiesThatAffectWorkflow()
		{
			return new[]
			{
				JobConsolSchema.JK_TransportMode.Name,
				JobConsolSchema.JK_RL_NKLoadPort.Name,
				JobConsolSchema.JK_RL_NKDischargePort.Name,
				JobConsolSchema.JK_OA_ShippingLineAddress.Name
			};
		}

		protected override FormCustomisableElementCollection GetDisplayTabs()
		{
			FormCustomisableElementCollection result = new FormCustomisableElementCollection();
			result.SuspendValidation();
			result.Add(ResString.GetMultilingualString("e55f04c0-3102-4c74-81ee-bdfb131a09c4", "NCTS"), "NCTSTabPage");
			result.ResumeValidation();
			return result;
		}
	}
}
