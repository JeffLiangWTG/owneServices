using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class CusContainerSynchroniser : BusinessObjectSynchroniser
	{
		public CusContainerSynchroniser(BaseCusContainer destination, CommonContainer source)
			: base(destination, source)
		{
		}

		public new BaseCusContainer Destination
		{
			get { return (BaseCusContainer)base.Destination; }
		}

		public new CommonContainer Source
		{
			get { return (CommonContainer)base.Source; }
		}

		#region Implementation

		protected override void HookSynchronisers()
		{
			base.HookSynchronisers();
			Synchronisers.Add(new FieldSynchroniser(Destination.CO_ContainerNumberInfo, Source.JC_ContainerNumInfo));
			var sealSynchroniser = new FieldSynchroniser(Destination.CO_SealInfo, Source.JC_SealNumInfo);
			Synchronisers.Add(sealSynchroniser);
			sealSynchroniser.Format += SealSynchroniser_Format;
			var secondSealSynchroniser = new FieldSynchroniser(Destination.CO_SecondSealInfo, Source.JC_AdditionalSealNumInfo);
			secondSealSynchroniser.Format += SealSynchroniser_Format;
			Synchronisers.Add(secondSealSynchroniser);
			Synchronisers.Add(new FieldSynchroniser(Destination.CO_RCInfo, Source.JC_RCInfo));

			var containerModeSynchroniser = new FieldSynchroniser(Destination.CO_FCL_LCL_AIRInfo, Source.JC_ContainerModeInfo);
			containerModeSynchroniser.Format += ContainerModeSynchroniser_Format;
			Synchronisers.Add(containerModeSynchroniser);

			Synchronisers.Add(new FieldSynchroniser(Destination.CO_WeightInfo, GetTotalGoodsWeight, GetTotalGoodsWeightInfos));
			Synchronisers.Add(new FieldSynchroniser(Destination.CO_WeightUQInfo, GetGoodsWeightUQ, GetTotalGoodsWeightUQInfos));
		}

		protected override void ForceSynchroniseCore()
		{
			((IBusinessObjectInternals)Destination).IsCopying = true;
			try
			{
				base.ForceSynchroniseCore();
			}
			finally
			{
				((IBusinessObjectInternals)Destination).IsCopying = false;
			}
		}

		void SealSynchroniser_Format(object sender, FieldSynchroniser.ConvertEventArgs e)
		{
			if (e.DesiredType == typeof(ZString))
			{
				e.Value = ((ZString)e.Value).ToUpper();
			}
		}

		void ContainerModeSynchroniser_Format(object sender, FieldSynchroniser.ConvertEventArgs e)
		{
			if (e.DesiredType == typeof(ZString))
			{
				e.Value = Destination.GetContainerModeFromFreight(Source.JC_ContainerMode);
			}
		}

		IZType GetTotalGoodsWeight()
		{
			var result = ZDecimal.Zero;

			if (Source.Consol.JK_ConsolMode == "BCN")
			{
				var container = (CommonContainer)Source.Consol.Containers.FirstOrDefault(x => x.PK == Source.PK);
				if (container != null)
				{
					result = container.GoodsWeight;
				}
			}
			else
			{
				var unit = (ZString)GetGoodsWeightUQ();
				if (!unit.IsEmpty)
				{
					foreach (ForwardingPackLine packLine in Destination.Declaration.Shipment.OuterPackLines)
					{
						if (packLine.Containers.Contains(Source) && Core.Constants.Weight.ContainsCode(packLine.JL_ActualWeightUQ))
						{
							result += Core.Constants.Weight.Convert(packLine.JL_ActualWeight, packLine.JL_ActualWeightUQ, unit);
						}
					}
				}
			}
			if (!result.IsWithinSqlPrecisionAndScale(CusContainerSchema.CO_Weight.Precision, CusContainerSchema.CO_Weight.Scale))
			{
				result = Destination.CO_Weight;
			}
			return result;
		}

		/// <summary>
		/// Gets notified of changes via IPackLineSynchronise.MarkSyncDirty
		/// </summary>
		/// <returns></returns>
		ZPropertyInfo[] GetTotalGoodsWeightInfos()
		{
			return Array.Empty<ZPropertyInfo>();
		}

		IZType GetGoodsWeightUQ()
		{
			ZString? result = null;
			if (Source.Consol.JK_ConsolMode == "BCN")
			{
				var container = (CommonContainer)Source.Consol.Containers.FirstOrDefault(x => x.PK == Source.PK);
				if (container != null)
				{
					result = container.GoodsWeightUQ;
				}
			}
			else
			{
				foreach (ForwardingPackLine packLine in Destination.Declaration.Shipment.OuterPackLines)
				{
					if (packLine.Containers.Contains(Source) && Core.Constants.Weight.ContainsCode(packLine.JL_ActualWeightUQ))
					{
						if (!result.HasValue)
						{
							result = packLine.JL_ActualWeightUQ;
						}
						else if (result.Value != packLine.JL_ActualWeightUQ)
						{
							result = Core.Constants.Weight.Kilograms;
							break;
						}
					}
				}
			}

			return result ?? ZString.Empty;
		}

		/// <summary>
		/// Gets notified of changes via IPackLineSynchronise.MarkSyncDirty
		/// </summary>
		/// <returns></returns>
		ZPropertyInfo[] GetTotalGoodsWeightUQInfos()
		{
			return Array.Empty<ZPropertyInfo>();
		}

		#endregion
	}
}
