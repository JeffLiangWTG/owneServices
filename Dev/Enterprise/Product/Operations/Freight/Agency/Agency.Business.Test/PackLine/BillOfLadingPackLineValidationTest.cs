using CargoWise.Types;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[CountrySpecificTest("AU")]
	internal class BillOfLadingPackLineValidationTest : BaseFreightTest
	{
		public void TestValidatePackedIntoEmptyContainer()
		{
			const string message = "This container has been marked as empty.";
			PackLine.JL_JC = ZGuid.Empty;
			PackLine.Validation.ValidateJL_JC();
			AssertNoError(PackLine.JL_JCInfo, message);
			Container.JC_IsEmptyContainer = true;
			PackLine.JL_JC = Container.PK;
			AssertHasError(PackLine.JL_JCInfo, message);
			Container.JC_IsEmptyContainer = false;
			packline.Validation.ValidateJL_JC();
			AssertNoError(PackLine.JL_JCInfo, message);
		}

		#region Implementation

		BillOfLading Shipment
		{
			get
			{
				return shipment ?? (shipment = Factory.New<BillOfLading>());
			}
		}

		BillOfLading shipment;
		BillOfLadingPackLine PackLine
		{
			get
			{
				return packline ?? (packline = Shipment.OuterPackLines.AddNew());
			}
		}

		BillOfLadingPackLine packline;
		BillOfLadingContainer Container
		{
			get
			{
				return container ?? (container = Shipment.RealContainers.AddNew());
			}
		}

		BillOfLadingContainer container;
		#endregion
	}
}
