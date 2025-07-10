using Blazor.Diagrams.Core.Events;
using Blazor.Diagrams.Core.Geometry;
using Enterprise.ZArchitecture.Core;

namespace NetworkVisualisation.GUI.Winzor.Test.Extensions;

/// <summary>
/// Represents an extension class for mouse/touch pointer to emulate pointer events and properties.
/// Provides helper extension methods to use in blazor diagram unit tests to emulate the behavior of a pointer.
/// </summary>
public static class Pointer
{
	/// <summary>
	/// Represents the left mouse button. Initializes the PointerEventArgs properties for left button.
	/// </summary>
	public static readonly PointerEventArgs LeftButton = CreateArrowPointer(button: 0);

	/// <summary>
	/// Represents the middle mouse button. Initializes the PointerEventArgs properties for middle button.
	/// </summary>
	public static readonly PointerEventArgs MiddleButton = CreateArrowPointer(button: 1);

	/// <summary>
	/// Represents the right mouse button. Initializes the PointerEventArgs properties for right button.
	/// </summary>
	public static readonly PointerEventArgs RightButton = CreateArrowPointer(button: 2);

	/// <summary>
	/// Sets the position of the pointer.
	/// </summary>
	/// <param name="x">X coordinate of the pointer position.</param>
	/// <param name="y">Y coordinate of the pointer position.</param>
	/// <returns>Event arguments containg the information of the pointer event</returns>
	public static PointerEventArgs At(this PointerEventArgs original, double x, double y) => original with
	{
		ClientX = x,
		ClientY = y
	};

	/// <summary>
	/// Sets the position of the pointer.
	/// </summary>
	/// <param name="location">Point denoting the new position of the pointer</param>
	/// <returns>Event arguments containg the information of the pointer event raised</returns>
	public static PointerEventArgs At(this PointerEventArgs original, Point location)
		=> original.At(location.X, location.Y);

	/// <summary>
	/// Moves the pointer to a given distance and sets the position.
	/// </summary>
	/// <param name="x">Distance to move the pointer on the x-axis.</param>
	/// <param name="y">Distance to move the pointer on the y-axis.</param>
	/// <returns>Event arguments containg the information of the pointer event raised</returns>
	public static PointerEventArgs MovedBy(this PointerEventArgs original, double x, double y) => original with
	{
		ClientX = original.ClientX + x,
		ClientY = original.ClientY + y
	};

	/// <summary>
	/// Moves the pointer to a given distance and sets the position.
	/// </summary>
	/// <param name="location">Point denoting the distance to move the pointer.</param>
	/// <returns>Event arguments containg the information of the pointer event raised</returns>
	public static PointerEventArgs MovedBy(this PointerEventArgs original, Point location)
		=> original.At(location.X, location.Y);

	static PointerEventArgs CreateArrowPointer(int button) => new PointerEventArgs(
		ClientX: 0,
		ClientY: 0,
		Button: button,
		Buttons: 0,
		CtrlKey: false,
		ShiftKey: false,
		AltKey: false,
		PointerId: 1,
		Width: 1,
		Height: 1,
		Pressure: 1,
		TiltX: 1,
		TiltY: 1,
		PointerType: (NoResString)"arrow",
		IsPrimary: true
	);
}
