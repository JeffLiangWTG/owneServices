using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.GUI;
using Enterprise.Security;
using Enterprise.TransportCommon.Registry;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.Module
{
	public class CartageWorkSheetController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => !FreightConfigurationRegistry.Instance.AllowInterCompanyHyperlinksToBeOpenedInReceivingCompany.Value;

		public CartageWorkSheetController()
			: this(GetDefaultDay())
		{
		}

		public CartageWorkSheetController(ZDateTime worksheetDateTime)
		{
			WorksheetDateTime = worksheetDateTime;
		}

		ZDateTime WorksheetDateTime { get; }

		static ZDateTime GetDefaultDay()
		{
			switch (TransportRegistry.Instance.DefaultRunSheetDay.Value)
			{
				case Constants.RunSheetNewModes.RS0_Today:
					return ZDateTime.Today;
				case Constants.RunSheetNewModes.RS1_Tomorrow:
					return ZDateTime.Today.AddDays(1);
				case Constants.RunSheetNewModes.RS2:
					return ZDateTime.Today.AddDays(2);
				case Constants.RunSheetNewModes.RS3:
					return ZDateTime.Today.AddDays(3);
				case Constants.RunSheetNewModes.RS4:
					return ZDateTime.Today.AddDays(4);
				case Constants.RunSheetNewModes.RS5:
					return ZDateTime.Today.AddDays(5);
				case Constants.RunSheetNewModes.RS6:
					return ZDateTime.Today.AddDays(6);
				default:
					return ZDateTime.Today;
			}
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.CartageWorkSheet; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.CartageWorkSheet; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CommonWorkSheet); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var workSheet = (CommonWorkSheet)businessEntity;
			workSheet.IsRoot = true;
			return new CartageWorkSheetForm(workSheet);
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var runSheet = Factory.New<CommonWorkSheet>();
			runSheet.EY_StartTime = WorksheetDateTime;
			runSheet.EY_EndTime = WorksheetDateTime.EndOfDay().AddSeconds(-59);
			return runSheet;
		}

		protected override void SetStrategyProvider(BusinessObjectFactory factory)
		{
			CommonCartageBehaviorStrategyProvider.SetProvider(factory, new CartageBehaviorStrategyProvider());
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.LocalTransportRunSheet; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.LocalTransportRunSheetNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.LocalTransportRunSheetEdit; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.LocalTransportRunSheetDelete; }
		}
	}
}
