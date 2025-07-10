using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Container = Enterprise.UniversalDataBuss.DataObjects.Universal.Container;
using PackingLine = Enterprise.UniversalDataBuss.DataObjects.Universal.PackingLine;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class ForwardingLibraryTest : TestCaseWithFactory
	{
		#region PackingLines

		public void TestGetPackingLinesForContainer_STD()
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.SetContainerCollection(() => new DataObjectList<Container>
				{
					new Container
					{
						Link = 1
					},
					new Container
					{
						Link = 2
					}
				});
			var innerShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ShipmentType = new CodeDescriptionPair
				{
					Code = "STD"
				}
			};
			innerShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>
						{
							new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
							{
								ContainerLink = 1,
								MarksAndNos = "AAA"
							},
							new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
							{
								ContainerLink = 1,
								MarksAndNos = "BBB"
							},
							new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
							{
								ContainerLink = 2,
								MarksAndNos = "CCC"
							},
							new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
							{
								MarksAndNos = "DDD"
							}
						});
			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>
				{
					innerShipment
				});

			var dynamicShipment = shipment.MakeDynamic();
			var dynamicContainers = (IDynamicDataCollection)dynamicShipment.Properties.GetOrCreate("ContainerCollection");
			var dynamicContainer1 = dynamicContainers.ElementAt(0);
			var dynamicContainer2 = dynamicContainers.ElementAt(1);

			using (var scope = new MacroScope(dynamicShipment))
			{
				var expr = "PackingLines.Format({MarksAndNos})"
					.With(Context)
					.CreateExpression();

				var result = expr.Evaluate(new MacroScope(scope, dynamicContainer1));

				AssertMultilineASCIIEquals("expect no errors", "", string.Join("\r\n", expr.Errors.Select(err => err.Message)));
				AssertEquals("eval result", "AAA, BBB", result);
			}

			using (var scope = new MacroScope(dynamicShipment))
			{
				var expr = "PackingLines.Format({MarksAndNos})"
					.With(Context)
					.CreateExpression();

				var result = expr.Evaluate(new MacroScope(scope, dynamicContainer2));

				AssertMultilineASCIIEquals("expect no errors", "", string.Join("\r\n", expr.Errors.Select(err => err.Message)));
				AssertEquals("eval result", "CCC", result);
			}

			using (var scope = new MacroScope(dynamicShipment))
			{
				var expr = "PackingLines.Format({MarksAndNos})"
					.With(Context)
					.CreateExpression();

				var result = expr.Evaluate(new MacroScope(scope));

				AssertMultilineASCIIEquals("expect no errors", "", string.Join("\r\n", expr.Errors.Select(err => err.Message)));
				AssertEquals("eval result", "AAA, BBB, CCC, DDD", result);
			}

			using (var scope = new MacroScope(dynamicShipment))
			{
				var expr = "PackingLines.Where({ContainerLink.IsNoneOrEmpty}).Format({MarksAndNos})"
					.With(Context)
					.CreateExpression();

				var result = expr.Evaluate(new MacroScope(scope));

				AssertMultilineASCIIEquals("expect no errors", "", string.Join("\r\n", expr.Errors.Select(err => err.Message)));
				AssertEquals("eval result", "DDD", result);
			}
		}

		public void TestGetPackingLinesForContainer_CLD()
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.SetContainerCollection(() => new DataObjectList<Container>
				{
					new Container
					{
						Link = 1
					},
					new Container
					{
						Link = 2
					}
				});

			var shippy1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ShipmentType = new CodeDescriptionPair
				{
					Code = "CLD"
				},
			};
			shippy1.SetPackingLineCollection(() => new DataObjectList<PackingLine>
						{
							new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
							{
								ContainerLink = 1,
								MarksAndNos = "AAA"
							},
							new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
							{
								ContainerLink = 1,
								MarksAndNos = "BBB"
							},
							new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
							{
								ContainerLink = 2,
								MarksAndNos = "CCC"
							},
							new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
							{
								MarksAndNos = "GGG"
							}
						});
			var shippy1Inner1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ShipmentType = new CodeDescriptionPair
				{
					Code = "STD"
				}
			};
			shippy1Inner1.SetPackingLineCollection(() => new DataObjectList<PackingLine>
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					ContainerLink = 1,
					MarksAndNos = "AAA"
				},
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					MarksAndNos = "GGG"
				}
			});
			var shippy1Inner2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ShipmentType = new CodeDescriptionPair
				{
					Code = "STD"
				}
			};
			shippy1Inner2.SetPackingLineCollection(() => new DataObjectList<PackingLine>
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					ContainerLink = 1,
					MarksAndNos = "BBB"
				},
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					ContainerLink = 2,
					MarksAndNos = "CCC"
				}
			});
			shippy1.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>
			{
				shippy1Inner1,
				shippy1Inner2
			});

			var shippy2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ShipmentType = new CodeDescriptionPair
				{
					Code = "STD"
				},
			};
			shippy2.SetPackingLineCollection(() => new DataObjectList<PackingLine>
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					ContainerLink = 1,
					MarksAndNos = "DDD"
				},
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					ContainerLink = 2,
					MarksAndNos = "EEE"
				},
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					ContainerLink = 3,
					MarksAndNos = "FFF"
				},
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					MarksAndNos = "HHH"
				}
			});
			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { shippy1, shippy2 });

			var dynamicShipment = shipment.MakeDynamic();
			var dynamicContainers = (IDynamicDataCollection)dynamicShipment.Properties.GetOrCreate("ContainerCollection");
			var dynamicContainer1 = dynamicContainers.ElementAt(0);

			using (var scope = new MacroScope(dynamicShipment))
			{
				var expr = "PackingLines.Format({MarksAndNos})"
					.With(Context)
					.CreateExpression();

				var result = expr.Evaluate(new MacroScope(scope, dynamicContainer1));

				AssertMultilineASCIIEquals("expect no errors", "", string.Join("\r\n", expr.Errors.Select(err => err.Message)));
				AssertEquals("eval result for 1st container", "AAA, BBB, DDD", result);
			}

			var dynamicContainer2 = dynamicContainers.ElementAt(1);

			using (var scope = new MacroScope(dynamicShipment))
			{
				var expr = "PackingLines.Format({MarksAndNos})"
					.With(Context)
					.CreateExpression();

				var result = expr.Evaluate(new MacroScope(scope, dynamicContainer2));

				AssertMultilineASCIIEquals("expect no errors", "", string.Join("\r\n", expr.Errors.Select(err => err.Message)));
				AssertEquals("eval result for 2nd container", "CCC, EEE", result);
			}

			using (var scope = new MacroScope(dynamicShipment))
			{
				var expr = "PackingLines.Format({MarksAndNos})"
					.With(Context)
					.CreateExpression();

				var result = expr.Evaluate(new MacroScope(scope));

				AssertMultilineASCIIEquals("expect no errors", "", string.Join("\r\n", expr.Errors.Select(err => err.Message)));
				AssertEquals("eval result", "AAA, GGG, BBB, CCC, DDD, EEE, FFF, HHH", result);
			}

			using (var scope = new MacroScope(dynamicShipment))
			{
				var expr = "PackingLines.Where({ContainerLink.IsNoneOrEmpty}).Format({MarksAndNos})"
					.With(Context)
					.CreateExpression();

				var result = expr.Evaluate(new MacroScope(scope));

				AssertMultilineASCIIEquals("expect no errors", "", string.Join("\r\n", expr.Errors.Select(err => err.Message)));
				AssertEquals("eval result", "GGG, HHH", result);
			}
		}

		public void TestGetPackingLinesForContainer_BCN()
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.SetContainerCollection(() => new DataObjectList<Container>
				{
					new Container
					{
						Link = 1
					},
					new Container
					{
						Link = 2
					}
				});
			var shippy = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ShipmentType = new CodeDescriptionPair
				{
					Code = "BCN"
				},
			};
			shippy.SetPackingLineCollection(() => new DataObjectList<PackingLine>
			{
				new PackingLine
				{
					ContainerLink = 1,
					MarksAndNos = "AAA"
				},
				new PackingLine
				{
					ContainerLink = 1,
					MarksAndNos = "BBB"
				}
			});
			var shippyInner1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ShipmentType = new CodeDescriptionPair
				{
					Code = "STD"
				},
			};
			shippyInner1.SetPackingLineCollection(() => new DataObjectList<PackingLine>
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					ContainerLink = 1,
					MarksAndNos = "CCC"
				},
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					MarksAndNos = "DDD"
				}
			});
			var shippyInner2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ShipmentType = new CodeDescriptionPair
				{
					Code = "STD"
				},
			};
			shippyInner2.SetPackingLineCollection(() => new DataObjectList<PackingLine>
			{
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					ContainerLink = 2,
					MarksAndNos = "EEE"
				},
				new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				{
					MarksAndNos = "FFF"
				}
			});
			shippy.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>
			{
				shippyInner1,
				shippyInner2
			});
			var shippy2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ShipmentType = new CodeDescriptionPair
				{
					Code = "STD"
				},
			};
			shippy2.SetPackingLineCollection(() => new DataObjectList<PackingLine>
					{
						new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
						{
							ContainerLink = 1,
							MarksAndNos = "GGG"
						},
						new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
						{
							ContainerLink = 2,
							MarksAndNos = "HHH"
						},
						new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
						{
							ContainerLink = 2,
							MarksAndNos = "III"
						},
						new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
						{
							MarksAndNos = "JJJ"
						}
					});
			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>
			{
				shippy,
				shippy2
			});

			var dynamicShipment = shipment.MakeDynamic();
			var dynamicContainers = (IDynamicDataCollection)dynamicShipment.Properties.GetOrCreate("ContainerCollection");
			var dynamicContainer1 = dynamicContainers.ElementAt(0);

			using (var scope = new MacroScope(dynamicShipment))
			{
				var expr = "PackingLines.Format({MarksAndNos})"
					.With(Context)
					.CreateExpression();

				var result = expr.Evaluate(new MacroScope(scope, dynamicContainer1));

				AssertMultilineASCIIEquals("expect no errors", "", string.Join("\r\n", expr.Errors.Select(err => err.Message)));
				AssertEquals("eval result for 1st container", "AAA, BBB, CCC, GGG", result);
			}

			var dynamicContainer2 = dynamicContainers.ElementAt(1);

			using (var scope = new MacroScope(dynamicShipment))
			{
				var expr = "PackingLines.Format({MarksAndNos})"
					.With(Context)
					.CreateExpression();

				var result = expr.Evaluate(new MacroScope(scope, dynamicContainer2));

				AssertMultilineASCIIEquals("expect no errors", "", string.Join("\r\n", expr.Errors.Select(err => err.Message)));
				AssertEquals("eval result for 2nd container", "EEE, HHH, III", result);
			}

			using (var scope = new MacroScope(dynamicShipment))
			{
				var expr = "PackingLines.Format({MarksAndNos})"
					.With(Context)
					.CreateExpression();

				var result = expr.Evaluate(new MacroScope(scope));

				AssertMultilineASCIIEquals("expect no errors", "", string.Join("\r\n", expr.Errors.Select(err => err.Message)));
				AssertEquals("eval result", "AAA, BBB, CCC, DDD, EEE, FFF, GGG, HHH, III, JJJ", result);
			}

			using (var scope = new MacroScope(dynamicShipment))
			{
				var expr = "PackingLines.Where({ContainerLink.IsNoneOrEmpty}).Format({MarksAndNos})"
					.With(Context)
					.CreateExpression();

				var result = expr.Evaluate(new MacroScope(scope));

				AssertMultilineASCIIEquals("expect no errors", "", string.Join("\r\n", expr.Errors.Select(err => err.Message)));
				AssertEquals("eval result", "DDD, FFF, JJJ", result);
			}
		}

		public void TestGetPackingLinesForContainer_3PT()
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SetContainerCollection(() => new DataObjectList<Container>
				{
					new Container
					{
						Link = 1
					},
					new Container
					{
						Link = 2
					}
				});
			var innerShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ShipmentType = new CodeDescriptionPair
				{
					Code = "3PT"
				}
			};
			innerShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>
					{
						new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
						{
							ContainerLink = 1,
							MarksAndNos = "AAA"
						},
						new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
						{
							ContainerLink = 1,
							MarksAndNos = "BBB"
						},
						new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
						{
							ContainerLink = 2,
							MarksAndNos = "CCC"
						},
						new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
						{
							MarksAndNos = "DDD"
						}
					});
			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>
			{
				innerShipment
			});

			var dynamicShipment = shipment.MakeDynamic();
			var dynamicContainers = (IDynamicDataCollection)dynamicShipment.Properties.GetOrCreate("ContainerCollection");
			var dynamicContainer1 = dynamicContainers.ElementAt(0);
			var dynamicContainer2 = dynamicContainers.ElementAt(1);

			using (var scope = new MacroScope(dynamicShipment))
			{
				var expr = "PackingLines.Format({MarksAndNos})"
					.With(Context)
					.CreateExpression();

				var result = expr.Evaluate(new MacroScope(scope, dynamicContainer1));

				AssertMultilineASCIIEquals("expect no errors", "", string.Join("\r\n", expr.Errors.Select(err => err.Message)));
				AssertEquals("eval result", "AAA, BBB", result);
			}

			using (var scope = new MacroScope(dynamicShipment))
			{
				var expr = "PackingLines.Format({MarksAndNos})"
					.With(Context)
					.CreateExpression();

				var result = expr.Evaluate(new MacroScope(scope, dynamicContainer2));

				AssertMultilineASCIIEquals("expect no errors", "", string.Join("\r\n", expr.Errors.Select(err => err.Message)));
				AssertEquals("eval result", "CCC", result);
			}

			using (var scope = new MacroScope(dynamicShipment))
			{
				var expr = "PackingLines.Format({MarksAndNos})"
					.With(Context)
					.CreateExpression();

				var result = expr.Evaluate(new MacroScope(scope));

				AssertMultilineASCIIEquals("expect no errors", "", string.Join("\r\n", expr.Errors.Select(err => err.Message)));
				AssertEquals("eval result", "AAA, BBB, CCC, DDD", result);
			}

			using (var scope = new MacroScope(dynamicShipment))
			{
				var expr = "PackingLines.Where({ContainerLink.IsNoneOrEmpty}).Format({MarksAndNos})"
					.With(Context)
					.CreateExpression();

				var result = expr.Evaluate(new MacroScope(scope));

				AssertMultilineASCIIEquals("expect no errors", "", string.Join("\r\n", expr.Errors.Select(err => err.Message)));
				AssertEquals("eval result", "DDD", result);
			}
		}

		public void TestPackingLinesForShipment()
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ShipmentType = new CodeDescriptionPair
				{
					Code = "STD"
				},
			};

			var subShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ShipmentType = new CodeDescriptionPair
				{
					Code = "STD"
				},
			};
			subShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>
					{
						new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
						{
							ContainerLink = 1,
							MarksAndNos = "BBB"
						},
						new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
						{
							ContainerLink = 1,
							MarksAndNos = "CCC"
						}
					});

			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>
					{
						new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
						{
							ContainerLink = 1,
							MarksAndNos = "AAA"
						},
					});

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { subShipment });

			var dynamicShipment = shipment.MakeDynamic();

			using (var scope = new MacroScope(dynamicShipment))
			{
				var expr = "SubShipmentCollection.ElementAt(0).PackingLines.Format({MarksAndNos})"
					.With(Context)
					.CreateExpression();

				var result = expr.Evaluate(new MacroScope(scope));

				AssertMultilineASCIIEquals("expect no errors", "", string.Join("\r\n", expr.Errors.Select(err => err.Message)));
				AssertEquals("eval result", "BBB, CCC", result);
			}
		}

		#endregion

		#region UniversalShipment

		public void TestShipment_STD()
		{
			var shipment1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AgentsReference = "shipment#1",
				ShipmentType = new CodeDescriptionPair
				{
					Code = "STD"
				},
			};
			shipment1.SetPackingLineCollection(() => new DataObjectList<PackingLine>
					{
						new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
					});

			var shipment2 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AgentsReference = "shipment#2",
				ShipmentType = new CodeDescriptionPair
				{
					Code = "STD"
				},
			};
			shipment2.SetPackingLineCollection(() => new DataObjectList<PackingLine>
					{
						new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
					});

			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { shipment1, shipment2 });

			var dynamicConsol = consol.MakeDynamic();

			var dynamicShipments = (IDynamicDataCollection)dynamicConsol.GetDynamicProperty("SubShipmentCollection");

			var dynamicShipment1 = dynamicShipments.First();
			var dynamicShipment2 = dynamicShipments.Skip(1).First();

			var dynamicPackingLine1 = ((IDynamicDataCollection)dynamicShipment1.GetDynamicProperty("PackingLineCollection")).First();
			var dynamicPackingLine2 = ((IDynamicDataCollection)dynamicShipment2.GetDynamicProperty("PackingLineCollection")).First();

			using (var scope = new MacroScope(dynamicConsol))
			{
				var expr = "Shipment.AgentsReference"
					.With(Context)
					.CreateExpression();

				var result = expr.Evaluate(new MacroScope(scope, dynamicPackingLine1)) as IDynamicData;

				AssertMultilineASCIIEquals("expect no errors", "", string.Join("\r\n", expr.Errors.Select(err => err.Message)));
				AssertEquals("eval result", "shipment#1", result.Value);
			}

			using (var scope = new MacroScope(dynamicConsol))
			{
				var expr = "Shipment.AgentsReference"
					.With(Context)
					.CreateExpression();

				var result = expr.Evaluate(new MacroScope(scope, dynamicPackingLine2)) as IDynamicData;

				AssertMultilineASCIIEquals("expect no errors", "", string.Join("\r\n", expr.Errors.Select(err => err.Message)));
				AssertEquals("eval result", "shipment#2", result.Value);
			}
		}

		public void TestShipment_BCN()
		{
			var shipment1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AgentsReference = "BCN",
				ShipmentType = new CodeDescriptionPair
				{
					Code = "BCN"
				},
			};
			var innerShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AgentsReference = "STD",
				ShipmentType = new CodeDescriptionPair
				{
					Code = "STD"
				}
			};
			innerShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>
					{
						new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
					});
			shipment1.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>
			{
				innerShipment
			});

			shipment1.SetPackingLineCollection(() => new DataObjectList<PackingLine> { new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) });

			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { shipment1 });

			var dynamicConsol = consol.MakeDynamic();

			var dynamicShipments = (IDynamicDataCollection)dynamicConsol.GetDynamicProperty("SubShipmentCollection");

			var dynamicShipmentBCN = dynamicShipments.First();

			var dynamicSubShipments = (IDynamicDataCollection)dynamicShipmentBCN.GetDynamicProperty("SubShipmentCollection");
			var dynamicSubShipmentSTD = dynamicSubShipments.First();

			var dynamicPackingLineBCN = ((IDynamicDataCollection)dynamicShipmentBCN.GetDynamicProperty("PackingLineCollection")).First();
			var dynamicPackingLineSTD = ((IDynamicDataCollection)dynamicSubShipmentSTD.GetDynamicProperty("PackingLineCollection")).First();

			using (var scope = new MacroScope(dynamicConsol))
			{
				var expr = "Shipment.AgentsReference"
					.With(Context)
					.CreateExpression();

				var result = expr.Evaluate(new MacroScope(scope, dynamicPackingLineBCN)) as IDynamicData;

				AssertMultilineASCIIEquals("expect no errors", "", string.Join("\r\n", expr.Errors.Select(err => err.Message)));
				AssertEquals("eval result", "BCN", result.Value);
			}

			using (var scope = new MacroScope(dynamicConsol))
			{
				var expr = "Shipment.AgentsReference"
					.With(Context)
					.CreateExpression();

				var result = expr.Evaluate(new MacroScope(scope, dynamicPackingLineSTD)) as IDynamicData;

				AssertMultilineASCIIEquals("expect no errors", "", string.Join("\r\n", expr.Errors.Select(err => err.Message)));
				AssertEquals("eval result", "STD", result.Value);
			}
		}

		public void TestShipment_ASM()
		{
			var shipment1 = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AgentsReference = "ASM",
				ShipmentType = new CodeDescriptionPair
				{
					Code = "ASM"
				},
			};
			var innerShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AgentsReference = "STD",
				ShipmentType = new CodeDescriptionPair
				{
					Code = "STD"
				},
			};
			innerShipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>
				{
					new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
				});
			shipment1.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { innerShipment });

			shipment1.SetPackingLineCollection(() => new DataObjectList<PackingLine> { new PackingLine(DefaultDataObjectWriterStrategy.TestInstance) });

			var consol = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment> { shipment1 });

			var dynamicConsol = consol.MakeDynamic();

			var dynamicShipments = (IDynamicDataCollection)dynamicConsol.GetDynamicProperty("SubShipmentCollection");

			var dynamicShipmentASM = dynamicShipments.First();

			var dynamicSubShipments = (IDynamicDataCollection)dynamicShipmentASM.GetDynamicProperty("SubShipmentCollection");
			var dynamicSubShipmentSTD = dynamicSubShipments.First();

			var dynamicPackingLineASM = ((IDynamicDataCollection)dynamicShipmentASM.GetDynamicProperty("PackingLineCollection")).First();
			var dynamicPackingLineSTD = ((IDynamicDataCollection)dynamicSubShipmentSTD.GetDynamicProperty("PackingLineCollection")).First();

			using (var scope = new MacroScope(dynamicConsol))
			{
				var expr = "Shipment.AgentsReference"
					.With(Context)
					.CreateExpression();

				var result = expr.Evaluate(new MacroScope(scope, dynamicPackingLineASM)) as IDynamicData;

				AssertMultilineASCIIEquals("expect no errors", "", string.Join("\r\n", expr.Errors.Select(err => err.Message)));
				AssertEquals("eval result", "ASM", result.Value);
			}

			using (var scope = new MacroScope(dynamicConsol))
			{
				var expr = "Shipment.AgentsReference"
					.With(Context)
					.CreateExpression();

				var result = expr.Evaluate(new MacroScope(scope, dynamicPackingLineSTD)) as IDynamicData;

				AssertMultilineASCIIEquals("expect no errors", "", string.Join("\r\n", expr.Errors.Select(err => err.Message)));
				AssertEquals("eval result", "STD", result.Value);
			}
		}

		#endregion

		#region Container

		public void TestGetContainerForPackingLine_FromContainerCollection()
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.SetContainerCollection(() => new DataObjectList<Container>
				{
					new Container
					{
						Link = 1,
						ContainerNumber = "C000001"
					},
					new Container
					{
						Link = 2,
						ContainerNumber = "C000002"
					}
				});

			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>
				{
					new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
					{
						ContainerLink = 1,
					},
					new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
					{
						ContainerLink = 2,
					}
				});

			var dynamicShipment = shipment.MakeDynamic();

			AssertContainerResult(dynamicShipment, 0, "C000001");
			AssertContainerResult(dynamicShipment, 1, "C000002");
		}

		public void TestGetContainerForPackingLine_FromRelatedShipments()
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>
				{
					new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
					{
						ContainerLink = 1,
					},
					new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
					{
						ContainerLink = 2,
					}
				});

			var subshipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>
				{
					subshipment
				});

			subshipment.SetContainerCollection(() => new DataObjectList<Container>
				{
					new Container
					{
						Link = 1,
						ContainerNumber = "C000001"
					},
				});

			var parentShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.SetParentShipmentCollection(() => new List<UniversalShipment>
				{
					parentShipment
				});

			parentShipment.SetContainerCollection(() => new DataObjectList<Container>
				{
					new Container
					{
						Link = 2,
						ContainerNumber = "C000002"
					},
				});

			var dynamicShipment = shipment.MakeDynamic();

			AssertContainerResult(dynamicShipment, 0, "C000001");
			AssertContainerResult(dynamicShipment, 1, "C000002");
		}

		public void TestGetContainerForPackingLine_FromRecursive()
		{
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.SetPackingLineCollection(() => new DataObjectList<PackingLine>
				{
					new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
					{
						ContainerLink = 1,
					},
					new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
					{
						ContainerLink = 2,
					}
				});

			var subshipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>
				{
					subshipment
				});

			var thirdShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			subshipment.SetSubShipmentCollection(() => new DataObjectList<UniversalShipment>
				{
					thirdShipment
				});

			thirdShipment.SetContainerCollection(() => new DataObjectList<Container>
				{
					new Container
					{
						Link = 1,
						ContainerNumber = "C000001"
					},
					new Container
					{
						Link = 2,
						ContainerNumber = "C000002"
					}
				});

			var dynamicShipment = shipment.MakeDynamic();

			AssertContainerResult(dynamicShipment, 0, "C000001");
			AssertContainerResult(dynamicShipment, 1, "C000002");
		}

		void AssertContainerResult(IDynamicData shipmentData, int packingLineIndex, string expectResult)
		{
			using (var scope = new MacroScope(shipmentData))
			{
				var macro = string.Format("\"<PackingLineCollection.ElementAt({0}).Container.ContainerNumber>\"", packingLineIndex);

				var expr = macro
					.With(Context)
					.CreateExpression();

				var result = expr.Evaluate(scope);

				AssertMultilineASCIIEquals("expect no errors", string.Empty, string.Join("\r\n", expr.Errors.Select(err => err.Message)));
				AssertEquals("eval result", expectResult, result);
			}
		}

		#endregion

		IMacroEvaluationContext Context
		{
			get
			{
				if (context == null)
				{
					context = new IMacroLibrary[]
					{
						new StandardLibrary(),
						new ForwardingLibrary()
					}
					.CreateContext();
				}

				return context;
			}
		}
		IMacroEvaluationContext context;
	}
}
