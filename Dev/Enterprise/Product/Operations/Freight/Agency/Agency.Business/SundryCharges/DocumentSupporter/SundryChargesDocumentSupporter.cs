using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using BusinessContext = CargoWise.Definitions.BusinessContext;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Freight.Agency.Business
{
	public class SundryChargesDocumentSupporter : DocumentSupporter
	{
		public SundryChargesDocumentSupporter(SundryCharges charges)
			: base(charges)
		{
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.AgencySundryCharges; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
		{
			if (contact == ContactType.ShippingLine)
			{
				return new OrgHeaderContact(Charges.BillToParty, null);
			}
			else
			{
				return base.GetContactOrganisation(menuName, contact, direction);
			}
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Charges);
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			return new DataContext[] { DataContext.GenericFreightJob };
		}

		public override ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return dataContext != DataContext.GenericFreightJob
				&& base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
		}

		#region Implementation

		SundryCharges Charges
		{
			get { return (SundryCharges)base.BusinessObject; }
		}

		#endregion
	}
}


