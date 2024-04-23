using Microsoft.AspNetCore.Mvc;
using BallisticCalculator;
using Gehtsoft.Measurements;
using TodoApi.Models; 

namespace TodoApi.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

[HttpPost(Name = "Post")]
public ShotResult Post(Shot shot)
{
    // Define M855 projectile out of 20 inch barrel
    var ammo = new Ammunition(
        weight: new Measurement<WeightUnit>(shot.ammo.weight, WeightUnit.Grain),
        muzzleVelocity: new Measurement<VelocityUnit>(shot.ammo.muzzleVelocity, VelocityUnit.MetersPerSecond),
        ballisticCoefficient: new BallisticCoefficient(shot.ammo.ballisticCoefficient, DragTableId.G1),
        bulletDiameter: new Measurement<DistanceUnit>(shot.ammo.bulletDiameter, DistanceUnit.Inch),
        bulletLength: new Measurement<DistanceUnit>(shot.ammo.bulletLength, DistanceUnit.Inch));

    // Define ACOG scope
    var sight = new Sight(
        sightHeight: new Measurement<DistanceUnit>(shot.ammo.sightHeight, DistanceUnit.Inch),
        verticalClick: new Measurement<AngularUnit>(0, AngularUnit.InchesPer100Yards),
        horizontalClick: new Measurement<AngularUnit>(0, AngularUnit.InchesPer100Yards));

    // M16 rifling
    var rifling = new Rifling(
        riflingStep: new Measurement<DistanceUnit>(12, DistanceUnit.Inch),
        direction: TwistDirection.Right);

    // Standard 100 yard ACOG zeroing
    var zero = new ZeroingParameters(
        distance: new Measurement<DistanceUnit>(shot.ammo.zeroDistance, DistanceUnit.Meter),
        ammunition: null,
        atmosphere: null);

    // Define rifle by sight, zeroing, and rifling parameters
    var rifle = new Rifle(sight: sight, zero: zero, rifling: rifling);

    // Define atmosphere
    var atmosphere = new Atmosphere(
        pressure: new Measurement<PressureUnit>(shot.scenary.pressure, PressureUnit.InchesOfMercury),
        pressureAtSeaLevel: true,
        altitude: new Measurement<DistanceUnit>(shot.scenary.altitude, DistanceUnit.Foot),
        temperature: new Measurement<TemperatureUnit>(shot.scenary.temperature, TemperatureUnit.Fahrenheit),
        humidity: shot.scenary.humidity);

    var calc = new TrajectoryCalculator();

    // Shot parameters
    var shotParams = new ShotParameters()
    {
        MaximumDistance = new Measurement<DistanceUnit>(shot.distance, DistanceUnit.Meter),
        Step = new Measurement<DistanceUnit>(10, DistanceUnit.Meter),
        // Calculate sight angle for the specified zero distance
        SightAngle = calc.SightAngle(ammo, rifle, atmosphere),
        CantAngle = new Measurement<AngularUnit>(shot.angle, AngularUnit.Degree),
    };

    // Define winds
    Wind[] wind = new Wind[1]
    {
        new Wind()
        {
            Direction = new Measurement<AngularUnit>(45, AngularUnit.Degree),
            Velocity = new Measurement<VelocityUnit>(50, VelocityUnit.KilometersPerHour),
        },
    };

    // Calculate trajectory
    var trajectory = calc.Calculate(ammo, rifle, atmosphere, shotParams, wind);
    
    // Assuming 'shot.x' is the starting position of the shot, 
    // correct the calculation of 'x' and 'y' in the ShotResult
    var lastPoint = trajectory.Last();
    return new ShotResult
    {
    x = shot.x + (float) lastPoint.Drop.In(DistanceUnit.Meter),
    y = shot.y + (float) lastPoint.Windage.In(DistanceUnit.Meter)

    };
}

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {

                //define M855 projectile out of 20 inch barrel
        var ammo = new Ammunition(
            weight: new Measurement<WeightUnit>(75, WeightUnit.Grain),
            muzzleVelocity: new Measurement<VelocityUnit>(200, VelocityUnit.MetersPerSecond),
            ballisticCoefficient: new BallisticCoefficient(0.5, DragTableId.G1),
            bulletDiameter: new Measurement<DistanceUnit>(0.224, DistanceUnit.Inch),
            bulletLength: new Measurement<DistanceUnit>(0.9, DistanceUnit.Inch));
    
        //define ACOG scope
        var sight = new Sight(
            sightHeight: new Measurement<DistanceUnit>(3.5, DistanceUnit.Inch),
            verticalClick: new Measurement<AngularUnit>(0, AngularUnit.InchesPer100Yards),
            horizontalClick: new Measurement<AngularUnit>(0, AngularUnit.InchesPer100Yards)
            );
    
        //M16 rifling
        var rifling = new Rifling(
            riflingStep: new Measurement<DistanceUnit>(12, DistanceUnit.Inch),
            direction: TwistDirection.Right);
    
        //standard 100 yard ACOG zeroing
        var zero = new ZeroingParameters(
            distance: new Measurement<DistanceUnit>(25, DistanceUnit.Meter),
            ammunition: null,
            atmosphere: null
            );
    
        //define rifle by sight, zeroing and rifling parameters
        var rifle = new Rifle(sight: sight, zero: zero, rifling: rifling);
    
        //define atmosphere
        var atmosphere = new Atmosphere(
            pressure: new Measurement<PressureUnit>(29.92, PressureUnit.InchesOfMercury),
            pressureAtSeaLevel: true,
            altitude: new Measurement<DistanceUnit>(100, DistanceUnit.Foot),
            temperature: new Measurement<TemperatureUnit>(59, TemperatureUnit.Fahrenheit),
            humidity: 0.78);
    
        var calc = new TrajectoryCalculator();
    
        //shot parameters
        var shot = new ShotParameters()
        {
            MaximumDistance = new Measurement<DistanceUnit>(100, DistanceUnit.Meter),
            Step = new Measurement<DistanceUnit>(10, DistanceUnit.Meter),
            //calculate sight angle for the specified zero distance
            SightAngle = calc.SightAngle(ammo, rifle, atmosphere),
            CantAngle =  new Measurement<AngularUnit>(45, AngularUnit.Degree)
        };
    
        //define winds
    
        Wind[] wind = new Wind[1]
        {
            new Wind()
            {
                Direction = new Measurement<AngularUnit>(45, AngularUnit.Degree),
                Velocity = new Measurement<VelocityUnit>(50, VelocityUnit.KilometersPerHour),
            },
        };
    
    
        //calculate trajectory
        var trajectory = calc.Calculate(ammo, rifle, atmosphere, shot, wind);
        // Console.WriteLine($"{trajectory.Last().Drop.In(DistanceUnit.Inch):N2}");
        foreach (var point in trajectory)
            Console.WriteLine($"{point.Time} {point.Distance.In(DistanceUnit.Meter):N0} {point.Velocity.In(VelocityUnit.MetersPerSecond):N0} {point.Drop.In(DistanceUnit.Inch):N2} {point.Windage.In(DistanceUnit.Meter):N2}");

        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = 10,
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}
