// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

#region Using directives

using System.Collections.Specialized;

#endregion

namespace System.Windows.Forms;

// this renderer supports high contrast for ToolStripProfessional and ToolStripSystemRenderer.
internal class ToolStripHighContrastRenderer : ToolStripSystemRenderer
{
	BitVector32 options;
	static readonly int optionsDottedBorder = BitVector32.CreateMask();
	static readonly int optionsDottedGrip = BitVector32.CreateMask(optionsDottedBorder);
	static readonly int optionsFillWhenSelected = BitVector32.CreateMask(optionsDottedGrip);

	public ToolStripHighContrastRenderer(bool systemRenderMode)
	{
		options[optionsDottedBorder | optionsDottedGrip | optionsFillWhenSelected] = !systemRenderMode;
	}
}
