using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.CFS.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.CFS.Module
{
	public class PackContainerRegistrationController : ZController, IContainerCopyAndTransformSupporter
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		#region Standard Module Overrides

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.PackContainerRegistration; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.PackContainerRegistration; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CFSContainer); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CFSContainerForm((CFSContainer)businessEntity);
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			CFSContainer container = Factory.New<CFSContainer>();
			return container;
		}
		#endregion

		#region Security Checkpoints

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.CFSContainerRegistration; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.CFSContainerRegistrationModify; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.CFSContainerRegistrationModify; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.CFSContainerRegistrationModify; }
		}

		#endregion

		#region IContainerCopyAndTransformSupporter Members

		IZForm IContainerCopyAndTransformSupporter.ShowCopyFormWithTransform(BusinessObject cFSContainer)
		{
			IZForm form = ShowCopyForm(cFSContainer, delegate(IBusiness container)
			{ return ((CFSContainer)container).CreateNewContainer(); });
			form.DisplayMode = ODisplayMode.Edit;
			return form;
		}

		#endregion

		#region SetStrategyProvider

		protected override void SetStrategyProvider(BusinessObjectFactory factory)
		{
			factory.SetFreightDomainContext(FreightDomainContext.CFS);
		}

		#endregion
	}
}
