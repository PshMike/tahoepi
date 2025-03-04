using System.Collections.Generic;

namespace Jenne.TahoePI
{
    class Telemetry
    {
        public Guid session { get; set; }
        public DateTime sessionStart { get; set; }
        public DateTime TimeGenerated { get; set; }
        public double sessionUptime { get; set; }

        public Dictionary<string, double> Samples { get; set; }

        public Telemetry()
        {
            this.session = Guid.NewGuid();
            this.sessionStart = DateTime.Now;
            Samples = new Dictionary<string, double>();
        }

        public Telemetry(Guid session, DateTime sessionStart)
        {
            this.session = session;
            this.sessionStart = sessionStart;
            Samples = new Dictionary<string, double>();
        }
    }
}

