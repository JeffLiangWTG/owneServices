using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public sealed class USCZipCode : AutoUSCZipCode
	{
		#region Constructors

		public USCZipCode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		public override bool SupportsNotes
		{
			get { return false; }
		}

		public const string BeginZipCodeSuffix = "00";
		public const string EndZipCodeSuffix = "99";
	}
}
