using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business
{
	/// <summary>
	/// Creates CusMAWB for Consol and CusHAWBs for its Shipments if required.
	/// </summary>
	public class CCRProcessor : Integration.Customs.Shared.ICCRProcessor, IProcessor
	{
		public CCRProcessor(ForwardingConsol consol, string countryCode)
		{
			Argument.NotNull(consol, "consol");

			this.consol = consol;
			this.countryCode = countryCode;
		}

		public void Process(INotifications notifications, CancellationToken token)
		{
			ICargoSafeCreator safeCreator = consol.IsAir ? new CusMAWBSafeCreator(consol) :
											(consol.IsSea ? new CusSCAOceanBillSafeCreator(consol) : null);

			if (safeCreator == null || !safeCreator.IsSupported(countryCode))
			{
				notifications.AddError(Res.GetString("E1847A93-655B-4D44-8F52-918B10B27E90", "Automated creation of {0} Customs Cargo Record is not available for this transport mode {1}.", countryCode, consol.HumanReadableName));
			}
			else if (!consol.IsImportTo(countryCode))
			{
				notifications.AddError(Res.GetString("95D5C75D-734F-4C24-A4B9-3F502C73A9FB", "Automated creation of {0} Customs Cargo Record is available for {0} import consolidations only.", countryCode));
			}
			else
			{
				var receivingAgent = consol.ReceivingForwarder;
				if (receivingAgent == null)
				{
					notifications.AddError(Res.GetString("9AF1F83D-1D44-4769-8EBD-66D2FE9715DD", "Receiving Agent is required for automated creation of {0} Customs Cargo Record.", countryCode));
				}
				else
				{
					var existingCargo = safeCreator.GetExistingCargo();
					var matchingBranch = existingCargo != null ? existingCargo.Branch : new GlbBranch.Loader(consol.Factory).LoadActiveMatchingBranchInThisCountry(receivingAgent, countryCode);
					if (matchingBranch != null)
					{
						using (DisposableEnvironment.ForBranch(matchingBranch.PK.ToGuid()))
						using (safeCreator.TryCreateOrUpdateCargoAndChildBills(notifications, out var cargo))
						{
							if (existingCargo == null && cargo != null)
							{
								LogCargoCreatedEvent();
							}
						}
					}
					else
					{
						notifications.AddError(Res.GetString("0F6932E2-17D3-438F-8CCA-A262BA1579C3", "Receiving Agent entered does not match any Organization of active {0} companies or its branches.", countryCode));
					}
				}
			}
		}

		void LogCargoCreatedEvent()
		{
			switch (countryCode)
			{
				case Core.Constants.CountryCodes.Australia:
					consol.Logs.AddNew(Events.CargoReportRecordsCreated);
					break;
			}
		}

		readonly ForwardingConsol consol;
		readonly string countryCode;
	}
}
