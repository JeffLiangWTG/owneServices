using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	public class InBondDataObjectWriterHelper : UniversalCommonHelper
	{
		public InBondDataObjectWriterHelper(CusInBondHeader header)
			: base(header.Factory)
		{
			this.header = header;
		}
		readonly CusInBondHeader header;

		protected CusInBondHeader Header
		{
			get { return header; }
		}

		#region List

		public ContainerModeList ContainerModeList
		{
			get { return factory.GetCachedValue<ContainerModeList>(); }
		}

		public Business.TransportTypeList TransportTypeList
		{
			get { return factory.GetCachedValue<Business.TransportTypeList>(); }
		}

		#endregion

		#region Bill Link

		public void AllocateBillLink(CusInBondBill billBO, AdditionalBill billData)
		{
			var link = 0;
			if (!billToLinkMap.TryGetValue(billBO.PK, out link))
			{
				link = lastBillLink++;
				billData.Link = link;
				billToLinkMap[billBO.PK] = link;
			}
			else
			{
				billData.Link = link;
			}
		}
		readonly Dictionary<ZGuid, int> billToLinkMap = new Dictionary<ZGuid, int>();
		int lastBillLink = 1;

		public void SetBillLink(CusInBondBill billBO, IAdditionalBillLinkParent additionalBillLinkParent)
		{
			if (billToLinkMap.ContainsKey(billBO.PK))
			{
				additionalBillLinkParent.AdditionalBillLink = billToLinkMap[billBO.PK];
			}
		}

		#endregion

		#region Container Link

		public void AllocateContainerLink(CusInBondContainer containerBO, Container containerData)
		{
			var link = 0;
			if (!containerToLinkMap.TryGetValue(containerBO.PK, out link))
			{
				link = lastContainerLink++;
				containerData.Link = link;
				containerToLinkMap[containerBO.PK] = link;
			}
			else
			{
				containerData.Link = link;
			}
		}
		readonly Dictionary<ZGuid, int> containerToLinkMap = new Dictionary<ZGuid, int>();
		int lastContainerLink = 1;

		#endregion

		#region Dispositions

		public void PopulateDispositions(BusinessObject parentBO, IAddInfoGroupCollectionParent parentData)
		{
			parentData.SetAddInfoGroupCollection(() =>
			{
				var addInfoGroupCollection = parentData.AddInfoGroupCollection ?? new List<AddInfoGroup>();
				var query = new ZQuery(CusAddInfoSchema.B7_ParentID, parentBO.PK);
				query.AddToFilter(CusAddInfoSchema.B7_Type, CusAddInfoTypeAttribute.Codes.USDisposition);
				query.FetchOnlyFromLocalCache = !parentBO.IsInDatabase;
				var dispositionBOs = Load<DispositionData>(query).OrderBy(x => x.US_Order).ToArray();
				foreach (DispositionData dispositionBO in dispositionBOs)
				{
					addInfoGroupCollection.Add(new AddInfoGroup()
					{
						Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.USDisposition, Description = "Disposition" },
						AddInfoCollection = AddInfoCollectionCreator.CreateCollection(dispositionBO.B7_AddInfoData)
					});
				}

				return addInfoGroupCollection;
			});
		}

		#endregion
	}
}
