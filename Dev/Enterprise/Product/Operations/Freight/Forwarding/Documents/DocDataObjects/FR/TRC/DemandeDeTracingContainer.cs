using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	public sealed class DemandeDeTracingContainer : DocDataObject
	{
		public DemandeDeTracingContainer(object identifier)
			: base(identifier)
		{
		}

		#region ContainerNumber

		public ZString ContainerNumber
		{
			get => containerNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ContainerNumberInfo, ref containerNumber, value))
				{
					Validate(ContainerNumberInfo);
				}
			}
		}

		ZString containerNumber;

		public ZPropertyInfo ContainerNumberInfo => GetZPropertyInfo(nameof(ContainerNumber));

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
