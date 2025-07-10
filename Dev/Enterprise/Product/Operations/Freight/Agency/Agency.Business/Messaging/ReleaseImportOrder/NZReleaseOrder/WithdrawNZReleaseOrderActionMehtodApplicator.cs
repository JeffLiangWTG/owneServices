using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Business
{
	public class WithdrawNZReleaseOrderActionMethodApplicator : NZReleaseOrderActionMethodApplicator
	{
		public WithdrawNZReleaseOrderActionMethodApplicator(ReleaseImportOrderSettings settings)
			: base(Res.GetString("6377e73a-9604-11e4-b643-902b34dc814a", "Withdraw Import Release Order Message"), settings) { }

		public override void SendMessage(INotifications logger, BillOfLadingContainer sourceContainer)
		{
			ZExceptionReporting.ProcessWithConcurrencyHandling(() =>
			{
				var factory = new BusinessObjectFactory
				{
					NameForDebugging = "WithdrawNZReleaseOrderActionMethodApplicatorFactory"
				};

				var container = (BillOfLadingContainer)factory.ImportFromAnotherFactory(sourceContainer);

				using (factory.AddDisposableService())
				{
					using (var exporter = new ManualDataExport(container.Factory, container, UniversalDataType.UniversalShipment, GetCommunicationModes(container), UniversalXmlSchema.Version_2012_11_DO_NOT_USE, dataWriterGetter: GetDataWriterGetter(container, MessagePurposes.Codes.Withdrawal)))
					{
						exporter.EventCode = Events.MessageWithdrawCancelRequest.Code;
						exporter.RecipientType = nameof(RecipientRoleType.PIR);
						exporter.SendData(logger);

						container.Logs.AddNew(
							Events.MessageWithdrawCancelRequest,
							GetMessageParameters(container, Core.Constants.EventReferenceMessageTypes.ImportReleaseOrderWithdrawal).ToArray());
					}
					factory.Save();
				}
				logger.Add(new InfoNotification((NoResString)"Delivery Succeeded."));
			}, () => logger.AddWarning((NoResString)"Error during delivery. Retrying."));
		}
	}
}
