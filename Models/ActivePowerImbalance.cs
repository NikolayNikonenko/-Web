namespace перенос_бд_на_Web.Models
{
    public class ActivePowerImbalance
    {
        public Guid ID { get; set; }
        public int n_nach_p { get; set; }
        public int n_kon_p { get; set; }
        public string name_p { get; set; }
        public double p_neb_p { get; set; }
        public Guid SliceID_p { get; set; }
        public int orderIndexP { get; set;}
    }
}
