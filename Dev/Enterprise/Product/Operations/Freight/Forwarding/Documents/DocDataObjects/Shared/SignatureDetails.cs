using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public sealed class SignatureDetails : DocDataObject, ISignatureDetails
	{
		public object Signature { get; set; }

		#region Name

		public ZString Name
		{
			get => name;
			set
			{
				if (SetNonPersistentPropertyValue(NameInfo, ref name, value))
				{
					Validate(NameInfo);
				}
			}
		}

		ZString name;

		public ZPropertyInfo NameInfo => GetZPropertyInfo(nameof(Name));

		#endregion

		#region DateTime

		public ZDateTime DateTime
		{
			get => dateTime;
			set
			{
				if (SetNonPersistentPropertyValue(DateTimeInfo, ref dateTime, value))
				{
					Validate(DateTimeInfo);
				}
			}
		}

		ZDateTime dateTime = ZDateTime.Now;

		public ZPropertyInfo DateTimeInfo => GetZPropertyInfo(nameof(DateTime));

		#endregion
	}
}
