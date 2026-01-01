using System;

namespace LockDownKiosk.Shared.Networking
{
    public sealed class KioskCommand
    {
        public KioskCommandType CommandType { get; set; }

        // Optional fields for future expansion
        public string? Sender { get; set; }
        public string? Note { get; set; }
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    }
}
