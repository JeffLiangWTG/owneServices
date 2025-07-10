using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Module;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.GUI;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Customs.US.InBond.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Module
{
	/// <summary>
	/// This controller is used when an In-Bond Movement is attached to a Declaration: to show the JobDeclarationForm from the In-Bond module
	/// </summary>
	public class CusInBondHeaderDeclarationController : JobDeclarationController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.US.InBondPluggedIntoDeclaration; }
		}

		protected override string GetIDForFormCache(IBusiness businessEntity)
		{
			return ControllerIDs.Customs.JobDeclaration.ToString();
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.US.InBond; }
		}

		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			IZForm result = null;
			var inBond = businessEntity as CusInBondHeader;
			var declaration = inBond == null ? businessEntity as JobDeclaration : inBond.Declaration;
			if (declaration != null)
			{
				var declarationForm = new JobDeclarationForm(declaration);
				declarationForm.PlugInIDToSelectOnLoaded = ControllerIDs.Customs.US.InBond;
				declarationForm.SkipRecentItems = skipRecentItems;
				result = declarationForm;
			}
			else
			{
				var inBondForm = new USInBondForm(inBond);
				inBondForm.SkipRecentItems = skipRecentItems;
				result = inBondForm;
			}
			return result;
		}
		bool? skipRecentItems;

		protected override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			IBusiness result = null;
			var inBond = factory.Load<CusInBondHeader>(sourceEntityPK);
			if (inBond != null)
			{
				result = inBond.Parent;
			}
			else
			{
				var declaration = factory.Load<JobDeclaration>(sourceEntityPK);
				if (declaration != null)
				{
					result = declaration;
				}
			}
			return result;
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CusInBondHeader); }
		}

		public override ResourceStringData PluginTabPageCaption { get { return Enterprise.Customs.US.InBond.Module.Res.GetData("PlugInTabPage|USInBond", "In-Bond"); } }

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new InBondPlugIn((ICusInBondParent)businessEntity);
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.USInBondEdit; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.USInBondView; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.USInBondNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		#endregion

		public IZForm ShowLoadedForm(IBusiness sourceEntity, FormAction action, bool skipRecentItemsToSet)
		{
			skipRecentItems = skipRecentItemsToSet;
			return base.ShowLoadedForm(sourceEntity, action);
		}

		public override ZGuid LastSavedPK
		{
			get
			{
				var form = LastShownForm as ZForm;
				IIdentified businessObject = form == null ? null : form.DataSource as IIdentified;
				if (businessObject != null)
				{
					var declaration = businessObject as JobDeclaration;
					if (declaration != null)
					{
						var inBond = declaration.GetInBondHeader(CusInBondApplicationCodeList.Codes.InBond);
						if (inBond != null)
						{
							return inBond.PK;
						}
					}
					else
					{
						var inBond = businessObject as CusInBondHeader;
						if (inBond != null)
						{
							return inBond.PK;
						}
					}
				}

				return base.LastSavedPK;
			}
		}
	}
}
