using System;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.LVS.Business
{
	public class NonApplicableCusUSLVConsignmentCollection : ActiveBusinessObjectCollection<CusUSLVConsignment>
	{
		public NonApplicableCusUSLVConsignmentCollection(CusUSLVClearance clearance)
			: base(clearance)
		{
		}

		public CusUSLVClearance Clearance => (CusUSLVClearance)Relationship.Master;

		protected override bool MatchesFilterCore(CusUSLVConsignment consignment, bool fetchOnlyFromLocalCache)
		{
			return consignment.CanBeConvertedToStandaloneDeclaration && consignment.CE_EntryNum.IsEmpty;
		}

		public event EventHandler<ConvertSelectionChangedEventArgs> ConvertSelectionChanged;

		protected override void OnLoadedIntoCollectionCore(CusUSLVConsignment loadedObject)
		{
			loadedObject.ShouldConvertToStandaloneDeclarationInfo.ValueChanged += OnConvertSelectionChanged;
			base.OnLoadedIntoCollectionCore(loadedObject);
		}

		void OnConvertSelectionChanged(object sender, EventArgs e)
		{
			if (ConvertSelectionChanged != null && !selectionChangedEventSuspended)
			{
				var selectedAll = this.All(c => c.ShouldConvertToStandaloneDeclaration);
				var selectedNone = !this.Any(c => c.ShouldConvertToStandaloneDeclaration);
				ConvertSelectionChanged.Invoke(sender, new ConvertSelectionChangedEventArgs(selectedAll, selectedNone));
			}
		}

		bool selectionChangedEventSuspended;

		protected override object[] GetCollectionState()
		{
			return new object[] { selectionChangedEventSuspended };
		}

		protected override bool AllowNew => false;

		public void UnSelectedAll()
		{
			selectionChangedEventSuspended = true;
			try
			{
				foreach (var consignment in this.Where(c => c.ShouldConvertToStandaloneDeclaration))
				{
					consignment.ShouldConvertToStandaloneDeclaration = false;
				}
			}
			finally
			{
				selectionChangedEventSuspended = false;
			}
		}

		public void SelectedAll()
		{
			selectionChangedEventSuspended = true;
			try
			{
				foreach (var consignment in this.Where(c => !c.ShouldConvertToStandaloneDeclaration))
				{
					consignment.ShouldConvertToStandaloneDeclaration = true;
				}
			}
			finally
			{
				selectionChangedEventSuspended = false;
			}
		}

		public void Refresh()
		{
			((IActiveBusinessObjectCollection)this).Refresh();
		}
	}

	public class ConvertSelectionChangedEventArgs : EventArgs
	{
		public ConvertSelectionChangedEventArgs(bool selectedAll = false, bool selectedNone = false)
		{
			SelectedAll = selectedAll;
			SelectedNone = selectedNone;
		}

		public bool SelectedAll { get; set; }
		public bool SelectedNone { get; set; }
	}
}
