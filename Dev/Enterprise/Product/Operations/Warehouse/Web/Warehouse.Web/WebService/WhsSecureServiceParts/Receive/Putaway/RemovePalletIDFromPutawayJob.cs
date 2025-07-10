using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	partial class WhsSecureService
	{
		[WebMethod(Description = "Remove given pallet from user's current putaway job")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse RemovePalletIDFromPutawayJob(string palletID)
		{
			return HandleWebServiceRequest<WebServiceResponse>(r => RemovePalletIDFromPutawayJob(r, palletID));
		}

		void RemovePalletIDFromPutawayJob(WebServiceResponse response, string palletID)
		{
			var staff = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
			var warehouse = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);

			if (staff != null && warehouse != null)
			{
				LoadJobAndRemovePalletIfExists();
			}
			else
			{
				response.LogBusinessValidationError(Res.GetString("7f020eee-dca0-4421-9fa8-776f35c91f4f", "Please provide login credentials to use this service."));
			}

			void LoadJobAndRemovePalletIfExists()
			{
				var job = PutawayHelper.LoadUnfinalisedPutawayJob(Factory, warehouse, staff);
				if (job == null)
				{
					response.LogBusinessValidationError(Res.GetString("b9634a2f-e3db-48f7-9f13-940cddf15737", "Putaway Job cannot be found."));
				}
				else
				{
					var line = job.Lines.FirstOrDefault(l => l.WPL_PalletID.EqualsIgnoringCase(palletID));
					if (line == null)
					{
						response.LogBusinessValidationError(Res.GetString("1983c5e3-430d-4272-ba2d-1db14eb79387", "Pallet {0} cannot be found in current putaway job.", palletID));
					}
					else
					{
						var putawayTransferLine = Factory.LoadTop1<WhsTransferLine>(GetTransferLineQuery());
						if (putawayTransferLine != null)
						{
							var connection = ((IDbConnected)Factory).Connection;
							using (var manager = connection.BeginTransactionWithManager())
							{
								putawayTransferLine.WE_GS_NKPutawayBy = "";
								line.WPL_IsPuttingAway = false;
								Factory.Save();

								if (!line.WPL_IsPuttingAway)
								{
									job.Lines.Delete(line);
									Factory.Save();
								}
								manager.CommitTransaction();
							}
						}
						else
						{
							response.LogBusinessValidationError(Res.GetString("691253ff-f99a-4405-8a86-4d6384c4381a", "Putaway transfer line for pallet {0} cannot be found.", palletID));
						}
					}
				}
			}

			ZDBOnlyQuery GetTransferLineQuery()
			{
				var docketQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketSchema.PK);
				docketQuery.AddToFilter(WhsDocketSchema.WD_WW_Whs, SQLComparisonOperator.Equal, warehouse.PK);
				docketQuery.AddToFilter(WhsDocketSchema.WD_IsPutawayTransfer, true);
				docketQuery.AddToFilter(WhsDocketSchema.WD_DocketStatus, SQLComparisonOperator.NotEqual, DocketStatus.Codes.Finalised);

				var lineQuery = new ZDBOnlyQuery(typeof(WhsTransferLine));
				lineQuery.AddToFilter(WhsDocketLineSchema.WE_DocketLineType, SQLComparisonOperator.Equal, DocketType.Codes.Transfer);
				lineQuery.AddToFilter(WhsDocketLineSchema.WE_TransferFromPalletId, palletID);
				lineQuery.AddToFilter(WhsDocketLineSchema.WE_GS_NKPutawayBy, staff.GS_Code);
				lineQuery.AddToFilter(WhsDocketLineSchema.WE_DocketLineStatus, SQLComparisonOperator.NotEqual, DocketStatus.Codes.Finalised);
				lineQuery.AddSubQuery(WhsDocketLineSchema.WE_WD, docketQuery, JoinCondition.And);
				return lineQuery;
			}
		}
	}
}
