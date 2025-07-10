using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class ConversionToSKU : IPackTypeConversion
	{
		public ConversionToSKU(string packType, string uomType)
		{
			packType = packType.ToUpper();
			PackType = packType;
			UOMType = uomType;
			QtySKU = 1;
			ConversionPath = packType;
		}

		public ConversionToSKU(string packType, string uomType, decimal qtySKU, string previousConversionPath)
		{
			packType = packType.ToUpper();
			PackType = packType;
			UOMType = uomType;
			QtySKU = qtySKU;
			ConversionPath = packType + "->" + previousConversionPath;
		}

		public ZString PackType { get; }
		public ZString UOMType { get; }
		public decimal QtySKU { get; }
		public string ConversionPath { get; }
		public bool IsInvalid { get; set; }

		public IEnumerable<string> PackTypesFromConversionPath
		{
			get { return ConversionPath.Split(new[] { "->" }, StringSplitOptions.None); }
		}

		public bool ContainsLoopForThePackType
		{
			get { return PackTypesFromConversionPath.Count(x => x == PackType.ToString()) > 1; }
		}

		decimal IPackTypeConversion.PackQty => 1m;

		public override bool Equals(object obj)
		{
			var target = obj as ConversionToSKU;
			return (target != null) ? PackType.Equals(target.PackType) : base.Equals(obj);
		}

		public override int GetHashCode() => PackType.GetHashCode();

		public override string ToString() => $"{PackType} {QtySKU} ({ConversionPath})";
	}
}
