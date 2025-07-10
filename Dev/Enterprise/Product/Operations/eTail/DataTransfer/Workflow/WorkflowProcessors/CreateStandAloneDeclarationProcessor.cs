using System.Threading;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.eTail.Business;
using Enterprise.eTail.Integration;

namespace Enterprise.eTail.DataTransfer
{
	public class CreateStandAloneDeclarationProcessor : IProcessor
	{
		public CreateStandAloneDeclarationProcessor(HVLVConsignment consignment)
		{
			Consignment = consignment;
		}

		HVLVConsignment Consignment { get; }

		public void Process(INotifications notifications, CancellationToken token = default)
		{
			var response = ConvertToStandAloneDeclarationService.ConvertToStandAloneDeclaration(Consignment);
			if (!response.ConversionSucceeded)
			{
				notifications.AddError(ResString.GetMultilingualString("1878f595-9542-4b85-802f-8bd157c852c2", "Stand Alone Declaration could not be created for HVLV Consignment ({0}): {1}", Consignment.HVC_WaybillNumber, response.ConversionFailureReason));
			}
		}

		IConvertToStandAloneDeclarationService ConvertToStandAloneDeclarationService => convertToStandAloneDeclarationService ?? (convertToStandAloneDeclarationService = ObjectFactory.Get<IConvertToStandAloneDeclarationService>());
		IConvertToStandAloneDeclarationService convertToStandAloneDeclarationService;
	}
}
