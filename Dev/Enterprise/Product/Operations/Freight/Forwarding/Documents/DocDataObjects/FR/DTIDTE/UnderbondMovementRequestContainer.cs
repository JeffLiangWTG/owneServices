using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	internal class UnderbondMovementRequestContainer : DocDataObject
	{
		#region Number

		public ZString Number
		{
			get => number;
			set
			{
				if (SetNonPersistentPropertyValue(NumberInfo, ref number, value))
				{
					Validate(NumberInfo);
				}
			}
		}

		ZString number;

		public ZPropertyInfo NumberInfo => GetZPropertyInfo(nameof(Number));

		#endregion

		#region ECTICTNumber

		public ZString ECTICTNumber
		{
			get => ecticTNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ECTICTNumberInfo, ref ecticTNumber, value))
				{
					Validate(ECTICTNumberInfo);
				}
			}
		}

		ZString ecticTNumber;

		public ZPropertyInfo ECTICTNumberInfo => GetZPropertyInfo(nameof(ECTICTNumber));

		#endregion

		#region Mode

		public ContainerMode ContainerMode
		{
			get => containerMode;
			set => containerMode = SetChild(containerMode, value);
		}

		ContainerMode containerMode;

		#endregion

		#region Type

		public ContainerType ContainerType
		{
			get => containerType;
			set => containerType = SetChild(containerType, value);
		}

		ContainerType containerType;

		#endregion

		#region IsEmptyContainer

		public ZBool IsEmptyContainer
		{
			get => isEmptyContainer;
			set
			{
				if (SetNonPersistentPropertyValue(IsEmptyContainerInfo, ref isEmptyContainer, value))
				{
					Validate(IsEmptyContainerInfo);
				}
			}
		}

		ZBool isEmptyContainer;

		public ZPropertyInfo IsEmptyContainerInfo => GetZPropertyInfo(nameof(IsEmptyContainer));

		#endregion

		#region IsNonOperativeReefer

		public ZBool IsNonOperativeReefer
		{
			get => isNonOperativeReefer;
			set
			{
				if (SetNonPersistentPropertyValue(IsNonOperativeReeferInfo, ref isNonOperativeReefer, value))
				{
					Validate(IsNonOperativeReeferInfo);
				}
			}
		}

		ZBool isNonOperativeReefer;

		public ZPropertyInfo IsNonOperativeReeferInfo => GetZPropertyInfo(nameof(IsNonOperativeReefer));

		#endregion
	}
}
