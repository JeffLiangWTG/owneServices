using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ModuleConsolCollectionTest : BaseFreightTest
	{
		public void TestMultipleCRNNumbersReturned()
		{
			Consols = new ArrayList();
			CreateConsolsAndCustomsNumbers(new BusinessObjectFactory());

			string[] entryNums = new string[]
			{
				CusEntryNum1,
				CusEntryNum2,
				CusEntryNum3,
				CusEntryNum4,
				CusEntryNum5,
				CusEntryNum6,
				CusEntryNum7,
				CusEntryNum8,
				CusEntryNum9,
				CusEntryNum10
			};

			ZQuery filter = new ZQuery(JobConsolSchema.JK_MasterBillNum, entryNums);

			ForwardingModuleConsolCollection collection = new ForwardingModuleConsolCollection(Factory);
			collection.Load(filter);

			AssertEquals(entryNums.Length, collection.Count);
			foreach (ForwardingModuleConsol consol in collection)
			{
				if (Consols.Contains(consol.PK))
				{
					AssertEquals("Cus Entry Num not loaded", consol.JK_MasterBillNum, consol.JK_CRN);
				}
			}
		}

		#region Implementation

		const string CusEntryNum1 = "1S040241001IFK";
		const string CusEntryNum2 = "1M040231001IKK";
		const string CusEntryNum3 = "1S041551001VIK";
		const string CusEntryNum4 = "1M042051001SDK";
		const string CusEntryNum5 = "1M042291002APK";
		const string CusEntryNum6 = "1M040571002JDK";
		const string CusEntryNum7 = "2M040441001YPK";
		const string CusEntryNum8 = "3B040441001NVK";
		const string CusEntryNum9 = "3B040291001VOK";
		const string CusEntryNum10 = "1S040441001SUK";

		void CreateConsolsAndCustomsNumbers(BusinessObjectFactory factory)
		{
			ForwardingModuleConsol consol1 = factory.New<ForwardingModuleConsol>();
			CreateCAN(consol1, CusEntryNum1);
			ForwardingModuleConsol consol2 = factory.New<ForwardingModuleConsol>();
			CreateCAN(consol2, CusEntryNum2);
			ForwardingModuleConsol consol3 = factory.New<ForwardingModuleConsol>();
			CreateCAN(consol3, CusEntryNum3);
			ForwardingModuleConsol consol4 = factory.New<ForwardingModuleConsol>();
			CreateCAN(consol4, CusEntryNum4);
			ForwardingModuleConsol consol5 = factory.New<ForwardingModuleConsol>();
			CreateCAN(consol5, CusEntryNum5);
			ForwardingModuleConsol consol6 = factory.New<ForwardingModuleConsol>();
			CreateCAN(consol6, CusEntryNum6);
			ForwardingModuleConsol consol7 = factory.New<ForwardingModuleConsol>();
			CreateCAN(consol7, CusEntryNum7);
			ForwardingModuleConsol consol8 = factory.New<ForwardingModuleConsol>();
			CreateCAN(consol8, CusEntryNum8);
			ForwardingModuleConsol consol9 = factory.New<ForwardingModuleConsol>();
			CreateCAN(consol9, CusEntryNum9);
			ForwardingModuleConsol consol10 = factory.New<ForwardingModuleConsol>();
			CreateCAN(consol10, CusEntryNum10);
			factory.Save();
		}

		BusinessObject CreateCAN(ForwardingModuleConsol consol, ZString entryNumber)
		{
			BusinessObject result = consol.CusEntryNums.AddNew();
			result[CusEntryNumSchema.Constants.CE_ParentID] = consol.PK;
			result[CusEntryNumSchema.Constants.CE_ParentTable] = consol.TableName;
			result[CusEntryNumSchema.Constants.CE_RN_NKCountryCode] = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			result[CusEntryNumSchema.Constants.CE_EntryType] = CusEntryNumberTypes.Australia.CRN;
			result[CusEntryNumSchema.Constants.CE_EntryIsSystemGenerated] = true;
			result[CusEntryNumSchema.Constants.CE_EntryNum] = entryNumber;
			consol.JK_MasterBillNum = entryNumber;
			Consols.Add(consol.PK);
			return result;
		}

		ArrayList Consols;

		#endregion

	}
}
