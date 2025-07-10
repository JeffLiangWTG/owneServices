using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class ForceReplenishmentToPickFaceActionMethodApplicator : WhsOperationalActionMethodApplicator
	{
		public ForceReplenishmentToPickFaceActionMethodApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("f342f13a-22e9-42e7-8244-97f0d74d9c07", "Force Replenishment for Selected Pick Faces."), factory)
		{
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			var validPickFaces = ValidatePickFaces(log, targets.Cast<WhsPickFaceView>());
			if (validPickFaces.Any())
			{
				AddFetchHints(Factory, validPickFaces);
				var pickFaceInfos = CreatePickFaceInfos(validPickFaces);
				ObjectFactory.Get<IPickFaceCreateTransfers>().CreateAndSaveTransfers(pickFaceInfos, new TransferLogWrapper(log), allowMultipleTransfersToReplenish: true);
			}
			else
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, Res.GetString("0cac900b-f21c-41af-b585-4edb62c6942a", "Did not find any Pick Faces that needed replenishing."));
			}
		}

		void AddFetchHints(BusinessObjectFactory factory, IEnumerable<WhsPickFaceView> pickFaces)
		{
			foreach (var pickFace in pickFaces)
			{
				factory.AddFetchHint(WhsLocationViewSchema.PK, pickFace.WPV_WL);    // Get Location to display in the log message
				if (pickFace.WPV_OP.IsValid)
				{
					factory.AddFetchHint(OrgSupplierPartSchema.PK, pickFace.WPV_OP);    // Get SupplierPart to display in the log message
				}
			}
		}

		#region Validation

		IEnumerable<WhsPickFaceView> ValidatePickFaces(IOperationalActionSectionLog log, IEnumerable<WhsPickFaceView> pickfaces)
		{
			foreach (var locationString in pickfaces.Where(f => !f.IsPickFaceAssigned).Select(pickFace => pickFace.Location.ToLocationString()).Distinct())
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, Res.GetString("1a1e56a5-f172-4663-9593-ad832ed0fb1a", "Location: {0} is not assigned to a Pick Face therefore it cannot be replenished and has been ignored."), locationString);
			}

			var assignedPickfaces = pickfaces.Where(f => f.IsPickFaceAssigned).ToArray();
			foreach (var pickFaceView in assignedPickfaces.Where(f => !f.ReplenishMultipleTransferCanFit))
			{
				log.NotifyFormat(
					OperationalActionLogErrorLevel.Warning,
					Res.GetString("2574a285-409e-433a-9e10-8e1425f33fb6", "Pickface Location: {0} for Client: {1} and Product: {2} does not have capacity for a single Replenish Multiple, therefore it cannot be replenished and has been ignored."),
						pickFaceView.Location.ToLocationString(), pickFaceView.Client.OH_FullName, pickFaceView.SupplierPart.OP_PartNum);
			}

			return assignedPickfaces.Where(f => f.ReplenishMultipleTransferCanFit);
		}

		#endregion

		#region Implementation

		class TransferLogWrapper : IWhsDocketCreationLogger
		{
			public TransferLogWrapper(IOperationalActionSectionLog logger)
			{
				Logger = logger;
			}

			IOperationalActionSectionLog Logger { get; }

			void IWhsDocketCreationLogger.LogFailure(string message) => Logger.NotifyFormat(OperationalActionLogErrorLevel.Warning, message);

			void IWhsDocketCreationLogger.LogSuccess(string message) => Logger.NotifyFormat(OperationalActionLogErrorLevel.Informational, message);

			void IWhsDocketCreationLogger.LogWarning(string message) => Logger.NotifyFormat(OperationalActionLogErrorLevel.Warning, message);

			public void LogHyperLinkSuccess(WhsDocket docket, string message) => Logger.NotifyFormat(OperationalActionLogErrorLevel.Informational, message, GetDocketIdLink(docket));
		}

		IEnumerable<IPickFaceInfo> CreatePickFaceInfos(IEnumerable<WhsPickFaceView> pickfaces)
		{
			foreach (var pickFace in pickfaces)
			{
				var replenishAmt = pickFace.WPV_ReplenishMaximum - (pickFace.WPV_AvailableToPick + pickFace.WPV_Incoming);
				yield return new PickFaceInfo(
					pickFace.WPV_WW_Whs,
					pickFace.WPV_OH,
					pickFace.WPV_OP,
					pickFace.WPV_WL,
					replenishAmt > 0m ? replenishAmt : 0m,
					pickFace.WPV_ReplenishmentMultiple,
					false);
			}
		}

		#endregion
	}
}
