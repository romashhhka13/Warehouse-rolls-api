using System;

namespace WarehouseRolls.Models
{
    public class Roll
    {
        public Guid Id { get; set; }

        public double Length;
        public double Weight;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset DeletedAt { get; set; }
    }
}