using System.Collections.Generic;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Tracking.Web.Testing
{
	class DocumentRequestHandlerForTest<T> : DocumentRequestHandler<T>
			where T : DocumentRequestHelper, new()
	{
		protected override byte[] GetDocument(DocumentPack pack, OrgContact contact)
		{
			fBranch = Env.CurrentBranch;
			return base.GetDocument(pack, contact);
		}

		public byte[] GetDocumentForTest(DocumentPack pack, OrgContact contact)
		{
			return base.GetDocument(pack, contact);
		}

		protected override OrgHeader CurrentLoggedInOrg
		{
			get { return Organisation; }
		}

		protected override OrgContact CurrentLoggedInUser
		{
			get { return Contact; }
		}

		public IBranch Branch
		{
			get { return fBranch; }
		}
		IBranch fBranch;

		public OrgHeader Organisation
		{
			get { return fOrganisation; }
			set { fOrganisation = value; }
		}
		OrgHeader fOrganisation;

		public OrgContact Contact
		{
			get { return fContact; }
			set { fContact = value; }
		}
		OrgContact fContact;

		public List<string> SupportedContentTypesForTest
		{
			get
			{
				return base.SupportedContentTypes;
			}
		}
	}
}
