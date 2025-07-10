using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.Business
{
	public abstract class DISDocumentCollectionBase<T> : NonPersistentBusinessObjectCollection<T> where T : XmlSerializableNonPersistentBusinessObject, IDISDocumentBase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		protected DISDocumentCollectionBase(DISHostWrapperBase<T> wrapper) : base(wrapper.Factory)
		{
			this.wrapper = wrapper;
			this.disHost = wrapper.DISHost;
			this.initialDocumentAddInfos = new List<JobRequiredDocumentAddInfo>();
			PopulateCollection();
		}

		protected readonly DISHostWrapperBase<T> wrapper;
		protected readonly IDISHost disHost;
		readonly List<JobRequiredDocumentAddInfo> initialDocumentAddInfos;

		void PopulateCollection()
		{
			foreach (JobRequiredDocument requiredDocument in disHost.RequiredDocumentsProvider.RequiredDocuments)
			{
				foreach (JobRequiredDocumentAddInfo documentAddInfo in requiredDocument.AddInfos)
				{
					if (documentAddInfo.EX_GC_Company == disHost.CompanyPK && documentAddInfo.EX_ApplicationCode == ApplciationCode)
					{
						if (!documentAddInfo.EX_AddInfo.IsEmpty)
						{
							var disDoc = CreateElement();

							using (disDoc.SuspendSettingHasChanges())
							using (disDoc.GetValidationSuspender())
							{
								disDoc.RequiredDocumentPK = requiredDocument.PK;
								disDoc.RequiredDocumentAddInfo = documentAddInfo;
								disDoc.Status = documentAddInfo.EX_Status;
								disDoc.Deserialize(documentAddInfo.EX_AddInfo, XmlNamespace);
								Add(disDoc as BusinessObject);
							}
						}

						initialDocumentAddInfos.Add(documentAddInfo);
					}
				}
			}
		}

		public void ClearHasChanges()
		{
			foreach (IDISDocumentBase disDocument in this)
			{
				if (disDocument.RequiredDocument != null)
				{
					var requiredDocumentAddInfo = disDocument.RequiredDocumentAddInfo;
					if (requiredDocumentAddInfo?.HasChanges ?? false)
					{
						requiredDocumentAddInfo.ClearHasChanges();
					}
				}
			}
		}

		public void Serialize()
		{
			foreach (IDISDocumentBase disDocument in this)
			{
				if (disDocument.RequiredDocument != null)
				{
					var requiredDocumentAddInfo = disDocument.RequiredDocumentAddInfo;

					if (requiredDocumentAddInfo == null)
					{
						requiredDocumentAddInfo = disDocument.RequiredDocument.AddInfos.AddNew();
						requiredDocumentAddInfo.EX_GC_Company = disHost.CompanyPK;
						requiredDocumentAddInfo.EX_ApplicationCode = ApplciationCode;

						disDocument.RequiredDocumentAddInfo = requiredDocumentAddInfo;
					}

					requiredDocumentAddInfo.EX_AddInfo = disDocument.Serialize(XmlNamespace);
					requiredDocumentAddInfo.EX_Status = disDocument.Status;

					initialDocumentAddInfos.Remove(requiredDocumentAddInfo);
				}
			}

			initialDocumentAddInfos.Where(x => !x.IsDeleted).ToList().ForEach(x => x.Delete());
			initialDocumentAddInfos.Clear();

			foreach (IDISDocumentBase disDocument in this)
			{
				var requiredDocumentAddInfo = disDocument.RequiredDocumentAddInfo;

				if (requiredDocumentAddInfo != null)
				{
					initialDocumentAddInfos.Add(requiredDocumentAddInfo);
				}
			}
		}

		protected abstract string ApplciationCode { get; }
		protected abstract string XmlNamespace { get; }
		protected abstract IDISDocumentBase CreateElement();
	}
}
