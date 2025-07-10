using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;
using WinzorFramework.Extensions;

namespace CargoWise.NetworkVisualisation.GUI;

/// <summary>
/// Utility class for handling various style-related operations for network visualization.
/// </summary>
public static class StyleUtils
{
	/// <summary>
	/// Converts a <see cref="NodeColors"/> object to a CSS style string.
	/// </summary>
	/// <param name="colors">The <see cref="NodeColors"/> object containing color information.</param>
	/// <returns>A CSS style string representing the background color or gradient.</returns>
	public static string ToStyle(this NodeColors colors)
	{
		var colorCount = colors.colors.Count();
		if (colorCount == 0)
		{
			return string.Empty;
		}
		else if (colorCount == 1)
		{
			var color = colors.colors.First().color;
			return $"background-color: {color.GetColorStyleValue()};";
		}
		else
		{
			var colorsPercentage = string.Join(", ", colors.colors.Select(co =>
			{
				var percentage = co.offset * 100;
				return $"{co.color.GetColorStyleValue(colors.opacity)} {percentage}%";
			}));

			return $"background-image: linear-gradient({colors.angle + 90}deg, {colorsPercentage});";
		}
	}

	/// <summary>
	/// Converts a <see cref="NodeFontWeight"/> enum value to a corresponding font weight integer.
	/// </summary>
	/// <param name="weight">The <see cref="NodeFontWeight"/> value.</param>
	/// <returns>An integer representing the font weight.</returns>
	public static int ToWeight(this NodeFontWeight weight)
	{
		switch (weight)
		{
			case NodeFontWeight.Thin:
				return 100;
			case NodeFontWeight.UltraLight:
			case NodeFontWeight.ExtraLight:
				return 200;
			case NodeFontWeight.Light:
				return 300;
			default:
			case NodeFontWeight.Normal:
			case NodeFontWeight.Regular:
				return 400;
			case NodeFontWeight.Medium:
				return 500;
			case NodeFontWeight.DemiBold:
			case NodeFontWeight.SemiBold:
				return 600;
			case NodeFontWeight.Bold:
				return 700;
			case NodeFontWeight.ExtraBold:
			case NodeFontWeight.UltraBold:
				return 800;
			case NodeFontWeight.Black:
			case NodeFontWeight.Heavy:
				return 900;
			case NodeFontWeight.ExtraBlack:
			case NodeFontWeight.UltraBlack:
				return 950;
		}
	}

	/// <summary>
	/// Gets the brush color for a notification based on the state of the network entity.
	/// </summary>
	/// <param name="entity">The <see cref="INetworkEntity"/> to check for notifications.</param>
	/// <returns>
	/// A string representing the color for notifications:
	/// "Red" if the entity has errors,
	/// "Orange" if it has warnings,
	/// "DodgerBlue" if it has messages,
	/// or null if there are no notifications.
	public static string GetBrushColorForNotification(this INetworkEntity entity)
	{
		if (entity != null)
		{
			if (entity.HasErrors())
			{
				return (NoResString)"Red";
			}
			else if (entity.HasWarnings())
			{
				return (NoResString)"Orange";
			}
			else if (entity.HasMessages())
			{
				return "DodgerBlue";
			}
		}
		return null;
	}
}
