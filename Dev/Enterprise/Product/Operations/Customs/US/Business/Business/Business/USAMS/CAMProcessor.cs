using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.US.Business
{
	/// <summary>
	/// Creates US AMS for Consol if required.
	/// </summary>
	public class CAMProcessor : Integration.Customs.US.ICAMProcessor, IProcessor
	{
		public CAMProcessor(ForwardingConsol consol)
		{
			this.consol = Argument.NotNull(consol, "consol");
		}
		readonly ForwardingConsol consol;

		public void Process(INotifications notifications, CancellationToken token)
		{
			var amsCreator = consol.IsSea || consol.IsRail ? (Integration.Customs.US.USAMS.IUSAMSCreator)ObjectFactory.New<Integration.Customs.US.USAMS.ICusUSAMSCreator>(consol)
				: (consol.IsAir ? ObjectFactory.New<Integration.Customs.ASYCUDA.ACEManifest.IUSAMSImportAirManifestCreator>(consol) : null);

			if (amsCreator != null)
			{
				var errorMessage = amsCreator.CheckSupported;
				if (errorMessage.IsEmpty)
				{
					using (amsCreator.TryCreateUSAMSData(notifications))
					{
					}
				}
				else
				{
					notifications.AddError(errorMessage);
				}
			}
			else
			{
				notifications.AddError(Res.GetString("512F0BF9-582E-4F79-9D77-FE0791E25741", "Consol is not valid of US AMS"));
			}
		}
	}
}
