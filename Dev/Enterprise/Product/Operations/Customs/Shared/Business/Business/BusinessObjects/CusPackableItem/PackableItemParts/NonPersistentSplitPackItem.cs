using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public class NonPersistentSplitPackItem : AutoNonPersistentSplitPackItem, IShortSequenceNumberLine
	{
		public NonPersistentSplitPackItem(CusPackageCusPackableItemRelation packageCusPackableItemRelation, PackableItemsSplitter splitter)
			: base(packageCusPackableItemRelation.Factory)
		{
			this.packageCusPackableItemRelation = packageCusPackableItemRelation;
			this.splitter = splitter;
		}

		readonly CusPackageCusPackableItemRelation packageCusPackableItemRelation;
		readonly PackableItemsSplitter splitter;

		[List(nameof(PackTypes))]
		public override ZString PackableUQ { get => base.PackableUQ; set => base.PackableUQ = value; }

		[List(nameof(WeightUQs))]
		public override ZString NetWeightUQ { get => base.NetWeightUQ; set => base.NetWeightUQ = value; }

		public CodeDescriptionPairList PackTypes => packageCusPackableItemRelation.Lookups.PackTypes;

		public CodeDescriptionPairList WeightUQs => packageCusPackableItemRelation.Lookups.WeightUQs;

		public override ZShort Sequence
		{
			get => base.Sequence;
			set
			{
				if (value > 0)
				{
					ZShort oldValue = Sequence;
					base.Sequence = value;
					if (!IsCopying)
					{
						splitter.PackableItemPartSequenceNumberGenerator.RecalculateWhenRenumbered(this, oldValue);
					}
				}
			}
		}

		public override void Delete()
		{
			splitter.PackableItemPartSequenceNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
			base.Delete();
		}

		#region IShortSequenceNumberLine
		ZShort ISequenceNumberLine<ZShort>.SequenceNumber { get => Sequence; set => Sequence = value; }

		ZGuid ISequenceNumberLine.FKToHeader => ZGuid.Empty;
		#endregion
	}
}
