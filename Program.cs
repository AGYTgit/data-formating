using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

class Program {
    static void Main(string[] args)
    {
        if (args.Length == 0) {
            Console.WriteLine("Please provide a file name as an argument.");
            return;
        }

        string fileContent = parseFileToString(args[0]);

        List<Product> productList = Product.parseData(fileContent);

        Console.WriteLine("Produkty:");
        foreach (Product p in productList) {
            if (p.count == null) {
                Console.WriteLine($"{p.name}: neznáme množstvo €, {p.price} €");
            } else {
                Console.WriteLine($"{p.name}: {p.count} ks, {p.price} €");
            }
        }
		Console.WriteLine();
		Console.WriteLine($"Celková cena produktov: {Product.getTotalProductsPrice(productList)} €");
		Console.WriteLine($"Priemerná váha položky: {Product.getAverageItemWeight(productList)} kg");
    }

    static string parseFileToString(string fp) {
        string filePath = fp;

        if (!File.Exists(filePath)) {
            Console.WriteLine($"File '{filePath}' not found.");
            return "";
        }

        return File.ReadAllText(filePath);
    }

    public enum WeightUnit {g, dkg, kg}

    public struct Weight {
        public double value { get; }
        public WeightUnit Unit { get; }

        public Weight(double _value, WeightUnit unit) {
            value = _value;
            Unit = unit;
        }
    }

    class Product {
        public string name { get; }
        public double price { get; }
        public int? count { get; }
        public Weight weight { get; }

        public Product(string _name, double _price, int? _count, Weight _weight) {
            name = _name;
            price = _price;
            count = _count;
            weight = _weight;
        }

        public static List<Product> parseData(string fc) {
            List<string> lineList = fc.Split('\n').ToList();
            lineList = lineList.Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
            int lineIndex = 0;

            if (lineList[lineIndex].IndexOf("products:") == -1) {
                throw new Exception("Error: \"products:\" must be on first line");
            }
            lineIndex++;

            List<Product> productList = new List<Product>();

            while (lineIndex < lineList.Count) {
                lineList[lineIndex] = lineList[lineIndex].Trim();
                string name = parseName(lineList[lineIndex], lineIndex);
                lineIndex++;

                double price = parsePrice(lineList[lineIndex], lineIndex);
                lineIndex++;

                int? count = parseQuantity(lineList[lineIndex], lineIndex);
                if (count != null) {
                    lineIndex++;
                }

                Weight weight = parseWeight(lineList[lineIndex], lineIndex);
                lineIndex++;

                productList.Add(new Product(name, price, count, weight));
            }

            return productList;
        }

        private static string parseName(string line, int lineIndex) {
            int dashIndex = line.IndexOf('-');
            int colonIndex = line.IndexOf(':');

            if (dashIndex == -1) {
                throw new Exception($"Error: Line({lineIndex}): \'-\' must preceede each product name");
            }
            if (colonIndex == -1) {
                throw new Exception($"Error: Line({lineIndex}): \':\' must follow each product name");
            }

            return line.Substring(dashIndex + 2, colonIndex - dashIndex - 2).Trim();
        }

        private static double parsePrice(string line, int lineIndex) {
            int dashIndex = line.IndexOf('-');
            int priceIndex = line.IndexOf("price", dashIndex);
            int colonIndex = line.IndexOf(':', priceIndex);

            double price;

            if (dashIndex == -1) {
                throw new Exception($"Error: Line({lineIndex}): \'-\' must preceede each product price");
            }
            if (priceIndex == -1) {
                throw new Exception($"Error: Line({lineIndex}): \"price\" not fount");
            }
            if (colonIndex == -1) {
                throw new Exception($"Error: Line({lineIndex}): \':\' must follow each product price");
            }

            string priceStr = line.Substring(colonIndex + 1).Trim();
            if (!double.TryParse(priceStr, out price)) {
                throw new Exception($"Error: Line({lineIndex}): {priceStr} is not a valid double");
            }

            return price;
        }

        private static int? parseQuantity(string line, int lineIndex) {
            int dashIndex = line.IndexOf('-');
            int? quantityIndex = line.IndexOf("quantity", dashIndex);
            if (quantityIndex == -1) {
                return null;
            }
            int colonIndex = line.IndexOf(':', (int)quantityIndex);

            int quantity;

            if (dashIndex == -1) {
                throw new Exception($"Error: Line({lineIndex}): \'-\' must preceede each product quantity");
            }
            if (colonIndex == -1) {
                throw new Exception($"Error: Line({lineIndex}): \':\' must follow each product quantity");
            }

            string quantityStr = line.Substring(colonIndex + 1).Trim();
            if (!int.TryParse(quantityStr, out quantity)) {
                throw new Exception($"Error: Line({lineIndex}): {quantityStr} is not a valid int");
            }

            return quantity;
        }

        private static Weight parseWeight(string line, int lineIndex) {
            int dashIndex = line.IndexOf('-');
            int weightIndex = line.IndexOf("weight", dashIndex);
            int colonIndex = line.IndexOf(':', weightIndex);

            if (dashIndex == -1) {
                throw new Exception($"Error: Line({lineIndex}): \'-\' must preceede each product weight");
            }
            if (weightIndex == -1) {
                throw new Exception($"Error: Line({lineIndex}): \"weight\" not found");
            }
            if (colonIndex == -1) {
                throw new Exception($"Error: Line({lineIndex}): \':\' must follow each product weight");
            }

            string weightStr = line.Substring(colonIndex + 1).Trim();
            string[] weightParts = weightStr.Split(' ');
            if (weightParts.Length != 2) {
                throw new Exception($"Error: Line({lineIndex}): Invalid weight format");
            }

            if (!double.TryParse(weightParts[0], out double weightValue)) {
                throw new Exception($"Error: Line({lineIndex}): {weightParts[0]} is not a valid number");
            }

            if (!Enum.TryParse(weightParts[1], true, out WeightUnit unit)) {
                throw new Exception($"Error: Line({lineIndex}): Invalid weight unit");
            }

            return new Weight(weightValue, unit);
        }

        public static double getTotalProductsPrice(List<Product> productList) {
			double totalPrice = 0;

			foreach (Product p in productList) {
				if (p.count != null) {
					totalPrice += p.price * (double)p.count;
				}
			}

			return totalPrice;
		}
        
		public static double getAverageItemWeight(List<Product> productList) {
			double totalWeight = 0;
			int totalProducts = 0;

			foreach (Product p in productList) {
				double weightInKg = 0;

				weightInKg = getNormalizedValue(p);
				totalWeight += weightInKg;
				totalProducts++;
			}

			if (totalProducts == 0) {
				return 0;
			}

			return Math.Round(totalWeight / totalProducts, 3);
		}

		private static double getNormalizedValue(Product p) {
			double weightInKg = 0;

			switch (p.weight.Unit) {
			case WeightUnit.g:
				weightInKg = p.weight.value / 1000;
				break;
			case WeightUnit.dkg:
				weightInKg = p.weight.value / 100;
				break;
			case WeightUnit.kg:
				weightInKg = p.weight.value;
				break;
			}

			return weightInKg;
		}
    }
}
