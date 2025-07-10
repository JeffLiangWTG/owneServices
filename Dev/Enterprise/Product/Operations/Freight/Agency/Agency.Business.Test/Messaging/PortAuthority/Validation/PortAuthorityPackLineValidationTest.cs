using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[CountrySpecificTest("AU")]
	internal sealed class PortAuthorityPackLineValidationTest : BaseAgencyTest
	{
		public void TestValidateIsPackedOrHasMarks_Containerised()
		{
			const string message = "All packlines need to either be packed or have marks and numbers entered.";

			Shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			PackLine.JL_MarksAndNumbers = "";
			PackLine.JL_JC = Container.PK;
			PackLine.Validation.ValidateJL_JC();
			PackLine.Validation.ValidateJL_MarksAndNumbers();
			AssertNoMessageError(PackLine.JL_JCInfo, message);
			AssertNoMessageError(PackLine.JL_MarksAndNumbersInfo, message);

			PackLine.JL_JC = ZGuid.Empty;
			PackLine.Validation.ValidateJL_JC();
			PackLine.Validation.ValidateJL_MarksAndNumbers();
			AssertHasMessageError(PackLine.JL_JCInfo, message);
			AssertHasMessageError(PackLine.JL_MarksAndNumbersInfo, message);

			PackLine.JL_MarksAndNumbers = "Random Crap";
			PackLine.Validation.ValidateJL_JC();
			PackLine.Validation.ValidateJL_MarksAndNumbers();
			AssertNoMessageError(PackLine.JL_JCInfo, message);
			AssertNoMessageError(PackLine.JL_MarksAndNumbersInfo, message);
		}

		public void TestValidateIsPackedOrHasMarks_NonContainerised()
		{
			const string message = "Marks and numbers are needed for port authority messaging.";

			Shipment.JS_PackingMode = Constants.ContainerModes.BreakBulk;

			PackLine.JL_MarksAndNumbers = "";
			PackLine.Validation.ValidateJL_MarksAndNumbers();
			AssertHasMessageError(PackLine.JL_MarksAndNumbersInfo, message);

			PackLine.JL_MarksAndNumbers = "Random Crap";
			PackLine.Validation.ValidateJL_MarksAndNumbers();
			AssertNoMessageError(PackLine.JL_MarksAndNumbersInfo, message);
		}

		public void TestJL_DetailedDescription()
		{
			const string message = "You have not entered a Detailed Packline Description.";

			PackLine.JL_DetailedDescription = "Fread";
			AssertNoMessageError(packline.JL_DetailedDescriptionInfo, message);

			packline.JL_DetailedDescription = "";
			AssertHasMessageError(packline.JL_DetailedDescriptionInfo, message);
		}

		#region Implementation

		BillOfLading Shipment
		{
			get { return shipment ?? (shipment = Factory.New<BillOfLading>()); }
		}
		BillOfLading shipment;

		BillOfLadingPackLine PackLine
		{
			get { return packline ?? (packline = Shipment.OuterPackLines.AddNew()); }
		}
		BillOfLadingPackLine packline;

		BillOfLadingContainer Container
		{
			get { return container ?? (container = Shipment.RealContainers.AddNew()); }
		}
		BillOfLadingContainer container;

		protected override void SetUp()
		{
			base.SetUp();
			PortAuthorityBusinessObjectValidation.RegisterForFactory(Factory);
		}

		#endregion
	}
}
