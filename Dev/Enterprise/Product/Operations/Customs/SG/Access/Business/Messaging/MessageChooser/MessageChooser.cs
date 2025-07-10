using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using ISelectionItem = Enterprise.Customs.ASYCUDA.Business.ISelectionItem;

namespace Enterprise.Customs.SG.Access.Business
{
	public interface ICycleDetailSupporter
	{
		ZBool RequiresCycleFields { get; }
		ZDateTime CycleDate { get; }
		ZString CycleNumber { get; }
	}

	public class MessageChooser : ASYCUDA.Business.MessageChooser, ICycleDetailSupporter
	{
		public MessageChooser(AsycudaManifestHeader header, IEnumerable<ISelectionItem> items, bool showStatus)
			: base(header, items, showStatus)
		{
		}

		#region Schema
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Schema : ASYCUDA.Business.AutoMessageChooser.Schema
		{
			public const string CycleDate = "CycleDate";
			public const string CycleNumber = "CycleNumber";
			public const int CycleNumberMaxLength = 10;
		}
		#endregion

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

		public ZBool RequiresCycleFields { get; private set; }

		public void KeepCycleFieldsConsistentIfNeeded()
		{
			if (RequiresCycleFields)
			{
				foreach (MessageChooserItem chooserItem in GetSelectedMessageChooserItems())
				{
					var chooser = chooserItem.Chooser;
					var bill = chooserItem.Bill as AsycudaBill;
					if (chooser != null && bill != null)
					{
						if (bill.CycleDate != chooser.CycleDate)
						{
							bill.CycleDate = chooser.CycleDate;
						}

						if (bill.CycleNumber != chooser.CycleNumber)
						{
							bill.CycleNumber = chooser.CycleNumber;
						}
					}
				}
			}
		}

		protected override void DefaultSelect(bool showStatus)
		{
			RequiresCycleFields = showStatus && Header.IsImport;
			if (RequiresCycleFields)
			{
				var bc = ChooserItems.Cast<MessageChooserItem>()
					.Select(x => x.Bill as AsycudaBill)
					.Where(x =>
					{
						ZString cycNumber;
						return x != null && !x.CycleDate.IsEmpty && !(cycNumber = x.CycleNumber).IsEmpty && x.Lookups.CycleNumbers.ContainsCode(cycNumber);
					})
					.OrderBy(x => x.CycleDate).ThenBy(x => ZInt.TryParse(x.CycleNumber, out var no) ? no : ZInt.Zero)
					.FirstOrDefault();
				CycleDate = bc?.CycleDate ?? ZDateTime.Empty;
				CycleNumber = bc?.CycleNumber ?? ZString.Empty;
			}
			else
			{
				base.DefaultSelect(showStatus);
			}
		}

		public new MessageChooserItemCollection ChooserItems => (MessageChooserItemCollection)base.ChooserItems;
		protected override ASYCUDA.Business.MessageChooserItemCollection CreateNewMessageChooserItemCollection()
		{
			return new MessageChooserItemCollection();
		}

		#region CycleDate
		[CargoWiseOne.ResourceStrings.ResourceStringData("SGMessageChooser|CycleDate", Caption = "Cycle Date")]
		public virtual ZDateTime CycleDate
		{
			get => cycleDate;
			set
			{
				if (SetNonPersistentPropertyValue(CycleDateInfo, ref cycleDate, value))
				{
					SelectCycleNodes();
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateCycleDate();
				}
			}
		}
		ZDateTime cycleDate;

		public virtual ZPropertyInfo CycleDateInfo => GetZPropertyInfo(Schema.CycleDate);

		void SelectCycleNodes()
		{
			foreach (MessageChooserItem node in ChooserItems)
			{
				var bill = node.Bill as AsycudaBill;
				node.Checked = bill != null
							&& bill.CycleDate == CycleDate
							&& bill.CycleNumber == CycleNumber
							&& ShouldSelectChooserItem(node);
			}
		}
		#endregion

		#region CycleNumber
		[CargoWiseOne.ResourceStrings.ResourceStringData("SGMessageChooser|CycleNumber", Caption = "Cycle Number")]
		[MaxLength(Schema.CycleNumberMaxLength)]
		[List(nameof(Lookups) + "." + nameof(MessageChooserLookups.CycleNumbers))]
		public virtual ZString CycleNumber
		{
			get => cycleNumber;
			set
			{
				CheckMaximumLength(CycleNumberInfo, value);
				if (SetNonPersistentPropertyValue(CycleNumberInfo, ref cycleNumber, value))
				{
					SelectCycleNodes();
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateCycleNumber();
				}
			}
		}
		ZString cycleNumber;

		public virtual ZPropertyInfo CycleNumberInfo => GetZPropertyInfo(Schema.CycleNumber);
		#endregion

		public new MessageChooserValidation Validation => (MessageChooserValidation)base.Validation;
		protected override ASYCUDA.Business.MessageChooserValidation GetNewValidation() => new MessageChooserValidation(this);

		public new MessageChooserLookups Lookups => (MessageChooserLookups)base.Lookups;
		protected override ASYCUDA.Business.MessageChooserLookups GetNewLookups() => new MessageChooserLookups(this);
	}
}
