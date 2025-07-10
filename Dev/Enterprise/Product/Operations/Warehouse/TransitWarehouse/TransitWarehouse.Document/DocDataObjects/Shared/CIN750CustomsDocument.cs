using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Warehouse.Transit.Document.DocDataObjects
{
	public class CIN750CustomsDocument : DocDataObject
	{
		#region RefType

		public ZString RefType
		{
			get => refType;
			set
			{
				if (SetNonPersistentPropertyValue(RefTypeInfo, ref refType, value) || value.IsEmpty)
				{
					Validate(RefTypeInfo);
					Validate(RefCodeInfo);
				}
			}
		}

		ZString refType;

		public ZPropertyInfo RefTypeInfo => GetZPropertyInfo(nameof(RefType));

		#endregion

		#region RefCode

		public ZString RefCode
		{
			get => refCode;
			set
			{
				if (SetNonPersistentPropertyValue(RefCodeInfo, ref refCode, value) || value.IsEmpty)
				{
					Validate(RefTypeInfo);
					Validate(RefCodeInfo);
				}
			}
		}
		ZString refCode;

		public ZPropertyInfo RefCodeInfo => GetZPropertyInfo(nameof(RefCode));

		#endregion
	}
}
