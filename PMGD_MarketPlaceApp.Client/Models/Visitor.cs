namespace PMGD_MarketPlaceApp.Client.Models {
    public class Visitor {
        public int Uid { get; set; }
        public string Npm { get; set; }
        public string Email { get; set; }
        public University University { get; set; } = new();
        public Departement Departement { get; set; } = new();
    }
}
