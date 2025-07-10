using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class Note : DocDataObject, INote
	{
		#region Text

		public ZString Text
		{
			get => text;
			set
			{
				if (SetNonPersistentPropertyValue(TextInfo, ref text, value))
				{
				}
			}
		}

		ZString text;

		public ZPropertyInfo TextInfo => GetZPropertyInfo(nameof(Text));

		#endregion

		#region Description

		public ZString Description
		{
			get => description;
			set
			{
				if (SetNonPersistentPropertyValue(DescriptionInfo, ref description, value))
				{
				}
			}
		}

		ZString description;

		public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(nameof(Description));

		#endregion
	}
}
