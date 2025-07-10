using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(SundryCharges))]
	internal sealed partial class SundryChargesTest : EnterpriseBusinessObjectTestCase
	{
		public void TestReadOnlyness()
		{
			Sundry.D4_OH_BillToParty = Factory.NewWithValidTestData<OrgHeader>().PK;

			ZPropertyInfo[] readonlyInfos = new ZPropertyInfo[]
			{
				Sundry.D4_JobNumberInfo,
			};

			CombineAssertions(delegate
			{
				foreach (ZPropertyInfo info in readonlyInfos)
				{
					AssertEquals("Before Save: " + info.Name + "Info.ReadOnly", true, info.ReadOnly);
				}
			});
		}

		public void TestSetDefaultValues()
		{
			CodeDescriptionPairList activities = new CodeDescriptionPairList();
			activities.AddPair("AC1", "Activity 1");
			activities.AddPair("AC2", "Activity 2");
			activities.AddPair("AC3", "Activity 3");
			activities.DefaultCode = "AC1";

			CodeDescriptionPairList modes = new CodeDescriptionPairList();
			modes.AddPair("MD1", "Mode 1");
			modes.AddPair("MD2", "Mode 2");
			modes.AddPair("MD3", "Mode 3");
			modes.DefaultCode = "MD2";

			CodeDescriptionPairList types = new CodeDescriptionPairList();
			types.AddPair("TP1", "Type 1");
			types.AddPair("TP2", "Type 2");
			types.AddPair("TP3", "Type 3");
			types.DefaultCode = "TP3";

			Set(AgencyRegistry.Instance.SundryChargeActivities, activities);
			Set(AgencyRegistry.Instance.SundryChargeModes, modes);
			Set(AgencyRegistry.Instance.SundryChargeTypes, types);

			SundryCharges sundry = Factory.New<SundryCharges>();

			CombineAssertions(delegate
			{
				AssertEquals("D4_SundryJobActivity", "AC1", sundry.D4_SundryJobActivity);
				AssertEquals("D4_SundryJobMode", "MD2", sundry.D4_SundryJobMode);
				AssertEquals("D4_SundriesJobType", "TP3", sundry.D4_SundriesJobType);
			});
		}

		#region Implementation

		void Set(CodeDescriptionPairListWithDefaultCodeRegistryItem item, CodeDescriptionPairList list)
		{
			SystemDefinableCodeDescriptionBoolCollection collection = new SystemDefinableCodeDescriptionBoolCollection();

			foreach (ICodeDescription pair in list)
			{
				CodeDescriptionBool cdb = collection.AddNew();
				cdb.Code = pair.Code;
				cdb.Description = (NoResString)pair.Description;

				if (pair.Code == list.DefaultCode)
				{
					cdb.Bool = true;
				}
			}

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		SundryCharges Sundry
		{
			get { return sundry ?? (sundry = Factory.New<SundryCharges>()); }
		}
		SundryCharges sundry;

		#endregion
	}
}
