using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Interfaces;

namespace Enterprise.Customs.Business
{
	public abstract class CusUnderbondUnionCollection : BusinessObjectCollection<CusUnderbond>
	{
		protected CusUnderbondUnionCollection(ICusUnderbondUnionCollectionParent parent) : base(parent.Factory)
		{
			this.parent = parent;
		}

		public new CusUnderbond AddNew(Type businessObjectType)
		{
			return base.AddNew(businessObjectType);
		}

		protected override bool AllowNewCore
		{
			get
			{
				return false;
			}
		}

		public CusUnderbond AddNewUnderbond(ICusUnderbondMessageSender sender)
		{
			ICusUnderbondDependentCollectionParent provider = sender.GetProviderToAddUnderbondTo(GetListOfAllPossibleProviders());
			if (provider != null)
			{
				CusUnderbond newUnderbond = provider.Underbonds.AddNew();
				base.Add(newUnderbond);
				newUnderbond.SetDefaultValuesFromParent();
				return newUnderbond;
			}
			return null;
		}

		protected ICusUnderbondDependentCollectionParent[] GetListOfAllPossibleProviders()
		{
			return parent.GetAllPossibleCollectionProviders();
		}

		public override void Load()
		{
			foreach (ICusUnderbondDependentCollectionParent possibleParent in GetListOfAllPossibleProviders())
			{
				foreach (CusUnderbond underbond in possibleParent.Underbonds)
				{
					base.Add(underbond);
				}
			}
		}

		protected ICusUnderbondUnionCollectionParent parent;

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);

			var child = (CusUnderbond)bizOAdded;
			child.C4_ModeOfMovementInfo.ValueChanged += new EventHandler(C4_ModeOfMovementInfo_ValueChanged);
			child.CanDoOutturnChanged += new EventHandler(CusUnderbondUnionCollection_CanDoOutturnChanged);
			child.C4_MovementReasonInfo.ValueChanged += new EventHandler(C4_MovementReasonInfo_ValueChanged);
		}

		protected override void OnRemoving(BusinessObject bizO)
		{
			base.OnRemoving(bizO);
			var child = (CusUnderbond)bizO;
			child.C4_ModeOfMovementInfo.ValueChanged -= new EventHandler(C4_ModeOfMovementInfo_ValueChanged);
			child.CanDoOutturnChanged -= new EventHandler(CusUnderbondUnionCollection_CanDoOutturnChanged);
			child.C4_MovementReasonInfo.ValueChanged -= new EventHandler(C4_MovementReasonInfo_ValueChanged);
		}

		void C4_ModeOfMovementInfo_ValueChanged(object sender, EventArgs e)
		{
			if (ModeOfMovementChanged != null)
			{
				ModeOfMovementChanged(this, EventArgs.Empty);
			}
		}

		void CusUnderbondUnionCollection_CanDoOutturnChanged(object sender, EventArgs e)
		{
			if (CanSendOutturnChanged != null)
			{
				CanSendOutturnChanged(this, EventArgs.Empty);
			}
		}

		void C4_MovementReasonInfo_ValueChanged(object sender, EventArgs e)
		{
			if (MovementReasonChanged != null)
			{
				MovementReasonChanged(this, EventArgs.Empty);
			}
		}

		public event EventHandler ModeOfMovementChanged;
		public event EventHandler CanSendOutturnChanged;
		public event EventHandler MovementReasonChanged;
	}
}
