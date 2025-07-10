using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	public class PointMatchingLog : List<PointMatchingLogEntry>, IPointMatchingLog
	{
		public PointMatchingLog(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		public BusinessObjectFactory Factory { get; }
		public MeasureType CurrentMeasureType { get; set; }

		public void LogPartDidNotMatchLine(string lineDisplayInfo, string partReadableName, string dimensionReadableName, string partValueAsText, string lineValueAsText)
		{
			Add(new PointMatchingLogEntry(lineDisplayInfo,
				CurrentMeasureType.ToString(),
				dimensionReadableName,
				partReadableName,
				partValueAsText,
				lineValueAsText,
				pointRejected: true));
		}

		public void LogPartMatchedLine(string lineDisplayInfo, string partReadableName, string partValueAsText)
		{
			Add(new PointMatchingLogEntry(lineDisplayInfo,
				CurrentMeasureType.ToString(),
				string.Empty,
				partReadableName,
				partValueAsText,
				string.Empty,
				pointRejected: false));
		}

		public string GetLog()
		{
			return this.GroupBy(x => x.RateLineInfo)
				.Select(grp => (NoResString)"\t\t\tRateLine " + grp.Key + System.Environment.NewLine // Constant needed for string formatting
						+ (NoResString)"\t\t\t\tJob's info:" + System.Environment.NewLine // part of autorating log
						+ grp.OrderBy(x => x.PointRejected)
							.ThenBy(x => x.EntityName)
							.Select(x => x.GetLogString())
							.ToStringWithNewLineBetweenStrings())
				.ToStringWithNewLineBetweenStrings();
		}

		public string GetPartName(IHasPartDimensions partList, IRateablePart part)
		{
			var name = string.Empty;

			if (partList.HasContainerType)
			{
				if (part.ContainerTypePk.HasValue)
				{
					var container = Factory.Load<RefContainer>(part.ContainerTypePk.Value);
					if (container != null)
					{
						name = container.HumanReadableName + " " + container.RC_Code;
					}
				}
			}
			else if (partList.HasProduct)
			{
				if (part.ProductPk.HasValue)
				{
					var product = Factory.Load<OrgSupplierPart>(part.ProductPk.Value);
					if (product != null)
					{
						name = product.HumanReadableName;
					}
				}
			}
			else if (partList.HasPalletID)
			{
				if (!string.IsNullOrEmpty(part.PalletID))
				{
					name = GetPalletName(part.PalletID);
				}
			}

			return name;
		}

		public static string GetPalletName(string palletID)
			=> Res.GetString("43992656-114C-4CA5-AAD3-7E0D52FBA2EA", "Pallet {0}", palletID);
	}
}
