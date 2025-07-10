using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Rating.DataTransfer.Testing
{
	public static class RateTestHelper
	{
		public static ZString MapChargeCode(Guid chargeCodePK, string mapCode, OrgHeader proxy, BusinessObjectFactory factory)
		{
			LoadProxyIfIsNull(ref proxy, factory);
			var freightCharge = factory.Load<AccChargeCode>(chargeCodePK);

			OrgPatternMatchOverride map = proxy.CreatePatternMatchOverrideForTest();
			map.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
			map.OO_ForeignCode = mapCode;
			map.OO_LocalCode = freightCharge.AC_Code;

			return freightCharge.AC_Code;
		}

		public static void MapPort(string portCode, string mapCode, OrgHeader proxy, BusinessObjectFactory factory)
		{
			LoadProxyIfIsNull(ref proxy, factory);
			var refUNLOCO = factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, portCode);

			OrgPatternMatchOverride map = proxy.CreatePatternMatchOverrideForTest();
			map.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Port;
			map.OO_ForeignCode = mapCode;
			map.OO_LocalGuid = refUNLOCO.PK;
		}

		public static void MapServiceLevel(string serviceLevelCode, string mapCode, OrgHeader proxy, BusinessObjectFactory factory)
		{
			LoadProxyIfIsNull(ref proxy, factory);
			var refServiceLevel = factory.LoadFromNaturalKey<RefServiceLevel>(RefServiceLevelSchema.RS_Code, serviceLevelCode);

			OrgPatternMatchOverride map = proxy.CreatePatternMatchOverrideForTest();
			map.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ServiceLevel;
			map.OO_ForeignCode = mapCode;
			map.OO_LocalGuid = refServiceLevel.PK;
		}

		public static void MapCurrency(string currencyCode, string mapCode, OrgHeader proxy, BusinessObjectFactory factory)
		{
			LoadProxyIfIsNull(ref proxy, factory);
			var refCurrency = factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currencyCode);

			OrgPatternMatchOverride map = proxy.CreatePatternMatchOverrideForTest();
			map.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Currency;
			map.OO_ForeignCode = mapCode;
			map.OO_LocalGuid = refCurrency.PK;
		}

		public static ValueObjectImportContext GetContext(BusinessObjectFactory factory)
		{
			var notification = new NotificationBuffer();
			return new ValueObjectImportContext(factory, notification);
		}

		public static AccChargeCode FindChargeCode(ZString xsdValue, BusinessObjectFactory factory)
		{
			var context = new Mock<IValueObjectImportContext>();
			var converter = new Mock<IStringToBusinessObjectFieldConverter>();
			converter.Setup(x => x.GetPKFromNKGivenFKType(factory, xsdValue, ForeignKeyType.ChargeCodeNK, context.Object)).Returns(ZGuid.Empty);
			context.Setup(x => x.Converter).Returns(converter.Object);
			context.Setup(x => x.Factory).Returns(factory);

			return RateImportHelper.Instance.FindChargeCode(xsdValue, context.Object);
		}

		static void LoadProxyIfIsNull(ref OrgHeader proxy, BusinessObjectFactory factory)
		{
			if (proxy == null)
			{
				proxy = factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			}
		}
	}
}
