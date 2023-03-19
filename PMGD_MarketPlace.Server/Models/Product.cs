namespace PMGD_MarketPlace.Server.Models {
    public class Product {
        public int Uid { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public Account Account { get; set; } = new();
        public string Filename { get; set; }
        public string Size { get; set; }
        public int Counter { get; set; }
        public int Like { get; set; }
    }
}
