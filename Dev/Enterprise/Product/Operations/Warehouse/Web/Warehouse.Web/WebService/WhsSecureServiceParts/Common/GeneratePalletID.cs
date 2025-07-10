using System;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Application;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region GeneratePalletID

		[WebMethod(Description = "Generate pallet ID for docket.")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public PalletIDWebServiceResponse GeneratePalletID(Guid docketPK, int count)
		{
			return HandleWebServiceRequest<PalletIDWebServiceResponse>(result => GeneratePalletIDCore(result, docketPK, count));
		}

		void GeneratePalletIDCore(PalletIDWebServiceResponse response, Guid docketPK, int count)
		{
			var docket = Factory.Load<WhsDocket>(docketPK);
			if (docket != null)
			{
				BuildPalletID(response, docket, count);
			}
			else
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = Res.GetString("A243D234-D6AC-4BE2-ABAA-70B518CC9D3E", "Docket is invalid. Failed to generate Pallet ID.");
			}
		}

		void BuildPalletID(PalletIDWebServiceResponse response, WhsDocket docket, int count)
		{
			var lineCountToBuildPalletIdFrom = count;
			var generatedId = PalletIDGenerator.GenerateIDs(docket, 1, shouldPrompt: false, lineCountToBuildPalletIdFrom).SingleOrDefault();

			if (!generatedId.HasID)
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = Res.GetString("8125984F-397F-490C-BE92-5524049E97FF", "No IDs left. Failed to generate Pallet ID.");
			}
			else
			{
				response.PalletID = generatedId.FormattedID;
				response.UpdatedCountToBuildFrom = ++generatedId.IDNumber;
			}
		}

		IPalletIDGenerator PalletIDGenerator
		{
			get => palletIDGenerator ??= ObjectFactory.Get<IPalletIDGenerator>();
		}

		IPalletIDGenerator palletIDGenerator;

		#endregion
	}
}
