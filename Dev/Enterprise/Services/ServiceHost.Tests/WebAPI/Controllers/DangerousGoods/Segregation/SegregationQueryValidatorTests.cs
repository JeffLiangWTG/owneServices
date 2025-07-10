using System;
using CargoWise.Definitions.Freight.DangerousGoods.SegregationApi;
using Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.Services.ServiceHost.Tests.WebAPI.Controllers.DangerousGoods.Segregation
{
	class SegregationQueryValidatorTests : TestCase
	{
		public void TestIsQueryValid_ShouldRejectQueryIfStandardsAreInvalid()
		{
			var query = new SegregationQuery
			{
				Standards = new[] { "invalidStandard" },
				Ids = new[] { Guid.NewGuid(), Guid.NewGuid() }
			};

			var validator = new SegregationQueryValidator();
			var (validationResult, validationMessage) = validator.IsQueryValid(query);
			AssertEquals("The following standard specified in the query is not valid: invalidStandard", validationMessage);
		}

		public void TestIsQueryValid_ShouldAcceptQueryIfStandardsAreValid()
		{
			var query = new SegregationQuery
			{
				Standards = new[] { UNDGSubstanceStandardTypes.IMO, UNDGSubstanceStandardTypes.IATA },
				Ids = new[] { Guid.NewGuid(), Guid.NewGuid() }
			};

			var validator = new SegregationQueryValidator();
			var (validationResult, validationMessage) = validator.IsQueryValid(query);
			AssertEquals(true, validationResult);
			AssertEquals(string.Empty, validationMessage);
		}

		public void TestIsQueryValid_ShouldRejectQueryIfStandardsContainDuplicates()
		{
			var query = new SegregationQuery
			{
				Standards = new[] { UNDGSubstanceStandardTypes.IMO, UNDGSubstanceStandardTypes.IMO },
				Ids = new[] { Guid.NewGuid(), Guid.NewGuid() }
			};

			var validator = new SegregationQueryValidator();
			var (validationResult, validationMessage) = validator.IsQueryValid(query);
			AssertEquals("List of standards cannot contain duplicates", validationMessage);
		}

		public void TestIsQueryValid_ShouldRejectQueryIfIdsContainDuplicates()
		{
			var id1 = Guid.NewGuid();
			var id2 = Guid.NewGuid();
			var query = new SegregationQuery
			{
				Standards = new[] { UNDGSubstanceStandardTypes.IMO, UNDGSubstanceStandardTypes.IATA },
				Ids = new[] { id1, id2, id1 }
			};
			var validator = new SegregationQueryValidator();
			var (validationResult, validationMessage) = validator.IsQueryValid(query);
			AssertEquals("List of ids cannot contain duplicates", validationMessage);
		}

		public void TestIsQueryValid_ShouldRejectQueryIfStandardsAreEmpty()
		{
			var query = new SegregationQuery
			{
				Standards = Array.Empty<string>(),
				Ids = new[] { Guid.NewGuid(), Guid.NewGuid() }
			};

			var validator = new SegregationQueryValidator();
			var (validationResult, validationMessage) = validator.IsQueryValid(query);
			AssertEquals("List of standards cannot be empty", validationMessage);
		}

		public void TestIsQueryValid_ShouldRejectQueryIfIdsAreEmpty()
		{
			var query = new SegregationQuery
			{
				Standards = new[] { UNDGSubstanceStandardTypes.IMO, UNDGSubstanceStandardTypes.IATA },
				Ids = Array.Empty<Guid>()
			};

			var validator = new SegregationQueryValidator();
			var (validationResult, validationMessage) = validator.IsQueryValid(query);
			AssertEquals("List of Ids cannot be empty", validationMessage);
		}

		public void TestIsQueryValid_ShouldRejectQueryIfIdsContainLessThanTwoIds()
		{
			var query = new SegregationQuery
			{
				Standards = new[] { UNDGSubstanceStandardTypes.IMO, UNDGSubstanceStandardTypes.IATA },
				Ids = new[] { Guid.NewGuid() }
			};

			var validator = new SegregationQueryValidator();
			var (validationResult, validationMessage) = validator.IsQueryValid(query);
			AssertEquals("List of Ids must contain at least two entries", validationMessage);
		}

		public void TestIsQueryValid_ShouldRejectQueryIfQueryIsNull()
		{
			var validator = new SegregationQueryValidator();
			var (validationResult, validationMessage) = validator.IsQueryValid(null);
			AssertEquals("Query cannot be null", validationMessage);
		}

		public void TestIsQueryValid_ShouldRejectQueryIfStandardIsNull()
		{
			var query = new SegregationQuery
			{
				Standards = null,
				Ids = new[] { Guid.NewGuid(), Guid.NewGuid() }
			};

			var validator = new SegregationQueryValidator();
			var (validationResult, validationMessage) = validator.IsQueryValid(query);
			AssertEquals("List of standards cannot be empty", validationMessage);
		}

		public void TestIsQueryValid_ShouldRejectQueryIfStandardIsEmpty()
		{
			var query = new SegregationQuery
			{
				Standards = new[] { string.Empty },
				Ids = new[] { Guid.NewGuid(), Guid.NewGuid() }
			};

			var validator = new SegregationQueryValidator();
			var (validationResult, validationMessage) = validator.IsQueryValid(query);
			AssertEquals("Standard cannot be empty", validationMessage);
		}

		public void TestIsQueryValid_ShouldRejectQueryIfStandardIsWhitespace()
		{
			var query = new SegregationQuery
			{
				Standards = new[] { " " },
				Ids = new[] { Guid.NewGuid(), Guid.NewGuid() }
			};

			var validator = new SegregationQueryValidator();
			var (validationResult, validationMessage) = validator.IsQueryValid(query);
			AssertEquals("Standard cannot be empty", validationMessage);
		}

		public void TestIsEntityQueryValid_ShouldRejectQueryIfStandardsAreInvalid()
		{
			var query = new SegregationEntityQuery
			{
				Standards = new[] { "invalidStandard" },
				UNDGDataItemDTOs = new[]
				{
					new UNDGDataItemDTO()
					{
						UNDGSubstanceDTOs = new[]
						{
							new UNDGSubstanceDTO { Unno = "1111", Variant = "vA", Standard = "IMO" },
							new UNDGSubstanceDTO { Unno = "1111", Variant = "vA", Standard = "IAT" },
						}
					},
					new UNDGDataItemDTO()
					{
						UNDGSubstanceDTOs = new[]
						{
							new UNDGSubstanceDTO { Unno = "2222", Variant = "vB", Standard = "IMO" },
							new UNDGSubstanceDTO { Unno = "2222", Variant = "vB", Standard = "IAT" },
						}
					},
				}
			};

			var validator = new SegregationQueryValidator();
			var (validationResult, validationMessage) = validator.IsEntityQueryValid(query);
			AssertEquals("The following standard specified in the query is not valid: invalidStandard", validationMessage);
		}

		public void TestIsEntityQueryValid_ShouldAcceptQueryIfStandardsAreValid()
		{
			var query = new SegregationEntityQuery
			{
				Standards = new[] { UNDGSubstanceStandardTypes.IMO, UNDGSubstanceStandardTypes.IATA },
				UNDGDataItemDTOs = new[]
				{
					new UNDGDataItemDTO()
					{
						UNDGSubstanceDTOs = new[]
						{
							new UNDGSubstanceDTO { Unno = "1111", Variant = "vA", Standard = "IMO" },
							new UNDGSubstanceDTO { Unno = "1111", Variant = "vA", Standard = "IAT" },
						}
					},
					new UNDGDataItemDTO()
					{
						UNDGSubstanceDTOs = new[]
						{
							new UNDGSubstanceDTO { Unno = "2222", Variant = "vB", Standard = "IMO" },
							new UNDGSubstanceDTO { Unno = "2222", Variant = "vB", Standard = "IAT" },
						}
					},
				}
			};

			var validator = new SegregationQueryValidator();
			var (validationResult, validationMessage) = validator.IsEntityQueryValid(query);
			AssertEquals(true, validationResult);
			AssertEquals(string.Empty, validationMessage);
		}

		public void TestIsEntityQueryValid_ShouldRejectQueryIfStandardsContainDuplicates()
		{
			var query = new SegregationEntityQuery
			{
				Standards = new[] { UNDGSubstanceStandardTypes.IMO, UNDGSubstanceStandardTypes.IMO },
				UNDGDataItemDTOs = new[]
				{
					new UNDGDataItemDTO()
					{
						UNDGSubstanceDTOs = new[]
						{
							new UNDGSubstanceDTO { Unno = "1111", Variant = "vA", Standard = "IMO" },
							new UNDGSubstanceDTO { Unno = "1111", Variant = "vA", Standard = "IAT" },
						}
					},
					new UNDGDataItemDTO()
					{
						UNDGSubstanceDTOs = new[]
						{
							new UNDGSubstanceDTO { Unno = "2222", Variant = "vB", Standard = "IMO" },
							new UNDGSubstanceDTO { Unno = "2222", Variant = "vB", Standard = "IAT" },
						}
					},
				}
			};

			var validator = new SegregationQueryValidator();
			var (validationResult, validationMessage) = validator.IsEntityQueryValid(query);
			AssertEquals("List of standards cannot contain duplicates", validationMessage);
		}

		public void TestIsEntityQueryValid_ShouldRejectQueryIfStandardsAreEmpty()
		{
			var query = new SegregationEntityQuery
			{
				Standards = Array.Empty<string>(),
				UNDGDataItemDTOs = new[]
				{
					new UNDGDataItemDTO()
					{
						UNDGSubstanceDTOs = new[]
						{
							new UNDGSubstanceDTO { Unno = "1111", Variant = "vA", Standard = "IMO" },
							new UNDGSubstanceDTO { Unno = "1111", Variant = "vA", Standard = "IAT" },
						}
					},
					new UNDGDataItemDTO()
					{
						UNDGSubstanceDTOs = new[]
						{
							new UNDGSubstanceDTO { Unno = "2222", Variant = "vB", Standard = "IMO" },
							new UNDGSubstanceDTO { Unno = "2222", Variant = "vB", Standard = "IAT" },
						}
					},
				}
			};

			var validator = new SegregationQueryValidator();
			var (validationResult, validationMessage) = validator.IsEntityQueryValid(query);
			AssertEquals("List of standards cannot be empty", validationMessage);
		}

		public void TestIsEntityQueryValid_ShouldRejectQueryIfUNDGDataItemDTOsAreEmpty()
		{
			var query = new SegregationEntityQuery
			{
				Standards = new[] { UNDGSubstanceStandardTypes.IMO, UNDGSubstanceStandardTypes.IATA },
				UNDGDataItemDTOs = Array.Empty<UNDGDataItemDTO>()
			};

			var validator = new SegregationQueryValidator();
			var (validationResult, validationMessage) = validator.IsEntityQueryValid(query);
			AssertEquals("List of UNDGDataItemDTOs cannot be empty", validationMessage);
		}

		public void TestIsEntityQueryValid_ShouldRejectQueryIfUNDGSubstanceDTOIsEmpty()
		{
			var query = new SegregationEntityQuery
			{
				Standards = new[] { UNDGSubstanceStandardTypes.IMO, UNDGSubstanceStandardTypes.IATA },
				UNDGDataItemDTOs = new[]
				{
					new UNDGDataItemDTO()
					{
						UNDGSubstanceDTOs = new[]
						{
							new UNDGSubstanceDTO { Unno = "1111", Variant = "vA", Standard = "IMO" },
							new UNDGSubstanceDTO { Unno = "1111", Variant = "vB", Standard = "IMO" },
						}
					},
					new UNDGDataItemDTO()
					{
						UNDGSubstanceDTOs = Array.Empty<UNDGSubstanceDTO>()
					},
				}
			};

			var validator = new SegregationQueryValidator();
			var (validationResult, validationMessage) = validator.IsEntityQueryValid(query);
			AssertEquals("Length of substance list cannot be less than length of standards list", validationMessage);
		}

		public void TestIsEntityQueryValid_ShouldRejectQueryIfUNDGSubstanceDTOContainsDuplicates()
		{
			var query = new SegregationEntityQuery
			{
				Standards = new[] { UNDGSubstanceStandardTypes.IMO, UNDGSubstanceStandardTypes.IATA },
				UNDGDataItemDTOs = new[]
				{
					new UNDGDataItemDTO()
					{
						UNDGSubstanceDTOs = new[]
						{
							new UNDGSubstanceDTO { Unno = "1111", Variant = "vA", Standard = "IMO" },
							new UNDGSubstanceDTO { Unno = "1111", Variant = "vB", Standard = "IMO" },
						}
					},
					new UNDGDataItemDTO()
					{
						UNDGSubstanceDTOs = new[]
						{
							new UNDGSubstanceDTO { Unno = "1111", Variant = "vA", Standard = "IAT" },
							new UNDGSubstanceDTO { Unno = "1111", Variant = "vA", Standard = "IAT" },
						}
					},
				}
			};

			var validator = new SegregationQueryValidator();
			var (validationResult, validationMessage) = validator.IsEntityQueryValid(query);
			AssertEquals("List of substances cannot contain duplicates", validationMessage);
		}

		public void TestIsEntityQueryValid_ShouldRejectQueryIfUNDGDataItemDTOsContainLessThanTwoEntries()
		{
			var query = new SegregationEntityQuery
			{
				Standards = new[] { UNDGSubstanceStandardTypes.IMO, UNDGSubstanceStandardTypes.IATA },
				UNDGDataItemDTOs = new[]
				{
					new UNDGDataItemDTO()
					{
						UNDGSubstanceDTOs = new[]
						{
							new UNDGSubstanceDTO { Unno = "1111", Variant = "vA", Standard = "IMO" },
							new UNDGSubstanceDTO { Unno = "1111", Variant = "vA", Standard = "IAT" },
						}
					},
				}
			};

			var validator = new SegregationQueryValidator();
			var (validationResult, validationMessage) = validator.IsEntityQueryValid(query);
			AssertEquals("List of UNDGDataItemDTOs must contain at least two entries", validationMessage);
		}

		public void TestIsEntityQueryValid_ShouldRejectQueryIfQueryIsNull()
		{
			var validator = new SegregationQueryValidator();
			var (validationResult, validationMessage) = validator.IsEntityQueryValid(null);
			AssertEquals("Query cannot be null", validationMessage);
		}

		public void TestIsEntityQueryValid_ShouldRejectQueryIfStandardIsNull()
		{
			var query = new SegregationEntityQuery
			{
				Standards = null,
				UNDGDataItemDTOs = new[]
				{
					new UNDGDataItemDTO()
					{
						UNDGSubstanceDTOs = new[]
						{
							new UNDGSubstanceDTO { Unno = "1111", Variant = "vA", Standard = "IMO" },
							new UNDGSubstanceDTO { Unno = "1111", Variant = "vA", Standard = "IAT" },
						}
					},
					new UNDGDataItemDTO()
					{
						UNDGSubstanceDTOs = new[]
						{
							new UNDGSubstanceDTO { Unno = "2222", Variant = "vB", Standard = "IMO" },
							new UNDGSubstanceDTO { Unno = "2222", Variant = "vB", Standard = "IAT" },
						}
					},
				}
			};

			var validator = new SegregationQueryValidator();
			var (validationResult, validationMessage) = validator.IsEntityQueryValid(query);
			AssertEquals("List of standards cannot be empty", validationMessage);
		}

		public void TestIsEntityQueryValid_ShouldRejectQueryIfStandardIsEmpty()
		{
			var query = new SegregationEntityQuery
			{
				Standards = new[] { string.Empty },
				UNDGDataItemDTOs = new[]
				{
					new UNDGDataItemDTO()
					{
						UNDGSubstanceDTOs = new[]
						{
							new UNDGSubstanceDTO { Unno = "1111", Variant = "vA", Standard = "IMO" },
							new UNDGSubstanceDTO { Unno = "1111", Variant = "vA", Standard = "IAT" },
						}
					},
					new UNDGDataItemDTO()
					{
						UNDGSubstanceDTOs = new[]
						{
							new UNDGSubstanceDTO { Unno = "2222", Variant = "vB", Standard = "IMO" },
							new UNDGSubstanceDTO { Unno = "2222", Variant = "vB", Standard = "IAT" },
						}
					},
				}
			};

			var validator = new SegregationQueryValidator();
			var (validationResult, validationMessage) = validator.IsEntityQueryValid(query);
			AssertEquals("Standard cannot be empty", validationMessage);
		}

		public void TestIsEntityQueryValid_ShouldRejectQueryIfStandardIsWhitespace()
		{
			var query = new SegregationEntityQuery
			{
				Standards = new[] { " " },
				UNDGDataItemDTOs = new[]
				{
					new UNDGDataItemDTO()
					{
						UNDGSubstanceDTOs = new[]
						{
							new UNDGSubstanceDTO { Unno = "1111", Variant = "vA", Standard = "IMO" },
							new UNDGSubstanceDTO { Unno = "1111", Variant = "vA", Standard = "IAT" },
						}
					},
					new UNDGDataItemDTO()
					{
						UNDGSubstanceDTOs = new[]
						{
							new UNDGSubstanceDTO { Unno = "2222", Variant = "vB", Standard = "IMO" },
							new UNDGSubstanceDTO { Unno = "2222", Variant = "vB", Standard = "IAT" },
						}
					},
				}
			};

			var validator = new SegregationQueryValidator();
			var (validationResult, validationMessage) = validator.IsEntityQueryValid(query);
			AssertEquals("Standard cannot be empty", validationMessage);
		}
	}
}
