using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.DataMapping;
// using Enterprise.MasterFiles.Module.Testing.Organisation.OrgImport.Testing;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class SimpleModuleDataTransferProcessorForTest : SimpleModuleDataTransferProcessor<Organisation.OrgImport.Testing.DummyHeader, Organisation.OrgImport.Testing.DummyFlattened>
	{
		public SimpleModuleDataTransferProcessorForTest(IBusinessObjectCollection headerCollection, IImportCollectionInfo flattenedImportCollectionInfo)
			: base(headerCollection, flattenedImportCollectionInfo)
		{
		}

		public IEnumerable<PropertyInfo> FlattenedProperties_Exposed
		{
			get { return FlattenedProperties; }
		}

		public Organisation.OrgImport.Testing.DummyHeader GetHeader_Exposed(ZString uniqueReference, Func<Organisation.OrgImport.Testing.DummyHeader, string> uniqueReferenceFunc)
		{
			return GetHeader(uniqueReference, uniqueReferenceFunc);
		}

		protected override void SetupNewHeader(Organisation.OrgImport.Testing.DummyHeader header)
		{
			header.HasBeenSetup = true;
		}

		public void CopyIdenticallyNamedProperties_Exposed(BusinessObject record, BusinessObject flattenedRecord, string prefix, string prefixToRemove = "")
		{
			CopyIdenticallyNamedProperties(record, flattenedRecord, prefix, prefixToRemove);
		}

		public void SetOrgReference_Exposed(ZString orgCode, Action<OrgHeader> setAction)
		{
			SetOrgReference(orgCode, setAction);
		}

		public void SetJobDocAddress_Exposed(string prefix, BusinessObject flattenedRecord, JobDocAddress docAddress)
		{
			SetJobDocAddress(prefix, flattenedRecord, docAddress);
		}

		public OrgAddress GetAddress_Exposed(BusinessObject flattenedRecord, string prefix, bool fallbackToMainAddress = false)
		{
			return GetAddress(flattenedRecord, prefix, fallbackToMainAddress);
		}

		protected override Organisation.OrgImport.Testing.DummyHeader CreateHeader(IBusinessObjectCollection headerCollection, Organisation.OrgImport.Testing.DummyFlattened flattenedRecord)
		{
			return (Organisation.OrgImport.Testing.DummyHeader)headerCollection.AddNew();
		}
	}
}
