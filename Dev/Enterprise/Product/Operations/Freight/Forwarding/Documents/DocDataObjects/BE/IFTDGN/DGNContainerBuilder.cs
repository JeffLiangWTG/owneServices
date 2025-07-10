using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BE
{
	sealed class DGNContainerBuilder
	{
		public DGNContainer Build(CommonContainer containerBO, IReadOnlyCollection<DGNPackingLine> packingLines = null)
		{
			if (containerBO == null)
			{
				return null;
			}

			var context = new CommonContext(containerBO.Factory);
			var container = new DGNContainer(containerBO.PK);
			container.PackingLines = packingLines;

			var dgnIndex = 1;
			foreach (var dgnDangerousGood in container.PackingLines.OfType<DGNPackingLine>().SelectMany(p => p.DangerousGoods))
			{
				dgnDangerousGood.ContainerDGNIndex = dgnIndex++;
				AddDangerousGoodValidation(dgnDangerousGood);
			}
			container.Number = containerBO.JC_ContainerNum;
			container.PackCount = packingLines?.Sum(p => p.Quantity) ?? 0;
			container.HasDangerousGoods = container.PackingLines != null && container.PackingLines.Any(p => p.DangerousGoods != null && p.DangerousGoods.Any());
			container.IsNonOperativeReefer = containerBO.JC_IsNonOperativeReefer;

			return container;
		}

		void AddDangerousGoodValidation(DGNDangerousGood dgnDangerousGood)
		{
			var errorMessage = Res.GetString("4579BCFF-8240-4E8D-B454-F795834238FC", "At most one of the options (Limited Quantity/Excepted Quantity) can be selected.");
			var missingRadioactivityUnitCode = Res.GetString("A68E974D-84EA-4066-A037-A6F9F097B4B6", "Unit of Radioactivity is required.");
			var missingNetExplosiveWeightUnitCode = Res.GetString("9F24CB03-21BE-4721-BDD8-6016A11122F7", "Unit of Net explosive weight is required.");

			dgnDangerousGood.PackedInExceptedQuantityInfo.AddMessageError(() => ((bool)dgnDangerousGood.PackedInExceptedQuantity && (bool)dgnDangerousGood.PackedInLimitedQuantity), errorMessage);
			dgnDangerousGood.PackedInLimitedQuantityInfo.AddMessageError(() => ((bool)dgnDangerousGood.PackedInExceptedQuantity && (bool)dgnDangerousGood.PackedInLimitedQuantity), errorMessage);
			((CodeDescription)dgnDangerousGood.Radioactivity.Unit).CodeInfo.AddMessageError(() => (dgnDangerousGood.Radioactivity.Value > 0 && dgnDangerousGood.Radioactivity.Unit.Code.IsEmpty), missingRadioactivityUnitCode);
			((CodeDescription)dgnDangerousGood.NetExplosiveWeight.Unit).CodeInfo.AddMessageError(() => (dgnDangerousGood.NetExplosiveWeight.Value > 0 && dgnDangerousGood.NetExplosiveWeight.Unit.Code.IsEmpty), missingNetExplosiveWeightUnitCode);
		}
	}
}
