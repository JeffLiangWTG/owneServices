using System.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business.Testing
{
	class DummyDocumentSupportable : DummyBusinessObject, IDocumentSupportable
	{
		public DummyDocumentSupportable(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		DocumentSupporter IDocumentSupportable.DocumentSupporter => new DummySupporter(this);

		string IDocumentSupportable.TableName => TableName;

		public ZGuid ConsigneePK { get; set; }

		public OrgHeader Consignee => Factory.Load<OrgHeader>(ConsigneePK);
	}

	class DummySupporter : DocumentSupporter
	{
		public DummySupporter(BusinessObject bizO)
			: base(bizO)
		{
		}

		public override BusinessContext BusinessContext => BusinessContext.Test;

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => null;

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun) => System.Array.Empty<DocumentWrapper>();

		protected override Core.Constants.DataContext[] GetSupportedDataContexts() => System.Array.Empty<Core.Constants.DataContext>();

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			IDocumentDeliveryContact result = null;
			if (contactType == ContactType.Consignee)
			{
				result = new OrgHeaderContact(((DummyDocumentSupportable)BusinessObject).Consignee, null);
			}
			return result;
		}
	}
}
