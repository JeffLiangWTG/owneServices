using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.AU.CMR
{
	public class ReferenceFile
	{
		#region Constants

		public abstract class Constants
		{
			public const string ChangeFileName = "change.tar.gz";
			public const string MainFileName = "main.tar.gz";
			public const string TestingChangeFileName = "change-testing.tar.gz";
			public const string TestingMainFileName = "main-testing.tar.gz";

			public const string TimestampFormat = "yyyy-MM-dd";
		}

		#endregion

		public ReferenceFile(ZString name, ZBlob data)
		{
			this.fName = name;
			this.fData = data;
		}

		public ZString Name
		{
			get { return fName; }
		}
		readonly ZString fName;

		public ZBlob Data
		{
			get { return fData; }
		}
		readonly ZBlob fData;
	}
}
