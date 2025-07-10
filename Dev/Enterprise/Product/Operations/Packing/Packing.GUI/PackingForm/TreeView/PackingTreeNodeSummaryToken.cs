using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Packing.GUI
{
	public class PackingTreeNodeSummaryToken : ICustomizableColumn
	{
		public delegate ZString GetNodeTextForPackage(IPackageSummary packageSummary);

		public PackingTreeNodeSummaryToken(ZString columnName, ZString columnCaption, byte columnPosition, GetNodeTextForPackage getTextDelegate, GetNodeTextForPackage getCaptionDelegate = null)
		{
			Argument.NotNull(getTextDelegate, "getTextDelegate");

			ColumnName = columnName;
			ColumnCaption = columnCaption;
			ColumnPosition = columnPosition;
			GetTextDelegate = getTextDelegate;
			GetCaptionDelegate = getCaptionDelegate;
		}

		public override string ToString()
		{
			return ColumnCaption;
		}

		public string ColumnName
		{
			get;
			private set;
		}

		public byte ColumnPosition { get; set; }
		public bool IsMandatory { get { return false; } }
		public bool IsVisible { get; set; }
		public bool IsCustomColumn { get; set; }
		readonly ZString ColumnCaption;

		#region GetCaption / GetText

		public ZString GetCaption(IPackageSummary packageSummaries)
		{
			return (GetCaptionDelegate != null) ? GetCaptionDelegate(packageSummaries) : null;
		}

		public ZString GetText(IPackageSummary packageSummaries)
		{
			return GetTextDelegate(packageSummaries);
		}

		readonly GetNodeTextForPackage GetCaptionDelegate;
		readonly GetNodeTextForPackage GetTextDelegate;

		#endregion
	}
}
