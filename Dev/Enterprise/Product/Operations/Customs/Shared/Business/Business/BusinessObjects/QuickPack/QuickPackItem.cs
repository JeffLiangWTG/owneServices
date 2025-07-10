using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class QuickPackItem : AutoQuickPackItem
	{
		public QuickPackItem(CusPackableItem packableItem) : base(packableItem.Factory)
		{
			PackableItem = Argument.NotNull(packableItem, nameof(packableItem));
			base.PackSeq = "0";
		}

		public readonly CusPackableItem PackableItem;

		public QuickPackItemLookups Lookups
		{
			get
			{
				if (lookups == null || !IsLookupsCachedInBase)
				{
					lookups = new QuickPackItemLookups(this);
				}
				return lookups;
			}
		}

		QuickPackItemLookups lookups;

		public override ZShort Sequence => PackableItem.CUI_Sequence;

		[ReadOnlyMember(nameof(Pack_ReadOnly))]
		public override ZString Pack
		{
			get => base.Pack;
			set => base.Pack = value;
		}
		ZBool Pack_ReadOnly => PackSeq != "0";

		[List(nameof(Lookups) + "." + nameof(QuickPackItemLookups.QuickPackSeqList))]
		public override ZString PackSeq
		{
			get => base.PackSeq;
			set
			{
				base.PackSeq = value;
				if (PackableItem?.PackingList?.PackageJob?.Packages?.QuickPackSeqDictionary is IDictionary<ZString, ZString> quickPackSeqDictionary
					&& quickPackSeqDictionary.TryGetValue(value, out var pack))
				{
					Pack = pack;
				}
				else
				{
					Pack = ZString.Empty;
				}
			}
		}

		public override ZString InvoiceNumber => PackableItem.CUI_InvoiceNumber;

		public override ZShort InvoiceLineNumber => PackableItem.CUI_InvoiceLineNumber;

		public override ZString GoodsDesc => PackableItem.CUI_GoodsDescription;

		public override ZDecimal NotPackedQty => PackableItem.NotPackedQty;

		public override ZString PackableUQ => PackableItem.CUI_PackableUQ;

		public void DoQuickPackAction(HashSet<ZString> newPackageMarksAndNumbers)
		{
			var packages = PackableItem.PackingList?.PackageJob?.Packages;
			if (packages != null)
			{
				var pack = Pack;
				var packSeq = PackSeq;
				var packedPackage = packages.Cast<CusPackage>().OrderByDescending(c => c.KP_Sequence).FirstOrDefault(c => c.KP_MarksAndNumbers == pack && (packSeq == c.KP_Sequence.ToString() || (packSeq == (ZString)"0" && newPackageMarksAndNumbers.Contains(pack))));
				if (packedPackage == null && !pack.IsEmpty)
				{
					var newPackage = packages.AddNew();
					newPackage.KP_MarksAndNumbers = pack;
					newPackage.CustomsPackItem(PackableItem, PackedQty);
					newPackageMarksAndNumbers.Add(pack);
				}
				else if (packedPackage != null)
				{
					packedPackage.CustomsPackItem(PackableItem, PackedQty);
				}
				packages.RefreshBinding();
			}
		}
	}
}
