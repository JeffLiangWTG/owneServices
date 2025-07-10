using System.Data;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class OrgAddressForTest : OrgAddress
	{
		public OrgAddressForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		public override async Task<WebAddressValidationResult> ValidateAddressAsync(CancellationTokenSource cancellationToken, CleanseAction cleanseAction = CleanseAction.ValidateAndSuggest)
		{
			ValidationStatus = AddressValidationStatus.Invalid;
			return await Task.FromResult(new WebAddressValidationResult());
		}
	}
}
