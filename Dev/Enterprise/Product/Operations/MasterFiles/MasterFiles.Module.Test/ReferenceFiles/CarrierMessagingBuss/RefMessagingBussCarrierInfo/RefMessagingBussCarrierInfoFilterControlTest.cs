using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	class RefMessagingBussCarrierInfoFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestColumns()
		{
			var searchBO = new RefMessagingBussCarrierInfoFilterBusinessObject();
			var collection = new RefMessagingBussCarrierInfoCollection(Factory);

			using (var filterControl = new RefMessagingBussCarrierInfoFilterControl(collection, searchBO))
			{
				var columns = filterControl.FilteredGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => $"{x} IsVisible:{x.IsVisible}").ToArray();

				AssertArrayEqualsByElements(new[]
				{
					$"{RefMessagingBussCarrierInfoSchema.Constants.ZMC_CarrierCode} (ZTextBoxColumnStyleInfo) IsVisible:True",
					$"{RefMessagingBussCarrierInfoSchema.Constants.ZMC_CarrierName} (ZTextBoxColumnStyleInfo) IsVisible:True",
					$"{RefMessagingBussCarrierInfoSchema.Constants.ZMC_CountryCode} (ZTextBoxColumnStyleInfo) IsVisible:True"
				}, columns);
			}
		}
	}
}
