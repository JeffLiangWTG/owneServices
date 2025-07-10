using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.PickByLabel;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region CancelPickByLabel

		[WebMethod(Description = "Cancel pick by label.")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WebServiceResponse CancelPickByLabel(string packageId)
		{
			return HandleWebServiceRequest<WebServiceResponse>(r => CancelPickByLabelCore(r, packageId));
		}

		void CancelPickByLabelCore(WebServiceResponse response, string packageId)
		{
			var warehouse = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
			var rfUser = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
			var finder = WhsPickByLabelHelper.PickByLabelPackageFinder(Factory, packageId, warehouse.PK, rfUser);

			var package = PickByLabelHelper.FindPickByLabelPackage(response, finder, packageId);
			if (response.ErrorMessage.IsNullOrEmpty())
			{
				var existingLabel = Factory.LoadTop1<WhsPickByLabelLabel>(new ZQuery(WhsPickByLabelLabelSchema.WTL_KP_Package, package.PK));

				if (existingLabel == null)
				{
					response.LogError(ErrorTypes.BusinessValidationError,
						Res.GetString("50fa47ac-442e-4b0f-9572-bfb817a5202a", "Pick by Label task for label '{0}' does not exist.", packageId));
				}
				else
				{
					var pickByLabelJob = existingLabel.PickByLabelJob;
					if (pickByLabelJob != null && pickByLabelJob.WTK_FinalisedDate.IsValid)
					{
						response.LogError(ErrorTypes.BusinessValidationError,
								Res.GetString("cd5e6c45-df22-4d1f-834c-700f46cc8723", "Label '{0}' cannot be canceled as the job is already finalized.", packageId));
					}
					else
					{
						var pickLines = package.GetPickLines();
						if (pickLines != null && pickLines.Any(pickLine => pickLine.IsPickedFromPutawayLocation))
						{
							response.LogError(ErrorTypes.BusinessValidationError,
								Res.GetString("90c4896b-5032-4e5a-9574-de0d43b2bb78", "Label '{0}' is already picked and cannot be canceled.", packageId));
						}
						else
						{
							CancelPickByLabel(response, existingLabel, pickLines);
						}
					}
				}
			}
		}

		void CancelPickByLabel(WebServiceResponse response, WhsPickByLabelLabel label, IEnumerable<WhsPickLine> pickLines)
		{
			var errorMessage = ObjectFactory.Get<IWhsPickDockDoorAssignmentService>().RemovePickDockDoorAssignment(label.WTL_KP_Package, Factory);
			if (string.IsNullOrEmpty(errorMessage))
			{
				WhsPickByLabelHelper.CancelPickByLabel(label);
				if (pickLines != null)
				{
					UnAssignPickLines(pickLines);
				}

				try
				{
					Factory.Save();
				}
				catch (ZCannotSaveException ex)
				{
					response.LogError(ErrorTypes.BusinessValidationError, ex.Message);
				}
				catch (ZSaveException ex)
				{
					response.LogError(ErrorTypes.BusinessValidationError, ex.FriendlyMessage);
				}
			}
			else
			{
				response.LogBusinessValidationError(errorMessage);
			}
		}

		static void UnAssignPickLines(IEnumerable<WhsPickLine> pickLines)
		{
			foreach (var pickLine in pickLines)
			{
				pickLine.WZ_GS_NKAssignedTo = string.Empty;
				pickLine.WZ_IsPicking = false;
			}
		}

		#endregion
	}
}
