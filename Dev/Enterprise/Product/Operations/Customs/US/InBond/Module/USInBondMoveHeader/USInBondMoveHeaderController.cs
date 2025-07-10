using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Customs.US.InBond.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.InBond.Module
{
	public class USInBondMoveHeaderController : ZController
	{
		public override ControllerID ID => ControllerIDs.Customs.US.InBondMoveHeader;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.US.InBondMoveHeader;

		public override Type TypeOfTopLevelBusinessObject => typeof(USInBondMoveHeader);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.USInBondEdit;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.USInBondEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new USInBondForm(((USInBondMoveHeader)businessEntity).Header);
		}

		protected override IZForm ShowLoadedForm(IBusiness sourceEntity, FormAction action)
		{
			IZForm result = null;

			if (sourceEntity is USInBondMoveHeader moveHeader && moveHeader.Header is CusInBondHeader header)
			{
				if (header.Parent is ForwardingShipment shipment)
				{
					var shipmentController = (CusInBondHeaderShipmentController)ZControllerFactory.Create(ControllerIDs.Customs.US.InBondPluggedIntoShipment);
					shipmentController.ShowLoadedForm(shipment, action, true);
					result = shipmentController.LastShownForm;
				}
				else if (header.Parent is JobDeclaration declaration)
				{
					var declarationController = (CusInBondHeaderDeclarationController)ZControllerFactory.Create(ControllerIDs.Customs.US.InBondPluggedIntoDeclaration);
					declarationController.ShowLoadedForm(declaration, action, true);
					result = declarationController.LastShownForm;
				}
				else
				{
					var inBondController = (CusInBondHeaderController)ZControllerFactory.Create(ControllerIDs.Customs.US.InBond);
					inBondController.ShowLoadedForm(header, action, true);
					result = inBondController.LastShownForm;
				}

				AddRecentItems(sourceEntity, moveHeader.HumanReadableShortcutName);
				SelectMoveHeader(result, moveHeader.MoveHeader);
				LastShownForm = result;
			}

			return result;
		}

		void AddRecentItems(IBusiness sourceEntity, string description)
		{
			var favoriteProvider = ObjectFactory.Get<Core.Modules.IFavoriteProvider>();
			favoriteProvider.AddToRecentItems(new ZArchitecture.Favorites.LinkWrapper(
				ModuleID.Name,
				sourceEntity.Identifier.ToGuid(),
				ZArchitecture.ShowEditFormUrlHandler.Instance.Create(ID, sourceEntity.Identifier),
				description
			));
		}

		void SelectMoveHeader(IZForm form, Business.CusInBondMoveHeader moveHeader)
		{
			if (moveHeader != null && form is ZForm zForm)
			{
				USInBondHeaderDetailUserControl headerDetailsUserControl = null;

				if (zForm.FindSingleOrDefault<USInBondUserControl>() is USInBondUserControl inBondUserControl)
				{
					inBondUserControl.MainTabControl.SelectedTab = inBondUserControl.DetailsTabPage;
					headerDetailsUserControl = inBondUserControl.DetailsTabPage?.FindSingleOrDefault<USInBondHeaderDetailUserControl>();
				}
				else
				{
					headerDetailsUserControl = zForm.FindSingleOrDefault<USInBondHeaderDetailUserControl>();
				}

				if (headerDetailsUserControl != null)
				{
					var movementHeadersGrid = headerDetailsUserControl?.MovementHeadersGrid;
					if (movementHeadersGrid != null && movementHeadersGrid.List != null)
					{
						movementHeadersGrid.SelectSingleElement(moveHeader);
					}
				}
			}
		}
	}
}
