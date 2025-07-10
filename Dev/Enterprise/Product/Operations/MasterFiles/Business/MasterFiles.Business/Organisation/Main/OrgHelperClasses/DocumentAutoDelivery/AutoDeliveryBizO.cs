using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business
{
	[ExcludeDocumentSupporterTest]
	public class AutoDeliveryBizO : DocumentSupporter
	{
		public AutoDeliveryBizO(OrgHeader organisation)
			: this(organisation, "")
		{
		}

		public AutoDeliveryBizO(OrgHeader organisation, string transportMode)
			: this(organisation, transportMode, "")
		{
		}

		public AutoDeliveryBizO(OrgHeader organisation, string transportMode, string containerMode)
			: this(organisation, transportMode, containerMode, "", "")
		{
		}

		public AutoDeliveryBizO(OrgHeader organisation, string transportMode, string containerMode, string localPort, string foreignPort, bool isImport)
			: this(organisation, transportMode, containerMode, localPort, foreignPort, isImport, null)
		{
		}

		public AutoDeliveryBizO(OrgHeader organisation, string transportMode, string containerMode, string localPort, string foreignPort)
			: this(organisation, transportMode, containerMode, localPort, foreignPort, null)
		{
		}

		public AutoDeliveryBizO(OrgHeader organisation, string transportMode, string containerMode, string localPort, string foreignPort, OrgHeader relatedParty)
			: this(organisation, transportMode, containerMode, localPort, foreignPort, false, relatedParty)
		{
		}

		public AutoDeliveryBizO(OrgHeader organisation, string transportMode, string containerMode, string localPort, string foreignPort, bool isImport, OrgHeader relatedParty)
			: this(organisation, transportMode, containerMode, localPort, foreignPort, isImport, relatedParty, null)
		{
		}

		public AutoDeliveryBizO(OrgHeader organisation, string transportMode, string containerMode, string localPort, string foreignPort, bool isImport, OrgHeader relatedParty, IDocAddress overriddenDeliveryDetails)
			: this(organisation, transportMode, containerMode, localPort, foreignPort, isImport, relatedParty, overriddenDeliveryDetails, "")
		{
		}

		public AutoDeliveryBizO(OrgHeader organisation, string transportMode, string containerMode, string localPort, string foreignPort, bool isImport, OrgHeader relatedParty, IDocAddress overriddenDeliveryDetails, string relatedBranch)
			: this(organisation, transportMode, containerMode, localPort, foreignPort, isImport, relatedParty, overriddenDeliveryDetails, relatedBranch, "")
		{
		}

		public AutoDeliveryBizO(OrgHeader organisation, string transportMode, string containerMode, string localPort, string foreignPort, bool isImport, OrgHeader relatedParty, IDocAddress overriddenDeliveryDetails, string relatedBranch, string relatedCompany)
			: this(organisation, transportMode, containerMode, localPort, foreignPort, isImport, relatedParty, overriddenDeliveryDetails, relatedBranch, relatedCompany, "")
		{
		}

		public AutoDeliveryBizO(OrgHeader organisation, string transportMode, string containerMode, string localPort, string foreignPort, bool isImport, OrgHeader relatedParty, IDocAddress overriddenDeliveryDetails, string relatedBranch, string relatedCompany, string relatedDepartment)
			: base(organisation)
		{
			fOrganisation = organisation;
			fTransportMode = transportMode;
			fContainerMode = containerMode;
			fLocalPort = localPort;
			fForeignPort = foreignPort;
			fRelatedParty = relatedParty;
			fIsImport = isImport;
			fOverriddenDeliveryDetails = overriddenDeliveryDetails;
			fRelatedBranch = relatedBranch;
			fRelatedCompany = relatedCompany;
			fRelatedDepartment = relatedDepartment;
		}

		protected readonly OrgHeader fOrganisation;
		protected readonly string fTransportMode;
		protected readonly string fContainerMode;
		protected readonly string fLocalPort;
		protected readonly string fForeignPort;
		protected readonly string fRelatedBranch;
		protected readonly string fRelatedCompany;
		protected readonly string fRelatedDepartment;
		protected readonly bool fIsImport;
		protected readonly OrgHeader fRelatedParty;
		protected readonly IDocAddress fOverriddenDeliveryDetails;

		public override BusinessContext BusinessContext
		{
			get { return new BusinessContext(); }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return null;
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return null;
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		public override string LocalPort(IContactType type, DocumentDirection direction)
		{
			return fLocalPort;
		}

		public override string ForeignPort(IContactType type, DocumentDirection direction)
		{
			return fForeignPort;
		}

		public override string TransportMode
		{
			get { return fTransportMode; }
		}

		public override string ContainerMode
		{
			get { return fContainerMode; }
		}

		public override string RelatedBranch => fRelatedBranch;

		public override string RelatedCompany => fRelatedCompany;

		public override string RelatedDepartment => fRelatedDepartment;

		public override bool IsImport
		{
			get { return fIsImport; }
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			return new OrgHeaderContact(fOrganisation, fRelatedParty, null);
		}

		public override IDocAddress GetOverriddenDeliveryDetails(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			return fOverriddenDeliveryDetails;
		}
	}
}
