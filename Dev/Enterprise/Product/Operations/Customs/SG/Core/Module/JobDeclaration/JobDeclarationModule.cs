using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.SG.V4.Business.PermitPrinting;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.SG.V4.Module
{
	public class JobDeclarationModule : Customs.Module.JobDeclarationModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject() => new JobDeclarationFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new JobDeclarationFilterStripControl(this, GridCollection, FilterBusinessObject);

		protected override Customs.Module.JobDeclarationController GetControllerForStandAlone() => (Customs.Module.JobDeclarationController)ZControllerFactory.Create(ControllerIDs.Customs.JobDeclaration);

		protected override IBusinessObjectCollection GetNewGridCollection() => new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			ZController result;

			var declaration = selectedBusinessObject as JobDeclaration;

			if (declaration != null && declaration.Shipment == null && !(selectedBusinessObject is Customs.Business.DeclarationFromShipmentPuller) && !declaration.IsTradenet4OrITF)
			{
				result = ZControllerFactory.Create(ControllerIDs.Customs.SGV3JobDeclaration);
			}
			else
			{
				result = base.GetNewController(selectedBusinessObject);
			}

			return result;
		}

		#region Menus

		public override MenuItem[] FormActionMenu
		{
			get
			{
				var result = new List<MenuItem>(base.FormActionMenu);
				if (CopyDeclarationOnlyMenuItem != null)
				{
					result.Remove(CopyDeclarationOnlyMenuItem);
				}
				return result.ToArray();
			}
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var menus = new List<MenuItem>(base.GetNewActionMenuItems());

			menus.Insert(menus.Count, new ZMenuItem("-"));
			menus.Insert(menus.Count, new ZMenuItem("Print All Permits", new EventHandler(PrintAllPermits_Click)));
			menus.Insert(menus.Count, new ZMenuItem("Print Selected Permits", new EventHandler(PrintSelectedPermits_Click)));

			return menus.ToArray();
		}

		void PrintSelectedPermits_Click(object sender, EventArgs e)
		{
			PrintPermits(GetSelectedElements());
		}

		void PrintAllPermits_Click(object sender, EventArgs e)
		{
			PrintPermits(GridCollection.ToArray());
		}

		void PrintPermits(BusinessObject[] declarations)
		{
			var menuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Permit"));
			var checkpoint = Env.Security.FindOrCreateDocumentCheckpoint(menuItem.PK.ToGuid(), menuItem.SU_MenuNameMultilingual, ModuleIDs.Customs.JobDeclaration, Env.Security.Operations);
			if (!checkpoint.IsAllowed)
			{
				Globals.Message.Show(checkpoint.ErrorMessageForNotAllowed, "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			else
			{
				DocPack = new DocumentPack();
				var printPermitProcessor = new PrintPermitProcessor();
				if (declarations != null)
				{
					foreach (JobDeclaration declaration in declarations)
					{
						if (!declaration.IsDeleted)
						{
							printPermitProcessor.PrintLastPermit(declaration, DocPack);
						}
					}
				}

				if (DocPack.Count > 0)
				{
					printPermitProcessor.DoPrintTask(DocPack, true);
				}
				else
				{
					Globals.Message.ShowInformation("There are no permits to print");
				}
			}
		}

		protected virtual DocumentPack DocPack
		{
			get { return fDocPack; }
			set { fDocPack = value; }
		}
		DocumentPack fDocPack;

		#endregion
	}
}
