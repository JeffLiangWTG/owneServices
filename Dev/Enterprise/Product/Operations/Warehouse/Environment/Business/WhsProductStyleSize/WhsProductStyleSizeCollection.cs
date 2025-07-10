using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsProductStyleSizeCollection : ActiveBusinessObjectCollection<WhsProductStyleSize>
	{
		public WhsProductStyleSizeCollection(WhsProductStyle productStyle)
			: base(productStyle.Factory, productStyle, new ZQuery(), WhsProductStyleSizeSchema.WSZ_WST_ProductStyle)
		{
			Sequencer.SortBySequence(); // this does not sort until the elements are accessed
		}

		#region SetDefaultsForNewElement

		protected override void SetDefaultsForNewElementCore(WhsProductStyleSize size)
		{
			base.SetDefaultsForNewElementCore(size);
			size.WSZ_Sequence = ZByte.ParseSafe((Count + 1).ToString(CultureInfo.InvariantCulture), 0);
		}

		#endregion

		#region Move / Sequence

		public void MoveUp(WhsProductStyleSize size)
		{
			if (!SequenceSemaphore.IsSuspended)
			{
				Sequencer.MoveUp(size);
			}
		}

		public void MoveDown(WhsProductStyleSize size)
		{
			if (!SequenceSemaphore.IsSuspended)
			{
				Sequencer.MoveDown(size);
			}
		}

		public void Sequence()
		{
			if (!SequenceSemaphore.IsSuspended)
			{
				Sequencer.Sequence();
			}
		}

		CollectionSequencer<WhsProductStyleSize> Sequencer
		{
			get { return sequencer ?? (sequencer = new CollectionSequencer<WhsProductStyleSize>(this, WhsProductStyleSizeSchema.WSZ_Sequence, b => b.Validation.ValidateWSZ_Sequence())); }
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		CollectionSequencer<WhsProductStyleSize> sequencer;

		public Semaphore SequenceSemaphore
		{
			get { return sequenceSemaphore ?? (sequenceSemaphore = new Semaphore()); }
		}

#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		Semaphore sequenceSemaphore;

		#endregion
	}
}
