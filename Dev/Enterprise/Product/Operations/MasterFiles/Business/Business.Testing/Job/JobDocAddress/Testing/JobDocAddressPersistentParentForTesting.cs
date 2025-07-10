using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class JobDocAddressPersistentParentForTesting : RefCountry, IDocAddresses
	{
		public JobDocAddressPersistentParentForTesting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (!IsInDatabase && RN_Code.IsEmpty)
			{
				// RN_Code has a LEN() = 2 constraint
				RN_Code = "XX";
			}
		}

		public virtual JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					fDocAddresses = new JobDocAddressDependentCollection(this);
					fDocAddresses.IsManagedForDataRefresh = true;
					fDocAddresses.Load();
				}
				return fDocAddresses;
			}
		}

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

		public CodeDescriptionPairList SelectableDocAddressTypes
		{
			get
			{
				if (fSelectableDocAddressTypes == null)
				{
					fSelectableDocAddressTypes = new CodeDescriptionPairList();
				}
				return fSelectableDocAddressTypes;
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get { return "FriendlyName"; }
		}

		JobDocAddressDependentCollection fDocAddresses;
		JobDocAddressManager fDocAddressManager;
		CodeDescriptionPairList fSelectableDocAddressTypes;

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return null;
		}

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get { return System.Array.Empty<DocAddressType>(); }
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return DocAddressRequirementForTesting != null && DocAddressRequirementForTesting.DefaultDocAddressType == addressType ? DocAddressRequirementForTesting : null;
		}

		public JobDocAddressRequirement DocAddressRequirementForTesting;

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

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return false;
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}
	}
}
