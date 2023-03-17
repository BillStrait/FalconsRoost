namespace FalconsRoost.Models
{
    public class AttackDescription
    {
        public bool Lethal = false;
        public string Description { get; set; }
        public int Damage { get; set; }
        public bool Hit { get; set; } = false;

        public AttackDescription(string description)
        {
            Description = description;
        }
    }
}
