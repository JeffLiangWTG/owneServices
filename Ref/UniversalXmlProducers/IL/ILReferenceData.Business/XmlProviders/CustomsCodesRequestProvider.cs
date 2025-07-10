using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.ILReferenceData.Business.CustomsCodesRequest;

namespace CargoWise.RefDbRepo.ILReferenceData.Business
{
	public class CustomsCodesRequestProvider
	{
		public CustomsCodesRequestProvider(ISYSTBL_NG_9000_MSG_SystemTableRequest systemTableRequestWrapper)
		{
			this.systemTableRequestWrapper = Argument.NotNull(systemTableRequestWrapper, nameof(systemTableRequestWrapper));
		}

		public string GetSystemTableRequest()
		{
			var systemTableRequest = BuildSystemTableRequest();

			var xml = Helpers.Serialize(systemTableRequest);
			return xml;
		}

		SYSTBL_NG_9000_MSG_SystemTableRequest BuildSystemTableRequest()
		{
			var systemTableRequest = new SYSTBL_NG_9000_MSG_SystemTableRequest();

			var requestContentHeader = systemTableRequestWrapper.RequestContentHeader;

			systemTableRequest.RequestContentHeader = new RequestContentHeader()
			{
				TransmitionDateTime = requestContentHeader.TransmitionDateTime,
				RecieverID = new[] { requestContentHeader.RecieverID },
				SenderID = requestContentHeader.SenderID
			};

			systemTableRequest.tableName = systemTableRequestWrapper.TableName;

			if (systemTableRequestWrapper.SelectOptions != null)
			{
				systemTableRequest.SelectOptions = new SYSTBL_NG_9000_MSG_SystemTableRequestSelectOptions()
				{
					GetAsDataTable = systemTableRequestWrapper.SelectOptions.GetAsDataTable,
					PageNumber = systemTableRequestWrapper.SelectOptions.PageNumber,
					PageSize = systemTableRequestWrapper.SelectOptions.PageSize
				};
			}

			return systemTableRequest;
		}


		readonly ISYSTBL_NG_9000_MSG_SystemTableRequest systemTableRequestWrapper;
	}
}
