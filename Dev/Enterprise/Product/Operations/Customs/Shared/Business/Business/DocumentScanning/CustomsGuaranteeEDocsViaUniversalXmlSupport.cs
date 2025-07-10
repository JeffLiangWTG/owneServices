using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.DocumentScanning
{
	public class CustomsGuaranteeEDocsViaUniversalXmlSupport : IEDocsViaUniversalXmlSupport
	{
		public ZString ExpectedCodeFormat => CodeParts.ExpectedCodeFormat;

		public ZString ExampleCodeFormat => "12345678|GB|HYECMRLON|COD|1|19000101|20790606";

		public BusinessObject LoadBusinessObjectFromCode(BusinessObjectFactory factory, ZString code)
		{
			var codeParts = new CodeParts(code);
			var query = new ZQuery()
				.AddToFilter(CusPermitHeaderSchema.CPH_Number, codeParts.PermitNumber)
				.AddToFilter(CusPermitHeaderSchema.CPH_RN_NKCountryCode, codeParts.CountryCode)
				.AddToFilter(CusPermitHeaderSchema.CPH_OH_PermitHolder, GetOrgHeaderPK(factory, codeParts.PermitHolder))
				.AddToFilter(CusPermitHeaderSchema.CPH_Type, codeParts.PermitType)
				.AddToFilter(CusPermitHeaderSchema.CPH_SubType, codeParts.PermitSubType)
				.AddToFilter(CusPermitHeaderSchema.CPH_StartDate, codeParts.StartDate)
				.AddToFilter(CusPermitHeaderSchema.CPH_EndDate, codeParts.EndDate);
			return factory.Load<BaseCusGuaranteeHeader>(query).FirstOrDefault();
		}

		ZGuid GetOrgHeaderPK(BusinessObjectFactory factory, ZString orgCode)
		{
			var orgHeader = factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, orgCode)
				?? throw new ArgumentException($"No matching permit holder found ({orgCode})");
			return orgHeader.PK;
		}

		class CodeParts
		{
			public CodeParts(ZString code)
			{
				codeParts = code.Split(Separator);
				if (codeParts.Length != 7)
				{
					throw new ArgumentException($"Code should contain \"{ExpectedCodeFormat}\"");
				}
				PopulateValues();
				Validate();
			}

			readonly ZString[] codeParts;

			public ZString PermitNumber { get; private set; }
			public ZString CountryCode { get; private set; }
			public ZString PermitHolder { get; private set; }
			public ZString PermitType { get; private set; }
			public ZString PermitSubType { get; private set; }
			public ZDateTime StartDate { get; private set; }
			public ZDateTime EndDate { get; private set; }

			public const string Separator = "|";

			public static ZString ExpectedCodeFormat
			{
				get
				{
					var properties = new[]
					{
						nameof(PermitNumber),
						nameof(CountryCode),
						nameof(PermitHolder),
						nameof(PermitType),
						nameof(PermitSubType),
						nameof(StartDate),
						nameof(EndDate)
					};
					return string.Join(Separator, properties);
				}
			}

			void PopulateValues()
			{
				PermitNumber = codeParts[0];
				CountryCode = codeParts[1];
				PermitHolder = codeParts[2];
				PermitType = codeParts[3];
				PermitSubType = codeParts[4];
				StartDate = ParseDate(codeParts[5]);
				EndDate = ParseDate(codeParts[6]);
			}

			void Validate()
			{
				var propertiesToValidate = new[]
				{
					new { Name = nameof(PermitNumber), Value = (IZType)PermitNumber },
					new { Name = nameof(CountryCode), Value = (IZType)CountryCode },
					new { Name = nameof(PermitHolder), Value = (IZType)PermitHolder },
					new { Name = nameof(PermitType), Value = (IZType)PermitType },
					new { Name = nameof(StartDate), Value = (IZType)StartDate }
				};
				var emptyProperties = propertiesToValidate.Where(v => v.Value.IsEmpty).Select(v => v.Name).ToArray();
				if (emptyProperties.Length > 0)
				{
					throw new ArgumentException($"Invalid empty values ({string.Join(", ", emptyProperties)})");
				}
				if (!StartDate.IsValid || (!EndDate.IsEmpty && !EndDate.IsValid))
				{
					throw new ArgumentException("Invalid date format (yyyyMMdd)");
				}
			}

			ZDateTime ParseDate(ZString value)
			{
				ZDateTime.TryParseExact(value, out var result, "yyyyMMdd");
				return result;
			}
		}
	}
}
