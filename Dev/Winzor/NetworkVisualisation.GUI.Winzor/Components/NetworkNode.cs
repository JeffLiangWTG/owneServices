#nullable enable
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI.Models;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.GUI.Components;

/// <summary>
/// Represents a base class for network nodes.
/// </summary>
/// <typeparam name="T">The type of the network node model.</typeparam>
public abstract class NetworkNode<T> : NetworkNodeBase<T> where T : NetworkNodeModel
{
	/// <summary>
	/// Gets the status color style of the node, this color is used as the node's background color.
	/// </summary>
	/// <returns>The CSS Representation of the node's background color or an empty string.</returns>
	public string StatusColorStyle => Node?.StatusColors.ToStyle() ?? string.Empty;

	/// <summary>
	/// Gets the font weight of the status text.
	/// </summary>
	/// <returns>The font weight of the status text, represented as an integer value.</returns>
	public int StatusFontWeight => Node?.StatusTextWeight switch
	{
		NodeFontWeight.Thin => 100,
		NodeFontWeight.UltraLight or NodeFontWeight.ExtraLight => 200,
		NodeFontWeight.Light => 300,
		NodeFontWeight.Normal or NodeFontWeight.Regular => 400,
		NodeFontWeight.Medium => 500,
		NodeFontWeight.DemiBold or NodeFontWeight.SemiBold => 600,
		NodeFontWeight.Bold => 700,
		NodeFontWeight.UltraBold or NodeFontWeight.ExtraBold => 800,
		NodeFontWeight.Black or NodeFontWeight.Heavy => 900,
		NodeFontWeight.ExtraBlack or NodeFontWeight.UltraBlack => 950,
		_ => 400,
	};

	/// <summary>
	/// Gets the entity state image associated with the node, the entity state image is displayed as an icon in the node.
	/// </summary>
	/// <returns> The image resource associated with the entity state, or null if no image is associated.</returns>
	public string? EntityStateImage
	{
		get
		{
			if (Node is null)
			{
				return null;
			}

			if (Node.EntityState.HasFlag(EntityState.HasErrors))
			{
				return Resources.Error;
			}
			else if (Node.EntityState.HasFlag(EntityState.HasWarnings))
			{
				return Resources.Warning;
			}
			else if (Node.EntityState.HasFlag(EntityState.HasMessages))
			{
				return Resources.Message;
			}
			else if (Node.EntityState.HasFlag(EntityState.Approved))
			{
				return Resources.ThumbUp;
			}
			else if (Node.EntityState.HasFlag(EntityState.NotApproved))
			{
				return Resources.ThumbUpQuestionMark;
			}
			else if (Node.EntityState.HasFlag(EntityState.Fixed))
			{
				return Resources.pin_in;
			}
			else
			{
				return null;
			}
		}
	}

	/// <summary>
	/// Gets the notifications brush color associated with the node, this is used to display a border color on the left hand side of a node.	/// </summary>
	/// <returns>A string representing a color or null.</returns>
	public string? NotificationsBrushColor
	{
		get
		{
			if (Node is null)
			{
				return null;
			}
			else
			{
				if (Node.EntityState.HasFlag(EntityState.HasErrors))
				{
					return (NoResString)"Red";
				}
				else if (Node.EntityState.HasFlag(EntityState.HasWarnings))
				{
					return (NoResString)"Orange";
				}
				else if (Node.EntityState.HasFlag(EntityState.HasMessages))
				{
					return "DodgerBlue";
				}
				else
				{
					return null;
				}
			}
		}
	}
}
