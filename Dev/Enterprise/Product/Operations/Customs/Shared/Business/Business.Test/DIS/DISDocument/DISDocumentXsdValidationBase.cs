using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.MasterFiles.Business.Xsd.Testing;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class DISDocumentXsdValidationBase<T> : XsdSchemaValidationTest where T : XmlSerializableNonPersistentBusinessObject, IDISDocumentBase
	{
		protected override void ExtraAssertion(XmlSerializableNonPersistentBusinessObject bizObj)
		{
			var disDocument = (T)bizObj;
			base.ExtraAssertion(bizObj);
			AssertMultilineASCIIEquals("JobRequiredDocumentAddInfo.EX_AddInfo", GetExpectedXmlData(), disDocument.RequiredDocumentAddInfo.EX_AddInfo);
		}

		protected override int XsdVersion
		{
			get { return 1; }
		}

		protected override XmlSerializableNonPersistentBusinessObject GetBizObjInDiffFactory(XmlSerializableNonPersistentBusinessObject bizObj, BusinessObjectFactory newFactory)
		{
			var requiredDocument = newFactory.Load<JobRequiredDocument>(((T)bizObj).RequiredDocumentPK);
			var documentAddInfo = requiredDocument.AddInfos[0];
			var docAndCartage = newFactory.Load<Integration.Freight.IJobDocsAndCartage>(requiredDocument.EQ_ParentID);
			var declaration = newFactory.Load<BaseJobDeclaration>(docAndCartage.JP_ParentID);
			var hostWrapper = GetDISHostWrapper((IDISHost)declaration);
			var disDoc = GetDISDocument(hostWrapper);

			using (disDoc.SuspendSettingHasChanges())
			using (disDoc.GetValidationSuspender())
			{
				disDoc.RequiredDocumentPK = requiredDocument.PK;
				disDoc.RequiredDocumentAddInfo = documentAddInfo;
				disDoc.Status = documentAddInfo.EX_Status;
				disDoc.Deserialize(documentAddInfo.EX_AddInfo, XsdNamespace);
			}
			return disDoc;
		}

		protected abstract DISHostWrapperBase<T> GetDISHostWrapper(IDISHost disHost);
		protected abstract T GetDISDocument(DISHostWrapperBase<T> hostWrapper);
	}
}
