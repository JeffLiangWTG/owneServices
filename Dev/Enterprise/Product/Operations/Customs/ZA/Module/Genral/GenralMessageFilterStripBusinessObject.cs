using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.ZA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ZA.Module
{
	public class GenralMessageFilterStripBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			var pimaFilter = result.AddTextFilter("PIMA", EDIMessageSchema.EM_ApplicationReference);
			pimaFilter.MultilingualDescription = ResString.GetMultilingualString("F4624D22-6F68-41CB-9096-111BE9FDDBDD", "PIMA");

			var localProfileFilter = result.AddTextFilter("Local Profile", EDIMessageSchema.EM_MessageOwner);
			localProfileFilter.MultilingualDescription = ResString.GetMultilingualString("5506E951-C98B-456A-8CE2-FD4A1C9E16B9", "Local Profile");

			var statusFilter = result.AddTextFilter("Status", EDIMessageSchema.EM_Status, () => new EDIMessageStatusList());
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("69528F03-AA1F-4C5B-A699-5B69EA7FE46D", "Status");

			var directionFilter = result.AddTextFilter("Direction", EDIMessageSchema.EM_ReceiveTransmit, new ReceiveTransmitList());
			directionFilter.MultilingualDescription = ResString.GetMultilingualString("390BBC63-60D7-4C56-9D96-15E8560A4A5C", "Direction");

			var dateCreatedFilter = result.AddDateFilter("Date created", EDIMessageSchema.EM_SystemCreateTimeUtc, convertFromLocalToUTC: true);
			dateCreatedFilter.MultilingualDescription = ResString.GetMultilingualString("1DA06881-F3A3-4C82-84ED-DB1DC00C107F", "Date created");

			var purposeFilter = result.AddTextFilter("Purpose", EDIMessageSchema.EM_MessageSubType, () => new GenralMessageSubTypeList());
			purposeFilter.MultilingualDescription = ResString.GetMultilingualString("A356FD98-174A-441F-9389-E25C92FA8750", "Purpose");

			return result;
		}

		public override ZQuery Filter
		{
			get
			{
				var f = base.Filter;
				var branchPks = (from GlbBranch b in GlbCompany.CurrentCompany.Branches select b.PK).ToArray();
				f.AddToFilter(EDIMessageSchema.EM_GB, branchPks);
				f.AddToFilter(EDIMessageSchema.EM_MessageType, CustomsResponseMessageTypeList.Codes.GEN);
				return f;
			}
		}
	}
}
