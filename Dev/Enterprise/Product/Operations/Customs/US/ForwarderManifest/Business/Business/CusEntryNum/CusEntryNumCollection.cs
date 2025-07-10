using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class CusEntryNumCollection : DependentBusinessObjectCollection<CusEntryNumber, USExportAsycudaBill>
	{
		public CusEntryNumCollection(USExportAsycudaBill bill, ZString entryType, ZString humanReadableName)
			: base(bill, additionalFilter: GetAdditionalFilter(bill.PK, entryType))
		{
			this.entryType = entryType.Trim();
			this.humanReadableName = humanReadableName;
		}
		readonly ZString entryType;
		readonly ZString humanReadableName;

		static ZQuery GetAdditionalFilter(ZGuid pk,ZString entryType)
		{
			var query = new ZQuery(CusEntryNumSchema.CE_ParentID, pk);
			query.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryType, entryType);
			return query;
		}

		public void RefreshCollection(ZString newValue)
		{
			this.DeleteAll();
			var newNumbers = newValue.Split(',').Select(x => x.Trim()).Where(x => !x.IsEmpty).ToArray();
			foreach (var number in newNumbers)
			{
				var multipleCode = AddNew();
				multipleCode.CE_EntryNum = number.Trim();
			}
		}

		public ZString GetCodesAsCommaSeparatedString()
		{
			var zStringBuilder = new ZStringBuilder();
			foreach (var item in GetCodesAsCollection())
			{
				zStringBuilder.Append(item);
			}

			return zStringBuilder.ToStringWithDelimiterBetweenAppends(",");
		}

		IEnumerable<ZString> GetCodesAsCollection()
		{
			return from CusEntryNumber x in this
				   select x.CE_EntryNum.Trim() into x
				   where !x.IsEmpty
				   select x;
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => CusEntryNumSchema.CE_ParentID;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			if (child is CusEntryNumber entryNumber)
			{
				entryNumber.Parent = Master;
				entryNumber.CE_EntryType = entryType;
				entryNumber.CE_ParentTable = AsycudaBillSchema.Constants.TableName;
			}
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			if (bizOAdded is CusEntryNumber entryNumber)
			{
				entryNumber.CE_EntryNumInfo.HumanReadableName = humanReadableName;
			}
		}
	}
}
