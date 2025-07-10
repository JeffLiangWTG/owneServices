using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Packing.GUI
{
	public class PackingTreeNodeSummary
	{
		// tokens

		#region Tokens

		public IList<ICustomizableColumn> Tokens => TokensInternal.OrderBy(t => t.ColumnPosition).ToArray();
		public IReadOnlyCollection<PackingTreeNodeSummaryToken> GetOrderedVisibleTokens() => TokensInternal.Where(t => t.IsVisible).OrderBy(t => t.ColumnPosition).ToArray();

		List<PackingTreeNodeSummaryToken> TokensInternal
		{
			get
			{
				if (tokens == null)
				{
					tokens = new List<PackingTreeNodeSummaryToken>(new[]
					{
						new PackingTreeNodeSummaryToken(Constants.PackageContentsID, Constants.PackageContents, 1, GetContents) { IsVisible = true },
						new PackingTreeNodeSummaryToken(Constants.PackageIDID, Constants.PackageID, 2, GetPackageID, GetPackageIDCaption) { IsVisible = true },
						new PackingTreeNodeSummaryToken(Constants.WeightID, Constants.Weight, 3, GetWeight, GetWeightCaption) { IsVisible = true },
						new PackingTreeNodeSummaryToken(Constants.VolumeID, Constants.Volume, 4, GetVolume, GetVolumeCaption),
						new PackingTreeNodeSummaryToken(Constants.DimensionsID, Constants.Dimensions, 5, GetDIMs, GetDIMsCaption),
						new PackingTreeNodeSummaryToken(Constants.CommodityID, Constants.Commodity, 6, GetCommodity, GetCommodityCaption),
						new PackingTreeNodeSummaryToken(Constants.TemperatureID, Constants.Temperature, 7, GetTemperature, GetTemperatureCaption),
						new PackingTreeNodeSummaryToken(Constants.PackageSequenceID, Constants.PackageSequence, 8, GetPackageSequence, GetPackageSequenceCaption),
						new PackingTreeNodeSummaryToken(Constants.HeldID, Constants.Held, 9, GetHeld, GetHeldCaption),
						new PackingTreeNodeSummaryToken(Constants.HandlingUnitID, Constants.HandlingUnit, 10, GetHandlingUnit, GetHandlingUnitCaption)
					});
				}
				return tokens;
			}
		}

		List<PackingTreeNodeSummaryToken> tokens;

		#region Constants

		public static class Constants
		{
			public static string Dimensions { get { return Res.GetString(DimensionsID, "Dimensions"); } }
			public static string PackageContents { get { return Res.GetString(PackageContentsID, "Package Content Summary"); } }
			public static string PackageID { get { return Res.GetString(PackageIDID, "Package ID"); } }
			public static string Volume { get { return Res.GetString(VolumeID, "Volume"); } }
			public static string Weight { get { return Res.GetString(WeightID, "Weight"); } }
			public static string Commodity { get { return Res.GetString(CommodityID, "Commodity"); } }
			public static string Temperature { get { return Res.GetString(TemperatureID, "Temperature"); } }
			public static string PackageSequence { get { return Res.GetString(PackageSequenceID, "Package Sequence"); } }
			public static string Held { get { return Res.GetString(HeldID, "Held"); } }
			public static string HandlingUnit { get { return Res.GetString(HandlingUnitID, "Handling Unit"); } }

			public const string DimensionsID = "7c445e2d-d59f-4bfe-877e-4b721450cebb";
			public const string PackageContentsID = "600f0127-6604-4304-a236-d5410fc81783";
			public const string PackageIDID = "9e4c6d9a-3469-4c56-a9cf-0c5ea5130903";
			public const string VolumeID = "8219c074-ce0d-4781-b7f8-150236403754";
			public const string WeightID = "7d1d5f61-ae33-4fcf-a910-eee70072042a";
			public const string CommodityID = "edb17365-b64f-4036-bab4-c012639a6fd7";
			public const string TemperatureID = "7264e955-0b73-4d70-b7aa-651ff132007e";
			public const string PackageSequenceID = "C396B592-6675-4317-BE21-576428221C8E";
			public const string HeldID = "935E6EFD-E2D9-45F9-9B26-D9A67A6CA4AB";
			public const string HandlingUnitID = "5DEF5DE8-DA47-4AC6-B58D-6DB7AA58774F";
		}

		#endregion

		#endregion

		#region UpdateTokens

		public void UpdateTokens(IReadOnlyList<PackingTreeNodeSummaryToken> proposedTokensInDisplayOrder)
		{
			var tokensByColumnName = new Dictionary<string, (PackingTreeNodeSummaryToken Token, byte Position)>();

			for (byte index = 0; index < proposedTokensInDisplayOrder.Count; index++)
			{
				var token = proposedTokensInDisplayOrder[index];
				tokensByColumnName.Add(token.ColumnName, (token, index));
			}

			foreach (var token in TokensInternal)
			{
				if (tokensByColumnName.TryGetValue(token.ColumnName, out var updatedToken))
				{
					// make proposed tokens visible and set display order
					token.IsVisible = updatedToken.Token.IsVisible;
					token.ColumnPosition = updatedToken.Position;
				}
				else
				{
					// hide non-visible tokens
					token.IsVisible = false;
				}
			}
		}

		#endregion

		// delegates for package Caption + Text (eg: "Wgt: 100KG")

		#region GetCaption delegates

		ZString GetDIMsCaption(IPackageSummary packageSummary)
		{
			return packageSummary.DimensionsCaption;
		}

		ZString GetPackageIDCaption(IPackageSummary packageSummary)
		{
			return packageSummary.PackageIDCaption;
		}

		ZString GetVolumeCaption(IPackageSummary packageSummary)
		{
			return packageSummary.VolumeCaption;
		}

		ZString GetWeightCaption(IPackageSummary packageSummary)
		{
			return packageSummary.WeightCaption;
		}

		ZString GetCommodityCaption(IPackageSummary packageSummary)
		{
			return packageSummary.CommodityCaption;
		}

		ZString GetTemperatureCaption(IPackageSummary packageSummary)
		{
			return packageSummary.TemperatureCaption;
		}

		ZString GetPackageSequenceCaption(IPackageSummary packageSummary)
		{
			return packageSummary.PackageSequenceCaption;
		}

		ZString GetHeldCaption(IPackageSummary packageSummary)
		{
			return packageSummary.IsHeldCaption;
		}

		ZString GetHandlingUnitCaption(IPackageSummary packageSummary)
		{
			return packageSummary.HandlingUnitCaption;
		}

		#endregion

		#region GetText delegates

		// dims

		ZString GetDIMs(IPackageSummary packageSummary)
		{
			return packageSummary.Dimensions;
		}

		ZString GetPackageID(IPackageSummary packageSummary)
		{
			return packageSummary.PackageID;
		}

		ZString GetWeight(IPackageSummary packageSummary)
		{
			return packageSummary.Weight;
		}

		ZString GetVolume(IPackageSummary packageSummary)
		{
			return packageSummary.Volume;
		}

		ZString GetHeld(IPackageSummary packageSummary)
		{
			return packageSummary.IsHeld;
		}

		ZString GetHandlingUnit(IPackageSummary packageSummary)
		{
			return packageSummary.HandlingUnit;
		}

		// other

		ZString GetCommodity(IPackageSummary packageSummary)
		{
			return packageSummary.Commodity;
		}

		ZString GetTemperature(IPackageSummary packageSummary)
		{
			return packageSummary.IsTemperatureControlled ? packageSummary.Temperature : ZString.Empty;
		}

		ZString GetPackageSequence(IPackageSummary packageSummary)
		{
			return packageSummary.PackageSequence;
		}

		ZString GetContents(IPackageSummary packageSummary)
		{
			return packageSummary.Contents;
		}

		#endregion
	}
}
