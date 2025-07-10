using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobRequiredDocAttrib))]
	sealed class JobRequiredDocAttribTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIsBondedID()
		{
			var attrib = Factory.New<JobRequiredDocAttrib>();
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.BondedID;
			AssertEquals(true, attrib.IsBondedID);
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.PortOfEntry;
			AssertEquals(false, attrib.IsBondedID);
			attrib.D0_AttribName = ZString.Empty;
			AssertEquals(false, attrib.IsBondedID);
		}

		public void TestAttribValueFieldType()
		{
			var document = Factory.NewWithValidTestData<JobRequiredDocument>();
			var attrib = document.Attributes.AddNew();
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.Direction;
			AssertEquals(nameof(FieldType.TextDropEdit), attrib.AttribValueFieldType);

			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.PortOfEntry;
			AssertEquals(nameof(FieldType.Text), attrib.AttribValueFieldType);

			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.DocumentReceivedDate;
			AssertEquals(nameof(FieldType.DateTime), attrib.AttribValueFieldType);

			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.BuyerIssueDate;
			AssertEquals(nameof(FieldType.DateTime), attrib.AttribValueFieldType);

			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CompanyCode;
			AssertEquals(nameof(FieldType.Text), attrib.AttribValueFieldType);

			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.GovernmentAuthorisationReference;
			AssertEquals(nameof(FieldType.Text), attrib.AttribValueFieldType);

			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.SellerControlNumber;
			AssertEquals(nameof(FieldType.Text), attrib.AttribValueFieldType);

			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CeilingLimit;
			AssertEquals(nameof(FieldType.Decimal), attrib.AttribValueFieldType);

			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CostaRicaEXVDocumentType;
			AssertEquals(nameof(FieldType.TextDropEdit), attrib.AttribValueFieldType);

			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.IssuingAuthorityName;
			AssertEquals(nameof(FieldType.Text), attrib.AttribValueFieldType);

			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CustomsDistrict;
			AssertEquals(nameof(FieldType.TextDropEdit), attrib.AttribValueFieldType);

			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.BoxNumber;
			AssertEquals(nameof(FieldType.TextDropEdit), attrib.AttribValueFieldType);

			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.BondedID;
			AssertEquals(nameof(FieldType.TextDropEdit), attrib.AttribValueFieldType);

			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.TradePreferenceCode;
			AssertEquals(nameof(FieldType.Text), attrib.AttribValueFieldType);

			document.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Canada;
			AssertEquals(nameof(FieldType.TextDropEdit), attrib.AttribValueFieldType);

			using (WorkflowDataRegistry.Instance.EnableWorkflowValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new WorkflowValidationProcessTypeCollection { new WorkflowValidationProcessType { ProcessType = "BRK" } }))
			{
				Factory.ClearCachedValue<JobRequiredDocAttribTypeList>("JobRequiredDocAttribLookups.JobRequiredDocAttribTypeList");
				attrib.D0_AttribName = JobRequiredDocCustomAttribTypeList.Codes.Custom1 + "Test1";
				AssertEquals(nameof(FieldType.TextMacro), attrib.AttribValueFieldType);

				attrib.D0_AttribName = JobRequiredDocCustomAttribTypeList.Codes.Custom2 + "Test2";
				AssertEquals(nameof(FieldType.TextMacro), attrib.AttribValueFieldType);

				attrib.D0_AttribName = JobRequiredDocCustomAttribTypeList.Codes.Custom3 + "Test3";
				AssertEquals(nameof(FieldType.TextMacro), attrib.AttribValueFieldType);

				attrib.D0_AttribName = JobRequiredDocCustomAttribTypeList.Codes.Custom4 + "Test4";
				AssertEquals(nameof(FieldType.TextMacro), attrib.AttribValueFieldType);

				attrib.D0_AttribName = JobRequiredDocCustomAttribTypeList.Codes.Custom5 + "Test5";
				AssertEquals(nameof(FieldType.TextMacro), attrib.AttribValueFieldType);

				attrib.D0_AttribName = JobRequiredDocCustomAttribTypeList.Codes.Custom6 + "Test6";
				AssertEquals(nameof(FieldType.TextMacro), attrib.AttribValueFieldType);

				attrib.D0_AttribName = JobRequiredDocCustomAttribTypeList.Codes.Custom7 + "Test7";
				AssertEquals(nameof(FieldType.TextMacro), attrib.AttribValueFieldType);

				attrib.D0_AttribName = JobRequiredDocCustomAttribTypeList.Codes.Custom8 + "Test8";
				AssertEquals(nameof(FieldType.TextMacro), attrib.AttribValueFieldType);

				attrib.D0_AttribName = JobRequiredDocCustomAttribTypeList.Codes.Custom9 + "Test9";
				AssertEquals(nameof(FieldType.TextMacro), attrib.AttribValueFieldType);
			}
		}

		public void TestIsCustomAttributeWhenNotEnableWorkflowValidation()
		{
			var document = Factory.NewWithValidTestData<JobRequiredDocument>();
			var attrib = document.Attributes.AddNew();

			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CustomsDistrict;
			Assert(!attrib.IsCustomAttribute);

			attrib.D0_AttribName = JobRequiredDocCustomAttribTypeList.Codes.Custom1;
			Assert(!attrib.IsCustomAttribute);

			attrib.D0_AttribName = JobRequiredDocCustomAttribTypeList.Codes.Custom1 + "Test";
			Assert(!attrib.IsCustomAttribute);
		}

		public void TestIsCustomAttributeWhenEnableWorkflowValidation()
		{
			using (WorkflowDataRegistry.Instance.EnableWorkflowValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new WorkflowValidationProcessTypeCollection { new WorkflowValidationProcessType { ProcessType = "BRK" } }))
			{
				var document = Factory.NewWithValidTestData<JobRequiredDocument>();
				var attrib = document.Attributes.AddNew();

				attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CustomsDistrict;
				Assert(!attrib.IsCustomAttribute);

				attrib.D0_AttribName = JobRequiredDocCustomAttribTypeList.Codes.Custom1;
				Assert(attrib.IsCustomAttribute);

				attrib.D0_AttribName = JobRequiredDocCustomAttribTypeList.Codes.Custom1 + "Test";
				Assert(attrib.IsCustomAttribute);
			}
		}

		public void TestIsPortOfEntry()
		{
			JobRequiredDocAttrib attrib = Factory.New<JobRequiredDocAttrib>();
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.Direction;
			AssertEquals(false, attrib.IsPortOfEntry);
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.PortOfEntry;
			AssertEquals(true, attrib.IsPortOfEntry);
			attrib.D0_AttribName = ZString.Empty;
			AssertEquals(false, attrib.IsPortOfEntry);
		}

		public void TestIsDirection()
		{
			JobRequiredDocAttrib attrib = Factory.New<JobRequiredDocAttrib>();
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.Direction;
			AssertEquals(true, attrib.IsDirection);
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.PortOfEntry;
			AssertEquals(false, attrib.IsDirection);
			attrib.D0_AttribName = ZString.Empty;
			AssertEquals(false, attrib.IsDirection);
		}

		public void TestIsCompanyCode()
		{
			JobRequiredDocAttrib attrib = Factory.New<JobRequiredDocAttrib>();
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CompanyCode;
			AssertEquals(true, attrib.IsCompanyCode);
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.PortOfEntry;
			AssertEquals(false, attrib.IsCompanyCode);
			attrib.D0_AttribName = ZString.Empty;
			AssertEquals(false, attrib.IsCompanyCode);
		}

		public void TestIsDocumentReceivedDate()
		{
			JobRequiredDocAttrib attrib = Factory.New<JobRequiredDocAttrib>();
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.DocumentReceivedDate;
			AssertEquals(true, attrib.IsDocumentReceivedDate);
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.PortOfEntry;
			AssertEquals(false, attrib.IsDocumentReceivedDate);
			attrib.D0_AttribName = ZString.Empty;
			AssertEquals(false, attrib.IsDocumentReceivedDate);
		}

		public void TestIsBuyerIssueDate()
		{
			JobRequiredDocAttrib attrib = Factory.New<JobRequiredDocAttrib>();
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.BuyerIssueDate;
			AssertEquals(true, attrib.IsBuyerIssueDate);
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.PortOfEntry;
			AssertEquals(false, attrib.IsBuyerIssueDate);
			attrib.D0_AttribName = ZString.Empty;
			AssertEquals(false, attrib.IsBuyerIssueDate);
		}

		public void TestIsSellerControlNumber()
		{
			JobRequiredDocAttrib attrib = Factory.New<JobRequiredDocAttrib>();
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.SellerControlNumber;
			AssertEquals(true, attrib.IsSellerControlNumber);
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.PortOfEntry;
			AssertEquals(false, attrib.IsSellerControlNumber);
			attrib.D0_AttribName = ZString.Empty;
			AssertEquals(false, attrib.IsSellerControlNumber);
		}

		public void TestIsGovernmentAuthorisationReference()
		{
			JobRequiredDocAttrib attrib = Factory.New<JobRequiredDocAttrib>();
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.GovernmentAuthorisationReference;
			AssertEquals(true, attrib.IsGovernmentAuthorisationReference);
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.PortOfEntry;
			AssertEquals(false, attrib.IsGovernmentAuthorisationReference);
			attrib.D0_AttribName = ZString.Empty;
			AssertEquals(false, attrib.IsGovernmentAuthorisationReference);
		}

		public void TestIsCeilingLimit()
		{
			JobRequiredDocAttrib attrib = Factory.New<JobRequiredDocAttrib>();
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CeilingLimit;
			AssertEquals(true, attrib.IsCeilingLimit);
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.PortOfEntry;
			AssertEquals(false, attrib.IsCeilingLimit);
			attrib.D0_AttribName = ZString.Empty;
			AssertEquals(false, attrib.IsCeilingLimit);
		}

		public void TestIsCostaRicaEXVDocumentType()
		{
			JobRequiredDocAttrib attrib = Factory.New<JobRequiredDocAttrib>();
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CostaRicaEXVDocumentType;
			AssertEquals(true, attrib.IsCostaRicaEXVDocumentType);
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.PortOfEntry;
			AssertEquals(false, attrib.IsCostaRicaEXVDocumentType);
			attrib.D0_AttribName = ZString.Empty;
			AssertEquals(false, attrib.IsCostaRicaEXVDocumentType);
		}

		public void TestIsIssuingAuthorityName()
		{
			JobRequiredDocAttrib attrib = Factory.New<JobRequiredDocAttrib>();
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.IssuingAuthorityName;
			AssertEquals(true, attrib.IsIssuingAuthorityName);
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.PortOfEntry;
			AssertEquals(false, attrib.IsIssuingAuthorityName);
			attrib.D0_AttribName = ZString.Empty;
			AssertEquals(false, attrib.IsIssuingAuthorityName);
		}

		public void TestIsBoxNumber()
		{
			JobRequiredDocAttrib attrib = Factory.New<JobRequiredDocAttrib>();
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.BoxNumber;
			AssertEquals(true, attrib.IsBoxNumber);
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.PortOfEntry;
			AssertEquals(false, attrib.IsBoxNumber);
			attrib.D0_AttribName = ZString.Empty;
			AssertEquals(false, attrib.IsBoxNumber);
		}

		public void TestBoxNumber()
		{
			var customsDistrictDocAttrib = Factory.New<JobRequiredDocAttribForTestBoxNumber>();
			customsDistrictDocAttrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CustomsDistrict;
			var boxNumberDocAttrib = Factory.New<JobRequiredDocAttribForTestBoxNumber>();
			boxNumberDocAttrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.BoxNumber;

			var jobRequiredDocumentForTestBoxNumber = Factory.New<JobRequiredDocumentForTestBoxNumber>();
			jobRequiredDocumentForTestBoxNumber.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Taiwan;
			jobRequiredDocumentForTestBoxNumber.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			jobRequiredDocumentForTestBoxNumber.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			jobRequiredDocumentForTestBoxNumber.Attributes.DeleteAll();

			customsDistrictDocAttrib.D0_EQ = jobRequiredDocumentForTestBoxNumber.PK;
			boxNumberDocAttrib.D0_EQ = jobRequiredDocumentForTestBoxNumber.PK;

			customsDistrictDocAttrib.D0_AttribDisplayValue = "A";
			AssertEquals("111", boxNumberDocAttrib.D0_AttribDisplayValue);
			customsDistrictDocAttrib.D0_AttribDisplayValue = "B";
			AssertEquals("333", boxNumberDocAttrib.D0_AttribDisplayValue);
			customsDistrictDocAttrib.D0_AttribDisplayValue = "C";
			AssertEquals("", boxNumberDocAttrib.D0_AttribDisplayValue);
		}

		public void TestReadOnly()
		{
			JobRequiredDocument document = Factory.NewWithValidTestData<JobRequiredDocument>();
			JobRequiredDocAttrib attrib = document.Attributes.AddNew();
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CompanyCode;
			AssertEquals(false, attrib.ReadOnly);
			Factory.Save();
			AssertEquals(false, attrib.ReadOnly);

			attrib = document.Attributes.AddNew();
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.SellerControlNumber;
			AssertEquals(false, attrib.ReadOnly);
			document.EQ_DocType = Core.Constants.RefDocTypes.VATExporterExemption;
			AssertEquals(false, attrib.ReadOnly);
			document.EQ_DocUsage = JobRequiredDocument.DocUsage.Debtor;
			AssertEquals(false, attrib.ReadOnly);
			Factory.Save();
			AssertEquals(true, attrib.ReadOnly);
		}

		public void TestD0_AttribDisplayValue()
		{
			var currCompany = GlbCompany.CurrentCompany;
			var attribTypeList = new JobRequiredDocAttribTypeList();
			foreach (CodeDescriptionPair item in attribTypeList)
			{
				var document = Factory.NewWithValidTestData<JobRequiredDocument>();
				var attrib = document.Attributes.AddNew();
				attrib.D0_AttribName = item.Code;
				switch (attrib.D0_AttribName)
				{
					case JobRequiredDocAttribTypeList.Codes.DocumentReceivedDate:
						{
							attrib.D0_AttribValue = "27/12/2018";
							AssertEquals("27/12/2018", attrib.D0_AttribDisplayValue);
							attrib.D0_AttribDisplayValue = "27/12/2018";
							AssertEquals("27 Dec 2018 00:00", attrib.D0_AttribValue);
							break;
						}
					case JobRequiredDocAttribTypeList.Codes.CompanyCode:
						{
							attrib.D0_AttribValue = currCompany.PK.ToString();
							AssertEquals(currCompany.GC_Code, attrib.D0_AttribDisplayValue);
							attrib.D0_AttribDisplayValue = GlbCompany.DemoCompanyCode;
							AssertEquals(GlbCompany.GetDemoCompany(Factory).PK.ToString(), attrib.D0_AttribValue);
							break;
						}
					case JobRequiredDocAttribTypeList.Codes.CeilingLimit:
						{
							attrib.D0_AttribValue = "1234.56";
							AssertEquals("1,234.56", attrib.D0_AttribDisplayValue);
							attrib.D0_AttribDisplayValue = "9876.54";
							AssertEquals("9876.54", attrib.D0_AttribValue);

							using (currCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
							{
								attrib.D0_AttribValue = "1234.56";
								AssertEquals("1.234,56", attrib.D0_AttribDisplayValue);
								attrib.D0_AttribDisplayValue = "9876,54";
								AssertEquals("9876.54", attrib.D0_AttribValue);
							}
							break;
						}
					default:
						{
							attrib.D0_AttribValue = "some string value";
							AssertEquals("some string value", attrib.D0_AttribDisplayValue);
							attrib.D0_AttribDisplayValue = "translated value";
							AssertEquals("translated value", attrib.D0_AttribValue);
							break;
						}
				}
			}
		}

		public void TestD0_AttribDisplayValueInvalidDate()
		{
			JobRequiredDocument document = Factory.NewWithValidTestData<JobRequiredDocument>();
			JobRequiredDocAttrib attrib = document.Attributes.AddNew();
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.DocumentReceivedDate;
			attrib.D0_AttribDisplayValue = "12/27/2018";
			AssertEquals("<Invalid>", attrib.D0_AttribValue);
			AssertEquals("<Invalid>", attrib.D0_AttribDisplayValue);
		}

		public void TestIsCustomsDistrict()
		{
			JobRequiredDocAttrib attrib = Factory.New<JobRequiredDocAttrib>();
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CustomsDistrict;
			Assert(attrib.IsCustomsDistrict);
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.PortOfEntry;
			Assert(!attrib.IsCustomsDistrict);
			attrib.D0_AttribName = ZString.Empty;
			Assert(!attrib.IsCustomsDistrict);
		}

		public void TestIsTradePreferenceCode()
		{
			JobRequiredDocAttrib attrib = Factory.New<JobRequiredDocAttrib>();
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.TradePreferenceCode;
			AssertEquals(true, attrib.IsTradePreferenceCode);
			attrib.D0_AttribName = JobRequiredDocAttribTypeList.Codes.PortOfEntry;
			AssertEquals(false, attrib.IsTradePreferenceCode);
			attrib.D0_AttribName = ZString.Empty;
			AssertEquals(false, attrib.IsTradePreferenceCode);
		}

		public void TestD0_AttribDisplayValueRootTypeProviderAttribute()
		{
			var newOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			var jobRequiredDocument = Factory.New<JobRequiredDocument>();
			newOrganisation.RequiredDocuments.Add(jobRequiredDocument);
			var attrib = Factory.New<JobRequiredDocAttrib>();
			attrib.D0_AttribName = JobRequiredDocCustomAttribTypeList.Codes.Custom1 + "Transport";
			jobRequiredDocument.Attributes.Add(attrib);

			var currentType = attrib.GetType();
			var rootTypeAttribute = currentType.GetProperty(nameof(attrib.D0_AttribDisplayValue))
					.GetCustomAttributes(typeof(RootTypeProviderAttribute), true)
					.Cast<RootTypeProviderAttribute>()
					.Single();

			var getTypesMethod = currentType.GetMethod(rootTypeAttribute.TypeProviderFunctionName);
			var getRootsMethod = currentType.GetMethod(rootTypeAttribute.RootProviderFunctionName);

			jobRequiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Broker;
			var types = new List<Type>();
			var values = new List<BusinessObject>();
			types.AddRange((Type[])getTypesMethod.Invoke(attrib, Array.Empty<object>()));
			values.AddRange((BusinessObject[])getRootsMethod.Invoke(attrib, Array.Empty<object>()));

			var expectedTypes = new List<Type> { typeof(JobRequiredDocAttrib), typeof(OrgHeader), ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>() };
			AssertContainsExactElementsInAnyOrder(expectedTypes, types);
			AssertEquals(0, values.Count);

			jobRequiredDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Both;
			types = new List<Type>();
			types.AddRange((Type[])getTypesMethod.Invoke(attrib, Array.Empty<object>()));
			values = new List<BusinessObject>();
			values.AddRange((BusinessObject[])getRootsMethod.Invoke(attrib, Array.Empty<object>()));

			expectedTypes = new List<Type> { typeof(JobRequiredDocAttrib), typeof(OrgHeader) };
			AssertContainsExactElementsInAnyOrder(expectedTypes, types);
			AssertEquals(0, values.Count);
		}

		public void TestIsCARelatedCountry()
		{
			var document = Factory.NewWithValidTestData<JobRequiredDocument>();
			var attrib = document.Attributes.AddNew();
			AssertEquals(false, attrib.IsCARelatedCountry);
			document.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Canada;
			AssertEquals(true, attrib.IsCARelatedCountry);
			document.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals(false, attrib.IsCARelatedCountry);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			IDocsAndCartageParent shipment = (IDocsAndCartageParent)factory.New<Forwarding.IForwardingShipment>();
			JobRequiredDocument requiredDocument = shipment.RequiredDocumentsProvider.RequiredDocuments.AddNew();
			return requiredDocument.Attributes.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
		}

		#endregion
	}
}
