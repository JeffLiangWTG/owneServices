using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.ContainerYard.Business
{
	[UniversalDataContext(DataContextType.GateTransport)]
	[CodeProperty(Schema.GTT_VehicleRegistration), DescriptionProperty(Schema.GTT_DriverName)]
	public class GateTransport : AutoGateTransport, IDocAddresses, IEDocsProvider, IJobNumber
	{
		public GateTransport(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region GateTransportCFSDetails

		[ChildEditable]
		public GateTransportCFSDetailCollection GateTransportCFSDetails
		{
			get
			{
				if (gateTransportCFSDetails == null)
				{
					gateTransportCFSDetails = new GateTransportCFSDetailCollection(this);
					RegisterEditableChildObject(gateTransportCFSDetails);
				}

				return gateTransportCFSDetails;
			}
		}

		GateTransportCFSDetailCollection gateTransportCFSDetails;

		#endregion

		#region GateTransportCYDetails

		[ChildEditable]
		public GateTransportCYDetailCollection GateTransportCYDetails
		{
			get
			{
				if (gateTransportCYDetails == null)
				{
					gateTransportCYDetails = new GateTransportCYDetailCollection(this);
					RegisterEditableChildObject(gateTransportCYDetails);
				}

				return gateTransportCYDetails;
			}
		}

		GateTransportCYDetailCollection gateTransportCYDetails;

		#endregion

		#region Transport Company

		public OrgHeader TransportCompany
		{
			get { return (TransportCompanyDocumentaryAddress == null || !TransportCompanyDocumentaryAddress.HasRealOrganisation) ? null : TransportCompanyDocumentaryAddress.Organisation; }
		}

		public JobDocAddress TransportCompanyDocumentaryAddress
		{
			get { return DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.TransportCompanyDocumentaryAddress); }
		}

		#endregion

		#region IDocAddresses Members

		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (docAddresses == null)
				{
					docAddresses = new JobDocAddressDependentCollection(this);
					docAddresses.Load();
				}

				return docAddresses;
			}
		}
		JobDocAddressDependentCollection docAddresses;

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get { return new[] { DocAddressType.TransportCompanyDocumentaryAddress }; }
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress) => Env.Security.None;

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress) => false;

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType) => null;

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType) => null;

		ZString IDocAddresses.HumanReadableName => ZString.Empty;

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate) => null;

		#endregion

		#region IEDocsProvider

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				return docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.GateTransport));
			}
		}
		DocManagerInfo docManagerInfo;

		public EDocsProviderSupporter GetEDocsProviderSupporter()
		{
			return eDocsProviderSupporter ?? (eDocsProviderSupporter = new EDocsProviderSupporter(this));
		}
		EDocsProviderSupporter eDocsProviderSupporter;

		public DocumentSupporter DocumentSupporter => null;

		#endregion

		#region IJobNumber

		public string JobNumber => GTT_JobNumber;

		#endregion

		public override void Delete()
		{
			GateTransportCYDetails.DeleteAll();
			GateTransportCFSDetails.DeleteAll();

			base.Delete();
		}
	}
}
