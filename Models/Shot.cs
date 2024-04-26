namespace TodoApi.Models; 

public class Shot
{
    public float x { get; set; }
    public float y { get; set; }
    public float target_distance { get; set; }
    public float angle { get; set; } 
    public Scenary scenary { get; set; }
    public Ammo ammo { get; set; } 
}