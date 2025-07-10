using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.TR.NCTS.Business;
using Enterprise.Customs.TR.NCTS.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Module.Testing
{
	[TestedType(typeof(SPTSController))]
	class SPTSControllerTest : ZControllerBasherTest
	{
		public void TestNewFormHasCorrectHeaderType()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.Customs.TR.SimplifiedProcedureTransitSystem);
			using (var form = (SPTSHeaderForm)controller.ShowNewForm())
			{
				var sptsHeader = form.BusinessEntity;
				AssertEquals(NctsMovementType.Codes.Departure, sptsHeader.BH_HeaderType);
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.TR.SimplifiedProcedureTransitSystem;
		}

		protected override string CountryCode
		{
			get { return Core.Constants.CountryCodes.Turkey; }
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var header = Factory.New<SPTSHeader>();
			Factory.Save();
			return header;
		}

		public void TestColumnsForModuleGrid()
		{
			using (var userControl = new SPTSFilterControl(null, null))
			{
				CombineAssertions("Test For The Fields On Module Grid", () =>
				{
					AssertColumnStyles((ZTextBoxColumnStyleInfo)userControl.Grid.ColumnStyles[0], "BH_JobReference", 150);
					AssertColumnStyles((ZDropEditColumnStyleInfo)userControl.Grid.ColumnStyles[1], "MovementHeader+BM_InlandTransportMode", 150);
					AssertColumnStyles((ZTextBoxColumnStyleInfo)userControl.Grid.ColumnStyles[2], "RegistrationNumber", 150);
					AssertColumnStyles((ZDateEditColumnStyleInfo)userControl.Grid.ColumnStyles[3], "RegistrationDate", 150);
					AssertColumnStyles((ZTextBoxColumnStyleInfo)userControl.Grid.ColumnStyles[4], "MovementHeader+BM_PortOfPresentationCode", 150);
					AssertColumnStyles((ZTextBoxColumnStyleInfo)userControl.Grid.ColumnStyles[5], "MovementHeader+BM_DestinationPortCode", 150);
					AssertColumnStyles((ZTextBoxColumnStyleInfo)userControl.Grid.ColumnStyles[6], "BH_VoyageNumber", 150);
					AssertColumnStyles((ZDateEditColumnStyleInfo)userControl.Grid.ColumnStyles[7], "BH_SailingDate", 150);
					AssertColumnStyles((ZTextBoxColumnStyleInfo)userControl.Grid.ColumnStyles[8], "InBondCarrier", 150);
				});
			}
		}

		void AssertColumnStyles(ZTextBoxColumnStyleInfo columnInfo, string columnName, int width)
		{
			AssertEquals("ColumnName", columnName, columnInfo.ColumnName);
			AssertEquals(columnName + "- Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(width), columnInfo.Width);
		}

		void AssertColumnStyles(ZDateEditColumnStyleInfo columnInfo, string columnName, int width)
		{
			AssertEquals("ColumnName", columnName, columnInfo.ColumnName);
			AssertEquals(columnName + "- Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(width), columnInfo.Width);
		}

		void AssertColumnStyles(ZDropEditColumnStyleInfo columnInfo, string columnName, int width)
		{
			AssertEquals("ColumnName", columnName, columnInfo.ColumnName);
			AssertEquals(columnName + "- Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(width), columnInfo.Width);
		}
	}
}

