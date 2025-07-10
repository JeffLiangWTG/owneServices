using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class JobDocAddressParentForTesting : NonPersistentBusinessObject, IDocAddresses, IObsoleteValidation
	{
		public JobDocAddressParentForTesting(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public virtual JobDocAddressDependentCollection DocAddresses
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
		CodeDescriptionPairList fSelectableDocAddressTypes;

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return null;
		}

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get { return fSupportedAddressTypes; }
		}

		public void SetSupportedAddressTypes(DocAddressType[] types)
		{
			fSupportedAddressTypes = types;
		}
		DocAddressType[] fSupportedAddressTypes = Array.Empty<DocAddressType>();

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

		ZString IDocAddresses.HumanReadableName
		{
			get { throw new NotImplementedException(); }
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		public ZString SomethingToExpose
		{
			get { return "SomethingToExpose"; }
		}

		public ZPropertyInfo SomethingToExposeInfo
		{
			get { return GetZPropertyInfo(nameof(SomethingToExpose)); }
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return CanDeleteAddressForTesting != null && CanDeleteAddressForTesting(docAddress);
		}
		public CanDeleteAddressDelegateForTesting CanDeleteAddressForTesting;

		public delegate bool CanDeleteAddressDelegateForTesting(JobDocAddress docAddress);

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		public override string TablePrefix
		{
			get { return "Z0"; }
		}
	}
}
