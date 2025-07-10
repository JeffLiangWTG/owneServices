using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Definitions.Freight.DangerousGoods.SegregationApi;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation.Rules.IMO
{
	class ClassFilter
	{
		public FilterType FilterType { get; set; }
		public string ClassCode { get; set; }
		Regex _regex;
		public Regex RegexCode
		{
			get
			{
				if (_regex == null)
				{
					_regex = new Regex(ClassCode);
				}
				return _regex;
			}
		}
	}

	enum FilterType
	{
		Exact,
		RegEx
	}
	public class CheckSegregationCodeWithClass : ISegregationRule
	{
		public string ApplicableStandard => UNDGSubstanceStandardTypes.IMO;
		public bool IsExemption => false;

		readonly Dictionary<string, (ClassFilter Filter, string Message, bool IsWarning)> segregationCodeToClassMapping = new Dictionary<string, (ClassFilter Filter, string Message, bool IsWarning)>
		{
			{ "SG7", (new ClassFilter { FilterType = FilterType.Exact, ClassCode = "3" }, Res.GetString("26BEA2BE-321C-4D72-A6D8-C1C04163D635","SG7 : Stow \"away from\" class 3."), false) },
			{ "SG8", (new ClassFilter { FilterType = FilterType.Exact, ClassCode = "4.1" }, Res.GetString("95BAB8C7-F390-4A39-B30B-001D0BD78B47","SG8 : Stow \"away from\" class 4.1."), false) },
			{ "SG9", (new ClassFilter { FilterType = FilterType.Exact, ClassCode = "4.3" }, Res.GetString("150075A3-89B3-4CE1-8E06-983981D7A928","SG9 : Stow \"away from\" class 4.3."), false) },
			{ "SG10", (new ClassFilter { FilterType = FilterType.Exact, ClassCode = "5.1" }, Res.GetString("88A6A6DB-648D-49D9-9380-68B905670E7E","SG10 : Stow \"away from\" class 5.1."), false) },
			{ "SG11", (new ClassFilter { FilterType = FilterType.Exact, ClassCode = "6.2" }, Res.GetString("4C1CB61E-902A-43B1-A54A-D74C447A1A28","SG11 : Stow \"away from\" class 6.2."), false) },
			{ "SG12", (new ClassFilter { FilterType = FilterType.Exact, ClassCode = "7" }, Res.GetString("651FB50D-C936-4104-AE8F-F10318D891BA","SG12 : Stow \"away from\" class 7."), false) },
			{ "SG13", (new ClassFilter { FilterType = FilterType.Exact, ClassCode = "8" }, Res.GetString("F55124D2-9912-4BAB-844A-A469C8C41858","SG13 : Stow \"away from\" class 8."), false) },
			{ "SG15", (new ClassFilter { FilterType = FilterType.Exact, ClassCode = "3" }, Res.GetString("17A01B46-0C2F-4C2F-A174-D5EC7CDDC338","SG15 : Stow \"separated from\" class 3."), false) },
			{ "SG16", (new ClassFilter { FilterType = FilterType.Exact, ClassCode = "4.1" }, Res.GetString("8A413613-2AA9-4FA2-A67A-EA9000E60DD1","SG16 : Stow \"separated from\" class 4.1."), false) },
			{ "SG17", (new ClassFilter { FilterType = FilterType.Exact, ClassCode = "5.1" }, Res.GetString("AA10834E-849F-4FE8-B191-82811026BD22","SG17 : Stow \"separated from\" class 5.1."), false) },
			{ "SG18", (new ClassFilter { FilterType = FilterType.Exact, ClassCode = "6.2" }, Res.GetString("16D2356D-54C8-4C30-A43C-7369479D1C63","SG18 : Stow \"separated from\" class 6.2."), false) },
			{ "SG19", (new ClassFilter { FilterType = FilterType.Exact, ClassCode = "7" }, Res.GetString("F8A9B027-8FEC-4F94-B19D-C72D04280FD9","SG19 : Stow \"separated from\" class 7."), false) },
			{ "SG14", (new ClassFilter { FilterType = FilterType.RegEx, ClassCode = (NoResString)@"^1\.(?!4S)[A-Za-z0-9]*$" }, Res.GetString("C9B2E758-96DE-4697-B0E1-EB5DBC55CFFD","SG14 : Stow \"separated from\" class 1 except for division 1.4S."), false) },
			{ "SG25", (new ClassFilter { FilterType = FilterType.RegEx, ClassCode = (NoResString)@"^(2.1[A-Za-z0-9]*|3)$" }, Res.GetString("C926B255-946F-40BF-9603-3627AFAE835D","SG25 : Stow \"separated from\" goods of classes 2.1 and 3."), false) },
			{ "SG26", (new ClassFilter { FilterType = FilterType.RegEx, ClassCode = (NoResString)@"^(2\.1[A-Za-z0-9]*|3)$" }, Res.GetString("24511BE0-454A-4FC0-B5DD-FD504CCF89B1","SG26 : In addition: from goods of classes 2.1 and 3 when stowed on deck of a container ship a minimum distance of two container spaces athwart ship shall be maintained, when stowed on Ro-Ro ships a distance of 6 m athwart ship shall be maintained."), false) },
			{ "SG48", (new ClassFilter { FilterType = FilterType.RegEx, ClassCode = (NoResString)@"^1\.[A-Za-z0-9]*$|^2\.1[A-Za-z0-9]*$|^3$|^4\.1[A-Za-z0-9]*$|^5\.2[A-Za-z0-9]*$" }, Res.GetString("A9E77BE2-72E7-4CE7-9C24-B13E46623AB6","SG48 : Stow \"separated from\" combustible material (particularly liquids)."), true) },
			{ "SG53", (new ClassFilter { FilterType = FilterType.RegEx, ClassCode = (NoResString)@"^1\.[A-Za-z0-9]*$|^2\.1[A-Za-z0-9]*$|^3$|^4\.1[A-Za-z0-9]*$|^5\.2[A-Za-z0-9]*$" }, Res.GetString("97B92518-281A-4108-83A1-3A1C0A5B89D7","SG53 : Shall not be stowed together with combustible material in the same cargo transport unit."), false) },
			{ "SG63", (new ClassFilter { FilterType = FilterType.RegEx, ClassCode = (NoResString)@"^1\.[A-Za-z0-9]*$" }, Res.GetString("9F57CBD6-7366-40D5-A7F8-D5E654FFE744","SG63 : Stow \"separated longitudinally by an intervening complete compartment or hold\" from class 1."), false) },
			{ "SG65", (new ClassFilter { FilterType = FilterType.RegEx, ClassCode = (NoResString)@"^1\.(?!4)[A-Za-z0-9]*$" }, Res.GetString("B7EFA513-7554-42FB-90B4-4E14F7E33974","SG65 : Stow \"separated by a complete compartment or hold\" from class 1 except for division 1.4."), false) },
			{ "SG67", (new ClassFilter { FilterType = FilterType.RegEx, ClassCode = (NoResString)@"^1\.(1|2|3|4)[A-Za-z0-9]*$" }, Res.GetString("F8CDEDFD-F7DE-44E3-A13C-5C3B69385A99","SG67 : Stow \"separated from division 1.4 and \"separated longitudinally by an intervening complete compartment or hold from\" divisions 1.1, 1.2 and 1.3."), false) },
			{ "SG78", (new ClassFilter { FilterType = FilterType.RegEx, ClassCode = (NoResString)@"^1\.(1|2|5)[A-Za-z0-9]*$" }, Res.GetString("316DC686-CF9E-4FC4-981C-5E921A34F1A0","SG78 : Stow \"separated longitudinally by an intervening complete compartment or hold\" from division 1.1, 1.2 and 1.5."), false) },
		};

		public IEnumerable<Message> Check(UNDGSubstance substance1, UNDGSubstance substance2)
		{
			var listOfMessages = new List<Message>();
			Checker(substance1, substance2);
			Checker(substance2, substance1);

			void Checker(UNDGSubstance substance, UNDGSubstance substanceWithIncompatibleClass)
			{
				foreach (var segCode in substance.SegregationCodes)
				{
					if (segregationCodeToClassMapping.TryGetValue(segCode, out var value))
					{
						var isMatch = value.Filter.FilterType == FilterType.Exact && value.Filter.ClassCode == substanceWithIncompatibleClass.DG_Class ||
							value.Filter.FilterType == FilterType.RegEx && value.Filter.RegexCode.Match(substanceWithIncompatibleClass.DG_Class).Success;

						if (isMatch)
						{
							listOfMessages.Add(new Message(MessageType.Error, value.Message));
						}
						else if (value.IsWarning)
						{
							listOfMessages.Add(new Message(MessageType.Warning, value.Message));
						}
					}
				}
			}

			return listOfMessages;
		}
	}
}
