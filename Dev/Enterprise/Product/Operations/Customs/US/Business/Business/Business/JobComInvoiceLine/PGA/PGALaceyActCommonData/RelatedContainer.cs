using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class RelatedContainer : NonPersistentBusinessObject
		, IObsoleteValidation, IContainerNumber
	{
		public RelatedContainer(PGA pga)
			: base(pga.Factory)
		{
			this.pga = pga;
		}
		readonly PGA pga;

		#region related objects

		public void SetContainerInvoiceLinePivot(CusContainerInvoiceLinePivot containerInvoiceLinePivot)
		{
			ContainerInvoiceLinePivot = containerInvoiceLinePivot;
		}

		CusContainerInvoiceLinePivot ContainerInvoiceLinePivot
		{
			get { return containerInvoiceLinePivot == null || containerInvoiceLinePivot.IsDeleted ? null : containerInvoiceLinePivot; }
			set
			{
				containerInvoiceLinePivot = value;
				IsForPGALine = pga.ContainersForPGALine.Contains(containerInvoiceLinePivot);
			}
		}
		CusContainerInvoiceLinePivot containerInvoiceLinePivot;

		public PGARelatedContainersGenPivot Pivot
		{
			get { return ContainerInvoiceLinePivot != null ? pga.ContainersForPGALine.GetRelatedPivot(ContainerInvoiceLinePivot) : null; }
		}

		#endregion

		#region Properties

		#region ContainerNumber

		public ZString ContainerNumber
		{
			get { return ContainerInvoiceLinePivot == null || ContainerInvoiceLinePivot.Container == null ? ZString.Empty : ContainerInvoiceLinePivot.Container.CO_ContainerNumber; }
		}

		public ZPropertyInfo ContainerNumberInfo
		{
			get { return GetZPropertyInfo(nameof(ContainerNumber)); }
		}
		#endregion

		#region IsForPGALine

		[BusinessObjectTestExclude()]
		public ZBool IsForPGALine
		{
			get { return isForPGALine; }
			set
			{
				if (isForPGALine != value)
				{
					isForPGALine = value;

					if (containerInvoiceLinePivot != null)
					{
						if (isForPGALine)
						{
							pga.ContainersForPGALine.AddPivotFor(ContainerInvoiceLinePivot);
						}
						else
						{
							pga.ContainersForPGALine.DeletePivotFor(ContainerInvoiceLinePivot);
						}
					}
					if (!IsValidationSuspended)
					{
						ValidateIsForPGALine();
					}

					IsForPGALineInfo.RefreshBinding();
				}
			}
		}
		ZBool isForPGALine;

		public ZPropertyInfo IsForPGALineInfo
		{
			get { return GetZPropertyInfo(nameof(IsForPGALine)); }
		}

		#endregion

		#endregion

		void ValidateIsForPGALine()
		{
			IsForPGALineInfo.ClearAllNotifications();
			if (IsForPGALine)
			{
				PGARelatedContainersGenPivot pivot = Pivot;
				if (pivot != null)
				{
					pivot.Validation.ValidateXX_Relation2ID();
					IsForPGALineInfo.AddAllNotificationsFrom(pivot.XX_Relation2IDInfo);
				}
			}
		}

		protected override void AddToFactoryCache()
		{
			//DO NOT Allow memory to be held up by factory
		}

		#region IContainerNumber Members

		ZString IContainerNumber.ContainerEquipmentID
		{
			get { return ContainerNumber; }
		}

		#endregion
	}
}
