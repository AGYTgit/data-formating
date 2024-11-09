Console.OutputEncoding = System.Text.Encoding.UTF8;

string data = File.ReadAllText("./data.txt");

Product[] products = Product.ParseData(data);

// print formated data



Product p = new("naame", 1, 1, 1);

Console.WriteLine(p.name);


public enum weight_unit {
	k,
	dkg,
	kg
}

public struct str_weight(weight_unit _unit, double _weight) {
	weight_unit unit = _unit;
	double weight = _weight;
}

public class Product(string _name, double _price, int? _count, str_weight _weight) {
	public string name       { get; } = _name;
	public double price      { get; } = _price;
	public int? count        { get; } = _count;
	public str_weight weight { get; } = _weight;

	
		

	
	public static Product[] ParseData(string d) {
		return [];
	}

	public static double GetTotalProductsPrice(Product[] p) {
		return 0;
	}

	public static double GetAverageItemWeight(Product[] p) {
		return 0;
	}
}
