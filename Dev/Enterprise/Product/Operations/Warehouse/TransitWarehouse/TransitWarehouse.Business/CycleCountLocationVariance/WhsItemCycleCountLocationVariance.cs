using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Transit.Business
{
	[CodeAlive("This Business Object is used in Glow.")]
	public class WhsItemCycleCountLocationVariance : AutoWhsItemCycleCountLocationVariance, IWhsItemCycleCountLocationVariance
	{
		public WhsItemCycleCountLocationVariance(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Trigger Identifier.")]
		public const string PreventDeleteTriggerError = "Cycle Count Variances cannot be deleted.";

		#region CycleCountLocation

		public WhsItemCycleCountLocation CycleCountLocation
		{
			get { return Factory.Load<WhsItemCycleCountLocation>(WIV_WIC_CycleCountLocation); }
		}

		[RelatedBusinessObject("CycleCountLocation")]
		public override ZGuid WIV_WIC_CycleCountLocation { get => base.WIV_WIC_CycleCountLocation; set => base.WIV_WIC_CycleCountLocation = value; }

		#endregion

		#region ExpectedLocation

		public WhsLocation ExpectedLocation
		{
			get { return Factory.Load<WhsLocation>(WIV_WL_ExpectedStockLocation); }
		}

		[RelatedBusinessObject("ExpectedLocation")]
		public override ZGuid WIV_WL_ExpectedStockLocation { get => base.WIV_WL_ExpectedStockLocation; set => base.WIV_WL_ExpectedStockLocation = value; }

		#endregion

		#region ExpectedHandlingUnit

		public WhsItemPackageState ExpectedHandlingUnit
		{
			get { return Factory.Load<WhsItemPackageState>(WIV_WPS_ExpectedHandlingUnit); }
		}

		[RelatedBusinessObject("ExpectedHandlingUnit")]
		public override ZGuid WIV_WPS_ExpectedHandlingUnit { get => base.WIV_WPS_ExpectedHandlingUnit; set => base.WIV_WPS_ExpectedHandlingUnit = value; }

		#endregion

		#region PackageState

		public WhsItemPackageState PackageState
		{
			get { return Factory.Load<WhsItemPackageState>(WIV_WPS_PackageState); }
		}

		[RelatedBusinessObject("PackageState")]
		public override ZGuid WIV_WPS_PackageState { get => base.WIV_WPS_PackageState; set => base.WIV_WPS_PackageState = value; }

		#endregion

		#region TransferLine

		public WhsItemTransferLine TransferLine
		{
			get { return Factory.Load<WhsItemTransferLine>(WIV_WTF_TransferLine); }
		}

		[RelatedBusinessObject("TransferLine")]
		public override ZGuid WIV_WTF_TransferLine { get => base.WIV_WTF_TransferLine; set => base.WIV_WTF_TransferLine = value; }

		#endregion

		#region ReceiveConsignment

		public WhsItemReceiveConsignment ReceiveConsignment
		{
			get { return Factory.Load<WhsItemReceiveConsignment>(WIV_WRC_ReceiveConsignment); }
		}

		[RelatedBusinessObject("ReceiveConsignment")]
		public override ZGuid WIV_WRC_ReceiveConsignment { get => base.WIV_WRC_ReceiveConsignment; set => base.WIV_WRC_ReceiveConsignment = value; }

		#endregion
	}
}
