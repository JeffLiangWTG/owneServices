using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class AsycudaContainer : ASYCUDA.Business.AsycudaContainer, Integration.Customs.ASYCUDA.TRETrade.IAsycudaContainer
	{
		public AsycudaContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new partial class Schema : ManifestBase.AutoAsycudaContainer.Schema
		{
			public const string ContainerLevel = "ContainerLevel";
		}

		public ZString ContainerLevel
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.ContainerLevel);
			set
			{
				using (this.SuspendSettingHasChanges())
				using (this.SuspendMarkingAsNeedingValidation())
				{
					this.SetSystemDefinedValue(Schema.ContainerLevel, value);
				}
			}
		}

		public override void OnSaving()
		{
			if (ACN_ContainerNumber.IsEmpty)
			{
				this.Delete();
			}
			base.OnSaving();
		}
	}
}
