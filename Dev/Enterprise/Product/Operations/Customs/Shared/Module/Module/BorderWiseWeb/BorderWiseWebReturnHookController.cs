using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module
{
	public class BorderWiseWebReturnHookController : ZController
	{
		public override ControllerID ID => ControllerIDs.BorderWiseWebReturnHook;

		public override ModuleIdentifier ModuleID => ModuleIDs.BorderWiseWebReturnHook;

		public override Type TypeOfTopLevelBusinessObject => typeof(BorderWiseReturnHookNonPersistentBizo);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			return new BorderWiseReturnHookNonPersistentBizo(sourceEntityPK);
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new BorderWiseReturnHookNonPersistentBizo();
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return null; // This module represents a form that is shown manually
		}

		public override void SwitchToFormFor(IBusiness entity)
		{
			base.SwitchToFormFor(entity);

			var form = (WaitingForResponseFromBorderWiseWebMessageBox)GetOpenedForm(entity);
			var args = ArgsForNewForm.ToArray();

			if (args.Length > 0)
			{
				var borderWiseInvoiceLine = new BorderWiseInvoiceLine(args[0], "");

				if (args.Length > 1)
				{
					borderWiseInvoiceLine.StatCode = args[1];
				}

				form.FormBizo.BorderWiseInvoiceLines = new List<BorderWiseInvoiceLine>() { borderWiseInvoiceLine };

				form.Dispose();
			}
		}
	}
}
