using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class DummyWithDocAddress : DummyEnterpriseBusinessObject, IDocAddresses
	{
		public DummyWithDocAddress(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region IDocAddresses Members

		JobDocAddressDependentCollection addresses;
		JobDocAddressDependentCollection IDocAddresses.DocAddresses
		{
			get
			{
				if (addresses == null)
				{
					addresses = new JobDocAddressDependentCollection(this);
				}
				return addresses;
			}
		}

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

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			SecurityForTest security = new SecurityForTest(null, Env.CurrentUser.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
			SecurityCheckpoint result = new SecurityCheckpoint("Code", (NoResString)"Display Text", null, security);

			result.IsAllowed = AllowAddressOverride;

			return result;
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return false;
		}

		public bool AllowAddressOverride;

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		#endregion
	}
}
