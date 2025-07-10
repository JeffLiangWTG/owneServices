using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.CustomValues
{
	public class DateTimeFormatRule : ICustomAddOnRule
	{
		public DateTimeFormatRule()
		{
			Format = KDateTimeFormat.Short;
		}

		public Action<ZPropertyInfo> GetValidator()
		{
			return null;
		}

		public IEnumerable<DynamicMetaData> GetMetaData()
		{
			return new DynamicMetaData[] { DynamicMetaData.DateTimeFormat(Format) };
		}

		public bool CanBeApplied(Type type)
		{
			return typeof(ZDateTime).IsAssignableFrom(type);
		}

		public string Code => CargoWise.Workflow.CustomAddOnRuleTypes.DateTimeFormat;

		public string Name
		{
			get { return Res.GetString("CustomAddOnRule.DateTimeFormat.Name", "Date time format"); }
		}

		public KDateTimeFormat Format { get; set; }
		public bool IsEnabled { get; set; }

		public BusinessObject GetObjectForBinding()
		{
			CodeDescriptionPairList formats = new CodeDescriptionPairList();
			formats.AddPair(nameof(KDateTimeFormat.Short).ToUpper(), Res.GetString("547a38ec-d88c-4cf7-bae5-bc1e3ea63d11", "Date only"));
			formats.AddPair(nameof(KDateTimeFormat.Long).ToUpper(), Res.GetString("474ff95e-485c-4a03-9743-b4106c2b8909", "Date and time"));
			formats.AddPair(nameof(KDateTimeFormat.Time).ToUpper(), Res.GetString("b9aa3a00-376a-4da1-ba33-b22c37c8558f", "Time only"));

			string dateTimeFormatString = null;
			CustomBusinessObject cusObj = new CustomBusinessObject(null, new CustomPropertyCollectionImpl(
			propertyName =>
			{
				if (propertyName == "DateTimeFormat")
				{
					return dateTimeFormatString ?? Format.ToString().ToUpper();
				}
				else
				{
					return null;
				}
			},
				(propertyName, value) =>
				{
					if (propertyName == "DateTimeFormat")
					{
						dateTimeFormatString = (ZString)value;
						try
						{
							Format = (KDateTimeFormat)Enum.Parse(typeof(KDateTimeFormat), (ZString)value, true);
							dateTimeFormatString = null;
						}
						catch (ArgumentException) { }
						return true;
					}
					return false;
				})
			{
				{ typeof(ZString), "DateTimeFormat", info => { MandatoryValidation.CheckEntered(info); ListValidation.ErrorIfInvalidCode(info); }, DynamicMetaData.ListDataSource(formats) },
			});

			return cusObj;
		}

		public OnSet GetOnSetBehaviour() => null;

		public bool IsUpperCase => false;
	}
}
