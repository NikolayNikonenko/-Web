namespace перенос_бд_на_Web.Models
{
    public class ReactivePowerImbalance
    {
        public Guid ID { get; set; }
        public int n_nach_q { get; set; }
        public int n_kon_q { get; set; }
        public string name_q { get; set; }
        public double q_neb_q { get; set; }
        public Guid SliceID_q { get; set; }
        public int orderIndexQ { get; set; }
        public string experiment_label { get; set; }
    }
}
