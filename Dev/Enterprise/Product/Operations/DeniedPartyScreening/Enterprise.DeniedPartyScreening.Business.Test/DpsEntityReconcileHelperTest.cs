using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Moq;

namespace Enterprise.DeniedPartyScreening.Business.Testing
{
	public class DpsEntityReconcileHelperTest : TestCaseWithFactory
	{
		public void TestGetReconcileParametersFromFeatureData_WhenFeatureDataIsNull_ShouldReturnDisabledParameters()
		{
			ErrorReporter.Clear();
			var mockFeatureControlManager = new Mock<IFeatureControlManager>();
			mockFeatureControlManager
				.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.DpsEntityReconcileServiceTaskFlag, CancellationToken.None))
				.ReturnsAsync(default(IFeatureData));

			using (ObjectFactory.Substitute(mockFeatureControlManager.Object))
			{
				var parameters = DpsEntityReconcileHelper.GetReconcileParametersFromFeatureDataAsync().GetAwaiter().GetResult();

				AssertNull(parameters.StartDateUtc);
				AssertNull(parameters.EndDateUtc);
				AssertNull(parameters.BatchSize);
				AssertEquals(false, parameters.IsEnabled);
			}

			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestGetReconcileParametersFromFeatureData_WhenValidParametersProvided_ShouldReturnEnabledParameters()
		{
			ErrorReporter.Clear();
			var expectedFeatureParams = new ReconcileParametersFromFeatureControl
			{
				StartDateUtc = new DateTime(2025, 1, 1),
				EndDateUtc = new DateTime(2025, 1, 31),
				BatchSize = 500
			};

			var mockFeatureData = new Mock<IFeatureData>();
			mockFeatureData
				.Setup(x => x.TryDeserializeParameterAsJson(out expectedFeatureParams))
				.Returns(true);

			var mockFeatureControlManager = new Mock<IFeatureControlManager>();
			mockFeatureControlManager
				.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.DpsEntityReconcileServiceTaskFlag, CancellationToken.None))
				.ReturnsAsync(mockFeatureData.Object);

			using (ObjectFactory.Substitute(mockFeatureControlManager.Object))
			{
				var parameters = DpsEntityReconcileHelper.GetReconcileParametersFromFeatureDataAsync().GetAwaiter().GetResult();

				AssertEquals(expectedFeatureParams.StartDateUtc, parameters.StartDateUtc);
				AssertEquals(expectedFeatureParams.EndDateUtc, parameters.EndDateUtc);
				AssertEquals(expectedFeatureParams.BatchSize, parameters.BatchSize);
				AssertEquals(true, parameters.IsEnabled);
			}

			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestGetReconcileParametersFromFeatureData_WhenInvalidParametersProvided_ShouldReportErrorAndReturnDisabledParameters()
		{
			ErrorReporter.Clear();
			var invalidJson = "{ invalid json format }";
			ReconcileParametersFromFeatureControl outParam;

			var mockFeatureData = new Mock<IFeatureData>();
			mockFeatureData
				.Setup(x => x.TryDeserializeParameterAsJson(out outParam))
				.Returns(false);

			mockFeatureData
				.Setup(x => x.Parameter)
				.Returns(invalidJson);

			var mockFeatureControlManager = new Mock<IFeatureControlManager>();
			mockFeatureControlManager
				.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.DpsEntityReconcileServiceTaskFlag, CancellationToken.None))
				.ReturnsAsync(mockFeatureData.Object);

			using (ObjectFactory.Substitute(mockFeatureControlManager.Object))
			{
				var parameters = DpsEntityReconcileHelper.GetReconcileParametersFromFeatureDataAsync().GetAwaiter().GetResult();

				AssertNull(parameters.StartDateUtc);
				AssertNull(parameters.EndDateUtc);
				AssertNull(parameters.BatchSize);
				AssertEquals(false, parameters.IsEnabled);
			}

			AssertEquals($"Feature Control Data has been inputted incorrectly. [[{invalidJson}]]",
				ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestGetEntityDetailsHash_When_OrgHeaderOrRefVesselEntityPassed_Should_ReturnEntityHash()
		{
			var candidateCreator = new DpsCandidateCreator();
			var (orgHeaders, refVessels) = GetOrgAndVesselObjectsForTest(Factory);

			var entityHasher = new DpsEntityDetailsHasher();

			AssertEquals(getExpectedHash(orgHeaders[0], entityHasher), DpsEntityReconcileHelper.GetEntityDetailsHash(orgHeaders[0], entityHasher));
			AssertEquals(getExpectedHash(orgHeaders[1], entityHasher), DpsEntityReconcileHelper.GetEntityDetailsHash(orgHeaders[1], entityHasher));
			AssertEquals(getExpectedHash(orgHeaders[2], entityHasher), DpsEntityReconcileHelper.GetEntityDetailsHash(orgHeaders[2], entityHasher));
			AssertEquals(getExpectedHash(refVessels[0], entityHasher), DpsEntityReconcileHelper.GetEntityDetailsHash(refVessels[0], entityHasher));
		}

		static (IList<OrgHeader>, IList<RefVessel>) GetOrgAndVesselObjectsForTest(BusinessObjectFactory factory)
		{
			var name1 = factory.NewWithValidTestData<OrgBrandOrRelatedName>();
			name1.P1_RelatedName = "Related Org Name 1";
			var name2 = factory.NewWithValidTestData<OrgBrandOrRelatedName>();
			name2.P1_RelatedName = "Related Org Name 2";
			var name3 = factory.NewWithValidTestData<OrgBrandOrRelatedName>();
			name3.P1_RelatedName = "Related Org Name 3";

			var header1 = factory.NewWithValidTestData<OrgHeader>();
			header1.OH_FullName = "Test Org 1";
			header1.BrandsOrRelatedNames.Add(name1);
			header1.BrandsOrRelatedNames.Add(name2);
			header1.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			header1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "1234", "AU");
			header1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "67585", "IN");
			header1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "37448", "US");

			var header2 = factory.NewWithValidTestData<OrgHeader>();
			header2.OH_FullName = "Test Org 2";
			header2.BrandsOrRelatedNames.Add(name3);
			header2.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			header2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "1234", "AU");
			header2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "67585", "US");
			header2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.JNP, "37448", "US");

			var header3 = factory.NewWithValidTestData<OrgHeader>();
			header3.OH_FullName = "Test Org 3";
			header3.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			var refVessel1 = factory.NewWithValidTestData<RefVessel>();
			refVessel1.RV_Code = "Vessel 1";
			refVessel1.RV_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			refVessel1.RV_LloydsNumber = "123";
			refVessel1.RV_RN_NKCountryOfReg = "US";

			return (new[] { header1, header2, header3 }, new[] { refVessel1 });
		}

		static Byte[] getExpectedHash(BusinessObject entity, DpsEntityDetailsHasher entityHasher)
		{
			var candidateCreator = new DpsCandidateCreator();

			var candidate = entity switch
			{
				OrgHeader orgHeader => candidateCreator.NewRequestHeader(orgHeader),
				RefVessel refVessel => candidateCreator.NewRequestHeader(refVessel),
				_ => null
			};

			if (candidate == null)
			{
				return Array.Empty<byte>();
			}

			var entityDetailsForHahshing = new DpsEntityDetailsForHashing
			{
				Names = candidate.DpsNameCandidates.Select(name => name.FullName),
				ScreeningStatus = ((IScreeningPartyProvider)entity).ScreeningStatus,
				RegCodeDetails = candidate.DpsRegistrationCodeCandidates?
					.Where(regCode => new[] { "IMO", "PAS", "DUN" }.Contains(regCode.RegCodeType))
					.Select(regCode => new DpsRegCodeDetails
					{
						RegCodeValue = regCode.RegCodeValue,
						RegCodeType = regCode.RegCodeType,
						RegCountryCode = regCode.RegCountryCode
					}),
			};

			return entityHasher.GetHashAndReset(entityDetailsForHahshing);
		}
	}
}
