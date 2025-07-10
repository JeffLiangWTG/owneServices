using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class FDARelatedContainer : NonPersistentBusinessObject, IObsoleteValidation, IUSContainer
	{
		public FDARelatedContainer(IFDARelatedContainer fda)
			: base(fda.Factory)
		{
			this.fda = fda;
		}
		readonly IFDARelatedContainer fda;

		#region related objects

		public void SetContainerInvoiceLinePivot(CusContainerInvoiceLinePivot containerInvoiceLinePivot)
		{
			ContainerInvoiceLinePivot = containerInvoiceLinePivot;
		}

		public CusContainerInvoiceLinePivot ContainerInvoiceLinePivot
		{
			get { return containerInvoiceLinePivot == null || containerInvoiceLinePivot.IsDeleted ? null : containerInvoiceLinePivot; }
			set
			{
				containerInvoiceLinePivot = value;
				IsForFDALine = fda.ContainersForFDALine.Contains(containerInvoiceLinePivot);
			}
		}
		CusContainerInvoiceLinePivot containerInvoiceLinePivot;

		public FDARelatedContainersGenPivot Pivot
		{
			get { return ContainerInvoiceLinePivot != null ? fda.ContainersForFDALine.GetRelatedPivot(ContainerInvoiceLinePivot) : null; }
		}

		#endregion

		#region Properties

		#region ContainerNumber

		public ZString ContainerNumber
		{
			get
			{
				var container = ContainerInvoiceLinePivot == null ? null : ContainerInvoiceLinePivot.Container;
				return container == null ? ZString.Empty : container.CO_ContainerNumber;
			}
		}

		public ZPropertyInfo ContainerNumberInfo
		{
			get { return GetZPropertyInfo(nameof(ContainerNumber)); }
		}
		#endregion

		#region IsForFDALine

		[BusinessObjectTestExclude()]
		public ZBool IsForFDALine
		{
			get { return isForFDALine; }
			set
			{
				if (isForFDALine != value)
				{
					isForFDALine = value;

					if (ContainerInvoiceLinePivot != null)
					{
						if (isForFDALine)
						{
							fda.ContainersForFDALine.AddPivotFor(ContainerInvoiceLinePivot);
						}
						else
						{
							fda.ContainersForFDALine.DeletePivotFor(ContainerInvoiceLinePivot);
						}
					}
					if (!IsValidationSuspended)
					{
						ValidateIsForFDALine();
					}
					IsForFDALineInfo.RefreshBinding();
				}
			}
		}
		ZBool isForFDALine;

		public ZPropertyInfo IsForFDALineInfo
		{
			get { return GetZPropertyInfo(nameof(IsForFDALine)); }
		}

		#endregion

		#endregion

		protected override void AddToFactoryCache()
		{
			//DO NOT Allow memory to be held up by factory
		}

		void ValidateIsForFDALine()
		{
			IsForFDALineInfo.ClearAllNotifications();
			if (IsForFDALine)
			{
				FDARelatedContainersGenPivot pivot = Pivot;
				if (pivot != null)
				{
					pivot.Validation.ValidateXX_Relation2ID();
					IsForFDALineInfo.AddAllNotificationsFrom(pivot.XX_Relation2IDInfo);
				}
			}
		}

		#region IContainerNumber Members

		ZString IContainerNumber.ContainerEquipmentID
		{
			get { return ContainerNumber; }
		}

		#endregion

		#region IUSContainer Members

		ZString IUSContainer.USContainerCode
		{
			get
			{
				ZString result = ZString.Empty;

				if (ContainerInvoiceLinePivot != null)
				{
					if (ContainerInvoiceLinePivot.Container.Container != null)
					{
						result = ContainerInvoiceLinePivot.Container.Container.GetCountrySpecificContainerCode(Enterprise.Core.Constants.CountryCodes.UnitedStates);
					}
				}

				return result;
			}
		}

		ZBool IUSContainer.IsRailCar
		{
			get
			{
				ZBool result = false;

				if (ContainerInvoiceLinePivot != null)
				{
					CusContainer tempContainer = ContainerInvoiceLinePivot.Container;
					if (ContainerInvoiceLinePivot.Container.Container != null)
					{
						result = tempContainer.IsRailCar(ContainerInvoiceLinePivot.Container.Container.GetCountrySpecificContainerCode(Enterprise.Core.Constants.CountryCodes.UnitedStates));
					}
				}

				return result;
			}
		}

		#endregion
	}
}
