using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class DummyRequiredDocumentsParent : DummyEnterpriseBusinessObject, IHaveRequiredDocuments, IDocManagerSupport
	{
		public DummyRequiredDocumentsParent(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		JobRequiredDocumentDependentCollection IHaveRequiredDocuments.RequiredDocuments
		{
			get { return RequiredDocuments; }
		}

		public JobRequiredDocumentDependentCollection RequiredDocuments
		{
			get
			{
				if (requiredDocuments == null)
				{
					requiredDocuments = new JobRequiredDocumentDependentCollection(this, Factory);
					RegisterEditableChildObject(requiredDocuments);
				}
				return requiredDocuments;
			}
		}
		JobRequiredDocumentDependentCollection requiredDocuments;

		ZString IHaveRequiredDocuments.TableCode
		{
			get { return DummyBizoSchema.Constants.Prefix; }
		}

		BusinessObject IHaveRequiredDocuments.UltimateDocumentParent
		{
			get { return this; }
		}

		OrgHeader IHaveRequiredDocuments.ExportBroker
		{
			get { return null; }
		}

		ZString IHaveRequiredDocuments.UniqueConsignRef
		{
			get { return "UniqueConsignRef"; }
		}

		ZString IHaveRequiredDocuments.HouseBill
		{
			get { return "HouseBill"; }
		}

		ZString IHaveRequiredDocuments.MasterBill
		{
			get { return "MasterBill"; }
		}

		IReadOnlyList<ZString> IHaveRequiredDocuments.AdditionalRefTypes
		{
			get { return System.Array.Empty<ZString>(); }
		}

		public void PreLogAllDocumentsReceivedEvents()
		{
		}

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, DocManagerCode);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;
		public string DocManagerCode = "ORG";

		#endregion
	}
}
