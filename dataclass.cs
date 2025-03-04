namespace Jenne.TahoePI
{
class Telemetry
{
    public Guid session { get; set; }
    public DateTime sessionStart { get; set; }
    public double sessionUptime { get; set; }
    public double Temp1 { get; set; }
    public double Cell0 { get; set; }
    public double Cell1 { get; set; }
    public double Cell2 { get; set; }
    public double Cell3 { get; set; }
    public double Battery0 { get; set; }
    public int LightLevel { get; set; }

    public Telemetry()
    {
        this.session = Guid.NewGuid();
        this.sessionStart = DateTime.Now;
        
    }

}
}

