using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.DataTransfer.Universal;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.US.ISF.DataTransfer.Universal
{
	public class ISFBondEventProcessor : BaseBondEventProcessor
	{
		public ISFBondEventProcessor(Event eventDataObject, BusinessObjectFactory factory)
			: base(eventDataObject, factory)
		{
		}

		public override BusinessObject[] Process()
		{
			CusISFHeader[] result = null;
			var query = GetFilterForCustomsReference();
			if (query != null && !query.IsEmpty)
			{
				result = factory.Load<CusISFHeader>(query);
			}

			var header = result?.FirstOrDefault();

			var uri = header != null ? ObjectFactory.Get<IShowEditFormUrlCreator>().Create(header) : string.Empty;
			var jobNumber = header != null ? header.BF_JobReference : ZString.Empty;
			var branch = header != null && header.Branch != null ? header.Branch : GlbBranch.CurrentBranch;
			SendNotification(uri, jobNumber, ZString.Empty, branch, System.Array.Empty<string>());

			if (header != null)
			{
				LinkMessage(header.BF_CustomsReference.Replace("-", ""), header);
			}
			return result;
		}

		ZQuery GetFilterForCustomsReference()
		{
			ZDBOnlyQuery filter = new ZDBOnlyQuery(typeof(CusISFHeader));
			var xmlEventValueObject = eventDataObject as IXmlEventValueObject;
			var transactionId = xmlEventValueObject?.Context.EntryNumber ?? ZString.Empty;
			if (transactionId.Length > 3)
			{
				filter.AddToFilter(CusISFHeaderSchema.BF_CustomsReference, ZString.Format("{0}-{1}", transactionId.SubstringSafe(0, 3), transactionId.SubstringSafe(3)));
			}
			return filter;
		}
	}
}
