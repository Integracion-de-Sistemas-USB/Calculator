using Microsoft.AspNetCore.Mvc;
using BallisticCalculator;
using Gehtsoft.Measurements;
using TodoApi.Models; 

namespace TodoApi.Controllers;

[ApiController]
[Route("calculate_shot")]
public class WeatherForecastController : ControllerBase
{

[HttpPost(Name = "calculate_shot")]
public ShotResult Post(Shot shot)
{
    // Define M855 projectile out of 20 inch barrel
    var ammo = new Ammunition(
        weight: new Measurement<WeightUnit>(shot.ammo.weight, WeightUnit.Gram),
        muzzleVelocity: new Measurement<VelocityUnit>(shot.ammo.muzzleVelocity, VelocityUnit.MetersPerSecond),
        ballisticCoefficient: new BallisticCoefficient(shot.ammo.ballisticCoefficient, DragTableId.G1),
        bulletDiameter: new Measurement<DistanceUnit>(shot.ammo.bulletDiameter, DistanceUnit.Millimeter),
        bulletLength: new Measurement<DistanceUnit>(shot.ammo.bulletLength, DistanceUnit.Millimeter));

    // Define ACOG scope
    var sight = new Sight(
        sightHeight: new Measurement<DistanceUnit>(shot.ammo.sightHeight, DistanceUnit.),
        verticalClick: new Measurement<AngularUnit>(0, AngularUnit.InchesPer100Yards),
        horizontalClick: new Measurement<AngularUnit>(0, AngularUnit.InchesPer100Yards));

    // M16 rifling
    var rifling = new Rifling(
        riflingStep: new Measurement<DistanceUnit>(shot.ammo.riflingStep, DistanceUnit.Centimeter),
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
        pressure: new Measurement<PressureUnit>(shot.scenary.pressure, PressureUnit.MillimetersOfMercury),
        pressureAtSeaLevel: true,
        altitude: new Measurement<DistanceUnit>(shot.scenary.altitude, DistanceUnit.Meter),
        temperature: new Measurement<TemperatureUnit>(shot.scenary.temperature, TemperatureUnit.Celsius),
        humidity: shot.scenary.humidity);

    var calc = new TrajectoryCalculator();

    // Shot parameters
    var shotParams = new ShotParameters()
    {
        MaximumDistance = new Measurement<DistanceUnit>(shot.target_distance, DistanceUnit.Meter),
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
            Direction = new Measurement<AngularUnit>(shot.scenary.windDirection, AngularUnit.Degree),
            Velocity = new Measurement<VelocityUnit>(shot.scenary.windVelocity, VelocityUnit.KilometersPerHour),
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

}
