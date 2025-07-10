using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;

namespace Enterprise.MasterFiles.Business.Testing
{
	class MockJobDocAddressParentForLookupsTest : OrgHeader, IDocAddresses
	{
		public MockJobDocAddressParentForLookupsTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			InitialiseDocAddressRequirements();
		}

		public new JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					fDocAddresses = new JobDocAddressDependentCollection(this);
					fDocAddresses.Load();
				}
				return fDocAddresses;
			}
		}

		JobDocAddressDependentCollection fDocAddresses;

		public JobDocAddressManager DocAddressManager
		{
			get
			{
				if (fDocAddressManager == null)
				{
					fDocAddressManager = new JobDocAddressManager(DocAddresses);
				}

				return fDocAddressManager;
			}
		}

		JobDocAddressManager fDocAddressManager;

		void InitialiseDocAddressRequirements()
		{
			fConsigneeDocAddressRequirement = AddConsigneeDocAddressRequirement();
			fConsignorDocAddressRequirement = AddConsignorDocAddressRequirement();
		}

		#region ConsigneeDocAddressRequirement

		public JobDocAddressRequirement ConsigneeDocAddressRequirement
		{
			get { return fConsigneeDocAddressRequirement; }
		}

		protected virtual JobDocAddressRequirement AddConsigneeDocAddressRequirement()
		{
			JobDocAddressRequirement requirement = new JobDocAddressRequirement(DocAddressType.ConsigneeDocumentaryAddress);
			requirement.AddLinkedRequirement(new JobDocAddressRequirement(DocAddressType.ConsigneePickupDeliveryAddress));
			DocAddressManager.AddRequirement(requirement);
			return requirement;
		}

		JobDocAddressRequirement fConsigneeDocAddressRequirement;

		#endregion

		#region ConsignorDocAddressRequirement

		public JobDocAddressRequirement ConsignorDocAddressRequirement
		{
			get { return fConsignorDocAddressRequirement; }
		}

		protected virtual JobDocAddressRequirement AddConsignorDocAddressRequirement()
		{
			JobDocAddressRequirement requirement = new JobDocAddressRequirement(DocAddressType.ConsignorDocumentaryAddress);
			requirement.AddLinkedRequirement(new JobDocAddressRequirement(DocAddressType.ConsignorPickupDeliveryAddress));
			DocAddressManager.AddRequirement(requirement);
			return requirement;
		}

		JobDocAddressRequirement fConsignorDocAddressRequirement;

		#endregion

		#region IDocAddresses Members

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return null;
		}

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get { return new DocAddressType[] { DocAddressType.ConsigneeAddress }; }
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return null;
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return false;
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			OrgHeaderCollection result = null;
			if (addressType == DocAddressType.ConsigneeAddress)
			{
				result = new ConsigneeCollection(Factory);
			}
			return result;
		}

		#endregion
	}
}
