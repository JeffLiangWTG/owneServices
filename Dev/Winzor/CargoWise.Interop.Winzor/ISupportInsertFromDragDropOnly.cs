using System;
using System.Windows.Forms;

namespace CargoWise.Interop.DataObjects;

/// <summary>
/// Implemented on a ZDataObject implementation to indicate pasting operations are not allowed.
/// </summary>
public interface ISupportInsertFromDragDropOnly : IDataObject
{
}
