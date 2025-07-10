using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Rating.DataTransfer.Calculators.Testing
{
	public class CalculatorGeneratorFactoryTest : TestCaseWithFactory
	{
		public void TestGetCalculatorGenerator()
		{
			AssertEquals("AGY", typeof(AGYCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator(new Xsd.AGYCalculator()).GetType());
			AssertEquals("CTG", typeof(CTGCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator(new Xsd.CTGCalculator()).GetType());
			AssertEquals("CTZ", typeof(CTZCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator(new Xsd.CTZCalculator()).GetType());
			AssertEquals("FPA", typeof(FPACalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator(new Xsd.FPACalculator()).GetType());
			AssertEquals("FLT", typeof(FLTCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator(new Xsd.FLTCalculator()).GetType());
			AssertEquals("FPU", typeof(FPUCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator(new Xsd.FPUCalculator()).GetType());
			AssertEquals("HRC", typeof(HRCCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator(new Xsd.HRCCalculator()).GetType());
			AssertEquals("IAT", typeof(IATCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator(new Xsd.IATCalculator()).GetType());
			AssertEquals("MPU", typeof(MPUCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator(new Xsd.MPUCalculator()).GetType());
			AssertEquals("PER", typeof(PERCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator(new Xsd.PERCalculator()).GetType());
			AssertEquals("PEB", typeof(PEBCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator(new Xsd.PEBCalculator()).GetType());
			AssertEquals("PRS", typeof(PSRCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator(new Xsd.PSRCalculator()).GetType());
			AssertEquals("UNT", typeof(UNTCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator(new Xsd.UNTCalculator()).GetType());
			AssertEquals("CMB", typeof(CMBCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator(new Xsd.CMBCalculator()).GetType());
			AssertEquals("IXC", typeof(IXCCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator(new Xsd.IXCCalculator()).GetType());
			AssertEquals("FRT", typeof(FRTCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator(new Xsd.FRTCalculator()).GetType());
			AssertEquals("CST", typeof(CSTCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator(new Xsd.CSTCalculator()).GetType());
			AssertEquals("CTB", typeof(CTBCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator(new Xsd.CTBCalculator()).GetType());
			AssertEquals("DIN", typeof(DINCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator(new Xsd.DINCalculator()).GetType());
			AssertEquals("TME", typeof(TMECalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator(new Xsd.TMECalculator()).GetType());
			AssertEquals("NTE", typeof(NTECalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator(new Xsd.NTECalculator()).GetType());
			AssertEquals("HRT", typeof(HRTCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator(new Xsd.HRTCalculator()).GetType());
			AssertEquals("EQH", typeof(EQHCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator(new Xsd.EQHCalculator()).GetType());
			AssertEquals("VED", typeof(VEDCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator(new Xsd.VEDCalculator()).GetType());
			AssertEquals("WPK", typeof(WPKCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator(new Xsd.WPKCalculator()).GetType());
			AssertEquals("WLT", typeof(WLTCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator(new Xsd.WLTCalculator()).GetType());
			AssertEquals("MIN", typeof(MINCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator(new Xsd.MINCalculator()).GetType());
			AssertEquals("SMB", typeof(SMBCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator(new Xsd.SMBCalculator()).GetType());
			AssertEquals("EXL", typeof(EXLCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator(new Xsd.EXLCalculator()).GetType());
			AssertEquals("HCC", typeof(HCCCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator(new Xsd.HCCCalculator()).GetType());

			AssertEquals("AGY", typeof(AGYCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator("AGY").GetType());
			AssertEquals("CTG", typeof(CTGCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator("CTG").GetType());
			AssertEquals("CTZ", typeof(CTZCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator("CTZ").GetType());
			AssertEquals("FPA", typeof(FPACalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator("FPA").GetType());
			AssertEquals("FLT", typeof(FLTCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator("FLT").GetType());
			AssertEquals("FPU", typeof(FPUCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator("FPU").GetType());
			AssertEquals("HRC", typeof(HRCCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator("HRC").GetType());
			AssertEquals("IAT", typeof(IATCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator("IAT").GetType());
			AssertEquals("MPU", typeof(MPUCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator("MPU").GetType());
			AssertEquals("PER", typeof(PERCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator("PER").GetType());
			AssertEquals("PEB", typeof(PEBCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator("PEB").GetType());
			AssertEquals("PRS", typeof(PSRCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator("PSR").GetType());
			AssertEquals("UNT", typeof(UNTCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator("UNT").GetType());
			AssertEquals("CMB", typeof(CMBCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator("CMB").GetType());
			AssertEquals("IXC", typeof(IXCCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator("IXC").GetType());
			AssertEquals("FRT", typeof(FRTCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator("FRT").GetType());
			AssertEquals("CST", typeof(CSTCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator("CST").GetType());
			AssertEquals("CTB", typeof(CTBCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator("CTB").GetType());
			AssertEquals("DIN", typeof(DINCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator("DIN").GetType());
			AssertEquals("TME", typeof(TMECalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator("TME").GetType());
			AssertEquals("NTE", typeof(NTECalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator("NTE").GetType());
			AssertEquals("HRT", typeof(HRTCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator("HRT").GetType());
			AssertEquals("EQH", typeof(EQHCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator("EQH").GetType());
			AssertEquals("VED", typeof(VEDCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator("VED").GetType());
			AssertEquals("WPK", typeof(WPKCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator("WPK").GetType());
			AssertEquals("WLT", typeof(WLTCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator("WLT").GetType());
			AssertEquals("MIN", typeof(MINCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator("MIN").GetType());
			AssertEquals("SMB", typeof(SMBCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator("SMB").GetType());
			AssertEquals("EXL", typeof(EXLCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator("EXL").GetType());
			AssertEquals("HCC", typeof(HCCCalculatorGeneratorFromXSD), CalculatorGeneratorFactory.Instance.GetCalculatorGenerator("HCC").GetType());
		}
	}

	public class CalculatorXSDTest : TestCaseWithFactory
	{
		public void TestCalculatorHasCalculatorXSD()
		{
			SubClassRetriever calculatorRetriever = new SubClassRetriever(typeof(Calculator).Assembly, typeof(Calculator));

			foreach (Type calculatorSubClass in calculatorRetriever.Retrieve())
			{
				if (!calculatorSubClass.IsAbstract
					&& calculatorSubClass != typeof(NullCalculator)
					&& !calculatorSubClass.GetCustomAttributes(typeof(TestOnlyCalculatorAttribute), true).Any())
				{
					AssertEquals(calculatorSubClass.Name + " must have an IRateCalculatorGenerator", true, HasCalculatorXsdGenerator(calculatorSubClass));
				}
			}
		}

		bool HasCalculatorXsdGenerator(Type calculatorType)
		{
			SubClassRetriever calculatorGeneratorRetriever = new SubClassRetriever(typeof(IRateCalculatorGenerator).Assembly, typeof(RateCalculatorGeneratorFromXSDBase<,>));
			calculatorGeneratorRetriever.IncludeAbstractClasses = false;

			foreach (Type calculatorXsdGeneratorSubClass in calculatorGeneratorRetriever.Retrieve())
			{
				MethodInfo methodInfo = calculatorXsdGeneratorSubClass.GetMethod("ImportFromValueObjectCore", BindingFlags.NonPublic | BindingFlags.Instance);
				ParameterInfo[] parameterInfos = methodInfo.GetParameters();
				if ((parameterInfos.Length > 2) && (parameterInfos[1].ParameterType == calculatorType))
				{
					return true;
				}
			}
			return false;
		}
	}
}
