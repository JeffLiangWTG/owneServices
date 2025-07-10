using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;

namespace Enterprise.MasterFiles.Business.Testing
{
	public sealed class DocAddressCreatorHelper
	{
		public DocAddressCreatorHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
		}
		readonly BusinessObjectFactory Factory;

		#region CreateDocAddressParent

		public IDocAddresses CreateDocAddressParent(DocAddressType[] supportedAddressTypes)
		{
			var result = Factory.New<DummyDocAddressParent>();
			result.SetSupportedDocAddressTypes(supportedAddressTypes);
			return result;
		}

		#endregion

		#region CreateDocAddressCreatorHost

		public DocAddressCreatorHost CreateDocAddressCreatorHost()
		{
			var supportedAddressTypes = new DocAddressType[] { DocAddressType.LocalCartageCFS, DocAddressType.LocalCartageCTO };
			var orgCodes = new Dictionary<DocAddressType, ZString>() { { DocAddressType.LocalCartageCFS, "CFS" }, { DocAddressType.LocalCartageCTO, "CTO" } };
			return CreateDocAddressCreatorHost(supportedAddressTypes, orgCodes, DocAddressType.LocalCartageCTO);
		}

		public DocAddressCreatorHost CreateDocAddressCreatorHost(DocAddressType[] supportedDocAddressTypes, Dictionary<DocAddressType, ZString> orgCodes, DocAddressType defaultType)
		{
			var iDocAddresses = CreateDocAddressParent(supportedDocAddressTypes);

			DocAddressCreatorHost.DocAddressTypeCodeFormatter addressCodeConverter = delegate(DocAddressType docAddressType)
			{
				ZString code = "";
				orgCodes.TryGetValue(docAddressType, out code);
				return code;
			};

			return CreateDocAddressCreatorHost(iDocAddresses, addressCodeConverter, DocAddressType.LocalCartageCTO);
		}

		public DocAddressCreatorHost CreateDocAddressCreatorHost(IDocAddresses parent, DocAddressCreatorHost.DocAddressTypeCodeFormatter addressCoreConverter, DocAddressType defaultDocAddressType)
		{
			return new DocAddressCreatorHost(parent, addressCoreConverter, defaultDocAddressType, Factory);
		}

		public DocAddressCreatorHost CreateDocAddressCreatorHostEditor()
		{
			var supportedAddressTypes = new DocAddressType[] { DocAddressType.LocalCartageCFS, DocAddressType.LocalCartageCTO };
			var iDocAddresses = CreateDocAddressParent(supportedAddressTypes);
			var docAddress = iDocAddresses.DocAddresses.AddNew(DocAddressType.LocalCartageCFS);
			return new DocAddressCreatorHost(docAddress, Factory);
		}

		#endregion

		#region DummyDocAddressParent

		class DummyDocAddressParent : DummyBusinessObject, IDocAddresses
		{
			public DummyDocAddressParent(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region DocAddresses

			public JobDocAddressDependentCollection DocAddresses
			{
				get { return docAddresses ?? (docAddresses = new JobDocAddressDependentCollection(this)); }
			}
			JobDocAddressDependentCollection docAddresses;

			public JobDocAddressNumberCollection DocAddressNumbers => throw new NotImplementedException();

			#endregion

			#region SupportedAddressTypes

			IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
			{
				get { return supportedDocAddressTypes; }
			}
			DocAddressType[] supportedDocAddressTypes = Array.Empty<DocAddressType>();

			public void SetSupportedDocAddressTypes(DocAddressType[] supportedDocAddressTypes)
			{
				this.supportedDocAddressTypes = supportedDocAddressTypes;
			}

			#endregion

			#region IDocAddresses

			ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate) { return null; }
			SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress) { return null; }
			JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType) { return new JobDocAddressRequirement(); }
			void IDocAddresses.DocAddressChanged(JobDocAddress docAddress) { }
			void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress) { }
			void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress) { }
			void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress) { }
			void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress) { }
			bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress) { return false; }
			OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType) { return null; }

			#endregion
		}

		#endregion
	}
}
