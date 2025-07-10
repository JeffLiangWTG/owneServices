using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgHeaderForLoopTest : OrgHeader
	{
		public OrgHeaderForLoopTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override OrgAddress CreateTempOrgAddressFromOrgTranslatedAddress(OrgTranslatedAddress translatedAddress, OrgAddress originalAddress, string language)
		{
			if (throwExceptionWhenTranslatedAddressLoopOccurForTest && translatedAddressLoopTimesForTest++ > 10)
			{
				throw new DeveloperNotificationException("Infinite Loop Exception Occur.");
			}

			return base.CreateTempOrgAddressFromOrgTranslatedAddress(translatedAddress, originalAddress, language);
		}

		public override BusinessObjectFactory ReadOnlyFactory { get; } = new FactoryForLoopTest();

		[ThreadStatic]
		internal static bool throwExceptionWhenTranslatedAddressLoopOccurForTest;
		[ThreadStatic]
		internal static int translatedAddressLoopTimesForTest;
	}
}
