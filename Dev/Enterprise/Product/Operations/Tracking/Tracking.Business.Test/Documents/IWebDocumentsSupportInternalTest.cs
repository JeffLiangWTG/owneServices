using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(DummyBusinessObjectWithDocumentSupport))]
	sealed class IWebDocumentsSupportInternalTest : IWebDocumentsSupportBaseTest
	{
		#region Setup

		new DummyBusinessObjectWithDocumentSupport BizObj
		{
			get { return base.BizObj as DummyBusinessObjectWithDocumentSupport; }
		}

		protected override IReadOnlyCollection<ZGuid> ExpectedDocRelatedPKs
		{
			get { return BizObj.DocRelatedPKs.ToArray(); }
		}

		protected override IWebDocumentsSupport GetNewBusinessObject()
		{
			return Factory.New<DummyBusinessObjectWithDocumentSupport>();
		}

		#endregion

		#region DummyBusinessObjectWithDocumentSupport

		public class DummyBusinessObjectWithDocumentSupport : DummyEnterpriseBusinessObject, IDocManagerSupport, IWebDocumentsSupport, IHaveRequiredDocuments
		{
			public DummyBusinessObjectWithDocumentSupport(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
				RelatedPKs = new List<ZGuid>();
			}

			#region IDocManagerSupport Members

			public DocManagerInfo DocManagerInfo
			{
				get { return new DocManagerInfo(this, Core.Constants.DocManagerCodes.Shipment); }
			}

			#endregion

			#region IWebDocumentsSupport Members

			public ZGuid DocParentPK
			{
				get { return PK; }
			}

			public List<ZGuid> DocRelatedPKs
			{
				get { return RelatedPKs; }
			}

			readonly List<ZGuid> RelatedPKs;

			public void AddToDocRelatedPKs(ZGuid relatedPK)
			{
				RelatedPKs.Add(relatedPK);
			}

			public OrgContact LoggedInContact
			{
				get { return fLoggedInContact; }
				set { fLoggedInContact = value; }
			}
			OrgContact fLoggedInContact;

			public DocumentSupport DocumentHelper
			{
				get
				{
					if (fDocumentHelper == null)
					{
						fDocumentHelper = new DocumentSupport(this);
					}

					return fDocumentHelper;
				}
			}
			DocumentSupport fDocumentHelper;

			#endregion

			#region IHaveRequiredDocuments Members

			public IReadOnlyList<ZString> AdditionalRefTypes
			{
				get { return System.Array.Empty<ZString>(); }
			}

			public OrgHeader ExportBroker
			{
				get { return null; }
			}

			public ZString HouseBill
			{
				get { return ZString.Empty; }
			}

			public ZString MasterBill
			{
				get { return ZString.Empty; }
			}

			public JobRequiredDocumentDependentCollection RequiredDocuments
			{
				get { return fRequiredDocuments ?? (fRequiredDocuments = new JobRequiredDocumentDependentCollection(this, Factory)); }
			}
			JobRequiredDocumentDependentCollection fRequiredDocuments;

			public ZString TableCode
			{
				get { return "JS"; }
			}

			public BusinessObject UltimateDocumentParent
			{
				get { return this; }
			}

			public ZString UniqueConsignRef
			{
				get { return ZString.Empty; }
			}

			public void PreLogAllDocumentsReceivedEvents()
			{
			}

			#endregion

		}

		#endregion
	}
}
