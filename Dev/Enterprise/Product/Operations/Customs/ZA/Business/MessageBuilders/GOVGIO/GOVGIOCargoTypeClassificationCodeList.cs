using Enterprise.Edifact.D16A.Elements;

namespace Enterprise.Customs.ZA.Business.MessageBuilders
{
	internal class GOVGIOCargoTypeClassificationCodeList : CargoTypeClassificationCodeList
	{
		GOVGIOCargoTypeClassificationCodeList(string codeValue) : base(codeValue)
		{
		}

		/// <summary>
		/// DB = Dry bulk
		/// </summary>
		public static CargoTypeClassificationCodeList DryBulk => new GOVGIOCargoTypeClassificationCodeList("DB");

		/// <summary>
		/// LB = Liquid bulk
		/// </summary>
		public static CargoTypeClassificationCodeList LiquidBulk => new GOVGIOCargoTypeClassificationCodeList("LB");

		/// <summary>
		/// BB = Break Bulk
		/// </summary>
		public static CargoTypeClassificationCodeList BreakBulk => new GOVGIOCargoTypeClassificationCodeList("BB");

		/// <summary>
		/// CN = Container
		/// </summary>
		public static CargoTypeClassificationCodeList Container => new GOVGIOCargoTypeClassificationCodeList("CN");

		/// <summary>
		/// MX = Mixed Cargo
		/// </summary>
		public static CargoTypeClassificationCodeList MixedCargo => new GOVGIOCargoTypeClassificationCodeList("MX");
	}
}
