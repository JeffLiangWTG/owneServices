// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Windows.Forms;

/// <summary>
///  Provides an implementation for an object that can be inspected by an
///  accessibility application.
/// </summary>
public partial class AccessibleObject
{
	public AccessibleObject()
	{
	}

	/// <summary>
	///  Raises the LiveRegionChanged UIA event.
	///  This method must be overridden in derived classes that support the UIA live region feature.
	/// </summary>
	/// <returns>True if operation succeeds, False otherwise.</returns>
	public virtual bool RaiseLiveRegionChanged()
	{
		throw new NotSupportedException();
	}
}
