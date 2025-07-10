using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	[System.CodeDom.Compiler.GeneratedCode("CargoWise.EntityFramework", "1.0"), CodeProperty("ISOCode"), DescriptionProperty("Description"), PreventDelete(true)]
	public class ContainerISOType : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Container Category Enum

		public enum Categories
		{
			GeneralPurpose,
			OpenTop,
			Bolster,
			Refrigerated,
			FlatRack,
			Other
		}

		#endregion

		public ContainerISOType()
		{
			ResetAllInternalValues();
		}

		public ContainerISOType(BusinessObject bizOToRefreshBindingOn)
			: this()
		{
			this.BizOToRefreshBindingOn = bizOToRefreshBindingOn;
		}

		readonly BusinessObject BizOToRefreshBindingOn;

		#region Properties

		#region Details

		public ZString ISOCode
		{
			set
			{
				if (isoCode != value)
				{
					ReGenerateValues(value);
				}
			}
			get { return isoCode; }
		}
		ZString isoCode;

		public ZString Description
		{
			get { return IsKnown ? GroupDescription + (GroupDescription.IsEmpty || DetailedGroupDescription.IsEmpty ? "" : ", ") + DetailedGroupDescription : ""; }
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo("Description"); }
		}

		public ZString Dimensions
		{
			get { return IsKnown ? Res.GetString("a520b5b8-2ddc-448b-89d1-f0bea481bce2", "{0} Long, {1} High", Length, Height) + (Res.GetString("0f36dc34-95d1-41b9-b88a-dcf909fb8f49", ", {0} Wide", Width)) : ""; }
		}

		public ZPropertyInfo DimensionsInfo
		{
			get { return GetZPropertyInfo("Dimensions"); }
		}

		public ZString DimensionsAndDescription
		{
			get { return IsKnown ? "[" + Dimensions + "] : " + Description : ""; }
		}

		public ZPropertyInfo DimensionsAndDescriptionInfo
		{
			get { return GetZPropertyInfo("DimensionsAndDescription"); }
		}

		public ZBool IsKnown { get; private set; }

		public ZBool OverDimensionAllowed { get; private set; }

		public ZPropertyInfo OverDimensionAllowedInfo
		{
			get { return GetZPropertyInfo("OverDimensionAllowed"); }
		}

		public Categories Category { get; private set; }

		#endregion

		#region Length

		public ZString LengthInFeet { get; private set; }
		public ZString LengthInMM { get; private set; }

		public ZString Length
		{
			get { return LengthInFeet.IsEmpty ? LengthInMM : LengthInFeet; }
		}

		#endregion

		#region Height

		public ZString HeightInFeet { get; private set; }
		public ZString HeightInMM { get; private set; }

		public ZString Height
		{
			get { return HeightInFeet.IsEmpty ? HeightInMM : HeightInFeet; }
		}

		#endregion

		#region Width

		public ZString WidthInFeet { get; private set; }
		public ZString WidthInMM { get; private set; }

		public ZString Width
		{
			get { return WidthInFeet.IsEmpty ? WidthInMM : WidthInFeet; }
		}

		#endregion

		#region Type Group

		public ZString GroupCode { get; private set; }
		public ZString GroupDescription { get; private set; }

		#endregion

		#region Detailed Group

		public ZString DetailedGroupCode { get; private set; }
		public ZString DetailedGroupDescription { get; private set; }

		#endregion

		#endregion

		#region ISO Values

		void ReGenerateValues(ZString input)
		{
			ResetAllInternalValues();
			isoCode = input;

			if (!isoCode.IsEmpty)
			{
				IsKnown = true;
				SetISOLengthValues(isoCode.SubstringSafe(0, 1));
				SetISOHeightValues(isoCode.SubstringSafe(1, 1));
				SetISOWidthValues(isoCode.SubstringSafe(1, 1));
				SetISOGroupAndDetailedGroupInfo(isoCode.SubstringSafe(2, 2));
				SetGroupDescription(GroupCode);
			}

			if (BizOToRefreshBindingOn != null)
			{
				BizOToRefreshBindingOn.RefreshBinding();
			}
		}

		void ResetAllInternalValues()
		{
			isoCode = "";
			IsKnown = false;
			LengthInMM = "";
			LengthInFeet = "";
			WidthInFeet = "";
			WidthInMM = "";
			HeightInFeet = "";
			HeightInMM = "";
			GroupCode = "";
			GroupDescription = "";
			DetailedGroupCode = "";
			DetailedGroupDescription = "";
			OverDimensionAllowed = false;
			Category = Categories.Other;
		}

		void SetGroupDescription(ZString input)
		{
			switch (input)
			{
				case "AS":
					GroupDescription = Res.GetString("c17f98b4-d2e9-4aaf-9b75-6d16d249c1d4", "Air/Surface Container");
					break;

				case "BU":
					GroupDescription = Res.GetString("e2529135-ce1e-47d9-9540-3e237a5b49e3", "Dry Bulk Cargo - Non-Pressurized, Box Type");
					break;

				case "GF":
					GroupDescription = Res.GetString("E8F53E70-96A3-431C-AEF9-264A0536B206", "Foldable General Purpose Containers");
					Category = Categories.GeneralPurpose;
					break;

				case "GP":
					GroupDescription = Res.GetString("1da27ef4-8df6-40fd-8563-faed881fa2bb", "General Purpose Container Without Ventilation");
					Category = Categories.GeneralPurpose;
					break;

				case "HE":
					GroupDescription = Res.GetString("48e82146-3b80-4f59-af54-b6291e94fadc", "Thermal Container Eutectic");
					Category = Categories.Refrigerated;
					break;

				case "HI":
					GroupDescription = Res.GetString("9007fd6a-9a4d-44f1-a824-797dc280bc72", "Thermal Container Insulated");
					Category = Categories.Refrigerated;
					break;

				case "HR":
					GroupDescription = Res.GetString("b877af7d-78a3-48cd-9270-bfd171d31594", "Thermal Container Refrigerated and/or Heated With Removable Equipment");
					Category = Categories.Refrigerated;
					break;

				case "KL":
					GroupDescription = Res.GetString("2d248bc8-bf76-4b20-834f-c3049be0d937", "Pressurized Tank Container (Liquids and Gases)");
					break;

				case "NH":
					GroupDescription = Res.GetString("789a0dd9-1f5f-4432-b570-323d9fed6d17", "Hopper Tank Container (Dry)");
					break;

				case "NN":
					GroupDescription = Res.GetString("ac4d9d55-e939-4a52-89a8-2c0a6ca71263", "Non-Pressurized Tank Container (Dry)");
					break;

				case "NP":
					GroupDescription = Res.GetString("e5d75197-018a-441d-9900-1c563dab4835", "Pressurized Tank Container (Dry)");
					break;

				case "PC":
					GroupDescription = Res.GetString("1a9c3200-e0b8-4bf9-a361-be00a9562557", "Platform (Container) Folding (collapsible)");
					OverDimensionAllowed = true;
					Category = Categories.FlatRack;
					break;

				case "PF":
					GroupDescription = Res.GetString("11eaa9c5-aeed-4985-b294-7c32d9823711", "Platform (Container) Fixed");
					OverDimensionAllowed = true;
					Category = Categories.FlatRack;
					break;

				case "PL":
					GroupDescription = Res.GetString("14a7e5e2-d746-4469-a1e0-b75480b3a477", "Platform (Container) Platform-based container with incomplete superstructure");
					OverDimensionAllowed = true;
					Category = Categories.FlatRack;
					break;

				case "PS":
					GroupDescription = Res.GetString("f47c719b-1aa3-418d-a61e-f5f6eace126a", "Platform (Container) Platform Based Container With Complete Superstructure");
					OverDimensionAllowed = true;
					Category = Categories.FlatRack;
					break;

				case "PT":
					GroupDescription = Res.GetString("b673c4eb-521c-4861-b91c-8e0168684a26", "Platform (Container) Platform Based Container For Named Cargo");
					OverDimensionAllowed = true;
					Category = Categories.FlatRack;
					break;

				case "RE":
					GroupDescription = Res.GetString("aa3d1765-4245-425e-b5c4-54f5996079f2", "Thermal Container Refrigerated");
					Category = Categories.Refrigerated;
					break;

				case "RH":
					GroupDescription = Res.GetString("5ebea6cc-9400-4558-a66a-8f1e496fc537", "Heated");
					break;

				case "RI":
					GroupDescription = Res.GetString("dd30cc34-04da-408d-b0ad-de470c5c479e", "Integrated machinery");
					break;

				case "RS":
					GroupDescription = Res.GetString("5b762b1b-588b-48fb-bd93-a63c57eff429", "Thermal Container, Self-Powered");
					Category = Categories.Refrigerated;
					break;

				case "RT":
					GroupDescription = Res.GetString("4632c0c4-0855-406f-a3b4-9b98ccb443c0", "Thermal Container Refrigerated and Heated");
					Category = Categories.Refrigerated;
					break;

				case "SC":
					GroupDescription = Res.GetString("b4ee394b-4d69-4ec3-92a4-0bc418d8dcba", "Non-cargo carrying containers");
					break;

				case "SN":
					GroupDescription = Res.GetString("c39f324e-ea06-434a-9f64-5101aba0dcfb", "Named Cargo");
					break;

				case "UT":
					GroupDescription = Res.GetString("acd6ff0c-022c-4a6e-9700-30cab338e9cd", "Open-top Container");
					OverDimensionAllowed = true;
					Category = Categories.OpenTop;
					break;

				case "VH":
					GroupDescription = Res.GetString("4a6eb21f-00b5-4ff2-8a3e-904cfb2c67e9", "General Purpose Container With Ventilation");
					Category = Categories.GeneralPurpose;
					break;

				case "WR":
				case "WS":
					GroupDescription = Res.GetString("2d454316-877f-4e70-86c8-1e87aefb26a0", "Foldable general purpose containers");
					Category = Categories.GeneralPurpose;
					break;

				default:
					IsKnown = false;
					break;
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		static ContainerMeasurement GetISOLengthValues(ZString input)
		{
			switch (input)
			{
				case "1":
					return new ContainerMeasurement { InMM = 2991, PortionInFeet = 10 };

				case "2":
					return new ContainerMeasurement { InMM = 6068, PortionInFeet = 20 };

				case "3":
					return new ContainerMeasurement { InMM = 9125, PortionInFeet = 30 };

				case "4":
					return new ContainerMeasurement { InMM = 12192, PortionInFeet = 40 };

				case "5":
					return new ContainerMeasurement { InMM = 13716, PortionInFeet = 45 };

				case "A":
					return new ContainerMeasurement { InMM = 7150 };

				case "B":
					return new ContainerMeasurement { InMM = 7315, PortionInFeet = 24 };

				case "C":
					return new ContainerMeasurement { InMM = 7430, PortionInFeet = 24, PortionInInch = 6 };

				case "D":
					return new ContainerMeasurement { InMM = 7450 };

				case "E":
					return new ContainerMeasurement { InMM = 7820 };

				case "F":
					return new ContainerMeasurement { InMM = 8100 };

				case "G":
					return new ContainerMeasurement { InMM = 12500, PortionInFeet = 41 };

				case "H":
					return new ContainerMeasurement { InMM = 13106, PortionInFeet = 43 };

				case "K":
					return new ContainerMeasurement { InMM = 13600 };

				case "L":
					return new ContainerMeasurement { InMM = 13716, PortionInFeet = 45 };

				case "M":
					return new ContainerMeasurement { InMM = 14630, PortionInFeet = 48 };

				case "N":
					return new ContainerMeasurement { InMM = 14935, PortionInFeet = 49 };

				case "P":
					return new ContainerMeasurement { InMM = 16154, PortionInFeet = 53 };

				default:
					return new ContainerMeasurement { IsKnown = false };
			}
		}

		void SetISOLengthValues(ZString input)
		{
			var value = GetISOLengthValues(input);
			IsKnown = value.IsKnown;
			if (IsKnown)
			{
				LengthInMM = value.InMMDescription;
				LengthInFeet = value.InFeetDescription;
			}
		}

		public static void DefaultSizing(RefContainer refContainer)
		{
			if (refContainer != null)
			{
				var lengthValue = GetISOLengthValues(refContainer.RC_ISOType.SubstringSafe(0, 1));
				if (lengthValue.IsKnown)
				{
					if (lengthValue.PortionInFeet.HasValue)
					{
						refContainer.RC_Length = lengthValue.InFeet;
					}
					else if (lengthValue.InMM.HasValue)
					{
						refContainer.LengthMetres = lengthValue.InMeter;
					}
					else
					{
						refContainer.RC_Length = 0m;
					}
				}
				var heightValue = GetISOHeightValues(refContainer.RC_ISOType.SubstringSafe(1, 1));
				if (heightValue.IsKnown)
				{
					if (heightValue.PortionInFeet.HasValue)
					{
						refContainer.RC_Height = heightValue.InFeet;
					}
					else if (heightValue.InMM.HasValue)
					{
						refContainer.HeightMetres = heightValue.InMeter;
					}
					else
					{
						refContainer.RC_Height = 0m;
					}
				}
				var widthValue = GetISOWidthValues(refContainer.RC_ISOType.SubstringSafe(1, 1));
				if (widthValue.IsKnown)
				{
					if (widthValue.PortionInFeet.HasValue)
					{
						refContainer.RC_Width = widthValue.InFeet;
					}
					else if (widthValue.InMM.HasValue)
					{
						refContainer.WidthMetres = widthValue.InMeter;
					}
					else
					{
						refContainer.RC_Width = 0m;
					}
				}
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		static ContainerMeasurement GetISOHeightValues(ZString input)
		{
			switch (input)
			{
				case "0":
					return new ContainerMeasurement { InMM = 2438, PortionInFeet = 8 };

				case "2":
				case "C":
				case "L":
				case "R":
					return new ContainerMeasurement { InMM = 2591, PortionInFeet = 8, PortionInInch = 6 };

				case "4":
				case "D":
				case "M":
				case "S":
					return new ContainerMeasurement { InMM = 2743, PortionInFeet = 9 };

				case "5":
				case "E":
				case "N":
				case "T":
					return new ContainerMeasurement { InMM = 2896, PortionInFeet = 9, PortionInInch = 6 };

				case "6":
				case "F":
				case "P":
				case "U":
					return new ContainerMeasurement { InMMDescription = ">2896mm", InFeetDescription = ">9'6" }; // SuppressCodeSmell Reason = non-semantic text

				case "7":
					return new ContainerMeasurement { InMMDescription = "2438>h>1219", InFeetDescription = "8>h>4" }; // SuppressCodeSmell Reason = non-semantic text

				case "8":
					return new ContainerMeasurement { InMM = 1295, PortionInFeet = 4, PortionInInch = 3 };

				case "9":
					return new ContainerMeasurement { InMMDescription = "<=1219mm", InFeetDescription = "<=4'" }; // SuppressCodeSmell Reason = non-semantic text

				default:
					return new ContainerMeasurement { IsKnown = false };
			}
		}

		void SetISOHeightValues(ZString input)
		{
			var value = GetISOHeightValues(input);
			IsKnown = value.IsKnown;
			if (IsKnown)
			{
				HeightInMM = value.InMMDescription;
				HeightInFeet = value.InFeetDescription;
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		static ContainerMeasurement GetISOWidthValues(ZString input)
		{
			switch (input)
			{
				case "0":
				case "2":
				case "4":
				case "5":
				case "6":
				case "7":
				case "8":
				case "9":
					return new ContainerMeasurement { InMM = 2438, PortionInFeet = 8 };

				case "R":
				case "S":
				case "T":
				case "U":
				case "C":
				case "D":
				case "E":
				case "F":
					return new ContainerMeasurement { InMMDescription = ">2438mm & <=2500mm Overall" }; // SuppressCodeSmell Reason = non-semantic text

				case "L":
				case "M":
				case "N":
				case "P":
					return new ContainerMeasurement { InMMDescription = ">2500mm Overall" }; // SuppressCodeSmell Reason = non-semantic text

				default:
					return new ContainerMeasurement { IsKnown = false };
			}
		}

		void SetISOWidthValues(ZString input)
		{
			var value = GetISOWidthValues(input);
			IsKnown = value.IsKnown;
			if (IsKnown)
			{
				WidthInMM = value.InMMDescription;
				WidthInFeet = value.InFeetDescription;
			}
		}

		public static IEnumerable<string> ValidISOLengthValues
		{
			get
			{
				return new[] { "1", "2", "3", "4", "5", "A", "B", "C", "D", "E", "F", "G", "H", "K", "L", "M", "N", "P" };
			}
		}

		public static IEnumerable<string> ValidISOHeightAndWidthValues
		{
			get
			{
				return new[] { "0", "2", "4", "5", "6", "7", "8", "9", "R", "S", "T", "U", "C", "D", "E", "F", "L", "M", "N", "P" };
			}
		}

		public static IEnumerable<string> ValidISOGroupValues
		{
			get
			{
				return new[] { "G0", "G1", "G2", "G3", "G9", "V0", "V2", "V4",
				"B0", "B1", "B3", "B4", "B5", "B6", "B7", "B8", "B9", "S0",
				"S1", "S2", "S4", "S8", "S9", "R0", "R1", "R2", "R3", "R5",
				"R7", "R8", "H0", "H1", "H2", "H5", "H6", "H8", "U0", "U1",
				"U2", "U3", "U4", "U6", "U9", "P0", "P1", "P2", "P3", "P4",
				"P5", "P6", "P7", "P8", "P9", "K0", "K1", "K2", "K3", "K4",
				"K5", "K6", "K7", "K8", "N0", "N1", "N3", "N4", "N5", "N7",
				"N8", "N9", "A0", "W0", "W1" };
			}
		}

		void SetISOGroupAndDetailedGroupInfo(ZString input)
		{
			input = TransformTypeBToTypeACodes(input);

			DetailedGroupCode = input;

			if (GroupCodeDescriptionList.TryGetValue(input, out var groupCodeDescriptionPair))
			{
				GroupCode = groupCodeDescriptionPair.Code;
				DetailedGroupDescription = groupCodeDescriptionPair.MultilingualDescription;

				OverDimensionAllowed = (input == "H0" || input == "H2");
			}
			else
			{
				IsKnown = false;
				DetailedGroupCode = "";
			}
		}

		public static Dictionary<ZString, CodeDescriptionPair> GroupCodeDescriptionList
		{
			get
			{
				return new Dictionary<ZString, CodeDescriptionPair>
				{
					{ "G0", new CodeDescriptionPair("GP", Res.GetString("ac79acb8-0629-40d9-a1db-dfeb5d4369d5", "Openings at one end or both ends")) },
					{ "G1", new CodeDescriptionPair("GP", Res.GetString("9ba21d7a-590b-436d-8942-77f2da16b7e4", "Passive vents at upper part of cargo space")) },
					{ "G2", new CodeDescriptionPair("GP", Res.GetString("bd807203-087c-4515-9a91-7f894a24e525", "Opening(s) at one or both ends plus \"full\" opening(s) on one or both sides") + " ") },
					{ "G3", new CodeDescriptionPair("GP", Res.GetString("eaee0118-fb03-479e-afa2-f05aa641fe35", "Opening(s) at one or both ends plus \"partial\" opening(s) on one or both sides") + " ") },
					{ "G9", new CodeDescriptionPair("GP", Res.GetString("860f4395-9257-4f83-9ed6-ac51b6c72c56", "With bulk capabilities")) },
					{ "V0", new CodeDescriptionPair("VH", Res.GetString("fe7c05d1-7d4c-4f29-b3ee-0a39fba04ecd", "Non-mechanical system, vents at lower and upper parts of cargo space")) },
					{ "V2", new CodeDescriptionPair("VH", Res.GetString("1c108905-219f-4cbe-a308-690f57ed6643", "Mechanical ventilation system, located internally")) },
					{ "V4", new CodeDescriptionPair("VH", Res.GetString("48fc91fa-bc4b-424d-a19a-42b46e1d8389", "Mechanical ventilation system, located externally")) },
					{ "B0", new CodeDescriptionPair("BU", Res.GetString("9f873e80-4cee-4e58-8bc5-dfefccc783a0", "Closed")) },
					{ "B1", new CodeDescriptionPair("BU", Res.GetString("e168dc14-6a4b-418d-9b10-163b70054d6f", "Airtight")) },
					{ "B3", new CodeDescriptionPair("BU", Res.GetString("fad99e58-8334-47c0-870c-8d84d61a3682", "Rear discharge/cat flap type")) },
					{ "B4", new CodeDescriptionPair("BU", Res.GetString("77652517-6e16-4349-acbc-35ec4f5d35c1", "Rear discharge/full width opening")) },
					{ "B5", new CodeDescriptionPair("BU", Res.GetString("683aa8f9-562c-4d8c-b9d8-9403e49b27a7", "Rear discharge/full width fixed")) },
					{ "B6", new CodeDescriptionPair("BU", Res.GetString("33cbf9ba-1bd7-4f2a-8db3-6245a3bbb52b", "with removable hard top equipped with full length hinged hatch, and full length and width bottom discharge")) },
					{ "B7", new CodeDescriptionPair("BU", Res.GetString("b892b079-ac8e-4eb6-b214-55fd33b6aa25", "with open top container with full length and width bottom discharge")) },
					{ "B8", new CodeDescriptionPair("BU", Res.GetString("ef0118b5-9344-47cb-abdc-7b0912550e80", "Front discharge/full width")) },
					{ "B9", new CodeDescriptionPair("BU", Res.GetString("1427e69b-885a-4567-8c50-e91e8abec359", "Side discharge")) },
					{ "S0", new CodeDescriptionPair("SN", Res.GetString("cdd7d6ff-f38d-48f2-aa1b-44a6693c778a", "Livestock carrier")) },
					{ "S1", new CodeDescriptionPair("SN", Res.GetString("18943db2-4697-4c25-9c96-edeb5630b619", "Automotive carrier")) },
					{ "S2", new CodeDescriptionPair("SN", Res.GetString("dd5b4e23-9e9b-4aba-85c3-1b70e3bfab4c", "Live fish carrier")) },
					{ "S4", new CodeDescriptionPair("SC", Res.GetString("df4b3502-70a2-4cca-8755-f03efae38dfb", "Generator")) },
					{ "S8", new CodeDescriptionPair("SC", Res.GetString("7265d07c-3f2a-49d7-ad34-44ef0ff30313", "Non-cargo carrying container for sensitive installed equipment")) },
					{ "S9", new CodeDescriptionPair("SC", Res.GetString("2101a6d9-9ed3-4935-b614-5cf681ff5250", "Non-cargo carrying containers for residential or commercial use")) },
					{ "R0", new CodeDescriptionPair("RE", Res.GetString("abb349ed-326c-469e-87c9-0193127fb364", "Mechanically refrigerated")) },
					{ "R1", new CodeDescriptionPair("RT", Res.GetString("c88a0b0b-9440-4a2f-a441-b9867351e191", "Mechanically refrigerated and heated")) },
					{ "R2", new CodeDescriptionPair("RS", Res.GetString("d8681ad5-0719-4390-bfc4-9988c9e8d5b8", "Mechanically refrigerated")) },
					{ "R3", new CodeDescriptionPair("RS", Res.GetString("c88a0b0b-9440-4a2f-a441-b9867351e191", "Mechanically refrigerated and heated")) },
					{ "R5", new CodeDescriptionPair("RI", Res.GetString("274e095a-4570-4331-b480-9d4dfe93595a", "Integrated mechanically refrigerated and heated")) },
					{ "R7", new CodeDescriptionPair("RH", Res.GetString("35f3273f-226d-43fb-867c-f72a1e8d7d4a", "Heated")) },
					{ "R8", new CodeDescriptionPair("RH", Res.GetString("6be34335-3c9b-4ff9-93ec-f96873fd0b65", "Heated, self-powered")) },
					{ "H0", new CodeDescriptionPair("HR", Res.GetString("d7edbb16-33c1-412a-824d-74eb32d9cda9", "Refrigerated and/or heated with removable equipment located externally, heat transfer coefficient K = 0,4 W/(M2-K))")) },
					{ "H1", new CodeDescriptionPair("HR", Res.GetString("b634a4c9-8a6a-4c9e-9816-efc0eaa26189", "Refrigerated and/or heated with removable equipment located internally")) },
					{ "H2", new CodeDescriptionPair("HR", Res.GetString("c64632cd-7db9-4208-ab26-b4ddb0fb109f", "Refrigerated and/or heated with removable equipment located externally, heat transfer coefficient K = 0,7 W/(M2-K)")) },
					{ "H5", new CodeDescriptionPair("HI", Res.GetString("501d5de1-0875-4470-b8b4-c29d543c3d59", "Insulated; heat transfer coefficient K = 0,4 W/(M2-K)")) },
					{ "H6", new CodeDescriptionPair("HI", Res.GetString("4294465f-83fb-4ae6-8281-3b3602df28d5", "Insulated; heat transfer coefficient K = 0 W/(M2-K)")) },
					{ "H8", new CodeDescriptionPair("HE", Res.GetString("8032cf6c-59ef-4ef0-af37-9cc27753d0ee", "Eutectic, remote mechanical refrigeration")) },
					{ "U0", new CodeDescriptionPair("UT", Res.GetString("1209bd21-c10c-40ef-9b3c-0673e030e929", "Opening(s) at one or both ends")) },
					{ "U1", new CodeDescriptionPair("UT", Res.GetString("59686ef8-f636-4d23-a1b1-cb7b16861b9b", "Opening(s) at one or both ends, plus removable top member(s) in end frame(s)")) },
					{ "U2", new CodeDescriptionPair("UT", Res.GetString("414b3b76-6c9c-442e-92bc-6cda08e7a153", "Opening(s) at one of both ends, plus opening(s) on one or both sides")) },
					{ "U3", new CodeDescriptionPair("UT", Res.GetString("5b4f3b48-f658-4404-a925-35e5d9454fa7", "Opening(s) at one or both ends, plus opening(s) on one or both sides plus removable top member(s) in end frame(s)")) },
					{ "U4", new CodeDescriptionPair("UT", Res.GetString("b4df4bcb-6892-4d0a-b9d6-b51bbecff30e", "Opening(s) at one or both ends, plus partial opening on one side and full opening on the other side")) },
					{ "U6", new CodeDescriptionPair("UT", Res.GetString("0863136a-b885-416f-87d6-5fc6bc3ebebc", "Open topped container with removable hard top")) },
					{ "U9", new CodeDescriptionPair("UT", Res.GetString("17c90492-98f1-4e23-9511-3366a5735193", "Coil carrier")) },
					{ "P0", new CodeDescriptionPair("PL", Res.GetString("078fbc32-2f48-464f-973a-9a74821b4486", "Platform (container)")) },
					{ "P1", new CodeDescriptionPair("PF", Res.GetString("dae6989c-1159-41e3-926d-ea6d47a5f65c", "Two complete and fixed ends")) },
					{ "P2", new CodeDescriptionPair("PF", Res.GetString("63bd33e0-52ca-4512-a9d7-8f7d5c0408d5", "Fixed posts, either free-standing or with removable top member")) },
					{ "P3", new CodeDescriptionPair("PC", Res.GetString("a2d8f0df-40d9-43c2-b75b-c7b403e87888", "Folding complete end structure")) },
					{ "P4", new CodeDescriptionPair("PC", Res.GetString("7f5819e9-329a-403b-8f28-ab6696f98417", "Folding posts, either free-standing or with removable top member")) },
					{ "P5", new CodeDescriptionPair("PS", Res.GetString("4dc4b2dd-6dcb-4bb7-8b94-286f9bab74d0", "Open top, open ends (skeletal)")) },
					{ "P6", new CodeDescriptionPair("PT", Res.GetString("2fdef762-7873-4e3a-b9a3-4592b6e91ad9", "Ship's gear carrier")) },
					{ "P7", new CodeDescriptionPair("PT", Res.GetString("a07b9b7f-7bb1-4a9e-b9e8-0a3d3b93efb4", "Car carrier")) },
					{ "P8", new CodeDescriptionPair("PT", Res.GetString("d95119aa-6170-4fd1-b251-a0075ca304a5", "Timber/pipe carrier")) },
					{ "P9", new CodeDescriptionPair("PT", Res.GetString("e1680c09-da1e-4afd-b4d4-99073fb48ca4", "Coil carrier")) },
					{ "K0", new CodeDescriptionPair("KL", Res.GetString("1fc0ffd6-258a-458f-a096-5b914b0817cf", "Liquid tank non-regulated goods")) },
					{ "K1", new CodeDescriptionPair("KL", Res.GetString("71745ffd-d301-4d0a-baca-b13008a86d06", "Liquid tank dangerous goods less than or equal to 2,65 bar pressure")) },
					{ "K2", new CodeDescriptionPair("KL", Res.GetString("331d886b-5ada-4a93-9a2d-828665f422c7", "Liquid tank dangerous goods > 2,65 bar and less than or equal to 10 bar pressure")) },
					{ "K3", new CodeDescriptionPair("KL", Res.GetString("abac3648-1ef0-49d1-b03d-214ae4489f14", "Liquid tank dangerous goods > 10 bar high pressure")) },
					{ "K4", new CodeDescriptionPair("KL", Res.GetString("bd227b85-1bc6-4b45-ba22-91230128bc96", "Liquid tank non regulated goods requiring power supply")) },
					{ "K5", new CodeDescriptionPair("KL", Res.GetString("ec7b422d-b25f-4757-8a1a-4535eddf7472", "Liquid tank for dangerous goods less than or equal to 10 bar requiring power supply")) },
					{ "K6", new CodeDescriptionPair("KL", Res.GetString("f44045d8-0a3b-497b-8c98-b43ef8644fc7", "Liquid tank for dangerous goods > 10 bar pressure requiring power supply")) },
					{ "K7", new CodeDescriptionPair("KL", Res.GetString("733a306f-93b9-4149-a1c2-fb6330e91892", "Cryogenic tank")) },
					{ "K8", new CodeDescriptionPair("KL", Res.GetString("9302d57f-408f-4ab6-9644-1d91e7dfe074", "Gas tank")) },
					{ "N0", new CodeDescriptionPair("NH", Res.GetString("16b2a4bd-e177-41ea-b4ad-2b4745e5017b", "Hopper type vertical discharge")) },
					{ "N1", new CodeDescriptionPair("NH", Res.GetString("a3915ef8-3bdc-40de-b20b-a4c9febe4416", "Hopper type rear discharge")) },
					{ "N3", new CodeDescriptionPair("NN", Res.GetString("a84e8496-cc42-428f-864b-6dd9dfb9a712", "Non-pressurized rear discharge")) },
					{ "N4", new CodeDescriptionPair("NN", Res.GetString("e4c864f5-35dc-4753-8cbb-ef8fc7e04d24", "Non-pressurized side discharge")) },
					{ "N5", new CodeDescriptionPair("NN", Res.GetString("afa7af55-96ae-4499-b8d0-5244bef65e68", "Non-pressurized tipping discharge")) },
					{ "N7", new CodeDescriptionPair("NP", Res.GetString("84f8ff91-99e5-4957-aa50-20a7018507f9", "Pressurized rear discharge")) },
					{ "N8", new CodeDescriptionPair("NP", Res.GetString("ec9fbfa1-3b30-40c7-a980-0f4b7ab948f5", "Pressurized side discharge")) },
					{ "N9", new CodeDescriptionPair("NP", Res.GetString("d08c1b9b-bc00-4911-98b0-7d182b3b7522", "Pressurized tipping discharge")) },
					{ "A0", new CodeDescriptionPair("AS", Res.GetString("d73a8e54-e357-40ea-b006-5bfbec4449d1", "General Purpose")) },
					{ "W0", new CodeDescriptionPair("WR", Res.GetString("A0615A1E-D193-4530-9DCA-03E0B9DB049B", "Container folding on their base structure")) },
					{ "W1", new CodeDescriptionPair("WS", Res.GetString("C8DB0862-AE0E-4F77-83D0-E3566F4ADB24", "Container folding on their side structure")) }
				};
			}
		}

		/// <summary>
		/// ISO Container Detailed Type has two forms, one a letter followed by a number (Type A) and one with two letters (Type B).
		/// Since the "main characteristics" for both A and B are the same, and they mostly follow a pattern, we can transform one into the other.
		/// Reference: ISO Freight Containers - Coding, identification and marking [ISO 6346:1995/Amd.3:2012(E)]
		/// </summary>
		/// <param name="code">The two Letter code to determine the container detailed description, could be either Type A or B</param>
		/// <returns>Either the original input, an empty string or an ISO Container Tybe B Code</returns>
		ZString TransformTypeBToTypeACodes(ZString code)
		{
			ZString result = new ZString();

			if (code != ZString.Empty && code.Length == 2 && code.IsLettersOnlyOrEmpty)
			{
				result = CheckInputForExceptionsToPattern(code);

				if (result.IsEmpty)
				{
					switch (code[1])
					{
						case 'A':
							result = code[0] + "0";
							break;
						case 'B':
							result = code[0] + "1";
							break;
						case 'D':
							result = code[0] + "2";
							break;
						case 'G':
							result = code[0] + "3";
							break;
						case 'J':
							result = code[0] + "4";
							break;
						case 'M':
							result = code[0] + "5";
							break;
						case 'V':
							result = code[0] + "6";
							break;
						case 'W':
							result = code[0] + "7";
							break;
						case 'X':
							result = code[0] + "8";
							break;
						case 'Y':
							result = code[0] + "9";
							break;
						default:
							break;
					}
				}
			}

			return result.IsEmpty ? code : result;
		}

		ZString CheckInputForExceptionsToPattern(ZString input)
		{
			if (input == "NL")
			{
				return "N9";
			}

			return ZString.Empty;
		}

		#endregion

		class ContainerMeasurement
		{
			public ContainerMeasurement()
			{
				IsKnown = true;
			}

			public int? InMM
			{
				get { return fInMM; }
				set
				{
					fInMM = value;
					if (InMM.HasValue)
					{
						InMMDescription = InMM.Value + "mm"; // SuppressCodeSmell Reason = Universal metrics
					}
				}
			}
			int? fInMM;

			public ZString InMMDescription { get; set; }

			public ZDecimal InFeet => (PortionInFeet ?? 0m) + Core.Constants.Length.Convert(PortionInInch ?? 0m, Core.Constants.Length.Inches, Core.Constants.Length.Feet);
			public ZDecimal InMeter => Core.Constants.Length.Convert(InMM ?? 0m, Core.Constants.Length.Millimetres, Core.Constants.Length.Metres);

			public int? PortionInFeet
			{
				get
				{
					return fPortionInFeet;
				}
				set
				{
					fPortionInFeet = value;
					SetInFeetDescription();
				}
			}
			int? fPortionInFeet;
			public int? PortionInInch
			{
				get { return fPortionInInch; }
				set
				{
					fPortionInInch = value;
					SetInFeetDescription();
				}
			}
			int? fPortionInInch;

			void SetInFeetDescription()
			{
				var result = string.Empty;
				if (PortionInFeet.HasValue)
				{
					result = PortionInFeet.Value + "'";
				}
				if (PortionInInch.HasValue)
				{
					if (string.IsNullOrEmpty(result))
					{
						result = "0'";
					}
					result += PortionInInch.Value;
				}
				InFeetDescription = result;
			}

			public ZString InFeetDescription { get; set; }
			public bool IsKnown { get; set; }
		}
	}
}
